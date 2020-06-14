namespace CSharpMath.Display {
  using Atom;
  using Atoms = Atom.Atoms;
  using static InterElementSpaces.InterElementSpaceType;
  internal static class InterElementSpaces {
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
    // CA1814 is TRASH: https://github.com/MicrosoftDocs/visualstudio-docs/issues/3139
#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
    private static readonly InterElementSpaceType[,] Spaces = {
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
    public static float Get<TFont, TGlyph>(MathAtom left, MathAtom right,
      LineStyle style, TFont styleFont, FrontEnd.FontMathTable<TFont, TGlyph> mathTable)
      where TFont : FrontEnd.IFont<TGlyph> {
      static int GetInterElementSpaceArrayIndexForType(MathAtom atomType, bool row) =>
        atomType switch
        {
          Atoms.LargeOperator _ => 1,
          Atoms.BinaryOperator _ => 2,
          Atoms.Relation _ => 3,
          Atoms.Open _ => 4,
          Atoms.Close _ => 5,
          Atoms.Punctuation _ => 6,
          Atoms.Fraction _ => 7,
          Atoms.Inner _ => 7,
          Atoms.Table _ => 7, // Tables are considered as Inner
          Atoms.Radical _ when row => 8,
          Atoms.Radical _ => 0, // Radicals on the right are considered as Ord in rule 16.
          Atoms.Ordinary _ => 0,
          _ => 0 // Anything else are considered as Ord in rule 16.
        };
      var leftIndex = GetInterElementSpaceArrayIndexForType(left, true);
      var rightIndex = GetInterElementSpaceArrayIndexForType(right, false);
      var multiplier =
        Spaces[leftIndex, rightIndex] switch
        {
          Invalid => throw new Structures.InvalidCodePathException
                       ($"Invalid space between {left.TypeName} and {right.TypeName}"),
          None => 0,
          Thin => 3,
          NsThin => style < LineStyle.Script ? 3 : 0,
          NsMedium => style < LineStyle.Script ? 4 : 0,
          NsThick => style < LineStyle.Script ? 5 : 0,
          _ => throw new System.ComponentModel.InvalidEnumArgumentException
                  (nameof(style), (int)style, typeof(LineStyle))
        };
      return multiplier > 0 ? multiplier * mathTable.MuUnit(styleFont) : 0;
    }
  }
}