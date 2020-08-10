using System.Drawing;
using System.Collections;
using System.Collections.Generic;

namespace CSharpMath.Rendering.Text {
  using CSharpMath.Structures;
  public class TextAtomListBuilder : IReadOnlyList<TextAtom> {
    readonly List<TextAtom> _list = new List<TextAtom>();
    private void Add(TextAtom atom) => _list.Add(atom);
    public void ControlSpace() => Add(new TextAtom.ControlSpace());
    public void Accent(TextAtom atom, string accent) => Add(new TextAtom.Accent(atom, accent));
    public void Text(string text) {
      if (char.IsPunctuation(text, 0))
        switch (Last) {
          case TextAtom.Text { Content: var prevText }:
            Last = new TextAtom.Text(prevText + text);
            return;
          case TextAtom.Math { DisplayStyle: false, Content: var mathList }:
            mathList.Add(new Atom.Atoms.Punctuation(text));
            return;
        }
      Add(new TextAtom.Text(text));
    }
    public void Space(Space space) => Add(new TextAtom.Space(space));
    public void Style(TextAtom atom, Atom.FontStyle style) => Add(new TextAtom.Style(atom, style));
    public void Size(TextAtom atom, float fontSize) => Add(new TextAtom.Size(atom, fontSize));
    public void Color(TextAtom atom, Color color) => Add(new TextAtom.Colored(atom, color));
    public Result Math(string mathLaTeX, bool displayStyle, int startAt, ref int endAt) {
      var builder = new Atom.LaTeXParser(mathLaTeX);
      var (mathList, error) = builder.Build();
      if (error != null) {
        endAt = startAt - mathLaTeX.Length + builder.NextChar - 1;
        return Result.Err("[Math] " + error);
      } else {
        Add(new TextAtom.Math(mathList, displayStyle));
        return Result.Ok();
      }
    }
    public void List(IReadOnlyList<TextAtom> textAtoms) => Add(new TextAtom.List(textAtoms));
    public void Break() => Add(new TextAtom.Newline());
    public void Comment(string comment) => Add(new TextAtom.Comment(comment));
    public TextAtom Build() => _list.Count == 1 ? _list[0] : new TextAtom.List(this);
    public int TextLength { get; set; } = 0;
    [System.Diagnostics.CodeAnalysis.DisallowNull] // setter value cannot be null
    public TextAtom? Last { get => Count == 0 ? null : _list[Count - 1]; set => _list[Count - 1] = value; }
    public TextAtom this[int index] => _list[index];
    public int Count => _list.Count;
    public List<TextAtom>.Enumerator GetEnumerator() => _list.GetEnumerator();
    IEnumerator<TextAtom> IEnumerable<TextAtom>.GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
  }
}
