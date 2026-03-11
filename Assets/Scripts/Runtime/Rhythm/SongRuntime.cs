using ShadowRhythm.Data.Models;

namespace ShadowRhythm.Rhythm
{
    public sealed class SongRuntime
    {
        public string SongId { get; private set; }
        public float Bpm { get; private set; }
        public float OffsetSeconds { get; private set; }
        public int BeatsPerBar { get; private set; }
        public float SecondsPerBeat { get; private set; }

        public SongRuntime(SongMetaModel meta)
        {
            SongId = meta.songId;
            Bpm = meta.bpm;
            OffsetSeconds = meta.offsetSeconds;
            BeatsPerBar = meta.beatsPerBar;
            SecondsPerBeat = 60f / Bpm;
        }

        public float GetBeatTime(int beatIndex)
        {
            return OffsetSeconds + beatIndex * SecondsPerBeat;
        }

        public int GetBeatIndexAtTime(float songTime)
        {
            float adjustedTime = songTime - OffsetSeconds;
            if (adjustedTime < 0) return 0;
            return (int)(adjustedTime / SecondsPerBeat);
        }
    }
}