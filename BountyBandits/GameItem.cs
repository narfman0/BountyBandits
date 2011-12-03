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
        public uint weight = 1;
        public uint radius = 10;
        public uint startdepth = 0;
        public uint width = 1;
        public Vector2 loc, sideLengths;
        public Body body;
        public Vertices vertices;
        public bool immovable = false;
        public PhysicsPolygonType polygonType;
        protected GameItem() 
        {
        }
        public GameItem(XmlNode itemnode) : this()
        {
            fromXML(itemnode);
        }
        void fromXML(XmlNode itemnode)
        {
            foreach (XmlElement itemChild in itemnode)
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
                else if (itemChild.Name.Equals("vertices")){
                    vertices = new Vertices();
                    foreach (XmlElement vertexElement in itemChild.GetElementsByTagName("vertex"))
                        vertices.Add(XMLUtil.fromXMLVector2(vertexElement));
                }
        }
    }
}
