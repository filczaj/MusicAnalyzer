using MusicAnalyzer.Models;
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

namespace MusicAnalyzer.GUI
{
    /// <summary>
    /// Interaction logic for PlayWindow.xaml
    /// </summary>
    public partial class PlayerWindow : Window
    {
        string configDirectory;
        MidiFileStruct midiFile;
        bool playing = false;
        System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        Sequencer player;

        public PlayerWindow()
        {
            InitializeComponent();
        }

        public PlayerWindow(string configDir, MidiFileStruct musicFile)
        {
            InitializeComponent();
            this.configDirectory = configDir;
            this.midiFile = musicFile;
            player = new Sequencer();
            player.Sequence = new Sequence(); // To DO - podmienić IOTools na Sequence, żeby tu go podać!!!
            this.fileNameBox.Text = midiFile.fileName;
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                playing = true;
                player.Start();
                timer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK);
            }
        }

        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
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

        private void backButton_Click_1(object sender, RoutedEventArgs e)
        {
            stopButton_Click(null, null);
            this.Close();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            stopButton_Click(null, null);
            this.Close();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //save
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            // save as
        }

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            stopButton_Click(null, null);
            this.Close();
        }
    }
}
