﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;

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

        public static void asXMLColor(XmlNode parentNode, Color color, String name)
        {
            XmlElement colorElement = parentNode.OwnerDocument.CreateElement(name),
                rElement = parentNode.OwnerDocument.CreateElement("r"),
                gElement = parentNode.OwnerDocument.CreateElement("g"),
                bElement = parentNode.OwnerDocument.CreateElement("b");
            rElement.Value = color.R.ToString();
            gElement.Value = color.G.ToString();
            bElement.Value = color.B.ToString();
            colorElement.AppendChild(rElement);
            colorElement.AppendChild(gElement);
            colorElement.AppendChild(bElement);
            parentNode.AppendChild(colorElement);
        }
    }
}