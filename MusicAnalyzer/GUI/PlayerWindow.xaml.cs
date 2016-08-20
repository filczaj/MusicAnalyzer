using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using PSAMWPFControlLibrary;
using Sanford.Multimedia.Midi;
using MusicAnalyzer.Tools;
using MusicAnalyzer.Models;
using PSAMControlLibrary;

namespace MusicAnalyzer.GUI
{
    /// <summary>
    /// Interaction logic for PlayWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        List<IncipitViewerWPF> scoreViewers;
        List<CheckBox> checkedTracks; // set Checkbox of a track to delete it
        List<Grid> viewGrids;
        string configDirectory;
        string fileName;
        string newFileName;
        Sequence sequence;
        bool playing = false;
        bool closing = false;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Sequencer player;

        private OutputDevice outDevice;
        private int outDeviceID = 0;
        private int scoreViewHeight = 80;
        
        MusicPiece musicPiece;

        public PlayerWindow(MusicPiece musicPieceIn, string configDir, string fullFileName)
        {
            InitializeComponent();
            this.musicPiece = musicPieceIn;
            this.configDirectory = configDir;
            this.sequence = musicPiece.sequence;
            this.fileNameBox.Text = fileName = fullFileName;
            this.checkedTracks = new List<CheckBox>();
            this.viewGrids = new List<Grid>();
            initWindow();
        }

        public void initWindow()
        {
            initPlayer();
            initScoreViewer();
            fillScoreViewer();
            if (!musicPiece.isInputFileCorrect())
            {
                System.Windows.MessageBox.Show("There is some missing information in the input file. Composing music is unavailable.", "Inpute file error", MessageBoxButton.OK);
                composeButton.IsEnabled = false;
            }
            if (instrumentCombo.Items.Count > 0)
                instrumentCombo.SelectedIndex = 0;
            instrumentCombo.IsEnabled = false;
        }

        private void initPlayer()
        {
            this.sequence.Format = 1;
            player = new Sequencer();
            player.Sequence = sequence;
            this.player.PlayingCompleted += new System.EventHandler(this.HandlePlayingCompleted);
            this.player.ChannelMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.ChannelMessageEventArgs>(this.HandleChannelMessagePlayed);
            this.player.Stopped += new System.EventHandler<Sanford.Multimedia.Midi.StoppedEventArgs>(this.HandleStopped);
            this.player.SysExMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.SysExMessageEventArgs>(this.HandleSysExMessagePlayed);
            this.player.Chased += new System.EventHandler<Sanford.Multimedia.Midi.ChasedEventArgs>(this.HandleChased);
            this.player.Position = 0;
            this.timer.Interval = 1000;
            this.timer.Tick += new System.EventHandler(this.timer_Tick);
        }

        private void initScoreViewer()
        {
            scoreViewers = new List<IncipitViewerWPF>();
            for (int i = 0; i < sequence.Count; i++)            
                addTrackWPFView(i);
        }

        private void fillScoreViewer()
        {
            for (int i = 0; i < sequence.Count; i++)
                musicPiece.fillScoreViewer(scoreViewers[i], i);
            scoreGrid.Height = scoreViewHeight * scoreViewers.Count;
            scoreGrid.Width = 15000;
        }

        private void addTrackWPFView(int index)
        {
            IncipitViewerWPF scv = new IncipitViewerWPF();
            scv.Width = 15000;
            scv.Height = scoreViewHeight;
            scv.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            scv.Margin = new Thickness(20, 0, 0, 0);
            scoreViewers.Add(scv);
            RowDefinition rd = new RowDefinition();
            rd.Height = new GridLength(scoreViewHeight, GridUnitType.Pixel);
            scoreGrid.RowDefinitions.Add(rd);
            Grid trackGrid = new Grid();
            RowDefinition rd2 = new RowDefinition();
            rd2.Height = new GridLength(scoreViewHeight, GridUnitType.Pixel);
            trackGrid.RowDefinitions.Add(rd2);
            CheckBox checkBox = new CheckBox();
            checkBox.Width = 20;
            checkBox.Height = 20;
            checkBox.IsChecked = false;
            checkBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            checkBox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            trackGrid.Children.Add(checkBox);
            trackGrid.Children.Add(scv);
            scoreGrid.Children.Add(trackGrid);
            checkedTracks.Add(checkBox);
            viewGrids.Add(trackGrid);
            Grid.SetRow(trackGrid, index);
        }

        private void HandleChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
        {
            if (closing)
            {
                return;
            }

            outDevice.Send(e.Message);
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            // ????????
        }

        private void HandleChased(object sender, ChasedEventArgs e)
        {
            foreach (ChannelMessage message in e.Messages)
                outDevice.Send(message);
        }

