using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Platform;
using Avalonia.VisualTree;
using Diffy.Models;
using Diffy.ViewModels;

namespace Diffy.Views;

public partial class MainWindow : Window
{
    private bool _isDragging;
    private int _dragStartIndex = -1;
    private ListBox? _activeListBox;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Set storage provider and clipboard when the window is opened
        Opened += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.StorageProvider = StorageProvider;
                viewModel.Clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
            }
        };
        
        // Wire up selection changed events for copy functionality
        LeftDiffList.SelectionChanged += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedLeftLines = LeftDiffList.SelectedItems?
                    .OfType<DiffLine>()
                    .ToList() ?? [];
            }
        };
        
        RightDiffList.SelectionChanged += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.SelectedRightLines = RightDiffList.SelectedItems?
                    .OfType<DiffLine>()
                    .ToList() ?? [];
            }
        };
        
        // Enable drag-to-select for both diff lists
        SetupDragSelection(LeftDiffList);
        SetupDragSelection(RightDiffList);
    }
    
    private void SetupDragSelection(ListBox listBox)
    {
        listBox.PointerPressed += OnListBoxPointerPressed;
        listBox.PointerMoved += OnListBoxPointerMoved;
        listBox.PointerReleased += OnListBoxPointerReleased;
        listBox.PointerCaptureLost += OnListBoxPointerCaptureLost;
    }
    
    private void OnListBoxPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not ListBox listBox) return;
        
        var point = e.GetCurrentPoint(listBox);
        if (!point.Properties.IsLeftButtonPressed) return;
        
        var index = GetItemIndexAtPoint(listBox, e);
        if (index < 0) return;
        
        _isDragging = true;
        _dragStartIndex = index;
        _activeListBox = listBox;
        
        // Capture pointer for drag tracking
        e.Pointer.Capture(listBox);
        
        // Clear previous selection and select the starting item
        listBox.SelectedItems?.Clear();
        listBox.SelectedIndex = index;
    }
    
    private void OnListBoxPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_isDragging || sender is not ListBox listBox || listBox != _activeListBox) return;
        
        var currentIndex = GetItemIndexAtPoint(listBox, e);
        if (currentIndex < 0) return;
        
        // Select range from start to current
        SelectRange(listBox, _dragStartIndex, currentIndex);
    }
    
    private void OnListBoxPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            e.Pointer.Capture(null);
        }
        EndDrag();
    }
    
    private void OnListBoxPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        EndDrag();
    }
    
    private void EndDrag()
    {
        _isDragging = false;
        _dragStartIndex = -1;
        _activeListBox = null;
    }
    
    private int GetItemIndexAtPoint(ListBox listBox, PointerEventArgs e)
    {
        var position = e.GetPosition(listBox);
        
        // Find the ListBoxItem at the pointer position
        var hitElement = listBox.InputHitTest(position);
        if (hitElement == null) return -1;
        
        // Walk up the visual tree to find the ListBoxItem
        var visual = hitElement as Avalonia.Visual;
        while (visual != null)
        {
            if (visual is ListBoxItem item)
            {
                return listBox.IndexFromContainer(item);
            }
            visual = visual.GetVisualParent() as Avalonia.Visual;
        }
        
        return -1;
    }
    
    private void SelectRange(ListBox listBox, int startIndex, int endIndex)
    {
        var minIndex = Math.Min(startIndex, endIndex);
        var maxIndex = Math.Max(startIndex, endIndex);
        
        listBox.SelectedItems?.Clear();
        
        for (int i = minIndex; i <= maxIndex && i < listBox.ItemCount; i++)
        {
            var item = listBox.Items[i];
            if (item != null)
            {
                listBox.SelectedItems?.Add(item);
            }
        }
    }
}