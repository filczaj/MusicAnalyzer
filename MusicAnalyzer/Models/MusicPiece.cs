using MusicAnalyzer.Tools;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSAMControlLibrary;
using System.Windows.Controls;
using System.ComponentModel;
using System.IO;

namespace MusicAnalyzer.Models
{
    public class MusicPiece
    {
        readonly int defaultNoteVelocity = 80;
        MusicIntelligence musicIntelligence;
        public List<Tonation> tonations;
        public Sequence sequence;
        public NotesList notesList;
        MidiTools midiTools;
        int trackCount;
        SortedList<int, TonationChord> orderedNoteChords; // notes played at the specified moment, kept together create a chord
        public List<Metrum> meterChanges;
        public Dictionary<int, int> tracksProjection; // sorted trackID -> viewID
        public int Division { get; set; }
        ComposedTrack composedTrack;
        ComposedTrack inputTrack;

        public MusicPiece(Sequence seq, string configDir)
        {
            this.sequence = seq;
            this.Division = seq.Division;
            this.midiTools = new MidiTools(configDir);
            this.trackCount = this.sequence.Count;
            this.musicIntelligence = new MusicIntelligence();
        }

        public void initTools()
        {
            this.midiTools.initTools();
        }
        public void initNotes(){
            this.notesList = midiTools.decodeNotes(sequence) as NotesList;
            this.tracksProjection = midiTools.projectTracks(notesList);
        }

        public void initTonations(){
            this.tonations = midiTools.grabTonations(sequence);
        }
            
        public void initMetrum(){
            this.meterChanges = midiTools.findMeter(sequence);
        }

        public void setRightNotesAndTonations(IEnumerable<Note> notes)
        {
            if (notes == null)
                notes = notesList;
            if (meterChanges != null && meterChanges.Count > 0)
                tonations = musicIntelligence.setRightTonation(tonations, meterChanges, Division, notes, midiTools);
            if (tonations.Count > 0)
                musicIntelligence.setRightNoteSigns(tonations, ref notes);
        }

        public void setNotesRythmicValues(int trackID)
        {
            musicIntelligence.setNotesRythm(ref notesList, midiTools, Division, trackID);
        }

        public void initTrack(int vID, int tID)
        {
            composedTrack = new ComposedTrack(vID, tID);
        }

        public void completeNotesInfo()
        {
            orderedNoteChords = musicIntelligence.createOrderedChords(notesList, tonations);
            musicIntelligence.setChordsDuration(orderedNoteChords, Division, meterChanges);
            musicIntelligence.setChordTypes(orderedNoteChords, tonations, midiTools);
            musicIntelligence.initMeasureBeats(meterChanges);
            musicIntelligence.setBeatStrength(ref orderedNoteChords, meterChanges, midiTools, Division);
            inputTrack = new ComposedTrack(orderedNoteChords);
        }

