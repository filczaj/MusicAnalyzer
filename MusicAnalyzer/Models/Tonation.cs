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
    public class Tonation
    {
        // dodać pozostałe charakterystyczne akordy : od 2., od 6.
        public TonationChord tonic, subdominant, dominant;
        List<int> mainScaleNotes;
        public Offset offset;
        public ChordMode mode;
        NoteTools noteTools;
        public int startTick, endTick;
        public Key key;

        public Tonation(String key, ChordMode mode)
        {
            noteTools = new NoteTools();
            this.mode  = mode;
            this.mainScaleNotes = noteTools.setMainScaleNotes(mode);
            List<TonationChord> chords = noteTools.setMainChords(mode);
            if (chords.Count == 3)
            {
                tonic = chords[0];
                tonic.scaleChordType = ChordType.Tonic;
                tonic.priority = 3;
                subdominant = chords[1];
                subdominant.scaleChordType = ChordType.Dominant;
                dominant = chords[2];
                dominant.scaleChordType = ChordType.Subdominant;
            }
        }

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
            if (chords.Count == 3)
            {
                tonic = chords[0];
                tonic.scaleChordType = ChordType.Tonic;
                tonic.priority = 3;
                subdominant = chords[1];
                subdominant.scaleChordType = ChordType.Dominant;
                dominant = chords[2];
                dominant.scaleChordType = ChordType.Subdominant;
            }
        }

        public Tonation(int intOffset, ChordMode tonationMode, int starter, NoteTools tools)
        {
            this.noteTools = tools;
            this.key = noteTools.setKeyOnOffsetAndMode(intOffset, tonationMode);
            this.offset = noteTools.setOffset(key);
            this.mode = tonationMode;
            this.startTick = starter;
            this.endTick = int.MaxValue - 1;
            this.mainScaleNotes = noteTools.setMainScaleNotes(mode);
            List<TonationChord> chords = noteTools.setMainChords(mode);
            if (chords.Count == 3)
            {
                tonic = chords[0];
                tonic.scaleChordType = ChordType.Tonic;
                tonic.priority = 3;
                subdominant = chords[1];
                subdominant.scaleChordType = ChordType.Dominant;
                dominant = chords[2];
                dominant.scaleChordType = ChordType.Subdominant;
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
