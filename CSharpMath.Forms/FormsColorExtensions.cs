using System;
using CSharpMath.Structures;
using XColor = Xamarin.Forms.Color;

namespace CSharpMath.Forms {
  public static class FormsColorExtensions {
    public static XColor ToNative(this Color color) => new XColor(color.Rf, color.Gf, color.Bf, color.Af);
    public static XColor? ToNative(this Color? color) => color.HasValue ? ToNative(color.Value) : default(XColor?);
    public static Color FromNative(this XColor color) => new Color((float)color.R, (float)color.G, (float)color.B, (float)color.A);
    public static Color? FromNative(this XColor? color) => color.HasValue ? FromNative(color.Value) : default(Color?);
  }
}