using System;
using System.ComponentModel;
using System.Globalization;
using CSharpMath.Rendering.Text;

namespace CSharpMath.Avalonia {
  public sealed class TextSourceTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string) || sourceType == typeof(TextAtom);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
      value switch
      {
        string str => TextLaTeXBuilder.TextAtomFromLaTeX(str)
          .Match(atom => atom, error => new TextAtom.Color(new TextAtom.Text($"Error: {error}"), Structures.Color.PredefinedColors["red"])),
        TextAtom atom => atom,
        _ => throw new NotSupportedException()
      };
  }
}
