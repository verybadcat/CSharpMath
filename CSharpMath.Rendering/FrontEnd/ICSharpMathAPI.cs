using System.Drawing;
using Typography.OpenFont;

namespace CSharpMath.Rendering.FrontEnd {
  using Structures;
  public interface ICSharpMathAPI<TContent, TColor> where TContent : class {
    #region Non-display-recreating properties
    TColor HighlightColor { get; set; }
    TColor TextColor { get; set; }
    TColor ErrorColor { get; set; }
    ///<summary>Unit of measure: points; Defaults to <see cref="FontSize"/>.</summary>
    float? ErrorFontSize { get; set; }
    bool DisplayErrorInline { get; set; }
    PaintStyle PaintStyle { get; set; }
    float Magnification { get; set; }
    string? ErrorMessage { get; }
    #endregion Non-display-recreating properties
    #region Display-recreating properties
    /// <summary>Unit of measure: points</summary>
    float FontSize { get; set; }
    System.Collections.Generic.IEnumerable<Typeface> LocalTypefaces { get; set; }
    Atom.LineStyle LineStyle { get; set; }
    TContent? Content { get; set; }
    string? LaTeX { get; set; }
    #endregion Display-recreating properties
  }
  public static class IPainterExtensions {
    public static PointF GetDisplayPosition(
        float displayWidth, float displayAscent, float displayDescent,
        float fontSize,
        float width, float height,
        TextAlignment alignment, Thickness padding, float offsetX, float offsetY) {
      // Canvas is inverted!
      if ((alignment & (TextAlignment.Top | TextAlignment.Bottom)) != 0) {
        alignment ^= TextAlignment.Top;
        alignment ^= TextAlignment.Bottom;
      }
      // Invert y-coordinate as canvas is inverted
      offsetY *= -1;
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
      return new PointF(x + offsetX, y + offsetY - height);
    }
  }
}