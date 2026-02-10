using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input.Platform;
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
    
    /// <summary>
    /// Gets the application version from the assembly.
    /// </summary>
    public static string AppVersion => Assembly.GetExecutingAssembly()
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) 
        ?? "1.0.0";
    
    /// <summary>
    /// Gets the window title including version.
    /// </summary>
    public string WindowTitle => $"Diffy v{AppVersion} - File Comparison Tool";
    
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
    
    // Clipboard will be set from the View
    public IClipboard? Clipboard { get; set; }
    
    // Selected lines for copy functionality (set from View)
    public IList<DiffLine> SelectedLeftLines { get; set; } = new List<DiffLine>();
    public IList<DiffLine> SelectedRightLines { get; set; } = new List<DiffLine>();
    
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
    
    [RelayCommand]
    private async Task CopyLeft()
    {
        await CopyToClipboardAsync(LeftText);
        StatusMessage = "Left text copied to clipboard";
    }
    
    [RelayCommand]
    private async Task CopyRight()
    {
        await CopyToClipboardAsync(RightText);
        StatusMessage = "Right text copied to clipboard";
    }
    
    [RelayCommand]
    private async Task CopyUnifiedDiff()
    {
        if (LeftDiffLines.Count == 0 && RightDiffLines.Count == 0)
        {
            StatusMessage = "No diff to copy - run Compare first";
            return;
        }
        
        var unifiedDiff = GenerateUnifiedDiff();
        await CopyToClipboardAsync(unifiedDiff);
        StatusMessage = "Unified diff copied to clipboard";
    }
    
    [RelayCommand]
    private async Task CopySelectedLeftLines()
    {
        await CopySelectedLinesAsync(SelectedLeftLines, "left");
    }
    
    [RelayCommand]
    private async Task CopySelectedRightLines()
    {
        await CopySelectedLinesAsync(SelectedRightLines, "right");
    }
    
    [RelayCommand]
    private async Task CopyLine(DiffLine? line)
    {
        if (line == null || string.IsNullOrEmpty(line.Text))
        {
            StatusMessage = "No text to copy";
            return;
        }
        
        await CopyToClipboardAsync(line.Text);
        StatusMessage = "Line copied to clipboard";
    }
    
    [RelayCommand]
    private async Task CopyLineWithNumber(DiffLine? line)
    {
        if (line == null)
        {
            StatusMessage = "No line to copy";
            return;
        }
        
        var text = line.LineNumber.HasValue 
            ? $"{line.LineNumber}: {line.Text}" 
            : line.Text;
        await CopyToClipboardAsync(text);
        StatusMessage = "Line with number copied to clipboard";
    }
    
    private async Task CopySelectedLinesAsync(IList<DiffLine> selectedLines, string side)
    {
        if (selectedLines.Count == 0)
        {
            StatusMessage = "No lines selected";
            return;
        }
        
        var text = string.Join(Environment.NewLine, 
            selectedLines.OrderBy(l => l.LineNumber ?? int.MaxValue)
                         .Select(l => l.Text));
        await CopyToClipboardAsync(text);
        
        var lineWord = selectedLines.Count == 1 ? "line" : "lines";
        StatusMessage = $"{selectedLines.Count} {side} {lineWord} copied to clipboard";
    }
    
    private string GenerateUnifiedDiff()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"--- {LeftFilePath}");
        sb.AppendLine($"+++ {RightFilePath}");
        
        // Process left side for deletions and unchanged
        foreach (var line in LeftDiffLines)
        {
            switch (line.Type)
            {
                case DiffType.Deleted:
                    sb.AppendLine($"-{line.Text}");
                    break;
                case DiffType.Modified:
                    sb.AppendLine($"-{line.Text}");
                    break;
                case DiffType.Unchanged:
                    sb.AppendLine($" {line.Text}");
                    break;
            }
        }
        
        // Append insertions and modifications from right side
        sb.AppendLine();
        foreach (var line in RightDiffLines)
        {
            switch (line.Type)
            {
                case DiffType.Inserted:
                    sb.AppendLine($"+{line.Text}");
                    break;
                case DiffType.Modified:
                    sb.AppendLine($"+{line.Text}");
                    break;
            }
        }
        
        return sb.ToString().TrimEnd();
    }
    
    private async Task CopyToClipboardAsync(string text)
    {
        if (Clipboard != null)
        {
            await Clipboard.SetTextAsync(text);
        }
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
