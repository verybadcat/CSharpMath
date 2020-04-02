using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharpMath.Rendering.Text {
  using CSharpMath.Structures;
  public class TextAtomListBuilder : IReadOnlyList<TextAtom> {
    readonly List<TextAtom> _list = new List<TextAtom>();
    private void Add(TextAtom atom) => _list.Add(atom);
    public void ControlSpace() => Add(new TextAtom.ControlSpace());
    public void Accent(TextAtom atom, string accent) =>
      Add(new TextAtom.Accent(atom, accent));
    public void Text(string text, ReadOnlySpan<char> lookAheadForPunc) =>
      Add(new TextAtom.Text(text + lookAheadForPunc.ToString()));
    public void Space(Space space) => Add(new TextAtom.Space(space));
    public void Style(TextAtom atom, Atom.FontStyle style) => Add(new TextAtom.Style(atom, style));
    public void Size(TextAtom atom, float fontSize) => Add(new TextAtom.Size(atom, fontSize));
    public void Color(TextAtom atom, Color color) => Add(new TextAtom.Color(atom, color));
    public Result Math(string mathLaTeX, bool displayStyle) =>
      Atom.LaTeXParser.TryMathListFromLaTeX(mathLaTeX).Bind(mathList =>
        Add(new TextAtom.Math(mathList, displayStyle)));
    public void List(IReadOnlyList<TextAtom> textAtoms) => Add(new TextAtom.List(textAtoms));
    public void Break() => Add(new TextAtom.Newline());
    public void Comment(string comment) => Add(new TextAtom.Comment(comment));
    public TextAtom Build() => _list.Count == 1 ? _list[0] : new TextAtom.List(this);
    public int TextLength { get; set; } = 0;
    public TextAtom? Last => Count == 0 ? null : _list[Count - 1];
    public TextAtom this[int index] => _list[index];
    public int Count => _list.Count;
    public List<TextAtom>.Enumerator GetEnumerator() => _list.GetEnumerator();
    IEnumerator<TextAtom> IEnumerable<TextAtom>.GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
  }
}
