namespace CSharpMath.Atom.Atoms {
  /// <summary>AMSMath class 1: Prefix operator, e.g. \sin, \cos, \sum, \prod, \int</summary>
  public sealed class LargeOperator : MathAtom {
    bool? _limits;
    /// <summary>
    /// True: \limits
    /// False: \nolimits
    /// Null: (unset, depends on line style)
    /// </summary>
    public bool? Limits { get => ForceNoLimits ? false : _limits; set => _limits = value; }
    ///<summary>If true, overrides Limits and makes it treated as false</summary>
    public bool ForceNoLimits { get; }
    public LargeOperator(string value, bool? limits, bool forceNoLimits = false) : base(value) {
      Limits = limits;
      ForceNoLimits = forceNoLimits;
    }
    public new LargeOperator Clone(bool finalize) => (LargeOperator)base.Clone(finalize);
    protected override MathAtom CloneInside(bool finalize) =>
      new LargeOperator(Nucleus, Limits, ForceNoLimits);
    public override bool ScriptsAllowed => true;
    public override string DebugString => base.DebugString + Limits switch {
      true => @"\limits",
      false when !ForceNoLimits => @"\nolimits",
      _ => ""
    };
    // Don't care about \limits or \nolimits
    public bool EqualsLargeOperator(LargeOperator op) => EqualsAtom(op);
  }
}