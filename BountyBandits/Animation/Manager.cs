﻿using System;
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
        private Game gameref;

        public Manager(ContentManager content, Game gameref)
        {
            this.content = content;
            this.gameref = gameref;
        }
        public AnimationController getController(string controllerName)
        {
            if (!controllerMap.ContainsKey(controllerName))
                buildController(controllerName);
            int index = gameref.rand.Next(controllerMap[controllerName + "_p00"].PermutationCount);
            String modifier = "_p" + (index < 9 ? "0" : "") + index;
            return controllerMap[controllerName + modifier];
        }

        private void buildController(string controllerName)
        {
            if (controllerMap.ContainsKey(controllerName + "_p00"))
                return;
            string[] dirEntries = Directory.GetDirectories(@"Content\Beings");
            foreach (string dirPath in dirEntries)
            {
                String dirName = dirPath.Substring(dirPath.LastIndexOf('\\')+1);
                if (dirName.Equals(controllerName))
                {
                    AnimationController baseController = new AnimationController();
                    baseController.fromXML(content, dirPath.Split('\\')[dirPath.Split('\\').Length - 1]);
                    controllerMap.Add(baseController.name + "_p00", baseController);

                    int count = 0;
                    for (int permutations = 0; permutations < baseController.permutations.Count; permutations++)
                    {
                        for (int dest = 0; dest < baseController.permutations[permutations].Length; dest++)
                        {
                            count++;
                            AnimationController controller = new AnimationController();
                            controller.fromXML(content, dirPath.Split('\\')[dirPath.Split('\\').Length - 1]);
                            for (int textureIndex = 0; textureIndex < controller.frames.Count; textureIndex++)
                            {
                                Texture2D tex = controller.frames[textureIndex];
                                //AnimationController.replaceColor(ref tex, 
                                //    controller.permutations[permutations][0], 
                                //    controller.permutations[permutations][dest]);
                            }
                            String name = controller.name + "_p" + (count > 9 ? "" : "0") + count;
                            controllerMap.Add(name, controller);
                        }
                    }
                }
            }
        }
    }
}
