using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using FarseerGames.FarseerPhysics;
using FarseerGames.FarseerPhysics.Collisions;
using FarseerGames.FarseerPhysics.Dynamics;
using FarseerGames.FarseerPhysics.Factories;
using System.Xml;
using BountyBandits.Map;

namespace BountyBandits
{
    public enum PhysicsPolygonType
    {
        Rectangle, Circle, Polygon
    }

    public class GameItem : IMovableItem
    {
        public string name;
        public uint weight = 1, radius = 10, startdepth = 0, width = 1;
        public Guid guid;
        public Vector2 loc, sideLengths = Vector2.One;
        public Body body;
        public Geom geom;
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

        public void setPosition(Vector2 position)
        {
            this.loc = position - getScreenOffset();
            if (body != null)
            {
                body.Position = this.loc;
                body.LinearVelocity = Vector2.Zero;
            }
        }

        public Vector2 getPositionFromScreenCoords()
        {
            return body != null ? body.Position : loc + getScreenOffset();
        }

        public Vector2 getScreenOffset()
        {
            return new Vector2(-startdepth * Game.DEPTH_X_OFFSET, (3 - startdepth) * Game.DEPTH_MULTIPLE);
        }

        public bool contains(Vector2 position)
        {
            if (geom != null)
                return geom.AABB.Contains(position - getScreenOffset());
            return (position.X < loc.X + sideLengths.X / 2 && position.X > loc.X - sideLengths.X / 2 &&
                position.Y < loc.Y + sideLengths.Y / 2 && position.Y > loc.Y - sideLengths.Y / 2);
        }

        public static GameItem fromXML(XmlElement itemnode)
        {
            GameItem gameItem = new GameItem();
            gameItem.copyValues(itemnode);
            return gameItem;
        }

        public virtual XmlElement asXML(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement("gameItem");
            switch(polygonType)
            {
                case PhysicsPolygonType.Rectangle:
                    element.AppendChild(XMLUtil.asXMLVector2(doc, sideLengths, "sideLengths"));
                break;
                case PhysicsPolygonType.Circle:
                    XMLUtil.addElementValue(doc, element, "radius", radius.ToString());
                break;
            }
            XMLUtil.addElementValue(doc, element, "name", name);
            XMLUtil.addElementValue(doc, element, "location", loc.X + "," + loc.Y);
            XMLUtil.addElementValue(doc, element, "polygonType", polygonType.ToString());
            XMLUtil.addElementValue(doc, element, "width", width.ToString());
            XMLUtil.addElementValue(doc, element, "weight", weight.ToString());
            XMLUtil.addElementValue(doc, element, "startdepth", startdepth.ToString());
            XMLUtil.addElementValue(doc, element, "immovable", immovable.ToString());
            element.SetAttribute("guid", guid.ToString());
            element.SetAttribute("rotation", rotation.ToString());
            return element;
        }
    }
}
