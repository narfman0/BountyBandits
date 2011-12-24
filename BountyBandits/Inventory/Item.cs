using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using BountyBandits.Stats;
using System.Xml;
using Microsoft.Xna.Framework;

namespace BountyBandits.Inventory
{
    public class Item
    {
        private String name, textureName;   //for rendering purposes both when dropped and when equipping
        private StatSet stats = new StatSet();
        private ItemType type;
        private ItemClass itemClass;
        public Item(String name, StatSet stats, ItemType type, String textureName, ItemClass itemClass)
        {
            this.name = name;
            this.stats = stats;
            this.textureName = textureName;
            this.type = type;
            this.itemClass = itemClass;
        }
        public String getName() { return name; }
        public Color getPrimaryColor()
        {
            const byte color = 230;
            const byte antiColor = color - 50;
            if (stats.getStatValue(StatType.AbilityLevel) > 0)
                return new Color(color, antiColor, color);
            if (stats.getStatValue(StatType.EnhancedDamage) > 0 ||
                stats.getStatValue(StatType.LifeSteal) > 0)
                return new Color(color, antiColor, antiColor);
            if (stats.getStatValue(StatType.Defense) > 0 ||
                stats.getStatValue(StatType.EnhancedDefense) > 0 ||
                stats.getStatValue(StatType.DamageReduction) > 0)
                return new Color(antiColor, color, antiColor);
            if (stats.getStatValue(StatType.Magic) > 0 ||
                stats.getStatValue(StatType.Speed) > 0 ||
                stats.getStatValue(StatType.Speed) > 0)
                return new Color(antiColor, antiColor, color);
            if (stats.getStatValue(StatType.Knockback) > 0 ||
                stats.getStatValue(StatType.MinDamage) > 0 ||
                stats.getStatValue(StatType.MaxDamage) > 0)
                return new Color(color, color, color);
            return new Color(color, color, color);
        }
        public Color getSecondaryColor()
        {
            if (getPrimaryColor() == Color.Purple)
                return Color.Lavender;
            if (getPrimaryColor() == Color.Red)
                return Color.Maroon;
            if (getPrimaryColor() == Color.Green)
                return Color.ForestGreen;
            if (getPrimaryColor() == Color.Blue)
                return Color.DarkBlue;
            if (getPrimaryColor() == Color.Gray)
                return Color.DarkGray;
            Color primary = getPrimaryColor();
            return Color.Black;// new Color(primary.R + 5, primary.G + 5, primary.B + 5);
        }
        public StatSet getStats() { return stats; }
        public String getTextureName() { return textureName; }
        public ItemType getItemType() { return type; }
        public ItemClass getItemClass() { return itemClass; }
        public XmlNode asXML(XmlNode parentNode)
        {
            XmlElement itemElement = parentNode.OwnerDocument.CreateElement("item");
            itemElement.SetAttribute("type", type.ToString());
            itemElement.SetAttribute("class", itemClass.ToString());
            itemElement.SetAttribute("name", name);
            itemElement.SetAttribute("textureName", textureName);
            itemElement.AppendChild(stats.asXML(itemElement));
            return itemElement;
        }
        public static Item fromXML(XmlElement element)
        {
            ItemType type = (ItemType)Enum.Parse(typeof(ItemType), element.GetAttribute("type"));
            ItemClass itemClass = (ItemClass)Enum.Parse(typeof(ItemClass), element.GetAttribute("class"));
            String name = element.GetAttribute("name"), textureName = element.GetAttribute("textureName");
            StatSet stats = StatSet.fromXML((XmlElement)element.GetElementsByTagName("stats").Item(0));
            return new Item(name, stats, type, textureName, itemClass);
        }
    }
}
