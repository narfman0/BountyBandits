using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace BountyBandits.Map
{
    public class BackgroundItemStruct : IMovableItem
    {
        public string texturePath;
        public Vector2 location;
        public float rotation, scale, layer;

        public void setPosition(Vector2 positon)
        {
            this.location = positon;
        }

        public static BackgroundItemStruct fromXML(XmlElement element)
        {
            BackgroundItemStruct str = new BackgroundItemStruct();
            str.location = XMLUtil.fromXMLVector2(element.GetElementsByTagName("location")[0]);
            str.texturePath = element.GetElementsByTagName("path")[0].FirstChild.Value;
            str.rotation = element.HasAttribute("rotation") ? float.Parse(element.GetAttribute("rotation")) : 0f;
            str.scale = element.HasAttribute("scale") ? float.Parse(element.GetAttribute("scale")) : 1f;
            str.layer = element.HasAttribute("layer") ? float.Parse(element.GetAttribute("layer")) : 1f;
            return str;
        }

        public XmlNode asXML(XmlDocument doc)
        {
            XmlElement backgroundElement = doc.CreateElement("graphic");
            backgroundElement.SetAttribute("rotation", rotation.ToString());
            backgroundElement.SetAttribute("scale", scale.ToString());
            backgroundElement.SetAttribute("layer", layer.ToString());
            XMLUtil.addElementValue(doc, backgroundElement, "path", texturePath);
            backgroundElement.AppendChild(XMLUtil.asXMLVector2(doc, location, "location"));
            return backgroundElement;
        }
    }
}
