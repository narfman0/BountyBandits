using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Microsoft.Xna.Framework;

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
        public float startX;

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

        /// <summary>
        /// Holds being cutscene parts (who where when)
        /// </summary>
        public List<BeingController> beingControllers;

        /// <summary>
        /// For use after loaded - in the middle of level, defines whether or not this has been executed
        /// </summary>
        public bool executed = false;

        public static StoryElement fromXML(XmlNode node, Game gameref)
        {
            StoryElement element = new StoryElement();
            element.pathSegments = new List<CameraPathSegment>();
            element.audioElements = new List<AudioElement>();
            element.beingControllers = new List<BeingController>();
            foreach (XmlNode subnode in node.ChildNodes)
                if (subnode.Name.Equals("aveXTrigger"))
                    element.aveXTrigger = int.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("startX"))
                    element.startX = float.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("cutsceneLength"))
                    element.cutsceneLength = int.Parse(subnode.FirstChild.Value);
                else if (subnode.Name.Equals("pathSegments"))
                    foreach (XmlNode pathSegment in subnode.ChildNodes)
                        element.pathSegments.Add(CameraPathSegment.fromXML(pathSegment));
                else if (subnode.Name.Equals("audioElements"))
                    foreach (XmlNode audioSegment in subnode.ChildNodes)
                        element.audioElements.Add(AudioElement.fromXML(audioSegment));
                else if (subnode.Name.Equals("animations"))
                    foreach (XmlElement anim in subnode.ChildNodes)
                        element.beingControllers.Add(BeingController.fromXML(anim, gameref));
            return element;
        }

        public AudioElement popAudioElement(double time)
        {
            foreach (AudioElement audio in audioElements)
                if (!audio.executed && audio.startTime <= time)
                {
                    audio.executed = true;
                    return audio;
                }
            return null;
        }

        public void resetExecuted()
        {
            executed = false;
            foreach (AudioElement audioEle in audioElements)
                audioEle.executed = false;
        }

        public Vector2 getCameraOffset(GameTime gameTime)
        {
            foreach (CameraPathSegment segment in pathSegments)
                if (segment.msStart <= gameTime.TotalGameTime.TotalMilliseconds &&
                    (segment.msStart + segment.msSpan) >= gameTime.TotalGameTime.TotalMilliseconds)
                    return segment.getCurrent(gameTime);
            return Vector2.Zero;
        }

        public XmlElement asXML(XmlDocument doc)
        {
            XmlElement element = doc.CreateElement("story");
            XMLUtil.addElementValue(doc, element, "aveXTrigger", aveXTrigger.ToString());
            XMLUtil.addElementValue(doc, element, "startX", startX.ToString());
            XMLUtil.addElementValue(doc, element, "cutsceneLength", cutsceneLength.ToString());
            XmlElement pathSegmentsElement = doc.CreateElement("pathSegments");
            foreach(CameraPathSegment segment in pathSegments)
                pathSegmentsElement.AppendChild(segment.asXML(doc));
            element.AppendChild(pathSegmentsElement);
            XmlElement audioElement = doc.CreateElement("audioElements");
            foreach (AudioElement segment in audioElements)
                audioElement.AppendChild(segment.asXML(doc));
            element.AppendChild(audioElement);
            XmlElement beingControllersElement = doc.CreateElement("animations");
            foreach (BeingController segment in beingControllers)
                beingControllersElement.AppendChild(segment.asXML(doc));
            element.AppendChild(beingControllersElement);
            return element;
        }
    }
}
