using System;
using System.Linq;
using Diffy.Models;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace Diffy.Services;

public interface IDiffService
{
    DiffResult ComputeDiff(string leftText, string rightText);
}

public class DiffService : IDiffService
{
    private readonly ISideBySideDiffBuilder _diffBuilder;

    public DiffService()
    {
        _diffBuilder = new SideBySideDiffBuilder(new Differ());
    }

    public DiffResult ComputeDiff(string leftText, string rightText)
    {
        var diff = _diffBuilder.BuildDiffModel(leftText ?? "", rightText ?? "");
        var result = new DiffResult();
        
        int leftLineNum = 1;
        int rightLineNum = 1;

        // Process both sides in parallel
        int maxLines = Math.Max(diff.OldText.Lines.Count, diff.NewText.Lines.Count);
        
        for (int i = 0; i < diff.OldText.Lines.Count; i++)
        {
            var oldLine = diff.OldText.Lines[i];
            var newLine = i < diff.NewText.Lines.Count ? diff.NewText.Lines[i] : null;
            
            // Left side
            var leftDiffType = ConvertChangeType(oldLine.Type);
            result.LeftLines.Add(new DiffLine(
                oldLine.Type != ChangeType.Imaginary ? leftLineNum++ : null,
                oldLine.Text ?? "",
                leftDiffType
            ));
            
            // Right side
            if (newLine != null)
            {
                var rightDiffType = ConvertChangeType(newLine.Type);
                result.RightLines.Add(new DiffLine(
                    newLine.Type != ChangeType.Imaginary ? rightLineNum++ : null,
                    newLine.Text ?? "",
                    rightDiffType
                ));
            }
            
            // Update statistics
            switch (oldLine.Type)
            {
                case ChangeType.Deleted:
                    result.DeletedCount++;
                    break;
                case ChangeType.Modified:
                    result.ModifiedCount++;
                    break;
                case ChangeType.Unchanged:
                    result.UnchangedCount++;
                    break;
            }
        }
        
        // Handle any remaining new lines
        for (int i = diff.OldText.Lines.Count; i < diff.NewText.Lines.Count; i++)
        {
            var newLine = diff.NewText.Lines[i];
            result.LeftLines.Add(new DiffLine(null, "", DiffType.Imaginary));
            result.RightLines.Add(new DiffLine(
                newLine.Type != ChangeType.Imaginary ? rightLineNum++ : null,
                newLine.Text ?? "",
                ConvertChangeType(newLine.Type)
            ));
        }

        // Count insertions from right side
        result.InsertedCount = diff.NewText.Lines.Count(l => l.Type == ChangeType.Inserted);

        return result;
    }

    private static DiffType ConvertChangeType(ChangeType changeType)
    {
        return changeType switch
        {
            ChangeType.Unchanged => DiffType.Unchanged,
            ChangeType.Deleted => DiffType.Deleted,
            ChangeType.Inserted => DiffType.Inserted,
            ChangeType.Modified => DiffType.Modified,
            ChangeType.Imaginary => DiffType.Imaginary,
            _ => DiffType.Unchanged
        };
    }
}
