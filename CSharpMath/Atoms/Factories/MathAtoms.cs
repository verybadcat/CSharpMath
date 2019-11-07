using CSharpMath.Constants;
using CSharpMath.Enumerations;
using CSharpMath.Helpers;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;

namespace CSharpMath.Atoms {
  //https://mirror.hmc.edu/ctan/macros/latex/contrib/unicode-math/unimath-symbols.pdf
  public static class MathAtoms {
    private static AliasDictionary<string, MathAtom> _commands;
    public static AliasDictionary<string, MathAtom> Commands =>
      _commands ?? (_commands = new AliasDictionary<string, MathAtom> {
         { "square", Placeholder },
         
         // Greek characters
         { "alpha", Create(MathAtomType.Variable, "\u03B1") },
         { "beta", Create(MathAtomType.Variable, "\u03B2") },
         { "gamma", Create(MathAtomType.Variable, "\u03B3") },
         { "delta", Create(MathAtomType.Variable, "\u03B4") },
         { "varepsilon", Create(MathAtomType.Variable, "\u03B5") },
         { "zeta", Create(MathAtomType.Variable, "\u03B6") },
         { "eta", Create(MathAtomType.Variable, "\u03B7") },
         { "theta", Create(MathAtomType.Variable, "\u03B8") },
         { "iota", Create(MathAtomType.Variable, "\u03B9") },
         { "kappa", Create(MathAtomType.Variable, "\u03BA") },
         { "lambda", Create(MathAtomType.Variable, "\u03BB") },
         { "mu", Create(MathAtomType.Variable, "\u03BC") },
         { "nu", Create(MathAtomType.Variable, "\u03BD") },
         { "xi", Create(MathAtomType.Variable, "\u03BE") },
         { "omicron", Create(MathAtomType.Variable, "\u03BF") },
         { "pi", Create(MathAtomType.Variable, "\u03C0") },
         { "rho", Create(MathAtomType.Variable, "\u03C1") },
         { "varsigma", Create(MathAtomType.Variable, "\u03C2") },
         { "sigma", Create(MathAtomType.Variable, "\u03C3") },
         { "tau", Create(MathAtomType.Variable, "\u03C4") },
         { "upsilon", Create(MathAtomType.Variable, "\u03C5") },
         { "varphi", Create(MathAtomType.Variable, "\u03C6") },
         { "chi", Create(MathAtomType.Variable, "\u03C7") },
         { "psi", Create(MathAtomType.Variable, "\u03C8") },
         { "omega", Create(MathAtomType.Variable, "\u03C9") },
         { "vartheta", Create(MathAtomType.Variable, "\u03D1") },
         { "phi", Create(MathAtomType.Variable, "\u03D5") },
         { "varpi", Create(MathAtomType.Variable, "\u03D6") },
         { "varkappa", Create(MathAtomType.Variable, "\u03F0") },
         { "varrho", Create(MathAtomType.Variable, "\u03F1") },
         { "epsilon", Create(MathAtomType.Variable, "\u03F5") },
         // Capital greek characters
         { "Gamma", Create(MathAtomType.Variable, "\u0393") },
         { "Delta", Create(MathAtomType.Variable, "\u0394") },
         { "Theta", Create(MathAtomType.Variable, "\u0398") },
         { "Lambda", Create(MathAtomType.Variable, "\u039B") },
         { "Xi", Create(MathAtomType.Variable, "\u039E") },
         { "Pi", Create(MathAtomType.Variable, "\u03A0") },
         { "Sigma", Create(MathAtomType.Variable, "\u03A3") },
         { "Upsilon", Create(MathAtomType.Variable, "\u03A5") },
         { "Phi", Create(MathAtomType.Variable, "\u03A6") },
         { "Psi", Create(MathAtomType.Variable, "\u03A8") },
         { "Omega", Create(MathAtomType.Variable, "\u03A9") },
         
         // Open
         { "lceil", Create(MathAtomType.Open, "\u2308") },
         { "lfloor", Create(MathAtomType.Open, "\u230A") },
         { "langle", Create(MathAtomType.Open, "\u27E8") },
         { "lgroup", Create(MathAtomType.Open, "\u27EE") },
         
         // Close
         { "rceil", Create(MathAtomType.Close, "\u2309") },
         { "rfloor", Create(MathAtomType.Close, "\u230B") },
         { "rangle", Create(MathAtomType.Close, "\u27E9") },
         { "rgroup", Create(MathAtomType.Close, "\u27EF") },
         
         // Arrows
         { "leftarrow", "gets", Create(MathAtomType.Relation, "\u2190") },
         { "uparrow", Create(MathAtomType.Relation, "\u2191") },
         { "rightarrow", "to", Create(MathAtomType.Relation, "\u2192") },
         { "downarrow", Create(MathAtomType.Relation, "\u2193") },
         { "leftrightarrow", Create(MathAtomType.Relation, "\u2194") },
         { "updownarrow", Create(MathAtomType.Relation, "\u2195") },
         { "nwarrow", Create(MathAtomType.Relation, "\u2196") },
         { "nearrow", Create(MathAtomType.Relation, "\u2197") },
         { "searrow", Create(MathAtomType.Relation, "\u2198") },
         { "swarrow", Create(MathAtomType.Relation, "\u2199") },
         { "mapsto", Create(MathAtomType.Relation, "\u21A6") },
         { "Leftarrow", Create(MathAtomType.Relation, "\u21D0") },
         { "Uparrow", Create(MathAtomType.Relation, "\u21D1") },
         { "Rightarrow", Create(MathAtomType.Relation, "\u21D2") },
         { "Downarrow", Create(MathAtomType.Relation, "\u21D3") },
         { "Leftrightarrow", Create(MathAtomType.Relation, "\u21D4") },
         { "Updownarrow", Create(MathAtomType.Relation, "\u21D5") },
         { "longleftarrow", Create(MathAtomType.Relation, "\u27F5") },
         { "longrightarrow", Create(MathAtomType.Relation, "\u27F6") },
         { "longleftrightarrow", Create(MathAtomType.Relation, "\u27F7") },
         { "Longleftarrow", Create(MathAtomType.Relation, "\u27F8") },
         { "Longrightarrow", Create(MathAtomType.Relation, "\u27F9") },
         { "Longleftrightarrow", "iff", Create(MathAtomType.Relation, "\u27FA") },
         
         // Relations
         { "leq", "le", Create(MathAtomType.Relation, Symbols.LessEqual) },
         { "geq", "ge", Create(MathAtomType.Relation, Symbols.GreaterEqual) },
         { "neq", "ne", Create(MathAtomType.Relation, Symbols.NotEqual) },
         { "in", Create(MathAtomType.Relation, "\u2208") },
         { "notin", Create(MathAtomType.Relation, "\u2209") },
         { "ni", Create(MathAtomType.Relation, "\u220B") },
         { "propto", Create(MathAtomType.Relation, "\u221D") },
         { "mid", Create(MathAtomType.Relation, "\u2223") },
         { "parallel", Create(MathAtomType.Relation, "\u2225") },
         { "sim", Create(MathAtomType.Relation, "\u223C") },
         { "simeq", Create(MathAtomType.Relation, "\u2243") },
         { "cong", Create(MathAtomType.Relation, "\u2245") },
         { "approx", Create(MathAtomType.Relation, "\u2248") },
         { "asymp", Create(MathAtomType.Relation, "\u224D") },
         { "doteq", Create(MathAtomType.Relation, "\u2250") },
         { "equiv", Create(MathAtomType.Relation, "\u2261") },
         { "gg", Create(MathAtomType.Relation, "\u226A") },
         { "ll", Create(MathAtomType.Relation, "\u226B") },
         { "prec", Create(MathAtomType.Relation, "\u227A") },
         { "succ", Create(MathAtomType.Relation, "\u227B") },
         { "subset", Create(MathAtomType.Relation, "\u2282") },
         { "supset", Create(MathAtomType.Relation, "\u2283") },
         { "subseteq", Create(MathAtomType.Relation, "\u2286") },
         { "supseteq", Create(MathAtomType.Relation, "\u2287") },
         { "sqsubset", Create(MathAtomType.Relation, "\u228F") },
         { "sqsupset", Create(MathAtomType.Relation, "\u2290") },
         { "sqsubseteq", Create(MathAtomType.Relation, "\u2291") },
         { "sqsupseteq", Create(MathAtomType.Relation, "\u2292") },
         { "models", Create(MathAtomType.Relation, "\u22A7") },
         { "perp", Create(MathAtomType.Relation, "\u27C2") },
         
         // operators
         { "times", Times },
         { "div"  , Divide },
         { "pm"   , Create(MathAtomType.BinaryOperator, "\u00B1") },
         { "dagger", Create(MathAtomType.BinaryOperator, "\u2020") },
         { "ddagger", Create(MathAtomType.BinaryOperator, "\u2021") },
         { "mp"   , Create(MathAtomType.BinaryOperator, "\u2213") },
         { "setminus", Create(MathAtomType.BinaryOperator, "\u2216") },
         { "ast"  , Create(MathAtomType.BinaryOperator, "\u2217") },
         { "circ" , Create(MathAtomType.BinaryOperator, "\u2218") },
         { "bullet", Create(MathAtomType.BinaryOperator, "\u2219") },
         { "wedge", "land", Create(MathAtomType.BinaryOperator, "\u2227") },
         { "vee", "lor", Create(MathAtomType.BinaryOperator, "\u2228") },
         { "cap", Create(MathAtomType.BinaryOperator, "\u2229") },
         { "cup", Create(MathAtomType.BinaryOperator, "\u222A") },
         { "wr", Create(MathAtomType.BinaryOperator, "\u2240") },
         { "uplus", Create(MathAtomType.BinaryOperator, "\u228E") },
         { "sqcap", Create(MathAtomType.BinaryOperator, "\u2293") },
         { "sqcup", Create(MathAtomType.BinaryOperator, "\u2294") },
         { "oplus", Create(MathAtomType.BinaryOperator, "\u2295") },
         { "ominus", Create(MathAtomType.BinaryOperator, "\u2296") },
         { "otimes", Create(MathAtomType.BinaryOperator, "\u2297") },
         { "oslash", Create(MathAtomType.BinaryOperator, "\u2298") },
         { "odot", Create(MathAtomType.BinaryOperator, "\u2299") },
         { "star" , Create(MathAtomType.BinaryOperator, "\u22C6") },
         { "cdot" , Create(MathAtomType.BinaryOperator, "\u22C5") },
         { "amalg", Create(MathAtomType.BinaryOperator, "\u2A3F") },
         
         // No limit operators
         { "log", Create(MathAtomType.Operator, "log") },
         { "lg", Create(MathAtomType.Operator, "lg") },
         { "ln", Create(MathAtomType.Operator, "ln") },
         { "sin", Create(MathAtomType.Operator, "sin") },
         { "arcsin", Create(MathAtomType.Operator, "arcsin") },
         { "sinh", Create(MathAtomType.Operator, "sinh") },
         { "arsinh", Create(MathAtomType.Operator, "arsinh") }, //not in iosMath
         { "cos", Create(MathAtomType.Operator, "cos") },
         { "arccos", Create(MathAtomType.Operator, "arccos") },
         { "cosh", Create(MathAtomType.Operator, "cosh") },
         { "arcosh", Create(MathAtomType.Operator, "arcosh") }, //not in iosMath
         { "tan", Create(MathAtomType.Operator, "tan") },
         { "arctan", Create(MathAtomType.Operator, "arctan") },
         { "tanh", Create(MathAtomType.Operator, "tanh") },
         { "artanh", Create(MathAtomType.Operator, "artanh") },  //not in iosMath
         { "cot", Create(MathAtomType.Operator, "cot") },
         { "arccot", Create(MathAtomType.Operator, "arccot") },  //not in iosMath
         { "coth", Create(MathAtomType.Operator, "coth") },
         { "arcoth", Create(MathAtomType.Operator, "arcoth") },  //not in iosMath
         { "sec", Create(MathAtomType.Operator, "sec") },
         { "arcsec", Create(MathAtomType.Operator, "arcsec") },  //not in iosMath
         { "sech", Create(MathAtomType.Operator, "sech") },  //not in iosMath
         { "arsech", Create(MathAtomType.Operator, "arsech") },  //not in iosMath
         { "csc", Create(MathAtomType.Operator, "csc") },
         { "arccsc", Create(MathAtomType.Operator, "arccsc") },  //not in iosMath
         { "csch", Create(MathAtomType.Operator, "csch") },  //not in iosMath
         { "arcsch", Create(MathAtomType.Operator, "arcsch") },  //not in iosMath
         { "arg", Create(MathAtomType.Operator, "arg") },
         { "ker", Create(MathAtomType.Operator, "ker") },
         { "dim", Create(MathAtomType.Operator, "dim") },
         { "hom", Create(MathAtomType.Operator, "hom") },
         { "exp", Create(MathAtomType.Operator, "exp") },
         { "deg", Create(MathAtomType.Operator, "deg") },
         
         // Limit operators
         { "lim", Create(MathAtomType.Operator, "lim") },
         { "limsup", Create(MathAtomType.Operator, "lim sup") },
         { "liminf", Create(MathAtomType.Operator, "lim inf") },
         { "max", Create(MathAtomType.Operator, "max") },
         { "min", Create(MathAtomType.Operator, "min") },
         { "sup", Create(MathAtomType.Operator, "sup") },
         { "inf", Create(MathAtomType.Operator, "inf") },
         { "det", Create(MathAtomType.Operator, "det") },
         { "Pr", Create(MathAtomType.Operator, "Pr") },
         { "gcd", Create(MathAtomType.Operator, "gcd") },
         
         // Large operators
         { "prod", new LargeOperator("\u220F" , null) },
         { "coprod", new LargeOperator("\u2210" , null) },
         { "sum", new LargeOperator("\u2211" , null) },
         { "int", new LargeOperator("\u222B" , false) },
         { "iint", new LargeOperator("\u222C", false) }, //not in iosMath
         { "iiint", new LargeOperator("\u222D", false) }, //not in iosMath
         { "iiiint", new LargeOperator("\u2A0C", false) }, //not in iosMath
         { "oint", new LargeOperator("\u222E" , false) },
         { "oiint", new LargeOperator("\u222F" , false) }, //not in iosMath
         { "oiiint", new LargeOperator("\u2230" , false) }, //not in iosMath
         { "intclockwise", new LargeOperator("\u2231" , false) }, //not in iosMath
         { "awint", new LargeOperator("\u2A11" , false) }, //not in iosMath
         { "varointclockwise", new LargeOperator("\u2232" , false) }, //not in iosMath
         { "ointctrclockwise", new LargeOperator("\u2233" , false) }, //not in iosMath
         { "bigwedge", new LargeOperator("\u22C0" , null) },
         { "bigvee", new LargeOperator("\u22C1" , null) },
         { "bigcap", new LargeOperator("\u22C2" , null) },
         { "bigcup", new LargeOperator("\u22C3" , null) },
         { "bigbot", new LargeOperator("\u27D8" , null) }, //not in iosMath
         { "bigtop", new LargeOperator("\u27D9" , null) }, //not in iosMath
         { "bigodot", new LargeOperator("\u2A00" , null) },
         { "bigoplus", new LargeOperator("\u2A01" , null) },
         { "bigotimes", new LargeOperator("\u2A02" , null) },
         { "bigcupdot", new LargeOperator("\u2A03" , null) }, //not in iosMath
         { "biguplus", new LargeOperator("\u2A04" , null) },
         { "bigsqcap", new LargeOperator("\u2A05" , null) }, //not in iosMath
         { "bigsqcup", new LargeOperator("\u2A06" , null) },
         { "bigtimes", new LargeOperator("\u2A09" , null) }, //not in iosMath
         
         // Latex command characters
         { "{", "lbrace", Create(MathAtomType.Open, "{") },
         { "}", "rbrace", Create(MathAtomType.Close, "}") },
         { "$", Create(MathAtomType.Ordinary, "$") },
         { "&", Create(MathAtomType.Ordinary, "&") },
         { "#", Create(MathAtomType.Ordinary, "#") },
         { "%", Create(MathAtomType.Ordinary, "%") },
         { "_", Create(MathAtomType.Ordinary, "_") },
         { " ", Create(MathAtomType.Ordinary, " ") },
         { "backslash", Create(MathAtomType.Ordinary, "\\") },
         
         // Punctuation
         // Note: \colon is different from, which is a relation
         { "colon", Create(MathAtomType.Punctuation, ":") },
         { "cdotp", Create(MathAtomType.Punctuation, "\u00B7") },
         
         // Other symbols
         { "degree", Create(MathAtomType.Ordinary, "\u00B0") },
         { "neg", "lnot", Create(MathAtomType.Ordinary, "\u00AC") },
         { "angstrom", "AA", Create(MathAtomType.Ordinary, "\u00C5") },
         { "|", "Vert", Create(MathAtomType.Ordinary, "\u2016") },
         { "vert", Create(MathAtomType.Ordinary, "|") },
         { "ldots", Create(MathAtomType.Ordinary, "\u2026") },
         // \prime is removed
         { "hbar", Create(MathAtomType.Ordinary, "\u210F") },
         { "Im", Create(MathAtomType.Ordinary, "\u2111") },
         { "ell", Create(MathAtomType.Ordinary, "\u2113") },
         { "wp", Create(MathAtomType.Ordinary, "\u2118") },
         { "Re", Create(MathAtomType.Ordinary, "\u211C") },
         { "mho", Create(MathAtomType.Ordinary, "\u2127") },
         { "aleph", Create(MathAtomType.Ordinary, "\u2135") },
         { "beth", Create(MathAtomType.Ordinary, "\u2136") }, //not in iosMath
         { "gimel", Create(MathAtomType.Ordinary, "\u2137") }, //not in iosMath
         { "daleth", Create(MathAtomType.Ordinary, "\u2138") }, //not in iosMath
         { "forall", Create(MathAtomType.Ordinary, "\u2200") },
         { "exists", Create(MathAtomType.Ordinary, "\u2203") },
         { "because", Create(MathAtomType.Ordinary, "\u2235") }, //not in iosMath
         { "therefore", Create(MathAtomType.Ordinary, "\u2234") }, //not in iosMath
         { "emptyset", Create(MathAtomType.Ordinary, "\u2205") },
         { "nabla", Create(MathAtomType.Ordinary, "\u2207") },
         { "infty", Create(MathAtomType.Ordinary, "\u221E") },
         { "angle", Create(MathAtomType.Ordinary, "\u2220") },
         { "top", Create(MathAtomType.Ordinary, "\u22A4") },
         { "bot", Create(MathAtomType.Ordinary, "\u22A5") },
         { "vdots", Create(MathAtomType.Ordinary, "\u22EE") },
         { "cdots", Create(MathAtomType.Ordinary, "\u22EF") },
         { "ddots", Create(MathAtomType.Ordinary, "\u22F1") },
         { "triangle", Create(MathAtomType.Ordinary, "\u25B3") },
         { "imath", Create(MathAtomType.Ordinary, "\U0001D6A4") },
         { "jmath", Create(MathAtomType.Ordinary, "\U0001D6A5") },
         { "partial", Create(MathAtomType.Ordinary, "\U0001D715") },
         
         // Spacing
         { ",", Space(Structures.Space.ShortSpace) },
         { ":", ">", Space(Structures.Space.MediumSpace) },
         { ";", Space(Structures.Space.LongSpace) },
         { "!", Space(-Structures.Space.ShortSpace) },
         { "quad", Space(Structures.Space.EmWidth) },
         { "qquad", Space(Structures.Space.EmWidth * 2) },
         
         // Style
         { "displaystyle", new Style(LineStyle.Display) },
         { "textstyle", new Style(LineStyle.Text) },
         { "scriptstyle", new Style(LineStyle.Script) },
         { "scriptscriptstyle",  new Style(LineStyle.ScriptScript) },

         // Accents
         { "grave", new Accent("\u0300") },
         { "acute", new Accent("\u0301") },
         { "hat", "widehat", new Accent("\u0302") },  // In our implementation hat and widehat behave the same.
         { "tilde", "widetilde", new Accent("\u0303") }, // In our implementation tilde and widetilde behave the same.
         { "bar", new Accent("\u0304") },
         { "overbar", new Accent("\u0305") }, //not in iosMath
         { "breve", new Accent("\u0306") },
         { "dot", new Accent("\u0307") },
         { "ddot", new Accent("\u0308") },
         { "ovhook", new Accent("\u0309") }, //not in iosMath
         { "ocirc", new Accent("\u030A") }, //not in iosMath
         { "check", new Accent("\u030C") },
         { "leftharpoonaccent", new Accent("\u20D0") }, //not in iosMath
         { "rightharpoonaccent", new Accent("\u20D1") }, //not in iosMath
         { "vertoverlay", new Accent("\u20D2") }, //not in iosMath
         { "vec", new Accent("\u20D7") },
         { "dddot", new Accent("\u20DB") }, //not in iosMath
         { "ddddot", new Accent("\u20DC") }, //not in iosMath
         { "widebridgeabove", new Accent("\u20E9") }, //not in iosMath
         { "asteraccent", new Accent("\u20F0") }, //not in iosMath
         { "threeunderdot", new Accent("\u20E8") } //not in iosMath
      });
    
