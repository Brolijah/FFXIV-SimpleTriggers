using System;
using System.Speech.Synthesis;
using SimpleTriggers.Logger;
using SimpleTriggers.TextToSpeech;

public class STWinSpeech : ITextToSpeech
{
    public AudioPlayer AudioPlayer { get; }
    private SpeechSynthesizer synth {get; init;}
    public STWinSpeech()
    {
        synth = new SpeechSynthesizer();
        AudioPlayer = new AudioPlayer();
        // TODO
        synth.SetOutputToDefaultAudioDevice();
    }

    public void SetVoice(string voice)
    {
        try
        {
            synth.SelectVoice(voice);
        } catch (Exception e)
        {
            STLog.Log.Warning(e, $"Voice \"{voice}\" not found.");
        }
    }

    public void SetVolume(float volume)
    {
        synth.Volume = (int)Math.Clamp(volume, 0, 100);
    }

    public void SetSpeed(float speed)
    {
        synth.Rate = (int)speed;
    }

    public void SetLanguage(string lang)
    { } // Language is controlled by the user's Windows Settings

    public void Speak(string message, bool extra)
    {
        try
        {
            synth.SpeakAsync(message);
        } catch (Exception e)
        {
            STLog.Log.Error(e, "STWinSpeech.Speak(): Exception caught:");
        }
    }

    public bool IsInitialized()
    {
        return true;
    }

    public void Dispose()
    {
        AudioPlayer.Dispose();
        synth.Dispose();
    }
}