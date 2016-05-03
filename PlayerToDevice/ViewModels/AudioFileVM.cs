using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using PlayerToDevice.Input;
using PlayerToDevice.Models;
using PlayerToDevice.ViewModels.Commands;

namespace PlayerToDevice.ViewModels
{
    public class AudioFileVM : ViewModel
    {
        private readonly MainVM _parent;

        public AudioFileVM(MainVM mainVM, AudioFile file = null)
        {
            _parent = mainVM;
            if (file == null)
            {
                Model = new AudioFile();
            }
            else
            {
                Model = file;
            }

            OnPropertyChanged();
            Play = new RouteCommand(o => { AudioWrapper.Play(FilePath, _parent.Model.SpeakKey); },
                o => File.Exists(FilePath));
            Browse = new RouteCommand(o =>
            {
                var dialog = new OpenFileDialog();
                dialog.Filter = "Audio|*.mp3;*.wav;*.wma";
                if (dialog.ShowDialog() == true)
                {
                    FilePath = dialog.FileName;
                }
            });
            StartRecord = new RouteCommand(o =>
            {
                var element = o as DependencyObject;
                if (element != null)
                {
                    Keyboard.AddKeyUpHandler(element, KeyUp);
                }
            });
            StopRecord = new RouteCommand(o =>
            {
                var element = o as DependencyObject;
                if (element != null)
                {
                    Keyboard.RemoveKeyUpHandler(element, KeyUp);
                    _parent.CreateHotKey(this, Model.Key);
                }
            });
            DeleteKey = new RouteCommand(o =>
            {
                _parent.DeleteHotKey(this);
                Model.Key = null;
                OnPropertyChanged(nameof(KeyName));
            });
            _parent.CreateHotKey(this, Model.Key);
        }

        public RouteCommand Play { get; }
        public RouteCommand Browse { get; private set; }
        public RouteCommand StopRecord { get; private set; }
        public RouteCommand StartRecord { get; private set; }

        public string KeyName => Model.Key.HasValue ? ((VirtualKeyShort) Model.Key).ToString() : string.Empty;

        public string FilePath
        {
            get { return Model.FilePath; }
            set
            {
                var filePath = Model.FilePath;
                Model.FilePath = value;
                ChangeField(ref filePath, value);
                Play.OnCanExecuteChanged();
            }
        }

        public AudioFile Model { get; }

        public RouteCommand DeleteKey { get; private set; }

        private void KeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            var key = keyEventArgs.Key;
            if (_parent.CanCreateKey(this, key))
            {
                Model.Key = KeyInterop.VirtualKeyFromKey(key);
            }
            OnPropertyChanged(nameof(KeyName));
        }
    }
}