    //public static LargeOperator LargeOperator(string name, bool? limits, bool noLimits = false) => new LargeOperator(name, limits, noLimits);

    //public static Operator Operator(string name) => new Operator(name);
    public static Space Space(Structures.Space sp) => new Space(sp);
    public static MathAtom Create(MathAtomType type, char value) => Create(type, value.ToString());
    public static MathAtom Create(MathAtomType type, string value) {
      switch (type) {
        case MathAtomType.Accent:
          return new Accent(value);
        case MathAtomType.Color:
          return new Color();
        case MathAtomType.Fraction:
          return new Fraction();
        case MathAtomType.Inner:
          return new Inner();
        case MathAtomType.LargeOperator:
          throw new ArgumentException(
            "Do not use Create(MathAtomType.LargeOperator, string)." +
            "Use LargeOperator(string, bool?, bool) instead.", nameof(type));
        case MathAtomType.Overline:
          return new Overline();
        case MathAtomType.Underline:
          return new Underline();
        case MathAtomType.Space:
          throw new ArgumentException(
            "Do not use Create(MathAtomType.Space, string)." +
            "Use Space(int, bool) instead.", nameof(type));
        default:
          return new MathAtom(type, value);
      }
    }

    public static MathAtom Times => Create(MathAtomType.BinaryOperator, Symbols.Multiplication);
    public static MathAtom Divide => Create(MathAtomType.BinaryOperator, Symbols.Division);

