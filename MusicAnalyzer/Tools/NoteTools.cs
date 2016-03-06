using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;

namespace MusicAnalyzer.Tools
{
    public class NoteTools
    {
        public string configDirectory;
        private String basicNotesFile;
        private String majorScaleSeqFile; 
        private String minorScaleSeqFile;
        private String majorChordsFile;
        private String minorChordsFile;
        public Dictionary<int, string> basicNotesSequence;
        public List<int> majorScaleSeq, minorScaleSeq;

        public readonly int lowNoteID = 21; // A0
        public readonly int highNoteID = 109; // C8

        protected void initTools()
        {
            readNotesSequences();
            setScaleSequences();
        }
        public NoteTools(string configDir)
        {
            configDirectory = configDir;
            basicNotesFile = configDir + "\\basicPitches.txt";
            majorScaleSeqFile = configDir + "\\majorSequence.txt";
            minorScaleSeqFile = configDir + "\\minorSequence.txt";
            majorChordsFile = configDir + "\\majorChords.txt";
            minorChordsFile = configDir + "\\minorChords.txt";
        }

        #region Old
        public NoteTools(){
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

#endregion

        private void readNotesSequences()
        {
            Array lines = IOTools.ReadFrom(basicNotesFile).ToArray<String>();
            basicNotesSequence = new Dictionary<int, string>();
            foreach (String line in lines)
            {
                basicNotesSequence[Int32.Parse(line.Split(' ')[0].Trim())] = line.Split(' ')[1].Trim();
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

        public List<int> setMainScaleNotes(ChordMode mode)
        {
            if (mode == ChordMode.Major)
                return majorScaleSeq;
            else
                return minorScaleSeq;
        }

        public List<Chord> setMainChords(ChordMode mode)
        {
            String chordsFile;
            List<Chord> chords = new List<Chord>();
            if (mode == ChordMode.Major)
                chordsFile = majorChordsFile;
            else
                chordsFile = minorChordsFile;
            IEnumerable<String> lines = IOTools.ReadFrom(chordsFile);

            Chord chord = new Chord(mode, true, 0);
            chord.chordNotes = new List<int>();
            int[] asIntegers = lines.ElementAt(0).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new Chord(mode, true, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(1).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new Chord(mode, true, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(2).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            return chords;
        }

        public Note fillNoteData(Note n)
        {
            n.note = getNoteById(n.noteID);
            n.basicNote = getBasicNoteById(n.noteID);
            if (n.startTime > 0 && n.duration > 0)
                n.endTime = n.startTime + n.duration;
            return n;
        }

        public string getNoteById(int id)
        {
            int key = (id - 3) % 12;
            int octave = (id + 9) / 12;
            if (basicNotesSequence.ContainsKey(key))
                return basicNotesSequence[key] + octave.ToString();
            else
                return "";
        }

        public string getBasicNoteById(int id)
        {
            int key = (id - 3) % 12;
            if (basicNotesSequence.ContainsKey(key))
                return basicNotesSequence[key];
            else
                return "";
        }

        public void setNoteOff(int noteID, int endTime, NotesList allNotes)
        {
            Note n = allNotes.FirstOrDefault(x => (x.endTime == -1 && x.noteID == noteID));
            if (n != null && n.noteID == noteID)
            {
                n.endTime = endTime;
                n.duration = n.endTime - n.startTime;
            }
        }

        public Offset setOffset(Sanford.Multimedia.Key key)
        {
            Offset offset = Offset.CMajor;
            string s_key = Enum.GetName(typeof(Sanford.Multimedia.Key), key);
            try
            {
                offset = (Offset)Enum.Parse(typeof(Offset), s_key);
            }
            catch (InvalidCastException ex)
            {

            }
            return offset;
        }

        public Sanford.Multimedia.Key setKeyOnOffsetAndMode(int offset, ChordMode mode)
        {
            string[] offsetNames;
            if (mode == ChordMode.Major)
                offsetNames = Enum.GetNames(typeof(Offset)).Where(x => x.Contains("Major") && ((int)(Offset)(Enum.Parse(typeof(Offset), x))) == offset).ToArray();
            else
                offsetNames = Enum.GetNames(typeof(Offset)).Where(x => x.Contains("Minor") && ((int)(Offset)(Enum.Parse(typeof(Offset), x))) == offset).ToArray();
            if (offsetNames.Length > 0)
                return (Sanford.Multimedia.Key)Enum.Parse(typeof(Sanford.Multimedia.Key), offsetNames[0]);
            else
                return Sanford.Multimedia.Key.CMajor;
        }
    }
}

