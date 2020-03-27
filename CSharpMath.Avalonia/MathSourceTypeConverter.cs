using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace CSharpMath.Avalonia {
  internal sealed class MathSourceTypeConverter : TypeConverter {
    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) =>
      sourceType == typeof(string) || sourceType == typeof(Atom.MathList);

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) =>
      value switch
      {
        string str => Atom.LaTeXBuilder.TryMathListFromLaTeX(str).Match(list => list,
          error => new Atom.MathList(new Atom.Atoms.Color(Structures.Color.PredefinedColors["red"], new Atom.MathList($"Error: {error}".Select(c => new Atom.Atoms.Ordinary(c.ToStringInvariant())))))),
        Atom.MathList list => list,
        _ => throw new NotSupportedException()
      };
  }
}
