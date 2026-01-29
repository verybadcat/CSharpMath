using System;
using CSharpMath.Rendering.FrontEnd;

namespace CSharpMath.VectSharp;

public static class Extensions {
  internal static global::VectSharp.Colour ToNative(this System.Drawing.Color color) =>
    global::VectSharp.Colour.FromRgba(color.R, color.G, color.B, color.A);
  internal static System.Drawing.Color FromNative(this global::VectSharp.Colour colour) =>
    System.Drawing.Color.FromArgb((int)Math.Round(colour.A * 255), (int)Math.Round(colour.R * 255), (int)Math.Round(colour.G * 255), (int)Math.Round(colour.B * 255));
  public static global::VectSharp.Page DrawToPage<TContent>(this Painter<global::VectSharp.Page, TContent, global::VectSharp.Colour> painter, float textPainterCanvasWidth = TextPainter.DefaultCanvasWidth, TextAlignment alignment = TextAlignment.TopLeft) where TContent : class {
    var size = painter.Measure(textPainterCanvasWidth).Size;
    var page = new global::VectSharp.Page(size.Width, size.Height);
    painter.Draw(page, alignment);
    return page;
  }
}