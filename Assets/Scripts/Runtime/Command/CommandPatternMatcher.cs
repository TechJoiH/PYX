using System.Collections.Generic;
using ShadowRhythm.Input;
using UnityEngine;

namespace ShadowRhythm.Command
{
    /// <summary>
    /// 命令模式匹配结果
    /// </summary>
    public struct PatternMatchResult
    {
        public CommandType commandType;
        public int priority;
        public bool isPerfect;
        public float deltaMs;
        public RhythmInputType[] consumedInputs;
        public int[] consumedBeatIndices;

        public bool IsValid => commandType != CommandType.None;

        public static PatternMatchResult None => new PatternMatchResult
        {
            commandType = CommandType.None,
            priority = 0,
            isPerfect = false,
            deltaMs = 0f,
            consumedInputs = System.Array.Empty<RhythmInputType>(),
            consumedBeatIndices = System.Array.Empty<int>()
        };
    }

    /// <summary>
    /// 命令模式匹配器 - 负责匹配输入模式到具体命令
    /// </summary>
    public sealed class CommandPatternMatcher
    {
        /// <summary>同拍双键容差时间（毫秒）</summary>
        public float SimultaneousToleranceMs { get; set; } = 80f;

        /// <summary>序列技最大间隔拍数</summary>
        public int SequenceMaxBeatGap { get; set; } = 1;

        /// <summary>
        /// 尝试匹配所有模式，返回优先级最高的结果
        /// </summary>
        public PatternMatchResult TryMatch(CommandContext context)
        {
            var results = new List<PatternMatchResult>();

            // 1. 尝试匹配序列技（最高优先级）
            var sequenceResult = TryMatchSequence(context);
            if (sequenceResult.IsValid)
            {
                results.Add(sequenceResult);
            }

            // 2. 尝试匹配同拍双键组合
            var comboResult = TryMatchSimultaneous(context);
            if (comboResult.IsValid)
            {
                results.Add(comboResult);
            }

            // 3. 尝试匹配单键命令
            var singleResult = TryMatchSingle(context);
            if (singleResult.IsValid)
            {
                results.Add(singleResult);
            }

            // 返回优先级最高的结果
            if (results.Count == 0)
            {
                return PatternMatchResult.None;
            }

            results.Sort((a, b) => b.priority.CompareTo(a.priority));
            return results[0];
        }

        /// <summary>
        /// 匹配单键命令
        /// </summary>
        public PatternMatchResult TryMatchSingle(CommandContext context)
        {
            if (context.CurrentBeatInputs.Count == 0)
                return PatternMatchResult.None;

            // 获取当前拍最佳输入
            var bestInput = context.GetBestInputAtCurrentBeat();
            if (!bestInput.HasValue || bestInput.Value.isConsumed)
                return PatternMatchResult.None;

            var input = bestInput.Value;

            // ★ 关键修改：Miss 判定不触发命令
            if (input.judgeResult == RhythmJudgeResult.Miss)
                return PatternMatchResult.None;

            CommandType cmdType = input.inputType switch
            {
                RhythmInputType.Lift => CommandType.Lift,
                RhythmInputType.Flick => CommandType.Flick,
                RhythmInputType.Shake => CommandType.Shake,
                RhythmInputType.Flash => CommandType.Flash,
                _ => CommandType.None
            };

            if (cmdType == CommandType.None)
                return PatternMatchResult.None;

            return new PatternMatchResult
            {
                commandType = cmdType,
                priority = cmdType.GetPriority(),
                isPerfect = input.judgeResult == RhythmJudgeResult.Perfect,
                deltaMs = input.deltaMs,
                consumedInputs = new[] { input.inputType },
                consumedBeatIndices = new[] { input.quantizedBeatIndex }
            };
        }

        /// <summary>
        /// 匹配同拍双键组合
        /// </summary>
        public PatternMatchResult TryMatchSimultaneous(CommandContext context)
        {
            var inputTypes = context.GetCurrentBeatInputTypes();
            if (inputTypes.Count < 2)
                return PatternMatchResult.None;

            // 检查时间容差
            if (!AreInputsSimultaneous(context.CurrentBeatInputs, SimultaneousToleranceMs))
                return PatternMatchResult.None;

            // ★ 关键修改：检查是否有有效输入（非 Miss）
            if (!HasValidInput(context.CurrentBeatInputs))
                return PatternMatchResult.None;

            CommandType cmdType = CommandType.None;

            // 拨+闪 = 冲刺斩
            if (inputTypes.Contains(RhythmInputType.Flick) && inputTypes.Contains(RhythmInputType.Flash))
            {
                cmdType = CommandType.DashSlash;
            }
            // 拨+提 = 上挑斩
            else if (inputTypes.Contains(RhythmInputType.Flick) && inputTypes.Contains(RhythmInputType.Lift))
            {
                cmdType = CommandType.RisingStrike;
            }
            // 提+闪 = 弹反
            else if (inputTypes.Contains(RhythmInputType.Lift) && inputTypes.Contains(RhythmInputType.Flash))
            {
                cmdType = CommandType.ParryGuard;
            }
            // 抖+闪 = 快速后撤
            else if (inputTypes.Contains(RhythmInputType.Shake) && inputTypes.Contains(RhythmInputType.Flash))
            {
                cmdType = CommandType.QuickRetreat;
            }

            if (cmdType == CommandType.None)
                return PatternMatchResult.None;

            // 获取最佳时机
            var bestInput = context.GetBestInputAtCurrentBeat();
            bool isPerfect = bestInput.HasValue && bestInput.Value.judgeResult == RhythmJudgeResult.Perfect;
            float deltaMs = bestInput.HasValue ? bestInput.Value.deltaMs : 0f;

            return new PatternMatchResult
            {
                commandType = cmdType,
                priority = cmdType.GetPriority(),
                isPerfect = isPerfect,
                deltaMs = deltaMs,
                consumedInputs = inputTypes.ToArray(),
                consumedBeatIndices = new[] { context.CurrentBeatIndex }
            };
        }

