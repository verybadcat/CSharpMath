using System.Drawing;
using Typography.OpenFont;

namespace CSharpMath.Rendering.Renderer {
  using Structures;
  public interface IPainter<TSource, TColor> where TSource : ISource {
    #region Non-redisplaying properties
    TColor HighlightColor { get; set; }
    TColor TextColor { get; set; }
    TColor ErrorColor { get; set; }
    ///<summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    float? ErrorFontSize { get; set; }
    bool DisplayErrorInline { get; set; }
    PaintStyle PaintStyle { get; set; }
    float Magnification { get; set; }
    string ErrorMessage { get; }
    #endregion Non-redisplaying properties
    #region Redisplaying properties
    /// <summary>Unit of measure: points</summary>
    float FontSize { get; set; }
    ObservableRangeCollection<Typeface> LocalTypefaces { get; }
    Atoms.LineStyle LineStyle { get; set; }
    TSource Source { get; set; }
    #endregion Redisplaying properties
  }
  public static class IPainterExtensions {
    public static PointF GetDisplayPosition(
        float displayWidth, float displayAscent, float displayDescent,
        float fontSize, bool bottomLeftCoords,
        float width, float height,
        TextAlignment alignment, Thickness padding, float offsetX, float offsetY) {
      if (!bottomLeftCoords) {
        //Canvas is inverted!
        if ((alignment & (TextAlignment.Top | TextAlignment.Bottom)) != 0) {
          alignment ^= TextAlignment.Top;
          alignment ^= TextAlignment.Bottom;
        }
        //invert y-coordinate as canvas is inverted
        offsetY *= -1;
      }
      var x =
        (alignment & TextAlignment.Left) != 0
        ? padding.Left
        : (alignment & TextAlignment.Right) != 0
        ? width - padding.Right - displayWidth
        : padding.Left + (width - padding.Left - padding.Right - displayWidth) / 2;
      float contentHeight = System.Math.Max(displayAscent + displayDescent, fontSize / 2);
      var y =
        (alignment & TextAlignment.Top) != 0
        ? padding.Top + displayDescent
        : (alignment & TextAlignment.Bottom) != 0
        ? height - padding.Bottom - displayAscent
        : (height - padding.Top - padding.Bottom - contentHeight) / 2
          + padding.Top + displayDescent;
      return new PointF(x + offsetX, y + offsetY - (bottomLeftCoords ? 0 : height));
    }
  }
  public interface ICanvasPainter<TCanvas, TSource, TColor> : IPainter<TSource, TColor> where TSource : ISource {
    ICanvas WrapCanvas(TCanvas canvas);
    Color WrapColor(TColor color);
    TColor UnwrapColor(Color color);
    void Draw(TCanvas canvas, TextAlignment alignment, Thickness padding = default, float offsetX = 0, float offsetY = 0);
  }
}