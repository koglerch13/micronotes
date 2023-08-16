using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using DynamicData;

namespace MicroNotes;

public class NotesCollection : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public event EventHandler<Note>? ItemChanged; 

    private List<Note> _notes;
    public IReadOnlyCollection<Note> Notes => _notes;

    public NotesCollection()
    {
        _notes = new List<Note>();
    }

    public void SetItems(ICollection<Note> notes)
    {
        _notes = new List<Note>(notes);
        TriggerPropertyChanged();
    }

    public void Add(Note note)
    {
        _notes.Add(note);
        _notes = new List<Note>(_notes);
        TriggerPropertyChanged();
    }

    public void Remove(Note note)
    {
        _notes.Remove(note);
        _notes = new List<Note>(_notes);
        TriggerPropertyChanged();
    }

    public void Sort()
    {
        _notes = _notes.OrderBy(x => x.OriginalTitle).ToList();
        TriggerPropertyChanged();
    }

    private void TriggerPropertyChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
    }
}