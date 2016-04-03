using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.IO;
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
using MusicAnalyzer.Tools;
using MusicAnalyzer.GUI;
using Sanford.Multimedia.Midi;
using MusicAnalyzer.Models;

namespace MusicAnalyzer
{
    /// <summary>
    /// Interaction logic for StartWindows.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        string configDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\ConfigFiles";
        string midisDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + "\\Midis";
        Sequence reader;
        private readonly BackgroundWorker midiAnalyzer = new BackgroundWorker();
        private MusicPiece musicPiece;

        public StartWindow()
        {
            InitializeComponent();
            configDirTextBox.Text = configDirectory;
            midiAnalyzer.DoWork += midiAnalyzer_DoWork;
            midiAnalyzer.RunWorkerCompleted += midiAnalyzer_RunWorkerCompleted;
            midiAnalyzer.ProgressChanged += midiAnalyzer_ReportProgress;
            midiAnalyzer.WorkerReportsProgress = true;
        }

        private void openMidButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".mid";
            dlg.Filter = "MIDI Files (*.mid)|*.mid";
            if (Directory.Exists(midisDirectory))
                dlg.InitialDirectory = midisDirectory;
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                midFileTextBox.Text = dlg.FileName;
            }
        }

        private void setDirButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.SelectedPath = configDirectory;
            dlg.ShowNewFolderButton = false;
            dlg.ShowDialog();
            if (dlg.SelectedPath != "")
            {
                configDirTextBox.Text = dlg.SelectedPath;
            }
            configDirectory = dlg.SelectedPath;
        }

        private void readButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(midFileTextBox.Text))
            {
                reader = new Sequence();
                reader.LoadProgressChanged += HandleLoadProgressChanged;
                reader.LoadCompleted += HandleLoadCompleted;
                reader.LoadAsync(midFileTextBox.Text);
            }
            else
            {
                MessageBox.Show("Please specify a correct input file.", "File error", MessageBoxButton.OK);
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HandleLoadProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            readProgressBar.Value = e.ProgressPercentage / 2;
        }

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                musicPiece = new MusicPiece(reader, configDirectory);
                if (Directory.Exists(configDirectory))
                {
                    midiAnalyzer.RunWorkerAsync();
                }
                else
                {
                    System.Windows.MessageBox.Show("Plesase set config directory.", "Incomplete data");
                }
            }
            else
            {
                MessageBox.Show("Reading input file failed.\n " + e.Error.Message, "Error", MessageBoxButton.OK);
            }
        }

        private void midiAnalyzer_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            var window = new PlayerWindow(musicPiece, configDirectory, midFileTextBox.Text);
            window.Owner = this;
            window.Show();
        }

        private void midiAnalyzer_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            musicPiece.initTools();
            midiAnalyzer.ReportProgress(20);
            musicPiece.initNotes();
            midiAnalyzer.ReportProgress(40);
            musicPiece.initTonations();
            midiAnalyzer.ReportProgress(60);
            musicPiece.initMetrum();
            midiAnalyzer.ReportProgress(80);
            musicPiece.setRightNotesAndTonations();
            midiAnalyzer.ReportProgress(100);
        }

        private void midiAnalyzer_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            readProgressBar.Value = 50 + e.ProgressPercentage / 2;
        }
    }
}
