using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BountyBandits.Stats
{
    public class Stats
    {
        Hashtable statsTable;
        public Stats()
        {
            statsTable = new Hashtable();
            foreach (BountyBandits.Stats.Type type in Enum.GetValues(typeof(Type)))
                statsTable.Add(type, new Stat(type, 0));
        }
        public Stat getStat(Type type) { return (Stat)statsTable[type]; }
        public int getStatValue(Type type) { return getStat(type).getValue(); }
        public void setStatValue(Type type, int value) { getStat(type).setValue(value); }
        public void addStatValue(Type type, int addValue) { setStatValue(type, getStatValue(type) + addValue); }
    }
}
