using AForge.Math;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Analyzer
{
    class Harmonics
    {
        List<double[]> harmonics;
        List<double> peaks;
        List<int> peakIndices;

        public Harmonics()
        {
            harmonics = new List<double[]>();
            peaks = new List<double>();
            peakIndices = new List<int>();
        }

        public int getJustBasicHarmonic(double[] data)
        {
            return maxIndex(data);
        }
        public double[] getNHarmonic(double[] data)
        {
            int index = maxIndex(data);
            int a=index-1;
            int b=index+1;
            peaks.Add(data[index]);
            peakIndices.Add(index);
            while (data[a] > data[index]/(double)5) a--;
            while (data[b] > data[index] / (double)5 && b < 511) b++;
            double[] period = new double[b-a-1];
            for (int i = 0; i < period.Length; i++)
            {
                period[i] = data[a + i+1];
                data[a + i +1] = 0.0;
            }
            harmonics.Add(period);
            return data;
        }

        public int getBestHarmonic()
        {
            int bestIndex = 0;
            for (int i=0; i< harmonics.Count; i++)
                if ((harmonics[i]).Length < (harmonics[bestIndex]).Length)
                    bestIndex = i;
            return peakIndices[bestIndex];
        }

        private int maxIndex(double[] data)
        {
            int maxIndex = 0;
            for (int i = 0; i < data.Length; i++)
                if (data[i] > data[maxIndex]) maxIndex = i;
            return maxIndex;
        }

        public IEnumerable<double> getHarmonicList()
        {
            for (int i = 0; i < peaks.Count; i++)
            {
                yield return peaks[i];
            }
        }

        public IEnumerable<int> getHarmonicIndices()
        {
            for (int i = 0; i < peakIndices.Count; i++)
            {
                yield return peakIndices[i];
            }
        }
    }
}
