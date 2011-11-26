using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.Animation;
using System.Xml;
using Microsoft.Xna.Framework;

namespace BountyBandits.Story
{
    public class BeingController
    {
        /// <summary>
        /// Time when being appears in cutscene
        /// </summary>
        public int entranceMS;

        public AnimationController animationController;

        public Vector2 startLocation;

        /// <summary>
        /// Contains all times (key) when animation changes. Animation name is value
        /// </summary>
        public List<TimeAnimationStruct> animations = new List<TimeAnimationStruct>();

        public List<ActionStruct> actions = new List<ActionStruct>();

        public static BeingController fromXML(XmlElement element, Game gameref)
        {
            BeingController controller = new BeingController();
            controller.entranceMS = int.Parse(element.GetAttribute("entranceMS"));
            controller.animationController = gameref.animationManager.getController(element.GetAttribute("animationName"));
            foreach (XmlElement locElement in element.GetElementsByTagName("startLocation"))
                controller.startLocation = XMLUtil.fromXMLVector2(locElement);
            foreach (XmlElement animationElement in element.GetElementsByTagName("timeAnimationStruct"))
                controller.animations.Add(TimeAnimationStruct.fromXML(animationElement));
            foreach(XmlElement actionElement in element.GetElementsByTagName("action"))
                controller.actions.Add(ActionStruct.fromXML(actionElement));
            return controller;
        }

        public String getCurrentAnimation(GameTime gameTime)
        {
            for (int i = animations.Count-1; i >= 0; i--)
                if (gameTime.TotalGameTime.TotalMilliseconds - entranceMS > animations[i].time)
                    return animations[i].animationName;
            return "idle";
        }
    }

    public enum ActionEnum
    {
        WalkRight, WalkLeft, Jump, Stop
    }

    public struct ActionStruct
    {
        public ActionEnum action;
        /// <summary>
        /// Time to start action (ms)
        /// </summary>
        public int time;
        /// <summary>
        /// Intensity at which to do action... ie if walking, the force 
        /// (more force walks faster)
        /// </summary>
        public float intensity;

        public static ActionStruct fromXML(XmlElement actionElement)
        {
            ActionStruct str = new ActionStruct();
            foreach (XmlElement ele in actionElement.ChildNodes)
                if (ele.Name == "actionEnum")
                    str.action = (ActionEnum)Enum.Parse(typeof(ActionEnum), ele.FirstChild.Value);
                else if (ele.Name == "time")
                    str.time = int.Parse(ele.FirstChild.Value);
                else if (ele.Name == "intensity")
                    str.intensity = float.Parse(ele.FirstChild.Value);
            return str;
        }
    }

    public struct TimeAnimationStruct
    {
        /// <summary>
        /// Time (ms) when animation should change
        /// </summary>
        public int time;
        /// <summary>
        /// Animation to transition to
        /// </summary>
        public String animationName;

        public static TimeAnimationStruct fromXML(XmlElement animationElement)
        {
            TimeAnimationStruct str = new TimeAnimationStruct();
            str.animationName = animationElement.FirstChild.Value;
            str.time = int.Parse(animationElement.GetAttribute("time"));
            return str;
        }
    }
}
