using System;
using System.Collections.Generic;
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
using System.ComponentModel;
using MusicAnalyzer.Tools;
using MusicAnalyzer.GUI;
using Sanford.Multimedia.Midi;

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

        public StartWindow()
        {
            InitializeComponent();
            configDirTextBox.Text = configDirectory;
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
            readProgressBar.Value = e.ProgressPercentage;
        }

        private void HandleLoadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            readProgressBar.Value = 0;
            if (e.Error == null)
            {
                readProgressBar.Visibility = Visibility.Hidden;
                readProgressBar.Visibility = Visibility.Visible;
                if (Directory.Exists(configDirectory))
                {
                    var window = new PlayerWindow(configDirectory, reader, midFileTextBox.Text);
                    window.Owner = this;
                    window.Show();
                }
                else
                {
                    System.Windows.MessageBox.Show("Plesase set config directory.", "Incomplete data");
                }
            }
            else
            {
                MessageBox.Show("Reading the file failed.\n " + e.Error.Message, "Error", MessageBoxButton.OK);
            }
        }
    }
}
