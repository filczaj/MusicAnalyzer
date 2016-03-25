using MusicAnalyzer.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class TonationChord : Chord
    {
        public ChordType scaleChordType;
        public bool isScaleBasic;
        public Tonation tonation;
        public int priority;
        public Offset offset;

        public TonationChord(ChordMode mode, bool isBasic, int turn) : base(mode, turn)
        {
            this.isScaleBasic = isBasic;
            if (isBasic)
                this.priority = 2;
            else
                this.priority = 1;
        }

        public TonationChord(Note n, Tonation t) : base(n) // to do!!!!
        {
            this.offset = t.offset;
            this.scaleChordType = ChordType.Other;
            this.tonation = t;
        }

        public TonationChord(Note n, Tonation tonation, MusicIntelligence musicAI) : base(n) // creates a specified chord based on a given note
        {
            if (tonation == null)
                return;
            else
            {
                this.tonation = tonation;
                this.offset = tonation.offset;
            }
            int noteIndex = tonation.getNoteIndexInScale(n);
            this.chordNotes = tonation.getChordNotesOnIndex(noteIndex);
            this.turnover = 0; // setTurnover() ???????
            setChordMode();
            setChordType(musicAI);
        }

        public void setChordType(MusicIntelligence musicAI)
        {
            if ((int)musicAI.matchChords(this, musicAI.turnIntoPureChord(tonation.tonic)) > (int)Match.Medium)
            {
                this.isScaleBasic = true;
                this.scaleChordType = ChordType.Tonic;
            }
            else if ((int)musicAI.matchChords(this, musicAI.turnIntoPureChord(tonation.subdominant)) > (int)Match.Medium)
            {
                this.isScaleBasic = true;
                this.scaleChordType = ChordType.Subdominant;
            }
            else if ((int)musicAI.matchChords(this, musicAI.turnIntoPureChord(tonation.dominant)) > (int)Match.Medium)
            {
                this.isScaleBasic = true;
                this.scaleChordType = ChordType.Dominant;
            }
            else
            {
                this.isScaleBasic = false;
                this.scaleChordType = ChordType.Other;
            }
        }

        public override string ToString()
        {
            string ret = "Chord notes count = " + this.chordNotes.Count.ToString() + "; " ;
            foreach (int i in chordNotes)
            {
                ret += i.ToString() + ", ";
            }
            ret += "T.Offset: " + offset.ToString();
            ret += " Mode: " + mode.ToString();
            ret += " Scale chord: " + scaleChordType.ToString();
            return ret;
        }
    }
}
