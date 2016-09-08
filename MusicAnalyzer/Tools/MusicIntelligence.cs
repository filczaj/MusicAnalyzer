using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Tools
{
    public class MusicIntelligence
    {
        Dictionary<Metrum, List<MeasureBeats>> measureBeats;

        public List<Tonation> setRightTonation(List<Tonation> tonations, List<Metrum> meterChanges, int division, IEnumerable<Note> notesList, MidiTools midiTools) // differentatie Cdur from Amoll
        {
            int starter;
            int endTick;
            int majorCounter;
            int minorCounter;
            int upper, lower;
            int majorTonicOffset, minorTonicOffset;
            int minorTonicCounter, majorTonicCounter;
            Metrum metrum = null;
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
                    majorTonicOffset = (int)t.offset;
                    minorTonicOffset = majorTonicOffset - 3;
                }
                else
                {
                    lower = ((int)t.offset + 10) % 12;
                    upper = ((int)t.offset + 11) % 12;
                    minorTonicOffset = (int)t.offset;
                    majorTonicOffset = minorTonicOffset + 3;
                }
                minorTonicCounter = 0;
                majorTonicCounter = 0;
                foreach (Note n in notesList)
                {
                    if (n.startTime >= starter && n.startTime < endTick && ((n.noteID - 3) % 12) == lower)
                        majorCounter++;
                    if (n.startTime >= starter && n.startTime < endTick && ((n.noteID - 3) % 12) == upper)
                        minorCounter++;
                    metrum = MidiTools.getCurrentMetrum(n.startTime, meterChanges);
                    int measureBeat = (n.startTime - metrum.startTick) % (metrum.Numerator * (division * 4 / metrum.Denominator));
                    if (measureBeat == 0 && ((n.noteID - 3) % 12) == minorTonicOffset)
                        minorTonicCounter++;
                    if (measureBeat == 0 && ((n.noteID - 3) % 12) == majorTonicOffset)
                        majorTonicCounter++;
                }
                if (((majorCounter > 2 * minorCounter || majorTonicCounter > minorTonicCounter) && t.mode == ChordMode.Minor) ||
                    ((majorCounter <= 2 * minorCounter || majorTonicCounter <= minorTonicCounter) && t.mode == ChordMode.Major))
                {
                    newTonations.Add(midiTools.getFullSiblingTonation(t));
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

        public void setRightNoteSigns(List<Tonation> tonations, ref IEnumerable<Note> notesList){
            foreach (Note n in notesList)
            {
                if (MidiTools.getSharpOrFlat(n) != 0)
                {
                    int fifths = MidiTools.getTonationFifths(MidiTools.getCurrentTonation(tonations, n.startTime));
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

        public void setNotesRythm(ref NotesList notesList, MidiTools midiTools, int division, int TrackID) {
            foreach (Note n in notesList)
            {
                if (TrackID == -1 || TrackID == n.trackID)
                {
                    double p = midiTools.ProperDurationAndExtension(n, division);
                    string[] rythmicValues = Enum.GetNames(typeof(PSAMControlLibrary.MusicalSymbolDuration)).Where(x => (int)(PSAMControlLibrary.MusicalSymbolDuration)(Enum.Parse(typeof(PSAMControlLibrary.MusicalSymbolDuration), x)) == p).ToArray();
                    if (rythmicValues.Length > 0)
                        n.rythmicValue = (PSAMControlLibrary.MusicalSymbolDuration)Enum.Parse(typeof(PSAMControlLibrary.MusicalSymbolDuration), rythmicValues[0]);
                    else
                        n.rythmicValue = PSAMControlLibrary.MusicalSymbolDuration.Quarter;
                }
            }
        }

        public PSAMControlLibrary.Clef setTrackClef(List<Note> notesList)
        {
            if (notesList == null || notesList.Count == 0)
                return new PSAMControlLibrary.Clef(PSAMControlLibrary.ClefType.GClef, 2);
            int bassCounter = notesList.Where(x => x.octave < 4).Count();
            if (bassCounter > notesList.Count / 2)
                return new PSAMControlLibrary.Clef(PSAMControlLibrary.ClefType.FClef, 4);
            else
                return new PSAMControlLibrary.Clef(PSAMControlLibrary.ClefType.GClef, 2);
        }

        public SortedList<int, TonationChord> createOrderedChords(IEnumerable<Note> notesList, List<Tonation> tonations)
        {
            SortedList<int, TonationChord> orderedNoteChords = new SortedList<int, TonationChord>();
            foreach (Note n in notesList)
            {
                if (!orderedNoteChords.ContainsKey(n.startTime))
                {
                    orderedNoteChords.Add(n.startTime, new TonationChord(n, MidiTools.getCurrentTonation(tonations, n.startTime)));
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

        public void setChordsDuration(SortedList<int, TonationChord> chordsList, int division, List<Metrum> meterChanges, ComposedTrack track = null)
        {
            if (chordsList.Count == 1)
                chordsList.First().Value.duration = 0;
            List<int> timeIndices = chordsList.Keys.ToList();
            for (int i = timeIndices.Count - 2; i >= 0; i--)
                chordsList[timeIndices[i]].duration = timeIndices[i + 1] - timeIndices[i];
            Metrum m = MidiTools.getCurrentMetrum(timeIndices[timeIndices.Count - 1], meterChanges);
            chordsList[timeIndices[timeIndices.Count - 1]].duration = (m.Numerator * 4 * division) / m.Denominator;
            if (track != null)
                track.AVGChordDuration = track.noteChords.Average(x => x.Value.duration);
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
                Metrum m = MidiTools.getCurrentMetrum(timeIndex, meterChanges);
                List<MeasureBeats> beatMeasure = measureBeats[m];
                int measureBeat = (timeIndex - m.startTick) % (m.Numerator * (division * 4 / m.Denominator)); 
                if (measureBeat % (division * 4 / m.Denominator) != 0){
                    if ((2 * measureBeat) % (division * 4 / m.Denominator) != 0)
                        orderedNoteChords[timeIndex].beatStrength = MeasureBeats.Default;
                    else
                        orderedNoteChords[timeIndex].beatStrength = MeasureBeats.Weak;
                }
                else{
                    measureBeat = measureBeat / (division * 4 / m.Denominator);
                    if (measureBeat < beatMeasure.Count)
                        orderedNoteChords[timeIndex].beatStrength = beatMeasure[measureBeat];
                    else
                    {
                        if (((2 * measureBeat) % m.Numerator) % (division * 4 / m.Denominator) != 0)
                            orderedNoteChords[timeIndex].beatStrength = MeasureBeats.Default;
                        else
                            orderedNoteChords[timeIndex].beatStrength = MeasureBeats.Weak;
                    }
                }   
            }
#if DEBUG
            MidiTools.genericListSerizliator<Chord>(orderedNoteChords.Values.ToList<Chord>(), midiTools.configDirectory + "\\sortedNotes.txt");
#endif
        }

        public void setCoreChords(ComposedTrack composedTrack, ComposedTrack inputTrack, MusicPiece musicPiece)
        {
            TonationChord lastChord = null;
            Dictionary<int, List<TonationChord>> notesAtChord = new Dictionary<int, List<TonationChord>>();
            int chordTime = 0;
            foreach (int timeIndex in inputTrack.noteChords.Keys)
            {
                if ((int)inputTrack.noteChords[timeIndex].beatStrength > (int)MeasureBeats.Weak)
                {
                    chordTime = timeIndex;
                    notesAtChord[chordTime] = new List<TonationChord>();
                }
                if ((int)inputTrack.noteChords[timeIndex].beatStrength > (int)MeasureBeats.Default)
                {
                    if (!notesAtChord.ContainsKey(chordTime))
                        notesAtChord[chordTime] = new List<TonationChord>();
                    notesAtChord[chordTime].Add(inputTrack.noteChords[timeIndex]);
                }
            }
            foreach (int timeIndex in notesAtChord.Keys)
            {
                TonationChord newChord = createMatchingChord(notesAtChord[timeIndex], lastChord, MidiTools.getCurrentTonation(musicPiece.tonations, timeIndex));
                composedTrack.noteChords.Add(timeIndex, newChord);
                lastChord = composedTrack.noteChords[timeIndex];
            }
        }

        public ComposedTrack createTrackFromScratch(ComposedTrack inputTrack, MusicPiece musicPiece)
        {
            ComposedTrack newTrack = new ComposedTrack();
            setCoreChords(newTrack, inputTrack, musicPiece);
            joinSimilarChords(newTrack);
            setChordsDuration(newTrack.noteChords, musicPiece.Division, musicPiece.meterChanges);
            return newTrack;
        }

        private void joinSimilarChords(ComposedTrack track) // half of the similar chords which are next to each other convert to a longer one chord with higher priority
        {
            List<int> timeIndices = track.noteChords.Keys.ToList();
            TonationChord lastChord = null, thisChord = null;
            int lastChordTime = 0;
            Random r = new Random();
            foreach (int timeIndex in timeIndices)
            {
                thisChord = track.noteChords[timeIndex];
                if (lastChord != null && lastChord.beatStrength >= thisChord.beatStrength && getAlternativeChords(thisChord).Contains(lastChord) && r.Next() % 2 == 0)
                {
                    if ((int)lastChord.priority < (int)thisChord.priority)
                    {
                        MeasureBeats mb = lastChord.beatStrength;
                        lastChord = new TonationChord(thisChord);
                        lastChord.beatStrength = mb;
                    }
                    track.noteChords.Remove(timeIndex);
                }
                else
                {
                    lastChord = track.noteChords[timeIndex];
                    lastChordTime = timeIndex;
                }
            }
        }

        public void changeRandomChords(ref ComposedTrack track, ComposedTrack inputTrack, int count) // we keep trying until we will change a dedicated number of chords
        {
            Random random = new Random();
            TonationChord tchInitial = null;
            int timeId = 0;
            int tries = 0;
            int constDuration;
            for (int i = 0; i < count; i++)
            {
                tries = 0;
                while (tries < 30 && (tchInitial == null || tchInitial.scaleChordType == ChordType.Other)){
                    timeId = random.Next(track.noteChords.Keys.Count);
                    tchInitial = track.noteChords[track.noteChords.Keys[timeId]];
                    tries++;
                }
                if (tchInitial == null || tchInitial.scaleChordType == ChordType.Other)
                    continue;
                tries = 0;
                constDuration = tchInitial.duration;
                while (tries < 30 && (tchInitial.priority == track.noteChords[track.noteChords.Keys[timeId]].priority))
                {
                    tries++;
                    tchInitial = getRandomAlternativeChord(inputTrack.noteChords[track.noteChords.Keys[timeId]]);
                }
                track.noteChords[track.noteChords.Keys[timeId]] = tchInitial;
                track.noteChords[track.noteChords.Keys[timeId]].duration = constDuration;
                track.noteChords[track.noteChords.Keys[timeId]].fillTonationChordData(inputTrack.noteChords[track.noteChords.Keys[timeId]]);
            }
        }

        public TonationChord getRandomAlternativeChord(TonationChord inputChord)
        {
            List<TonationChord> chordsToChoose = getAlternativeChords(inputChord);
            if (chordsToChoose.Count == 0)
                return new TonationChord(inputChord);
            if (chordsToChoose.Count == 1)
                return new TonationChord(chordsToChoose[0]);
            Random r = new Random();
            if (chordsToChoose.Count > 1 && r.Next(1000) % 3 != 0)
                return new TonationChord(chordsToChoose[0]);
            if (chordsToChoose.Count > 2)
            {
                if (r.Next(1000) % 2 == 0)
                    return new TonationChord(chordsToChoose[2]);
            }
            return new TonationChord(chordsToChoose[1]);
        }

        public List<TonationChord> getAlternativeChords(TonationChord inputChord, Tonation t = null)
        {
            Tonation tonation;
            if (t == null)
                tonation = inputChord.tonation;
            else
                tonation = t;
            List<TonationChord> alternativeChords = new List<TonationChord>();
            switch (inputChord.scaleChordType)
            {
                case ChordType.Tonic:
                    alternativeChords.Add(tonation.sixthStep);
                    alternativeChords.Add(tonation.thirdStep);
                    break;
                case ChordType.Subdominant:
                    alternativeChords.Add(tonation.secondStep);
                    alternativeChords.Add(tonation.sixthStep);
                    break;
                case ChordType.Dominant:
                    alternativeChords.Add(tonation.thirdStep);
                    if (tonation.mode == ChordMode.Minor)
                        alternativeChords.Add(tonation.majorThird);
                    break;
                case ChordType.SixthStep:
                    alternativeChords.Add(tonation.tonic);
                    alternativeChords.Add(tonation.subdominant);
                    break;
                case ChordType.SecondStep:
                    alternativeChords.Add(tonation.subdominant);
                    break;
                case ChordType.ThirdStep:
                    alternativeChords.Add(tonation.dominant);
                    alternativeChords.Add(tonation.tonic);
                    if (tonation.mode == ChordMode.Minor)
                        alternativeChords.Add(tonation.minorDominant);
                    break;
                default:
                    alternativeChords.Add(tonation.tonic);
                    break;
            }
            return alternativeChords;
        }

        TonationChord createMatchingChord(List<TonationChord> inputChords, TonationChord lastChord, Tonation tonation)
        {
            List<TonationChord> possibleChords = getPossibleChords(inputChords);
            TonationChord resultChord = getRandomlyWeightedChord(possibleChords, lastChord, inputChords[0].beatStrength, tonation);
            resultChord.fillTonationChordData(inputChords[0]);
            return resultChord;
        }

        public int penaltyMatchChords(TonationChord composed, TonationChord input, TonationChord lastChord, double AVGDuration, Metrum metrum, int division)
        {
            int penalty = 0;
            if (input.Equals(composed))
                return penalty;
            foreach (int noteIndex in input.chordNotes)
            {
                if (!input.tonation.mainScaleNotes.Contains(noteIndex))
                    continue;
                if (!composed.chordNotes.Contains(noteIndex))
                    penalty += 1;
                penalty += penaltySeventhStepChordCheck(composed, noteIndex);
            }
            if ((lastChord != null) && ((int)lastChord.priority < (int)composed.priority && lastChord.beatStrength > composed.beatStrength))
                penalty++;
            if (lastChord != null && lastChord.scaleChordType == composed.scaleChordType && lastChord.beatStrength < composed.beatStrength)
                penalty++;
            if (composed.duration > AVGDuration && !composed.isScaleBasic)
                penalty++;
            if (composed.duration < AVGDuration && composed.isScaleBasic)
                penalty++;
            //if (composed.duration <= (division * 4 / metrum.Denominator))
                //penalty++;
            if (composed.chordNotes.Count == 0)
                return 100;
            if (composed.chordNotes.Count < 3)
                penalty += (10 / composed.chordNotes.Count);
            return penalty;
        }

        private int penaltySeventhStepChordCheck(TonationChord chord, int noteIndex)
        {
            int seventh = (chord.chordNotes.Last() + 3) % 12;
            if ((chord.tonation.mainScaleNotes.Contains(seventh)) && (seventh == noteIndex))
                return -1;
            else
                return 0;
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

        public bool isBarlineNow(int now, List<Metrum> meterChanges, int division, MidiTools midiTools)
        {
            Metrum m = MidiTools.getCurrentMetrum(now, meterChanges);
            if (m == null)
                return false;
            else
                return ((now - m.startTick) % (m.Numerator * (division * 4 / m.Denominator)) == 0);
        }

        public PSAMControlLibrary.Rest isRestNow(Note last, Note now, int division, MidiTools midiTools)
        {
            if (last == null)
                return null;
            int p = (int)(4 * division / midiTools.ProperDurationAndExtension(last, division));
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

        public PSAMControlLibrary.Key isTonationChangeNow(int now, List<Tonation> tonations)
        {
            Tonation t = tonations.FirstOrDefault(x => x.startTick == now);
            if (t != null)
                return new PSAMControlLibrary.Key(MidiTools.getTonationFifths(t));
            else
                return null;
        }

        List<TonationChord> getPossibleChords(List<TonationChord> inputChords)
        {
            List<TonationChord> possibleChords;
            possibleChords = new List<TonationChord>() { inputChords[0].tonation.tonic, inputChords[0].tonation.subdominant, 
                inputChords[0].tonation.dominant, inputChords[0].tonation.secondStep, inputChords[0].tonation.sixthStep, inputChords[0].tonation.thirdStep };
            if (inputChords[0].tonation.mode == ChordMode.Minor){
                possibleChords.Add(inputChords[0].tonation.minorDominant);
                possibleChords.Add(inputChords[0].tonation.majorThird);
            }
            Dictionary<TonationChord,bool> isChordFine = new Dictionary<TonationChord,bool>();
            foreach (TonationChord chord in possibleChords)
                isChordFine.Add(chord, true);
            foreach (TonationChord inputChord in inputChords)
            {
                foreach (TonationChord possibleChord in possibleChords)
                {
                    if (!possibleChord.chordNotes.Intersect(inputChord.chordNotes).Any())
                        isChordFine[possibleChord] = false;
                }
            }
            possibleChords.RemoveAll(x => isChordFine[x] == false);
            if (possibleChords.Count == 0)
                possibleChords.Add(inputChords[0].tonation.dominant);
            if (inputChords.Count == 1 && inputChords[0].chordNotes.Count == 1)
            {
                TonationChord tch = possibleChords.Find(x => x.chordNotes[0] == inputChords[0].chordNotes[0]);
                if (tch != null)
                    tch.priority = ChordPriority.Tonic;
            }
            return possibleChords;
        }

        TonationChord getRandomlyWeightedChord(List<TonationChord> possibleChords, TonationChord lastChord, MeasureBeats beatStrength, Tonation tonation) 
        {
            if (possibleChords.Count == 0)
                return null;
            
            Random random = new Random();
            if (possibleChords.Exists(x => x.Equals(lastChord)))
                if (random.Next() % 4 == 0)
                    return new TonationChord(lastChord);

            List<int> chordsPriorities = new List<int>();
            // tonic and similar chords are twice more likely to be hosen at begining of the bars
            // subodominant, dominant or similar chords are twice more likely to be hosen at other strong measures in the bar
            if ((beatStrength == MeasureBeats.Begin && (possibleChords[0].priority == ChordPriority.Tonic || getAlternativeChords(tonation.tonic, tonation).Contains(possibleChords[0]))) ||
                (beatStrength == MeasureBeats.Strong && (possibleChords[0].priority == ChordPriority.Subdominant || getAlternativeChords(tonation.subdominant, tonation).Contains(possibleChords[0]))) ||
                (beatStrength == MeasureBeats.Strong && (possibleChords[0].priority == ChordPriority.Dominant || getAlternativeChords(tonation.dominant, tonation).Contains(possibleChords[0]))))
                chordsPriorities.Add(2 * (int)possibleChords[0].priority);
            else
                chordsPriorities.Add((int)possibleChords[0].priority);
            for (int i = 1; i < possibleChords.Count; i++)
            {
                if ((beatStrength == MeasureBeats.Begin && (possibleChords[i].priority == ChordPriority.Tonic || getAlternativeChords(tonation.tonic, tonation).Contains(possibleChords[i]))) ||
                (beatStrength == MeasureBeats.Strong && (possibleChords[i].priority == ChordPriority.Subdominant || getAlternativeChords(tonation.subdominant, tonation).Contains(possibleChords[i]))) ||
                (beatStrength == MeasureBeats.Strong && (possibleChords[i].priority == ChordPriority.Dominant || getAlternativeChords(tonation.dominant, tonation).Contains(possibleChords[i]))))
                    chordsPriorities.Add(chordsPriorities[i - 1] + 2 * (int)possibleChords[i].priority);
                else
                    chordsPriorities.Add(chordsPriorities[i - 1] + (int)possibleChords[i].priority);
            }
            int index = random.Next(chordsPriorities.Last());
            index = chordsPriorities.First(x => x >= index);
            index = chordsPriorities.IndexOf(index);
            return new TonationChord(possibleChords[index]);
        }
    }
}
