using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using System.Xml;

namespace BountyBandits
{
    public enum PhysicsPolygonType
    {
        Rectangle, Circle, Polygon
    }

    public class GameItem
    {
        public string name;
        public uint weight = 1, radius = 10, startdepth = 0, width = 1;
        public Guid guid;
        public Vector2 loc, sideLengths = Vector2.One;
        public Body body;
        public float rotation = 0f;
        public bool immovable = false;
        public PhysicsPolygonType polygonType;
        public GameItem() 
        {
            guid = Guid.NewGuid();
        }
        public void copyValues(XmlElement element)
        {
            if (element.HasAttributes && element.Attributes.GetNamedItem("rotation").Value != null)
                rotation = float.Parse(element.Attributes.GetNamedItem("rotation").Value);
            foreach (XmlElement itemChild in element)
                if (itemChild.Name.Equals("name"))
                    name = itemChild.FirstChild.Value;
                else if (itemChild.Name.Equals("loc"))
                {
                    string[] locStr = itemChild.FirstChild.Value.Split(',');
                    loc = new Vector2(float.Parse(locStr[0]), float.Parse(locStr[1]));
                }
                else if (itemChild.Name.Equals("radius"))
                    radius = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("startdepth"))
                    startdepth = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("weight"))
                    weight = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("width"))
                    width = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("immovable"))
                    immovable = bool.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("sideLengths"))
                    sideLengths = XMLUtil.fromXMLVector2(itemChild);
                else if (itemChild.Name.Equals("polygonType"))
                    polygonType = (PhysicsPolygonType)Enum.Parse(typeof(PhysicsPolygonType), itemChild.FirstChild.Value);
            try
            {
                guid = Guid.Parse(element.GetAttribute("guid"));
            }
            catch (Exception e)
            {
                Log.write(LogType.Debug, "Guid exception, trace=" + e.StackTrace);
                guid = Guid.NewGuid();
            }
        }
        public static GameItem fromXML(XmlElement itemnode)
        {
            GameItem gameItem = new GameItem();
            gameItem.copyValues(itemnode);
            return gameItem;
        }
        public virtual XmlElement asXML(XmlNode parentNode)
        {
            XmlElement element = parentNode.OwnerDocument.CreateElement("gameItem"),
                locationNode = parentNode.OwnerDocument.CreateElement("loc"),
                nameNode = parentNode.OwnerDocument.CreateElement("name"),
                radiusNode = parentNode.OwnerDocument.CreateElement("radius"),
                startDepthNode = parentNode.OwnerDocument.CreateElement("startdepth"),
                weightNode = parentNode.OwnerDocument.CreateElement("weight"),
                widthNode = parentNode.OwnerDocument.CreateElement("width"),
                immovableNode = parentNode.OwnerDocument.CreateElement("immovable"),
                polygonTypeNode = parentNode.OwnerDocument.CreateElement("polygonType");
            element.SetAttribute("guid", guid.ToString());
            nameNode.InnerText = name;
            radiusNode.InnerText = radius.ToString();
            startDepthNode.InnerText = startdepth.ToString();
            weightNode.InnerText = weight.ToString();
            widthNode.InnerText = width.ToString();
            immovableNode.InnerText = immovable.ToString();
            polygonTypeNode.InnerText = polygonType.ToString();
            locationNode.InnerText = loc.X + "," + loc.Y;
            switch(polygonType)
            {
                case PhysicsPolygonType.Rectangle:
                    element.AppendChild(XMLUtil.asXMLVector2(element, sideLengths, "sideLengths"));
                break;
                case PhysicsPolygonType.Circle:
                    element.AppendChild(radiusNode);
                break;
            }
            element.AppendChild(locationNode);
            element.AppendChild(nameNode);
            element.AppendChild(startDepthNode);
            element.AppendChild(weightNode);
            element.AppendChild(widthNode);
            element.AppendChild(immovableNode);
            element.Attributes.GetNamedItem("rotation").Value = rotation.ToString();
            return element;
        }
    }
}
