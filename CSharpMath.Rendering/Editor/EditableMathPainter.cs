using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  using Editor;
  public abstract class EditableMathPainter<TCanvas, TColor> : MathPainter<TCanvas, TColor> {
    public EditableMathPainter(float fontSize = DefaultFontSize * 3 / 2) : base(fontSize) { }

    readonly CaretView<Fonts, Glyph> caretView;
    readonly List<MathListIndex> highlighted;
    MathListIndex insertionIndex;
    protected override void SetRedisplay() {
      base.SetRedisplay();
      insertionIndex = MathListIndex.Level0Index(MathList.Atoms.Count);
    }

    protected override void DrawAfterSuccess(ICanvas c) {
      base.DrawAfterSuccess(c);
    }
  }
}
