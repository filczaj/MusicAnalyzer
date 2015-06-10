using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Models
{
    class Tonation
    {
        private String majorChordsFile = "D:\\magisterka\\majorChords.txt";
        private String minorChordsFile = "D:\\magisterka\\minorChords.txt";

        Chord tonic, subdominant, dominant;
        List<int> mainScaleNotes;
        String tonationKey;
        int offset;
        bool majorMinor;
        NoteTools noteTools;

        public Tonation()
        {
            noteTools = new NoteTools();
            setMainChords();
        }

        public Tonation(String key, bool majorMinor)
        {
            noteTools = new NoteTools();
            tonationKey = key;
            offset = noteTools.notesSequence[tonationKey];
            this.majorMinor = majorMinor;
            setScaleNotes();
            setMainChords();
        }

        private void setScaleNotes(){
            mainScaleNotes = new List<int>();
            List<int> notesSeq;
            if (majorMinor)
                notesSeq = noteTools.majorScaleSeq;
            else
                notesSeq = noteTools.minorScaleSeq;
            foreach (int n in notesSeq)
            {
                mainScaleNotes.Add((n + offset)%12);
            }
        }

        private void setMainChords()
        {
            String chordsFile;
            if (majorMinor)
                chordsFile = majorChordsFile;
            else
                chordsFile = minorChordsFile;
            IEnumerable<String>lines = IOTools.ReadFrom(chordsFile);

            tonic = new Chord(majorMinor, true, 0);
            tonic.chordNotes = new List<int>();
            int[] asIntegers = lines.ElementAt(0).Split(' ').Select(s => int.Parse(s) + offset).ToArray();
            tonic.chordNotes.AddRange(asIntegers);

            subdominant = new Chord(majorMinor, true, 0);
            subdominant.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(1).Split(' ').Select(s => int.Parse(s) + offset).ToArray();
            subdominant.chordNotes.AddRange(asIntegers);

            dominant = new Chord(majorMinor, true, 0);
            dominant.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(2).Split(' ').Select(s => int.Parse(s) + offset).ToArray();
            dominant.chordNotes.AddRange(asIntegers);
        }
    }
}
