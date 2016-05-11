using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class Chord
    {
        public List<int> chordNotes;
        public int turnover;
        public ChordMode mode;

        public Chord()
        {

        }
        public Chord(ChordMode mode, int turn)
        {
            this.mode = mode;
            turnover = turn;
            this.chordNotes = new List<int>();
        }

        public Chord(Note n)
        {
            this.chordNotes = new List<int>();
            chordNotes.Add((n.noteID - 3) % 12);
            setChordMode();
        }

        public Chord(List<int> notesList)
        {
            this.chordNotes = notesList;
            setChordMode();
        }

        public void setChordMode()
        {
            if (chordNotes.Count < 3)
                mode = ChordMode.Other;
            else if ((chordNotes.Count > 3) &&
                ((chordNotes[1] - chordNotes[0] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow && chordNotes[3] - chordNotes[2] == (int)Interval.TerceLow) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow && chordNotes[3] - chordNotes[2] == (int)Interval.Second) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.Second && chordNotes[2] - chordNotes[1] == (int)Interval.Terce) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.Second && chordNotes[2] - chordNotes[1] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow)))
                mode = ChordMode.Dominant;
            else if ((chordNotes.Count > 3) &&
                (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow && chordNotes[3] - chordNotes[2] == (int)Interval.TerceLow))
                mode = ChordMode.SeventhDim;
            else if ((chordNotes[1] - chordNotes[0] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.Quart) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.Quart && chordNotes[2] - chordNotes[1] == (int)Interval.Terce))
                mode = ChordMode.Major;
            else if ((chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.Terce) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.Quart) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.Quart && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow))
                mode = ChordMode.Minor;
            else if ((chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.Triton) ||
                (chordNotes[1] - chordNotes[0] == (int)Interval.Triton && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow))
                mode = ChordMode.Diminished;
            else if (chordNotes[1] - chordNotes[0] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.Terce)
                mode = ChordMode.Enlarged;
            else mode = ChordMode.Other;
        }

        public void addNote(Note n)
        {
            int noteIndex = (n.noteID - 3) % 12;
            if (!chordNotes.Contains(noteIndex))
            {
                this.chordNotes.Add(noteIndex);
                chordNotes.Sort();
                setChordMode();
            }
        }

        public override string ToString()
        {
            string ret = "Chord notes count = " + this.chordNotes.Count.ToString() + "; " ;
            foreach (int i in chordNotes)
            {
                ret += i.ToString() + ", ";
            }
            ret += "Mode: " + mode.ToString();
            return ret;
        }
    }
}
