using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class HarmonySearchParams
    {
        public int hms;
        public double hmcr; // harmony memory considering rate
        public double par; // pitch adjusting rate
        public int delta; // the amount between two neighboring values in discrete candidate set
        public int maxIterations;
        public int bestTrackUnchanged;
        public int maxExecutionTimeInSeconds;

        public HarmonySearchParams(int hms, double hmcr, double par, int delta, int maxIter, int bestTrack, int maxTime)
        {
            this.hms = hms;
            this.hmcr = hmcr;
            this.par = par;
            this.delta = delta;
            this.maxIterations = maxIter;
            this.bestTrackUnchanged = bestTrack;
            this.maxExecutionTimeInSeconds = maxTime;
        }
    }
}
