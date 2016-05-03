using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using NAudio.Wave;
using PlayerToDevice.Input;
using PlayerToDevice.Models;
using PlayerToDevice.ViewModels.Commands;

namespace PlayerToDevice.ViewModels
{
    public class MainVM : ViewModel
    {
        private const string LastPresetFile = "lp.path";
        private readonly Dictionary<int, HotKey> _keys;

        private AudioFileVM _currentFile;
        private string _preset;

        public MainVM()
        {
            Files = new ObservableCollection<AudioFileVM>();
            _keys = new Dictionary<int, HotKey>();
            Recent = new ObservableCollection<string>();
            LoadPreset();

            Devices = new ObservableCollection<DirectSoundDeviceInfo>(AudioWrapper.Instance.Devices);
            New = new RouteCommand(o => { Preset = string.Empty; });
            Add = new RouteCommand(o => { Files.Add(new AudioFileVM(this)); });
            Save = new RouteCommand(o => { SaveDataAs(Preset); });
            SaveAs = new RouteCommand(o => { SaveDataAs(string.Empty); });
            Load = new RouteCommand(o =>
            {
                var path = o as string;
                if (string.IsNullOrEmpty(path))
                {
                    var openFile = new OpenFileDialog();
                    openFile.Filter = "Preset File|*.pjs";
                    if (openFile.ShowDialog() == true && File.Exists(openFile.FileName))
                    {
                        path = openFile.FileName;
                    }
                }
                if (!string.IsNullOrEmpty(path))
                {
                    Preset = path;
                }
            });
            Remove = new RouteCommand(o =>
            {
                if (CurrentFile != null)
                {
                    DeleteHotKey(CurrentFile);
                    Files.Remove(CurrentFile);
                    CurrentFile = null;
                }
            }, o => CurrentFile != null);

            StartRecord = new RouteCommand(o =>
            {
                var element = o as TextBox;
                if (element != null)
                {
                    Keyboard.AddKeyUpHandler(element, KeyUp);
                }
            });
            StopRecord = new RouteCommand(o =>
            {
                var element = o as TextBox;
                if (element != null)
                {
                    Keyboard.RemoveKeyUpHandler(element, KeyUp);
                    if (element.Tag.Equals("HotKey"))
                    {
                        CreateHotKey();
                    }
                }
            });
            DeleteKey = new RouteCommand(o =>
            {
                if (o.Equals("HotKey"))
                {
                    DeleteHotKey(null);
                    Model.StopKey = null;
                    OnPropertyChanged(nameof(KeyName));
                }
                else if (o.Equals("SpeakKey"))
                {
                    Model.SpeakKey = null;
                    OnPropertyChanged(nameof(SpeakKeyName));
                }
            });
            Stop = new RouteCommand(o => { AudioWrapper.Instance.Stop(); });
        }

        public Main Model { get; private set; }

        public ObservableCollection<AudioFileVM> Files { get; private set; }
        public ObservableCollection<DirectSoundDeviceInfo> Devices { get; }

        public RouteCommand Add { get; private set; }
        public RouteCommand Remove { get; }
        public RouteCommand Save { get; private set; }
        public RouteCommand SaveAs { get; private set; }
        public RouteCommand Load { get; private set; }
        public RouteCommand StopRecord { get; private set; }
        public RouteCommand StartRecord { get; private set; }

        public string Preset
        {
            get { return _preset; }
            set
            {
                if (ChangeField(ref _preset, value))
                {
                    SavePreset(_preset);
                }
                LoadData(_preset);
            }
        }

        public string KeyName => Model.StopKey.HasValue ? ((VirtualKeyShort) Model.StopKey).ToString() : string.Empty;

        public string SpeakKeyName
            => Model.SpeakKey.HasValue ? ((VirtualKeyShort) Model.SpeakKey).ToString() : string.Empty;

        public double Volume
        {
            get { return Model.Volume; }
            set
            {
                var volume = Model.Volume;
                Model.Volume = (float) value;
                if (ChangeField(ref volume, Model.Volume))
                {
                    AudioWrapper.Instance.Volume = Model.Volume;
                }
            }
        }

        public bool AltOn
        {
            get { return Model.Modifiers.Any(x => x.Equals(Key.LeftAlt)); }
            set
            {
                if (value != AltOn)
                {
                    if (!value)
                        Model.Modifiers.Remove(Key.LeftAlt);
                    else Model.Modifiers.Add(Key.LeftAlt);
                    OnPropertyChanged();
                    RecreateKeys();
                }
            }
        }

        public bool CtrlOn
        {
            get { return Model.Modifiers.Any(x => x.Equals(Key.LeftCtrl)); }
            set
            {
                if (value != CtrlOn)
                {
                    if (!value)
                        Model.Modifiers.Remove(Key.LeftCtrl);
                    else Model.Modifiers.Add(Key.LeftCtrl);
                    OnPropertyChanged();
                    RecreateKeys();
                }
            }
        }

        public bool ShiftOn
        {
            get { return Model.Modifiers.Any(x => x.Equals(Key.LeftShift)); }
            set
            {
                if (value != ShiftOn)
                {
                    if (!value)
                        Model.Modifiers.Remove(Key.LeftShift);
                    else Model.Modifiers.Add(Key.LeftShift);
                    OnPropertyChanged();
                    RecreateKeys();
                }
            }
        }

