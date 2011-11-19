using System;
using System.Collections.Generic;
using System.Text;
using BountyBandits.Inventory;

namespace BountyBandits
{
    public class DropManager
    {
        private static Random rand = new Random();
        public static Item generateItem(Being being)
        {
            //TODO being passed in does nothing. boring.
            BountyBandits.Inventory.ItemType type = rollType(being);
            BountyBandits.Inventory.ItemClass itemClass = rollClass(being);
            Stats.StatSet stats = rollStats(being, itemClass, type);
            String name = rollName(being, itemClass, type, stats);
            //Following: dirty stinking hack to get the names of the colors tacked on to texture name
            //String primaryColor = new Item(name, stats, type, "", itemClass).getPrimaryColor();
            //String secondaryColor = new Item(name, stats, type, "", itemClass).getSecondaryColor();
            
            String textureName = Enum.GetName(typeof(BountyBandits.Inventory.ItemType), type) + rand.Next(4);
            //if (itemClass != Class.Unique)
            //    textureName += primaryColor + secondaryColor;
            return new Item(name, stats, type, textureName, itemClass);
        }
        private static Inventory.ItemClass rollClass(Being being)
        {
            //roll for item class/quality
            BountyBandits.Inventory.ItemClass itemClass = ItemClass.Normal;
            double randMod = rand.NextDouble();
            if (randMod < .001)
                itemClass = ItemClass.Unique;
            else if (randMod < .015)
                itemClass = ItemClass.Rare;
            else if (randMod < .20)
                itemClass = ItemClass.Magic;
            return itemClass;
        }
        private static Inventory.ItemType rollType(Being being)
        {
            BountyBandits.Inventory.ItemType[] values = (BountyBandits.Inventory.ItemType[])Enum.GetValues(typeof(BountyBandits.Inventory.ItemType));
            return values[rand.Next(0, values.Length)];
        }
        private static Stats.StatSet rollStats(Being being, Inventory.ItemClass itemClass, Inventory.ItemType type)
        {
            Stats.StatSet stats = new BountyBandits.Stats.StatSet();
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

            if(type == BountyBandits.Inventory.ItemType.Chest)
                stats.addStatValue(BountyBandits.Stats.StatType.Defense, 20 + rand.Next()%10);
            if(type == BountyBandits.Inventory.ItemType.Head)
                stats.addStatValue(BountyBandits.Stats.StatType.Defense, 5 + rand.Next()%5);
            if(type == BountyBandits.Inventory.ItemType.Legs)
                stats.addStatValue(BountyBandits.Stats.StatType.Defense, 7 + rand.Next()%10);
            if(type == BountyBandits.Inventory.ItemType.MainHand){
                stats.addStatValue(BountyBandits.Stats.StatType.MinDamage, 2);
                stats.addStatValue(BountyBandits.Stats.StatType.MaxDamage, 2);
            }
            switch (itemClass)
            {
                case ItemClass.Normal:
                    break;
                case ItemClass.Magic:
                    if(stats.getStatValue(BountyBandits.Stats.StatType.Defense) > 0)
                        stats.setStatValue(BountyBandits.Stats.StatType.Defense, stats.getStatValue(BountyBandits.Stats.StatType.Defense) + 3);
                    break;
                case ItemClass.Rare:
                    if(stats.getStatValue(BountyBandits.Stats.StatType.Defense) > 0)
                        stats.setStatValue(BountyBandits.Stats.StatType.Defense, stats.getStatValue(BountyBandits.Stats.StatType.Defense) * 2);
                    break;
                case ItemClass.Unique:
                    if(stats.getStatValue(BountyBandits.Stats.StatType.Defense) > 0)
                        stats.setStatValue(BountyBandits.Stats.StatType.Defense, stats.getStatValue(BountyBandits.Stats.StatType.Defense) * 3);
                    break;
            }
            #region Add that random amount of modifiers
            for (; randomStatItems > 0; randomStatItems--)
            {
                //get random modifier
                Stats.StatType[] values = (Stats.StatType[])Enum.GetValues(typeof(Stats.StatType));
                Stats.StatType stat = values[rand.Next(0, values.Length)];

                stats.addStatValue(stat, (int)(1 + rand.Next() % 8));
            }
            #endregion
            return stats;
        }
        private static String rollName(Being being, Inventory.ItemClass itemClass, Inventory.ItemType type, Stats.StatSet stats)
        {
            String name = "";
            if (type == BountyBandits.Inventory.ItemType.Chest)
                name = "Plate";
            if (type == BountyBandits.Inventory.ItemType.Head)
                name = "Helmet";
            if (type == BountyBandits.Inventory.ItemType.Legs)
                name = "Leggings";
            if (type == BountyBandits.Inventory.ItemType.MainHand)
                name = "Sword";
            if (type == BountyBandits.Inventory.ItemType.OffHand)
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
                                case BountyBandits.Stats.StatType.DamageReduction:
                                    name += " of the masochist";
                                    break;
                                case BountyBandits.Stats.StatType.Agility:
                                    name += " of the fox";
                                    break;
                                case BountyBandits.Stats.StatType.Defense:
                                    name += " of the bear";
                                    break;
                                case BountyBandits.Stats.StatType.EnhancedDamage:
                                    name = "Slayer's " + name;
                                    break;
                                case BountyBandits.Stats.StatType.Life:
                                    name = "Life giving " + name;
                                    break;
                                case BountyBandits.Stats.StatType.LifeSteal:
                                    name = "Vampire's " + name;
                                    break;
                                case BountyBandits.Stats.StatType.Speed:
                                    name = "Pheonix's " + name;
                                    break;
                                case BountyBandits.Stats.StatType.Strength:
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
