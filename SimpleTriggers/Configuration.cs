using Dalamud.Configuration;
using System;
using SimpleTriggers.TextToSpeech;
using SimpleTriggers.Triggers;

namespace SimpleTriggers;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool EnableTriggers = true;
    public uint MaxLogHistory = 500;
    public TextToSpeechType TTSProvider = TextToSpeechType.None;
    public KokoroVoiceKind TTSKokoroVoice = 0;
    public bool KokoroUseEspeak = false;
    public float TTSSpeed = 1.0f;
    public float TTSVolume = 100.0f;
    public string WinSpeechVoice = "";
    public TriggerTree TriggerTree {get; set;} = new();

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
