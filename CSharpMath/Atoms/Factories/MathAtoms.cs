using CSharpMath.Constants;
using CSharpMath.Enumerations;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;

namespace CSharpMath.Atoms {
  public static class MathAtoms {
    private static Dictionary<string, string> _aliases = null;
    public static Dictionary<string, string> Aliases {
      get {
        if (_aliases == null) {
          _aliases = new Dictionary<string, string> {
            {"lnot", "neg" },
            {"land", "wedge" },
            {"lor", "vee" },
            {"ne", "neq" },
            {"le", "leq" },
            {"ge", "geq" },
            {"lbrace", "{" },
            {"rbrace", "}" },
            {"Vert", "|" },
            {"gets", "leftarrow" },
            {"to", "rightarrow" },
            {"iff", "Longleftrightarrow" },
            {"AA", "angstrom" }
          };
        }
        return _aliases;
      }
    }
    public static MathAtom Create(MathAtomType type, string value) {
      switch (type) {
        case MathAtomType.Accent:
          return new Accent(value);
        case MathAtomType.Color:
          return new MathColor();
        case MathAtomType.Fraction:
          return new Fraction();
        case MathAtomType.Inner:
          return new Inner();
        case MathAtomType.LargeOperator:
          return new LargeOperator(value, true);
        case MathAtomType.Overline:
          return new Overline();
        case MathAtomType.Underline:
          return new Underline();
        case MathAtomType.Space:
          return new MathSpace(0);
        default:
          return new MathAtom(type, value);
      }
    }
    public static MathAtom Create(MathAtomType type, char value)
      => Create(type, value.ToString());
    public static MathAtom Placeholder
      => Create(MathAtomType.Placeholder, Symbols.WhiteSquare);

    public static MathAtom Times
      => Create(MathAtomType.BinaryOperator, Symbols.Multiplication);

    public static MathAtom Divide
      => Create(MathAtomType.BinaryOperator, Symbols.Division);

    private static MathList PlaceholderList {
      get {
        var r = new MathList();
        r.AddAtom(Placeholder);
        return r;
      }
    }

    public static Fraction PlaceholderFraction {
      get {
        var r = new Fraction();
        r.Numerator = PlaceholderList;
        r.Denominator = PlaceholderList;
        return r;
      }
    }

    public static Radical PlaceholderRadical {
      get {
        var r = new Radical();
        r.Degree = PlaceholderList;
        r.Radicand = PlaceholderList;
        return r;
      }
    }

    public static MathAtom PlaceholderSquareRoot {
      get {
        var r = new Radical();
        r.Radicand = PlaceholderList;
        return r;
      }
    }

    public static LargeOperator Operator(string name, bool limits)
      => new LargeOperator(name, limits);

    public static MathAtom ForCharacter(char c) {
      if (c < 0x21 || c > 0x7E) {
        return null; // skip non ascii characters and spaces
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
          return Create(MathAtomType.Relation, "\u2236");
        case '-': // use the math minus sign
          return Create(MathAtomType.BinaryOperator, "\u2212");
        case '+':
        case '*':
          return Create(MathAtomType.BinaryOperator, c);
        case '.':
          return Create(MathAtomType.Number, c);
        case '"':
        case '/':
        case '@':
        case '`':
        case '|':
          return Create(MathAtomType.Ordinary, c);
        default:
          throw new Exception($"Ascii character {c} should have been accounted for.");
      }
    }

    internal static FontStyle? FontStyle(string command) {
      var dict = FontStyleExtensions.FontStyles;
      FontStyle? r = dict.ContainsKey(command) ? (FontStyle?) dict[command] : null;
      return r;
    }

    private static Dictionary<string, IMathAtom> _supportedLatexSymbols;

