using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
  public readonly struct Space : IEquatable<Space>, Interfaces.ISpace {
    public float Length { get; }

    //Is the length in math units (mu) or points (pt)?
    public bool IsMu { get; }

    //To anyone reading this, please use arithmetic operators instead of new
    private Space(float length, bool isMu) {
      Length = length;
      IsMu = isMu;
    }

    public float ActualLength<TFont, TGlyph>(FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.MathFont<TGlyph> =>
      IsMu ? Length * mathTable.MuUnit(font) : Length;

    public static Result<Space> Create(string length, string unit, bool useTextUnits) {
      if (string.IsNullOrWhiteSpace(unit) || unit.Length != 2 || unit[1] == default || unit[0] == default)
        return "Expected two-character length unit";
      else if (!float.TryParse(length,
                           System.Globalization.NumberStyles.AllowLeadingSign |
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                           out var value))
        return "Invalid length value";
      else if (useTextUnits)
        switch (unit) {
          case "mu":
            return "The length unit mu is not allowed in text mode";
          case var _ when PredefinedUnits.TryGetValue(unit, out var space):
            return space * value;
          default:
            return $"Unsupported length unit {unit}";
        }
      else if (unit != "mu")
        return "Only the length unit mu is allowed in math mode";
      else
        return MathUnit * value;
    }

    private static bool DetermineSameLengthType(Space left, Space right) {
      if (left.IsMu && right.IsMu) return true;
      else if (left.IsMu || right.IsMu)
        throw new ArgumentException("The IsMu property of two Spaces must not differ " +
          "in order to perform addition or subtraction on them.");
      else return false;
    }

    public override bool Equals(object obj) => obj is Space s ? this == s : false;
    public bool Equals(Space otherSpace) => this == otherSpace;
    public override int GetHashCode() =>
      unchecked(73 * Length.GetHashCode() + 277 * IsMu.GetHashCode());
    public static bool operator ==(Space left, Space right) =>
      left.Length == right.Length && left.IsMu == right.IsMu;
    public static bool operator !=(Space left, Space right) =>
      left.Length != right.Length || left.IsMu != right.IsMu;
    public static Space operator +(Space space) =>
      new Space(+space.Length, space.IsMu);
    public static Space operator -(Space space) =>
      new Space(-space.Length, space.IsMu);
    public static Space operator +(Space left, Space right) =>
      new Space(left.Length + right.Length, DetermineSameLengthType(left, right));
    public static Space operator -(Space left, Space right) =>
      new Space(left.Length - right.Length, DetermineSameLengthType(left, right));
    public static Space operator *(float magnitude, Space length) =>
      new Space(length.Length * magnitude, length.IsMu);
    public static Space operator *(Space length, float magnitude) =>
      new Space(length.Length * magnitude, length.IsMu);
    public static Space operator /(Space length, float magnitude) =>
      new Space(length.Length / magnitude, length.IsMu);

    public static readonly Space Point = new Space(1, false);
    public static readonly Space Millimeter = new Space(7227f / 2540f, false);
    public static readonly Space Centimeter = new Space(7227f / 254f, false);
    public static readonly Space Inch = new Space(72.27f, false);
    public static readonly Space EmWidth = new Space(18, true);
    public static readonly Space ExHeight = new Space(9, true);
    public static readonly Space MathUnit = new Space(1, true);
    public static readonly Space ShortSpace = 3 * Point;
    public static readonly Space MediumSpace = 4 * Point;
    public static readonly Space LongSpace = 5 * Point;
    //https://github.com/latex3/latex2e/blob/b45b88761d659bfe0a0de4638e82122db2ab8184/base/classes.dtx#L775
    public static readonly Space ParagraphIndent = 1.5f * EmWidth;
    public static Dictionary<string, Space> PredefinedUnits { get; } =
      new Dictionary<string, Space> {
          //https://en.wikibooks.org/wiki/LaTeX/Lengths
          { "pt", Point },
          { "mm", Millimeter },
          { "cm", Centimeter },
          { "in", Inch },
          { "bp", Point * 803 / 800 },
          { "pc", Point * 12 },
          { "dd", Point * 1238 / 1157 },
          { "cc", Point * 14856 / 1157 },
          { "nd", Point * 685 / 642 },
          { "nc", Point * 1370 / 107 },
          { "sp", Point / 65536 },
          { "em", EmWidth },
          { "ex", ExHeight },
          { "mu", MathUnit },
      };
  }
}