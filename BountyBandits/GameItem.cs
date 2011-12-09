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
        public static GameItem fromXML(XmlElement itemnode)
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
            try
            {
                gameItem.guid = Guid.Parse(itemnode.GetAttribute("guid"));
            }
            catch (Exception e)
            {
                Log.write(LogType.Debug, "Guid exception, trace=" + e.StackTrace);
                gameItem.guid = Guid.NewGuid();
            }
            return gameItem;
        }
        public virtual XmlElement asXML(XmlNode parentNode)
        {
            XmlElement element = parentNode.OwnerDocument.CreateElement("gameItem"),
                nameNode = parentNode.OwnerDocument.CreateElement("name"),
                radiusNode = parentNode.OwnerDocument.CreateElement("radius"),
                startDepthNode = parentNode.OwnerDocument.CreateElement("startDepth"),
                weightNode = parentNode.OwnerDocument.CreateElement("weight"),
                widthNode = parentNode.OwnerDocument.CreateElement("width"),
                immovableNode = parentNode.OwnerDocument.CreateElement("immovable"),
                polygonTypeNode = parentNode.OwnerDocument.CreateElement("polygonType"),
                verticesNode = parentNode.OwnerDocument.CreateElement("vertices");
            XMLUtil.asXMLVector2(element, loc, "loc");
            XMLUtil.asXMLVector2(element, sideLengths, "sideLengths");
            element.SetAttribute("guid", guid.ToString());
            nameNode.Value = name;
            radiusNode.Value = radius.ToString();
            startDepthNode.Value = startDepthNode.ToString();
            weightNode.Value = weight.ToString();
            widthNode.Value = width.ToString();
            immovableNode.Value = immovable.ToString();
            foreach(Vector2 vertex in vertices)
                XMLUtil.asXMLVector2(verticesNode, vertex, "vertex");
            element.AppendChild(nameNode);
            element.AppendChild(radiusNode);
            element.AppendChild(startDepthNode);
            element.AppendChild(weightNode);
            element.AppendChild(widthNode);
            element.AppendChild(immovableNode);
            element.AppendChild(verticesNode);
            return element;
        }
    }
}
