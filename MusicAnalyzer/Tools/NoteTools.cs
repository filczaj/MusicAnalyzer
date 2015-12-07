using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;

namespace MusicAnalyzer.Tools
{
    class NoteTools
    {
        private String basicNotesFile = ""; //MusicAnalyzer.MainWindow.configDirectory + "\\basicPitches.txt";
        private String majorScaleSeqFile = ""; //MusicAnalyzer.MainWindow.configDirectory + "\\majorSequence.txt";
        private String minorScaleSeqFile = ""; //MusicAnalyzer.MainWindow.configDirectory + "\\minorSequence.txt";
        public Dictionary<String, int> notesSequence;
        public List<int> majorScaleSeq, minorScaleSeq;

        public NoteTools(){
            readNotesSequences();
            setScaleSequences();
        }

        public List<Note> setNotesDuration(List<Note> notesList)
        {
            for (int i=0; i<notesList.Count-1; i++){
                notesList[i].endTime = notesList[i + 1].startTime;
                notesList[i].duration = notesList[i].endTime - notesList[i].startTime;
            }
            notesList[notesList.Count - 1].duration = AVGNoteDuration(notesList);
            notesList[notesList.Count - 1].endTime = notesList[notesList.Count - 1].startTime + notesList[notesList.Count - 1].duration;
            return notesList;
        }

        public int AVGNoteDuration(List<Note> allNotes)
        {
            int avgDuration = 0;
            for (int i = 1; i < allNotes.Count - 1; i++)
            {
                avgDuration += allNotes[i].duration;
            }
            return avgDuration / allNotes.Count - 2;
        }

        public int getNotesDiff(Note a, Note b){
            int diff = getBasicNotesDiff(a, b);
            diff = Math.Min(diff, 12 - diff);
            int octaves = Math.Abs(getNoteOctave(a) - getNoteOctave(b));
            return diff + (octaves *12);
        }

        public int getBasicNotesDiff(Note a, Note b)
        {
            int diff = 0;
            return diff;
        }

        public int getNoteOctave(Note note){
            return Convert.ToInt32(note.note[note.note.Length - 1]);
        }

        private void readNotesSequences()
        {
            Array lines = IOTools.ReadFrom(basicNotesFile).ToArray<String>();
            notesSequence = new Dictionary<string, int>();
            foreach (String line in lines)
            {
                notesSequence[line.Split(' ')[1].Trim()] = Int32.Parse(line.Split(' ')[0].Trim());
            }
        }

        private void setScaleSequences()
        {
            Array lines = IOTools.ReadFrom(majorScaleSeqFile).ToArray<String>();
            majorScaleSeq = new List<int>();
            foreach (String line in lines)
            {
                majorScaleSeq.Add(Int32.Parse(line.Trim()));
            }
            lines = IOTools.ReadFrom(minorScaleSeqFile).ToArray<String>();
            minorScaleSeq = new List<int>();
            foreach (String line in lines)
            {
                minorScaleSeq.Add(Int32.Parse(line.Trim()));
            }
        }
    }
}

