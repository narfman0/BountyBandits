using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits.Stats
{
    public class Stat
    {
        Type type;
        int value = 0;
        public Stat(Type type, int value)
        {
            this.type = type;
            this.value = value;
        }
        public int getValue() { return value; }
        public void setValue(int value) { this.value = value; }
        public Type getType() { return type; }
    }
    public class Buff : Stat
    {
        int timeOfExpire;
        public Buff(Type type, int value, int timeOfExpire)
            : base(type, value)
        {
            this.timeOfExpire = timeOfExpire;
        }
    }
    public class Aura : Stat
    {
        int radius;
        public Aura(Type type, int value, int radius)
            : base(type, value)
        {
            this.radius = radius;
        }
    }
    public class BuffAura : Stat
    {
        int radius;
        int timeOfExpire;
        public BuffAura(Type type, int value, int timeOfExpire, int radius)
            : base(type, value)
        {
            this.radius = radius;
            this.timeOfExpire = timeOfExpire;
        }
    }
}
