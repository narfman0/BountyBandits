using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.Animation
{
    public class Manager
    {
        private Dictionary<String, AnimationController> controllerMap = 
            new Dictionary<string, AnimationController>();
        private ContentManager content;

        public Manager(ContentManager content)
        {
            this.content = content;
        }
        public AnimationController getController(string controllerName)
        {
            if (!controllerMap.ContainsKey(controllerName))
                buildController(controllerName);
            return controllerMap[controllerName];
        }

        private void buildController(string controllerName)
        {
            string[] dirEntries = Directory.GetDirectories(@"Content\Beings");
            foreach (string dirPath in dirEntries)
            {
                String dirName = dirPath.Substring(dirPath.LastIndexOf('\\')+1);
                if (dirName.Equals(controllerName))
                {
                    AnimationController controller = new AnimationController();
                    controller.fromXML(content, dirPath.Split('\\')[dirPath.Split('\\').Length - 1]);
                    controllerMap.Add(controller.name, controller);
                }
            }
        }
    }
}
