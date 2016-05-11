using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using System.Diagnostics;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Tools
{
    public class HarmonySearch
    {
        readonly int hms = 30;
        readonly double hmcr = 0.9; // harmony memory considering rate
        readonly double par = 0.3; // pitch adjusting rate
        readonly int delta = 1; // the amount between two neighboring values in discrete candidate set

        readonly int maxIterations = 1000000;
        readonly int bestTrackUnchanged = 10000;
        readonly int maxExecutionTimeInSeconds = 300;

        int it = 0;
        int bestTrackChangeCounter = 0;
        bool isFromMemory = false;
        Stopwatch stopwatch;
        Random random = new Random();

        SortedList<int, ComposedTrack> harmonyMemory; // memory of compositions with harmony match value as a key
        ComposedTrack inputTrack;
        MusicIntelligence musicAI;

        public HarmonySearch(ComposedTrack input, MusicIntelligence musicIntelligence)
        {
            this.inputTrack = input;
            this.musicAI = musicIntelligence;
        }

        public void runHarmonySearchLoop()
        {
            stopwatch = Stopwatch.StartNew();
            while (!isStopCriteriaReached())
            {
                it++;
                ComposedTrack newTrack = getNewTrack();
                improviseOnTrack(ref newTrack, inputTrack);
                updateMemoryWithTrack(newTrack);
            }
            stopwatch.Stop();
        }

        ComposedTrack getNewTrack()
        {
            ComposedTrack newTrack;
            if (random.NextDouble() <= hmcr)
            {
                newTrack = musicAI.createTrackFromScratch(inputTrack);
                newTrack.setHarmonyMatch(inputTrack, musicAI);
                isFromMemory = false;
            }
            else
            {
                newTrack = harmonyMemory[random.Next(hms)];
                isFromMemory = true;
            }
            return newTrack;
        }

        void improviseOnTrack(ref ComposedTrack track, ComposedTrack inputTrack)
        {
            if (isFromMemory && random.NextDouble() <= par)
            {
                musicAI.changeRandomChords(ref track, inputTrack, delta);
                track.setHarmonyMatch(inputTrack, musicAI);
                isFromMemory = false;
            }
        }
        void updateMemoryWithTrack(ComposedTrack track)
        {
            if (track.harmonyMatch < harmonyMemory[0].harmonyMatch)
                bestTrackChangeCounter = 0;
            else
                bestTrackChangeCounter++;
            if (track.harmonyMatch < harmonyMemory[hms - 1].harmonyMatch)
            {
                harmonyMemory.RemoveAt(hms - 1);
                harmonyMemory.Add(track.harmonyMatch, track);
            }
        }

        bool isStopCriteriaReached()
        {
            if (bestTrackChangeCounter >= bestTrackUnchanged || it >= maxIterations || stopwatch.ElapsedMilliseconds > maxExecutionTimeInSeconds * 1000)
                return true;
            else
                return false;
        }

        public void generateInitialMemorySet()
        {
            for (int i = 0; i < hms; i++)
            {
                ComposedTrack newTrack = musicAI.createTrackFromScratch(inputTrack);
                newTrack.setHarmonyMatch(inputTrack, musicAI);
                harmonyMemory.Add(newTrack.harmonyMatch, newTrack);
            }
        }

        public ComposedTrack getBestTrack()
        {
            if (harmonyMemory != null && harmonyMemory.Count > 0)
                return harmonyMemory[0];
            else
                return null;
        }
    }
}
