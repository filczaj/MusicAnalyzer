using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace MusicAnalyzer.Analyzer
{
    class AudioPlayer
    {
        private SoundPlayer player;

        public AudioPlayer(String filename)
        {
            player = new SoundPlayer(filename);
        }
        public void play()
        {
            player.Play();
        }

        public void stop()
        {
            player.Stop();
        }
    }
}
