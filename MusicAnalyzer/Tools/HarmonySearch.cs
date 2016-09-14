using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using System.Diagnostics;
using MusicAnalyzer.Tools;
using System.ComponentModel;

namespace MusicAnalyzer.Tools
{
    public class HarmonySearch
    {
        string configDirectory = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.FullName + "\\ConfigFiles";
        List<string> bestTrackHistory;

        readonly int hms = 50;
        readonly double hmcr = 0.7; // harmony memory considering rate
        readonly double par = 0.7; // pitch adjusting rate
        readonly int delta = 1; // the amount between two neighboring values in discrete candidate set

        readonly int maxIterations = 15000;
        readonly int bestTrackUnchanged = 5000;
        readonly int maxExecutionTimeInSeconds = 120;

        int it = 0;
        int bestTrackChangeCounter = 0;
        bool isFromMemory = false;
        Stopwatch stopwatch;
        Random random = new Random();

        List<ComposedTrack> harmonyMemory; // memory of compositions with harmony match value as a key
        ComposedTrack inputTrack;
        MusicIntelligence musicAI;
        MusicPiece musicPiece;

        public HarmonySearch(ComposedTrack input, MusicPiece musicPiece, MusicIntelligence musicIntelligence)
        {
            this.inputTrack = input;
            this.musicAI = musicIntelligence;
            this.harmonyMemory = new List<ComposedTrack>();
            this.bestTrackHistory = new List<string>();
            this.musicPiece = musicPiece;
        }

        public void runHarmonySearchLoop(BackgroundWorker composeWorker)
        {
            stopwatch = Stopwatch.StartNew();
            while (!isStopCriteriaReached())
            {
                it++;
                ComposedTrack newTrack = getNewTrack();
                improviseOnTrack(ref newTrack, inputTrack);
                updateMemoryWithTrack(newTrack);
                if (it % 10 == 1)
                {
                    bestTrackHistory.Add("Epoch: " + it.ToString() + "; match = " + getBestTrack().harmonyMatch.ToString());
                }
                composeWorker.ReportProgress(100 * it / maxIterations);
            }
            MidiTools.genericListSerizliator<string>(bestTrackHistory, configDirectory + "\\bestTrackHistory.txt");
            MidiTools.genericListSerizliator<Chord>(harmonyMemory[0].noteChords.Values.ToList<Chord>(), configDirectory + "\\composedChords.txt");
            stopwatch.Stop();
        }

        ComposedTrack getNewTrack()
        {
            ComposedTrack newTrack;
            if (random.NextDouble() > hmcr)
            {
                newTrack = musicAI.createTrackFromScratch(inputTrack, musicPiece);
                newTrack.setHarmonyMatch(inputTrack, musicAI, musicPiece);
                isFromMemory = false;
            }
            else
            {
                int newTrackId = 0;
                while (newTrackId == 0)
                    newTrackId = random.Next(hms);
                newTrack = harmonyMemory[newTrackId];
                isFromMemory = true;
            }
            return newTrack;
        }

        void improviseOnTrack(ref ComposedTrack track, ComposedTrack inputTrack)
        {
            if (isFromMemory && random.NextDouble() <= par)
            {
                musicAI.changeRandomChords(ref track, inputTrack, delta);
                track.setHarmonyMatch(inputTrack, musicAI, musicPiece);
            }
        }

        void updateMemoryWithTrack(ComposedTrack track)
        {
            if (track.harmonyMatch < harmonyMemory[0].harmonyMatch)
                bestTrackChangeCounter = 0;
            else
                bestTrackChangeCounter++;
            if (!isFromMemory)
                if (track.harmonyMatch < harmonyMemory[hms - 1].harmonyMatch)
                {
                    harmonyMemory.RemoveAt(hms - 1);
                    harmonyMemory.Add(track);
                }
            harmonyMemory.Sort((x, y) => x.harmonyMatch.CompareTo(y.harmonyMatch));
        }

        bool isStopCriteriaReached()
        {
            if (bestTrackChangeCounter >= bestTrackUnchanged || it >= maxIterations || stopwatch.ElapsedMilliseconds > (maxExecutionTimeInSeconds * 1000))
                return true;
            else
                return false;
        }

        public void generateInitialMemorySet(string dir)
        {
            ComposedTrack newTrack;
            for (int i = 0; i < hms; i++)
            {
                newTrack = musicAI.createTrackFromScratch(inputTrack, musicPiece);
                newTrack.setHarmonyMatch(inputTrack, musicAI, musicPiece);
                harmonyMemory.Add(newTrack);
            }
            harmonyMemory.Sort((x, y) => x.harmonyMatch.CompareTo(y.harmonyMatch));
#if DEBUG
            List<int> listwa = new List<int>();
            foreach(ComposedTrack track in harmonyMemory)
                listwa.Add(track.harmonyMatch);
            MidiTools.genericListSerizliator<int>(listwa, dir + "\\initalSetMatch.txt");
#endif
        }

        public ComposedTrack getBestTrack()
        {
            if (harmonyMemory != null && harmonyMemory.Count > 0){
                harmonyMemory.Sort((x, y) => x.harmonyMatch.CompareTo(y.harmonyMatch));
                return harmonyMemory[0];
            }                
            else
                return null;
        }
    }
}