        public DirectSoundDeviceInfo CurrentDevice
        {
            get { return Devices.FirstOrDefault(x => x.Guid.Equals(Model.Device)); }
            set
            {
                if (Devices.Any(x => x.Guid.Equals(value.Guid)))
                {
                    Model.Device = value.Guid;
                    AudioWrapper.Instance.ActiveDevice = Model.Device;
                    OnPropertyChanged();
                }
            }
        }

        public AudioFileVM CurrentFile
        {
            get { return _currentFile; }
            set
            {
                if (ChangeField(ref _currentFile, value))
                {
                    Remove.OnCanExecuteChanged();
                }
            }
        }

        public Key[] Modifiers => Model.Modifiers.ToArray();

        public RouteCommand Stop { get; private set; }

        public ObservableCollection<string> Recent { get; }

        public RouteCommand New { get; private set; }

        public RouteCommand DeleteKey { get; private set; }

        private void RecreateKeys()
        {
            var ints = _keys.Keys.ToList();
            foreach (var i in ints)
            {
                var key = _keys[i];
                var action = key.Action;
                var k = key.Key;
                key.Unregister();
                key.Dispose();
                _keys[i] = new HotKey(k, action, Modifiers);
            }
        }

        private void LoadPreset()
        {
            var result = string.Empty;
            if (File.Exists(LastPresetFile))
            {
                var lines = File.ReadAllLines(LastPresetFile);

                if (lines.Length > 0)
                    result = lines[0];
            }
            Preset = result;
        }

        private void SavePreset(string preset)
        {
            var presets = new HashSet<string>();
            if (!string.IsNullOrWhiteSpace(preset))
            {
                presets.Add(preset);
            }
            if (File.Exists(LastPresetFile))
            {
                var allLines = File.ReadAllLines(LastPresetFile);
                Recent.Clear();
                foreach (var line in allLines)
                {
                    if (presets.Add(line))
                    {
                        Recent.Add(line);
                    }
                }
            }
            File.WriteAllLines(LastPresetFile, presets.ToArray());
        }


        public void CreateHotKey(AudioFileVM vm, int? key)
        {
            DeleteHotKey(vm);

            var hash = vm?.GetHashCode() ?? 0;
            if (key.HasValue && HotKey.CanRegister(KeyInterop.KeyFromVirtualKey(key.Value), Modifiers))
            {
                var hotKey = new HotKey(KeyInterop.KeyFromVirtualKey(key.Value), () =>
                {
                    if (vm != null)
                        vm.Play.Execute(null);
                    else
                        AudioWrapper.Instance.Stop();
                }, Modifiers);
                _keys.Add(hash, hotKey);
            }
        }

        public void DeleteHotKey(AudioFileVM vm)
        {
            HotKey hotKey;
            var hash = vm?.GetHashCode() ?? 0;
            if (_keys.TryGetValue(hash, out hotKey))
            {
                hotKey.Unregister();
                hotKey.Dispose();
                _keys.Remove(hash);
            }
        }

        public bool CanCreateKey(AudioFileVM vm, Key key)
        {
            HotKey oldKey;
            var hash = vm?.GetHashCode() ?? 0;
            return (_keys.TryGetValue(hash, out oldKey) && oldKey.Key.Equals(key)) || HotKey.CanRegister(key, Modifiers);
        }

        private void CreateHotKey()
        {
            CreateHotKey(null, Model.StopKey);
        }

        private void KeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            var obj = sender as TextBox;
            if (obj != null)
            {
                var key = keyEventArgs.Key;
                if (obj.Tag.Equals("HotKey"))
                {
                    if (CanCreateKey(null, key))
                    {
                        Model.StopKey = KeyInterop.VirtualKeyFromKey(key);
                    }
                    OnPropertyChanged(nameof(KeyName));
                }
                else if (obj.Tag.Equals("SpeakKey"))
                {
                    AudioWrapper.Instance.Stop();
                    Model.SpeakKey = KeyInterop.VirtualKeyFromKey(key);
                    OnPropertyChanged(nameof(SpeakKeyName));
                }
            }
        }


        private void LoadData(string file)
        {
            AudioWrapper.Instance.Stop();
            Model = null;
            if (File.Exists(file))
            {
                var json = File.ReadAllText(file);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var serializer = new JavaScriptSerializer();
                    Model = serializer.Deserialize<Main>(json);
                }
            }
            if (Model == null)
            {
                Model = new Main();
            }
            if (Files != null)
                while (Files.Count > 0)
                {
                    DeleteHotKey(Files[0]);
                    Files.RemoveAt(0);
                }
            Model.Files.ForEach(x => Files.Add(new AudioFileVM(this, x)));
            CreateHotKey(null, Model.StopKey);

            AudioWrapper.Instance.Volume = Model.Volume;
            AudioWrapper.Instance.ActiveDevice = Model.Device;
            OnPropertyChanged();
        }

        private void SaveDataAs(string name)
        {
            var cont = true;
            if (string.IsNullOrEmpty(name))
            {
                var saveFile = new SaveFileDialog {Filter = "Preset File|*.pjs"};

                if (saveFile.ShowDialog() == true)
                {
                    name = saveFile.FileName;
                }
                else
                {
                    cont = false;
                }
            }
            if (cont)
            {
                SaveData(name);
                Preset = name;
            }
        }

        private void SaveData(string file)
        {
            Model.Files.Clear();
            Model.Files.AddRange(Files.Select(x => x.Model));
            var serializer = new JavaScriptSerializer();
            var json = serializer.Serialize(Model);

            File.WriteAllText(file, json);
        }
    }
}