using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.Inventory;

namespace BountyBandits
{
    public class DropItem : GameItem
    {
        private Item item;
        public Item getItem() { return item; }
        public void setItem(Item item) { this.item = item; }
    }
}
