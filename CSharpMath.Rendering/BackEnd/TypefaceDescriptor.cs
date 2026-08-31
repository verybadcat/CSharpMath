using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CSharpMath.Atom;
using Typography.OpenFont;
using Typography.OpenFont.Extensions;

namespace CSharpMath.Rendering.BackEnd {
  /// <summary>Explicit semantic metadata used when selecting a text typeface.</summary>
  public sealed class TypefaceDescriptor : IEquatable<TypefaceDescriptor> {
    public TypefaceDescriptor(Typeface typeface, FontFamily family, FontWeight weight,
      FontPosture posture, Typeface smallCapitalsTypeface = null,
      IReadOnlyDictionary<int, ushort> smallCapitalsGlyphMap = null,
      IEnumerable<string> supportedFeatures = null) {
      Typeface = typeface ?? throw new ArgumentNullException(nameof(typeface));
      if (!Enum.IsDefined(typeof(FontFamily), family)) throw new ArgumentOutOfRangeException(nameof(family));
      if (!Enum.IsDefined(typeof(FontWeight), weight)) throw new ArgumentOutOfRangeException(nameof(weight));
      if (!Enum.IsDefined(typeof(FontPosture), posture)) throw new ArgumentOutOfRangeException(nameof(posture));
      Family = family;
      Weight = weight;
      Posture = posture;
      SmallCapitalsTypeface = smallCapitalsTypeface;
      var map = new Dictionary<int, ushort>();
      foreach (var pair in smallCapitalsGlyphMap ?? new Dictionary<int, ushort>()) {
        if (pair.Key < 0 || pair.Key > 0x10ffff || pair.Key is >= 0xd800 and <= 0xdfff)
          throw new ArgumentOutOfRangeException(nameof(smallCapitalsGlyphMap));
        if (pair.Value == 0 || pair.Value >= (smallCapitalsTypeface ?? typeface).GlyphCount)
          throw new ArgumentOutOfRangeException(nameof(smallCapitalsGlyphMap));
        map.Add(pair.Key, pair.Value);
      }
      SmallCapitalsGlyphMap = new ReadOnlyDictionary<int, ushort>(map);
      var features = new HashSet<string>(StringComparer.Ordinal);
      foreach (var feature in supportedFeatures ?? Enumerable.Empty<string>()) {
        if (feature == null || feature.Length != 4 || feature.Any(c => c > 0x7f))
          throw new ArgumentException("Features must be four ASCII characters.", nameof(supportedFeatures));
        features.Add(feature);
      }
      SupportedFeatures = new ReadOnlyCollection<string>(features.ToArray());
    }
    public Typeface Typeface { get; }
    public FontFamily Family { get; }
    public FontWeight Weight { get; }
    public FontPosture Posture { get; }
    public Typeface SmallCapitalsTypeface { get; }
    public IReadOnlyDictionary<int, ushort> SmallCapitalsGlyphMap { get; }
    public IReadOnlyCollection<string> SupportedFeatures { get; }
    public bool Equals(TypefaceDescriptor other) => other != null &&
      ReferenceEquals(Typeface, other.Typeface) && Family == other.Family && Weight == other.Weight &&
      Posture == other.Posture && ReferenceEquals(SmallCapitalsTypeface, other.SmallCapitalsTypeface) &&
      SmallCapitalsGlyphMap.Count == other.SmallCapitalsGlyphMap.Count &&
      SmallCapitalsGlyphMap.All(pair => other.SmallCapitalsGlyphMap.TryGetValue(pair.Key, out var value) && value == pair.Value) &&
      SupportedFeatures.Count == other.SupportedFeatures.Count &&
      SupportedFeatures.All(other.SupportedFeatures.Contains);
    public override bool Equals(object obj) => Equals(obj as TypefaceDescriptor);
    public override int GetHashCode() {
      unchecked {
        var hash = (Typeface, Family, Weight, Posture, SmallCapitalsTypeface).GetHashCode();
        foreach (var pair in SmallCapitalsGlyphMap.OrderBy(pair => pair.Key)) hash = hash * 31 + pair.GetHashCode();
        foreach (var feature in SupportedFeatures.OrderBy(feature => feature, StringComparer.Ordinal)) hash = hash * 31 + feature.GetHashCode();
        return hash;
      }
    }

    internal static TypefaceDescriptor Adapt(Typeface typeface) {
      var flags = typeface.TranslateOS2FontStyle();
      var weight = (flags & TranslatedOS2FontStyle.BOLD) != 0
        ? FontWeight.Bold : FontWeight.Regular;
      var posture = (flags & TranslatedOS2FontStyle.OBLIQUE) != 0
        ? FontPosture.Slanted
        : (flags & TranslatedOS2FontStyle.ITALIC) != 0
          ? FontPosture.Italic : FontPosture.Upright;
      return new TypefaceDescriptor(typeface, FontFamily.Default, weight, posture);
    }
  }
}
