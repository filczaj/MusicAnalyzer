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
using Sanford.Multimedia.Midi;
using MusicAnalyzer.Tools;
using NAudio.Midi;
using MusicAnalyzer.Models;

namespace MusicAnalyzer.GUI
{
    /// <summary>
    /// Interaction logic for PlayWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        string configDirectory;
        string fileName;
        Sequence sequence;
        bool playing = false;
        bool closing = false;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Sequencer player;
        int selectedTrack = -1;

        private OutputDevice outDevice;
        private int outDeviceID = 0;
        
        MusicPiece musicPiece;

        public PlayerWindow()
        {
            InitializeComponent();
        }

        public PlayerWindow(string configDir, Sequence reader, string fullFileName)
        {
            InitializeComponent();
            this.configDirectory = configDir;
            this.sequence = reader;
            initPlayer();
            this.musicPiece = new MusicPiece(reader, configDirectory);

            this.fileNameBox.Text = fileName = fullFileName;          
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
            sequence.SaveAsync(fileName);
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
                //sequence = new Sequence();
                //player.Sequence = sequence;
                fileName = dlg.FileName;
                fileNameBox.Text = fileName;
                sequence.SaveAsync(fileName);
            }
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (playing)
                stopButton_Click(null, null);
            this.Close();
        }

        private void addTrackButton_Click(object sender, RoutedEventArgs e)
        {
            Track newTrack = new Track();
            sequence.Add(newTrack);
        }

        private void deleteTrackButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedTrack >= 0)
                sequence.Remove(sequence[selectedTrack]);
        }

        private void composeButton_Click(object sender, RoutedEventArgs e)
        {
            // to do!!!!!!!!!!!!!
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
