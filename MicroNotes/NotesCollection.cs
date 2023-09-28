using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using DynamicData;

namespace MicroNotes;

public class NotesCollection : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private List<Note> _notes;
    private List<Note> _originalNotes;
    private Note? _selectedNote;

    public IReadOnlyList<Note> Notes => _notes;

    public bool HasUnsavedNotes => _notes.Any(x => x.HasUnsavedChanges);

    public Note? SelectedNote
    {
        get => _selectedNote;
        set
        {
            _selectedNote = value;
            TriggerSelectedNoteChanged();
        }
    }

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

    public void SelectNextNote()
    {
        if (SelectedNote == null)
        {
            SelectedNote = Notes.First();
            return;
        }

        var selectedIndex = Notes.IndexOf(SelectedNote);
        if (selectedIndex == Notes.Count - 1)
        {
            SelectedNote = Notes.First();
            return;
        }

        SelectedNote = Notes[selectedIndex + 1];
    }

    public void SelectPreviousNote()
    {
        if (SelectedNote == null)
        {
            SelectedNote = Notes.Last();
            return;
        }

        var selectedIndex = Notes.IndexOf(SelectedNote);
        if (selectedIndex == 0)
        {
            SelectedNote = Notes.Last();
            return;
        }

        SelectedNote = Notes[selectedIndex - 1];
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

    private void TriggerSelectedNoteChanged()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedNote)));
    }
}