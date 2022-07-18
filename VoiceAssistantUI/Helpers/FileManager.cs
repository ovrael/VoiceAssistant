using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VoiceAssistantUI
{
    public static class FileManager
    {
        public static void SaveToFile(List<string> lines, string file)
        {
            using (StreamWriter sw = File.CreateText(file))
            {
                foreach (var line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }

        public static List<string> LoadFromFile(string file)
        {
            List<string> lines = new List<string>();

            try
            {
                lines = File.ReadLines(file).ToList();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return lines;
        }
    }
}
