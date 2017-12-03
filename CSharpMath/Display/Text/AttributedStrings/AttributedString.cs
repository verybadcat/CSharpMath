using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString<TGlyph> {
    private List<AttributedGlyphRun<TGlyph>> _Runs { get; }
    public AttributedString(IEnumerable<AttributedGlyphRun<TGlyph>> runs = null) {
      _Runs = runs?.ToList() ?? new List<AttributedGlyphRun<TGlyph>>();
      FuseMatchingRuns();
    }
    public void SetFont(MathFont font) {
      _Runs.ForEach(r => r.Font = font);
    }
    public string Text {
      get {
        string r = "";
        foreach (var run in Runs) {
          r += run.Text;
        }
        return r;
      }
    }
    public int Length => _Runs.Sum(r => r.Length);
    public IEnumerable<AttributedGlyphRun<TGlyph>> Runs => _Runs;
    internal void FuseMatchingRuns() {
      for (int i=_Runs.Count-1; i>0; i--) {
        TryFuseRunAt(i);
      }
    }
    public bool TryFuseRunAt(int index) {
      if (_Runs[index].AttributesMatch(_Runs[index - 1])) {
        _Runs[index - 1].Text = _Runs[index - 1].Text.Concat(_Runs[index].Text).ToArray();
        _Runs.RemoveAt(index);
        return true;
      }
      return false;
    }
    public void AppendAttributedString(AttributedString<TGlyph> other) {
      _Runs.AddRange(other.Runs);
      FuseMatchingRuns();
    }

    internal void AppendGlyphRun(AttributedGlyphRun<TGlyph> run) {
      _Runs.Add(run);
      TryFuseRunAt(_Runs.Count - 1);
    }

    public override string ToString() => "AttributedString " + Text;
  }

  public static class AttributedStringExtensions {
    public static AttributedString<TGlyph> Combine<TGlyph>(AttributedString<TGlyph> attr1, AttributedString<TGlyph> attr2) {
      if (attr1 == null) {
        return attr2;
      }
      if (attr2 == null) {
        return attr1;
      }
      attr1.AppendAttributedString(attr2);
      return attr1;
    }
    public static AttributedString<TGlyph> Combine<TGlyph>(AttributedGlyphRun<TGlyph> run1, AttributedGlyphRun<TGlyph> run2)
      => AttributedStrings.FromGlyphRuns(run1, run2);

    public static AttributedString<TGlyph> Combine<TGlyph>(AttributedString<TGlyph> aStr, AttributedGlyphRun<TGlyph> run) {
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
