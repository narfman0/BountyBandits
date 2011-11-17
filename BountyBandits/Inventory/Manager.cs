using System;
using System.Collections.Generic;
using System.Text;

namespace BountyBandits.Inventory
{
    public class Manager
    {
        Dictionary<Type, Item> items = new Dictionary<Type, Item>();

        /// <summary>Gets an Item from the item dictionary.</summary>
        /// <param name="type">The type of item to retrieve</param>
        public Item getItem(Type type) { 
            if(items.ContainsKey(type))
                return items[type];
            return null;
        }

        /// <summary>Puts an Item into the item dictionary.</summary>
        /// <param name="type">The type of item to put</param>
        /// <returns>If slot occupied, returns item, otherwise null</returns>
        public Item putItem(Item item)
        {
            Item old = null;
            if (items.ContainsKey(item.getType()))
            {
                old = items[item.getType()];
                items.Remove(item.getType());
            }
            items.Add(item.getType(), item);
            return old;
        }

        /// <summary>Return the integer sum of a stat.</summary>
        /// <param name="type">The type of stat to acquire</param>
        /// <returns>The total value of the stat</returns>
        public int getStatBonus(BountyBandits.Stats.Type type)
        {
            int total = 0;
            foreach (Item item in items.Values)
                total += item.getStats().getStatValue(type);
            return total;
        }
    }
}
