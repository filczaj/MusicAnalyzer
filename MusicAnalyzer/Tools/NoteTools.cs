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

        public void initTools()
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

        public int getNoteOctave(Note note)
        {
            return Convert.ToInt32(note.note[note.note.Length - 1]) - 48;
        }

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

        public List<TonationChord> setMainChords(ChordMode mode)
        {
            String chordsFile;
            List<TonationChord> chords = new List<TonationChord>();
            if (mode == ChordMode.Major)
                chordsFile = majorChordsFile;
            else
                chordsFile = minorChordsFile;
            IEnumerable<String> lines = IOTools.ReadFrom(chordsFile);
            
            if (lines.ToList().Count < 6)
                return chords;

            TonationChord chord = new TonationChord(mode, ChordPriority.Tonic, 0);
            chord.chordNotes = new List<int>();
            int[] asIntegers = lines.ElementAt(0).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new TonationChord(mode, ChordPriority.Subdominant, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(1).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new TonationChord(mode, ChordPriority.Dominant, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(2).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new TonationChord(mode, ChordPriority.SecondStep, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(3).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new TonationChord(mode, ChordPriority.SixthStep, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(4).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            chord = new TonationChord(mode, ChordPriority.ThirdStep, 0);
            chord.chordNotes = new List<int>();
            asIntegers = lines.ElementAt(5).Split(' ').Select(s => int.Parse(s)).ToArray();
            chord.chordNotes.AddRange(asIntegers);
            chords.Add(chord);

            if (lines.ToList().Count > 7)
            {
                chord = new TonationChord(mode, ChordPriority.Dominant, 0);
                chord.chordNotes = new List<int>();
                asIntegers = lines.ElementAt(6).Split(' ').Select(s => int.Parse(s)).ToArray();
                chord.chordNotes.AddRange(asIntegers);
                chords.Add(chord);

                chord = new TonationChord(mode, ChordPriority.ThirdStep, 0);
                chord.chordNotes = new List<int>();
                asIntegers = lines.ElementAt(7).Split(' ').Select(s => int.Parse(s)).ToArray();
                chord.chordNotes.AddRange(asIntegers);
                chords.Add(chord);
            }

            return chords;
        }

        public void fillNoteData(Note n)
        {
            n.note = getNoteById(n.noteID);
            n.basicNote = getBasicNoteById(n.noteID);
            n.octave = getNoteOctave(n);
            if (n.startTime >= 0 && n.duration >= 0 && n.endTime < 0)
                n.endTime = n.startTime + n.duration;
            if (n.startTime >= 0 && n.endTime >= 0 && n.duration < 0)
                n.duration = n.endTime - n.startTime;
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

        public static int getSharpOrFlat(Note n)
        {
            if (n.basicNote.Contains("Sharp"))
                return 1;
            if (n.basicNote.Contains("Flat"))
                return -1;
            return 0;
        }

        public Note setNoteOff(int noteID, int endTime, NotesList allNotes, int channel)
        {
            Note n = allNotes.FirstOrDefault(x => (x.endTime == -1 && x.noteID == noteID && x.trackID == channel));
            if (n != null && n.noteID == noteID)
            {
                n.endTime = endTime;
                n.duration = n.endTime - n.startTime;
            }
            return n;
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

        public double ProperDurationAndExtension(Note n, int division)
        {
            if (n.duration < 1)
                return 0;
            double scaleDuration = n.duration / (double)(division * 4);
            double p = Math.Log(scaleDuration, 2);
            p = Math.Floor(p);
            p = Math.Pow(2, (-1 * p));
            if (scaleDuration > 1.75 / (double)p)
                return p / 2.0;
            else if (scaleDuration > 1.5 / (double)p)
                n.noteExtension = 1.75;
                else if (scaleDuration > 1.0 / (double)p)
                    n.noteExtension = 1.5;
                return p;
        }

        public PSAMControlLibrary.NoteStemDirection getNoteStemDirection(Note n, Note last, PSAMControlLibrary.ClefType clef)
        {
            if (last != null && n.startTime == last.startTime)
                return last.noteStemDirection;
            switch (clef)
            {
                case PSAMControlLibrary.ClefType.GClef:
                    if (n.noteID > 49) return PSAMControlLibrary.NoteStemDirection.Down;
                    else
                        return PSAMControlLibrary.NoteStemDirection.Up;
                case PSAMControlLibrary.ClefType.FClef:
                    if (n.noteID > 28) return PSAMControlLibrary.NoteStemDirection.Down;
                    else
                        return PSAMControlLibrary.NoteStemDirection.Up;
                default:
                    return PSAMControlLibrary.NoteStemDirection.Up;
            }
        }
    }
}