using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
  public readonly struct Space : IEquatable<Space>, Interfaces.ISpace {
    public float Length { get; }

    //is the length in math units (mu) or points (pt)?
    public bool IsMu { get; }

    public Space(float length, bool isMu) {
      Length = length;
      IsMu = isMu;
    }

    public float ActualLength<TFont, TGlyph>(FrontEnd.FontMathTable<TFont, TGlyph> mathTable, TFont font)
      where TFont : Display.MathFont<TGlyph> =>
      IsMu ? Length * mathTable.MuUnit(font) : Length;

    public static (Space? result, string error) Create(string length, string unit, bool useTextUnits) {

      if (string.IsNullOrWhiteSpace(unit) || unit.Length != 2 || unit[1] == default || unit[0] == default) {
        return (null, "Expected two-character length unit");
      }

      if (!(float.TryParse(length,
                           System.Globalization.NumberStyles.AllowLeadingSign |
                           System.Globalization.NumberStyles.AllowDecimalPoint,
                           System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                           out var points))) {
        return (null, "Invalid length value");
      }
      if (useTextUnits) {
        switch (unit) {
          //https://en.wikibooks.org/wiki/LaTeX/Lengths
          case "pt": break;
          case "mm": points *= 7227f / 2540f; break;
          case "cm": points *= 7227f / 254f; break;
          case "in": points *= 72.27f; break;
          case "bp": points *= 1.00375f; break;
          case "pc": points *= 12f; break;
          case "dd": points *= 1238f / 1157f; break;
          case "cc": points *= 14856f / 1157f; break;
          case "nd": points *= 685f / 642f; break;
          case "nc": points *= 1370f / 107f; break;
          case "sp": points /= 65536f; break;

          case "em":
            return (new Space(points * 18f, true), null);
          case "ex":
            return (new Space(points * 9f, true), null);
          case "mu":
            return (null, "The length unit mu is not allowed in text mode");
          default:
            return (null, $"Unsupported length unit {unit}");
        }
        return (new Space(points, false), null);
      } else {
        if (unit != "mu") {
          return (null, "Only the length unit mu is allowed in math mode");
        }
        return (new Space(points, true), null);
      }
    }


    public override bool Equals(object obj) => obj is Space s ? this == s : false;
    public bool Equals(Space otherSpace) => this == otherSpace;
    public override int GetHashCode() =>
      unchecked(73 * Length.GetHashCode() + 277 * IsMu.GetHashCode());
    public static bool operator ==(Space left, Space right) =>
      left.Length == right.Length && left.IsMu == right.IsMu;
    public static bool operator !=(Space left, Space right) =>
      left.Length != right.Length || left.IsMu != right.IsMu;
  }
}