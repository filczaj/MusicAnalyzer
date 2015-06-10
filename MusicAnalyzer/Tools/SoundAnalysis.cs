using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Math;
using MusicAnalyzer.Models;
using MusicAnalyzer.Analyzer;

namespace MusicAnalyzer.Tools
{
    class SoundAnalysis
    {
        Complex[] fourierData;
        WAVReader wavReader;
        public SoundAnalysis(WAVReader wavReader)
        {
            this.wavReader = wavReader;
        }
        public void runDFT(int starter)
        {
            fourierData = new Complex[wavReader.sampleSize];
            for (int i = 0; i < wavReader.sampleSize; i++)
            {
                fourierData[i].Re = wavReader.channelBuffer[starter + i];
                fourierData[i].Im = 0.0;
            }
            FourierTransform.DFT(fourierData, FourierTransform.Direction.Forward);
        }

        public int getBasicHarmonic()
        {
            double[] data = new double[fourierData.Length/2];
            for (int i = 0; i < fourierData.Length/2; i++)
            {
                if (i < 50)
                    data[i] = 0;
                else
                    data[i] = Math.Sqrt(Math.Pow(fourierData[i].Re, 2) + Math.Pow(fourierData[i].Im, 2));
            }
            return maxIndex(data);
        }

        private int maxIndex(double[] data)
        {
            int maxIndex = 0;
            for (int i = 0; i < data.Length; i++)
                if (data[i] > data[maxIndex]) maxIndex = i;
            return maxIndex;
        }

        public Complex[] getFourierData()
        {
            return fourierData;
        }
    }
}
