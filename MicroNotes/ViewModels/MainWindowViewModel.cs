using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaEdit.Document;
using DynamicData.Binding;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;

namespace MicroNotes.ViewModels;

// TODO:
// Persist folder
// Save all
// Persist window size & separator width
// Add new note -> save -> should be ordered
// Add new note -> focus automatically in title
// Go to note by title shortcut

public class MainWindowViewModel : ViewModelBase
{
    private readonly IClassicDesktopStyleApplicationLifetime _desktop;
    private readonly Window _mainWindow;
    private string? _folderPath;

    private ObservableCollectionExtended<Note> _notes = new();
    private Note? _selectedNote;

    public MainWindowViewModel(Window mainWindow, IClassicDesktopStyleApplicationLifetime desktop)
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
        if (!hasUnsavedChanges) return;

        e.Cancel = true;

        var result = await MessageBoxManager.GetMessageBoxStandard("Unsaved changes",
                "You have unsaved changes. These will be lost if you close the application now.\nDo you really want to quit?",
                ButtonEnum.YesNo, Icon.Question, WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);
        if (result == ButtonResult.Yes) _desktop.Shutdown();
    }

    private async Task Save()
    {
        if (SelectedNote?.Document == null) return;

        if (string.IsNullOrEmpty(SelectedNote.Title)) return;

        if (Notes.Any(x => x.OriginalTitle == SelectedNote.Title && x != SelectedNote))
        {
            await MessageBoxManager.GetMessageBoxStandard("File already exists.",
                    "A file with this title already exists. Please choose another title.", ButtonEnum.Ok, Icon.Error, WindowStartupLocation.CenterOwner)
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
    }

    private void New()
    {
        var newNote = new Note { OriginalTitle = "<new>", Document = new TextDocument() };

        Notes.Add(newNote);

        SelectedNote = newNote;
    }

    private async Task Delete()
    {
        if (SelectedNote == null) return;

        var result = await MessageBoxManager.GetMessageBoxStandard("Delete?",
                $"Do you want to delete the note called '{SelectedNote.OriginalTitle}'?", ButtonEnum.YesNo,
                Icon.Question, WindowStartupLocation.CenterOwner)
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
        var dialog = new OpenFolderDialog { Directory = FolderPath };

        var result = await dialog.ShowAsync(_mainWindow);

        if (string.IsNullOrEmpty(result)) return;

        FolderPath = result;
        LoadFiles();
    }

    private void LoadFiles()
    {
        if (FolderPath == null) return;

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
        if (e.PropertyName == nameof(SelectedNote)) OnSelectedNoteChanged();
    }

    private void OnSelectedNoteChanged()
    {
        if (SelectedNote == null) return;

        if (SelectedNote.Document != null) return;

        var allText = File.ReadAllText(SelectedNote.Path);
        SelectedNote.Document = new TextDocument(allText);
    }
}