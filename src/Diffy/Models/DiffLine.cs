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

/// <summary>
/// Represents a segment of text within a line with its own diff type.
/// Used for character-level highlighting.
/// </summary>
public class DiffSegment
{
    public string Text { get; set; } = string.Empty;
    public DiffType Type { get; set; }
    
    public DiffSegment(string text, DiffType type)
    {
        Text = text;
        Type = type;
    }
}

public class DiffLine
{
    public int? LineNumber { get; set; }
    public string Text { get; set; } = string.Empty;
    public DiffType Type { get; set; }
    
    /// <summary>
    /// Character-level segments for inline diff highlighting.
    /// Only populated for Modified lines.
    /// </summary>
    public List<DiffSegment> Segments { get; set; } = new();
    
    /// <summary>
    /// Returns true if this line has character-level diff segments.
    /// </summary>
    public bool HasSegments => Segments.Count > 0;
    
    public DiffLine(int? lineNumber, string text, DiffType type)
    {
        LineNumber = lineNumber;
        Text = text;
        Type = type;
    }
    
    public DiffLine(int? lineNumber, string text, DiffType type, List<DiffSegment> segments)
    {
        LineNumber = lineNumber;
        Text = text;
        Type = type;
        Segments = segments;
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
