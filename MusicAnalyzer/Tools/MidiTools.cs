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
#if DEBUG
            MidiTools.genericListSerizliator<MetaType>(types, configDirectory + "\\Tonations.txt");
#endif
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
                            Note n = new Note(message.Data1 - lowNoteID, e.AbsoluteTicks, message.MidiChannel);
                            allNotes.Add(n);
                        }
                        else if ((message.Command == ChannelCommand.NoteOn && message.Data1 >= lowNoteID && message.Data1 <= highNoteID && message.Data2 == 0) ||
                            (message.Command == ChannelCommand.NoteOff && message.Data1 >= lowNoteID && message.Data1 <= highNoteID))
                            {
                                Note n2 = setNoteOff(message.Data1 - lowNoteID, e.AbsoluteTicks, allNotes, message.MidiChannel);
                                fillNoteData(n2);
                            }
                    }
                }
            }
#if DEBUG
            genericListSerizliator<Note>(allNotes.ToList<Note>(), configDirectory + "\\allNotes.txt");
#endif
            return allNotes;
        }

        public Dictionary<int, int> projectTracks(NotesList notesList)
        {
            Dictionary<int, int> tracksProjection = new Dictionary<int, int>();
            List<int> trackIDs = new List<int>();
            foreach (Note n in notesList)
            {
                if (!trackIDs.Contains(n.trackID))
                    trackIDs.Add(n.trackID);
            }
            trackIDs.Sort();
            for (int i = 0; i < trackIDs.Count; i++)
                tracksProjection.Add(trackIDs[i], i);
            return tracksProjection;
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
            return fillLastingInfo(tonations);
        }

        public List<Metrum> findMeter(Sequence sequence)
        {
            MetaMessage metaM;
            List<Metrum> meters = new List<Metrum>();
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
                        meters.Add(new Metrum((int)builder.Numerator, (int)builder.Denominator, e.AbsoluteTicks));
                    }
                }
            }
            meters = fillLastingInfo(meters);
#if DEBUG
            genericListSerizliator<Metrum>(meters, configDirectory + "\\MeterChanges.txt");
#endif
            return meters;
        }

        public Tonation getCurrentTonation(List<Tonation> tonations, int timeIndex)
        {
            Tonation outTonation = tonations.FirstOrDefault(x => timeIndex >= x.startTick && (x.endTick == 0 || timeIndex <= x.endTick));
            if (outTonation == null)
            {
                throw new NullReferenceException("Tonation not found on the list.");
            }
            return outTonation;
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
            return new Tonation(newOffset, outTonationMode, t.startTick, this, t.endTick);
        }

        public int getTonationFifths(Tonation t) // number of tonation key flat or sharps signs; if flats - negative value
        {
            if (t.mode == ChordMode.Minor)
                t = getSiblingTonation(t);
            int sharpFifths = 0;
            int flatFifths = 0;
            while (((int)t.offset + ((int)Interval.Fith * sharpFifths) % 12 != 0) && (sharpFifths > -7))
                sharpFifths--;
            while (((int)t.offset + ((int)Interval.Fith * flatFifths) % 12 != 0) && (flatFifths < 7))
                flatFifths++;
            if (Math.Abs(sharpFifths) < flatFifths)
                return sharpFifths * -1;
            else
                return flatFifths * -1;
        }

        public Metrum getCurrentMetrum(int now, List<Metrum> meterChanges)
        {
            return meterChanges.FirstOrDefault(x => x.startTick <= now && x.endTick > now);
        }

        public List<Tonation> fillLastingInfo(List<Tonation> timeSpanEventCollection)
        {
            var sortedEvents = from t in timeSpanEventCollection
                                  orderby t.startTick
                                  select t;
            timeSpanEventCollection = sortedEvents.ToList<Tonation>();
            for (int i = 0; i < timeSpanEventCollection.Count; i++)
                timeSpanEventCollection[i].endTick = timeSpanEventCollection[(i + 1) % timeSpanEventCollection.Count].startTick;
            if (timeSpanEventCollection.Count > 0)
                timeSpanEventCollection[timeSpanEventCollection.Count - 1].endTick = int.MaxValue - 1;
            return timeSpanEventCollection;
        }

        public List<Metrum> fillLastingInfo(List<Metrum> timeSpanEventCollection)
        {
            var sortedEvents = from t in timeSpanEventCollection
                               orderby t.startTick
                               select t;
            timeSpanEventCollection = sortedEvents.ToList<Metrum>();
            for (int i = 0; i < timeSpanEventCollection.Count; i++)
                timeSpanEventCollection[i].endTick = timeSpanEventCollection[(i + 1) % timeSpanEventCollection.Count].startTick;
            if (timeSpanEventCollection.Count > 0)
                timeSpanEventCollection[timeSpanEventCollection.Count - 1].endTick = int.MaxValue - 1;
            return timeSpanEventCollection;
        }
    }
}
