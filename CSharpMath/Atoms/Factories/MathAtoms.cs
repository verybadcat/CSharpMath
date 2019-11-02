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
         { "log", Operator("log", false, true) },
         { "lg", Operator("lg", false, true) },
         { "ln", Operator("ln", false, true) },
         { "sin", Operator("sin", false, true) },
         { "arcsin", Operator("arcsin", false, true) },
         { "sinh", Operator("sinh", false, true) },
         { "arsinh", Operator("arsinh", false, true) }, //not in iosMath
         { "cos", Operator("cos", false, true) },
         { "arccos", Operator("arccos", false, true) },
         { "cosh", Operator("cosh", false, true) },
         { "arcosh", Operator("arcosh", false, true) }, //not in iosMath
         { "tan", Operator("tan", false, true) },
         { "arctan", Operator("arctan", false, true) },
         { "tanh", Operator("tanh", false, true) },
         { "artanh", Operator("artanh", false, true) },  //not in iosMath
         { "cot", Operator("cot", false, true) },
         { "arccot", Operator("arccot", false, true) },  //not in iosMath
         { "coth", Operator("coth", false, true) },
         { "arcoth", Operator("arcoth", false, true) },  //not in iosMath
         { "sec", Operator("sec", false, true) },
         { "arcsec", Operator("arcsec", false, true) },  //not in iosMath
         { "sech", Operator("sech", false, true) },  //not in iosMath
         { "arsech", Operator("arsech", false, true) },  //not in iosMath
         { "csc", Operator("csc", false, true) },
         { "arccsc", Operator("arccsc", false, true) },  //not in iosMath
         { "csch", Operator("csch", false, true) },  //not in iosMath
         { "arcsch", Operator("arcsch", false, true) },  //not in iosMath
         { "arg", Operator("arg", false, true) },
         { "ker", Operator("ker", false, true) },
         { "dim", Operator("dim", false, true) },
         { "hom", Operator("hom", false, true) },
         { "exp", Operator("exp", false, true) },
         { "deg", Operator("deg", false, true) },
         
         // Limit operators
         { "lim", Operator("lim", null) },
         { "limsup", Operator("limsup", null) },
         { "liminf", Operator("liminf" , null) },
         { "max", Operator("max", null) },
         { "min", Operator("min", null) },
         { "sup", Operator("sup", null) },
         { "inf", Operator("inf", null) },
         { "det", Operator("det", null) },
         { "Pr", Operator("Pr", null) },
         { "gcd", Operator("gcd", null) },
         
         // Large operators
         { "prod", Operator("\u220F", null, false, "prod") },
         { "coprod", Operator("\u2210", null, false, "coprod") },
         { "sum", Operator("\u2211", null, false, "sum") },
         { "int", Operator("\u222B", false, false, "int") },
         { "iint", Operator("\u222C", false, false, "iint") }, //not in iosMath
         { "iiint", Operator("\u222D", false, false, "iiint") }, //not in iosMath
         { "iiiint", Operator("\u2A0C", false, false, "iiiint") }, //not in iosMath
         { "oint", Operator("\u222E", false, false, "oint") },
         { "oiint", Operator("\u222F", false, false, "oiint") }, //not in iosMath
         { "oiiint", Operator("\u2230", false, false, "oiiint") }, //not in iosMath
         { "intclockwise", Operator("\u2231", false, false, "intclockwise") }, //not in iosMath
         { "awint", Operator("\u2A11", false, false, "awint") }, //not in iosMath
         { "varointclockwise", Operator("\u2232", false, false, "varointclockwise") }, //not in iosMath
         { "ointctrclockwise", Operator("\u2233", false, false, "ointctrclockwise") }, //not in iosMath
         { "bigwedge", Operator("\u22C0", null, false, "bigwedge") },
         { "bigvee", Operator("\u22C1", null, false, "bigvee") },
         { "bigcap", Operator("\u22C2", null, false, "bigcap") },
         { "bigcup", Operator("\u22C3", null, false, "bigcup") },
         { "bigbot", Operator("\u27D8", null, false, "bigbot") }, //not in iosMath
         { "bigtop", Operator("\u27D9", null, false, "bigtop") }, //not in iosMath
         { "bigodot", Operator("\u2A00", null, false, "bigodot") },
         { "bigoplus", Operator("\u2A01", null, false, "bigoplus") },
         { "bigotimes", Operator("\u2A02", null, false, "bigotimes") },
         { "bigcupdot", Operator("\u2A03", null, false, "bigcupdot") }, //not in iosMath
         { "biguplus", Operator("\u2A04", null, false, "biguplus") },
         { "bigsqcap", Operator("\u2A05", null, false, "bigsqcap") }, //not in iosMath
         { "bigsqcup", Operator("\u2A06", null, false, "bigsqcup") },
         { "bigtimes", Operator("\u2A09", null, false, "bigtimes") }, //not in iosMath
         
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

    public static LargeOperator Operator(string displayChars, bool? limits, bool noLimits = false, string name = "") => new LargeOperator(displayChars, limits, noLimits, name);
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
            "Use Operator(string, bool?, bool) instead.", nameof(type));
        case MathAtomType.Overline:
          return new Overline();
        case MathAtomType.Underline:
          return new Underline();
        case MathAtomType.Space:
          throw new ArgumentException(
            "Do not use Create(MathAtomType.Space, string)." +
            "Use Space(int, bool) instead.", nameof(type));
        default:
          return new MathAtom(type, value, string.Empty);
      }
    }

    public static MathAtom Times => Create(MathAtomType.BinaryOperator, Symbols.Multiplication);
    public static MathAtom Divide => Create(MathAtomType.BinaryOperator, Symbols.Division);

    public static MathAtom Placeholder => Create(MathAtomType.Placeholder, Symbols.WhiteSquare);
    public static MathList PlaceholderList => new MathList { Placeholder };
    public static Fraction PlaceholderFraction => new Fraction { Numerator = PlaceholderList, Denominator = PlaceholderList };
    public static Radical PlaceholderRadical => new Radical { Degree = PlaceholderList, Radicand = PlaceholderList };
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

    public static string LatexSymbolNameForAtom(MathAtom atom) => Commands.TryGetKey(atom, out var name) ? name : null;

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


    public static Structures.Result<IMathAtom> Table(string environment, List<List<IMathList>> rows) {
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
