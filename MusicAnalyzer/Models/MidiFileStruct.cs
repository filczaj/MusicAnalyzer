using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class MidiFileStruct
    {
        private string _fileName;
        public string fileName 
        {
            get { return this._fileName; } 
            set { this._fileName = value; } 
        }

        private List<Track> tracks = new List<Track>();

        private MidiFileProperties properties = new MidiFileProperties();

        public MidiFileStruct(List<Track> tracks_, MidiFileProperties props, string file_)
        {
            this.tracks = tracks_;
            this.properties = props;
            this.fileName = file_;
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
