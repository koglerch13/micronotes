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
    private List<Note> _originalNotes;
    
    public IReadOnlyList<Note> Notes => _notes;

    public bool HasUnsavedNotes => _notes.Any(x => x.HasUnsavedChanges);

    public NotesCollection()
    {
        _notes = new List<Note>();
        _originalNotes = new List<Note>();
    }

    public void SetItems(ICollection<Note> notes)
    {
        foreach (var note in _originalNotes)
        {
            note.PropertyChanged -= OnNotePropertyChanged;
        }
        
        foreach (var note in notes)
        {
            note.PropertyChanged += OnNotePropertyChanged;
        }
        
        _originalNotes = new List<Note>(notes);
        
        _notes = new List<Note>(_originalNotes);
        TriggerNotesChanged();
    }

    public void Add(Note note)
    {
        note.PropertyChanged += OnNotePropertyChanged;
        
        _originalNotes.Add(note);
        _originalNotes = new List<Note>(_originalNotes);
        
        _notes = new List<Note>(_originalNotes);
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
        
        _originalNotes.Remove(note);
        
        _notes = new List<Note>(_originalNotes);
        TriggerNotesChanged();
    }

    public void Sort()
    {
        _notes = _originalNotes.OrderBy(x => x.OriginalTitle).ToList();
        TriggerNotesChanged();
    }

    public void Filter(string? title)
    {
        if (string.IsNullOrEmpty(title))
        {
            _notes = new List<Note>(_originalNotes);
            TriggerNotesChanged();
        }
        else
        {
            _notes = _originalNotes.Where(x => x.OriginalTitle.Contains(title, StringComparison.InvariantCultureIgnoreCase)).ToList();
            TriggerNotesChanged();
        }
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