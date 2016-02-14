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
        public bool isScaleBasic;
        public int turnover;
        public ChordMode mode;
        public int priority;

        public Chord(ChordMode mode, bool isBasic, int turn)
        {
            this.mode = mode;
            isScaleBasic = isBasic;
            turnover = turn;
            this.chordNotes = new List<int>();
            if (isBasic)
                this.priority = 2;
            else
                this.priority = 1;
        }

        public Chord(Note n, Tonation tonation) // creates a specified chord based on a given note
        {
            if (tonation == null)
                return;
            int noteIndex = tonation.getNoteIndexInScale(n);
            if (noteIndex == 0 || noteIndex == 4 || noteIndex == 5)
                this.isScaleBasic = true;
            else
                this.isScaleBasic = false;
            this.chordNotes = tonation.getChordNotesOnIndex(noteIndex);
            this.turnover = 0;
            this.mode = getChordMode();
        }

        public Chord(Note n)
        {
            this.chordNotes = new List<int>();
            chordNotes.Add(n.noteID % 12);
            isScaleBasic = false;
            mode = getChordMode();
        }

        public ChordMode getChordMode()
        {
            if (chordNotes.Count != 3)
                return ChordMode.Other;
            if (chordNotes[1] - chordNotes[0] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow)
                return ChordMode.Major;
            if (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.Terce)
                return ChordMode.Minor;
            if (chordNotes[1] - chordNotes[0] == (int)Interval.TerceLow && chordNotes[2] - chordNotes[1] == (int)Interval.TerceLow)
                return ChordMode.Diminished;
            if (chordNotes[1] - chordNotes[0] == (int)Interval.Terce && chordNotes[2] - chordNotes[1] == (int)Interval.Terce)
                return ChordMode.Enlarged;
            return ChordMode.Other;
        }

        public void addNote(Note n)
        {
            int noteIndex = n.noteID % 12; ;
            this.chordNotes.Add(noteIndex);
            this.mode = getChordMode();
        }

        public override string ToString()
        {
            string ret = "";
            foreach (int i in chordNotes)
            {
                ret += i.ToString() + ", ";
            }
            ret += "Mode: " + mode.ToString();
            return ret;
        }
    }
}
