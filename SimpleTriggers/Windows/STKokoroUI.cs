using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface;
using SimpleTriggers.TextToSpeech;
using SimpleTriggers.Gui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Components;
using KokoroSharp.Core;

namespace SimpleTriggers.Windows;

public static class STKokoroUI
{
    public static void DrawKokoroSettings(Plugin plugin)
    {
        ImGui.SetNextItemWidth(160 * ImGuiHelpers.GlobalScale);
        using (var box = ImRaii.Combo("##KokoroVoiceBox", KokoroVoiceHelper.ToName(plugin.Configuration.Kokoro.Voice), ImGuiComboFlags.HeightLarge))
        {
            if(box)
            {
                for(var i = 0; i < Enum.GetNames<KokoroVoiceKind>().Length; ++i)
                {
                    if(ImGui.Selectable(KokoroVoiceHelper.ToName((KokoroVoiceKind)i)))
                    {
                        plugin.Configuration.Kokoro.Voice = (KokoroVoiceKind)i;
                        plugin.SetTTSVoice(KokoroVoiceHelper.ToString((KokoroVoiceKind)i));
                        plugin.Configuration.Save();
                    }
                }
            }
        }

        ImGui.SameLine();
        if(ImGuiComponents.IconButton(FontAwesomeIcon.Play))
        {
            plugin.SpeakTTS("This is a test of the Kokoro voice.");
        }
        ImGui.SameLine();
        ImGui.Text("Test Voice");

        // Volume and Speed
        ImGui.SetNextItemWidth(192 * ImGuiHelpers.GlobalScale);
        ImGui.SliderFloat("Voice Speed", ref plugin.Configuration.Kokoro.Speed,0.5f, 1.5f,"%.1fx");
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            plugin.SetTTSSpeed(plugin.Configuration.Kokoro.Speed);
            plugin.Configuration.Save();
        }
        
        ImGui.SetNextItemWidth(192 * ImGuiHelpers.GlobalScale);
        ImGui.SliderFloat("Voice Volume", ref plugin.Configuration.Kokoro.Volume,1.0f, 100.0f,"%.0f%%");
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            plugin.SetTTSVolume(plugin.Configuration.Kokoro.Volume);
            plugin.Configuration.Save();
        }

        if(ImGui.Checkbox("Use espeak for phonemes?", ref plugin.Configuration.Kokoro.UseEspeak))
        {
            plugin.Configuration.Save();
        }
        ImGui.SameLine();
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.Text($"{FontAwesomeIcon.ExclamationCircle.ToIconString()}");
        ImGui.PopFont();
        ImGuiCustom.HoverTooltip("May result in more natural voices, \nhowever if it causes issues, leave disabled.");

        // Show language setting -- only espeak compatible
        if(plugin.Configuration.Kokoro.UseEspeak)
        {
            ImGui.SetNextItemWidth(180 * ImGuiHelpers.GlobalScale);
            using(var box = ImRaii.Combo("Language##KokoroLanguageBox", NormalizedLangName(plugin.Configuration.Kokoro.Language)))
            {
                if(box)
                {
                    foreach(var lang in Enum.GetValues<KokoroLanguage>())
                    {
                        var langCode = KokoroLangCodeHelper.GetLangCode(lang);
                        if(ImGui.Selectable(NormalizedLangName(langCode)))
                        {
                            plugin.SetTTSLanguage(plugin.Configuration.Kokoro.Language = langCode);
                            plugin.Configuration.Save();
                        }
                    }
                }
            }
        }

        // Information Text
        if(!plugin.CanSpeak())
        {
            ImGui.Text("Downloading model...");
        }
    }

    public static string NormalizedLangName(string langCode)
    {
        return langCode switch
        {
            "en-us" => "American English",
            "en-gb" => "British English",
            "pt-br" => "Brazilian Portuguese",
            "es"    => "Spanish",
            "fr"    => "French",
            "it"    => "Italian",
            "hi"    => "Hindi",
            "ja"    => "Japanese",
            "cmn"   => "Mandarin Chinese",
            _       => "UNKNOWN"
        };
    }

}
