using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Tools;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;

namespace MusicAnalyzer.Models
{
    public class Tonation : TimeSpanEvent
    {
        // dodać pozostałe charakterystyczne akordy : od 2., od 6.
        public TonationChord tonic, subdominant, dominant, secondStep, sixthStep, thirdStep;
        public List<int> mainScaleNotes;
        public Offset offset;
        public ChordMode mode;
        NoteTools noteTools;
        public Key key;

        public Tonation(MetaMessage metaM, int ticks, NoteTools tools)
        {
            KeySignatureBuilder keyBuilder = new KeySignatureBuilder(metaM);
            this.key = keyBuilder.Key;
            if (key > Key.ASharpMinor)
                this.mode = ChordMode.Major;
            else
                this.mode = ChordMode.Minor;
            this.startTick = ticks;
            this.endTick = int.MaxValue - 1;
            this.noteTools = tools;
            this.offset = noteTools.setOffset(key);
            this.mainScaleNotes = noteTools.setMainScaleNotes(mode);
            List<TonationChord> chords = noteTools.setMainChords(mode);
            if (chords.Count >= 6)
            {
                tonic = chords[0];
                tonic.initMainChord(ChordType.Tonic, offset, ChordPriority.Tonic);
                subdominant = chords[1];
                subdominant.initMainChord(ChordType.Subdominant, offset, ChordPriority.Subdominant);
                dominant = chords[2];
                dominant.initMainChord(ChordType.Dominant, offset, ChordPriority.Dominant);
                secondStep = chords[3];
                secondStep.initMainChord(ChordType.SecondStep, offset, ChordPriority.SecondStep);
                sixthStep = chords[4];
                sixthStep.initMainChord(ChordType.SixthStep, offset, ChordPriority.SixthStep);
                thirdStep = chords[5];
                thirdStep.initMainChord(ChordType.ThirdStep, offset, ChordPriority.ThirdStep);
            }
        }

        public Tonation(int intOffset, ChordMode tonationMode, int starter, NoteTools tools, int ender)
        {
            this.noteTools = tools;
            this.key = noteTools.setKeyOnOffsetAndMode(intOffset, tonationMode);
            this.offset = noteTools.setOffset(key);
            this.mode = tonationMode;
            this.startTick = starter;
            this.endTick = ender;
            this.mainScaleNotes = noteTools.setMainScaleNotes(mode);
            List<TonationChord> chords = noteTools.setMainChords(mode);
            if (chords.Count >= 6)
            {
                tonic = chords[0];
                tonic.initMainChord(ChordType.Tonic, offset, ChordPriority.Tonic);
                subdominant = chords[1];
                subdominant.initMainChord(ChordType.Subdominant, offset, ChordPriority.Subdominant);
                dominant = chords[2];
                dominant.initMainChord(ChordType.Dominant, offset, ChordPriority.Dominant);
                secondStep = chords[3];
                secondStep.initMainChord(ChordType.SecondStep, offset, ChordPriority.SecondStep);
                sixthStep = chords[4];
                sixthStep.initMainChord(ChordType.SixthStep, offset, ChordPriority.SixthStep);
                thirdStep = chords[5];
                thirdStep.initMainChord(ChordType.ThirdStep, offset, ChordPriority.ThirdStep);
            }
        }

        public int getNoteIndexInScale(Note n)
        {
            int basicId = (n.noteID - 3) % 12 + (int)offset;
            return basicId;
        }

        public List<int> getChordNotesOnIndex(int index)
        {
            List<int> chordNoteIndexes = new List<int>();
            chordNoteIndexes.Add((mainScaleNotes[index] + (int)offset) % 12);
            chordNoteIndexes.Add((mainScaleNotes[(index+2) % 7] + (int)offset) % 12);
            chordNoteIndexes.Add((mainScaleNotes[(index + 4) % 7] + (int)offset) % 12);
            return chordNoteIndexes;
        }

        public override string ToString()
        {
            return "Key: " + key.ToString() + " Mode: " + mode.ToString() + " Offset: " + offset.ToString() + " Start: " + startTick.ToString()
                + " End: " + endTick.ToString();
        }
    }
}
