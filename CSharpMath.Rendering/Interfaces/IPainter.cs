using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using CSharpMath.Enumerations;
using Typography.OpenFont;

namespace CSharpMath.Rendering {
  public interface IPainter<TSource, TColor> where TSource : struct, ISource {
    #region Non-redisplaying properties
    TColor HighlightColor { get; set; }
    TColor TextColor { get; set; }
    TColor ErrorColor { get; set; }
    /// <summary>
    /// Unit of measure: points;
    /// Defaults to <see cref="FontSize"/>.
    /// </summary>
    float? ErrorFontSize { get; set; }
    bool DisplayErrorInline { get; set; }
    PaintStyle PaintStyle { get; set; }
    float Magnification { get; set; }

    string ErrorMessage { get; }
    #endregion Non-redisplaying properties

    #region Redisplaying properties
    /// <summary>
    /// Unit of measure: points
    /// </summary>
    float FontSize { get; set; }
    ObservableRangeCollection<Typeface> LocalTypefaces { get; }
    LineStyle LineStyle { get; set; }
    //(Color glyph, Color textRun)? GlyphBoxColor { get; set; }
    TSource Source { get; set; }
    #endregion Redisplaying properties

  }
  public static class IPainterExtensions {
    public static PointF GetDisplayPosition(
        float displayWidth, float displayAscent, float displayDescent,
        float fontSize, bool bottomLeftCoords,
        float width, float height,
        TextAlignment alignment, Thickness padding, float offsetX, float offsetY) {
      float x, y;
      if ((alignment & TextAlignment.Left) != 0)
        x = padding.Left;
      else if ((alignment & TextAlignment.Right) != 0)
        x = width - padding.Right - displayWidth;
      else
        x = padding.Left + (width - padding.Left - padding.Right - displayWidth) / 2;
      float contentHeight = displayAscent + displayDescent;
      if (contentHeight < fontSize / 2) {
        contentHeight = fontSize / 2;
      }
      if (!bottomLeftCoords) {
        //Canvas is inverted!
        if ((alignment & (TextAlignment.Top | TextAlignment.Bottom)) != 0) {
          alignment ^= TextAlignment.Top;
          alignment ^= TextAlignment.Bottom;
        }
        //invert y-coordinate as canvas is inverted
        offsetY *= -1;
      }
      if ((alignment & TextAlignment.Top) != 0)
        y = padding.Top + displayDescent;
      else if ((alignment & TextAlignment.Bottom) != 0)
        y = height - padding.Bottom - displayAscent;
      else {
        float availableHeight = height - padding.Top - padding.Bottom;
        y = ((availableHeight - contentHeight) / 2) + padding.Top + displayDescent;
      }
      return new PointF(x + offsetX, (y + offsetY) - (bottomLeftCoords ? 0 : height));
    }
  }
  public interface ICanvasPainter<TCanvas, TPathWrapper, TSource, TColor> : IPainter<TSource, TColor> where TSource : struct, ISource where TPathWrapper : IPath {
    ICanvas<TPathWrapper> WrapCanvas(TCanvas canvas);
    Structures.Color WrapColor(TColor color);
    TColor UnwrapColor(Structures.Color color);
    void Draw(TCanvas canvas, TextAlignment alignment, Thickness padding = default, float offsetX = 0, float offsetY = 0);
  }
}