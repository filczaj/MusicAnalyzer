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
        HarmonySearchParams HSparams;

        int it = 0;
        int bestTrackChangeCounter = 0;
        bool isFromMemory = false;
        Stopwatch stopwatch;
        Random random = new Random();

        List<ComposedTrack> harmonyMemory; // memory of compositions tracks
        ComposedTrack inputTrack;
        MusicIntelligence musicAI;
        MusicPiece musicPiece;

        public HarmonySearch(ComposedTrack input, MusicPiece musicPiece, MusicIntelligence musicIntelligence, HarmonySearchParams HSParams)
        {
            this.inputTrack = input;
            this.musicAI = musicIntelligence;
            this.harmonyMemory = new List<ComposedTrack>();
            this.bestTrackHistory = new List<string>();
            this.musicPiece = musicPiece;
            this.HSparams = HSParams;
        }

        public void runHarmonySearchLoop(BackgroundWorker composeWorker)
        {
            List<string> bestTrackHistory2 = new List<string>();
            stopwatch = Stopwatch.StartNew();
            while (!isStopCriteriaReached())
            {
                it++;
                ComposedTrack newTrack = getNewTrack();
                improviseOnTrack(ref newTrack, inputTrack);
                updateMemoryWithTrack(newTrack);
#if DEBUG
                if (it % 10 == 1)
                {
                    bestTrackHistory.Add(it.ToString());
                    bestTrackHistory2.Add(getBestTrack().harmonyMatch.ToString());
                }
#endif
                composeWorker.ReportProgress(100 * it / HSparams.maxIterations);
            }
#if DEBUG
            MidiTools.genericListSerizliator<string>(bestTrackHistory, configDirectory + "\\bestTrackHistory.txt");
            MidiTools.genericListSerizliator<string>(bestTrackHistory2, configDirectory + "\\bestTrackHistory2.txt");
            MidiTools.genericListSerizliator<Chord>(harmonyMemory[0].noteChords.Values.ToList<Chord>(), configDirectory + "\\composedChords.txt");
#endif
            stopwatch.Stop();
        }

        ComposedTrack getNewTrack()
        {
            ComposedTrack newTrack;
            if (random.NextDouble() > HSparams.hmcr)
            {
                newTrack = musicAI.createTrackFromScratch(inputTrack, musicPiece);
                newTrack.setHarmonyMatch(inputTrack, musicAI, musicPiece);
                isFromMemory = false;
            }
            else
            {
                int newTrackId = 0;
                while (newTrackId == 0)
                    newTrackId = random.Next(HSparams.hms);
                newTrack = harmonyMemory[newTrackId];
                isFromMemory = true;
            }
            return newTrack;
        }

        void improviseOnTrack(ref ComposedTrack track, ComposedTrack inputTrack)
        {
            if (isFromMemory && random.NextDouble() <= HSparams.par)
            {
                musicAI.changeRandomChords(ref track, inputTrack, HSparams.delta);
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
                if (track.harmonyMatch < harmonyMemory[HSparams.hms - 1].harmonyMatch)
                {
                    harmonyMemory.RemoveAt(HSparams.hms - 1);
                    harmonyMemory.Add(track);
                }
            harmonyMemory.Sort((x, y) => x.harmonyMatch.CompareTo(y.harmonyMatch));
        }

        bool isStopCriteriaReached()
        {
            if (bestTrackChangeCounter >= HSparams.bestTrackUnchanged || it >= HSparams.maxIterations || stopwatch.ElapsedMilliseconds > (HSparams.maxExecutionTimeInSeconds * 1000))
                return true;
            else
                return false;
        }

        public void generateInitialMemorySet(string dir)
        {
            ComposedTrack newTrack;
            for (int i = 0; i < HSparams.hms; i++)
            {
                newTrack = musicAI.createTrackFromScratch(inputTrack, musicPiece);
                newTrack.setHarmonyMatch(inputTrack, musicAI, musicPiece);
                harmonyMemory.Add(newTrack);
            }
            harmonyMemory.Sort((x, y) => x.harmonyMatch.CompareTo(y.harmonyMatch));
#if DEBUG
            List<double> listwa = new List<double>();
            foreach(ComposedTrack track in harmonyMemory)
                listwa.Add(track.harmonyMatch);
            MidiTools.genericListSerizliator<double>(listwa, dir + "\\initalSetMatch.txt");
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
