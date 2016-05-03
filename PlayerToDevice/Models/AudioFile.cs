using System;

namespace PlayerToDevice.Models
{
    [Serializable]
    public class AudioFile
    {
        public int? Key { get; set; }
        public string FilePath { get; set; }
    }
}