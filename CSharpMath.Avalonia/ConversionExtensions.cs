using SizeF = System.Drawing.SizeF;

using Avalonia;
using Avalonia.Media;
using AvaloniaColor = Avalonia.Media.Color;
using AvaloniaTextAlignment= Avalonia.Media.TextAlignment;

using CSharpMathColor = CSharpMath.Structures.Color;
using CSharpMathTextAlignment = CSharpMath.Rendering.TextAlignment;

namespace CSharpMath.Avalonia {
  internal static class ConversionExtensions {
    public static AvaloniaColor ToAvaloniaColor(this CSharpMathColor color) =>
        new AvaloniaColor(color.A, color.R, color.G, color.B);

    public static Size ToAvaloniaSize(this SizeF size) =>
        new Size(size.Width, size.Height);

    public static CSharpMathColor ToCSharpMathColor(this AvaloniaColor color) =>
        new CSharpMathColor(color.R, color.G, color.B, color.A);

    public static CSharpMathTextAlignment ToCSharpMathTextAlignment(this AvaloniaTextAlignment alignment) =>
      alignment switch
      {
        AvaloniaTextAlignment.Left => CSharpMathTextAlignment.TopLeft,
        AvaloniaTextAlignment.Center => CSharpMathTextAlignment.Top,
        AvaloniaTextAlignment.Right => CSharpMathTextAlignment.TopRight,
        _ => CSharpMathTextAlignment.Left
      };

    public static SolidColorBrush ToSolidColorBrush(this CSharpMathColor color) =>
        new SolidColorBrush(color.ToAvaloniaColor());
  }
}