    public static MathAtom Placeholder => Create(MathAtomType.Placeholder, Symbols.WhiteSquare);
    public static MathList PlaceholderList => new MathList { Placeholder };
    public static Fraction PlaceholderFraction => new Fraction { Numerator = PlaceholderList, Denominator = PlaceholderList };
    public static Radical PlaceholderRadical => new Radical { Degree = PlaceholderList,  Radicand = PlaceholderList };
    public static Radical PlaceholderSquareRoot => new Radical { Radicand = PlaceholderList };

    public static MathAtom ForCharacter(char c) {
      if (char.IsControl(c) || char.IsWhiteSpace(c)) {
        return null; // skip spaces
      }
      if (c >= '0' && c <= '9') {
        return Create(MathAtomType.Number, c);
      }
      if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
        return Create(MathAtomType.Variable, c);
      }
      switch (c) {
        case '$':
        case '%':
        case '#':
        case '&':
        case '~':
        case '\'':
        case '^':
        case '_':
        case '{':
        case '}':
        case '\\': // All these are special characters we don't support.
          return null;
        case '(':
        case '[':
          return Create(MathAtomType.Open, c);
        case ')':
        case ']':
        case '!':
        case '?':
          return Create(MathAtomType.Close, c);
        case ',':
        case ';':
          return Create(MathAtomType.Punctuation, c);
        case '=':
        case '<':
        case '>':
          return Create(MathAtomType.Relation, c);
        case ':': // Colon is a ratio. Regular colon is \colon
        case '\u2236':
          return Create(MathAtomType.Relation, "\u2236");
        case '-': // use the math minus sign
        case '\u2212':
          return Create(MathAtomType.BinaryOperator, "\u2212");
        case '+':
        case '*': // Star operator, not times symbol
          return Create(MathAtomType.BinaryOperator, c);
        case '×':
          return Times;
        case '÷':
          return Divide;
        case '.':
          return Create(MathAtomType.Number, c);
        case '"':
        case '/':
        case '@':
        case '`':
        case '|':
          return Create(MathAtomType.Ordinary, c);
        default: //also support non-ascii characters
          return Create(MathAtomType.Ordinary, c);
          //throw new NotImplementedException($"Ascii character {c} should have been accounted for.");
      }
    }

