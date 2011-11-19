using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace BountyBandits.Story
{
    public class StoryElement
    {
        /// <summary>
        /// When players reach aveX >= this, begin quest/story dynamic
        /// </summary>
        public int aveXTrigger;

        /// <summary>
        /// Start camera at this x value before panning anywhere
        /// </summary>
        public int startX;

        /// <summary>
        /// Cutscene length in ms
        /// </summary>
        public int cutsceneLength;

        /// <summary>
        /// Holds all camera path segments for cutscene
        /// </summary>
        public List<CameraPathSegment> pathSegments;

        /// <summary>
        /// Holds all audio segments for cutscene
        /// </summary>
        public List<AudioElement> audioElements;

        public static StoryElement fromXML(XmlNode node)
        {
            StoryElement element = new StoryElement();
            element.pathSegments = new List<CameraPathSegment>();
            element.audioElements = new List<AudioElement>();
            foreach (XmlNode subnode in node.ChildNodes)
                if (subnode.Name.Equals("aveXTrigger"))
                    element.aveXTrigger = int.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("startX"))
                    element.startX = int.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("cutsceneLength"))
                    element.cutsceneLength = int.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("pathSegments"))
                    foreach (XmlNode pathSegment in subnode.ChildNodes)
                        element.pathSegments.Add(CameraPathSegment.fromXML(pathSegment));
                else if (subnode.Name.Equals("audioElements"))
                    foreach (XmlNode audioSegment in subnode.ChildNodes)
                        element.audioElements.Add(AudioElement.fromXML(audioSegment));
            return element;
        }
    }
}
