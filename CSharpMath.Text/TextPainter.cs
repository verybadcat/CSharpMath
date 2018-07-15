using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
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
    public static TextPainter<TCanvas, TColor> Create<TCanvas, TColor>(IPainter<TCanvas, MathSource, TColor> painter) =>
#pragma warning disable 618 //Intended
      new TextPainter<TCanvas, TColor>(painter);
#pragma warning restore 618
  }
  public class TextPainter<TCanvas, TColor> : IPainter<TCanvas, TextSource, TColor> {
    [Obsolete("Use " + nameof(CSharpMath) + "." + nameof(Text) + "." + nameof(TextPainter) + "." + nameof(TextPainter.Create) + " instead." )]
    public TextPainter(IPainter<TCanvas, MathSource, TColor> painter) => _painter = painter;
    readonly IPainter<TCanvas, MathSource, TColor> _painter;

    public TColor BackgroundColor { get; set; }
    public TColor TextColor { get; set; }
    public TColor ErrorColor { get; set; }
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; }
    public PaintStyle PaintStyle { get; set; }
    public float Magnification { get; set; }

    public RectangleF? Measure => throw new NotImplementedException();

    /// <summary>
    /// Always null as there are no errors possible.
    /// </summary>
    public string ErrorMessage => null;

    public float FontSize { get; set; }

    public ObservableCollection<Typeface> LocalTypefaces => throw new NotImplementedException();

    public LineStyle LineStyle { get; set; }
    public TextSource Source { get; set; }

    public ICanvas WrapCanvas(TCanvas canvas) => _painter.WrapCanvas(canvas);

    public void Draw(TCanvas canvas, TextAlignment alignment = TextAlignment.Center, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      throw new NotImplementedException();
    }

    public void Draw(TCanvas canvas, float x, float y) {
      throw new NotImplementedException();
    }

    public void Draw(TCanvas canvas, PointF position) {
      throw new NotImplementedException();
    }

    public void UpdateDisplay() {
      throw new NotImplementedException();
    }
  }
}
