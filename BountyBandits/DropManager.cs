using System;
using System.Collections.Generic;
using System.Text;
using BountyBandits.Inventory;
using BountyBandits.Stats;
using BountyBandits.Character;

namespace BountyBandits
{
    public class DropManager
    {
        private static Random rand = new Random();
        public static Item generateItem(Being being)
        {
            ItemType type = rollType(being);
            ItemClass itemClass = rollClass(being);
            StatSet stats = rollStats(being, itemClass, type);
            String name = rollName(being, itemClass, type, stats);
            String textureName = Enum.GetName(typeof(ItemType), type) + rand.Next(4);
            return new Item(name, stats, type, textureName, itemClass);
        }
        private static ItemClass rollClass(Being being)
        {
            //roll for item class/quality
            ItemClass itemClass = ItemClass.Normal;
            double randMod = rand.NextDouble();
            if (randMod < .001)
                itemClass = ItemClass.Unique;
            else if (randMod < .015)
                itemClass = ItemClass.Rare;
            else if (randMod < .20)
                itemClass = ItemClass.Magic;
            return itemClass;
        }
        private static ItemType rollType(Being being)
        {
            ItemType[] values = (ItemType[])Enum.GetValues(typeof(ItemType));
            return values[rand.Next(0, values.Length)];
        }
        private static StatSet rollStats(Being being, ItemClass itemClass, ItemType type)
        {
            StatSet stats = new StatSet();
            #region Calculate how many random stat modifiers there shall be
            int randomStatItems = 0;
            switch (itemClass)
            {
                case ItemClass.Normal:
                    randomStatItems++;
                    break;
                case ItemClass.Magic:
                    randomStatItems+=1+rand.Next()%2;
                    break;
                case ItemClass.Rare:
                    randomStatItems+=2+rand.Next()%2;
                    break;
                case ItemClass.Unique:
                    randomStatItems+=4+rand.Next()%6;
                    break;
            }
            #endregion

            if(type == ItemType.Chest)
                stats.addStatValue(StatType.Defense, 20 + rand.Next()%10);
            if(type == ItemType.Head)
                stats.addStatValue(StatType.Defense, 5 + rand.Next()%5);
            if(type == ItemType.Legs)
                stats.addStatValue(StatType.Defense, 7 + rand.Next()%10);
            if(type == ItemType.MainHand){
                stats.addStatValue(StatType.MinDamage, 2);
                stats.addStatValue(StatType.MaxDamage, 2);
            }
            switch (itemClass)
            {
                case ItemClass.Normal:
                    break;
                case ItemClass.Magic:
                    if(stats.getStatValue(StatType.Defense) > 0)
                        stats.setStatValue(StatType.Defense, stats.getStatValue(StatType.Defense) + 3);
                    break;
                case ItemClass.Rare:
                    if(stats.getStatValue(StatType.Defense) > 0)
                        stats.setStatValue(StatType.Defense, stats.getStatValue(StatType.Defense) * 2);
                    break;
                case ItemClass.Unique:
                    if(stats.getStatValue(StatType.Defense) > 0)
                        stats.setStatValue(StatType.Defense, stats.getStatValue(StatType.Defense) * 3);
                    break;
            }
            #region Add that random amount of modifiers
            for (; randomStatItems > 0; randomStatItems--)
            {
                //get random modifier
                StatType[] values = (Stats.StatType[])Enum.GetValues(typeof(StatType));
                StatType stat = values[rand.Next(0, values.Length)];

                stats.addStatValue(stat, (int)(1 + rand.Next() % 8));
            }
            #endregion
            return stats;
        }
        private static String rollName(Being being, ItemClass itemClass, ItemType type, StatSet stats)
        {
            String name = "";
            if (type == ItemType.Chest)
                name = "Plate";
            if (type == ItemType.Head)
                name = "Helmet";
            if (type == ItemType.Legs)
                name = "Leggings";
            if (type == ItemType.MainHand)
                name = "Sword";
            if (type == ItemType.OffHand)
                name = "Trinket";
            switch (itemClass)
            {
                case ItemClass.Normal:
                    break;
                case ItemClass.Magic:
                case ItemClass.Rare:
                    foreach (Stats.StatType stat in Enum.GetValues(typeof(Stats.StatType)))
                    {
                        if(stats.getStatValue(stat) > 0)
                            switch (stat)
                            {
                                case StatType.DamageReduction:
                                    name += " of the masochist";
                                    break;
                                case StatType.Agility:
                                    name += " of the fox";
                                    break;
                                case StatType.Defense:
                                    name += " of the bear";
                                    break;
                                case StatType.EnhancedDamage:
                                    name = "Slayer's " + name;
                                    break;
                                case StatType.Life:
                                    name = "Life giving " + name;
                                    break;
                                case StatType.LifeSteal:
                                    name = "Vampire's " + name;
                                    break;
                                case StatType.Speed:
                                    name = "Pheonix's " + name;
                                    break;
                                case StatType.Strength:
                                    name = "Vin Diesel's " + name;
                                    break;
                                default:
                                    name += " of " + stat.ToString();
                                    break;
                            }
                    }
                    break;
                case ItemClass.Unique:
                    name += " of Doom";
                    break;
            }
            return name;
        }
    }
}
