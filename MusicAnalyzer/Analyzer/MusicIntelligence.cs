using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Analyzer
{
    public class MusicIntelligence
    {
        public List<Tonation> setRightTonation(List<Tonation> tonations, IEnumerable<Note> notesList, MidiTools midiTools) // differentatie Cdur from Amoll
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
#if DEBUG
            MidiTools.genericListSerizliator<Tonation>(tonations, "C:\\Magisterka\\MusicAnalyzer\\MusicAnalyzer\\ConfigFiles\\Tonations.txt");
#endif
            return tonations;
        }

        public SortedList<int, Chord> createOrderedChords(IEnumerable<Note> notesList, MidiTools midiTools)
        {
            //Dictionary<int, Chord> orderedNoteChords = new Dictionary<int, Chord>();
            SortedList<int, Chord> orderedNoteChords = new SortedList<int, Chord>();
            //List<Note> orderedNotesList = notesList.OrderBy(x => x.startTime).ToList<Note>();
            foreach (Note n in notesList)
            {
                if (!orderedNoteChords.ContainsKey(n.startTime))
                {
                    orderedNoteChords.Add(n.startTime, new Chord(n));
                }
                else
                    orderedNoteChords[n.startTime].addNote(n);
            }
            foreach (Note n in notesList){
                if (n.startTime == orderedNoteChords.Keys[orderedNoteChords.Count - 1])
                    break;
                else
                {
                    int durationCounter = 1;
                    int timeIndex = orderedNoteChords.ElementAt(orderedNoteChords.IndexOfKey(n.startTime) + durationCounter).Key;
                    while (n.endTime >= timeIndex)
                    {
                        orderedNoteChords[timeIndex].addNote(n);
                        if (timeIndex == orderedNoteChords.Keys[orderedNoteChords.Count - 1])
                            break;
                        durationCounter++;
                        timeIndex = orderedNoteChords.ElementAt(orderedNoteChords.IndexOfKey(n.startTime) + durationCounter).Key;
                    }
                }
            }
            return orderedNoteChords;
        }

        public void setChordTypes(SortedList<int, Chord> chordsList, List<Tonation> tonations, MidiTools midiTools)
        {
            //int noteIndex;
            foreach (int chordsIndex in chordsList.Keys)
            {
                //noteIndex = midiTools.getCurrentTonation(tonations, 0).getNoteIndexInScale(new Note()); // to do!!!!!!
                chordsList[chordsIndex].setChordType(midiTools.getCurrentTonation(tonations, chordsIndex), this);
            }
#if DEBUG
            MidiTools.genericListSerizliator<Chord>(chordsList.Values.ToList<Chord>(), midiTools.configDirectory + "\\sortedNotes.txt");
#endif
        }

        public Match matchChords(Chord a, Chord b)
        {
            int counter = 0;
            foreach (int note in a.chordNotes)
                if (!b.chordNotes.Contains(note))
                    counter++;
            foreach (int note in b.chordNotes)
                if (!a.chordNotes.Contains(note))
                    counter++;
            if (counter == 0) return Match.Perfect;
            else if (counter == 1) return Match.Good;
            else if (counter == 2) return Match.Medium;
            else if (counter == 3) return Match.Poor;
            else return Match.None;
        }

        public Match notesMatch(Chord chord, Note note, Tonation tonation)
        {
            Match match = Match.None;
            
            return match;
        }
    }
}
