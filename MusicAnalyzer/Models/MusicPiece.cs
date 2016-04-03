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
        int composedTrack;

        public MusicPiece(Sequence seq, string configDir)
        {
            this.sequence = seq;
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
            musicIntelligence.setRightNoteSigns(tonations, ref notesList, midiTools);
        }

        public void completeNotesInfo()
        {
            orderedNoteChords = musicIntelligence.createOrderedChords(notesList, midiTools, tonations);
            musicIntelligence.setChordTypes(orderedNoteChords, tonations, midiTools);
        }

        public void fillScoreViewer(List<PSAMWPFControlLibrary.IncipitViewerWPF> views)
        {
            PSAMControlLibrary.Key key;
            if (isInputFileCorrect())
                key = new PSAMControlLibrary.Key(midiTools.getTonationFifths(midiTools.getCurrentTonation(tonations, 0)));
            else
                key = new PSAMControlLibrary.Key(0);
            for (int i = 0; i < views.Count; i++)
            {
                views[i].ClearMusicalIncipit();
                Clef c = new Clef(ClefType.GClef, 2); // which clef : bass or violin????
                views[i].AddMusicalSymbol(c);
                views[i].AddMusicalSymbol(key);
                TimeSignature ts = new TimeSignature(TimeSignatureType.Numbers, Convert.ToUInt32(meterChanges[0].Numerator), Convert.ToUInt32(meterChanges[0].Denominator));
                views[i].AddMusicalSymbol(ts);
            }
            foreach (Note n in notesList)
            {
                PSAMControlLibrary.Note nView = new PSAMControlLibrary.Note(n.basicNote[0].ToString(), 
                    midiTools.getSharpOrFlat(n), midiTools.getNoteOctave(n), 
                    MusicalSymbolDuration.Quarter, NoteStemDirection.Down, NoteTieType.None, 
                    new List<NoteBeamType>() { NoteBeamType.Single });
                if (views.Count > n.trackID && n.trackID >= 0)
                    views[n.trackID].AddMusicalSymbol(nView);
            }
        }

        public void findChordChanges() // at composedTrack
        {
            // tworzy tonikę na dźwięku - new Chord(n, getCurrentTonation(n.startTime);
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
