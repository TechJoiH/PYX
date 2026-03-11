namespace ShadowRhythm.Input
{
    public struct InputSample
    {
        public RhythmInputType inputType;
        public float pressedSongTime;
        public int quantizedBeatIndex;
        public float deltaMs;
        public RhythmJudgeResult judgeResult;

        public InputSample(RhythmInputType type, float songTime, int beatIndex, float delta, RhythmJudgeResult result)
        {
            inputType = type;
            pressedSongTime = songTime;
            quantizedBeatIndex = beatIndex;
            deltaMs = delta;
            judgeResult = result;
        }
    }
}