using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CSharpMath.Display.Text {
  using Structures;
  /// <summary>Like an attributed string, but the attributes other than Kern are required to be fixed
  /// over the whole string. We use KernedGlyph objects instead of Glyphs to
  /// allow us to set kern on a per-glyph basis.</summary>
  public class AttributedGlyphRun<TFont, TGlyph>
    where TFont : IFont<TGlyph> {
    public AttributedGlyphRun(string text, IEnumerable<TGlyph> glyphs, TFont font, Color? foreground = null, bool isPlaceHolder = false) {
      Text = new StringBuilder(text);
      KernedGlyphs = glyphs.Select(g => new GlyphWithInfo<TGlyph>(g)).ToList();
      Font = font;
      Foreground = foreground;
      Placeholder = isPlaceHolder;
    }

    //Attributes
    public bool Placeholder { get; }
    public Color? Foreground { get; set; }
    public TFont Font { get; set; }

    //Text
    public StringBuilder Text { get; }
    public List<GlyphWithInfo<TGlyph>> KernedGlyphs { get; }
    public IEnumerable<TGlyph> Glyphs => KernedGlyphs.Select(g => g.Glyph);
    public int Length => KernedGlyphs.Count;
    
    public override string ToString() => $"AttributedGlyphRun {KernedGlyphs.Count} glyphs";
  }


  public static class AttributedGlyphRunExtensions {
    public static bool AttributesMatch<TFont, TGlyph>(this AttributedGlyphRun<TFont, TGlyph> run1, AttributedGlyphRun<TFont, TGlyph> run2) where TFont : IFont<TGlyph> =>
      !(run1 is null || run2 is null) &&
      EqualityComparer<TFont>.Default.Equals(run1.Font, run2.Font) &&
      run1.Foreground.Equals(run2.Foreground);
  }
}
