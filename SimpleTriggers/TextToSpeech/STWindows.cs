using System.Speech.Synthesis;

public class STWindows : ITextToSpeech
{
    //private float volume = 1.0f;
    //private float speed = 1.0f;
    private SpeechSynthesizer synth {get; init;}
    public STWindows()
    {
        synth = new SpeechSynthesizer();
        synth.SetOutputToDefaultAudioDevice();
    }

    public void Dispose()
    {
        //throw new System.NotImplementedException();
    }

    public bool IsInitialized()
    {
        return true;
    }

    public void SetVoice(string voice)
    {
        //throw new System.NotImplementedException();
    }

    public void SetVolume(float volume)
    { }

    public void SetSpeed(float speed)
    { }

    public void Speak(string message)
    {
        //throw new System.NotImplementedException();
        synth.SpeakAsync(message);
    }
}