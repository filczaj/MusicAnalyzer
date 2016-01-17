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
        Chord tonic, subdominant, dominant;
        List<int> mainScaleNotes;
        int offset; // to do!!!!
        bool majorMinor;
        NoteTools noteTools;
        public int startTick, endTick;
        public Key key;

        public Tonation(String key, bool majorMinor)
        {
            noteTools = new NoteTools();
            this.majorMinor = majorMinor;
            this.mainScaleNotes = noteTools.setMainScaleNotes(majorMinor);
            List<Chord> chords = noteTools.setMainChords(majorMinor);
            if (chords.Count == 3)
            {
                tonic = chords[0];
                dominant = chords[1];
                subdominant = chords[2];
            }
        }

        public Tonation(MetaMessage metaM, int ticks, NoteTools tools)
        {
            KeySignatureBuilder keyBuilder = new KeySignatureBuilder(metaM);
            this.key = keyBuilder.Key;
            this.majorMinor = (key > Key.ASharpMinor);
            this.startTick = ticks;
            this.noteTools = tools;
            this.mainScaleNotes = noteTools.setMainScaleNotes(majorMinor);
            List<Chord> chords = noteTools.setMainChords(majorMinor);
            if (chords.Count == 3)
            {
                tonic = chords[0];
                subdominant = chords[1];
                dominant = chords[2];
            }
        }

        public override string ToString()
        {
            return "Key: " + key.ToString() + " Ismajor: " + majorMinor.ToString() + " Start: " + startTick.ToString()
                + " End: " + endTick.ToString() + " chords: " + tonic.chordNotes.Count.ToString() + "; " + dominant.chordNotes.Count.ToString() + "; "
                    + subdominant.chordNotes.Count.ToString() + " ScaleNotes: " + mainScaleNotes.Count.ToString();
        }
    }
}