    public static Dictionary<string, IMathAtom> Commands {
      get {
        if (_supportedLatexSymbols == null) {
          _supportedLatexSymbols = new Dictionary<string, IMathAtom> {
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
                     { "leftarrow", Create(MathAtomType.Relation, "\u2190") },
                     { "uparrow", Create(MathAtomType.Relation, "\u2191") },
                     { "rightarrow", Create(MathAtomType.Relation, "\u2192") },
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
                     { "Longleftrightarrow", Create(MathAtomType.Relation, "\u27FA") },
                     
                     // Relations
                     { "leq", Create(MathAtomType.Relation, Symbols.LessEqual) },
                     { "geq", Create(MathAtomType.Relation, Symbols.GreaterEqual) },
                     { "neq", Create(MathAtomType.Relation, Symbols.NotEqual) },
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
                     { "wedge", Create(MathAtomType.BinaryOperator, "\u2227") },
                     { "vee", Create(MathAtomType.BinaryOperator, "\u2228") },
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
                     { "log", Operator("log" , false) },
                     { "lg", Operator("lg" , false) },
                     { "ln", Operator("ln" , false) },
                     { "sin", Operator("sin" , false) },
                     { "arcsin", Operator("arcsin" , false) },
                     { "sinh", Operator("sinh" , false) },
                     { "cos", Operator("cos" , false) },
                     { "arccos", Operator("arccos" , false) },
                     { "cosh", Operator("cosh" , false) },
                     { "tan", Operator("tan" , false) },
                     { "arctan", Operator("arctan" , false) },
                     { "tanh", Operator("tanh" , false) },
                     { "cot", Operator("cot" , false) },
                     { "coth", Operator("coth" , false) },
                     { "sec", Operator("sec" , false) },
                     { "csc", Operator("csc" , false) },
                     { "arg", Operator("arg" , false) },
                     { "ker", Operator("ker" , false) },
                     { "dim", Operator("dim" , false) },
                     { "hom", Operator("hom" , false) },
                     { "exp", Operator("exp" , false) },
                     { "deg", Operator("deg" , false) },
                     
                     // Limit operators
                     { "lim", Operator("lim" , true) },
                     { "limsup", Operator("lim sup" , true) },
                     { "liminf", Operator("lim inf" , true) },
                     { "max", Operator("max" , true) },
                     { "min", Operator("min" , true) },
                     { "sup", Operator("sup" , true) },
                     { "inf", Operator("inf" , true) },
                     { "det", Operator("det" , true) },
                     { "Pr", Operator("Pr" , true) },
                     { "gcd", Operator("gcd" , true) },
                     
                     // Large operators
                     { "prod", Operator("\u220F" , true) },
                     { "coprod", Operator("\u2210" , true) },
                     { "sum", Operator("\u2211" , true) },
                     { "int", Operator("\u222B" , false) },
                     { "oint", Operator("\u222E" , false) },
                     { "bigwedge", Operator("\u22C0" , true) },
                     { "bigvee", Operator("\u22C1" , true) },
                     { "bigcap", Operator("\u22C2" , true) },
                     { "bigcup", Operator("\u22C3" , true) },
                     { "bigodot", Operator("\u2A00" , true) },
                     { "bigoplus", Operator("\u2A01" , true) },
                     { "bigotimes", Operator("\u2A02" , true) },
                     { "biguplus", Operator("\u2A04" , true) },
                     { "bigsqcup", Operator("\u2A06" , true) },
                     
                     // Latex command characters
                     { "{", Create(MathAtomType.Open, "{") },
                     { "}", Create(MathAtomType.Close, "}") },
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
                     { "neg", Create(MathAtomType.Ordinary, "\u00AC") },
                     { "angstrom", Create(MathAtomType.Ordinary, "\u00C5") },
                     { "|", Create(MathAtomType.Ordinary, "\u2016") },
                     { "vert", Create(MathAtomType.Ordinary, "|") },
                     { "ldots", Create(MathAtomType.Ordinary, "\u2026") },
                     { "prime", Create(MathAtomType.Ordinary, "\u2032") },
                     { "hbar", Create(MathAtomType.Ordinary, "\u210F") },
                     { "Im", Create(MathAtomType.Ordinary, "\u2111") },
                     { "ell", Create(MathAtomType.Ordinary, "\u2113") },
                     { "wp", Create(MathAtomType.Ordinary, "\u2118") },
                     { "Re", Create(MathAtomType.Ordinary, "\u211C") },
                     { "mho", Create(MathAtomType.Ordinary, "\u2127") },
                     { "aleph", Create(MathAtomType.Ordinary, "\u2135") },
                     { "forall", Create(MathAtomType.Ordinary, "\u2200") },
                     { "exists", Create(MathAtomType.Ordinary, "\u2203") },
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
                     { ",", new MathSpace(3) },
                     { ">", new MathSpace(4) },
                     { ";", new MathSpace(5) },
                     { "!", new MathSpace(-3) },
                     { "quad", new MathSpace(18) },  // quad = 1em = 18mu
                     { "qquad", new MathSpace(36) }, // qquad = 2em
                     
                     // Style
                     { "displaystyle", new MathStyle(LineStyle.Display) },
                     { "textstyle", new MathStyle(LineStyle.Text) },
                     { "scriptstyle", new MathStyle(LineStyle.Script) },
                     { "scriptscriptstyle",  new MathStyle(LineStyle.ScriptScript) }
          };
        }
        return _supportedLatexSymbols;
      }
    }

    public static MathList MathListForCharacters(string chars) {
      var r = new MathList();
      foreach (char c in chars) {
        r.AddAtom(ForCharacter(c));
      }
      return r;
    }

    public static IMathAtom ForLatexSymbolName(string symbolName) {
      if (symbolName == null) {
        throw new ArgumentNullException();
      }
      if (Aliases.ContainsKey(symbolName)) {
        symbolName = Aliases[symbolName];
      }
      Dictionary<string, IMathAtom> commands = Commands;
      if (commands.ContainsKey(symbolName)) {
        return AtomCloner.Clone(commands[symbolName], false);
      }
      return null;
    }

    public static string LatexSymbolNameForAtom(IMathAtom atom) {
      if (atom.Nucleus.IsEmpty()) {
        return null;
      }
      string r = TextToLatexSymbolNames.GetValueOrDefault(atom.Nucleus);
      return r;
    }

    public static void AddLatexSymbol(string name, IMathAtom atom) {
      _supportedLatexSymbols[name] = atom;
      if (atom.Nucleus.IsNonEmpty()) {
        _textToLatexSymbolNames[atom.Nucleus] = name;
      }
    }

