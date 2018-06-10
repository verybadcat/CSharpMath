using System;
using CSharpMath.Structures;
using SKColor = SkiaSharp.SKColor;

namespace CSharpMath.SkiaSharp {
  public static class SkiaColorExtensions {
    public static SKColor ToNative(this Color color) => new SKColor(color.R, color.G, color.B, color.A);
    public static SKColor? ToNative(this Color? color) => color.HasValue ? ToNative(color.Value) : default(SKColor?);
    public static Color FromNative(this SKColor color) => new Color(color.Red, color.Green, color.Blue, color.Alpha);
    public static Color? FromNative(this SKColor? color) => color.HasValue ? FromNative(color.Value) : default(Color?);
  }
}