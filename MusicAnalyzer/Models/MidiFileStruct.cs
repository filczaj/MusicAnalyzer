using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    class MidiFileStruct
    {
        private List<Track> tracks = new List<Track>();

        private MidiFileProperties properties = new MidiFileProperties();

        public MidiFileStruct(List<Track> tracks_, MidiFileProperties props)
        {
            this.tracks = tracks_;
            this.properties = props;
        }

        public List<Track> getTracks(){
            return this.tracks;
        }

        public MidiFileProperties getProperties()
        {
            return this.properties;
        }
    }
}
