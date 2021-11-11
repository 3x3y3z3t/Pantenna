// ;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pantenna
{
    public enum SignalRelation
    {
        Unknown = 0,
        Ally,
        Neutral,
        Enemy
    }

    public enum SignalType
    {
        Unknown = 0,
        LargeGrid,
        SmallGrid,
        Character
    }

    public struct SignalData
    {
        public long EntityId;
        public SignalType SignalType;
        public SignalRelation Relation;
        public float Distance;
        public float Velocity;
        public string DisplayName;

        public SignalData(long _entityId, SignalType _type, SignalRelation _relation, float _distance, float _velocity, string _displayName)
        {
            EntityId = _entityId;
            SignalType = _type;
            Relation = _relation;
            Distance = _distance;
            Velocity = _velocity;
            DisplayName = _displayName;
        }

    }
}
