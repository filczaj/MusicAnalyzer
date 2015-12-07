using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;

namespace MusicAnalyzer.Tools
{
    class FreqMapper
    {
        String pitchesFile = ""; //MusicAnalyzer.MainWindow.configDirectory + "\\pitches.txt";
        Array lines;
        List<String> notes;
        List<double> frequencies;

        public FreqMapper()
        {
            notes = new List<string>();
            frequencies = new List<double>();
            lines = IOTools.ReadFrom(pitchesFile).ToArray<String>();
            foreach (String line in lines)
            {
                notes.Add(line.Split('\t')[0]);
                frequencies.Add(double.Parse(line.Split('\t')[1].Replace('.', ',')));
            }
        }

        public String findPeach(double freq)
        {
            int bestIndex = findBest(freq, frequencies);
            return notes[bestIndex];
        }

        public Note getNote(double freq)
        {
            int bestIndex = findBest(freq, frequencies);
            Note n = new Note(frequencies[bestIndex], 0, 0, notes[bestIndex], getBasicNote(notes[bestIndex]));
            return n;
        }

        public Note getNote(double freq, int startTime, int endTime)
        {
            int bestIndex = findBest(freq, frequencies);
            Note n = new Note(frequencies[bestIndex], startTime, endTime, notes[bestIndex], getBasicNote(notes[bestIndex]));
            return n;
        }

        private int findBest(double freq, List<double> frequencies)
        {
            int index = 0;
            double minDiv = frequencies[0];
            foreach (double f in frequencies)
            {
                if (Math.Abs(freq - f) < minDiv)
                {
                    minDiv = Math.Abs(freq - f);
                    index = frequencies.IndexOf(f);
                }
            }
            return index;
        }

        String getBasicNote(double freq)
        {
            int bestIndex = findBest(freq, frequencies);
            String mainNote = notes[bestIndex];
            return mainNote.Substring(0, mainNote.Length - 2);
        }

        String getBasicNote(String note)
        {
            return note.Substring(0, note.Length - 2);
        }

        public List<Note> notesMapper(List<MyFrame> frameNotes)
        {
            List<Note> notes = new List<Note>();
            foreach (MyFrame f in frameNotes)
            {
                notes.Add(new Note(f.value, f.index, 0, getNote(f.value).note, getBasicNote(f.value)));
            }
            return notes;
        }

        public List<MyFrame> notesReverseMapper(List<Note> notes)
        {
            List<MyFrame> frames = new List<MyFrame>();
            foreach (Note f in notes)
            {
                frames.Add(new MyFrame(f.freq, f.startTime));
            }
            return frames;
        }
    }
}
