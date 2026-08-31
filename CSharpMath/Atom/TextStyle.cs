using System;

namespace CSharpMath.Atom {
  public enum FontFamily {
    Default,
    Roman,
    SansSerif,
    Monospace,
    Calligraphic,
    Fraktur,
    Blackboard
  }

  public enum FontWeight {
    Regular,
    Bold
  }

  public enum FontPosture {
    Upright,
    Italic,
    Slanted
  }

  public enum FontCapitals {
    Normal,
    SmallCapitals
  }

  /// <summary>A composable, semantic description of a requested text or math style.</summary>
  public readonly struct TextStyle : IEquatable<TextStyle> {
    public TextStyle(FontFamily family, FontWeight weight, FontPosture posture, FontCapitals capitals) {
      Validate(family, weight, posture, capitals);
      (Family, Weight, Posture, Capitals) = (family, weight, posture, capitals);
    }
    internal static void Validate(FontFamily family, FontWeight weight, FontPosture posture, FontCapitals capitals) {
      if (!Enum.IsDefined(typeof(FontFamily), family)) throw new ArgumentOutOfRangeException(nameof(family));
      if (!Enum.IsDefined(typeof(FontWeight), weight)) throw new ArgumentOutOfRangeException(nameof(weight));
      if (!Enum.IsDefined(typeof(FontPosture), posture)) throw new ArgumentOutOfRangeException(nameof(posture));
      if (!Enum.IsDefined(typeof(FontCapitals), capitals)) throw new ArgumentOutOfRangeException(nameof(capitals));
    }

    public static TextStyle Default => default;
    public FontFamily Family { get; }
    public FontWeight Weight { get; }
    public FontPosture Posture { get; }
    public FontCapitals Capitals { get; }

    public TextStyle WithFamily(FontFamily family) => new TextStyle(family, Weight, Posture, Capitals);
    public TextStyle WithWeight(FontWeight weight) => new TextStyle(Family, weight, Posture, Capitals);
    public TextStyle WithPosture(FontPosture posture) => new TextStyle(Family, Weight, posture, Capitals);
    public TextStyle WithCapitals(FontCapitals capitals) => new TextStyle(Family, Weight, Posture, capitals);

    public static TextStyle FromFontStyle(FontStyle style) => style switch {
      FontStyle.Default => Default,
      FontStyle.Roman => Default.WithFamily(FontFamily.Roman),
      FontStyle.Bold => Default.WithWeight(FontWeight.Bold),
      FontStyle.Caligraphic => Default.WithFamily(FontFamily.Calligraphic),
      FontStyle.Typewriter => Default.WithFamily(FontFamily.Monospace),
      FontStyle.Italic => Default.WithPosture(FontPosture.Italic),
      FontStyle.SansSerif => Default.WithFamily(FontFamily.SansSerif),
      FontStyle.Fraktur => Default.WithFamily(FontFamily.Fraktur),
      FontStyle.Blackboard => Default.WithFamily(FontFamily.Blackboard),
      FontStyle.BoldItalic => Default.WithWeight(FontWeight.Bold).WithPosture(FontPosture.Italic),
      _ => throw new ArgumentOutOfRangeException(nameof(style))
    };

    /// <summary>Returns an exact legacy representation when all requested axes fit <see cref="FontStyle"/>.</summary>
    public bool TryGetFontStyle(out FontStyle style) {
      if (Capitals != FontCapitals.Normal || Posture == FontPosture.Slanted) {
        style = default;
        return false;
      }
      if (Family == FontFamily.Default) {
        style = (Weight, Posture) switch {
          (FontWeight.Regular, FontPosture.Upright) => FontStyle.Default,
          (FontWeight.Bold, FontPosture.Upright) => FontStyle.Bold,
          (FontWeight.Regular, FontPosture.Italic) => FontStyle.Italic,
          (FontWeight.Bold, FontPosture.Italic) => FontStyle.BoldItalic,
          _ => default
        };
        return true;
      }
      if (Weight != FontWeight.Regular || Posture != FontPosture.Upright) {
        style = default;
        return false;
      }
      style = Family switch {
        FontFamily.Roman => FontStyle.Roman,
        FontFamily.SansSerif => FontStyle.SansSerif,
        FontFamily.Monospace => FontStyle.Typewriter,
        FontFamily.Calligraphic => FontStyle.Caligraphic,
        FontFamily.Fraktur => FontStyle.Fraktur,
        FontFamily.Blackboard => FontStyle.Blackboard,
        _ => default
      };
      return true;
    }

    /// <summary>
    /// Projects this semantic style onto the legacy enum. Family takes precedence for combinations
    /// that the enum cannot represent; slanted projects to italic and the capitals axis is preserved
    /// only by this value, not by the returned enum. Use <see cref="TryGetFontStyle"/> to detect loss.
    /// </summary>
    public FontStyle ToFontStyle() {
      if (TryGetFontStyle(out var exact)) return exact;
      if (Family != FontFamily.Default)
        return Family switch {
          FontFamily.Roman => FontStyle.Roman,
          FontFamily.SansSerif => FontStyle.SansSerif,
          FontFamily.Monospace => FontStyle.Typewriter,
          FontFamily.Calligraphic => FontStyle.Caligraphic,
          FontFamily.Fraktur => FontStyle.Fraktur,
          FontFamily.Blackboard => FontStyle.Blackboard,
          _ => FontStyle.Default
        };
      return (Weight, Posture) switch {
        (FontWeight.Bold, FontPosture.Upright) => FontStyle.Bold,
        (FontWeight.Bold, _) => FontStyle.BoldItalic,
        (FontWeight.Regular, FontPosture.Italic or FontPosture.Slanted) => FontStyle.Italic,
        _ => FontStyle.Default
      };
    }

    public bool Equals(TextStyle other) =>
      Family == other.Family && Weight == other.Weight && Posture == other.Posture && Capitals == other.Capitals;
    public override bool Equals(object obj) => obj is TextStyle other && Equals(other);
    public override int GetHashCode() => (Family, Weight, Posture, Capitals).GetHashCode();
  }

