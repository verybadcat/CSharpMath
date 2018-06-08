using System;
using CSharpMath.Structures;
using XColor = Xamarin.Forms.Color;

namespace CSharpMath.Forms {
  public static class FormsColorExtensions {
    public static XColor ToNative(this Color color) => new XColor(color.R / 255, color.G / 255, color.B / 255, color.A / 255);
    public static XColor? ToNative(this Color? color) => color.HasValue ? ToNative(color.Value) : default(XColor?);
    public static Color FromNative(this XColor color) => new Color((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255), (byte)(color.A * 255));
    public static Color? FromNative(this XColor? color) => color.HasValue ? FromNative(color.Value) : default(Color?);
  }
}