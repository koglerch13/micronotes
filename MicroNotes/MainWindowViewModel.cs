using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using DynamicData.Binding;
using MicroNotes.MessageBox;
using ReactiveUI;

namespace MicroNotes;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly IMessageBoxService _messageBoxService;
    private readonly MainWindow _mainWindow;
    private string? _folderPath;

    private ObservableCollectionExtended<Note> _notes = new();
    private Note? _selectedNote;

    public MainWindowViewModel(MainWindow mainWindow, IClassicDesktopStyleApplicationLifetime desktop,
        IMessageBoxService messageBoxService)
    {
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

        if (_folderPath != null)
            LoadFiles();
        else
            _ = OpenFolder();
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
    public ReactiveCommand<Unit, Unit> SaveAllCommand { get; set; }
    public ReactiveCommand<Unit, Unit> NewCommand { get; set; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; set; }
    public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; set; }

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

    public ObservableCollectionExtended<Note> Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private async void OnClosing(object? sender, CancelEventArgs e)
    {
        var hasUnsavedChanges = Notes.Any(x => x.HasUnsavedChanges);
        if (!hasUnsavedChanges)
            return;

        e.Cancel = true;

        var result = await _messageBoxService.ConfirmDiscardUnsavedChanges();

        if (result)
            _desktop.TryShutdown();
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

        if (Notes.Any(x => x.OriginalTitle == noteToSave.Title && x != noteToSave))
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
    }

    private void ReorderNotes()
    {
        Notes = new ObservableCollectionExtended<Note>(Notes.OrderBy(x => x.OriginalTitle));
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

        Notes.Add(newNote);

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

            Notes.Remove(noteToDelete);
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
        }).OrderBy(x => x.Title);

        Notes = new ObservableCollectionExtended<Note>(notes);
        SelectedNote = null;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedNote))
            OnSelectedNoteChanged();
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
}