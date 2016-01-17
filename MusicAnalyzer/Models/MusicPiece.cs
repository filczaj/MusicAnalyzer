using MusicAnalyzer.Tools;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    class MusicPiece
    {
        List<Tonation> tonations;
        Sequence sequence;
        NotesList notesList;
        MidiTools midiTools;
        int composedTrack;

        public MusicPiece(Sequence seq, string configDir)
        {
            this.sequence = seq;
            this.midiTools = new MidiTools(configDir);
            this.notesList = midiTools.decodeNotes(sequence) as NotesList;
            this.tonations = midiTools.grabTonations(sequence);
            midiTools.serialize(notesList);
            serializeTonations();
        }

        void serializeTonations()
        {
            string fileName = "C:\\Magisterka\\MusicAnalyzer\\MusicAnalyzer\\ConfigFiles\\tonations.txt";
            List<string> allLines = new List<string>();
            foreach (Tonation t in tonations)
            {
                allLines.Add(t.ToString());
            }
            IOTools.saveTo(allLines, fileName);
        }
    }
}
