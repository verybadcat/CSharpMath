using System;
using System.Collections.Generic;

namespace CSharpMath.Structures {
  public readonly struct Space : IEquatable<Space> {
    public float Length { get; }
    // If IsMu is true, then the length is in math units (mu), else points (pt)
    public bool IsMu { get; }
    // Use arithmetic operators instead of new
    private Space(float length, bool isMu) { Length = length; IsMu = isMu; }
    public float ActualLength<TFont, TGlyph>(
      Display.FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.FrontEnd.IFont<TGlyph> =>
      IsMu ? Length * mathTable.MuUnit(font) : Length;
    public static Result<Space> Create(string length, string unit, bool useTextUnits) =>
      string.IsNullOrWhiteSpace(unit)
      || unit.Length != 2
      || unit[0] == default
      || unit[1] == default
      ? "Expected two-character length unit"
      : !float.TryParse(length,
        System.Globalization.NumberStyles.AllowLeadingSign |
        System.Globalization.NumberStyles.AllowDecimalPoint,
        System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
        out var value)
      ? "Invalid length value"
      : useTextUnits
      ? unit switch {
        "mu" => "The length unit mu is not allowed in text mode",
        var _ when PredefinedLengthUnits.TryGetValue(unit, out var space) => space * value,
        _ => $"Unsupported length unit {unit}",
      } : unit != "mu"
      ? "Only the length unit mu is allowed in math mode"
      : (Result<Space>)(MathUnit * value);
    private static bool UnifyIsMu(Space left, Space right) =>
      left.IsMu && right.IsMu
      || (left.IsMu || right.IsMu
          ? throw new ArgumentException("The IsMu property of two Spaces must not differ " +
              "in order to perform addition or subtraction on them.")
          : false);
    public override bool Equals(object obj) => obj is Space s && this == s;
    public bool EqualsSpace(Space otherSpace) => this == otherSpace;
    bool IEquatable<Space>.Equals(Space other) => EqualsSpace(other);
    public override int GetHashCode() =>
      unchecked(73 * Length.GetHashCode() + 277 * IsMu.GetHashCode());
    public static bool operator ==(Space left, Space right) =>
      left.Length == right.Length && left.IsMu == right.IsMu;
    public static bool operator !=(Space left, Space right) =>
      left.Length != right.Length || left.IsMu != right.IsMu;
    public static Space operator +(Space space) => space;
    public static Space Plus(Space space) => +space;
    public static Space operator -(Space space) => new Space(-space.Length, space.IsMu);
    public static Space Negate(Space space) => -space;
    public static Space operator +(Space left, Space right) =>
      new Space(left.Length + right.Length, UnifyIsMu(left, right));
    public static Space Add(Space left, Space right) => left + right;
    public static Space operator -(Space left, Space right) =>
      new Space(left.Length - right.Length, UnifyIsMu(left, right));
    public static Space Subtract(Space left, Space right) => left - right;
    public static Space operator *(float magnitude, Space length) =>
      new Space(magnitude * length.Length, length.IsMu);
    public static Space Multiply(float magnitude, Space length) => magnitude * length;
    public static Space operator *(Space length, float magnitude) =>
      new Space(length.Length * magnitude, length.IsMu);
    public static Space Multiply(Space length, float magnitude) => length * magnitude;
    public static Space operator /(Space length, float magnitude) =>
      new Space(length.Length / magnitude, length.IsMu);
    public static Space Divide(Space length, float magnitude) => length / magnitude;
    public static readonly Space Point = new Space(1, false);
    public static readonly Space Millimeter = new Space(7227f / 2540f, false);
    public static readonly Space Centimeter = new Space(7227f / 254f, false);
    public static readonly Space Inch = new Space(72.27f, false);
    public static readonly Space EmWidth = new Space(18, true);
    public static readonly Space ExHeight = new Space(9, true);
    public static readonly Space MathUnit = new Space(1, true);
    public static readonly Space ShortSpace = 3 * MathUnit;
    public static readonly Space MediumSpace = 4 * MathUnit;
    public static readonly Space LongSpace = 5 * MathUnit;
    //https://github.com/latex3/latex2e/blob/b45b88761d659bfe0a0de4638e82122db2ab8184/base/classes.dtx#L775
    public static readonly Space ParagraphIndent = 1.5f * EmWidth;
    public static Dictionary<string, Space> PredefinedLengthUnits { get; } =
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
          { "mu", MathUnit }
      };
  }
}