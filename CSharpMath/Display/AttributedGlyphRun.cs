using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display {
  /// <summary>Like an attributed string, but the attributes other than Kern are required to be fixed
  /// over the whole string. We use KernedGlyph objects instead of Glyphs to
  /// allow us to set kern on a per-glyph basis.</summary>
  public class AttributedGlyphRun<TFont, TGlyph> where TFont : FrontEnd.IFont<TGlyph> {
    public AttributedGlyphRun(string text, IEnumerable<TGlyph> glyphs, TFont font, bool isPlaceHolder = false, Color? color = null) {
      Text = new StringBuilder(text);
      GlyphInfos = glyphs.Select(g => new GlyphInfo<TGlyph>(g) { Foreground = color }).ToList();
      Font = font;
      Placeholder = isPlaceHolder;
    }

    //Attributes
    public bool Placeholder { get; }
    public TFont Font { get; set; }

    //Text
    public StringBuilder Text { get; }
    public List<GlyphInfo<TGlyph>> GlyphInfos { get; }
    public IEnumerable<TGlyph> Glyphs => GlyphInfos.Select(g => g.Glyph);
    public int Length => GlyphInfos.Count;
    public override string ToString() => $"AttributedGlyphRun {Text}";
  }


  public static class AttributedGlyphRunExtensions {
    public static bool AttributesMatch<TFont, TGlyph>(
        this AttributedGlyphRun<TFont, TGlyph>? run1,
        AttributedGlyphRun<TFont, TGlyph>? run2) where TFont : FrontEnd.IFont<TGlyph> =>
      run1 != null
      && run2 != null
      && EqualityComparer<TFont>.Default.Equals(run1.Font, run2.Font);
  }
}
