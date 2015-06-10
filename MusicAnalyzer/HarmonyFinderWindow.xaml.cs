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
using MusicAnalyzer.Models;
using MusicAnalyzer.Analyzer;

namespace MusicAnalyzer
{
    /// <summary>
    /// Interaction logic for HarmonyFinderWindow.xaml
    /// </summary>
    public partial class HarmonyFinderWindow : Window
    {
        List<Note> allNotes, mainNotes;
        int bitrate;
        Tonation tonation;
        TonationFinder tonationFinder;

        public HarmonyFinderWindow()
        {
            InitializeComponent();
        }

        public HarmonyFinderWindow(List<Note> allFoundNotes, int bitrate)
        {
            this.bitrate = bitrate;
            this.allNotes = allFoundNotes;
            tonationFinder = new TonationFinder(allNotes, bitrate);
            mainNotes = tonationFinder.getMainNotes();
            InitializeComponent();
        }
    }
}
