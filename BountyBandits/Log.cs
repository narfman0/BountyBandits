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
            Directory.CreateDirectory(SaveManager.SAVE_PATH);
            File.AppendAllLines(SaveManager.SAVE_PATH + "/" + type + ".txt", new String[] {DateTime.Now.ToString("HH:mm:ss tt") + "   " + log });
        }
    }
}
