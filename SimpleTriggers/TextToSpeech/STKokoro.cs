using System;
using System.IO;
using KokoroSharp;
using KokoroSharp.Core;
using KokoroSharp.Processing;
using SimpleTriggers.Logger;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Net.Http;
using System.Threading;

namespace SimpleTriggers.TextToSpeech;

public class STKokoro : ITextToSpeech
{
    // sha256 = c1610a859f3bdea01107e73e50100685af38fff88f5cd8e5c56df109ec880204
    private const string ModelUri = "https://github.com/taylorchu/kokoro-onnx/releases/download/v0.2.0/kokoro-quant.onnx";
    private readonly string configPath;
    private readonly AudioPlayer audioPlayer;
    private readonly Task<KokoroModel?> modelTask;
    private readonly CancellationTokenSource cts = new();
    private float speed = 1.0f;
    private string lang = "en-us";
    private KokoroVoice kv;
    public STKokoro(string binPath, string configPath, AudioPlayer player)
    {
        audioPlayer = player;
        audioPlayer.SetSourceWaveFormat(24000, 1);
        this.configPath = configPath;
        modelTask = LoadModelAsync();
        //ipaTask = LoadDictionaryAsync(Path.Join(binPath, "en_US.txt"));
        Tokenizer.eSpeakNGPath = Path.Join(binPath, "espeak");
        KokoroVoiceManager.LoadVoicesFromPath(Path.Join(binPath,"voices"));
        kv = KokoroVoiceManager.GetVoice("af_bella");
    }

    private async Task<KokoroModel?> LoadModelAsync()
    {
        bool download = false;
        var path = GetModelPath();
        try {
            if(Path.Exists(path)) // if the model file exists on disk
            {
                var hash = SHA256.HashData(await File.ReadAllBytesAsync(path, cts.Token));
                if(!(Convert.ToHexStringLower(hash) == "c1610a859f3bdea01107e73e50100685af38fff88f5cd8e5c56df109ec880204"))
                {
                    // mismatch, flag for download
                    File.Delete(path);
                    STLog.Log.Warning("KokoroTTS model mismatched hash, redownloading");
                    download = true;
                } else { STLog.Log.Information("KokoroTTS model already on disk. Using existing file."); }
            } else { download = true; }

            if(download)
            {
                STLog.Log.Information("Downloading KokoroTTS model...");
                using var client = new HttpClient();
                using var response = await client.GetAsync(ModelUri, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                using var responseStream = await response.Content.ReadAsStreamAsync(cts.Token);
                using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await responseStream.CopyToAsync(fileStream, cts.Token);
                    await fileStream.FlushAsync(cts.Token);
                }
                STLog.Log.Information("Kokoro model download completed");
            }
        } catch (Exception e)
        {
            STLog.Log.Error(e, "STKokoro.LoadModelAsync(): Exception caught:");
            return null;
        }

        return new KokoroModel(path);
    }

    private bool TryGetKokoroModel([NotNullWhen(true)] out KokoroModel? model)
    {
        if(modelTask.IsCompletedSuccessfully)
        {
            model = modelTask.Result;
        } else { model = null; }

        return model != null;
    }

    private string GetModelPath()
    {
        return Path.Join(configPath, "kokoro-quant.onnx");
    }

    public void SetVoice(string strVoice)
    {
        kv = KokoroVoiceManager.GetVoice(strVoice);
    }

    public void SetVolume(float volume)
    {
        audioPlayer.SetVolume(volume);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    public void SetLanguage(string lang)
    {
        this.lang = lang;
    }

    public void Speak(string message)
    {
        if(TryGetKokoroModel(out var model))
        {
            try {
                // KokoroSharp's tokenizer is not equipped with all the sounds eSpeak can produce.
                // this MIGHT work fine for western languages, but not eastern ones.
                var tokens = Tokenizer.TokenizePhonemes(ESpeakNgWrapper.ToPhonemes(message, lang).ToCharArray());
                var tokensList = SegmentationSystem.SplitToSegments(tokens, new()
                {
                    MinFirstSegmentLength = 20,
                    MaxFirstSegmentLength = 200,
                    MaxSecondSegmentLength = 200
                });
                foreach (var tc in tokensList)
                {
                    var bytes = KokoroPlayback.GetBytes(model.Infer(tc, kv.Features, speed));
                    audioPlayer.Enqueue(bytes);
                }
            } catch (Exception e)
            {
                STLog.Log.Error(e, "STKokoro.Speak(): Exception caught: ");
            }
        } else {
            STLog.Log.Warning("Attempted TTS before model loaded.");
        }
    }

    public bool IsInitialized()
    {
        return TryGetKokoroModel(out _);
    }

    public void Dispose()
    {
        cts.Cancel();
        cts.Dispose();
        if(TryGetKokoroModel(out var model))
        {
            model.Dispose();
        }
    }
}
