namespace ShadowRhythm.Command
{
    public struct CommandExecutionRequest
    {
        public CommandType commandType;
        public int sourceBeatIndex;
        public bool isPerfectTiming;

        public CommandExecutionRequest(CommandType type, int beatIndex, bool perfect)
        {
            commandType = type;
            sourceBeatIndex = beatIndex;
            isPerfectTiming = perfect;
        }
    }
}