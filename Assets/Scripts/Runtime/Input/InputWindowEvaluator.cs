using UnityEngine;
using ShadowRhythm.Data.Models;

namespace ShadowRhythm.Input
{
    /// <summary>
    /// 输入窗口判定器 - 根据时间偏差判定 Perfect/Good/Miss
    /// </summary>
    public sealed class InputWindowEvaluator
    {
        /// <summary>Perfect 窗口（毫秒）</summary>
        public float PerfectWindowMs { get; private set; }

        /// <summary>Good 窗口（毫秒）</summary>
        public float GoodWindowMs { get; private set; }

        /// <summary>Miss 窗口（毫秒）- 超出此范围不计入</summary>
        public float MissWindowMs { get; private set; }

        /// <summary>弹反窗口（毫秒）</summary>
        public float ParryWindowMs { get; private set; }

        /// <summary>
        /// 使用默认窗口初始化
        /// </summary>
        public InputWindowEvaluator()
        {
            // 默认判定窗口
            PerfectWindowMs = 60f;
            GoodWindowMs = 120f;
            MissWindowMs = 200f;
            ParryWindowMs = 80f;
        }

        /// <summary>
        /// 使用配置初始化
        /// </summary>
        public InputWindowEvaluator(JudgeWindowConfigModel config)
        {
            if (config != null)
            {
                PerfectWindowMs = config.perfectMs > 0 ? config.perfectMs : 60f;
                GoodWindowMs = config.goodMs > 0 ? config.goodMs : 120f;
                MissWindowMs = config.missMs > 0 ? config.missMs : 200f;
                ParryWindowMs = config.parryMs > 0 ? config.parryMs : 80f;
            }
            else
            {
                PerfectWindowMs = 60f;
                GoodWindowMs = 120f;
                MissWindowMs = 200f;
                ParryWindowMs = 80f;
            }

            Debug.Log($"[InputWindowEvaluator] 判定窗口 - Perfect:±{PerfectWindowMs}ms Good:±{GoodWindowMs}ms Miss:±{MissWindowMs}ms");
        }

        /// <summary>
        /// 更新判定窗口配置
        /// </summary>
        public void UpdateConfig(JudgeWindowConfigModel config)
        {
            if (config == null) return;

            PerfectWindowMs = config.perfectMs;
            GoodWindowMs = config.goodMs;
            MissWindowMs = config.missMs;
            ParryWindowMs = config.parryMs;
        }

        /// <summary>
        /// 根据偏差毫秒数判定结果
        /// </summary>
        /// <param name="deltaMs">距离最近拍点的偏差（毫秒），正数表示晚，负数表示早</param>
        /// <returns>判定结果</returns>
        public RhythmJudgeResult Evaluate(float deltaMs)
        {
            float absDelta = Mathf.Abs(deltaMs);

            if (absDelta <= PerfectWindowMs)
            {
                return RhythmJudgeResult.Perfect;
            }
            else if (absDelta <= GoodWindowMs)
            {
                return RhythmJudgeResult.Good;
            }
            else if (absDelta <= MissWindowMs)
            {
                return RhythmJudgeResult.Miss;
            }
            else
            {
                // 超出 Miss 窗口，仍返回 Miss（或可定义为 None）
                return RhythmJudgeResult.Miss;
            }
        }

        /// <summary>
        /// 判断是否在有效输入窗口内
        /// </summary>
        public bool IsWithinWindow(float deltaMs)
        {
            return Mathf.Abs(deltaMs) <= MissWindowMs;
        }

        /// <summary>
        /// 判断是否在弹反窗口内
        /// </summary>
        public bool IsWithinParryWindow(float deltaMs)
        {
            return Mathf.Abs(deltaMs) <= ParryWindowMs;
        }

        /// <summary>
        /// 获取判定结果的显示文本
        /// </summary>
        public static string GetResultText(RhythmJudgeResult result)
        {
            return result switch
            {
                RhythmJudgeResult.Perfect => "PERFECT",
                RhythmJudgeResult.Good => "GOOD",
                RhythmJudgeResult.Miss => "MISS",
                _ => ""
            };
        }

        /// <summary>
        /// 获取判定结果的颜色
        /// </summary>
        public static Color GetResultColor(RhythmJudgeResult result)
        {
            return result switch
            {
                RhythmJudgeResult.Perfect => new Color(1f, 0.84f, 0f),    // 金色
                RhythmJudgeResult.Good => new Color(0.2f, 0.8f, 0.2f),    // 绿色
                RhythmJudgeResult.Miss => new Color(0.8f, 0.2f, 0.2f),    // 红色
                _ => Color.white
            };
        }
    }
}