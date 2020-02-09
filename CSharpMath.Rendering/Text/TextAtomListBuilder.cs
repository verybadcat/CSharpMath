using System;
using System.Collections;
using System.Collections.Generic;

namespace CSharpMath.Rendering {
  public class TextAtomListBuilder : IReadOnlyList<TextAtom> {
    readonly List<TextAtom> _list = new List<TextAtom>();
    private void Add(TextAtom atom) { _list.Add(atom); TextLength += atom.Range.Length; }
    public void Add() => Add(new TextAtom.ControlSpace(TextLength));
    public void Add(TextAtom atom, string accent, int sourceLength) =>
      Add(new TextAtom.Accent(atom, accent, TextLength, sourceLength));
    public void Add(string text, ReadOnlySpan<char> lookAheadForPunc) =>
      Add(new TextAtom.Text(text + lookAheadForPunc.ToString(), TextLength));
    public void Add(Structures.Space space, int sourceLength) =>
      Add(new TextAtom.Space(space, TextLength, sourceLength));
    public void Add(TextAtom atom, Enumerations.FontStyle style, int commandLength) =>
      Add(new TextAtom.Style(atom, style, TextLength, commandLength));
    public void Add(TextAtom atom, float fontSize, int commandLength) =>
      Add(new TextAtom.Size(atom, fontSize, TextLength, commandLength));
    public void Add(TextAtom atom, Structures.Color color, int commandLength) =>
      Add(new TextAtom.Color(atom, color, TextLength, commandLength));
    public Structures.Result Add(string mathLaTeX, bool displayStyle) {
      var mathSource = new MathSource(mathLaTeX);
      if (mathSource.ErrorMessage != null) return mathSource.ErrorMessage;
      Add(new TextAtom.Math(mathSource.MathList, displayStyle,
                            new Atoms.Range(TextLength, mathLaTeX.Length)));
      return Structures.Result.Ok();
    }
    public void Add(IReadOnlyList<TextAtom> textAtoms) =>
      Add(new TextAtom.List(textAtoms, TextLength));
    public void Break(int sourceLength) =>
      Add(new TextAtom.Newline(TextLength, sourceLength));
    public void Comment(string comment) =>
      Add(new TextAtom.Comment(comment, TextLength));
    public TextAtom.List Build() => new TextAtom.List(this, 0);
    public int TextLength { get; set; } = 0;
    public TextAtom Last => Count == 0 ? null : _list[Count - 1];
    public TextAtom this[int index] => _list[index];
    public int Count => _list.Count;
    public List<TextAtom>.Enumerator GetEnumerator() => _list.GetEnumerator();
    IEnumerator<TextAtom> IEnumerable<TextAtom>.GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
  }
}
