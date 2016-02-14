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
        Chord tonic, subdominant, dominant;
        List<int> mainScaleNotes;
        Offset offset;
        ChordMode mode;
        NoteTools noteTools;
        public int startTick, endTick;
        public Key key;

        public Tonation(String key, ChordMode mode)
        {
            noteTools = new NoteTools();
            this.mode  = mode;
            this.mainScaleNotes = noteTools.setMainScaleNotes(mode);
            List<Chord> chords = noteTools.setMainChords(mode);
            if (chords.Count == 3)
            {
                tonic = chords[0];
                tonic.priority = 3;
                dominant = chords[1];
                subdominant = chords[2];
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
            this.noteTools = tools;
            this.offset = noteTools.setOffset(key);
            this.mainScaleNotes = noteTools.setMainScaleNotes(mode);
            List<Chord> chords = noteTools.setMainChords(mode);
            if (chords.Count == 3)
            {
                tonic = chords[0];
                subdominant = chords[1];
                dominant = chords[2];
            }
        }

        public int getNoteIndexInScale(Note n)
        {
            int index = -1;
            int basicId = (n.noteID - (int)offset) % 12;
            index = mainScaleNotes.IndexOf(basicId);
            return index;
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
