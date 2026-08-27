namespace CSharpMath.Atom.Atoms {
  /// <summary>One of TeX's explicitly sized delimiters (\big, \Big, etc.).</summary>
  public sealed class LargeDelimiter : MathAtom {
    public enum DelimiterSize { Size1 = 1, Size2, Size3, Size4 }
    public DelimiterSize Size { get; }
    public LargeDelimiter(string nucleus, DelimiterSize size, System.Type mathClass) : base(nucleus) {
      if (size < DelimiterSize.Size1 || size > DelimiterSize.Size4) throw new System.ArgumentOutOfRangeException(nameof(size));
      if (mathClass != typeof(Ordinary) && mathClass != typeof(Open) && mathClass != typeof(Close) && mathClass != typeof(Relation))
        throw new System.ArgumentException("Large delimiter must be ordinary, open, close, or relation", nameof(mathClass));
      Size = size;
      MathClass = mathClass;
    }
    public System.Type MathClass { get; }
    public override bool ScriptsAllowed => true;
    protected override MathAtom CloneInside(bool finalize) => new LargeDelimiter(Nucleus, Size, MathClass);
    public new LargeDelimiter Clone(bool finalize) => (LargeDelimiter)base.Clone(finalize);
  }
}
