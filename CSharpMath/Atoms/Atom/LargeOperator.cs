namespace CSharpMath.Atoms.Atom {
  /// <summary>A large operator such as sin/cos, integral, etc.</summary>
  public class LargeOperator : MathAtom {
    bool? _limits;
    /// <summary>
    /// True: \limits
    /// False: \nolimits
    /// Null: (unset, depends on line style)
    /// </summary>
    public bool? Limits { get => NoLimits ? false : _limits; set => _limits = value; }
    ///<summary>If true, overrides Limits and makes it treated as false</summary>
    public bool NoLimits { get; }
    public LargeOperator(string value, bool? limits, bool noLimits = false): base(value) {
      Limits = limits;
      NoLimits = noLimits;
    }
    public new LargeOperator Clone(bool finalize) => (LargeOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new LargeOperator(Nucleus, Limits, NoLimits);
    public override bool ScriptsAllowed => true;
    public override string DebugString => base.DebugString + Limits switch {
      true => @"\limits",
      false when !NoLimits => @"\nolimits",
      _ => ""
    };
    // Don't care about \limits or \nolimits
    public bool EqualsLargeOperator(LargeOperator op) => EqualsAtom(op);
  }
}