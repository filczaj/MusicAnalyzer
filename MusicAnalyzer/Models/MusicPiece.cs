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
    class MusicPiece
    {
        MusicIntelligence musicIntelligence;
        List<Tonation> tonations;
        Sequence sequence;
        NotesList notesList;
        MidiTools midiTools;
        int trackCount;
        SortedList<int, TonationChord> orderedNoteChords; // notes played at the specified moment, kept together create a chord
        List<MeterChange> meterChanges;
        int composedTrack;

        public MusicPiece(Sequence seq, string configDir) // to do! - wrzucić to do oddzielnego wątku!
        {
            this.sequence = seq;
            this.midiTools = new MidiTools(configDir);
            this.notesList = midiTools.decodeNotes(sequence) as NotesList;
            this.tonations = midiTools.grabTonations(sequence);
            this.trackCount = this.sequence.Count;
            this.meterChanges = midiTools.findMeter(seq);
            this.musicIntelligence = new MusicIntelligence();
            this.tonations = musicIntelligence.setRightTonation(tonations, notesList, midiTools);
        }

        public void completeNotesInfo()
        {
            orderedNoteChords = musicIntelligence.createOrderedChords(notesList, midiTools, tonations);
            musicIntelligence.setChordTypes(orderedNoteChords, tonations, midiTools);
        }

        public void findChordChanges() // at composedTrack
        {
            // tworzy tonikę na dźwięku - new Chord(n, getCurrentTonation(n.startTime);
        }
    }
}
