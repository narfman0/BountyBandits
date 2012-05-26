using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace BountyBandits
{
    public enum LogType { NetworkServer, NetworkClient, Saves, Debug }
    public static class Log
    {
        public static void write(LogType type, String log)
        {
            try
            {
                Directory.CreateDirectory(SaveManager.BB_USER_PATH);
                File.AppendAllLines(SaveManager.BB_USER_PATH + "/" + type + ".txt", new String[] { DateTime.Now.ToString("HH:mm:ss tt") + "   " + log });
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }
    }
}
