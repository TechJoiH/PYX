using System.Collections.Generic;
using ShadowRhythm.Input;
using ShadowRhythm.Fighter;

namespace ShadowRhythm.Command
{
    /// <summary>
    /// 命令解析上下文 - 提供解析命令时需要的所有信息
    /// </summary>
    public sealed class CommandContext
    {
        /// <summary>当前拍点序号</summary>
        public int CurrentBeatIndex { get; set; }

        /// <summary>当前歌曲时间</summary>
        public float CurrentSongTime { get; set; }

        /// <summary>当前角色状态</summary>
        public FighterState CurrentFighterState { get; set; }

        /// <summary>最近的输入采样列表</summary>
        public List<InputSample> RecentInputs { get; set; }

        /// <summary>当前拍的输入</summary>
        public List<InputSample> CurrentBeatInputs { get; set; }

        /// <summary>上一拍的输入</summary>
        public List<InputSample> PreviousBeatInputs { get; set; }

        /// <summary>是否可以取消当前动作</summary>
        public bool CanCancel { get; set; }

        /// <summary>是否在受击硬直中</summary>
        public bool IsInHitstun { get; set; }

        /// <summary>是否在格挡中</summary>
        public bool IsGuarding { get; set; }

        /// <summary>上一个执行的命令</summary>
        public CommandType LastCommand { get; set; }

        /// <summary>上一个命令的拍点</summary>
        public int LastCommandBeatIndex { get; set; }

        public CommandContext()
        {
            RecentInputs = new List<InputSample>();
            CurrentBeatInputs = new List<InputSample>();
            PreviousBeatInputs = new List<InputSample>();
            CurrentFighterState = FighterState.Idle;
            CanCancel = true;
            LastCommand = CommandType.None;
            LastCommandBeatIndex = -1;
        }

        /// <summary>
        /// 检查当前拍是否有指定类型的输入
        /// </summary>
        public bool HasInputAtCurrentBeat(RhythmInputType inputType)
        {
            foreach (var input in CurrentBeatInputs)
            {
                if (input.inputType == inputType && !input.isConsumed)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查上一拍是否有指定类型的输入
        /// </summary>
        public bool HasInputAtPreviousBeat(RhythmInputType inputType)
        {
            foreach (var input in PreviousBeatInputs)
            {
                if (input.inputType == inputType && !input.isConsumed)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取当前拍的所有输入类型
        /// </summary>
        public List<RhythmInputType> GetCurrentBeatInputTypes()
        {
            var types = new List<RhythmInputType>();
            foreach (var input in CurrentBeatInputs)
            {
                if (!input.isConsumed && !types.Contains(input.inputType))
                {
                    types.Add(input.inputType);
                }
            }
            return types;
        }

        /// <summary>
        /// 获取当前拍最佳（最接近拍点）的输入
        /// </summary>
        public InputSample? GetBestInputAtCurrentBeat()
        {
            InputSample? best = null;
            float bestDelta = float.MaxValue;

            foreach (var input in CurrentBeatInputs)
            {
                if (!input.isConsumed)
                {
                    float absDelta = System.Math.Abs(input.deltaMs);
                    if (absDelta < bestDelta)
                    {
                        bestDelta = absDelta;
                        best = input;
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// 重置上下文
        /// </summary>
        public void Reset()
        {
            CurrentBeatIndex = 0;
            CurrentSongTime = 0f;
            CurrentFighterState = FighterState.Idle;
            RecentInputs.Clear();
            CurrentBeatInputs.Clear();
            PreviousBeatInputs.Clear();
            CanCancel = true;
            IsInHitstun = false;
            IsGuarding = false;
            LastCommand = CommandType.None;
            LastCommandBeatIndex = -1;
        }
    }
}