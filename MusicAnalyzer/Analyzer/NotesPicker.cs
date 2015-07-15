using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Analyzer
{
    class NotesPicker
    {
        short[] data;
        int bitrate;
        List<int> notePeaks;
        List<MyFrame> resultNotes;
        WAVReader wavReader;
        SoundAnalysis soundAnalysis;
        NoteTools noteTools;

        public NotesPicker(short[] inputData, int bitrate, WAVReader wavreader)
        {
            this.data = inputData;
            this.bitrate = bitrate;
            this.wavReader = wavreader;
            this.soundAnalysis = new SoundAnalysis(wavreader);
            this.noteTools = new NoteTools();
        }

        public List<int> findAllNotes(){
            int delta = bitrate / 10;
            notePeaks = new List<int>();
            double newAVG, lastAVG = 0;
            for (int i = delta; i < data.Length - delta; i += delta)
            {
                newAVG = getRangeAVG(i, i + delta, 100);
                if (newAVG > 1.5 * lastAVG) notePeaks.Add(getRangeMaxIndex(i, i+delta));
                lastAVG = newAVG;
            }
            return notePeaks;
        }

        public List<MyFrame> findAllPitchChanges()
        {
            resultNotes= new List<MyFrame>();
            int delta = bitrate / 8;
            double tempFreq, lastFreq = -1.0;
            for (int i = delta; i < data.Length - wavReader.sampleSize; i += delta)
            {
                soundAnalysis.runDFT(i);
                tempFreq = soundAnalysis.getBasicHarmonic();
                if (Math.Abs(lastFreq - tempFreq) < 4) {
                    resultNotes.Add(new MyFrame(2.0*tempFreq, i*1000/bitrate));
                   lastFreq = tempFreq;
                }
            }
            return resultNotes;
        }

        public List<Note> removeRepeatedNotes(List<Note> allNotes)
        {
            String tempNote = "X0";
            double tempFreq = 0.0;
            for (int i = 0; i < allNotes.Count;)
            {
                if ((allNotes[i].note[0] == tempNote[0]) || (Math.Abs(allNotes[i].freq - tempFreq) <= 3)) allNotes.RemoveAt(i);
                else
                {
                    if (allNotes[i].freq == 0) allNotes.RemoveAt(i); else
                    {   
                        tempNote = allNotes[i].note;
                        tempFreq = allNotes[i].freq;
                        i++;
                    }
                }
            }
            allNotes = noteTools.setNotesDuration(allNotes);
            for (int i = 0; i < allNotes.Count;)
            {
                if (allNotes[i].duration <= 250) allNotes.RemoveAt(i); else i++;
            }
            return allNotes;
        }    

        private double getRangeAVG(int starter, int end, int count){
            double[] probes = new double[count];
            int range = end - starter;
            Random r = new Random();
            for (int i = 0; i < count; i++)
            {
                probes[i] = Math.Abs(data[r.Next(range) + starter]);
            }
            return probes.Average();
        }

        private int getRangeMaxIndex(int starter, int end)
        {
            int index = starter;
            for (int i = starter; i < end; i++)
            {
                if (Math.Abs(data[i]) > Math.Abs(data[index])) index = i;
            }
            return index;
        }

        public List<MyFrame> getAllPitches()
        {
            resultNotes = new List<MyFrame>();
            for (int i = 0; i < notePeaks.Count; i++)
            {
                if (notePeaks[i] + (wavReader.bitrate / 4) < wavReader.channelBuffer.Length)
                {
                    soundAnalysis.runDFT(notePeaks[i] + (bitrate / 8));
                    MyFrame note = new MyFrame(2.0 * (double)soundAnalysis.getBasicHarmonic(), (1000 * notePeaks[i]) / bitrate);
                    if ((resultNotes.Count == 0) || (resultNotes[resultNotes.Count - 1].value != note.value))
                        resultNotes.Add(note);
                }
            }
            return resultNotes;
        }
    }
}
