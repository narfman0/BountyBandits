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
        public Vector2 loc, sideLengths;
        public Body body;
        public Vertices vertices;
        public bool immovable = false;
        public PhysicsPolygonType polygonType;
        public GameItem() 
        {
            guid = Guid.NewGuid();
        }
        public static GameItem fromXML(XmlNode itemnode)
        {
            GameItem gameItem = new GameItem();
            foreach (XmlElement itemChild in itemnode)
                if (itemChild.Name.Equals("name"))
                    gameItem.name = itemChild.FirstChild.Value;
                else if (itemChild.Name.Equals("loc"))
                {
                    string[] locStr = itemChild.FirstChild.Value.Split(',');
                    gameItem.loc = new Vector2(float.Parse(locStr[0]), float.Parse(locStr[1]));
                }
                else if (itemChild.Name.Equals("radius"))
                    gameItem.radius = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("startdepth"))
                    gameItem.startdepth = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("weight"))
                    gameItem.weight = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("width"))
                    gameItem.width = uint.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("immovable"))
                    gameItem.immovable = bool.Parse(itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("sideLengths"))
                    gameItem.sideLengths = XMLUtil.fromXMLVector2(itemChild);
                else if (itemChild.Name.Equals("polygonType"))
                    gameItem.polygonType = (PhysicsPolygonType)Enum.Parse(typeof(PhysicsPolygonType), itemChild.FirstChild.Value);
                else if (itemChild.Name.Equals("vertices")){
                    gameItem.vertices = new Vertices();
                    foreach (XmlElement vertexElement in itemChild.GetElementsByTagName("vertex"))
                        gameItem.vertices.Add(XMLUtil.fromXMLVector2(vertexElement));
                }
            return gameItem;
        }
    }
}
