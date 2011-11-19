using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Stats
{
    public class Stat
    {
        StatType type;
        int value = 0;
        public Stat(StatType type, int value)
        {
            this.type = type;
            this.value = value;
        }
        public int getValue() { return value; }
        public void setValue(int value) { this.value = value; }
        public StatType getType() { return type; }
    }
    public class Buff : Stat
    {
        int timeOfExpire;
        public Buff(StatType type, int value, int timeOfExpire)
            : base(type, value)
        {
            this.timeOfExpire = timeOfExpire;
        }
    }
    public class Aura : Stat
    {
        int radius;
        public Aura(StatType type, int value, int radius)
            : base(type, value)
        {
            this.radius = radius;
        }
    }
    public class BuffAura : Stat
    {
        int radius;
        int timeOfExpire;
        public BuffAura(StatType type, int value, int timeOfExpire, int radius)
            : base(type, value)
        {
            this.radius = radius;
            this.timeOfExpire = timeOfExpire;
        }
    }
}
