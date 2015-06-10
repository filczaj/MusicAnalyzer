using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicAnalyzer.Models;
using MusicAnalyzer.Tools;

namespace MusicAnalyzer.Analyzer
{
    class TonationFinder
    {
        List<Note> allNotes;
        List<Note> mainNotes;
        Dictionary<String, int> keyNotesOcurruences;
        Dictionary<String, double> keyNotesDurations;
        NoteTools noteTools;
        int bitrate;

        public TonationFinder()
        {

        }

        public TonationFinder(List<Note> notes, int bit)
        {
            this.bitrate = bit;
            this.allNotes = notes;
            this.noteTools = new NoteTools();
            this.noteTools.setNotesDuration(notes);
        }

        public void findHarmony()
        {
            mainNotes = this.getMainNotes();

        }

        public List<Note> getMainNotes()
        {
            mainNotes = new List<Note>();
            int avgDuration = noteTools.AVGNoteDuration(allNotes);
            for (int i = 0; i < allNotes.Count - 1; i++)
            {
                if (allNotes[i].duration > 0.7 * avgDuration)
                    mainNotes.Add(allNotes[i]);
            }
            mainNotes.Add(allNotes[allNotes.Count - 1]);
            return mainNotes;
        }

        private void generateStats()
        {
            keyNotesDurations = new Dictionary<string, double>();
            keyNotesOcurruences = new Dictionary<string, int>();
            foreach (Note n in mainNotes ){
                keyNotesOcurruences[n.basicNote]++;
                keyNotesDurations[n.basicNote] += n.duration;
            }

        }
    }
}
