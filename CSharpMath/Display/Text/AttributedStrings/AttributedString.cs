using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString<TFont, TGlyph> where TFont: IFont<TGlyph> {
    private readonly List<AttributedGlyphRun<TFont, TGlyph>> _runs;

    public AttributedString(IEnumerable<AttributedGlyphRun<TFont, TGlyph>> runs = null) {
      _runs = runs?.ToList() ?? new List<AttributedGlyphRun<TFont, TGlyph>>();
      FuseMatchingRuns();
    }
    public void SetFont(TFont font) {
      foreach (var r in _runs)
        r.Font = font;
    }
    public string Text => string.Concat(Runs.Select(r => r.Text));
    public int Length => _runs.Sum(r => r.Length);
    public IEnumerable<AttributedGlyphRun<TFont, TGlyph>> Runs => _runs;
    internal void FuseMatchingRuns() {
      for (int i = _runs.Count - 1; i > 0; i--)
        TryFuseRunAt(i);
    }
    public bool TryFuseRunAt(int index) {
      if (index > 0 && _runs[index].AttributesMatch(_runs[index - 1])) {
        _runs[index - 1].GlyphInfos.AddRange(_runs[index].GlyphInfos);
        _runs[index - 1].Text.Append(_runs[index].Text);
        _runs.RemoveAt(index);
        return true;
      }
      return false;
    }
    public void AppendAttributedString(AttributedString<TFont, TGlyph> other) {
      _runs.AddRange(other.Runs);
      FuseMatchingRuns();
    }

    internal void AppendGlyphRun(AttributedGlyphRun<TFont, TGlyph> run) {
      _runs.Add(run);
      TryFuseRunAt(_runs.Count - 1);
    }

    public void Clear() => _runs.Clear();

    public override string ToString() => "AttributedString " + Text;
  }

  [Obsolete("Is any code using this?", true)]
  public static class AttributedStringExtensions {
    [Obsolete("Is any code using this?", true)]
    public static AttributedString<TFont, TGlyph> Combine<TFont, TGlyph>(AttributedString<TFont, TGlyph> attr1, AttributedString<TFont, TGlyph> attr2) 
        where TFont: IFont<TGlyph> {
      if (attr1 == null) {
        return attr2;
      }
      if (attr2 == null) {
        return attr1;
      }
      attr1.AppendAttributedString(attr2);
      return attr1;
    }

    [Obsolete("Is any code using this?", true)]
    public static AttributedString<TFont, TGlyph> Combine<TFont, TGlyph>(AttributedGlyphRun<TFont, TGlyph> run1, AttributedGlyphRun<TFont, TGlyph> run2)
      where TFont: IFont<TGlyph>
      => AttributedStrings.FromGlyphRuns(run1, run2);

    [Obsolete("Is any code using this?", true)]
    public static AttributedString<TFont, TGlyph> Combine<TFont, TGlyph>(AttributedString<TFont, TGlyph> aStr, AttributedGlyphRun<TFont, TGlyph> run) 
      where TFont: IFont<TGlyph> {
      if (aStr == null) {
        return AttributedStrings.FromGlyphRuns(run);
      } else {
        if (run != null) {
          aStr.AppendGlyphRun(run);
        }
        return aStr;
      }
    }
    
  }
}
