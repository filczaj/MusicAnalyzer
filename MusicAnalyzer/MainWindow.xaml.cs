using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using MusicAnalyzer.Analyzer;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AForge.Math;
using System.IO;
using MusicAnalyzer.Models;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string configDirectory;
        AudioPlayer ap;
        WAVReader wavReader = new WAVReader();
        public MyFrame[] resData;
        short[] channelData;
        int bitrate;
        NotesPicker picker;
        List<Note> allNotes;
        List<MyFrame> resultNotes;
        FreqMapper freqMapper;
        Stopwatch watcher;
        SoundAnalysis soundAnalysis;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(string config_directory)
        {
            this.configDirectory = config_directory;
            this.wavReader = new WAVReader();
            this.channelData = wavReader.channelBuffer;
            this.bitrate = wavReader.bitrate;
            this.soundAnalysis = new SoundAnalysis(wavReader);
            this.freqMapper = new FreqMapper();
        }

        private void openFileButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".wav";
            dlg.Filter = "Wave Files (*.wav)|*.wav";
            dlg.InitialDirectory = configDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                fileNameBox.Text = dlg.FileName;
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileNameBox.Text != "")
            {
                ap = new AudioPlayer(fileNameBox.Text);
                ap.play();
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            if (ap != null)
                ap.stop();
        }

        public void ClearLines()
        {
            var lgc = new Collection<IPlotterElement>();
            foreach (var x in pitchPlotter.Children)
            {
                if (x is LineGraph || x is ElementMarkerPointsGraph)
                    lgc.Add(x);
            }
            foreach (var x in lgc)
            {
                pitchPlotter.Children.Remove(x);
            }
        }

        private void readButton_Click(object sender, RoutedEventArgs e)
        {
            if (fileNameBox.Text != "" && channelsBox.Text != "" && bitrateBox.Text != "")
            {
                this.bitrate = Convert.ToInt32(bitrateBox.Text);
                wavReader.read(fileNameBox.Text, bitrate, Convert.ToInt32(channelsBox.Text));
                this.channelData = wavReader.channelBuffer;
                framesBox.Text = wavReader.channelBuffer.Length.ToString();
                updateGraph(-1);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("To do!!!");
        }

        private void notesFinderButton_Click(object sender, RoutedEventArgs e)
        {
            picker = new NotesPicker(channelData, bitrate, wavReader);
            watcher = Stopwatch.StartNew();
            resultNotes = picker.findAllPitchChanges();
            watcher.Stop();
            allNotes = freqMapper.notesMapper(resultNotes);
            allNotes = picker.removeRepeatedNotes(allNotes);
            resultNotes = freqMapper.notesReverseMapper(allNotes);
            MessageBox.Show("All notes found in time: " + watcher.ElapsedMilliseconds.ToString(), "Notes found.", MessageBoxButton.OK);
            pitchListBox.Items.Clear();
            foreach (MyFrame note in resultNotes)
                pitchListBox.Items.Add(note.index.ToString());
            updateGraph();
        }

        private void updateGraph()
        {
            IPointDataSource _eds = null;
            LineGraph line;

            List<MyFrame> graphResults = resultNotes.GetRange(0, resultNotes.Count);
            graphResults = convertResultsToGraph(graphResults);
            ClearLines();

            EnumerableDataSource<MyFrame> _edsSPP = new EnumerableDataSource<MyFrame>(graphResults);
            _edsSPP.SetYMapping(p => p.value);
            _edsSPP.SetXMapping(p => p.index);
            _eds = _edsSPP;

            line = new LineGraph(_eds);
            line.LinePen = new Pen(Brushes.Blue, 2);
            pitchPlotter.Children.Add(line);
            pitchPlotter.FitToView();
        }

        private void updateGraph(int starter)
        {
            IPointDataSource _eds = null;
            LineGraph line;
            List<MyFrame> graphResults = new List<MyFrame>();
            if (starter >= 0)
            {
                Complex[] resData = soundAnalysis.getFourierData();
                double[] data = new double[resData.Length / 2];
                for (int i = 0; i < resData.Length /2; i++)
                    data[i] = Math.Sqrt(Math.Pow(resData[i].Re, 2) + Math.Pow(resData[i].Im, 2));
                for (int i = 0; i < data.Length; i++)
                    graphResults.Add(new MyFrame(2.0 * data[i], 2 * i));
            }
            else
            {
                graphResults = wavReader.viewDataSource().ToList<MyFrame>();
            }
            ClearLines();

            EnumerableDataSource<MyFrame> _edsSPP = new EnumerableDataSource<MyFrame>(graphResults);
            _edsSPP.SetYMapping(p => p.value);
            _edsSPP.SetXMapping(p => p.index);
            _eds = _edsSPP;

            line = new LineGraph(_eds);
            line.LinePen = new Pen(Brushes.Red, 2);
            pitchPlotter.Children.Add(line);
            pitchPlotter.FitToView();
        }

        private List<MyFrame> convertResultsToGraph(List<MyFrame> dataResults)
        {
            dataResults.Insert(0, new MyFrame(0.0, 0));
            int originCount = dataResults.Count;
            for (int i = 0; i < originCount * 2 - 2; i += 2)
                dataResults.Insert(i + 1, new MyFrame(dataResults[i].value, dataResults[i + 1].index - 2));
            dataResults.Insert(dataResults.Count, new MyFrame(dataResults[dataResults.Count - 1].value, Convert.ToInt32(wavReader.totalSeconds * 1000)));

            return dataResults;
        }

        private void pitchListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = pitchListBox.SelectedIndex;
            if (index >= 0)
            {
                pitchHzBox.Text = resultNotes[index].value.ToString();
                pitchBox.Text = freqMapper.findPeach(resultNotes[index].value);
                soundAnalysis.runDFT(resultNotes[index].index);
                updateGraph(resultNotes[index].index);
            }
        }
    }
}
