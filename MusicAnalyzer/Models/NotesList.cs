using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicAnalyzer.Models
{
    public class NotesList : IEnumerable<Note>
    {
        List<Note> notes;

        public NotesList()
        {
            notes = new List<Note>();
        }
        public IEnumerator<Note> GetEnumerator()
        {
            return this.notes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.notes.GetEnumerator();
        }

        public void Add(Note note)
        {
            if (notes != null)
                notes.Add(note);
        }

        public void Remove(Note note)
        {
            if (notes != null)
                notes.Remove(note);
        }

        public void RemoveAt(int index)
        {
            if (notes != null)
                notes.RemoveAt(index);
        }

        public int Count()
        {
            return notes.Count;
        }
    }
}
