using System;
using System.Collections.Generic;
using UnityEngine;

namespace ShadowRhythm.Input
{
    /// <summary>
    /// 输入缓冲区 - 保存最近几拍的输入用于组合技判定
    /// </summary>
    public sealed class InputBuffer
    {
        /// <summary>缓冲区容量（保留多少拍的输入）</summary>
        public int BufferBeatCount { get; private set; }

        /// <summary>当前缓冲区中的输入数量</summary>
        public int Count => _samples.Count;

        private readonly List<InputSample> _samples;
        private readonly object _lock = new object();

        /// <summary>新输入事件</summary>
        public event Action<InputSample> OnInputAdded;

        /// <summary>
        /// 创建输入缓冲区
        /// </summary>
        /// <param name="bufferBeatCount">保留多少拍的输入（默认8拍）</param>
        public InputBuffer(int bufferBeatCount = 8)
        {
            BufferBeatCount = bufferBeatCount;
            _samples = new List<InputSample>(32);
        }

        /// <summary>
        /// 添加输入采样
        /// </summary>
        public void Push(InputSample sample)
        {
            lock (_lock)
            {
                _samples.Add(sample);
                OnInputAdded?.Invoke(sample);
            }
        }

        /// <summary>
        /// 获取所有缓冲的输入（只读）
        /// </summary>
        public IReadOnlyList<InputSample> GetAllSamples()
        {
            lock (_lock)
            {
                return _samples.AsReadOnly();
            }
        }

        /// <summary>
        /// 获取指定拍点的所有输入
        /// </summary>
        public List<InputSample> GetSamplesAtBeat(int beatIndex)
        {
            var result = new List<InputSample>();
            lock (_lock)
            {
                foreach (var sample in _samples)
                {
                    if (sample.quantizedBeatIndex == beatIndex)
                    {
                        result.Add(sample);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取最近 N 拍的输入
        /// </summary>
        public List<InputSample> GetRecentSamples(int currentBeatIndex, int beatRange = 4)
        {
            var result = new List<InputSample>();
            int minBeat = currentBeatIndex - beatRange;

            lock (_lock)
            {
                foreach (var sample in _samples)
                {
                    if (sample.quantizedBeatIndex >= minBeat && sample.quantizedBeatIndex <= currentBeatIndex)
                    {
                        result.Add(sample);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取最近 N 个未消耗的输入
        /// </summary>
        public List<InputSample> GetUnconsumedSamples(int count = 8)
        {
            var result = new List<InputSample>();
            lock (_lock)
            {
                for (int i = _samples.Count - 1; i >= 0 && result.Count < count; i--)
                {
                    if (!_samples[i].isConsumed)
                    {
                        result.Insert(0, _samples[i]);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 标记指定拍点的输入为已消耗
        /// </summary>
        public void MarkConsumed(int beatIndex, RhythmInputType inputType)
        {
            lock (_lock)
            {
                for (int i = 0; i < _samples.Count; i++)
                {
                    var sample = _samples[i];
                    if (sample.quantizedBeatIndex == beatIndex &&
                        sample.inputType == inputType &&
                        !sample.isConsumed)
                    {
                        sample.MarkConsumed();
                        _samples[i] = sample;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 清理过期的输入（保留最近 N 拍）
        /// </summary>
        public void TrimOldSamples(int currentBeatIndex)
        {
            int minBeatToKeep = currentBeatIndex - BufferBeatCount;

            lock (_lock)
            {
                _samples.RemoveAll(s => s.quantizedBeatIndex < minBeatToKeep);
            }
        }

        /// <summary>
        /// 检查指定拍点是否有特定类型的输入
        /// </summary>
        public bool HasInputAtBeat(int beatIndex, RhythmInputType inputType)
        {
            lock (_lock)
            {
                foreach (var sample in _samples)
                {
                    if (sample.quantizedBeatIndex == beatIndex && sample.inputType == inputType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 检查最近一拍是否有同时按下的多个键（用于组合技）
        /// </summary>
        public List<RhythmInputType> GetSimultaneousInputs(int beatIndex, float toleranceMs = 50f)
        {
            var result = new List<RhythmInputType>();
            float? firstTime = null;

            lock (_lock)
            {
                foreach (var sample in _samples)
                {
                    if (sample.quantizedBeatIndex == beatIndex)
                    {
                        if (firstTime == null)
                        {
                            firstTime = sample.pressedSongTime;
                            result.Add(sample.inputType);
                        }
                        else if (Mathf.Abs(sample.pressedSongTime - firstTime.Value) * 1000f <= toleranceMs)
                        {
                            if (!result.Contains(sample.inputType))
                            {
                                result.Add(sample.inputType);
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取最后一个输入
        /// </summary>
        public InputSample? GetLastSample()
        {
            lock (_lock)
            {
                if (_samples.Count > 0)
                {
                    return _samples[_samples.Count - 1];
                }
            }
            return null;
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _samples.Clear();
            }
        }

        /// <summary>
        /// 获取缓冲区统计信息
        /// </summary>
        public string GetStats()
        {
            lock (_lock)
            {
                int perfect = 0, good = 0, miss = 0;
                foreach (var s in _samples)
                {
                    switch (s.judgeResult)
                    {
                        case RhythmJudgeResult.Perfect: perfect++; break;
                        case RhythmJudgeResult.Good: good++; break;
                        case RhythmJudgeResult.Miss: miss++; break;
                    }
                }
                return $"Buffer: {_samples.Count} inputs | P:{perfect} G:{good} M:{miss}";
            }
        }
    }
}