    private static Dictionary<string, string> _textToLatexSymbolNames = null;
    public static Dictionary<string, string> TextToLatexSymbolNames {
      get {
        if (_textToLatexSymbolNames == null) {
          _textToLatexSymbolNames = DictionaryHelpers.BuildValueToKeyDictionary(Commands.Keys, key => Commands[key].Nucleus, str => !(string.IsNullOrEmpty(str)));
        }
        return _textToLatexSymbolNames;
      }
    }

    public static IEnumerable<string> SupportedLatexSymbolNames => _supportedLatexSymbols.Keys;

    public static IAccent Accent(string accentName) =>
      AccentNames.NameToValue.ContainsKey(accentName) ? new Accent(AccentNames.NameToValue[accentName]) : null;

    public static string AccentName(IAccent accent)
      => AccentNames.ValueToName.GetValueOrDefault(accent.Nucleus);

    public static IMathAtom BoundaryAtom(string delimiterName) {
      var dict = DelimiterNames.NameToValue;
      var value = dict.GetValueOrDefault(delimiterName);
      if (value == null) {
        return null;
      }
      return Create(MathAtomType.Boundary, value);
    }

    public static string DelimiterName(IMathAtom boundaryAtom) {
      string r = null;
      if (boundaryAtom.AtomType == MathAtomType.Boundary) {
        r = DelimiterNames.ValueToName.GetValueOrDefault(boundaryAtom.Nucleus);
      }
      return r;
    }

    public static IFraction Fraction(IMathList numerator, IMathList denominator) {
      var fraction = new Fraction {
        Numerator = numerator,
        Denominator = denominator
      };
      return fraction;
    }

    public static IFraction Fraction(string numerator, string denominator)
      => Fraction(MathListForCharacters(numerator), MathListForCharacters(denominator));

    private static Dictionary<string, List<string>> _matrixEnvironments { get; } =
      new Dictionary<string, List<string>> {
        { "matrix", new List<string>() } ,
        {"pmatrix", new List<string> {"(", ")"} } ,
        {"bmatrix", new List<string> {"[", "]"} },
        {"Bmatrix", new List<string>{"{", "}"} },
        {"vmatrix", new List<string>{"vert", "vert"} },
        {"Vmatrix", new List<string>{"Vert", "Vert"} }
      };
      

    public static IMathAtom Table(
      string environment,
      List<List<IMathList>> rows,
      out string errorMessage) {
      errorMessage = null;
      var table = new MathTable(environment) {
        Cells = rows
      };
      IMathAtom r = null;
      if (environment!=null && _matrixEnvironments.ContainsKey(environment)) {
        var delimiters = _matrixEnvironments[environment];
        table.Environment = "matrix"; // Environment is set to matrix as delimiters are converted to latex outside the table.
        table.InterColumnSpacing = 18;

        var style = new MathStyle(LineStyle.Text);
        foreach (var row in table.Cells) {
          foreach (var cell in row) {
            cell.Insert(0, style);
          }
        }

        if (delimiters.Count == 2) {
          var inner = new Inner {
            LeftBoundary = BoundaryAtom(delimiters[0]),
            RightBoundary = BoundaryAtom(delimiters[1]),
            InnerList = MathLists.WithAtoms(table)
          };
          r = inner;
        } else {
          r = table;
        }
      }
      else if (environment == null) {
        table.InterRowAdditionalSpacing = 1;
        for (int i=0; i<table.NColumns; i++) {
          table.SetAlignment(ColumnAlignment.Left, i);
        }
        r = table;
      }
      else if (environment == "eqalign" || environment == "split" || environment == "aligned") {
        if (table.NColumns!=2) {
          errorMessage = environment + " environment can have only 2 columns";
        } else {
          // add a spacer before each of the second column elements, in order to create the correct spacing for "=" and other relations.
          var spacer = Create(MathAtomType.Ordinary, "");
          foreach (var row in table.Cells) {
            if (row.Count > 1) {
              row[1].Insert(0, spacer);
            }
          }
          table.InterRowAdditionalSpacing = 1;
          table.SetAlignment(ColumnAlignment.Right, 0);
          table.SetAlignment(ColumnAlignment.Left, 1);
          r = table;
        }
      }
      else if (environment == "eqnarray") {
        if (table.NColumns!=3) {
          errorMessage = environment + " must have exactly 3 columns.";
        } else {
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 18;
          table.SetAlignment(ColumnAlignment.Right, 0);
          table.SetAlignment(ColumnAlignment.Center, 1);
          table.SetAlignment(ColumnAlignment.Left, 2);
          r = table;
        }
      }
      else if (environment == "cases") {
        if (table.NColumns!=2) {
          errorMessage = "cases environment must have exactly 2 columns";
        } else {
          table.InterColumnSpacing = 18;
          table.SetAlignment(ColumnAlignment.Left, 0);
          table.SetAlignment(ColumnAlignment.Left, 1);
          var style = new MathStyle(LineStyle.Text);
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
          r = inner;
        }

      }
      return r;

    }
   
  }
}
