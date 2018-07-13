using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;
using Typography.TextLayout;
using Typography.TextServices;

namespace CSharpMath.Text {
  public static class TextBuilder {
    public static TextAtom Build(string text) {
      var breaker = new CustomBreaker();
      var breakList = new List<BreakAtInfo>();
      breaker.LoadBreakAtList(breakList);
      foreach (var info in breakList) {
        info.wordKind == WordKind.
      }
    }
    public static StringBuilder Unbuild(TextAtom atom, StringBuilder b) {
      switch (atom) {
        case TextAtom.Text t:
          return b.Append(t.Content);
        case TextAtom.Math m:
          return b.Append(Atoms.MathListBuilder.MathListToString(m.Content));
        case TextAtom.List l:
          foreach (var a in l.Content) {
            b.Append(Unbuild(a, b));
          }
          return b;
        case var a:
          throw new TypeAccessException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
      }
    }
  }
}