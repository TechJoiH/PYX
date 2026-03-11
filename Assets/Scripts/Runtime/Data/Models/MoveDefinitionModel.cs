using System;
using System.Collections.Generic;

namespace ShadowRhythm.Data.Models
{
    [Serializable]
    public class MoveDefinitionModel
    {
        public string moveId;
        public string displayName;
        public string commandType;
        public int startupBeats;
        public int activeBeats;
        public int recoveryBeats;
        public int damage;
        public bool canParry;
        public bool canBeCancelled;
    }

    [Serializable]
    public class MoveDefinitionContainer
    {
        public List<MoveDefinitionModel> moves;
    }
}