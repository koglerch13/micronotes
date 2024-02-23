using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using MicroNotes.MessageBox;
using MicroNotes.UpdateManager;
using Microsoft.Extensions.Logging;
using ReactiveUI;

namespace MicroNotes;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly IMessageBoxService _messageBoxService;
    private readonly MainWindow _mainWindow;
    private readonly ILogger _logger;
    private readonly IUpdateManagerService _updateManagerService;
    private string? _folderPath;

    private bool _isNoteSearchActive;
    private string? _noteSearchQuery;

    public MainWindowViewModel(MainWindow mainWindow,
        IClassicDesktopStyleApplicationLifetime desktop,
        IMessageBoxService messageBoxService,
        ILogger logger,
        IUpdateManagerService updateManagerService)
    {
        NotesCollection = new NotesCollection();

        _messageBoxService = messageBoxService;
        _mainWindow = mainWindow;
        _desktop = desktop;
        _logger = logger;
        _updateManagerService = updateManagerService;
        
        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        SaveAllCommand = ReactiveCommand.CreateFromTask(SaveAll);
        NewCommand = ReactiveCommand.Create(New);
        DeleteCommand = ReactiveCommand.CreateFromTask(Delete);
        OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);
        StartNoteSearchCommand = ReactiveCommand.Create(StartNoteSearch);
        CancelNoteSearchCommand = ReactiveCommand.Create(CancelNoteSearch);
        SelectNextNoteCommand = ReactiveCommand.Create(NotesCollection.SelectNextNote);
        SelectPreviousNoteCommand = ReactiveCommand.Create(NotesCollection.SelectPreviousNote);
        FocusEditorCommand = ReactiveCommand.Create(FocusEditor);
        FocusTitleCommand = ReactiveCommand.Create(FocusTitle);
        
        _mainWindow.Closing += OnClosing;
        PropertyChanged += OnPropertyChanged;
        NotesCollection.PropertyChanged += OnNotesCollectionPropertyChanged;
        _mainWindow.NotesSearchTextBox.LostFocus += OnNotesSearchTextBoxFocusLost;
        
        if (_folderPath != null)
            LoadFiles();
        else
            _ = OpenFolder();

        _ = _updateManagerService.TryUpdate();
    }
    
    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(NoteSearchQuery):
                OnNoteSearchQueryChanged();
                break;
        }
    }
    
    private void OnNotesCollectionPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NotesCollection.SelectedNote))
            OnSelectedNoteChanged();
    }
    
    private void OnNotesSearchTextBoxFocusLost(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(NoteSearchQuery))
            IsNoteSearchActive = false;
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
        Dispatcher.UIThread.Post(() => _mainWindow.NotesSearchTextBox.Focus());
    }

    private void CancelNoteSearch()
    {
        IsNoteSearchActive = false;
        NoteSearchQuery = null;
    }

    private void FocusEditor()
    {
        if (NotesCollection.SelectedNote != null)
        {
            _mainWindow.TextEditor.Focus();
        }
    }

    private void FocusTitle()
    {
        if (NotesCollection.SelectedNote != null)
        {
            _mainWindow.TitleBox.Focus();
        }
    }

    private async Task Save()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;

        if (NotesCollection.SelectedNote == null)
            return;

        await Save(NotesCollection.SelectedNote);
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

        NotesCollection.SelectedNote = newNote;

        _mainWindow.TitleBox.SelectAll();
        _mainWindow.TitleBox.Focus();
    }

    private async Task Delete()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;

        if (NotesCollection.SelectedNote == null)
            return;

        var isDeleted = await Delete(NotesCollection.SelectedNote);
        if (isDeleted)
            NotesCollection.SelectedNote = null;
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
        NotesCollection.SelectedNote = null;
    }
    
    private void OnSelectedNoteChanged()
    {
        if (NotesCollection.SelectedNote == null)
            return;

        if (NotesCollection.SelectedNote.Document != null)
            return;

        var allText = File.ReadAllText(NotesCollection.SelectedNote.Path);
        NotesCollection.SelectedNote.Document = new TextDocument(allText);
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