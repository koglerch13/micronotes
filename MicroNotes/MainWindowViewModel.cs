using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaEdit.Document;
using DynamicData.Binding;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace MicroNotes;

// TODO:

// Fix:
// Style editor search

// Later:
// Save all 
// Go to note by title shortcut
// Delete note with context menu

// Maybe:
// Persist window size & separator width ?
// Persist folder ? 
// Theme support ?

// Cannot do:
// on mac: open note, go into text area, go into title box -> crash?
// Closing on mac hangs --> seems to be an avalonia issue

public class MainWindowViewModel : ReactiveObject
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly MainWindow _mainWindow;
    private string? _folderPath;

    private ObservableCollectionExtended<Note> _notes = new();
    private Note? _selectedNote;
    
    public MainWindowViewModel(MainWindow mainWindow, IClassicDesktopStyleApplicationLifetime desktop)
    {
        _mainWindow = mainWindow;
        _desktop = desktop;
        _mainWindow.Closing += OnClosing;
        PropertyChanged += OnPropertyChanged;

        SaveCommand = ReactiveCommand.CreateFromTask(Save);
        NewCommand = ReactiveCommand.Create(New);
        DeleteCommand = ReactiveCommand.CreateFromTask(Delete);
        OpenFolderCommand = ReactiveCommand.CreateFromTask(OpenFolder);

        if (_folderPath != null)
            LoadFiles();
        else
            _ = OpenFolder();
    }

    public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }
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

        var result = await MessageBoxManager.GetMessageBoxStandard("Unsaved changes",
                "You have unsaved changes. These will be lost if you close the application now.\nDo you really want to quit?",
                ButtonEnum.YesNo, Icon.None, WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);
        
        if (result == ButtonResult.Yes)
            _desktop.TryShutdown();
    }

    private async Task Save()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;
        
        if (SelectedNote?.Document == null)
            return;

        if (!SelectedNote.HasUnsavedChanges)
            return;

        if (!IsFilenameValid(SelectedNote.Title))
        {
            await MessageBoxManager.GetMessageBoxStandard("Invalid file name.",
                    "The file name is invalid (maybe it contains invalid characters?)", ButtonEnum.Ok, Icon.None, WindowStartupLocation.CenterOwner)
                .ShowWindowDialogAsync(_mainWindow);
            
            return;
        }

        if (Notes.Any(x => x.OriginalTitle == SelectedNote.Title && x != SelectedNote))
        {
            await MessageBoxManager.GetMessageBoxStandard("File already exists.",
                    "A file with this title already exists. Please choose another title.", ButtonEnum.Ok, Icon.None, WindowStartupLocation.CenterOwner)
                .ShowWindowDialogAsync(_mainWindow);
            
            return;
        }

        if (SelectedNote.IsNew)
        {
            SelectedNote.Path = Path.Join(FolderPath, SelectedNote.Title + ".txt");
            SelectedNote.OriginalTitle = SelectedNote.Title;
        }

        await File.WriteAllTextAsync(SelectedNote.Path, SelectedNote.Document.Text);

        if (SelectedNote.OriginalTitle != SelectedNote.Title)
        {
            // rename
            var fileInfo = new FileInfo(SelectedNote.Path);
            fileInfo.MoveTo(Path.Join(fileInfo.Directory!.FullName, SelectedNote.Title + ".txt"));
            SelectedNote.OriginalTitle = SelectedNote.Title;
        }

        SelectedNote.HasUnsavedChanges = false;
        
        ReorderNotes();
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

        _mainWindow.TitleBox.Focus();
    }

    private async Task Delete()
    {
        if (string.IsNullOrEmpty(FolderPath))
            return;
        
        if (SelectedNote == null)
            return;

        var result = await MessageBoxManager.GetMessageBoxStandard("Delete?",
                $"Do you want to delete the note called '{SelectedNote.OriginalTitle}'?", ButtonEnum.YesNo,
                Icon.None, WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);

        if (result == ButtonResult.Yes)
        {
            if (!SelectedNote.IsNew) File.Delete(SelectedNote.Path);

            Notes.Remove(SelectedNote);
            SelectedNote = null;
        }
    }

    private async Task OpenFolder()
    {
        var defaultFolder = !string.IsNullOrEmpty(FolderPath) ? await _mainWindow.StorageProvider.TryGetFolderFromPathAsync(new Uri(FolderPath)) : null;
        var result = await _mainWindow.StorageProvider.OpenFolderPickerAsync(new Avalonia.Platform.Storage.FolderPickerOpenOptions { SuggestedStartLocation = defaultFolder, AllowMultiple = false });

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