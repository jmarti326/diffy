using System.Collections.Generic;

namespace Diffy.Models;

public enum DiffType
{
    Unchanged,
    Inserted,
    Deleted,
    Modified,
    Imaginary  // Placeholder for alignment
}

public class DiffLine
{
    public int? LineNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public DiffType Type { get; set; }
    
    public DiffLine(int? lineNumber, string text, DiffType type)
    {
        LineNumber = lineNumber;
        Text = text;
        Type = type;
    }
}

public class DiffResult
{
    public List<DiffLine> LeftLines { get; set; } = new();
    public List<DiffLine> RightLines { get; set; } = new();
    public int InsertedCount { get; set; }
    public int DeletedCount { get; set; }
    public int ModifiedCount { get; set; }
    public int UnchangedCount { get; set; }
}
