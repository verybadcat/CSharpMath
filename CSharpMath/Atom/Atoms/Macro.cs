using System.Collections.Generic;

namespace CSharpMath.Atom.Atoms {
  /// <summary>A `#N` argument reference inside a macro's expansion template.
  /// A sentinel that only exists between "the template was parsed" and "the macro
  /// was expanded"; every instance is consumed by <see cref="Macro.Expansion"/>.</summary>
  public sealed class MacroParameter : MathAtom {
    /// <summary>The 1-based argument this placeholder stands for (1…9).</summary>
    public int ArgumentIndex { get; }
    public MacroParameter(int argumentIndex) : base("#" + argumentIndex) =>
      ArgumentIndex = argumentIndex;
    public override bool ScriptsAllowed => true;
    public new MacroParameter Clone(bool finalize) => (MacroParameter)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new MacroParameter(ArgumentIndex);
  }

  /// <summary>An unexpanded macro invocation (\pmod, \mod, \pod, \implies, …),
  /// expanded away when the list is finalized so it never reaches the typesetter.</summary>
  public sealed class Macro : MathAtom {
    /// <summary>The command name without the leading backslash, e.g. "pmod".</summary>
    public string Command { get; }
    /// <summary>The parsed arguments in invocation order (deep copies).</summary>
    public IReadOnlyList<MathList> Arguments { get; }
    /// <summary>The expansion template: a raw list whose `#N` references are
    /// <see cref="MacroParameter"/> atoms.</summary>
    public MathList TemplateExpression { get; }
    public override bool ScriptsAllowed => true;
    public Macro(string command, IEnumerable<MathList> arguments, MathList templateExpression) :
      base(string.Empty) {
      Command = command;
      var args = new List<MathList>();
      foreach (var argument in arguments)
        args.Add(argument.Clone(false));
      Arguments = args;
      TemplateExpression = templateExpression.Clone(false);
    }
    public new Macro Clone(bool finalize) => (Macro)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new Macro(Command, Arguments, TemplateExpression);
    public override string DebugString =>
      new System.Text.StringBuilder(@"\").Append(Command)
        .AppendDebugStringOfScripts(this).ToString();
    /// <summary>The raw atom stream this invocation stands for. Deep-copied throughout,
    /// so the stored template and arguments stay pristine for serialization and repeated
    /// expansions.</summary>
    public MathList Expansion() {
      var output = new MathList();
      foreach (var templateAtom in TemplateExpression) {
        if (!(templateAtom is MacroParameter parameter)) {
          output.Add(templateAtom.Clone(false));
          continue;
        }
        var index = parameter.ArgumentIndex;
        if (index < 1 || index > Arguments.Count) {
          // Arity disagreement is a bug in the macro table; carry the placeholder
          // through rather than making an argument silently vanish.
          output.Add(parameter.Clone(false));
          continue;
        }
        output.Append(Arguments[index - 1].Clone(false));
      }
      var flat = ExpandMacros(output);
      TransferScriptsTo(flat);
      return flat;
    }

    private void TransferScriptsTo(MathList expansion) {
      if (Superscript.IsEmpty() && Subscript.IsEmpty()) return;
      MathAtom? target = null;
      for (int i = expansion.Count - 1; i >= 0; i--) {
        if (expansion[i].ScriptsAllowed) {
          target = expansion[i];
          break;
        }
      }
      bool collides =
        target == null
        || (Superscript.IsNonEmpty() && target!.Superscript.IsNonEmpty())
        || (Subscript.IsNonEmpty() && target!.Subscript.IsNonEmpty());
      if (collides) {
        target = new Ordinary(string.Empty);
        expansion.Add(target);
      }
      if (Superscript.IsNonEmpty())
        target!.Superscript.Append(Superscript.Clone(false));
      if (Subscript.IsNonEmpty())
        target!.Subscript.Append(Subscript.Clone(false));
    }

    /// <summary>A copy of the list with every top-level <see cref="Macro"/> replaced by
    /// its raw expansion. Non-macro atoms are carried over by reference.</summary>
    internal static MathList ExpandMacros(MathList list) {
      var expanded = new MathList();
      foreach (var atom in list) {
        if (atom is Macro macro)
          expanded.Append(macro.Expansion());
        else
          expanded.Add(atom);
      }
      return expanded;
    }
  }
}