    internal static FontStyle? FontStyle(string command) => FontStyleExtensions.FontStyles.TryGetValue(command, out var fontStyle) ? fontStyle : default(FontStyle?);

    public static MathList MathListForCharacters(string chars) {
      var r = new MathList();
      foreach (char c in chars)
        r.Add(ForCharacter(c));
      return r;
    }

    public static IMathAtom ForLatexSymbolName(string symbolName) =>
      Commands.TryGetValue(
        symbolName ?? throw new ArgumentNullException(nameof(symbolName), "LaTeX Symbol name must not be null."
      ), out var symbol) ? AtomCloner.Clone(symbol, false) : null;

    public static string LatexSymbolNameForAtom(MathAtom atom) =>  Commands.TryGetKey(atom, out var name) ? name : null;

    public static void AddLatexSymbol(string name, MathAtom atom) => Commands.Add(name, atom);

    public static IEnumerable<string> SupportedLatexSymbolNames => Commands.Keys;
    
    public static AliasDictionary<string, string> BoundaryDelimiters { get; } = new AliasDictionary<string, string> {
      { ".", string.Empty }, // . means no delimiter
      { "(", "(" },
      { ")", ")" },
      { "[", "[" },
      { "]", "]" },
      { "{", "lbrace", "{" },
      { "}", "rbrace", "}" },
      { "<", "langle", "\u2329" },
      { ">", "rangle", "\u232A" },
      { "/", "/" },
      { "\\", "backslash", "\\" },
      { "|", "vert", "|" },
      { "||", "Vert","\u2016" },
      { "uparrow", "\u2191" },
      { "downarrow", "\u2193" },
      { "updownarrow", "\u2195" },
      { "Uparrow", "\u21D1" },
      { "Downarrow", "\u21D3" },
      { "Updownarrow", "\u21D5" },
      { "lgroup", "\u27EE" },
      { "rgroup", "\u27EF" },
      { "lceil", "\u2308" },
      { "rceil", "\u2309" },
      { "lfloor", "\u230A" },
      { "rfloor", "\u230B" }
    };

