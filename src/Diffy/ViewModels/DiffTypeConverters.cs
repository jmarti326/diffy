using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Diffy.Models;

namespace Diffy.ViewModels;

public static class DiffTypeConverters
{
    public static FuncValueConverter<DiffType, bool> IsUnchanged { get; } = 
        new(type => type == DiffType.Unchanged);
    
    public static FuncValueConverter<DiffType, bool> IsInserted { get; } = 
        new(type => type == DiffType.Inserted);
    
    public static FuncValueConverter<DiffType, bool> IsDeleted { get; } = 
        new(type => type == DiffType.Deleted);
    
    public static FuncValueConverter<DiffType, bool> IsModified { get; } = 
        new(type => type == DiffType.Modified);
    
    public static FuncValueConverter<DiffType, bool> IsImaginary { get; } = 
        new(type => type == DiffType.Imaginary);
}
