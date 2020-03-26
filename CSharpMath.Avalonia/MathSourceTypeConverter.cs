using System;
using System.ComponentModel;
using System.Globalization;
using CSharpMath.Interfaces;
using CSharpMath.Rendering;

namespace CSharpMath.Avalonia {
  internal sealed class MathSourceTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string) || sourceType == typeof(IMathList);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
      value switch
      {
        string str => new MathSource(str),
        IMathList list => new MathSource(list),
        _ => throw new NotSupportedException()
      };
  }
}
