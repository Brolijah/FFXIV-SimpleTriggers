using System;
using System.Collections.Generic;
using System.Text;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using SimpleTriggers.GameEnums;
using SimpleTriggers.SeFunctions;

namespace SimpleTriggers;

internal class ChatListener : IDisposable
{
    // Evil code, but these are the most common unique characters that appear in chat (mainly combat)
    private readonly HashSet<char> blacklist = [
        (char)SeIconChar.ArrowRight, (char)SeIconChar.LinkMarker, (char)SeIconChar.Buff, (char)SeIconChar.Debuff
    ];

    // These are XivChatTypes (known and undocumented ones) that log information such as
    // "Player recovered XXXX HP" or "Enemy received XXXX damage." A few of these are
    // also messages such as "attack missed" or "fully resisted."
    private readonly HashSet<int> DamageAndHealingSources = [
        2217, 2221, 2729, 2857,
        4269, 4397, 4777, 4778, 4905, 4906,
        8493, 8745, 8746, 8749, 8873, 8874, 9001, 9002,
        10409, 10537, 10793,
        12457, 12585, 12586, 12841,
        18605, 18733, 19113, 19241,
        23085, 23209, 23337
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
        // Ignore messages coming from this plugin or others
        if(type == XivChatType.Debug) return;

        // Check our channel filter first
        if(!plugin.Configuration.ChannelReadAllTypes && !plugin.Configuration.ChannelTypeFilter.Contains((int)type))
        { return; }

        // Check if we're ignoring healing and damage sources
        if(plugin.Configuration.IgnoreDamageAndHealing && DamageAndHealingSources.Contains((int)type))
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
                var chatType = Enum.IsDefined<AdditionalChatType>((AdditionalChatType)type) ? ((AdditionalChatType)type).ToString() : type.ToString();
                plugin.ChatLog.Enqueue($"{{{chatType}}} : {msgStr}");
            } else { plugin.ChatLog.Enqueue(msgStr); }
        }
    }
}