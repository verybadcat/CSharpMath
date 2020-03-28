using System;
using System.ComponentModel;
using System.Globalization;

namespace CSharpMath.Atom {
  public sealed class MathListTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string) || sourceType == typeof(MathList);
    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
      value switch
      {
        string str => LaTeXBuilder.TryMathListFromLaTeX(str).Match(list => list,
          error => new MathList(new Atoms.Color(Structures.Color.PredefinedColors["red"], new MathList(new Atoms.Ordinary($"Error: {error}"))))),
        MathList list => list,
        _ => throw new ArgumentException("Unsupported type", nameof(value))
      };
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) =>
      destinationType == typeof(string) || destinationType == typeof(MathList);
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) =>
      value is MathList source ?
        destinationType == typeof(string) ? (object)LaTeXBuilder.MathListToLaTeX(source).ToString() :
        destinationType == typeof(MathList) ? source :
        throw new ArgumentException("Unsupported type", nameof(destinationType)) :
      throw new ArgumentException("Unsupported type", nameof(value));
  }
}
