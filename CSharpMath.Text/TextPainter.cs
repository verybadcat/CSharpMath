using System;
using System.Collections.ObjectModel;
using System.Drawing;
using CSharpMath.Enumerations;
using CSharpMath.Rendering;
using CSharpMath.Structures;
using Typography.OpenFont;

namespace CSharpMath.Text
{
  public static class TextPainter {
    public TextPainter<TCanvas, TColor> Create<TCanvas, TSource, TColor>(IPainter<TCanvas, TSource, TColor> painter) =>

  }
  public class TextPainter<TCanvas, TColor> : IPainter<TCanvas, TextSource, TColor> {



    public TColor BackgroundColor { get; set; }
    public TColor TextColor { get; set; }
    public TColor ErrorColor { get; set; }
    public float? ErrorFontSize { get; set; }
    public bool DisplayErrorInline { get; set; }
    public PaintStyle PaintStyle { get; set; }
    public float Magnification { get; set; }

    public RectangleF? Measure => throw new NotImplementedException();

    public string ErrorMessage => throw new NotImplementedException();

    public float FontSize { get; set; }

    public ObservableCollection<Typeface> LocalTypefaces => throw new NotImplementedException();

    public LineStyle LineStyle { get; set; }
    public TextSource Source { get; set; }

    public ICanvas CreateCanvasWrapper(TCanvas canvas) {
      throw new NotImplementedException();
    }

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
