using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NAudio.Wave;
using PlayerToDevice.Input;

namespace PlayerToDevice
{
    public class AudioWrapper
    {
        private static AudioWrapper _instance;
        private static Thread _playThread;
        private Guid _activeDevice;
        private List<DirectSoundDeviceInfo> _devices;
        private DirectSoundOut _output;
        private float _volume;

        private AudioWrapper()
        {
            DirectSoundDeviceInfo deviceInfo = Devices.FirstOrDefault();
            if (deviceInfo != null) ActiveDevice = deviceInfo.Guid;
            Volume = 1f;
        }

        public List<DirectSoundDeviceInfo> Devices
        {
            get { return _devices = _devices ?? DirectSoundOut.Devices.ToList(); }
        }

        public Guid ActiveDevice
        {
            get { return _activeDevice; }
            set
            {
                if (_activeDevice != value)
                {
                    _activeDevice = value;
                    _output?.Dispose();
                }
            }
        }

        public float Volume
        {
            get { return _volume; }
            set { _volume = (float)Math.Round(value, 2); }
        }

        public static AudioWrapper Instance => _instance = _instance ?? new AudioWrapper();

        public void PlayFile(string fileName, int? key)
        {
            if (File.Exists(fileName))
            {
                var reader = new AudioFileReader(fileName);

                Stop();
                _playThread = new Thread(() =>
                {
                    if (key.HasValue)
                    {
                        KeyboardHook.KeyDown((VirtualKeyShort) key.Value);
                    }
                    _output = new DirectSoundOut(ActiveDevice);
                    reader.Volume = _volume;
                    _output.Init(reader);
                    _output.Play();
                    var interupted = false;
                    while (_output.PlaybackState == PlaybackState.Playing && !interupted)
                    {
                        try
                        {
                            Thread.Sleep(10);
                        }
                        catch (ThreadInterruptedException)
                        {
                            Debug.Print("Thread interupted");
                            interupted = true;
                        }
                        catch (ThreadAbortException)
                        {
                            Debug.Print("Thread aborted");
                            interupted = true;
                        }
                    }
                    if (key.HasValue)
                    {
                        KeyboardHook.KeyUp((VirtualKeyShort)key.Value);
                    }
                });
                _playThread.Start();
            }
        }

        public static void Play(string file, int? key)
        {
            Instance.PlayFile(file, key);
        }

        public void Stop()
        {
            if (_output != null)
            {
                _output.Stop();
                _output.Dispose();
            }
            _playThread?.Interrupt();
            Debug.Print("Interupting thread");
        }
    }
}