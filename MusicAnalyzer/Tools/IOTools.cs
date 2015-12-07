using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;
using Sanford.Multimedia.Midi;
using MusicAnalyzer.Models;

namespace MusicAnalyzer.Tools
{
    class IOTools
    {
        BackgroundWorker loadWorker;
        BackgroundWorker saveWorker;

        public MidiFileStruct midiFileStruct;

        public event ProgressChangedEventHandler LoadProgressChanged;

        public event EventHandler<AsyncCompletedEventArgs> LoadCompleted;
        public static IEnumerable<string> ReadFrom(string file)
        {
            string line;
            using (var reader = File.OpenText(file))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public void initWorker()
        {
            loadWorker = new BackgroundWorker();
            loadWorker.DoWork += new DoWorkEventHandler(loadWorker_DoWork);
            loadWorker.ProgressChanged += new ProgressChangedEventHandler(loadWorker_ProgressChanged);
            loadWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(loadWorker_RunWorkerCompleted);
            loadWorker.WorkerReportsProgress = true;
        }
        public void LoadAsync(string fileName) {
            loadWorker.RunWorkerAsync(fileName);
        }

        private void loadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            EventHandler<AsyncCompletedEventArgs> handler = LoadCompleted;

            if (handler != null)
            {
                handler(this, new AsyncCompletedEventArgs(e.Error, e.Cancelled, null));
            }
        }

        private void loadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressChangedEventHandler handler = LoadProgressChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void loadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = e.Argument.ToString();
            FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            using (stream)
            {
                MidiFileProperties newProperties = new MidiFileProperties();
                TrackReader reader = new TrackReader();
                List<Track> newTracks = new List<Track>();

                newProperties.Read(stream);

                float percentage;

                for (int i = 0; i < newProperties.TrackCount && !loadWorker.CancellationPending; i++)
                {
                    reader.Read(stream);
                    newTracks.Add(reader.Track);

                    percentage = (i + 1f) / newProperties.TrackCount;

                    loadWorker.ReportProgress((int)(100 * percentage));
                    System.Threading.Thread.Sleep(1000);
                }

                if (loadWorker.CancellationPending)
                {
                    e.Cancel = true;
                }
                else
                {
                    this.midiFileStruct = new MidiFileStruct(newTracks, newProperties);
                }
            }
        }
    }
}
