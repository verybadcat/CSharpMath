using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString<TFont, TGlyph>
    where TFont: IMathFont<TGlyph> {
    private List<AttributedGlyphRun<TFont, TGlyph>> _Runs { get; }
    public AttributedString(IEnumerable<AttributedGlyphRun<TFont, TGlyph>> runs = null) {
      _Runs = runs?.ToList() ?? new List<AttributedGlyphRun<TFont, TGlyph>>();
      FuseMatchingRuns();
    }
    public void SetFont(TFont font) {
      foreach (var r in _Runs) {
        r.Font = font;
      }
    }
    public string Text => string.Concat(Runs.Select(r => r.Text));
    public int Length => _Runs.Sum(r => r.Length);
    public IEnumerable<AttributedGlyphRun<TFont, TGlyph>> Runs => _Runs;
    internal void FuseMatchingRuns() {
      for (int i=_Runs.Count-1; i>0; i--) {
        TryFuseRunAt(i);
      }
    }
    public bool TryFuseRunAt(int index) {
      if (index > 0 && _Runs[index].AttributesMatch(_Runs[index - 1])) {
        _Runs[index - 1].KernedGlyphs.AddRange(_Runs[index].KernedGlyphs);
        _Runs[index - 1].Text.Append(_Runs[index].Text);
        _Runs.RemoveAt(index);
        return true;
      }
      return false;
    }
    public void AppendAttributedString(AttributedString<TFont, TGlyph> other) {
      _Runs.AddRange(other.Runs);
      FuseMatchingRuns();
    }

    internal void AppendGlyphRun(AttributedGlyphRun<TFont, TGlyph> run) {
      _Runs.Add(run);
      TryFuseRunAt(_Runs.Count - 1);
    }

    public void Clear() => _Runs.Clear();

    public override string ToString() => "AttributedString " + Text;
  }

  [Obsolete("Is any code using this?", true)]
  public static class AttributedStringExtensions {
    [Obsolete("Is any code using this?", true)]
    public static AttributedString<TFont, TGlyph> Combine<TFont, TGlyph>(AttributedString<TFont, TGlyph> attr1, AttributedString<TFont, TGlyph> attr2) 
        where TFont: IMathFont<TGlyph> {
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
      where TFont: IMathFont<TGlyph>
      => AttributedStrings.FromGlyphRuns(run1, run2);

    [Obsolete("Is any code using this?", true)]
    public static AttributedString<TFont, TGlyph> Combine<TFont, TGlyph>(AttributedString<TFont, TGlyph> aStr, AttributedGlyphRun<TFont, TGlyph> run) 
      where TFont: IMathFont<TGlyph> {
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
