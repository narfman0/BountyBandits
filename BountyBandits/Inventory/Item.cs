using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.Inventory
{
    public class Item
    {
        private String name, textureName;   //for rendering purposes both when dropped and when equipping
        private BountyBandits.Stats.Stats stats = new BountyBandits.Stats.Stats();
        private Type type;
        private BountyBandits.Inventory.Class itemClass;
        public Item(String name, BountyBandits.Stats.Stats stats, Type type, String textureName, BountyBandits.Inventory.Class itemClass)
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
            if (stats.getStatValue(BountyBandits.Stats.Type.AbilityLevel) > 0)
                return new Color(color, antiColor, color);
            if (stats.getStatValue(BountyBandits.Stats.Type.EnhancedDamage) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.LifeSteal) > 0)
                return new Color(color, antiColor, antiColor);
            if (stats.getStatValue(BountyBandits.Stats.Type.Defense) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.EnhanecdDefense) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.DamageReduction) > 0)
                return new Color(antiColor, color, antiColor);
            if (stats.getStatValue(BountyBandits.Stats.Type.Magic) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.Speed) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.Speed) > 0)
                return new Color(antiColor, antiColor, color);
            if (stats.getStatValue(BountyBandits.Stats.Type.Knockback) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.MinDamage) > 0 ||
                stats.getStatValue(BountyBandits.Stats.Type.MaxDamage) > 0)
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
        public BountyBandits.Stats.Stats getStats() { return stats; }
        public String getTextureName() { return textureName; }
        public Type getType() { return type; }
        public Class getClass() { return itemClass; }
    }
}
