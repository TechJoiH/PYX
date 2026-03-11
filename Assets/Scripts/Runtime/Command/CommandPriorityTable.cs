using System.Collections.Generic;

namespace ShadowRhythm.Command
{
    /// <summary>
    /// 命令优先级表 - 定义命令之间的优先级和冲突解决规则
    /// </summary>
    public sealed class CommandPriorityTable
    {
        /// <summary>
        /// 优先级层级定义
        /// </summary>
        public static class PriorityLevels
        {
            public const int Sequence = 30;      // 序列技
            public const int Simultaneous = 20;  // 同拍组合
            public const int Single = 10;        // 单键命令
            public const int None = 0;           // 无效
        }

        private readonly Dictionary<CommandType, int> _customPriorities;

        public CommandPriorityTable()
        {
            _customPriorities = new Dictionary<CommandType, int>();
        }

        /// <summary>
        /// 获取命令优先级
        /// </summary>
        public int GetPriority(CommandType type)
        {
            // 先检查自定义优先级
            if (_customPriorities.TryGetValue(type, out int priority))
            {
                return priority;
            }

            // 使用默认优先级
            return type.GetPriority();
        }

        /// <summary>
        /// 设置自定义优先级
        /// </summary>
        public void SetPriority(CommandType type, int priority)
        {
            _customPriorities[type] = priority;
        }

        /// <summary>
        /// 比较两个命令的优先级，返回优先级更高的
        /// </summary>
        public CommandType CompareAndSelect(CommandType a, CommandType b)
        {
            int priorityA = GetPriority(a);
            int priorityB = GetPriority(b);

            if (priorityA >= priorityB)
                return a;
            return b;
        }

        /// <summary>
        /// 从多个候选命令中选择优先级最高的
        /// </summary>
        public CommandType SelectHighestPriority(params CommandType[] commands)
        {
            CommandType best = CommandType.None;
            int bestPriority = -1;

            foreach (var cmd in commands)
            {
                int priority = GetPriority(cmd);
                if (priority > bestPriority)
                {
                    bestPriority = priority;
                    best = cmd;
                }
            }

            return best;
        }

        /// <summary>
        /// 检查命令是否可以打断另一个命令
        /// </summary>
        public bool CanInterrupt(CommandType newCommand, CommandType currentCommand)
        {
            // None 不能打断任何命令
            if (newCommand == CommandType.None)
                return false;

            // 任何命令都可以打断 None
            if (currentCommand == CommandType.None)
                return true;

            // 高优先级可以打断低优先级
            return GetPriority(newCommand) > GetPriority(currentCommand);
        }

        /// <summary>
        /// 重置为默认优先级
        /// </summary>
        public void ResetToDefault()
        {
            _customPriorities.Clear();
        }
    }
}