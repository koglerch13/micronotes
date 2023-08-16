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

    private List<Note> _notes;
    public IReadOnlyCollection<Note> Notes => _notes;

    public bool HasUnsavedNotes => _notes.Any(x => x.HasUnsavedChanges);

    public NotesCollection()
    {
        _notes = new List<Note>();
    }

    public void SetItems(ICollection<Note> notes)
    {
        foreach (var note in notes)
        {
            note.PropertyChanged += OnNotePropertyChanged;
        }
        
        _notes = new List<Note>(notes);
        TriggerNotesChanged();
    }

    public void Add(Note note)
    {
        note.PropertyChanged += OnNotePropertyChanged;
        
        _notes.Add(note);
        _notes = new List<Note>(_notes);
        TriggerNotesChanged();
    }

    private void OnNotePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Note.HasUnsavedChanges))
        {
            TriggerHasUnsavedNotesChanged();
        }
    }

    public void Remove(Note note)
    {
        note.PropertyChanged -= OnNotePropertyChanged;
        
        _notes.Remove(note);
        _notes = new List<Note>(_notes);
        TriggerNotesChanged();
    }

    public void Sort()
    {
        _notes = _notes.OrderBy(x => x.OriginalTitle).ToList();
        TriggerNotesChanged();
    }

    private void TriggerNotesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Notes)));
    }

    private void TriggerHasUnsavedNotesChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HasUnsavedNotes)));
    }
}