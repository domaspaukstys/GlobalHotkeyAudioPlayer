using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace PlayerToDevice.Models
{
    [Serializable]
    public class Main
    {
        public Main()
        {
            Files = new List<AudioFile>();
            Modifiers = new List<Key>();
            Volume = 0.5f;
        }

        public Guid Device { get; set; }
        public int? StopKey { get; set; }
        public int? SpeakKey { get; set; }


        public float Volume { get; set; }
        public List<AudioFile> Files { get; set; }
        public List<Key> Modifiers { get; set; }
    }
}