using System;
using System.Collections.Generic;

namespace CSharpMath.Atom {
  public readonly struct Length : IEquatable<Length> {
    public float Amount { get; }
    // If IsMu is true, then the length is in math units (mu), else points (pt)
    public bool IsMu { get; }
    // Use arithmetic operators instead of new
    private Length(float length, bool isMu) { Amount = length; IsMu = isMu; }
    public float ActualLength<TFont, TGlyph>(
      Display.FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.FrontEnd.IFont<TGlyph> =>
      IsMu ? Amount * mathTable.MuUnit(font) : Amount;
    public static Result<Length> Create(string length, string unit, bool useTextUnits) =>
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
      : (Result<Length>)(MathUnit * value);
    private static bool UnifyIsMu(Length left, Length right) =>
      left.IsMu && right.IsMu
      || (left.IsMu || right.IsMu
          ? throw new ArgumentException("The IsMu property of two Lengths must not differ " +
              "in order to perform addition or subtraction on them.")
          : false);
    public override bool Equals(object obj) => obj is Length s && this == s;
    public bool EqualsLength(Length otherLength) => this == otherLength;
    bool IEquatable<Length>.Equals(Length other) => EqualsLength(other);
    public override int GetHashCode() =>
      unchecked(73 * Amount.GetHashCode() + 277 * IsMu.GetHashCode());
    public static bool operator ==(Length left, Length right) =>
      left.Amount == right.Amount && left.IsMu == right.IsMu;
    public static bool operator !=(Length left, Length right) =>
      left.Amount != right.Amount || left.IsMu != right.IsMu;
    public static Length operator +(Length space) => space;
    public static Length Plus(Length space) => +space;
    public static Length operator -(Length space) => new Length(-space.Amount, space.IsMu);
    public static Length Negate(Length space) => -space;
    public static Length operator +(Length left, Length right) =>
      new Length(left.Amount + right.Amount, UnifyIsMu(left, right));
    public static Length Add(Length left, Length right) => left + right;
    public static Length operator -(Length left, Length right) =>
      new Length(left.Amount - right.Amount, UnifyIsMu(left, right));
    public static Length Subtract(Length left, Length right) => left - right;
    public static Length operator *(float magnitude, Length length) =>
      new Length(magnitude * length.Amount, length.IsMu);
    public static Length Multiply(float magnitude, Length length) => magnitude * length;
    public static Length operator *(Length length, float magnitude) =>
      new Length(length.Amount * magnitude, length.IsMu);
    public static Length Multiply(Length length, float magnitude) => length * magnitude;
    public static Length operator /(Length length, float magnitude) =>
      new Length(length.Amount / magnitude, length.IsMu);
    public static Length Divide(Length length, float magnitude) => length / magnitude;
    public static readonly Length Point = new Length(1, false);
    public static readonly Length Millimeter = new Length(7227f / 2540f, false);
    public static readonly Length Centimeter = new Length(7227f / 254f, false);
    public static readonly Length Inch = new Length(72.27f, false);
    public static readonly Length EmWidth = new Length(18, true);
    public static readonly Length ExHeight = new Length(9, true);
    public static readonly Length MathUnit = new Length(1, true);
    public static readonly Length ShortSpace = 3 * MathUnit;
    public static readonly Length MediumSpace = 4 * MathUnit;
    public static readonly Length LongSpace = 5 * MathUnit;
    //https://github.com/latex3/latex2e/blob/b45b88761d659bfe0a0de4638e82122db2ab8184/base/classes.dtx#L775
    public static readonly Length ParagraphIndent = 1.5f * EmWidth;
    public static Dictionary<string, Length> PredefinedLengthUnits { get; } =
      new Dictionary<string, Length> {
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