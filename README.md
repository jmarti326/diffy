# Diffy - Cross-Platform File Comparison Tool

A minimalistic file and text comparison application built with .NET and Avalonia UI. Similar to Beyond Compare, it provides side-by-side diff visualization.

## Features

- **Cross-Platform**: Runs natively on Linux, Windows, and macOS
- **Side-by-Side Comparison**: View differences in a split-pane interface
- **Syntax Highlighting**: Visual markers for insertions, deletions, and modifications
- **File Loading**: Open files directly from your file system
- **Text Input**: Paste or type text directly for comparison
- **Statistics**: View count of inserted, deleted, and modified lines
- **Swap Sides**: Quickly swap left and right content

## Technology Stack

- **.NET 10** - Runtime and SDK
- **Avalonia UI** - Cross-platform XAML UI framework
- **DiffPlex** - Text diff algorithm implementation
- **CommunityToolkit.Mvvm** - MVVM pattern support

## Requirements

- .NET 10 SDK or later

## Building

```bash
cd ~/source/diffy
dotnet build
```

## Running

```bash
dotnet run --project src/Diffy
```

## Publishing

Create a self-contained executable for your platform:

```bash
# Linux
dotnet publish src/Diffy -c Release -r linux-x64 --self-contained

# Windows
dotnet publish src/Diffy -c Release -r win-x64 --self-contained

# macOS (Intel)
dotnet publish src/Diffy -c Release -r osx-x64 --self-contained

# macOS (Apple Silicon)
dotnet publish src/Diffy -c Release -r osx-arm64 --self-contained
```

## Usage

1. **Load Files**: Click "Load Left" or "Load Right" to open files
2. **Paste Text**: Directly paste or type text in either panel
3. **Compare**: Click "Compare" to see differences highlighted
4. **Swap**: Use the "Swap" button to exchange left and right content
5. **Clear**: Reset both panels and start fresh

## Color Coding

| Color | Meaning |
|-------|---------|
| 🟢 Green | Inserted lines (new in right) |
| 🔴 Red | Deleted lines (removed from left) |
| 🟠 Orange | Modified lines |
| ⬜ Gray | Placeholder for alignment |

## License

This project is licensed under the [MIT License](LICENSE) - free to use, modify, and distribute.
