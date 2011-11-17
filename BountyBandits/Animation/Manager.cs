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
        public List<Controller> controllers = new List<Controller>();

        public Manager(ContentManager content)
        {
            string[] dirEntries = Directory.GetDirectories(@"Content\Beings");
            foreach (string dirName in dirEntries)
            {
                Controller controller = new Controller();
                controller.fromXML(content, dirName.Split('\\')[dirName.Split('\\').Length-1]);
                controllers.Add(controller);
            }
        }
        public Controller getController(string controllerName)
        {
            foreach (Controller contr in controllers)
                if (contr.name.Equals(controllerName))
                    return contr;
            return null;
        }
    }
}
