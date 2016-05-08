using MusicAnalyzer.Analyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class ComposedTrack
    {
        public int trackID;
        public int viewID;
        public SortedList<int, TonationChord> noteChords; // chords in the track
        public int harmonyMatch;

        public ComposedTrack(int vID, int tID)
        {
            this.trackID = tID;
            this.viewID = vID;
            this.noteChords = new SortedList<int,TonationChord>();
        }

        public ComposedTrack(SortedList<int, TonationChord> chordsList)
        {
            this.noteChords = chordsList;
        }

        public ComposedTrack()
        {

        }

        public void setHarmonyMatch(ComposedTrack inputTrack, MusicIntelligence musicAI)
        {
            this.harmonyMatch = 0;
            for (int i = 0; i < this.noteChords.Keys.Count; i++)
            {
                foreach (int inputIndex in inputTrack.noteChords.Keys.Where(x => x >= this.noteChords.Keys[i] && x < this.noteChords.Keys[i + 1]))
                {
                    harmonyMatch += musicAI.penaltyMatchChords(this.noteChords[i], inputTrack.noteChords[inputIndex]);
                }
            }
        }
    }
}
