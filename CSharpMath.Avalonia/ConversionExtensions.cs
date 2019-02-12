using Avalonia.Media;
using AvaloniaColor = Avalonia.Media.Color;

using CSharpMathColor = CSharpMath.Structures.Color;

namespace CSharpMath.Avalonia {
  internal static class ConversionExtensions {
    public static AvaloniaColor ToAvaloniaColor(this CSharpMathColor color) =>
        new AvaloniaColor(color.A, color.R, color.G, color.B);

    public static CSharpMathColor ToCSharpMathColor(this AvaloniaColor color) =>
        new CSharpMathColor(color.R, color.G, color.B, color.A);

    public static SolidColorBrush ToSolidColorBrush(this CSharpMathColor color) =>
        new SolidColorBrush(color.ToAvaloniaColor());
  }
}
