using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Tools;

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
            IEnumerable<int> timeIndices;
            for (int i = 0; i < this.noteChords.Keys.Count; i++)
            {
                timeIndices = inputTrack.noteChords.Keys.Where(x => x >= this.noteChords.Keys[i] && x < this.noteChords.Keys[i + 1]).ToArray();
                foreach (int inputIndex in timeIndices)
                {
                    harmonyMatch += (musicAI.penaltyMatchChords(this.noteChords[i], inputTrack.noteChords[inputIndex]) * inputTrack.noteChords[inputIndex].duration / 100);
                }
            }
        }
    }
}
