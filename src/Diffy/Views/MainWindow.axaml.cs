using Avalonia.Controls;
using Diffy.ViewModels;

namespace Diffy.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        // Set storage provider when the window is opened
        Opened += (sender, args) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.StorageProvider = StorageProvider;
            }
        };
    }
}