using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Models
{
    public class Note
    {
        public String basicNote { get; set; }
        public String note { get; set; }
        public int noteID { get; set; }
        public double freq { get; set; }
        public int duration { get; set; }
        public int startTime { get; set; }
        public int endTime { get; set; }
        public bool strongMetric { get; set; }
        public int trackID { get; set; }
        public int octave { get; set; }

        public PSAMControlLibrary.MusicalSymbolDuration rythmicValue { get; set; }

        public PSAMControlLibrary.NoteStemDirection noteStemDirection;

        public double noteExtension { get; set; }

        public Note(int note_id, int starter, int channel)
        {
            this.noteID = note_id;
            this.startTime = starter;
            this.duration = -1;
            this.endTime = -1;
            this.trackID = channel;
            noteExtension = 1.0;
        }

        public Note(int note_id, int starter, int duration, int trackID, bool metric)
        {
            this.noteID = note_id;
            this.startTime = starter;
            this.duration = duration;
            this.endTime = starter + duration;
            this.trackID = trackID;
            this.strongMetric = metric;
            this.octave = (note_id + 9) / 12;
            noteExtension = 1.0;
        }

        public override bool Equals(object obj)
        {
            Note other = null;
            try {
                other = (Note)obj;
            }
            catch(InvalidCastException exc){

            }
            if (other == null)
                return false;
            if (this.note == other.note && this.startTime == other.startTime && this.trackID == other.trackID)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return note + " " + startTime.ToString() + " Duration: " + duration.ToString() + " Extension: " + noteExtension.ToString() + " Track: " + trackID.ToString();
        }
    }
}
