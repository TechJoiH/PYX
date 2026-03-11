namespace ShadowRhythm.Command
{
    /// <summary>
    /// 命令类型枚举 - 定义所有可触发的动作命令
    /// </summary>
    public enum CommandType
    {
        None = 0,

        // ========== 单键命令 =========
        /// <summary>提 - 起跳/提势</summary>
        Lift = 1,

        /// <summary>拨 - 轻攻击</summary>
        Flick = 2,

        /// <summary>抖 - 碎步/短刺</summary>
        Shake = 3,

        /// <summary>闪 - 闪避</summary>
        Flash = 4,

        // ========== 同拍双键组合 =========
        /// <summary>拨+闪 - 冲刺斩</summary>
        DashSlash = 10,

        /// <summary>拨+提 - 上挑斩</summary>
        RisingStrike = 11,

        /// <summary>提+闪 - 弹反架势</summary>
        ParryGuard = 12,

        /// <summary>抖+闪 - 快速后撤</summary>
        QuickRetreat = 13,

        // ========== 两拍序列技 =========
        /// <summary>拨→抖 - 连刺</summary>
        ComboStab = 20,

        /// <summary>抖→拨 - 反击斩</summary>
        CounterSlash = 21,

        /// <summary>提→拨 - 跃斩</summary>
        JumpSlash = 22,

        /// <summary>闪→拨 - 闪击</summary>
        FlashStrike = 23,
    }

    /// <summary>
    /// 命令类型扩展方法
    /// </summary>
    public static class CommandTypeExtensions
    {
        /// <summary>
        /// 获取命令的中文名称
        /// </summary>
        public static string GetDisplayName(this CommandType type)
        {
            return type switch
            {
                CommandType.None => "无",
                CommandType.Lift => "提势",
                CommandType.Flick => "轻斩",
                CommandType.Shake => "短刺",
                CommandType.Flash => "闪避",
                CommandType.DashSlash => "冲刺斩",
                CommandType.RisingStrike => "上挑斩",
                CommandType.ParryGuard => "弹反",
                CommandType.QuickRetreat => "后撤",
                CommandType.ComboStab => "连刺",
                CommandType.CounterSlash => "反击斩",
                CommandType.JumpSlash => "跃斩",
                CommandType.FlashStrike => "闪击",
                _ => type.ToString()
            };
        }

        /// <summary>
        /// 获取命令优先级（数字越大优先级越高）
        /// </summary>
        public static int GetPriority(this CommandType type)
        {
            return type switch
            {
                // 序列技最高优先级
                CommandType.ComboStab => 30,
                CommandType.CounterSlash => 30,
                CommandType.JumpSlash => 30,
                CommandType.FlashStrike => 30,

                // 双键组合次之
                CommandType.DashSlash => 20,
                CommandType.RisingStrike => 20,
                CommandType.ParryGuard => 20,
                CommandType.QuickRetreat => 20,

                // 单键最低
                CommandType.Lift => 10,
                CommandType.Flick => 10,
                CommandType.Shake => 10,
                CommandType.Flash => 10,

                _ => 0
            };
        }

        /// <summary>
        /// 是否是组合技
        /// </summary>
        public static bool IsComboCommand(this CommandType type)
        {
            return (int)type >= 10;
        }

        /// <summary>
        /// 是否是序列技
        /// </summary>
        public static bool IsSequenceCommand(this CommandType type)
        {
            return (int)type >= 20;
        }
    }
}