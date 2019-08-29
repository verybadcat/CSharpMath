using System;
using System.ComponentModel;
using System.Globalization;
using CSharpMath.Rendering;

namespace CSharpMath.Avalonia {
  internal sealed class TextSourceTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string) || sourceType == typeof(TextAtom);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
      value switch
      {
        string str => new TextSource(str),
        TextAtom atom => new TextSource(atom),
        _ => throw new NotSupportedException()
      };
  }
}
