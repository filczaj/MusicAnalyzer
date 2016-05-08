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
        public ChordPriority priority;
        public Offset offset;
        public MeasureBeats beatStrength;

        public TonationChord() : base()
        {

        }
        public TonationChord(ChordMode mode, ChordPriority priority, int turn) : base(mode, turn)
        {
            if ((int)priority >= (int)ChordPriority.Dominant)
                this.isScaleBasic = true;
            else
                this.isScaleBasic = false;
            this.priority = priority;
        }

        public TonationChord(Note n, Tonation t) : base(n) // to do!!!!
        {
            this.offset = t.offset;
            this.scaleChordType = ChordType.Other;
            this.priority = ChordPriority.Default;
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
            setChordTypesAndPriority(musicAI);
        }

        public void setChordTypesAndPriority(MusicIntelligence musicAI)
        {
            if ((int)musicAI.matchChords(this, tonation.tonic) > (int)Match.Medium)
            {
                this.isScaleBasic = true;
                this.scaleChordType = ChordType.Tonic;
                this.priority = ChordPriority.Tonic;
            }
            else if ((int)musicAI.matchChords(this, tonation.subdominant) > (int)Match.Medium)
            {
                this.isScaleBasic = true;
                this.scaleChordType = ChordType.Subdominant;
                this.priority = ChordPriority.Subdominant;
            }
            else if ((int)musicAI.matchChords(this, tonation.dominant) > (int)Match.Medium)
            {
                this.isScaleBasic = true;
                this.scaleChordType = ChordType.Dominant;
                this.priority = ChordPriority.Dominant;
            }
            else if ((int)musicAI.matchChords(this, tonation.sixthStep) > (int)Match.Medium)
            {
                this.isScaleBasic = false;
                this.scaleChordType = ChordType.SixthStep;
                this.priority = ChordPriority.SixthStep;
            }
            else if ((int)musicAI.matchChords(this, tonation.secondStep) > (int)Match.Medium)
            {
                this.isScaleBasic = false;
                this.scaleChordType = ChordType.SecondStep;
                this.priority = ChordPriority.SecondStep;
            }
            else if ((int)musicAI.matchChords(this, tonation.thirdStep) > (int)Match.Medium)
            {
                this.isScaleBasic = false;
                this.scaleChordType = ChordType.ThirdStep;
                this.priority = ChordPriority.ThirdStep;
            }
            else
            {
                this.isScaleBasic = false;
                this.scaleChordType = ChordType.Other;
                this.priority = ChordPriority.Default;
            }
        }

        public void initMainChord(ChordType type, Offset off, ChordPriority priority)
        {
            scaleChordType = type;
            this.priority = priority;
            offset = off;
            if (type == ChordType.Other)
                isScaleBasic = false;
            else
                isScaleBasic = true;
            turnover = 0;
            for (int i = 0; i < chordNotes.Count; i++)
                chordNotes[i] = (chordNotes[i] + (int)off) % 12;
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
            ret += " Priority: " + (int)priority;
            ret += " Beat: " + beatStrength.ToString();
            return ret;
        }

        public override bool Equals(object obj)
        {
            TonationChord other = null;
            try {
                other = (TonationChord)obj;
            }
            catch(InvalidCastException exc){
                return false;
            }
            if (other == null)
                return false;

            if (chordNotes.Count != other.chordNotes.Count)
                return false;
            for (int i = 0; i < chordNotes.Count; i++)
            {
                if (chordNotes[i] != other.chordNotes[i])
                    return false;
            }
            return true;
        }
    }
}
