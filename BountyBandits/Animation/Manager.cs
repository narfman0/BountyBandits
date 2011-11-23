using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BountyBandits.Animation
{
    public class AnimationManager
    {
        private Dictionary<String, AnimationController> controllerMap = 
            new Dictionary<string, AnimationController>();
        private Game gameref;

        public AnimationManager(Game gameref)
        {
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
                    baseController.fromXML(gameref.Content, dirPath.Split('\\')[dirPath.Split('\\').Length - 1]);
                    controllerMap.Add(baseController.name + "_p00", baseController);

                    int count = 0;
                    for (int permutations = 0; permutations < baseController.permutations.Count; permutations++)
                    {
                        for (int dest = 0; dest < baseController.permutations[permutations].Length; dest++)
                        {
                            count++;
                            AnimationController controller = new AnimationController();
                            controller.fromXML(gameref.Content, dirPath.Split('\\')[dirPath.Split('\\').Length - 1]);
                            for (int textureIndex = 0; textureIndex < controller.frames.Count; textureIndex++)
                            {
                                Texture2D tex = controller.frames[textureIndex];
                                Color[] color = new Color[tex.Height * tex.Width];
                                tex.GetData<Color>(color);
                                tex = new Texture2D(gameref.GraphicsDevice, tex.Width, tex.Height);
                                tex.SetData<Color>(color);
                                AnimationController.replaceColor(ref tex, 
                                    controller.permutations[permutations][0], 
                                    controller.permutations[permutations][dest]);
                                controller.frames[textureIndex] = tex;
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
