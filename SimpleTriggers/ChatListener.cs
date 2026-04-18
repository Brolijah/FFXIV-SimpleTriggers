using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using SimpleTriggers.ChatEnums;
using SimpleTriggers.SeFunctions;

namespace SimpleTriggers;

internal class ChatListener : IDisposable
{
    // Evil code, but these are the most common unique characters that appear in chat (mainly combat)
    private readonly HashSet<char> blacklist = [
        (char)SeIconChar.ArrowRight, (char)SeIconChar.LinkMarker, (char)SeIconChar.Buff, (char)SeIconChar.Debuff
    ];

    private readonly Plugin plugin;
    private readonly IChatGui chatGui;

    internal ChatListener(Plugin plugin, IChatGui chatGui)
    {
        this.plugin = plugin;
        this.chatGui = chatGui;
        chatGui.ChatMessage += OnChatMessage;
    }

    private string SanitizeString(string text, HashSet<char>? characterSet = null)
    {
        var ret = new StringBuilder(text.Length);
        var bl = characterSet ?? blacklist;
        foreach(var c in text)
        {
            if(!bl.Contains(c)) ret.Append(c);
        }
        return ret.ToString().Trim();
    }

    public void Dispose()
    {
        chatGui.ChatMessage -= OnChatMessage;
    }

    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var logKind = (ChatType)((int)type & 0x7F);
        // Ignore messages coming from this plugin or others
        if(logKind == ChatType.Debug) return;

        // Check our channel filter first
        if(!plugin.Configuration.ChannelReadAllTypes && !plugin.Configuration.ChannelTypeFilter.Contains((int)logKind))
        { return; }

        // Check if we're ignoring healing and damage sources
        if(plugin.Configuration.IgnoreDamageAndHealing && (logKind is ChatType.Damage or ChatType.Miss or ChatType.Healing))
        { return; }
        
        var msgStr = SanitizeString(message.ToString());
        if(plugin.Configuration.EnableTriggers)
        {
            foreach(var category in plugin.Configuration.TriggerTree)
            {
                if(category.enabled)
                {
                    foreach(var trig in category.Triggers)
                    {
                        if(trig.enabled)
                        {
                            var expression = trig.expression;
                            if(msgStr.Contains(expression, StringComparison.CurrentCultureIgnoreCase))
                            {
                                if(trig.doResponseTTS && (trig.response.Length > 0))
                                {
                                    plugin.SpeakTTS(trig.response);
                                }
                                
                                if(trig.doPlaySound && trig.soundFx > 0)
                                {
                                    PlaySound.Play(SoundsExtensions.FromIdx(trig.soundFx));
                                }

                                if(trig.doPostInChat && (trig.response.Length > 0))
                                {
                                    plugin.PrintChatMsg(trig.response);
                                }
                            }
                        }
                    }
                }
            }
        }

        if(plugin.doLogChatHistory)
        {
            while(plugin.ChatLog.Count >= plugin.Configuration.MaxLogHistory)
            {
                plugin.ChatLog.Dequeue();
            }
            if(plugin.doIncludeChatTypeInfo)
            {
                var targetKind = (EntityRelationKind)((((int)type) >>  7) & 0xF);
                var casterKind = (EntityRelationKind)((((int)type) >> 11) & 0xF);
                plugin.ChatLog.Enqueue($"{{{logKind}/{targetKind}/{casterKind}/{type}}} : {msgStr}");
            } else { plugin.ChatLog.Enqueue(msgStr); }
        }
    }
}