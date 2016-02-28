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
        int trackCount;
        Dictionary<int, Chord> orderedNoteChords; // notes played at the specified moment, kept together create a chord
        int composedTrack;

        public MusicPiece(Sequence seq, string configDir)
        {
            this.sequence = seq;
            this.midiTools = new MidiTools(configDir);
            this.notesList = midiTools.decodeNotes(sequence) as NotesList;
            this.tonations = midiTools.grabTonations(sequence);
            this.trackCount = this.sequence.Count;
            midiTools.findMeter(seq);
            midiTools.serialize(notesList);
            serializeTonations(configDir);
        }

        public void orderAllNotesByStartTime()
        {
            orderedNoteChords = new Dictionary<int, Chord>();
            List<Note> orderedNotesList = notesList.OrderBy(x => x.startTime).ToList<Note>();
            foreach (Note n in orderedNotesList)
            {
                if (!orderedNoteChords.ContainsKey(n.startTime))
                {
                    orderedNoteChords.Add(n.startTime, new Chord(n));
                } else
                    orderedNoteChords[n.startTime].addNote(n);
                //int[] nextChords = orderedNoteChords.Keys.Where(x => x >= n.startTime && x <= n.endTime).ToArray();
                if (orderedNotesList.IndexOf(n) == orderedNotesList.Count - 1)
                    break;
                Note another = orderedNotesList[orderedNotesList.IndexOf(n) + 1];
                while (n.endTime >= another.startTime)
                {
                    orderedNoteChords[n.startTime].addNote(another);
                    if (orderedNotesList.IndexOf(another) == orderedNotesList.Count - 1)
                        break;
                    another = orderedNotesList[orderedNotesList.IndexOf(another) + 1];
                }
            }
            serializeSortNotes(midiTools.configDirectory);
        }

        public void findChordChanges()
        {
            // tworzy tonikę na dźwięku - new Chord(n, getCurrentTonation(n.startTime);
        }

        public Tonation getCurrentTonation(int timeIndex)
        {
            return tonations.FirstOrDefault(x => timeIndex >= x.startTick && timeIndex <= x.endTick);
        }

        void serializeTonations(string dir)
        {
            string fileName = dir + "\\tonations.txt";
            List<string> allLines = new List<string>();
            foreach (Tonation t in tonations)
            {
                allLines.Add(t.ToString());
            }
            IOTools.saveTo(allLines, fileName);
        }

        void serializeSortNotes(string dir)
        {
            string fileName = dir + "\\sortedNotes.txt";
            List<string> allLines = new List<string>();
            foreach (int index in orderedNoteChords.Keys)
            {
                allLines.Add("TimeStamp: " + index.ToString() + orderedNoteChords[index].ToString());
            }
            IOTools.saveTo(allLines, fileName);
        }
    }
}