        public void fillScoreViewer(PSAMWPFControlLibrary.IncipitViewerWPF view, int index)
        {
            view.ClearMusicalIncipit();
            Clef c = musicIntelligence.setTrackClef(notesList.Where(x => tracksProjection[x.trackID] == index).ToList());
            view.AddMusicalSymbol(c);
            TimeSignature ts = musicIntelligence.isTimeSignatureNow(0, meterChanges);
            if (ts != null)
                view.AddMusicalSymbol(ts);
            Key tonationKey = musicIntelligence.isTonationChangeNow(0, tonations);
            if (tonationKey != null)
                view.AddMusicalSymbol(tonationKey);
            Note last = null;
            bool barAdded = false, timeSignAdded = true, keySignAdded = true;
            List<Note> trackNotes = notesList.Where(x => tracksProjection[x.trackID] == index).ToList();
            foreach (Note n in trackNotes)
            {
                PSAMControlLibrary.Note nView = new PSAMControlLibrary.Note(n.basicNote[0].ToString(), 
                    MidiTools.getSharpOrFlat(n), midiTools.getNoteOctave(n), 
                    n.rythmicValue, midiTools.getNoteStemDirection(n, last, c.TypeOfClef), NoteTieType.None, 
                    new List<NoteBeamType>() { NoteBeamType.Single });
                if (n.noteExtension == 1.5)
                    nView.NumberOfDots = 1;
                if (n.noteExtension == 1.75)
                    nView.NumberOfDots = 2;
                if (last != null && last.startTime == n.startTime)
                    nView.IsChordElement = true;
                if (last != null && musicIntelligence.isBarlineNow(last.startTime + (int)(4 * Division / midiTools.ProperDurationAndExtension(last, Division)), meterChanges, Division, midiTools))
                {
                    view.AddMusicalSymbol(new Barline());
                    barAdded = true;
                    ts = musicIntelligence.isTimeSignatureNow(n.startTime, meterChanges);
                    if (ts != null && !timeSignAdded)
                    {
                        view.AddMusicalSymbol(ts);
                        timeSignAdded = true;
                    }
                    tonationKey = musicIntelligence.isTonationChangeNow(n.startTime, tonations);
                    if (tonationKey != null && !keySignAdded)
                    {
                        view.AddMusicalSymbol(tonationKey);
                        keySignAdded = true;
                    }
                }
                Rest rest = musicIntelligence.isRestNow(last, n, Division, midiTools);
                if (rest != null)
                    view.AddMusicalSymbol(rest);
                if (musicIntelligence.isBarlineNow(n.startTime, meterChanges, Division, midiTools))
                {
                    if (!barAdded && n.startTime > 0)
                    {
                        view.AddMusicalSymbol(new Barline());
                        barAdded = true;
                    }
                    ts = musicIntelligence.isTimeSignatureNow(n.startTime, meterChanges);
                    if (ts != null && !timeSignAdded)
                    {
                        view.AddMusicalSymbol(ts);
                        timeSignAdded = true;
                    }
                    tonationKey = musicIntelligence.isTonationChangeNow(n.startTime, tonations);
                    if (tonationKey != null && !keySignAdded)
                    {
                        view.AddMusicalSymbol(tonationKey);
                        keySignAdded = true;
                    }                        
                }
                else
                {
                    timeSignAdded = false;
                    keySignAdded = false;
                    barAdded = false;
                }
                view.AddMusicalSymbol(nView);
                last = n;
            }
            view.AddMusicalSymbol(new Barline());
        }

        public void findBestChords(BackgroundWorker composeWorker)
        {
            HarmonySearch harmonySearch = new HarmonySearch(inputTrack, this, musicIntelligence);
            harmonySearch.generateInitialMemorySet(midiTools.configDirectory);
            harmonySearch.runHarmonySearchLoop(composeWorker);
            composedTrack = harmonySearch.getBestTrack();
        }

        public void fillNewTrackNotes()
        {
            composedTrack.trackID = this.tracksProjection.Keys.Max() + 1;
            composedTrack.viewID = this.tracksProjection.Values.Max() + 1;
            this.tracksProjection.Add(composedTrack.trackID, composedTrack.viewID);
            foreach (int timeIndex in composedTrack.noteChords.Keys)
            {
                int lastNoteIndex = 0;
                foreach (int noteIndex in composedTrack.noteChords[timeIndex].chordNotes)
                {
                    Note n = null;
                    if (noteIndex + 15 < lastNoteIndex)
                        n = new Note(noteIndex + 27, defaultNoteVelocity, timeIndex, (int)(composedTrack.noteChords[timeIndex].duration * 0.9), composedTrack.trackID, true);
                    else
                        n = new Note(noteIndex + 15, defaultNoteVelocity, timeIndex, (int)(composedTrack.noteChords[timeIndex].duration * 0.9), composedTrack.trackID, true);
                    lastNoteIndex = n.noteID;
                    midiTools.fillNoteData(n);
                    notesList.Add(n);
                }
            }
            setNotesRythmicValues(composedTrack.trackID);
            setRightNotesAndTonations(notesList.Where(x => x.trackID == composedTrack.trackID));

            midiTools.AddMidiTrack(composedTrack, this);
#if DEBUG
            MidiTools.genericListSerizliator<Note>(notesList.Where(x => x.trackID == composedTrack.trackID).ToList<Note>(), midiTools.configDirectory + "\\allComposedNotes.txt");
#endif
        }

        public bool isInputFileCorrect()
        {
            if (tonations.Count == 0)
                return false;
            if (notesList.Count() == 0)
                return false;
            if (meterChanges.Count == 0) 
                return false;
            return true;
        }

        public bool isConfigDirCorrect()
        {
            if (!Directory.Exists(midiTools.configDirectory))
                return false;
            if (!midiTools.areConfigFilesCorrect())
                return false;
            return true;
        }
    }
}
