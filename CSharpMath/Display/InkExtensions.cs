using System;
using System.Drawing;
using CSharpMath.Atom.Atoms;
using CSharpMath.Display.Displays;
using CSharpMath.Display.FrontEnd;

namespace CSharpMath {
  using Display;

  /// <summary>Computes the horizontal painted extent without changing advance metrics.
  /// Positions in composite displays are normalized back to that display's origin.</summary>
  public static partial class Extensions {
    public static float InkWidth<TFont, TGlyph>(this IDisplay<TFont, TGlyph> display)
      where TFont : IFont<TGlyph> {
      if (display is IInkDisplay ink) return Math.Max(display.Width, ink.InkWidth);
      if (display is ListDisplay<TFont, TGlyph> list) {
        var result = list.Width;
        foreach (var child in list.Displays) result = Math.Max(result, child.Position.X + child.InkWidth());
        return result;
      }
      if (display is TextLineDisplay<TFont, TGlyph> line) {
        var result = line.Width;
        foreach (var run in line.Runs) result = Math.Max(result, run.Position.X + run.InkWidth);
        return result;
      }
      float resultComposite = display.Width;
      void Add(IDisplay<TFont, TGlyph>? child) {
        if (child is not null)
          resultComposite = Math.Max(resultComposite, child.Position.X - display.Position.X + child.InkWidth());
      }
      switch (display) {
        case FractionDisplay<TFont, TGlyph> fraction:
          Add(fraction.Numerator); Add(fraction.Denominator); break;
        case RadicalDisplay<TFont, TGlyph> radical:
          Add(radical.Radicand); Add(radical.Degree); break;
        case AccentDisplay<TFont, TGlyph> accent:
          Add(accent.Accentee); Add(accent.Accent); break;
        case InnerDisplay<TFont, TGlyph> inner:
          Add(inner.Left); Add(inner.Inner); Add(inner.Right); break;
        case LargeOpLimitsDisplay<TFont, TGlyph> limits:
          Add(limits.NucleusDisplay); Add(limits.UpperLimit); Add(limits.LowerLimit); break;
        case StackDisplay<TFont, TGlyph> stack:
          Add(stack.Base); Add(stack.Over); Add(stack.Under); break;
        case OverOrUnderlineDisplay<TFont, TGlyph> bar:
          Add(bar.Inner); break;
        case UnderAnnotationDisplay<TFont, TGlyph> annotation:
          Add(annotation.Inner); Add(annotation.UnderList); Add(annotation.AnnotationGlyph); break;
        case BoxDisplay<TFont, TGlyph> box:
          if (box.DrawChild) {
            // BoxDisplay updates Child.Position lazily in Draw. Measurement must
            // reproduce that offset instead of observing the stale position.
            var offset = box.KeepWidth ? 0 : box.HAlign switch {
              BoxHAlign.Right => -box.Child.Width,
              BoxHAlign.Center => -box.Child.Width / 2,
              _ => 0
            };
            resultComposite = Math.Max(resultComposite, offset + box.Child.InkWidth());
          }
          break;
      }
      return resultComposite;
    }
  }
}
