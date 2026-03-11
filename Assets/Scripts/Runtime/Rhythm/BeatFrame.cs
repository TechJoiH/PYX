namespace ShadowRhythm.Rhythm
{
    public readonly struct BeatFrame
    {
        public readonly int beatIndex;
        public readonly int barIndex;
        public readonly float beatProgress;
        public readonly float nearestBeatTime;

        public BeatFrame(int beatIndex, int barIndex, float beatProgress, float nearestBeatTime)
        {
            this.beatIndex = beatIndex;
            this.barIndex = barIndex;
            this.beatProgress = beatProgress;
            this.nearestBeatTime = nearestBeatTime;
        }
    }
}