using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using Sanford.Multimedia.Midi;
using MusicAnalyzer.Models;

namespace MusicAnalyzer.Tools
{
    class IOTools
    {
        public static IEnumerable<string> ReadFrom(string file)
        {
            string line;
            if (System.IO.File.Exists(file))
            {
                using (var reader = File.OpenText(file))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }

        public static void saveTo(List<string> lines, string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (string line in lines)
                {
                    sw.WriteLine(line);
                }
            }
        }
    }
}
