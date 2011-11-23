using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BountyBandits.Stats
{
    public class StatSet
    {
        public Dictionary<StatType, Stat> statsTable;
        public StatSet()
        {
            statsTable = new Dictionary<StatType, Stat>();
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
                statsTable.Add(type, new Stat(type, 0));
        }
        public Stat getStat(StatType type) { return (Stat)statsTable[type]; }
        public int getStatValue(StatType type) { return getStat(type).getValue(); }
        public void setStatValue(StatType type, int value) { getStat(type).setValue(value); }
        public void addStatValue(StatType type, int addValue) { setStatValue(type, getStatValue(type) + addValue); }

        public void asXML(XmlElement parentNode)
        {
            XmlElement statsElement = parentNode.OwnerDocument.CreateElement("stats");
            foreach (StatType type in statsTable.Keys)
            {
                Stat item = statsTable[type];
                XmlElement itemElement = parentNode.OwnerDocument.CreateElement("stat");
                itemElement.SetAttribute("type", type.ToString());
                itemElement.Value = item.getValue().ToString();
                statsElement.AppendChild(itemElement);
            }
            parentNode.AppendChild(statsElement);
        }
    }
}
