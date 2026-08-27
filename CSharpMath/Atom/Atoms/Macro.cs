using System.Collections.Generic;
namespace CSharpMath.Atom.Atoms {
  public sealed class Macro : MathAtom {
    public string Command { get; }
    public IReadOnlyList<string> Arguments { get; }
    internal MathList RawExpansion { get; }
    public override bool ScriptsAllowed => true;
    internal Macro(string command, IReadOnlyList<string> arguments, MathList rawExpansion) : base(string.Empty) {
      Command = command; Arguments = new System.Collections.ObjectModel.ReadOnlyCollection<string>(new List<string>(arguments)); RawExpansion = rawExpansion.Clone(false);
    }
    public new Macro Clone(bool finalize) => (Macro)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) => new Macro(Command, Arguments, RawExpansion);
    public override string DebugString => new System.Text.StringBuilder(@"\").Append(Command).AppendDebugStringOfScripts(this).ToString();
    public MathList Expansion() {
      var output = new MathList();
      foreach (var atom in RawExpansion)
        if (atom is Macro nested) output.Append(nested.Expansion()); else output.Add(atom.Clone(false));
      TransferScriptsTo(output); return output;
    }
    private void TransferScriptsTo(MathList expansion) {
      if (Superscript.IsEmpty() && Subscript.IsEmpty()) return;
      MathAtom? target = null;
      for (var i = expansion.Count - 1; i >= 0; i--) if (expansion[i].ScriptsAllowed) { target = expansion[i]; break; }
      if (target == null || Superscript.IsNonEmpty() && target.Superscript.IsNonEmpty() || Subscript.IsNonEmpty() && target.Subscript.IsNonEmpty()) {
        target = new Ordinary(string.Empty); expansion.Add(target);
      }
      if (Superscript.IsNonEmpty()) target.Superscript.Append(Superscript.Clone(false));
      if (Subscript.IsNonEmpty()) target.Subscript.Append(Subscript.Clone(false));
    }
    internal static MathList ExpandMacros(MathList list) {
      var expanded = new MathList();
      foreach (var atom in list) if (atom is Macro macro) expanded.Append(macro.Expansion()); else expanded.Add(atom);
      return expanded;
    }
  }
}
