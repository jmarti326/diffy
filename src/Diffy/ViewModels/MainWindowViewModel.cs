using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Diffy.Models;
using Diffy.Services;

namespace Diffy.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IDiffService _diffService;
    
    [ObservableProperty]
    private string _leftText = "";
    
    [ObservableProperty]
    private string _rightText = "";
    
    [ObservableProperty]
    private string _leftFilePath = "Untitled";
    
    [ObservableProperty]
    private string _rightFilePath = "Untitled";
    
    [ObservableProperty]
    private string _statusMessage = "Ready - Enter text or load files to compare";
    
    [ObservableProperty]
    private ObservableCollection<DiffLine> _leftDiffLines = new();
    
    [ObservableProperty]
    private ObservableCollection<DiffLine> _rightDiffLines = new();
    
    [ObservableProperty]
    private int _insertedCount;
    
    [ObservableProperty]
    private int _deletedCount;
    
    [ObservableProperty]
    private int _modifiedCount;
    
    // Storage provider will be set from the View
    public IStorageProvider? StorageProvider { get; set; }
    
    public MainWindowViewModel()
    {
        _diffService = new DiffService();
    }
    
    [RelayCommand]
    private void Compare()
    {
        var result = _diffService.ComputeDiff(LeftText, RightText);
        
        LeftDiffLines.Clear();
        RightDiffLines.Clear();
        
        foreach (var line in result.LeftLines)
            LeftDiffLines.Add(line);
            
        foreach (var line in result.RightLines)
            RightDiffLines.Add(line);
        
        InsertedCount = result.InsertedCount;
        DeletedCount = result.DeletedCount;
        ModifiedCount = result.ModifiedCount;
        
        StatusMessage = $"Compared: {result.InsertedCount} inserted, {result.DeletedCount} deleted, {result.ModifiedCount} modified, {result.UnchangedCount} unchanged";
    }
    
    [RelayCommand]
    private async Task LoadLeftFile()
    {
        var file = await PickFileAsync();
        if (file != null)
        {
            LeftFilePath = file.Name;
            await using var stream = await file.OpenReadAsync();
            using var reader = new System.IO.StreamReader(stream);
            LeftText = await reader.ReadToEndAsync();
            Compare();
        }
    }
    
    [RelayCommand]
    private async Task LoadRightFile()
    {
        var file = await PickFileAsync();
        if (file != null)
        {
            RightFilePath = file.Name;
            await using var stream = await file.OpenReadAsync();
            using var reader = new System.IO.StreamReader(stream);
            RightText = await reader.ReadToEndAsync();
            Compare();
        }
    }
    
    [RelayCommand]
    private void Clear()
    {
        LeftText = "";
        RightText = "";
        LeftFilePath = "Untitled";
        RightFilePath = "Untitled";
        LeftDiffLines.Clear();
        RightDiffLines.Clear();
        InsertedCount = 0;
        DeletedCount = 0;
        ModifiedCount = 0;
        StatusMessage = "Ready - Enter text or load files to compare";
    }
    
    [RelayCommand]
    private void SwapSides()
    {
        (LeftText, RightText) = (RightText, LeftText);
        (LeftFilePath, RightFilePath) = (RightFilePath, LeftFilePath);
        Compare();
    }
    
    [RelayCommand]
    private void ToggleTheme()
    {
        var app = Application.Current;
        if (app == null) return;
        
        // Toggle between Light and Dark themes
        app.RequestedThemeVariant = app.ActualThemeVariant == ThemeVariant.Dark 
            ? ThemeVariant.Light 
            : ThemeVariant.Dark;
    }
    
    private async Task<IStorageFile?> PickFileAsync()
    {
        if (StorageProvider == null)
            return null;
            
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a file to compare",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("All Files") { Patterns = new[] { "*" } },
                new FilePickerFileType("Text Files") { Patterns = new[] { "*.txt", "*.md", "*.json", "*.xml", "*.cs", "*.js", "*.ts", "*.py", "*.html", "*.css" } }
            }
        });
        
        return files.Count > 0 ? files[0] : null;
    }
    
    partial void OnLeftTextChanged(string value)
    {
        // Auto-compare on text change if both have content
        if (!string.IsNullOrEmpty(LeftText) && !string.IsNullOrEmpty(RightText))
        {
            // Debounce could be added here for performance
        }
    }
    
    partial void OnRightTextChanged(string value)
    {
        // Auto-compare on text change if both have content
        if (!string.IsNullOrEmpty(LeftText) && !string.IsNullOrEmpty(RightText))
        {
            // Debounce could be added here for performance
        }
    }
}
