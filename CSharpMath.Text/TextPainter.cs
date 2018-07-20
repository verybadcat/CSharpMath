using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using Typography.OpenFont;

namespace CSharpMath.Text {
  public static class TextPainter {
    /// <summary>
    /// Creates a new <see cref="TextPainter{TCanvas, TColor}"/> from a <see cref="MathPainter{TCanvas}"/>
    /// </summary>
    /// <typeparam name="TCanvas"></typeparam>
    /// <typeparam name="TColor"></typeparam>
    /// <param name="painter"></param>
    /// <returns></returns>
    public static TextPainter<TCanvas, TColor> Create<TCanvas, TColor>(ICanvasPainter<TCanvas, MathSource, TColor> painter) =>
#pragma warning disable 618 //Intended
      new TextPainter<TCanvas, TColor>(painter);
#pragma warning restore 618
  }
  public class TextPainter<TCanvas, TColor> : ICanvasPainter<TCanvas, TextSource, TColor> {
    [Obsolete("Use " + nameof(CSharpMath) + "." + nameof(Text) + "." + nameof(TextPainter) + "." + nameof(TextPainter.Create) + " instead." )]
    public TextPainter(ICanvasPainter<TCanvas, MathSource, TColor> painter) => _painter = painter;
    readonly ICanvasPainter<TCanvas, MathSource, TColor> _painter;

    public TColor HighlightColor { get => _painter.HighlightColor; set => _painter.HighlightColor = value; }
    public TColor TextColor { get => _painter.TextColor; set => _painter.TextColor = value; }
    public TColor ErrorColor { get => _painter.ErrorColor; set => _painter.ErrorColor = value; }
    public float? ErrorFontSize { get => _painter.ErrorFontSize; set => _painter.ErrorFontSize = value; }
    public bool DisplayErrorInline { get => _painter.DisplayErrorInline; set => _painter.DisplayErrorInline = value; }
    public PaintStyle PaintStyle { get => _painter.PaintStyle; set => _painter.PaintStyle = value; }
    public float Magnification { get => _painter.Magnification; set => _painter.Magnification = value; }

    public RectangleF? Measure => _painter.Measure;

    public string ErrorMessage => _painter.ErrorMessage;

    public float FontSize { get => _painter.FontSize; set => _painter.FontSize = value; }

    public ObservableCollection<Typeface> LocalTypefaces => _painter.LocalTypefaces;

    public LineStyle LineStyle { get => _painter.LineStyle; set => _painter.LineStyle = value; }

    public TextSource Source { get; set; }
    public string Text { get => Source.Text; set => Source = new TextSource(Text); }
    public string Error => Source.Error;

    public ICanvas WrapCanvas(TCanvas canvas) => _painter.WrapCanvas(canvas);

    public void UpdateDisplay() => _painter.UpdateDisplay();

    public void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) =>
      _painter.Draw(canvas, alignment, padding, offsetX, offsetY);

    public void Draw(TCanvas canvas, float x, float y) => _painter.Draw(canvas, x, y);

    public void Draw(TCanvas canvas, PointF position) => _painter.Draw(canvas, position);

    public Color WrapColor(TColor color) => _painter.WrapColor(color);

    public TColor UnwrapColor(Color color) => _painter.UnwrapColor(color);
  }
}
