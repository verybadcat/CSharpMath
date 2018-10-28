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
      var path = c.GetPath();
      var point = caretView.handle.InitialPoint;
      path.BeginRead(1);
      path.Foreground = caretView.handle.ActualColor;
      path.MoveTo(point.X, point.Y);
      point = caretView.handle.NextPoint1;
      path.LineTo(point.X, point.Y);
      point = caretView.handle.NextPoint2;
      path.LineTo(point.X, point.Y);
      point = caretView.handle.NextPoint3;
      path.LineTo(point.X, point.Y);
      point = caretView.handle.FinalPoint;
      path.LineTo(point.X, point.Y);
      path.CloseContour();
      path.EndRead();
    }
  }
}