  /// <summary>A set of optional axis changes applied by one scoped style command.</summary>
  public readonly struct TextStyleChange : IEquatable<TextStyleChange> {
    public TextStyleChange(FontFamily? family, FontWeight? weight, FontPosture? posture, FontCapitals? capitals) {
      if (family.HasValue && !Enum.IsDefined(typeof(FontFamily), family.Value)) throw new ArgumentOutOfRangeException(nameof(family));
      if (weight.HasValue && !Enum.IsDefined(typeof(FontWeight), weight.Value)) throw new ArgumentOutOfRangeException(nameof(weight));
      if (posture.HasValue && !Enum.IsDefined(typeof(FontPosture), posture.Value)) throw new ArgumentOutOfRangeException(nameof(posture));
      if (capitals.HasValue && !Enum.IsDefined(typeof(FontCapitals), capitals.Value)) throw new ArgumentOutOfRangeException(nameof(capitals));
      (Family, Weight, Posture, Capitals) = (family, weight, posture, capitals);
    }

    public FontFamily? Family { get; }
    public FontWeight? Weight { get; }
    public FontPosture? Posture { get; }
    public FontCapitals? Capitals { get; }

    public TextStyle ApplyTo(TextStyle style) => new TextStyle(
      Family ?? style.Family,
      Weight ?? style.Weight,
      Posture ?? style.Posture,
      Capitals ?? style.Capitals);

    public static TextStyleChange FromFontStyleCommand(FontStyle style) => style switch {
      FontStyle.Default => new TextStyleChange(FontFamily.Default, FontWeight.Regular, FontPosture.Upright, FontCapitals.Normal),
      FontStyle.Roman => new TextStyleChange(FontFamily.Roman, null, null, null),
      FontStyle.Bold => new TextStyleChange(null, FontWeight.Bold, null, null),
      FontStyle.Caligraphic => new TextStyleChange(FontFamily.Calligraphic, null, null, null),
      FontStyle.Typewriter => new TextStyleChange(FontFamily.Monospace, null, null, null),
      FontStyle.Italic => new TextStyleChange(null, null, FontPosture.Italic, null),
      FontStyle.SansSerif => new TextStyleChange(FontFamily.SansSerif, null, null, null),
      FontStyle.Fraktur => new TextStyleChange(FontFamily.Fraktur, null, null, null),
      FontStyle.Blackboard => new TextStyleChange(FontFamily.Blackboard, null, null, null),
      FontStyle.BoldItalic => new TextStyleChange(null, FontWeight.Bold, FontPosture.Italic, null),
      _ => throw new ArgumentOutOfRangeException(nameof(style))
    };

    internal static TextStyleChange TextNormal =>
      new TextStyleChange(FontFamily.Roman, FontWeight.Regular, FontPosture.Upright, FontCapitals.Normal);

    public bool Equals(TextStyleChange other) =>
      Family == other.Family && Weight == other.Weight && Posture == other.Posture && Capitals == other.Capitals;
    public override bool Equals(object obj) => obj is TextStyleChange other && Equals(other);
    public override int GetHashCode() => (Family, Weight, Posture, Capitals).GetHashCode();
  }
}
