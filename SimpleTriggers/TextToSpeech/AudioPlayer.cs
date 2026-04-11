using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SimpleTriggers.Logger;

namespace SimpleTriggers.TextToSpeech;

public class AudioPlayer : IDisposable
{
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private readonly WaveFormat waveFormat = new (24000, 16, 1);
    private readonly ConcurrentQueue<byte[]> queue = [];
    private WasapiOut waveOut;
    private volatile bool resetInit = false;
    private volatile float volume = 1.0f;
    private volatile bool hasExited = false;

    public AudioPlayer(string deviceId = "")
    {
        if(TryGetMMDevice(deviceId, out var device))
        {
            waveOut = new WasapiOut(device, AudioClientShareMode.Shared, true, 100);
        } else { waveOut = new WasapiOut(AudioClientShareMode.Shared, true, 100);} // fallback to default

        var t = new Thread(async() => {
            var mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 1));
            mixer.ReadFully = true;
            waveOut.Init(mixer);
            waveOut.Play();

            while(!hasExited) {
                await Task.Delay(50);
                await semaphore.WaitAsync();
                try {
                    if(resetInit)
                    {
                        waveOut.Init(mixer);
                        waveOut.Play();
                        resetInit = false;
                    }
                } catch (Exception e)
                { STLog.Log.Error(e, "Exception caught:"); 
                } finally { semaphore.Release(); }

                // check queue
                while(!hasExited && queue.TryDequeue(out var packet))
                {
                    try {
                        var stream = new RawSourceWaveStream(packet, 0, packet.Length, waveFormat);
                        var vmix = new VolumeSampleProvider(stream.ToSampleProvider()) { Volume = volume };
                        var smix = new WdlResamplingSampleProvider(vmix, mixer.WaveFormat.SampleRate);
                        mixer.AddMixerInput(smix);
                        //await Task.Delay(stream.TotalTime); // prevents streams from overlapping (it's funnier not to)
                    } catch (Exception e)
                    { STLog.Log.Error(e, "Exception caught:"); }
                }
            }
        }){ IsBackground = true };
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
    }

    public void SetOutputDevice(string deviceId)
    {
        semaphore.WaitAsync();
        try
        {
            waveOut.Stop();
            waveOut.Dispose();
            if(TryGetMMDevice(deviceId, out var device))
            {
                waveOut = new WasapiOut(device, AudioClientShareMode.Shared, true, 100);
            } else { waveOut = new WasapiOut(AudioClientShareMode.Shared, true, 100); } // fallback to default
            resetInit = true;
        } catch (Exception e)
        {
            STLog.Log.Error(e, "Exception caught:");
        } finally { semaphore.Release(); }
    }

    public bool TryGetMMDevice(string deviceId, [NotNullWhen(true)] out MMDevice? device)
    {
        try
        {
            using(var enumerator = new MMDeviceEnumerator())
            {
                device = enumerator.GetDevice(deviceId);
            }   
        } catch(Exception e)
        {
            STLog.Log.Warning($"Device ID does not exist: \"{deviceId}\"");
            STLog.Log.Warning(e, "Exception caught:");
            device = null;
        }
        return device != null;
    }
    
    public void StopPlayback(bool clearQueue = false)
    {
        if(clearQueue)
        {
            queue.Clear();
        }
    }

    // volume = [0.0f, 100.0f] // technically allows values >100, and that *might* be fine?
    public void SetVolume(float volume)
    {
        this.volume = volume/100f;
    }

    public void Dispose()
    {
        hasExited = true;
        waveOut.Stop();
        waveOut.Dispose();
        queue.Clear();
    }

    public void Enqueue(byte[] stream)
    {
        ObjectDisposedException.ThrowIf(hasExited, this);
        queue.Enqueue(stream);
    }
    
}