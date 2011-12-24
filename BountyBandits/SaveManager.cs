using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using BountyBandits.Character;

namespace BountyBandits
{
    public class SaveManager
    {
        public static String BB_USER_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\My Games\Bounty Bandits\";
        private static String SAVE_PATH = BB_USER_PATH + @"Saves\";
        private static int NUM_BACKUPS = 10;

        public static string[] getAvailableCharacterNames()
        {
            Directory.CreateDirectory(SAVE_PATH);
            string[] files = Directory.GetFiles(SAVE_PATH, "*.xml");
            for (int i = 0; i < files.Length; i++)
                files[i] = files[i].Substring(files[i].LastIndexOf(@"\")+1);
            return files;
        }
        public static void saveCharacter(Being being)
        {
            try
            {
                Directory.CreateDirectory(SAVE_PATH);
                string filename = SAVE_PATH + being.name + ".xml";
                backupSaves(filename, NUM_BACKUPS);
                XmlDocument xmlDoc = new XmlDocument();
                try
                {
                    xmlDoc.Load(filename);
                }
                catch (System.IO.FileNotFoundException)
                {
                    //if file is not found, create a new xml file
                    XmlTextWriter xmlWriter = new XmlTextWriter(filename, System.Text.Encoding.UTF8);
                    xmlWriter.Formatting = Formatting.Indented;
                    xmlWriter.WriteProcessingInstruction("xml", "version='1.0' encoding='UTF-8'");
                    xmlWriter.WriteStartElement("root");
                    //If WriteProcessingInstruction is used as above,
                    //Do not use WriteEndElement() here
                    //xmlWriter.WriteEndElement();
                    //it will cause the <Root> to be &ltRoot />
                    xmlWriter.Close();
                    xmlDoc.Load(filename);
                }
                XmlNode root = xmlDoc.DocumentElement;
                root.AppendChild(being.asXML(root));
                xmlDoc.Save(filename);
                Log.write(LogType.Saves, "saved character: " + being.name);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.StackTrace);
            }
        }
        public static void backupSaves(String path, int iteration)
        {
            if(iteration == 0)
                return;
            String newPath = path.Substring(0, path.Length - 2) + (iteration > 9 ? "" : "0") + iteration;
            try
            {
                File.Move(path, newPath);
            }
            catch (Exception e) { System.Console.WriteLine(e.StackTrace); }
            backupSaves(newPath, iteration-1);
        }
        public static Being loadCharacter(string beingName, Game gameref)
        {
            string filename = SAVE_PATH + beingName;
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filename);
            return Being.fromXML((XmlElement)xmlDoc.GetElementsByTagName("being").Item(0), gameref);
        }
    }
}
