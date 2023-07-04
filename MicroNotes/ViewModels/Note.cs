using System;
using System.ComponentModel;
using AvaloniaEdit.Document;
using ReactiveUI;

namespace MicroNotes.ViewModels;

public class Note : ViewModelBase
{
    private TextDocument? _document;
    private bool _hasUnsavedChanges;
    private bool _isNew;
    private string _originalTitle;
    private string _path;
    private string _title;

    public Note(string path, string title)
    {
        _path = path;
        _title = title;
        _originalTitle = title;
        _isNew = false;

        PropertyChanged += OnPropertyChanged;
    }

    public Note()
    {
        _path = "";
        _originalTitle = "<new>";
        _title = "";
        _isNew = true;
    }

    public string Path
    {
        get => _path;
        set => this.RaiseAndSetIfChanged(ref _path, value);
    }

    public string Title
    {
        get => _title;
        set => this.RaiseAndSetIfChanged(ref _title, value);
    }

    public string OriginalTitle
    {
        get => _originalTitle;
        set => this.RaiseAndSetIfChanged(ref _originalTitle, value);
    }

    public bool IsNew
    {
        get => _isNew;
        set => this.RaiseAndSetIfChanged(ref _isNew, value);
    }

    public TextDocument? Document
    {
        get => _document;
        set
        {
            if (_document != null)
                _document.TextChanged -= DocumentTextChanged;

            if (value != null)
                value.TextChanged += DocumentTextChanged;

            this.RaiseAndSetIfChanged(ref _document, value);
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => this.RaiseAndSetIfChanged(ref _hasUnsavedChanges, value);
    }

    private void DocumentTextChanged(object? sender, EventArgs e)
    {
        HasUnsavedChanges = true;
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Title)) HasUnsavedChanges = true;
    }
}