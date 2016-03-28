using MusicAnalyzer.Tools;
using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Analyzer;

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

        public void completeNotesInfo()
        {
            this.tonations = musicIntelligence.setRightTonation(tonations, notesList, midiTools);
            orderedNoteChords = musicIntelligence.createOrderedChords(notesList, midiTools, tonations);
            musicIntelligence.setChordTypes(orderedNoteChords, tonations, midiTools);
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
