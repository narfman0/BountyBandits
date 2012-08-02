using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace BountyBandits
{
    public static class XMLUtil
    {
        public static Vector2 fromXMLVector2(XmlNode node)
        {
            Vector2 vec = Vector2.Zero;
            foreach (XmlNode subnode in node.ChildNodes)
                if (subnode.Name.Equals("x"))
                    vec.X = float.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("y"))
                    vec.Y = float.Parse(subnode.FirstChild.Value);
            return vec;
        }

        public static XmlElement asXMLVector2(XmlDocument doc, Vector2 vector, String name)
        {
            XmlElement vectorElement = doc.CreateElement(name);
            vectorElement.InnerXml = "<x>" + vector.X.ToString() + "</x>" +
                "<y>" + vector.Y.ToString() + "</y>";
            return vectorElement;
        }

        public static Color fromXMLColor(XmlNode xmlNode)
        {
            byte r = 0, g = 0, b = 0;
            foreach (XmlNode subnode in xmlNode.ChildNodes)
                if (subnode.Name.Equals("r"))
                    r = byte.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("g"))
                    g = byte.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("b"))
                    b = byte.Parse(subnode.FirstChild.Value);
            return new Color(r,g,b);
        }

        public static XmlElement asXML(this string xml)
        {
            XmlDocumentFragment frag = new XmlDocument().CreateDocumentFragment();
            frag.InnerXml = xml;
            return frag.FirstChild as XmlElement;
        }

        public static XmlElement addElementValue(XmlDocument document, XmlNode parent,
            String name, String value)
        {
            XmlElement element = document.CreateElement(name);
            element.InnerText = value;
            parent.AppendChild(element);
            return element;
        }
    }
}
