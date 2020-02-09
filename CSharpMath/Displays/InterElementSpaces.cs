namespace CSharpMath.Displays {
  using static InterElementSpaceType;
  using Atoms;
  using Atom = Atoms.Atom;
  internal enum InterElementSpaceType {
    Invalid = -1,
    None = 0,
    Thin,
    ///<summary>Thin if not in script mode, else none</summary> 
    NsThin,
    ///<summary>Medium if not in script mode, else none</summary> 
    NsMedium,
    ///<summary>Thick if not in script mode, else none</summary> 
    NsThick
  }
  internal static class InterElementSpaces {
    public static int SpacingInMu(this InterElementSpaceType type, LineStyle _style) =>
      type switch
      {
        Invalid => -1,
        None => 0,
        Thin => 3,
        NsThin => _style < LineStyle.Script ? 3 : 0,
        NsMedium => _style < LineStyle.Script ? 4 : 0,
        NsThick => _style < LineStyle.Script ? 5 : 0,
        _ => throw new System.ComponentModel.InvalidEnumArgumentException
                (nameof(_style), (int)_style, typeof(LineStyle))
      };
    public static InterElementSpaceType[,] Spaces = {
//RIGHT ordinary  operator  binary    relation open      close    punct    fraction       LEFT
      { None,     Thin,     NsMedium, NsThick, None,     None,    None,    NsThin   }, // ordinary
      { Thin,     Thin,     Invalid,  NsThick, None,     None,    None,    NsThin   }, // operator
      { NsMedium, NsMedium, Invalid,  Invalid, NsMedium, Invalid, Invalid, NsMedium }, // binary
      { NsThick,  NsThick,  Invalid,  None,    NsThick,  None,    None,    NsThick  }, // relation
      { None,     None,     Invalid,  None,    None,     None,    None,    None     }, // open
      { None,     Thin,     NsMedium, NsThick, None,     None,    None,    NsThin   }, // close
      { NsThin,   NsThin,   Invalid,  NsThin,  NsThin,   NsThin,  NsThin,  NsThin   }, // punct
      { NsThin,   Thin,     NsMedium, NsThick, NsThin,   None,    NsThin,  NsThin   }, // fraction
      { NsMedium, NsThin,   NsMedium, NsThick, None,     None,    None,    NsThin   }, // radical
    };
    internal static float Get<TFont, TGlyph>(MathAtom left, MathAtom right,
      LineStyle style, TFont styleFont, FrontEnd.FontMathTable<TFont, TGlyph> mathTable)
      where TFont : Displays.IFont<TGlyph> {
      static int GetInterElementSpaceArrayIndexForType(MathAtom atomType, bool row) =>
        atomType switch
        {
          Atom.LargeOperator _ => 1,
          Atom.BinaryOperator _ => 2,
          Atom.Relation _ => 3,
          Atom.Open _ => 4,
          Atom.Close _ => 5,
          Atom.Punctuation _ => 6,
          Atom.Fraction _ => 7,
          Atom.Inner _ => 7,
          Atom.Table _ => 7, // Tables are considered as Inner
          Atom.Radical _ when row => 8,
          Atom.Radical _ => 0, // Radicals on the right are considered as Ord in rule 16.
          Atom.Ordinary _ => 0,
          _ => 0 // Anything else are considered as Ord in rule 16.
        };
      if (right is Atom.Prime)
        return 0;
      var leftIndex = GetInterElementSpaceArrayIndexForType(left, true);
      var rightIndex = GetInterElementSpaceArrayIndexForType(right, false);
      var spaceType = Spaces[leftIndex, rightIndex];
      if (spaceType == Invalid)
        throw new InvalidCodePathException($"Invalid space between {left.TypeName} and {right.TypeName}");
      var multiplier = spaceType.SpacingInMu(style);
      return multiplier > 0 ? multiplier * mathTable.MuUnit(styleFont) : 0;
    }
  }
}