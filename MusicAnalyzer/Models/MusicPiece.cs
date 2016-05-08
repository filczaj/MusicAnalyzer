using MusicAnalyzer.Tools;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Analyzer;
using PSAMControlLibrary;

namespace MusicAnalyzer.Models
{
    public class MusicPiece
    {
        MusicIntelligence musicIntelligence;
        List<Tonation> tonations;
        public Sequence sequence;
        NotesList notesList;
        MidiTools midiTools;
        int trackCount;
        SortedList<int, TonationChord> orderedNoteChords; // notes played at the specified moment, kept together create a chord
        List<Metrum> meterChanges;
        Dictionary<int, int> tracksProjection; // sorted trackID -> viewID
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

        public void setRightNotesAndTonations()
        {
            tonations = musicIntelligence.setRightTonation(tonations, notesList, midiTools);
            if (tonations.Count > 0)
                musicIntelligence.setRightNoteSigns(tonations, ref notesList, midiTools);
        }

        public void setNotesRythmicValues()
        {
            musicIntelligence.setNotesRythm(ref notesList, midiTools, Division);
        }

        public void initTrack(int vID, int tID)
        {
            composedTrack = new ComposedTrack(vID, tID);
        }

        public void completeNotesInfo()
        {
            orderedNoteChords = musicIntelligence.createOrderedChords(notesList, midiTools, tonations);
            musicIntelligence.setChordTypes(orderedNoteChords, tonations, midiTools);
            musicIntelligence.initMeasureBeats(meterChanges);
            musicIntelligence.setBeatStrength(ref orderedNoteChords, meterChanges, midiTools, Division);
            inputTrack = new ComposedTrack(orderedNoteChords);
        }

        public void fillScoreViewer(PSAMWPFControlLibrary.IncipitViewerWPF view, Track track, int index)
        {
            view.ClearMusicalIncipit();
            PSAMControlLibrary.Key key;
            if (isInputFileCorrect())
                key = new PSAMControlLibrary.Key(midiTools.getTonationFifths(midiTools.getCurrentTonation(tonations, 0)));
            else
                key = new PSAMControlLibrary.Key(0);
            view.ClearMusicalIncipit();
            Clef c = musicIntelligence.setTrackClef(notesList.Where(x => tracksProjection[x.trackID] == index).ToList());
            view.AddMusicalSymbol(c);
            Note last = null;
            bool barAdded = false;
            List<Note> trackNotes = notesList.Where(x => tracksProjection[x.trackID] == index).ToList();
            foreach (Note n in trackNotes)
            {
                PSAMControlLibrary.Note nView = new PSAMControlLibrary.Note(n.basicNote[0].ToString(), 
                    midiTools.getSharpOrFlat(n), midiTools.getNoteOctave(n), 
                    n.rythmicValue, NoteStemDirection.Down, NoteTieType.None, 
                    new List<NoteBeamType>() { NoteBeamType.Single });
                if (last != null && musicIntelligence.isBairlineNow(last.startTime + (int)(4 * Division / midiTools.ProperDurationScale(last, Division)), meterChanges, Division, midiTools))
                {
                    view.AddMusicalSymbol(new Barline());
                    barAdded = true;
                    TimeSignature ts = musicIntelligence.isTimeSignatureNow(n.startTime, meterChanges);
                    if (ts != null)
                        view.AddMusicalSymbol(ts);
                    Key tonationKey = musicIntelligence.isTonationChangeNow(n.startTime, tonations, midiTools);
                    if (tonationKey != null)
                        view.AddMusicalSymbol(tonationKey);
                }
                Rest rest = musicIntelligence.isRestNow(last, n, Division, midiTools);
                if (rest != null)
                    view.AddMusicalSymbol(rest);
                if (!barAdded && musicIntelligence.isBairlineNow(n.startTime, meterChanges, Division, midiTools))
                {
                    view.AddMusicalSymbol(new Barline());
                    TimeSignature ts = musicIntelligence.isTimeSignatureNow(n.startTime, meterChanges);
                    if (ts != null)
                        view.AddMusicalSymbol(ts);
                    Key tonationKey = musicIntelligence.isTonationChangeNow(n.startTime, tonations, midiTools);
                    if (tonationKey != null)
                        view.AddMusicalSymbol(tonationKey);
                }
                view.AddMusicalSymbol(nView);
                last = n;
                barAdded = false;
            }
        }

        public void findBestChords() // at composedTrack
        {
            HarmonySearch harmonySearch = new HarmonySearch(inputTrack, musicIntelligence);
            harmonySearch.generateInitialMemorySet();
            harmonySearch.runHarmonySearchLoop();
            composedTrack = harmonySearch.getBestTrack();
        }

        public void fillTrackNotes(ref Track addedTrack)
        {
            // create MidiTrack from ChordsList - to do !!!!!!!!!
            // create Notes and add to notesList with trackID
        }

        public bool isInputFileCorrect()
        {
            if (tonations.Count == 0)
                return false;
            if (notesList.Count() == 0)
                return false;
            //if (meterChanges.Count == 0) return false;
            return true;
        }
    }
}
