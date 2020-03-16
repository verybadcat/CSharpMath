using System;
using System.Collections.Generic;

namespace CSharpMath.Atom
 {
  using Atoms;
  //https://mirror.hmc.edu/ctan/macros/latex/contrib/unicode-math/unimath-symbols.pdf
  public static class MathAtoms {
    public static Structures.AliasDictionary<string, MathAtom> Commands { get; } =
      new Structures.AliasDictionary<string, MathAtom> {
         { "square", Placeholder },
         
         // Greek characters
         { "alpha", new Variable("\u03B1") },
         { "beta", new Variable("\u03B2") },
         { "gamma", new Variable("\u03B3") },
         { "delta", new Variable("\u03B4") },
         { "varepsilon", new Variable("\u03B5") },
         { "zeta", new Variable("\u03B6") },
         { "eta", new Variable("\u03B7") },
         { "theta", new Variable("\u03B8") },
         { "iota", new Variable("\u03B9") },
         { "kappa", new Variable("\u03BA") },
         { "lambda", new Variable("\u03BB") },
         { "mu", new Variable("\u03BC") },
         { "nu", new Variable("\u03BD") },
         { "xi", new Variable("\u03BE") },
         { "omicron", new Variable("\u03BF") },
         { "pi", new Variable("\u03C0") },
         { "rho", new Variable("\u03C1") },
         { "varsigma", new Variable("\u03C2") },
         { "sigma", new Variable("\u03C3") },
         { "tau", new Variable("\u03C4") },
         { "upsilon", new Variable("\u03C5") },
         { "varphi", new Variable("\u03C6") },
         { "chi", new Variable("\u03C7") },
         { "psi", new Variable("\u03C8") },
         { "omega", new Variable("\u03C9") },
         { "vartheta", new Variable("\u03D1") },
         { "phi", new Variable("\u03D5") },
         { "varpi", new Variable("\u03D6") },
         { "varkappa", new Variable("\u03F0") },
         { "varrho", new Variable("\u03F1") },
         { "epsilon", new Variable("\u03F5") },
         // Capital greek characters
         { "Gamma", new Variable("\u0393") },
         { "Delta", new Variable("\u0394") },
         { "Theta", new Variable("\u0398") },
         { "Lambda", new Variable("\u039B") },
         { "Xi", new Variable("\u039E") },
         { "Pi", new Variable("\u03A0") },
         { "Sigma", new Variable("\u03A3") },
         { "Upsilon", new Variable("\u03A5") },
         { "Phi", new Variable("\u03A6") },
         { "Psi", new Variable("\u03A8") },
         { "Omega", new Variable("\u03A9") },
         
         // Open
         { "lceil", new Open("\u2308") },
         { "lfloor", new Open("\u230A") },
         { "langle", new Open("\u27E8") },
         { "lgroup", new Open("\u27EE") },
         
         // Close
         { "rceil", new Close("\u2309") },
         { "rfloor", new Close("\u230B") },
         { "rangle", new Close("\u27E9") },
         { "rgroup", new Close("\u27EF") },
         
         // Arrows
         { "leftarrow", "gets", new Relation("\u2190") },
         { "uparrow", new Relation("\u2191") },
         { "rightarrow", "to", new Relation("\u2192") },
         { "downarrow", new Relation("\u2193") },
         { "leftrightarrow", new Relation("\u2194") },
         { "updownarrow", new Relation("\u2195") },
         { "nwarrow", new Relation("\u2196") },
         { "nearrow", new Relation("\u2197") },
         { "searrow", new Relation("\u2198") },
         { "swarrow", new Relation("\u2199") },
         { "mapsto", new Relation("\u21A6") },
         { "Leftarrow", new Relation("\u21D0") },
         { "Uparrow", new Relation("\u21D1") },
         { "Rightarrow", new Relation("\u21D2") },
         { "Downarrow", new Relation("\u21D3") },
         { "Leftrightarrow", new Relation("\u21D4") },
         { "Updownarrow", new Relation("\u21D5") },
         { "longleftarrow", new Relation("\u27F5") },
         { "longrightarrow", new Relation("\u27F6") },
         { "longleftrightarrow", new Relation("\u27F7") },
         { "Longleftarrow", new Relation("\u27F8") },
         { "Longrightarrow", new Relation("\u27F9") },
         { "Longleftrightarrow", "iff", new Relation("\u27FA") },
         
         // Relations
         { "leq", "le", new Relation("\u2264") },
         { "geq", "ge", new Relation("\u2265") },
         { "neq", "ne", new Relation("\u2260") },
         { "in", new Relation("\u2208") },
         { "notin", new Relation("\u2209") },
         { "ni", new Relation("\u220B") },
         { "propto", new Relation("\u221D") },
         { "mid", new Relation("\u2223") },
         { "parallel", new Relation("\u2225") },
         { "sim", new Relation("\u223C") },
         { "simeq", new Relation("\u2243") },
         { "cong", new Relation("\u2245") },
         { "approx", new Relation("\u2248") },
         { "asymp", new Relation("\u224D") },
         { "doteq", new Relation("\u2250") },
         { "equiv", new Relation("\u2261") },
         { "gg", new Relation("\u226A") },
         { "ll", new Relation("\u226B") },
         { "prec", new Relation("\u227A") },
         { "succ", new Relation("\u227B") },
         { "subset", new Relation("\u2282") },
         { "supset", new Relation("\u2283") },
         { "subseteq", new Relation("\u2286") },
         { "supseteq", new Relation("\u2287") },
         { "sqsubset", new Relation("\u228F") },
         { "sqsupset", new Relation("\u2290") },
         { "sqsubseteq", new Relation("\u2291") },
         { "sqsupseteq", new Relation("\u2292") },
         { "models", new Relation("\u22A7") },
         { "perp", new Relation("\u27C2") },
         
         // operators
         { "times", Times },
         { "div"  , Divide },
         { "pm"   , new BinaryOperator("\u00B1") },
         { "dagger", new BinaryOperator("\u2020") },
         { "ddagger", new BinaryOperator("\u2021") },
         { "mp"   , new BinaryOperator("\u2213") },
         { "setminus", new BinaryOperator("\u2216") },
         { "ast"  , new BinaryOperator("\u2217") },
         { "circ" , new BinaryOperator("\u2218") },
         { "bullet", new BinaryOperator("\u2219") },
         { "wedge", "land", new BinaryOperator("\u2227") },
         { "vee", "lor", new BinaryOperator("\u2228") },
         { "cap", new BinaryOperator("\u2229") },
         { "cup", new BinaryOperator("\u222A") },
         { "wr", new BinaryOperator("\u2240") },
         { "uplus", new BinaryOperator("\u228E") },
         { "sqcap", new BinaryOperator("\u2293") },
         { "sqcup", new BinaryOperator("\u2294") },
         { "oplus", new BinaryOperator("\u2295") },
         { "ominus", new BinaryOperator("\u2296") },
         { "otimes", new BinaryOperator("\u2297") },
         { "oslash", new BinaryOperator("\u2298") },
         { "odot", new BinaryOperator("\u2299") },
         { "star" , new BinaryOperator("\u22C6") },
         { "cdot" , new BinaryOperator("\u22C5") },
         { "amalg", new BinaryOperator("\u2A3F") },
         
         // No limit operators
         { "log", new LargeOperator("log", false, true) },
         { "lg", new LargeOperator("lg", false, true) },
         { "ln", new LargeOperator("ln", false, true) },
         { "sin", new LargeOperator("sin", false, true) },
         { "arcsin", new LargeOperator("arcsin", false, true) },
         { "sinh", new LargeOperator("sinh", false, true) },
         { "arsinh", new LargeOperator("arsinh", false, true) }, //not in iosMath
         { "cos", new LargeOperator("cos", false, true) },
         { "arccos", new LargeOperator("arccos", false, true) },
         { "cosh", new LargeOperator("cosh", false, true) },
         { "arcosh", new LargeOperator("arcosh", false, true) }, //not in iosMath
         { "tan", new LargeOperator("tan", false, true) },
         { "arctan", new LargeOperator("arctan", false, true) },
         { "tanh", new LargeOperator("tanh", false, true) },
         { "artanh", new LargeOperator("artanh", false, true) },  //not in iosMath
         { "cot", new LargeOperator("cot", false, true) },
         { "arccot", new LargeOperator("arccot", false, true) },  //not in iosMath
         { "coth", new LargeOperator("coth", false, true) },
         { "arcoth", new LargeOperator("arcoth", false, true) },  //not in iosMath
         { "sec", new LargeOperator("sec", false, true) },
         { "arcsec", new LargeOperator("arcsec", false, true) },  //not in iosMath
         { "sech", new LargeOperator("sech", false, true) },  //not in iosMath
         { "arsech", new LargeOperator("arsech", false, true) },  //not in iosMath
         { "csc", new LargeOperator("csc" , false, true) },
         { "arccsc", new LargeOperator("arccsc", false, true) },  //not in iosMath
         { "csch", new LargeOperator("csch", false, true) },  //not in iosMath
         { "arcsch", new LargeOperator("arcsch", false, true) },  //not in iosMath
         { "arg", new LargeOperator("arg", false, true) },
         { "ker", new LargeOperator("ker", false, true) },
         { "dim", new LargeOperator("dim", false, true) },
         { "hom", new LargeOperator("hom", false, true) },
         { "exp", new LargeOperator("exp", false, true) },
         { "deg", new LargeOperator("deg", false, true) },
         
         // Limit operators
         { "lim", new LargeOperator("lim" , null) },
         { "limsup", new LargeOperator("lim sup" , null) },
         { "liminf", new LargeOperator("lim inf" , null) },
         { "max", new LargeOperator("max" , null) },
         { "min", new LargeOperator("min" , null) },
         { "sup", new LargeOperator("sup" , null) },
         { "inf", new LargeOperator("inf" , null) },
         { "det", new LargeOperator("det" , null) },
         { "Pr", new LargeOperator("Pr" , null) },
         { "gcd", new LargeOperator("gcd" , null) },
         
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
         { "{", "lbrace", new Open("{") },
         { "}", "rbrace", new Close("}") },
         { "$", new Ordinary("$") },
         { "&", new Ordinary("&") },
         { "#", new Ordinary("#") },
         { "%", new Ordinary("%") },
         { "_", new Ordinary("_") },
         { " ", new Ordinary(" ") },
         { "backslash", new Ordinary("\\") },
         
         // Punctuation
         // Note: \colon is different from, which is a relation
         { "colon", new Punctuation(":") },
         { "cdotp", new Punctuation("\u00B7") },
         
         // Other symbols
         { "degree", new Ordinary("\u00B0") },
         { "neg", "lnot", new Ordinary("\u00AC") },
         { "angstrom", "AA", new Ordinary("\u00C5") },
         { "|", "Vert", new Ordinary("\u2016") },
         { "vert", new Ordinary("|") },
         { "ldots", new Ordinary("\u2026") },
         // \prime is removed
         { "hbar", new Ordinary("\u210F") },
         { "Im", new Ordinary("\u2111") },
         { "ell", new Ordinary("\u2113") },
         { "wp", new Ordinary("\u2118") },
         { "Re", new Ordinary("\u211C") },
         { "mho", new Ordinary("\u2127") },
         { "aleph", new Ordinary("\u2135") },
         { "beth", new Ordinary("\u2136") }, //not in iosMath
         { "gimel", new Ordinary("\u2137") }, //not in iosMath
         { "daleth", new Ordinary("\u2138") }, //not in iosMath
         { "forall", new Ordinary("\u2200") },
         { "exists", new Ordinary("\u2203") },
         { "because", new Ordinary("\u2235") }, //not in iosMath
         { "therefore", new Ordinary("\u2234") }, //not in iosMath
         { "emptyset", new Ordinary("\u2205") },
         { "nabla", new Ordinary("\u2207") },
         { "infty", new Ordinary("\u221E") },
         { "angle", new Ordinary("\u2220") },
         { "top", new Ordinary("\u22A4") },
         { "bot", new Ordinary("\u22A5") },
         { "vdots", new Ordinary("\u22EE") },
         { "cdots", new Ordinary("\u22EF") },
         { "ddots", new Ordinary("\u22F1") },
         { "diameter", new Ordinary("\u2300") }, // not in iosMath
         { "triangle", new Ordinary("\u25B3") },
         { "imath", new Ordinary("\U0001D6A4") },
         { "jmath", new Ordinary("\U0001D6A5") },
         { "partial", new Ordinary("\U0001D715") },
         
         // Spacing
         { ",", new Space(Structures.Space.ShortSpace) },
         { ":", ">", new Space(Structures.Space.MediumSpace) },
         { ";", new Space(Structures.Space.LongSpace) },
         { "!", new Space(-Structures.Space.ShortSpace) },
         { "quad", new Space(Structures.Space.EmWidth) },
         { "qquad", new Space(Structures.Space.EmWidth * 2) },
         
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
      };
    public static MathAtom Times => new BinaryOperator("\u00D7");
    public static MathAtom Divide => new BinaryOperator("\u00F7");

    public static MathAtom Placeholder => new Placeholder("\u25A1");
    public static MathList PlaceholderList => new MathList { Placeholder };
    public static Fraction PlaceholderFraction => new Fraction { Numerator = PlaceholderList, Denominator = PlaceholderList };
    public static Radical PlaceholderRadical => new Radical(PlaceholderList, PlaceholderList);
    public static Radical PlaceholderSquareRoot => new Radical(null, PlaceholderList);
    public static Radical PlaceholderCubeRoot => new Radical(new MathList(new Number("3")), PlaceholderList);

    public static MathAtom? ForCharacter(char c) {
      var s = c.ToStringInvariant();
      if (char.IsControl(c) || char.IsWhiteSpace(c)) {
        return null; // skip spaces
      }
      if (c >= '0' && c <= '9') {
        return new Number(s);
      }
      if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
        return new Variable(s);
      }
      switch (c) {
#warning Todo check if all these 9 characters are already accounted for in MathListBuilder, then remove
        case '$':
        case '%':
        case '#':
        case '&':
        case '~':
        case '\'':
        case '^':
        case '_':
        case '\\':
          return null;
        case '(':
        case '[':
        case '{':
          return new Open(s);
        case ')':
        case ']':
        case '}':
          return new Close(s);
        case '!':
        case '?':
          return new Close(s, false);
        case ',':
        case ';':
          return new Punctuation(s);
        case '=':
        case '<':
        case '>':
        case '≠':
        case '≥':
        case '≤':
          return new Relation(s);
        case ':': // Colon is a ratio. Regular colon is \colon
        case '\u2236':
          return new Relation("\u2236");
        case '-': // use the math minus sign
        case '\u2212':
          return new BinaryOperator("\u2212");
        case '+':
        case '*': // Star operator, not times symbol
          return new BinaryOperator(s);
        case '×':
          return Times;
        case '÷':
          return Divide;
        case '.':
          return new Number(s);
        case var _ when Display.UnicodeFontChanger.IsLowerGreek(c)
                     || Display.UnicodeFontChanger.IsUpperGreek(c):
          // All greek letters are rendered as variables.
          return new Variable(s);
        default:
          return new Ordinary(s);
      }
    }

    internal static FontStyle? FontStyle(string command) => FontStyleExtensions.FontStyles.TryGetValue(command, out var fontStyle) ? fontStyle : default(FontStyle?);

    public static MathAtom? ForLaTeXSymbolName(string symbolName) =>
      Commands.TryGetValue(
        symbolName ?? throw new ArgumentNullException(nameof(symbolName)
      ), out var symbol) ? symbol.Clone(false) : null;

    public static string? LaTeXSymbolNameForAtom(MathAtom atom) {
      var atomWithoutScripts = atom.Clone(true);
      atomWithoutScripts.Subscript = null;
      atomWithoutScripts.Superscript = null;
      return Commands.TryGetKey(atomWithoutScripts, out var name) ? name : null;
    }

    public static void AddLatexSymbol(string name, MathAtom atom) => Commands.Add(name, atom);

    public static IEnumerable<string> SupportedLatexSymbolNames => Commands.Keys;
    
    public static Structures.AliasDictionary<string, string> BoundaryDelimiters { get; } =
      new Structures.AliasDictionary<string, string> {
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

    public static Boundary? BoundaryAtom(string delimiterName) =>
      BoundaryDelimiters.TryGetValue(delimiterName, out var value) ? new Boundary(value) : null;

    public static string? DelimiterName(Boundary boundaryAtom) =>
      BoundaryDelimiters.TryGetKey(boundaryAtom.Nucleus, out var name) ? name : null;
  }
}
