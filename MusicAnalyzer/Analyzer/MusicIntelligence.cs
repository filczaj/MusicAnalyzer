using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Analyzer
{
    class MusicIntelligence
    {
        public void setRightTonation(List<Tonation> tonations, IEnumerable<Note> notesList, MidiTools midiTools) // differentatie Cdur from Amoll
        {
            int starter;
            int endTick;
            int majorCounter;
            int minorCounter;
            int upper, lower;
            List<Tonation> newTonations = new List<Tonation>();
            foreach (Tonation t in tonations)
            {
                starter = t.startTick;
                endTick = t.endTick;
                majorCounter = 0;
                minorCounter = 0;
                if (t.mode == ChordMode.Major)
                {
                    lower = ((int)t.offset + 7) % 12;
                    upper = ((int)t.offset + 8) % 12;
                }
                else
                {
                    lower = ((int)t.offset + 10) % 12;
                    upper = ((int)t.offset + 11) % 12;
                }
                foreach (Note n in notesList)
                {
                    if (n.startTime >= starter && n.startTime < endTick && ((n.noteID - 3) % 12) == lower)
                        majorCounter++;
                    if (n.startTime >= starter && n.startTime < endTick && ((n.noteID - 3) % 12) == upper)
                        minorCounter++;
                }
                if ((majorCounter > 2 * minorCounter && t.mode == ChordMode.Minor) ||
                    (majorCounter <= 2 * minorCounter && t.mode == ChordMode.Major))
                {
                    newTonations.Add(midiTools.getSiblingTonation(t));
                }
                else
                    newTonations.Add(t);
            }
            tonations = newTonations;
        }

        public Dictionary<int, Chord> createOrderedChords(IEnumerable<Note> notesList, MidiTools midiTools)
        {
            Dictionary<int, Chord> orderedNoteChords = new Dictionary<int, Chord>();
            List<Note> orderedNotesList = notesList.OrderBy(x => x.startTime).ToList<Note>();
            foreach (Note n in orderedNotesList)
            {
                if (!orderedNoteChords.ContainsKey(n.startTime))
                {
                    orderedNoteChords.Add(n.startTime, new Chord(n));
                }
                else
                    orderedNoteChords[n.startTime].addNote(n);
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
#if DEBUG
            MidiTools.genericListSerizliator<Chord>(orderedNoteChords.Values.ToList<Chord>(), midiTools.configDirectory + "\\sortedNotes.txt");
#endif
            return orderedNoteChords;
        }

        public Match notesMatch(Chord chord, Note note, Tonation tonation)
        {
            Match match = Match.None;
            
            return match;
        }
    }
}
