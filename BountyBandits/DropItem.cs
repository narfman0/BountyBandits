using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.Inventory;
using System.Xml;

namespace BountyBandits
{
    public class DropItem : GameItem
    {
        private Item item;
        public Item getItem() { return item; }
        public void setItem(Item item)
        {
            this.item = item;
            this.name = item.getTextureName();
        }
        public new static DropItem fromXML(XmlElement element)
        {
            DropItem item = new DropItem();
            item.copyValues((XmlElement)element.GetElementsByTagName("gameItem")[0]);
            item.setItem(Item.fromXML((XmlElement)element.GetElementsByTagName("item")[0]));
            return item;
        }
        public override XmlElement asXML(XmlDocument doc)
        {
            XmlElement beingElement = doc.CreateElement("dropItem");
            beingElement.AppendChild(item.asXML(beingElement));
            beingElement.AppendChild(base.asXML(doc));
            return beingElement;
        }
    }
}
