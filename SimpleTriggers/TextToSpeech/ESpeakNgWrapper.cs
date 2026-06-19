// This class will provide a simplified interface for espeak-ng's native calls
// Seeing as I intend to have both Kokoro and eSpeakTTS interface with this,
// it seems like the best solution to avoid writing duplicate code

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using SimpleTriggers.Logger;

namespace SimpleTriggers.TextToSpeech;

public static class ESpeakNgWrapper
{
    private static bool _initialized = false;

    // Must be called before anything else
    public static void Initialize(string binPath)
    {
        try {
            if(!_initialized)
            {
                var esPath = Path.Join(binPath, "espeak");
                var dllPath = Path.Join(esPath, ESpeakNgNative.LibraryName);
                STLog.Log.Warning($"espeak dll path = {dllPath}");

                if(!Path.Exists(dllPath))
                {
                    STLog.Log.Error("espeak dll not found");
                    return;
                }
                ESpeakNgNative.SetupResolver(dllPath);
                var res = ESpeakNgNative.espeak_Initialize(EsAudioOutput.AUDIO_OUTPUT_SYNCHRONOUS, 0, esPath, 0);
                STLog.Log.Warning($"espeak_Initialize returned {res}");

                if(res != -1) _initialized = true;
                else STLog.Log.Error("espeak_Initialize failed");
            }
        } catch (Exception e)
        {
            STLog.Log.Error(e, "ESpeakNgWrapper.Initialize(): Exception caught:");
            _initialized = false;
        }
    }

    public static bool IsInitialized()
    {
        return _initialized;
    }

    public static string ToPhonemes(string text, string voice = "en-us")
    {
        if(!IsInitialized()) return "";

        ESpeakNgNative.espeak_SetVoiceByName(voice);

        var builder = new StringBuilder();
        var clauses = Regex.Split(text, @"([\p{P}])"); // Split on punctuation
        foreach (var phrase in clauses)
        {
            var bytes = Encoding.UTF8.GetBytes(phrase);
            var ptrPhrase = Marshal.AllocHGlobal(bytes.Length + 1);
            try {
                Marshal.Copy(bytes, 0, ptrPhrase, bytes.Length);
                Marshal.WriteByte(ptrPhrase, bytes.Length, 0);

                var data = ESpeakNgNative.espeak_TextToPhonemes(ref ptrPhrase, EsCharMode.espeakCHARS_UTF8, 2);
                if(data != IntPtr.Zero)
                {
                    builder.Append(Marshal.PtrToStringUTF8(data));
                    builder.Append(' ');
                }
            } catch (Exception e) {
                STLog.Log.Error(e, "ESpeakNgWrapper.ToPhonemes(): Exception caught:");
            } finally {
                Marshal.FreeHGlobal(ptrPhrase);
            }
        }
        return builder.ToString();
    }

    public static string[] GetVoices()
    {
        
        return [];
    }
}