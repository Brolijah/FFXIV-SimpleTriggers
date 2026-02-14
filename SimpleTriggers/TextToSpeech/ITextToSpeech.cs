
using System;

public interface ITextToSpeech : IDisposable
{
    void Speak(string message);
    void SetVoice(string voice);
    bool IsInitialized();
}