    public static IMathAtom BoundaryAtom(string delimiterName) =>
      BoundaryDelimiters.TryGetValue(delimiterName, out var value) ? Create(MathAtomType.Boundary, value) : null;

    public static string DelimiterName(IMathAtom boundaryAtom) =>
      boundaryAtom.AtomType is MathAtomType.Boundary && BoundaryDelimiters.TryGetKey(boundaryAtom.Nucleus, out var name) ? name : null;

    public static IFraction Fraction(IMathList numerator, IMathList denominator) =>
      new Fraction { Numerator = numerator, Denominator = denominator };

    public static IFraction Fraction(string numerator, string denominator)
      => Fraction(MathListForCharacters(numerator), MathListForCharacters(denominator));

    private static Dictionary<string, Pair<string, string>?> _matrixEnvironments { get; } =
      new Dictionary<string, Pair<string, string>?> {
        { "matrix",  null } ,
        { "pmatrix", Pair.Create("(", ")") } ,
        { "bmatrix", Pair.Create("[", "]") },
        { "Bmatrix", Pair.Create("{", "}") },
        { "vmatrix", Pair.Create("vert", "vert") },
        { "Vmatrix", Pair.Create("Vert", "Vert") }
      };
      

    public static Structures.Result<IMathAtom> Table( string environment, List<List<IMathList>> rows) {
      Style style;
      var table = new Table(environment) { Cells = rows };
      switch (environment) {
        case null:
          table.InterRowAdditionalSpacing = 1;
          for (int i = 0; i < table.NColumns; i++) {
            table.SetAlignment(ColumnAlignment.Left, i);
          }
          return table;
        case var _ when _matrixEnvironments.TryGetValue(environment, out var delimiters):
          table.Environment = "matrix"; // TableEnvironment is set to matrix as delimiters are converted to latex outside the table.
          table.InterColumnSpacing = 18;

          style = new Style(LineStyle.Text);
          foreach (var row in table.Cells) {
            foreach (var cell in row) {
              cell.Insert(0, style);
            }
          }

          if (delimiters != null) {
            var inner = new Inner {
              LeftBoundary = BoundaryAtom(delimiters.Value.First),
              RightBoundary = BoundaryAtom(delimiters.Value.Second),
              InnerList = MathLists.WithAtoms(table)
            };
            return inner;
          } else {
            return table;
          }
        case "array":
          table.InterRowAdditionalSpacing = 1;
          for (int i = 0; i < table.NColumns; i++) {
            table.SetAlignment(ColumnAlignment.Left, i);
          }
          return table;
        case "eqalign":
        case "split":
        case "aligned":
          if (table.NColumns != 2) {
            return environment + " environment can have only 2 columns";
          } else {
            // add a spacer before each of the second column elements, in order to create the correct spacing for "=" and other relations.
            var spacer = Create(MathAtomType.Ordinary, string.Empty);
            foreach (var row in table.Cells) {
              if (row.Count > 1) {
                row[1].Insert(0, spacer);
              }
            }
            table.InterRowAdditionalSpacing = 1;
            table.SetAlignment(ColumnAlignment.Right, 0);
            table.SetAlignment(ColumnAlignment.Left, 1);
            return table;
          }
        case "displaylines":
        case "gather":
          if (table.NColumns != 1) {
            return environment + " environment can only have 1 column.";
          }
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 0;
          table.SetAlignment(ColumnAlignment.Center, 0);
          return table;
        case "eqnarray":
          if (table.NColumns != 3) {
            return environment + " must have exactly 3 columns.";
          } else {
            table.InterRowAdditionalSpacing = 1;
            table.InterColumnSpacing = 18;
            table.SetAlignment(ColumnAlignment.Right, 0);
            table.SetAlignment(ColumnAlignment.Center, 1);
            table.SetAlignment(ColumnAlignment.Left, 2);
            return table;
          }
        case "cases":
          if (table.NColumns < 1 || table.NColumns > 2) {
            return "cases environment must have 1 to 2 columns";
          } else {
            table.InterColumnSpacing = 18;
            table.SetAlignment(ColumnAlignment.Left, 0);
            if (table.NColumns == 2) table.SetAlignment(ColumnAlignment.Left, 1);
            style = new Style(LineStyle.Text);
            foreach (var row in table.Cells) {
              foreach (var cell in row) {
                cell.Insert(0, style);
              }
            }
            // add delimiters
            var inner = new Inner {
              LeftBoundary = BoundaryAtom("{"),
              RightBoundary = BoundaryAtom(".")
            };
            var space = ForLatexSymbolName(",");
            inner.InnerList = MathLists.WithAtoms(space, table);
            return inner;
          }
        default:
          return "Unknown environment " + environment;
      }
    }
  }
}
