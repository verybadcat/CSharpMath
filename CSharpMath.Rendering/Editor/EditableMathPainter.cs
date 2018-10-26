using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  using Editor;
  public abstract class EditableMathPainter<TCanvas, TColor> : MathPainter<TCanvas, TColor> {
    readonly CaretView<Fonts, Glyph> caretView;

  }
}
