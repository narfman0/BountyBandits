using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BountyBandits
{
    public class DropItem : GameItem
    {
        private BountyBandits.Inventory.Item item;

        public BountyBandits.Inventory.Item getItem() { return item; }
        public void setItem(BountyBandits.Inventory.Item item) { this.item = item; }
    }
}
