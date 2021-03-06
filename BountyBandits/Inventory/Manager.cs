﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BountyBandits.Inventory
{
    public class InventoryManager
    {
        Dictionary<ItemType, Item> items = new Dictionary<ItemType, Item>();

        /// <summary>Gets an Item from the item dictionary.</summary>
        /// <param name="type">The type of item to retrieve</param>
        public Item getItem(ItemType type) { 
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
            if (items.ContainsKey(item.getItemType()))
            {
                old = items[item.getItemType()];
                items.Remove(item.getItemType());
            }
            items.Add(item.getItemType(), item);
            return old;
        }

        /// <summary>Return the integer sum of a stat.</summary>
        /// <param name="type">The type of stat to acquire</param>
        /// <returns>The total value of the stat</returns>
        public int getStatBonus(BountyBandits.Stats.StatType type)
        {
            int total = 0;
            foreach (Item item in items.Values)
                total += item.getStats().getStatValue(type);
            return total;
        }

        /// <summary>
        /// Represent this in XML
        /// </summary>
        /// <param name="parentNode">parent to be appended</param>
        public XmlNode asXML(XmlNode parentNode)
        {
            XmlElement inventoryElement = parentNode.OwnerDocument.CreateElement("inventory");
            foreach (ItemType type in items.Keys)
                inventoryElement.AppendChild(items[type].asXML(inventoryElement));
            return inventoryElement;
        }

        public static InventoryManager fromXML(XmlElement element)
        {
            InventoryManager manager = new InventoryManager();
            foreach (XmlElement itemElement in element.GetElementsByTagName("item"))
                manager.putItem(Item.fromXML(itemElement));
            return manager;
        }
    }
}
