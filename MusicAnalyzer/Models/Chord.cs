using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    class Chord
    {
        public List<int> chordNotes;
        bool isScaleBasic;
        int turnover;
        bool majorMinor;

        public Chord(bool majorMinor)
        {
            this.majorMinor = majorMinor;
            this.chordNotes = new List<int>();
        }

        public Chord(bool majorMinor, bool isBasic, int turn)
        {
            this.majorMinor = majorMinor;
            isScaleBasic = isBasic;
            turnover = turn;
            this.chordNotes = new List<int>();
        }
    }
}