        /// <summary>
        /// 匹配连续拍序列技
        /// </summary>
        public PatternMatchResult TryMatchSequence(CommandContext context)
        {
            // 需要上一拍和当前拍的输入
            if (context.PreviousBeatInputs.Count == 0 || context.CurrentBeatInputs.Count == 0)
                return PatternMatchResult.None;

            // 检查拍点间隔
            int beatGap = context.CurrentBeatIndex - GetBeatIndexFromInputs(context.PreviousBeatInputs);
            if (beatGap > SequenceMaxBeatGap)
                return PatternMatchResult.None;

            // 获取上一拍和当前拍的主要输入
            var prevInput = GetPrimaryInput(context.PreviousBeatInputs);
            var currInput = GetPrimaryInput(context.CurrentBeatInputs);

            if (!prevInput.HasValue || !currInput.HasValue)
                return PatternMatchResult.None;

            if (prevInput.Value.isConsumed || currInput.Value.isConsumed)
                return PatternMatchResult.None;

            // ★ 关键修改：两个输入都必须是非 Miss
            if (prevInput.Value.judgeResult == RhythmJudgeResult.Miss ||
                currInput.Value.judgeResult == RhythmJudgeResult.Miss)
                return PatternMatchResult.None;

            CommandType cmdType = CommandType.None;

            // 拨→抖 = 连刺
            if (prevInput.Value.inputType == RhythmInputType.Flick &&
                currInput.Value.inputType == RhythmInputType.Shake)
            {
                cmdType = CommandType.ComboStab;
            }
            // 抖→拨 = 反击斩
            else if (prevInput.Value.inputType == RhythmInputType.Shake &&
                     currInput.Value.inputType == RhythmInputType.Flick)
            {
                cmdType = CommandType.CounterSlash;
            }
            // 提→拨 = 跃斩
            else if (prevInput.Value.inputType == RhythmInputType.Lift &&
                     currInput.Value.inputType == RhythmInputType.Flick)
            {
                cmdType = CommandType.JumpSlash;
            }
            // 闪→拨 = 闪击
            else if (prevInput.Value.inputType == RhythmInputType.Flash &&
                     currInput.Value.inputType == RhythmInputType.Flick)
            {
                cmdType = CommandType.FlashStrike;
            }

            if (cmdType == CommandType.None)
                return PatternMatchResult.None;

            // 以当前拍点时机为准
            bool isPerfect = currInput.Value.judgeResult == RhythmJudgeResult.Perfect;

            return new PatternMatchResult
            {
                commandType = cmdType,
                priority = cmdType.GetPriority(),
                isPerfect = isPerfect,
                deltaMs = currInput.Value.deltaMs,
                consumedInputs = new[] { prevInput.Value.inputType, currInput.Value.inputType },
                consumedBeatIndices = new[] { prevInput.Value.quantizedBeatIndex, currInput.Value.quantizedBeatIndex }
            };
        }

        /// <summary>
        /// 检查输入列表中是否有有效输入（非 Miss）
        /// </summary>
        private bool HasValidInput(List<InputSample> inputs)
        {
            foreach (var input in inputs)
            {
                if (!input.isConsumed && input.judgeResult != RhythmJudgeResult.Miss)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查输入是否在时间容差内同时发生
        /// </summary>
        private bool AreInputsSimultaneous(List<InputSample> inputs, float toleranceMs)
        {
            if (inputs.Count < 2) return false;

            float? firstTime = null;
            foreach (var input in inputs)
            {
                if (input.isConsumed) continue;

                if (firstTime == null)
                {
                    firstTime = input.pressedSongTime;
                }
                else
                {
                    float diffMs = Mathf.Abs(input.pressedSongTime - firstTime.Value) * 1000f;
                    if (diffMs > toleranceMs)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 获取输入列表中的主要输入（最接近拍点）
        /// </summary>
        private InputSample? GetPrimaryInput(List<InputSample> inputs)
        {
            InputSample? best = null;
            float bestDelta = float.MaxValue;

            foreach (var input in inputs)
            {
                // ★ 关键修改：跳过 Miss 判定的输入
                if (!input.isConsumed && input.judgeResult != RhythmJudgeResult.Miss)
                {
                    float absDelta = Mathf.Abs(input.deltaMs);
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
        /// 从输入列表获取拍点序号
        /// </summary>
        private int GetBeatIndexFromInputs(List<InputSample> inputs)
        {
            if (inputs.Count == 0) return -1;
            return inputs[0].quantizedBeatIndex;
        }
    }
}