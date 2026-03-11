using System;

namespace ShadowRhythm.Data.Models
{
    [Serializable]
    public class SongMetaModel
    {
        public string songId;
        public string displayName;
        public string musicFileName;
        public float bpm;
        public float offsetSeconds;
        public int beatsPerBar;
        public int previewStartMs;
    }
}