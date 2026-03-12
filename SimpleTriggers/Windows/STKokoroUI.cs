using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface;
using SimpleTriggers.TextToSpeech;
using SimpleTriggers.Gui;

namespace SimpleTriggers.Windows;

public static class STKokoroUI
{
    public static void DrawKokoroSettings(Plugin plugin)
    {
        ImGui.SetNextItemWidth(160);
        using (var box = ImRaii.Combo("##KokoroVoiceBox", KokoroVoiceHelper.ToName(plugin.Configuration.TTSKokoroVoice), ImGuiComboFlags.HeightLarge))
        {
            if(box)
            {
                for(var i = 0; i < Enum.GetNames<KokoroVoiceKind>().Length; ++i)
                {
                    if(ImGui.Selectable(KokoroVoiceHelper.ToName((KokoroVoiceKind)i)))
                    {
                        plugin.Configuration.TTSKokoroVoice = (KokoroVoiceKind)i;
                        plugin.Configuration.Save();
                        plugin.SetTTSVoice(KokoroVoiceHelper.ToString((KokoroVoiceKind)i));
                    }
                }
            }
        }

        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        if(ImGui.Button($"{FontAwesomeIcon.Play.ToIconString()}"))
        {
            plugin.SpeakTTS("This is a test of the Kokoro voice.");
        }
        ImGui.PopFont();
        ImGui.SameLine();
        ImGui.Text("Test Voice");

        // Volume and Speed
        ImGui.SetNextItemWidth(192);
        ImGui.SliderFloat("Voice Speed", ref plugin.Configuration.TTSSpeed,0.5f, 1.5f,"%.1fx");
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            plugin.SetTTSSpeed(plugin.Configuration.TTSSpeed);
            plugin.Configuration.Save();
        }

        // Note: Internally, KokoroSharp uses NAudio's WaveOutEvent. Which when you set the volume, it will actually
        // change the volume of the WHOLE application, if in Windows. But in Wine this behaves differently and 
        // only changes the volume of the TTS. A work around to allow windows users control of Kokoro's Volume
        // will probably require managing my own audio backend which I don't intend on doing yet.
        if(!OSHelper.IsWindows())
        {
            ImGui.SetNextItemWidth(192);
            ImGui.SliderFloat("Voice Volume", ref plugin.Configuration.TTSVolume,1.0f, 100.0f,"%.0f%%");
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                plugin.SetTTSVolume(plugin.Configuration.TTSVolume);
                plugin.Configuration.Save();
            }
        }

        if(ImGui.Checkbox("Use espeak for phonemes?", ref plugin.Configuration.KokoroUseEspeak))
        {
            plugin.Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text($"{FontAwesomeIcon.ExclamationCircle.ToIconString()}");
        ImGui.PopFont();
        ImGuiCustom.HoverTooltip("May result in more natural voices, \nhowever if it causes issues, leave disabled.");

        // Information Text
        if(!plugin.CanSpeak())
        {
            ImGui.Text("Downloading model...");
        }
    }

}
