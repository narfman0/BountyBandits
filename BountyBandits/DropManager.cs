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
            BountyBandits.Inventory.Type type = rollType(being);
            BountyBandits.Inventory.Class itemClass = rollClass(being);
            Stats.Stats stats = rollStats(being, itemClass, type);
            String name = rollName(being, itemClass, type, stats);
            //Following: dirty stinking hack to get the names of the colors tacked on to texture name
            //String primaryColor = new Item(name, stats, type, "", itemClass).getPrimaryColor();
            //String secondaryColor = new Item(name, stats, type, "", itemClass).getSecondaryColor();
            
            String textureName = Enum.GetName(typeof(BountyBandits.Inventory.Type), type) + rand.Next(4);
            //if (itemClass != Class.Unique)
            //    textureName += primaryColor + secondaryColor;
            return new Item(name, stats, type, textureName, itemClass);
        }
        private static Inventory.Class rollClass(Being being)
        {
            //roll for item class/quality
            BountyBandits.Inventory.Class itemClass = Class.Normal;
            double randMod = rand.NextDouble();
            if (randMod < .001)
                itemClass = Class.Unique;
            else if (randMod < .015)
                itemClass = Class.Rare;
            else if (randMod < .20)
                itemClass = Class.Magic;
            return itemClass;
        }
        private static Inventory.Type rollType(Being being)
        {
            BountyBandits.Inventory.Type[] values = (BountyBandits.Inventory.Type[])Enum.GetValues(typeof(BountyBandits.Inventory.Type));
            return values[rand.Next(0, values.Length)];
        }
        private static Stats.Stats rollStats(Being being, Inventory.Class itemClass, Inventory.Type type)
        {
            Stats.Stats stats = new BountyBandits.Stats.Stats();
            #region Calculate how many random stat modifiers there shall be
            int randomStatItems = 0;
            switch (itemClass)
            {
                case Class.Normal:
                    randomStatItems++;
                    break;
                case Class.Magic:
                    randomStatItems+=1+rand.Next()%2;
                    break;
                case Class.Rare:
                    randomStatItems+=2+rand.Next()%2;
                    break;
                case Class.Unique:
                    randomStatItems+=4+rand.Next()%6;
                    break;
            }
            #endregion

            if(type == BountyBandits.Inventory.Type.Chest)
                stats.addStatValue(BountyBandits.Stats.Type.Defense, 20 + rand.Next()%10);
            if(type == BountyBandits.Inventory.Type.Head)
                stats.addStatValue(BountyBandits.Stats.Type.Defense, 5 + rand.Next()%5);
            if(type == BountyBandits.Inventory.Type.Legs)
                stats.addStatValue(BountyBandits.Stats.Type.Defense, 7 + rand.Next()%10);
            if(type == BountyBandits.Inventory.Type.MainHand){
                stats.addStatValue(BountyBandits.Stats.Type.MinDamage, 2);
                stats.addStatValue(BountyBandits.Stats.Type.MaxDamage, 2);
            }
            switch (itemClass)
            {
                case Class.Normal:
                    break;
                case Class.Magic:
                    if(stats.getStatValue(BountyBandits.Stats.Type.Defense) > 0)
                        stats.setStatValue(BountyBandits.Stats.Type.Defense, stats.getStatValue(BountyBandits.Stats.Type.Defense) + 3);
                    break;
                case Class.Rare:
                    if(stats.getStatValue(BountyBandits.Stats.Type.Defense) > 0)
                        stats.setStatValue(BountyBandits.Stats.Type.Defense, stats.getStatValue(BountyBandits.Stats.Type.Defense) * 2);
                    break;
                case Class.Unique:
                    if(stats.getStatValue(BountyBandits.Stats.Type.Defense) > 0)
                        stats.setStatValue(BountyBandits.Stats.Type.Defense, stats.getStatValue(BountyBandits.Stats.Type.Defense) * 3);
                    break;
            }
            #region Add that random amount of modifiers
            for (; randomStatItems > 0; randomStatItems--)
            {
                //get random modifier
                Stats.Type[] values = (Stats.Type[])Enum.GetValues(typeof(Stats.Type));
                Stats.Type stat = values[rand.Next(0, values.Length)];

                stats.addStatValue(stat, (int)(1 + rand.Next() % 8));
            }
            #endregion
            return stats;
        }
        private static String rollName(Being being, Inventory.Class itemClass, Inventory.Type type, Stats.Stats stats)
        {
            String name = "";
            if (type == BountyBandits.Inventory.Type.Chest)
                name = "Plate";
            if (type == BountyBandits.Inventory.Type.Head)
                name = "Helmet";
            if (type == BountyBandits.Inventory.Type.Legs)
                name = "Leggings";
            if (type == BountyBandits.Inventory.Type.MainHand)
                name = "Sword";
            if (type == BountyBandits.Inventory.Type.OffHand)
                name = "Trinket";
            switch (itemClass)
            {
                case Class.Normal:
                    break;
                case Class.Magic:
                case Class.Rare:
                    foreach (Stats.Type stat in Enum.GetValues(typeof(Stats.Type)))
                    {
                        if(stats.getStatValue(stat) > 0)
                            switch (stat)
                            {
                                case BountyBandits.Stats.Type.DamageReduction:
                                    name += " of the masochist";
                                    break;
                                case BountyBandits.Stats.Type.Agility:
                                    name += " of the fox";
                                    break;
                                case BountyBandits.Stats.Type.Defense:
                                    name += " of the bear";
                                    break;
                                case BountyBandits.Stats.Type.EnhancedDamage:
                                    name = "Slayer's " + name;
                                    break;
                                case BountyBandits.Stats.Type.Life:
                                    name = "Life giving " + name;
                                    break;
                                case BountyBandits.Stats.Type.LifeSteal:
                                    name = "Vampire's " + name;
                                    break;
                                case BountyBandits.Stats.Type.Speed:
                                    name = "Pheonix's " + name;
                                    break;
                                case BountyBandits.Stats.Type.Strength:
                                    name = "Vin Diesel's " + name;
                                    break;
                                default:
                                    name += " of " + stat.ToString();
                                    break;
                            }
                    }
                    break;
                case Class.Unique:
                    name += " of Doom";
                    break;
            }
            return name;
        }
    }
}
