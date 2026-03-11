using System;
using System.Collections.Generic;

namespace ShadowRhythm.Data.Models
{
    [Serializable]
    public class EnemyPatternModel
    {
        public string patternId;
        public string enemyId;
        public List<EnemyPatternStepModel> steps;
    }

    [Serializable]
    public class EnemyPatternStepModel
    {
        public int beatIndex;
        public string cueType;
        public string commandType;
        public string expectedResponse;
    }
}