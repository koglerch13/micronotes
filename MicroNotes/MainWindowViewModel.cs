using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using DynamicData;
using MicroNotes.MessageBox;
using ReactiveUI;

namespace MicroNotes;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly IMessageBoxService _messageBoxService;
    private readonly MainWindow _mainWindow;
    private string? _folderPath;

    private Note? _selectedNote;
    private bool _isNoteSearchActive;
    private string? _noteSearchQuery;

    public MainWindowViewModel(MainWindow mainWindow, 
        IClassicDesktopStyleApplicationLifetime desktop,
        IMessageBoxService messageBoxService)
    {
        NotesCollection = new NotesCollection();
        
        _messageBoxService = messageBoxService;
        _mainWindow = mainWindow;
        _desktop = desktop;
        _mainWindow.Closing += OnClosing;
        PropertyChanged += OnPropertyChanged;

        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAll);
        NewCommand = ReactiveCommand.Create(New);
        DeleteCommand = ReactiveCommand.CreateFromTask(Delete);
        OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);
        StartNoteSearchCommand = ReactiveCommand.Create(StartNoteSearch);
        CancelNoteSearchCommand = ReactiveCommand.Create(CancelNoteSearch);
        SelectNextNoteCommand = ReactiveCommand.Create(SelectNextNote);
        SelectPreviousNoteCommand = ReactiveCommand.Create(SelectPreviousNote);
        FocusEditorCommand = ReactiveCommand.Create(FocusEditor);
        FocusTitleCommand = ReactiveCommand.Create(FocusTitle);

        if (_folderPath != null)
            LoadFiles();
        else
            _ = OpenFolder();
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; }
    public ReactiveCommand<Unit, Unit> NewCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; }
    public ReactiveCommand<Unit, Unit> StartNoteSearchCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelNoteSearchCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectNextNoteCommand { get; }
    public ReactiveCommand<Unit, Unit> SelectPreviousNoteCommand { get; }
    public ReactiveCommand<Unit, Unit> FocusEditorCommand { get; }
    public ReactiveCommand<Unit, Unit> FocusTitleCommand { get; }

    public string? FolderPath
    {
        get => _folderPath;
        set => this.RaiseAndSetIfChanged(ref _folderPath, value);
    }

    public Note? SelectedNote
    {
        get => _selectedNote;
        set => this.RaiseAndSetIfChanged(ref _selectedNote, value);
    }

    public bool IsNoteSearchActive
    {
        get => _isNoteSearchActive;
        set => this.RaiseAndSetIfChanged(ref _isNoteSearchActive, value);
    }

    public string? NoteSearchQuery
    {
        get => _noteSearchQuery;
        set => this.RaiseAndSetIfChanged(ref _noteSearchQuery, value);
    }

    public NotesCollection NotesCollection { get; }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        var hasUnsavedChanges = NotesCollection.HasUnsavedNotes;
        if (!hasUnsavedChanges)
            return;

        e.Cancel = true;

        var result = await _messageBoxService.ConfirmDiscardUnsavedChanges();

        if (result)
            _desktop.TryShutdown();
    }

    private void StartNoteSearch()
    {
        IsNoteSearchActive = true;
    }

    private void CancelNoteSearch()
    {
        IsNoteSearchActive = false;
    }

    private void SelectNextNote()
    {
        if (SelectedNote == null)
        {
            SelectedNote = NotesCollection.Notes.First();
            return;
        }

        var selectedIndex = NotesCollection.Notes.IndexOf(SelectedNote);
        if (selectedIndex == NotesCollection.Notes.Count - 1)
        {
            SelectedNote = NotesCollection.Notes.First();
            return;
        }

        SelectedNote = NotesCollection.Notes[selectedIndex + 1];
    }

    private void SelectPreviousNote()
    {
        if (SelectedNote == null)
        {
            SelectedNote = NotesCollection.Notes.Last();
            return;
        }
        
        var selectedIndex = NotesCollection.Notes.IndexOf(SelectedNote);
        if (selectedIndex == 0)
        {
            SelectedNote = NotesCollection.Notes.Last();
            return;
        }

        SelectedNote = NotesCollection.Notes[selectedIndex - 1];
    }

    private void FocusEditor()
    {
        if (SelectedNote != null)
        {
            _mainWindow.TextEditor.Focus();
        }
    }

    private void FocusTitle()
    {
        if (SelectedNote != null)
        {
            _mainWindow.TitleBox.Focus();
        }
    }

    private async Task Save()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;

        if (SelectedNote == null)
            return;

        await Save(SelectedNote);
    }

    private async Task SaveAll()
    {
        if (!NotesCollection.HasUnsavedNotes)
            return;
        
        foreach (var note in NotesCollection.Notes)
        {
            if (note.HasUnsavedChanges)
                await Save(note);
        }
    }

    private async Task Save(Note noteToSave)
    {
        if (noteToSave?.Document == null)
            return;

        if (!noteToSave.HasUnsavedChanges)
            return;

        if (!IsFilenameValid(noteToSave.Title))
        {
            await _messageBoxService.WarnForInvalidFilename();
            return;
        }

        if (NotesCollection.Notes.Any(x => x.OriginalTitle == noteToSave.Title && x != noteToSave))
        {
            await _messageBoxService.WarnForExistingFilename();
            return;
        }

        if (noteToSave.IsNew)
        {
            noteToSave.Path = Path.Join(FolderPath, noteToSave.Title + ".txt");
            noteToSave.OriginalTitle = noteToSave.Title;
        }

        await File.WriteAllTextAsync(noteToSave.Path, noteToSave.Document.Text);

        var isRenamed = noteToSave.OriginalTitle != noteToSave.Title;
        if (isRenamed)
        {
            // rename
            var fileInfo = new FileInfo(noteToSave.Path);
            fileInfo.MoveTo(Path.Join(fileInfo.Directory!.FullName, noteToSave.Title + ".txt"));
            noteToSave.OriginalTitle = noteToSave.Title;

            ReorderNotes();
        }

        noteToSave.HasUnsavedChanges = false;
        noteToSave.IsNew = false;
    }

    private void ReorderNotes()
    {
        NotesCollection.Sort();
    }

    private bool IsFilenameValid(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return false;

        const string invalidCharacters = "#%&{}\\$!'\":@<>*?/,`|=}";
        return invalidCharacters.All(character => !filename.Contains(character));
    }

    private void New()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;

        var newNote = new Note { OriginalTitle = "<new>", Document = new TextDocument() };

        NotesCollection.Add(newNote);

        SelectedNote = newNote;

        _mainWindow.TitleBox.SelectAll();
        _mainWindow.TitleBox.Focus();
    }

    private async Task Delete()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;

        if (SelectedNote == null)
            return;

        var isDeleted = await Delete(SelectedNote);
        if (isDeleted)
            SelectedNote = null;
    }

    private async Task<bool> Delete(Note noteToDelete)
    {
        var result = await _messageBoxService.ConfirmDelete(noteToDelete.OriginalTitle);
        if (result)
        {
            if (!noteToDelete.IsNew)
                File.Delete(noteToDelete.Path);

            NotesCollection.Remove(noteToDelete);
            return true;
        }

        return false;
    }

    private async Task OpenFolder()
    {
        var defaultFolder = !string.IsNullOrEmpty(FolderPath)
            ? await _mainWindow.StorageProvider.TryGetFolderFromPathAsync(new Uri(FolderPath))
            : null;
        var result = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            { SuggestedStartLocation = defaultFolder, AllowMultiple = false });

        if (result.Count != 1)
            return;

        var folderPath = result.First().TryGetLocalPath();

        if (string.IsNullOrEmpty(folderPath))
            return;

        FolderPath = folderPath;
        LoadFiles();
    }

    private void LoadFiles()
    {
        if (FolderPath == null)
            return;

        var directoryInfo = new DirectoryInfo(FolderPath);
        var files = directoryInfo.GetFiles("*.txt");
        var notes = files.Select(file =>
        {
            var title = Path.GetFileNameWithoutExtension(file.Name);
            return new Note(file.FullName, title);
        }).OrderBy(x => x.Title)
            .ToList();

        NotesCollection.SetItems(notes);
        SelectedNote = null;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SelectedNote):
                OnSelectedNoteChanged();
                break;
            case nameof(IsNoteSearchActive):
                OnIsNoteSearchActiveChanged();
                break;
            case nameof(NoteSearchQuery):
                OnNoteSearchQueryChanged();
                break;
        }
    }

    private void OnSelectedNoteChanged()
    {
        if (SelectedNote == null)
            return;

        if (SelectedNote.Document != null)
            return;

        var allText = File.ReadAllText(SelectedNote.Path);
        SelectedNote.Document = new TextDocument(allText);
    }

    private void OnIsNoteSearchActiveChanged()
    {
        if (IsNoteSearchActive)
        {
            Dispatcher.UIThread.Post(() => _mainWindow.NotesSearchTextBox.Focus());
        }
        else
        {
            NoteSearchQuery = null;
        }
    }

    private void OnNoteSearchQueryChanged()
    {
        if (string.IsNullOrEmpty(NoteSearchQuery))
        {
            NotesCollection.Filter(null);
        }
        else
        {
            NotesCollection.Filter(NoteSearchQuery);
        }
    }
}