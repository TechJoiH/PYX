using System;

namespace ShadowRhythm.Data.Models
{
    [Serializable]
    public class JudgeWindowConfigModel
    {
        public float perfectMs;
        public float goodMs;
        public float missMs;
        public float parryMs;
    }
}