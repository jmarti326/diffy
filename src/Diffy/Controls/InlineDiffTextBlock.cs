using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Diffy.Models;

namespace Diffy.Controls;

/// <summary>
/// A custom control that renders text with character-level diff highlighting.
/// Similar to Beyond Compare's inline diff highlighting.
/// </summary>
public class InlineDiffTextBlock : Control
{
    // Styled Property for DiffLine binding
    public static readonly StyledProperty<DiffLine?> DiffLineProperty =
        AvaloniaProperty.Register<InlineDiffTextBlock, DiffLine?>(nameof(DiffLine));

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        AvaloniaProperty.Register<InlineDiffTextBlock, FontFamily>(nameof(FontFamily), FontFamily.Default);

    public static readonly StyledProperty<double> FontSizeProperty =
        AvaloniaProperty.Register<InlineDiffTextBlock, double>(nameof(FontSize), 12);

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        AvaloniaProperty.Register<InlineDiffTextBlock, IBrush?>(nameof(Foreground));

    public DiffLine? DiffLine
    {
        get => GetValue(DiffLineProperty);
        set => SetValue(DiffLineProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    // Colors for highlighting - more visible than line-level backgrounds
    private static readonly IBrush UnchangedBackground = Brushes.Transparent;
    private static readonly IBrush InsertedBackground = new SolidColorBrush(Color.FromArgb(120, 0, 180, 0));
    private static readonly IBrush DeletedBackground = new SolidColorBrush(Color.FromArgb(120, 255, 80, 80));
    private static readonly IBrush ModifiedBackground = new SolidColorBrush(Color.FromArgb(140, 255, 200, 0));

    static InlineDiffTextBlock()
    {
        AffectsRender<InlineDiffTextBlock>(DiffLineProperty, FontFamilyProperty, FontSizeProperty, ForegroundProperty);
        AffectsMeasure<InlineDiffTextBlock>(DiffLineProperty, FontFamilyProperty, FontSizeProperty);
    }

    private IBrush GetForegroundBrush()
    {
        // Use set Foreground, or detect theme and pick appropriate color
        if (Foreground != null)
            return Foreground;
        
        var app = Application.Current;
        var isDark = app?.ActualThemeVariant == ThemeVariant.Dark;
        return isDark ? Brushes.White : Brushes.Black;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var diffLine = DiffLine;
        if (diffLine == null || string.IsNullOrEmpty(diffLine.Text))
            return new Size(0, FontSize * 1.2);

        var typeface = new Typeface(FontFamily);
        var formattedText = new FormattedText(
            diffLine.Text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            GetForegroundBrush());

        return new Size(formattedText.Width, formattedText.Height);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var diffLine = DiffLine;
        if (diffLine == null)
            return;

        var typeface = new Typeface(FontFamily);
        var foreground = GetForegroundBrush();
        double x = 0;

        // If we have character-level segments, render them with individual highlighting
        if (diffLine.HasSegments && diffLine.Segments.Count > 0)
        {
            foreach (var segment in diffLine.Segments)
            {
                if (string.IsNullOrEmpty(segment.Text))
                    continue;

                var formattedText = new FormattedText(
                    segment.Text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    FontSize,
                    foreground);

                var background = GetBackgroundBrush(segment.Type);
                var rect = new Rect(x, 0, formattedText.Width, formattedText.Height);

                // Draw background highlight for changed segments
                if (segment.Type != DiffType.Unchanged)
                {
                    context.DrawRectangle(background, null, rect);
                }

                // Draw the text
                context.DrawText(formattedText, new Point(x, 0));
                x += formattedText.Width;
            }
        }
        else
        {
            // No segments - render the whole line as plain text
            var text = diffLine.Text ?? "";
            if (!string.IsNullOrEmpty(text))
            {
                var formattedText = new FormattedText(
                    text,
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    FontSize,
                    foreground);

                context.DrawText(formattedText, new Point(0, 0));
            }
        }
    }

    private static IBrush GetBackgroundBrush(DiffType type)
    {
        return type switch
        {
            DiffType.Inserted => InsertedBackground,
            DiffType.Deleted => DeletedBackground,
            DiffType.Modified => ModifiedBackground,
            _ => UnchangedBackground
        };
    }
}
