using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BountyBandits.Animation;
using System.Xml;

namespace BountyBandits.Story
{
    public class BeingController
    {
        /// <summary>
        /// Time when being appears in cutscene
        /// </summary>
        public int entranceMS;

        public AnimationController animationController;

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
            foreach (XmlElement animationElement in element.GetElementsByTagName("timeAnimationStruct"))
                controller.animations.Add(TimeAnimationStruct.fromXML(animationElement));
            foreach(XmlElement actionElement in element.GetElementsByTagName("action"))
                controller.actions.Add(ActionStruct.fromXML(actionElement));
            return controller;
        }
    }

    public enum ActionEnum
    {
        WalkRight, WalkLeft, Jump, Stop
    }

    public struct ActionStruct
    {
        ActionEnum action;
        /// <summary>
        /// Time to start action (ms)
        /// </summary>
        int time;
        /// <summary>
        /// Intensity at which to do action... ie if walking, the force 
        /// (more force walks faster)
        /// </summary>
        float intensity;

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
        int time;
        /// <summary>
        /// Animation to transition to
        /// </summary>
        String animationName;

        public static TimeAnimationStruct fromXML(XmlElement animationElement)
        {
            TimeAnimationStruct str = new TimeAnimationStruct();
            str.animationName = animationElement.Value;
            str.time = int.Parse(animationElement.GetAttribute("time"));
            return str;
        }
    }
}
