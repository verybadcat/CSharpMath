using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString<TMathFont, TGlyph>
    where TMathFont: MathFont<TGlyph> {
    private List<AttributedGlyphRun<TMathFont, TGlyph>> _Runs { get; }
    public AttributedString(IEnumerable<AttributedGlyphRun<TMathFont, TGlyph>> runs = null) {
      _Runs = runs?.ToList() ?? new List<AttributedGlyphRun<TMathFont, TGlyph>>();
      FuseMatchingRuns();
    }
    public void SetFont(TMathFont font) {
      _Runs.ForEach(r => r.Font = font);
    }
    public string Text {
      get {
        string r = "";
        foreach (var run in Runs) {
          r += run.Glyphs;
        }
        return r;
      }
    }
    public int Length => _Runs.Sum(r => r.Length);
    public IEnumerable<AttributedGlyphRun<TMathFont, TGlyph>> Runs => _Runs;
    internal void FuseMatchingRuns() {
      for (int i=_Runs.Count-1; i>0; i--) {
        TryFuseRunAt(i);
      }
    }
    public bool TryFuseRunAt(int index) {
      if (index > 0 &&_Runs[index].AttributesMatch(_Runs[index - 1])) {
        _Runs[index - 1].Glyphs = _Runs[index - 1].Glyphs.Concat(_Runs[index].Glyphs).ToArray();
        _Runs[index - 1].Text = _Runs[index - 1].Text + _Runs[index].Text;
        _Runs.RemoveAt(index);
        return true;
      }
      return false;
    }
    public void AppendAttributedString(AttributedString<TMathFont, TGlyph> other) {
      _Runs.AddRange(other.Runs);
      FuseMatchingRuns();
    }

    internal void AppendGlyphRun(AttributedGlyphRun<TMathFont, TGlyph> run) {
      _Runs.Add(run);
      TryFuseRunAt(_Runs.Count - 1);
    }

    public override string ToString() => "AttributedString " + Text;
  }

  public static class AttributedStringExtensions {
    public static AttributedString<TMathFont, TGlyph> Combine<TMathFont, TGlyph>(AttributedString<TMathFont, TGlyph> attr1, AttributedString<TMathFont, TGlyph> attr2) 
        where TMathFont: MathFont<TGlyph> {
      if (attr1 == null) {
        return attr2;
      }
      if (attr2 == null) {
        return attr1;
      }
      attr1.AppendAttributedString(attr2);
      return attr1;
    }
    public static AttributedString<TMathFont, TGlyph> Combine<TMathFont, TGlyph>(AttributedGlyphRun<TMathFont, TGlyph> run1, AttributedGlyphRun<TMathFont, TGlyph> run2)
      where TMathFont: MathFont<TGlyph>
      => AttributedStrings.FromGlyphRuns(run1, run2);

    public static AttributedString<TMathFont, TGlyph> Combine<TMathFont, TGlyph>(AttributedString<TMathFont, TGlyph> aStr, AttributedGlyphRun<TMathFont, TGlyph> run) 
      where TMathFont: MathFont<TGlyph> {
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