        private void HandleSysExMessagePlayed(object sender, SysExMessageEventArgs e)
        {
            // nicość
        }

        private void HandleStopped(object sender, StoppedEventArgs e)
        {
            foreach (ChannelMessage message in e.Messages)
                outDevice.Send(message);
        }

        private void HandlePlayingCompleted(object sender, EventArgs e)
        {
            playing = false;
            timer.Stop();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                playing = true;
                player.Continue();
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK);
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                playing = false;
                player.Stop();
                timer.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK);
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                playing = false;
                player.Stop();
                timer.Stop();
                this.player.Position = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK);
            }
        }

        private void backButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (playing)
                stopButton_Click(null, null);
            this.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (playing)
                stopButton_Click(null, null);
            this.Close();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //save
            newFileName = fileName;
            sequence.SaveAsync(newFileName);
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            // save as
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = fileName.Split('\\').Last() + "_1";
            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI files (.mid)|*.mid";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                newFileName = dlg.FileName;
                fileNameBox.Text = newFileName;
                sequence.SaveAsync(newFileName);
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (playing)
                stopButton_Click(null, null);
            this.Close();
        }

        private void deleteTrackButton_Click(object sender, RoutedEventArgs e)
        {
            int i = 0;
            while(i < checkedTracks.Count)
            {
                if (checkedTracks[i].IsChecked == true)
                {
                    scoreViewers.RemoveAt(i);
                    sequence.Remove(sequence[i]);
                    scoreGrid.Children.Remove(viewGrids[i]);
                    viewGrids.RemoveAt(i);
                    musicPiece.tracksProjection.Remove(musicPiece.tracksProjection.First(x => x.Value == i).Key);
                    checkedTracks.RemoveAt(i);
                    foreach (int key in musicPiece.tracksProjection.Keys.ToList())
                        if (musicPiece.tracksProjection[key] > i)
                            musicPiece.tracksProjection[key]--;
                }
                else
                    i++;
            }
            updateScoreGridHeight();
            updateScoreGridWidth();
        }

        private void composeButton_Click(object sender, RoutedEventArgs e)
        {
            musicPiece.completeNotesInfo();
            musicPiece.findBestChords();
            musicPiece.fillNewTrackNotes();
            addTrackWPFView(scoreViewers.Count);
            musicPiece.fillScoreViewer(scoreViewers[scoreViewers.Count - 1], scoreViewers.Count - 1);
            scoreGrid.Height = scoreViewHeight * scoreViewers.Count;
            scoreViewers[scoreViewers.Count - 1].UpdateLayout();
            updateScoreGridWidth();
        }

        public void updateScoreGridWidth()
        {
            float maxLocationX = 0;
            foreach (IncipitViewerWPF viewer in scoreViewers)
            {
                PSAMControlLibrary.MusicalSymbol noteWPF = null;
                int lastNoteIndex = viewer.CountIncipitElements - 1;
                while ((noteWPF == null || noteWPF.Type != MusicalSymbolType.Note) && lastNoteIndex >=0)
                {
                    noteWPF = viewer.IncipitElement(lastNoteIndex);
                    lastNoteIndex--;
                }
                if (noteWPF != null){
                    PSAMControlLibrary.Note note = null;
                    try{
                        note = noteWPF as PSAMControlLibrary.Note;
                    }
                    catch(InvalidCastException ex){

                    }
                    if (note != null){
                        viewer.Width = note.Location.X + 40;
                        if (maxLocationX < note.Location.X)
                            maxLocationX = note.Location.X;
                    }
                }
            }
            scoreGrid.Width = maxLocationX + 44;
        }

        private void updateScoreGridHeight()
        {
            scoreGrid.Children.RemoveRange(0, scoreGrid.Children.Count);
            for (int i = 0; i < viewGrids.Count; i++)
            {
                scoreGrid.Children.Add(viewGrids[i]);
                Grid.SetRow(viewGrids[i], i);
            }
            scoreGrid.Height = scoreViewHeight * scoreViewers.Count;
        }
        
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            closing = true;
            if (playing)
                stopButton_Click(null, null);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (OutputDevice.DeviceCount == 0)
            {
                MessageBox.Show("No MIDI output devices available.", "Error!", MessageBoxButton.OK);

                Close();
            }
            else
            {
                try
                {
                    outDevice = new OutputDevice(outDeviceID);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK);

                    Close();
                }
            }
        }

        private void volDownButton_Click(object sender, RoutedEventArgs e)
        {
            SystemTools.VolDown(Window.GetWindow(this));
        }

        private void volUpButton_Click(object sender, RoutedEventArgs e)
        {
            SystemTools.VolUp(Window.GetWindow(this));
        }
    }
}
