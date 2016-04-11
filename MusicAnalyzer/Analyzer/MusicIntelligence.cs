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
        Dictionary<Metrum, List<MeasureBeats>> measureBeats;

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

        public void setRightNoteSigns(List<Tonation> tonations, ref NotesList notesList, MidiTools midiTools){
            foreach (Note n in notesList)
            {
                if (midiTools.getSharpOrFlat(n) != 0)
                {
                    int fifths = midiTools.getTonationFifths(midiTools.getCurrentTonation(tonations, n.startTime));
                    if (fifths > 0){
                        n.basicNote = n.basicNote.Split('/')[0];
                        n.note = n.basicNote + n.octave;
                    }
                        
                    else{
                        n.basicNote = n.basicNote.Split('/')[1];
                        n.note = n.basicNote + n.octave;
                    }
                        
                }
            }
        }

        public void setNotesRythm(ref NotesList notesList, MidiTools midiTools, int division){
            foreach (Note n in notesList)
            {
                double p = midiTools.ProperDurationScale(n, division);
                string[] rythmicValues = Enum.GetNames(typeof(PSAMControlLibrary.MusicalSymbolDuration)).Where(x => (int)(PSAMControlLibrary.MusicalSymbolDuration)(Enum.Parse(typeof(PSAMControlLibrary.MusicalSymbolDuration), x)) == p).ToArray();
                if (rythmicValues.Length > 0)
                    n.rythmicValue = (PSAMControlLibrary.MusicalSymbolDuration)Enum.Parse(typeof(PSAMControlLibrary.MusicalSymbolDuration), rythmicValues[0]);
                else
                    n.rythmicValue = PSAMControlLibrary.MusicalSymbolDuration.Quarter;
            }
        }

        public PSAMControlLibrary.Clef setTrackClef(List<Note> notesList)
        {
            int bassCounter = notesList.Where(x => x.octave < 4).Count();
            if (bassCounter > notesList.Count / 2)
                return new PSAMControlLibrary.Clef(PSAMControlLibrary.ClefType.FClef, 4);
            else
                return new PSAMControlLibrary.Clef(PSAMControlLibrary.ClefType.GClef, 2);
        }

        public SortedList<int, TonationChord> createOrderedChords(IEnumerable<Note> notesList, MidiTools midiTools, List<Tonation> tonations)
        {
            SortedList<int, TonationChord> orderedNoteChords = new SortedList<int, TonationChord>();
            foreach (Note n in notesList)
            {
                if (!orderedNoteChords.ContainsKey(n.startTime))
                {
                    orderedNoteChords.Add(n.startTime, new TonationChord(n, midiTools.getCurrentTonation(tonations, n.startTime)));
                }
                else
                    orderedNoteChords[n.startTime].addNote(n);
            }
            int durationCounter, timeIndex;
            foreach (Note n in notesList){
                durationCounter = 1;
                if (n.startTime == orderedNoteChords.Keys.Max())
                    continue;
                timeIndex = orderedNoteChords.ElementAt(orderedNoteChords.IndexOfKey(n.startTime) + durationCounter).Key;
                while (n.endTime >= timeIndex)
                {
                    orderedNoteChords[timeIndex].addNote(n);
                    if (timeIndex == orderedNoteChords.Keys.Max())
                        break;
                    durationCounter++;
                    timeIndex = orderedNoteChords.ElementAt(orderedNoteChords.IndexOfKey(n.startTime) + durationCounter).Key;
                }
            }
            return orderedNoteChords;
        }

        public void setChordTypes(SortedList<int, TonationChord> chordsList, List<Tonation> tonations, MidiTools midiTools)
        {
            foreach (int chordsIndex in chordsList.Keys)
            {
                chordsList[chordsIndex].setChordTypesAndPriority(this);
            }
        }

        public void initMeasureBeats(List<Metrum> meterChanges)
        {
            measureBeats = new Dictionary<Metrum, List<MeasureBeats>>();
            foreach (Metrum m in meterChanges)
            {
                List<MeasureBeats> beats = new List<MeasureBeats>();
                switch (m.Denominator * m.Numerator)
                {
                    case 4:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Strong };
                        break;
                    case 6:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Strong, MeasureBeats.Strong };
                        break;
                    case 8:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Strong};
                        break;
                    case 12:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Strong, MeasureBeats.Strong };
                        break;
                    case 16:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Weak };
                        break;
                    case 24:
                        if (m.Numerator == 3)
                            beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Strong, MeasureBeats.Strong };
                        else
                            beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Weak };
                        break;
                    case 32:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Weak };
                        break;
                    case 48:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Strong, MeasureBeats.Strong, MeasureBeats.Weak };
                        break;
                    case 72:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin, MeasureBeats.Weak, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Weak, MeasureBeats.Weak, MeasureBeats.Strong, MeasureBeats.Weak, MeasureBeats.Weak };
                        break;
                    default:
                        beats = new List<MeasureBeats>() { MeasureBeats.Begin };
                        for (int i = 1; i < m.Numerator; i++)
                            beats.Add(MeasureBeats.Weak);
                        break;
                }
                measureBeats[m] = beats;
            }
        }

        public void setBeatStrength(ref SortedList<int, TonationChord> orderedNoteChords, List<Metrum> meterChanges, MidiTools midiTools, int division)
        {
            foreach(int timeIndex in orderedNoteChords.Keys)
            {
                Metrum m = midiTools.getCurrentMetrum(timeIndex, meterChanges);
                List<MeasureBeats> beatMeasure = measureBeats[m];
                int measureBeat = (timeIndex - m.startTick) % (m.Numerator * (division * 4 / m.Denominator)); 
                if (measureBeat % (division * 4 / m.Denominator) != 0)
                    orderedNoteChords[timeIndex].beatStrength = MeasureBeats.Weak;
                else{
                    measureBeat = measureBeat / (division * 4 / m.Denominator);
                    if (measureBeat < beatMeasure.Count)
                        orderedNoteChords[timeIndex].beatStrength = beatMeasure[measureBeat];
                    else
                        orderedNoteChords[timeIndex].beatStrength = MeasureBeats.Weak;
                }   
            }
#if DEBUG
            MidiTools.genericListSerizliator<Chord>(orderedNoteChords.Values.ToList<Chord>(), midiTools.configDirectory + "\\sortedNotes.txt");
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

        public bool isBairlineNow(int now, List<Metrum> meterChanges, int division, MidiTools midiTools)
        {
            Metrum m = midiTools.getCurrentMetrum(now, meterChanges);
            if (m == null)
                return false;
            else
                return ((now - m.startTick) % (m.Numerator * (division * 4 / m.Denominator)) == 0);
        }

        public PSAMControlLibrary.Rest isRestNow(Note last, Note now, int division, MidiTools midiTools)
        {
            if (last == null)
                return null;
            int p = (int)(4 * division / midiTools.ProperDurationScale(last, division));
            int gap = now.startTime - (last.startTime + p);
            if (gap <= 0)
                return null;
            if (gap / (double)division == 1)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.Quarter);
            if (gap / (double)division == 2)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.Half);
            if (gap / (double)division == 4)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.Whole);
            if (gap / (double)division == 0.5)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.Eighth);
            if (gap / (double)division == 0.25)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.Sixteenth);
            if (gap / (double)division == 0.125)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.d32nd);
            if (gap / (double)division == 0.0625)
                return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.d64th);
            return new PSAMControlLibrary.Rest(PSAMControlLibrary.MusicalSymbolDuration.Unknown);
        }

        public PSAMControlLibrary.TimeSignature isTimeSignatureNow(int now, List<Metrum> meterChanges)
        {
            Metrum meter = meterChanges.FirstOrDefault(x => x.startTick == now);
            if (meter != null)
                return new PSAMControlLibrary.TimeSignature(PSAMControlLibrary.TimeSignatureType.Numbers, Convert.ToUInt32(meter.Numerator), Convert.ToUInt32(meter.Denominator));
            else
                return null;
        }

        public PSAMControlLibrary.Key isTonationChangeNow(int now, List<Tonation> tonations, MidiTools midiTools)
        {
            Tonation t = tonations.FirstOrDefault(x => x.startTick == now);
            if (t != null)
                return new PSAMControlLibrary.Key(midiTools.getTonationFifths(t));
            else
                return null;
        }

        public Chord turnIntoPureChord(TonationChord inChord)
        {
            List<int> pureNotes = new List<int>();
            foreach (int noteIndex in inChord.chordNotes)
            {
                pureNotes.Add((noteIndex + (int)inChord.offset) % 12);
            }
            pureNotes.Sort();
            return new Chord(pureNotes);
        }

        public Match notesMatch(Chord chord, Note note, Tonation tonation)
        {
            Match match = Match.None;
            
            return match;
        }
    }
}
