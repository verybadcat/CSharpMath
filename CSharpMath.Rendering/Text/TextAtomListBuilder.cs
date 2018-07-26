using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public class TextAtomListBuilder : IReadOnlyList<TextAtom> {
    List<TextAtom> _list = new List<TextAtom>();

    private void Add(TextAtom atom) { _list.Add(atom); TextLength += atom.Range.Length; }
    public void Add(string text) => Add(new TextAtom.Text(text, text.Length));
    public string Add(string mathLaTeX, bool displayStyle) {
      var mathSource = new Rendering.MathSource(mathLaTeX);
      if (mathSource.ErrorMessage.IsNonEmpty()) return mathSource.ErrorMessage;
      var mathList = mathSource.MathList;
      mathList.Insert(0, new Atoms.Style(displayStyle ? Enumerations.LineStyle.Display : Enumerations.LineStyle.Text));
      Add(new TextAtom.Math(mathList, new Atoms.Range(TextLength, mathLaTeX.Length)));
      return null;
    }
    public void Add(IReadOnlyList<TextAtom> textAtoms) => Add(new TextAtom.List(textAtoms, TextLength));
    public TextAtom.List Build() => new TextAtom.List(this, 0);

    public int TextLength { get; private set; } = 0;
    public TextAtom this[int index] => _list[index];
    public int Count => _list.Count;
    List<TextAtom>.Enumerator GetEnumerator() => _list.GetEnumerator();

    IEnumerator<TextAtom> IEnumerable<TextAtom>.GetEnumerator() => _list.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
  }
}
