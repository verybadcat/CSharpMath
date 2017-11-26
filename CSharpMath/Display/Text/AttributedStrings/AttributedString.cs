using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Display.Text {
  public class AttributedString {
    private List<AttributedGlyphRun> _Runs { get; }
    public AttributedString(IEnumerable<AttributedGlyphRun> runs = null) {
      _Runs = runs?.ToList() ?? new List<AttributedGlyphRun>();
      FuseMatchingRuns();
    }
    public void SetFont(MathFont font) {
      _Runs.ForEach(r => r.Font = font);
    }
    public int Length => _Runs.Sum(r => r.Length);
    public IEnumerable<AttributedGlyphRun> Runs => _Runs;
    internal void FuseMatchingRuns() {
      for (int i=_Runs.Count-1; i>0; i--) {
        TryFuseRunAt(i);
      }
    }
    public bool TryFuseRunAt(int index) {
      if (_Runs[index].AttributesMatch(_Runs[index - 1])) {
        _Runs[index - 1].Text += _Runs[index].Text;
        _Runs.RemoveAt(index);
        return true;
      }
      return false;
    }
    public void AppendAttributedString(AttributedString other) {
      _Runs.AddRange(other.Runs);
      FuseMatchingRuns();
    }

    internal void AppendGlyphRun(AttributedGlyphRun run) {
      _Runs.Add(run);
      TryFuseRunAt(_Runs.Count - 1);
    }
  }

  public static class AttributedStringExtensions {
    public static AttributedString Combine(AttributedString attr1, AttributedString attr2) {
      if (attr1 == null) {
        return attr2;
      }
      if (attr2 == null) {
        return attr1;
      }
      attr1.AppendAttributedString(attr2);
      return attr1;
    }
    public static AttributedString Combine(AttributedGlyphRun run1, AttributedGlyphRun run2)
      => AttributedStrings.FromGlyphRuns(run1, run2);

    public static AttributedString Combine(AttributedString aStr, AttributedGlyphRun run) {
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
