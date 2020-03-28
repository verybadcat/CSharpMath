using System;
using System.Globalization;
using System.ComponentModel;

namespace CSharpMath.Rendering.Text {
  public sealed class TextAtomTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string) || sourceType == typeof(TextAtom);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
      value switch
      {
        string str => TextLaTeXBuilder.TextAtomFromLaTeX(str)
          .Match(atom => atom, error => new TextAtom.Color(new TextAtom.Text($"Error: {error}"), Structures.Color.PredefinedColors["red"])),
        TextAtom atom => atom,
        _ => throw new ArgumentException("Unsupported type", nameof(value))
      };
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
      destinationType == typeof(string) || destinationType == typeof(TextAtom);
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
      value is TextAtom source ?
        destinationType == typeof(string) ? (object)TextLaTeXBuilder.TextAtomToLaTeX(source).ToString() :
        destinationType == typeof(TextAtom) ? source :
        throw new ArgumentException("Unsupported type", nameof(destinationType)) :
      throw new ArgumentException("Unsupported type", nameof(value));
  }
}
