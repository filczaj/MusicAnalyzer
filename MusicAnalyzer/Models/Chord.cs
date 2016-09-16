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

        public Chord(List<int> notesList, ChordMode modee)
        {
            this.chordNotes = new List<int>();
            foreach (int n in notesList)
                this.chordNotes.Add(n);
            this.mode = modee;
        }

        public void setChordMode()
        {
            if (chordNotes.Count < 3)
                mode = ChordMode.Other;
            else if ((chordNotes.Count > 3) &&
                (((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Terce && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[3] - chordNotes[2] + 12) % 12 == (int)Interval.TerceLow) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[3] - chordNotes[2] + 12) % 12 == (int)Interval.Second) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Second && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Terce) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Second && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Terce && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow)))
                mode = ChordMode.Dominant;
            else if ((chordNotes.Count > 3) &&
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[3] - chordNotes[2] + 12) % 12 == (int)Interval.TerceLow))
                mode = ChordMode.SeventhDim;
            else if (chordNotes.Count > 3)
                mode = ChordMode.Other;
            else if (((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Terce && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Quart) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Quart && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Terce))
                mode = ChordMode.Major;
            else if (((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Terce) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Terce && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Quart) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Quart && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow))
                mode = ChordMode.Minor;
            else if (((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.TerceLow && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Triton) ||
                ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Triton && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.TerceLow))
                mode = ChordMode.Diminished;
            else if ((chordNotes[1] - chordNotes[0] + 12) % 12 == (int)Interval.Terce && (chordNotes[2] - chordNotes[1] + 12) % 12 == (int)Interval.Terce)
                mode = ChordMode.Enlarged;
            else mode = ChordMode.Other;
        }

        public void addNote(Note n)
        {
            int noteIndex = (n.noteID - 3) % 12;
            addNoteIndex(noteIndex);
        }

        public void addNoteIndex(int noteID){
            if (!chordNotes.Contains(noteID))
            {
                this.chordNotes.Add(noteID);
                chordNotes.Sort();
                setChordMode();
            }
        }

        public List<int> getNotesInTurnover(int turnover)
        {
            List<int> chordNotesInTun = new List<int>();
            for (int i = 0; i < chordNotes.Count; i++)
                chordNotesInTun.Add(chordNotes[(i + turnover) % chordNotes.Count]);
            return chordNotesInTun;
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
