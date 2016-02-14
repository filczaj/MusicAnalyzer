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

        public Note() {
        }

        public Note(double freq, int startTime, int endTime, String note, String basicNote)
        {
            this.freq = freq;
            this.startTime = startTime;
            this.endTime = endTime;
            this.duration = endTime - startTime;
            this.note = note;
            this.basicNote = basicNote;
        }

        public Note(int note_id, int starter)
        {
            this.noteID = note_id;
            this.startTime = starter;
            this.duration = -1;
            this.endTime = -1;
        }

        public override string ToString()
        {
            return note + " " + startTime.ToString() + " " + duration.ToString();
        }
    }
}
