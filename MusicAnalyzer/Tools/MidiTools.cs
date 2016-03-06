using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using Sanford.Multimedia.Midi;

namespace MusicAnalyzer.Tools
{
    public class MidiTools : NoteTools
    {
        public MidiTools(string configDir) : base(configDir){
            initTools();
        }

        private void findAndSaveMetaTypes(Sequence seq)
        {
            List<MetaType> types = new List<MetaType>();
            MetaMessage meta;
            foreach (Track track in seq)
            {
                foreach (MidiEvent e in track.Iterator())
                {
                    meta = null;
                    try
                    {
                        meta = (e.MidiMessage as MetaMessage);
                    }
                    catch ( InvalidCastException exc)
                    {

                    }
                    if (meta!= null){
                        types.Add(meta.MetaType);
                    }
                }
            }
            string fileName = "C:\\Magisterka\\MusicAnalyzer\\MusicAnalyzer\\ConfigFiles\\MetaTypes.txt";
            List<string> allLines = new List<string>();
            foreach (MetaType n in types)
            {
                allLines.Add(n.ToString());
            }
            IOTools.saveTo(allLines, fileName);
        }

        public IEnumerable<Note> decodeNotes(Sequence sequence)
        {
            NotesList allNotes = new NotesList();
            ChannelMessage message;
            foreach (Track track in sequence)
            {
                foreach (MidiEvent e in track.Iterator())
                {
                    message = null;
                    try{
                        message = (e.MidiMessage as ChannelMessage);
                    }
                    catch(InvalidCastException ex){
                        message = null;
                    }
                    if (message != null)
                    {
                        if (message.Command == ChannelCommand.NoteOn && message.Data1 >= lowNoteID && message.Data1 <= highNoteID && message.Data2 > 0)
                        {
                            Note n = new Note(message.Data1 - lowNoteID, e.AbsoluteTicks);
                            if (allNotes.Count() > 0)
                                allNotes.ElementAt(allNotes.Count()-1).duration = e.DeltaTicks;
                            n = fillNoteData(n);
                            allNotes.Add(n);
                        }
                        else if (message.Command == ChannelCommand.NoteOff && message.Data1 >= lowNoteID && message.Data1 <= highNoteID)
                            setNoteOff(message.Data1 - lowNoteID, e.AbsoluteTicks, allNotes);
                    }
                }
            }
#if DEBUG
            genericListSerizliator<Note>(allNotes.ToList<Note>(), configDirectory + "\\allNotes.txt");
#endif
            return allNotes;
        }

        public List<Tonation> grabTonations(Sequence sequence)
        {
            MetaMessage metaM;
            List<Tonation> tonations = new List<Tonation>();
            foreach (Track track in sequence)
            {
                foreach (MidiEvent e in track.Iterator())
                {
                    try
                    {
                        metaM = (e.MidiMessage as MetaMessage);
                    }
                    catch (InvalidCastException ex)
                    {
                        metaM = null;
                    }
                    if (metaM != null && metaM.MetaType == MetaType.KeySignature)
                        if (!tonations.Exists(x => x.startTick == e.AbsoluteTicks))
                            tonations.Add(new Tonation(metaM, e.AbsoluteTicks, this));
                }
            }
            var sortedTonations = from t in tonations
                          orderby t.startTick
                          select t;
            tonations = sortedTonations.ToList<Tonation>();
            for (int i = 0; i < tonations.Count; i++)
                tonations[i].endTick = tonations[(i + 1) % tonations.Count].startTick;
            tonations[tonations.Count - 1].endTick = int.MaxValue;
#if DEBUG
            MidiTools.genericListSerizliator<Tonation>(tonations, configDirectory + "\\Tonations.txt");
#endif
            return tonations;
        }

        public List<MeterChange> findMeter(Sequence sequence)
        {
            MetaMessage metaM;
            List<MeterChange> meters = new List<MeterChange>();
            foreach (Track track in sequence)
            {
                foreach (MidiEvent e in track.Iterator())
                {
                    try
                    {
                        metaM = (e.MidiMessage as MetaMessage);
                    }
                    catch (InvalidCastException ex)
                    {
                        metaM = null;
                    }
                    if (metaM != null && metaM.MetaType == MetaType.TimeSignature)
                    {
                        TimeSignatureBuilder builder = new TimeSignatureBuilder(metaM);
                        meters.Add(new MeterChange((int)builder.Numerator, (int)builder.Denominator, e.AbsoluteTicks));
                    }
                }
            }
            
#if DEBUG
            genericListSerizliator<MeterChange>(meters, configDirectory + "\\MeterChanges.txt");
#endif
            return meters;
        }

        public void serialize(IEnumerable<Note> allNotes)
        {
            #region debug
            //fileName = "C:\\Magisterka\\MusicAnalyzer\\MusicAnalyzer\\ConfigFiles\\allStartTimesSorted.txt";
            //allLines = new List<string>();
            //Dictionary<int, int> startTimes = new Dictionary<int, int>();
            //foreach (Note n in allNotes)
            //{
            //    if (startTimes.ContainsKey(n.startTime))
            //        startTimes[n.startTime] += 1;
            //    else startTimes.Add(n.startTime, 1);
            //}
            //var items = from pair in startTimes
            //            orderby pair.Value descending
            //            select pair;
            //foreach (KeyValuePair<int, int> pair in items)
            //{
            //    allLines.Add(pair.Key + " : " + pair.Value);
            //}
            //IOTools.saveTo(allLines, fileName);
            #endregion
        }

        public static void genericListSerizliator<T>(List<T> elements, string fileName)
        {
            List<string> allLines = new List<string>();
            foreach (T element in elements)
            {
                allLines.Add(element.ToString());
            }
            IOTools.saveTo(allLines, fileName);
        }

        public Tonation getSiblingTonation(Tonation t) // returns a minor or major tonation with the same key signature
        {
            ChordMode outTonationMode;
            int newOffset;
            if (t.key > Sanford.Multimedia.Key.ASharpMinor)
            {
                outTonationMode = ChordMode.Minor;
                newOffset = ((int)t.offset + 9) % 12;
            }
            else
            {
                outTonationMode = ChordMode.Major;
                newOffset = ((int)t.offset + 3) % 12;
            }
            return new Tonation(newOffset, outTonationMode, t.startTick, new NoteTools(configDirectory));
        }
    }
}
