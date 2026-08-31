using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace CSharpMath.Rendering.Text {
  public class TextAtomListBuilder : IReadOnlyList<TextAtom> {
    readonly List<TextAtom> _list = new List<TextAtom>();
    string? declaration;
    float magnification = 1;
    int runStart = -1;
    private void Add(TextAtom atom) => _list.Add(atom);
    void CloseRun() {
      if (runStart >= 0 && runStart < _list.Count) {
        var content = _list.GetRange(runStart, _list.Count - runStart);
        _list.RemoveRange(runStart, _list.Count - runStart);
        _list.Add(new TextAtom.RelativeSize(BuildList(content), declaration!));
      }
      runStart = -1;
    }
    static TextAtom BuildList(List<TextAtom> content) => content.Count == 1 ? content[0] : new TextAtom.List(content);
    internal void RelativeSize(string name, float ratio) {
      CloseRun(); declaration = name; magnification = ratio; runStart = _list.Count;
    }
    internal string? RelativeDeclaration => declaration;
    internal float RelativeMagnification => magnification;
    internal void RestoreRelativeSize(string? name, float ratio) {
      CloseRun(); declaration = name; magnification = ratio;
      if (name != null) runStart = _list.Count;
    }
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
    public void Space(Atom.Length space) => Add(new TextAtom.Space(space));
    public void Style(TextAtom atom, Atom.FontStyle style) => Add(new TextAtom.Style(atom, style));
    public void Style(TextAtom atom, Atom.TextStyleChange styleChange) => Add(new TextAtom.Style(atom, styleChange));
    public void Size(TextAtom atom, float fontSize) => Add(new TextAtom.Size(atom, fontSize));
    public void Color(TextAtom atom, Color color) => Add(new TextAtom.Colored(atom, color));
    public Atom.Result Math(string mathLaTeX, bool displayStyle, int startAt, ref int endAt) {
      var builder = new Atom.LaTeXParser(mathLaTeX);
      var (mathList, error) = builder.Build();
      if (error != null) {
        endAt = startAt - mathLaTeX.Length + builder.NextChar - 1;
        return Atom.Result.Err("[Math] " + error);
      } else {
        Add(new TextAtom.Math(mathList, displayStyle));
        return Atom.Result.Ok();
      }
    }
    public void List(IReadOnlyList<TextAtom> textAtoms) => Add(new TextAtom.List(textAtoms));
    public void Break() => Add(new TextAtom.Newline());
    public void Comment(string comment) => Add(new TextAtom.Comment(comment));
    public TextAtom Build() { CloseRun(); return _list.Count == 1 ? _list[0] : new TextAtom.List(this); }
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
