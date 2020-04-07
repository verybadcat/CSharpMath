using System;
using System.Collections.Generic;

namespace CSharpMath.Atom {
  using Atoms;
  //https://mirror.hmc.edu/ctan/macros/latex/contrib/unicode-math/unimath-symbols.pdf
  public static class LaTeXSettings {
    public static MathAtom Times => new BinaryOperator("×");
    public static MathAtom Divide => new BinaryOperator("÷");
    public static MathAtom Placeholder => new Placeholder("\u25A1");
    public static MathList PlaceholderList => new MathList { Placeholder };

    public static MathAtom? ForAscii(sbyte c) {
      if (c < 0) throw new ArgumentOutOfRangeException(nameof(c), c, "The character cannot be negative");
      var s = ((char)c).ToStringInvariant();
      if (char.IsControl((char)c) || char.IsWhiteSpace((char)c)) {
        return null; // skip spaces
      }
      if (c >= '0' && c <= '9') {
        return new Number(s);
      }
      if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) {
        return new Variable(s);
      }
      switch (c) {
        case (sbyte)'$':
        case (sbyte)'%':
        case (sbyte)'#':
        case (sbyte)'&':
        case (sbyte)'~':
        case (sbyte)'\'':
        case (sbyte)'^':
        case (sbyte)'_':
        case (sbyte)'{':
        case (sbyte)'}':
        case (sbyte)'\\': // All these are special characters we don't support.
          return null;
        case (sbyte)'(':
        case (sbyte)'[':
          return new Open(s);
        case (sbyte)')':
        case (sbyte)']':
          return new Close(s);
        case (sbyte)'!':
        case (sbyte)'?':
          return new Close(s, hasCorrespondingOpen: false);
        case (sbyte)',':
        case (sbyte)';':
          return new Punctuation(s);
        case (sbyte)'=':
        case (sbyte)'<':
        case (sbyte)'>':
          return new Relation(s);
        case (sbyte)':': // Colon is a ratio. Regular colon is \colon
          return new Relation("\u2236");
        case (sbyte)'-': // Use the math minus sign
          return new BinaryOperator("\u2212");
        case (sbyte)'+':
        case (sbyte)'*': // Star operator, not multiplication
          return new BinaryOperator(s);
        case (sbyte)'.':
          return new Number(s);
        case (sbyte)'"':
        case (sbyte)'/':
        case (sbyte)'@':
        case (sbyte)'`':
        case (sbyte)'|':
          return new Ordinary(s);
        default:
          throw new Structures.InvalidCodePathException
            ($"Ascii character {c} should have been accounted for.");
      }
    }

    public static Structures.AliasDictionary<string, Boundary> BoundaryDelimiters { get; } =
      new Structures.AliasDictionary<string, Boundary> {
        { ".", Boundary.Empty }, // . means no delimiter
        { "(", new Boundary("(") },
        { ")", new Boundary(")") },
        { "[", new Boundary("[") },
        { "]", new Boundary("]") },
        { "{", "lbrace", new Boundary("{") },
        { "}", "rbrace", new Boundary("}") },
        { "<", "langle", new Boundary("\u2329") },
        { ">", "rangle", new Boundary("\u232A") },
        { "/", new Boundary("/") },
        { "\\", "backslash", new Boundary("\\") },
        { "|", "vert", new Boundary("|") },
        { "||", "Vert", new Boundary("\u2016") },
        { "uparrow", new Boundary("\u2191") },
        { "downarrow", new Boundary("\u2193") },
        { "updownarrow", new Boundary("\u2195") },
        { "Uparrow", new Boundary("\u21D1") },
        { "Downarrow", new Boundary("\u21D3") },
        { "Updownarrow", new Boundary("\u21D5") },
        { "lgroup", new Boundary("\u27EE") },
        { "rgroup", new Boundary("\u27EF") },
        { "lceil", new Boundary("\u2308") },
        { "rceil", new Boundary("\u2309") },
        { "lfloor", new Boundary("\u230A") },
        { "rfloor", new Boundary("\u230B") }
      };

    public static Structures.AliasDictionary<string, FontStyle> FontStyles { get; } =
      new Structures.AliasDictionary<string, FontStyle> {
        { "mathnormal", FontStyle.Default },
        { "mathrm", "rm", "text", FontStyle.Roman },
        { "mathbf", "bf", FontStyle.Bold },
        { "mathcal", "cal", FontStyle.Caligraphic },
        { "mathtt", FontStyle.Typewriter },
        { "mathit", "mit", FontStyle.Italic },
        { "mathsf", FontStyle.SansSerif },
        { "mathfrak", "frak", FontStyle.Fraktur },
        { "mathbb", FontStyle.Blackboard },
        { "mathbfit", "bm", FontStyle.BoldItalic },
      };

    public static MathAtom? AtomForCommand(string symbolName) =>
      Commands.TryGetValue(
        symbolName ?? throw new ArgumentNullException(nameof(symbolName)),
        out var symbol) ? symbol.Clone(false) : null;

    public static string? CommandForAtom(MathAtom atom) {
      var atomWithoutScripts = atom.Clone(false);
      atomWithoutScripts.Superscript.Clear();
      atomWithoutScripts.Subscript.Clear();
      if (atomWithoutScripts is IMathListContainer container)
        foreach (var list in container.InnerLists)
          list.Clear();
      return Commands.TryGetKey(atomWithoutScripts, out var name) ? name : null;
    }

    public static Structures.AliasDictionary<string, MathAtom> Commands { get; } =
      new Structures.AliasDictionary<string, MathAtom> {
         // Custom additions
         { " ", new Ordinary(" ") },
         { "degree", new Ordinary("\u00B0") },

         // LaTeX Symbol List: https://rpi.edu/dept/arc/training/latex/LaTeX_symbols.pdf
         // (Included in the same folder as this file)

         // Command <-> Unicode: https://www.johndcook.com/unicode_latex.html
         // Unicode char lookup: https://unicode-table.com/en/search/

         // Following tables are from the LaTeX Symbol List
         // Table 1: Escapable “Special” Characters
         { "$", new Ordinary("$") },
         { "%", new Ordinary("%") },
         { "_", new Ordinary("_") },
         { "}", "rbrace", new Close("}") },
         { "&", new Ordinary("&") },
         { "#", new Ordinary("#") },
         { "{", "lbrace", new Open("{") },

         // Table 2: LaTeX2ε Commands Deﬁned to Work in Both Math and Text Mode
         // $ is defined in Table 1
         { "P", new Ordinary("¶") },
         { "S", new Ordinary("§") },
         // _ is defined in Table 1
         { "copyright", new Ordinary("©") },
         { "dag", new Ordinary("†") },
         { "ddag", new Ordinary("‡") },
         { "dots", new Ordinary("…") },
         { "pounds", new Ordinary("£") },
         // { is defined in Table 1
         // } is defined in Table 1

         // Table 3: Non-ASCII Letters (Excluding Accented Letters)
         { "aa", new Ordinary("å") },
         { "AA", "angstrom", new Ordinary("Å") },
         { "AE", new Ordinary("Æ") },
         { "ae", new Ordinary("æ") },
         { "DH", new Ordinary("Ð") },
         { "dh", new Ordinary("ð") },
         { "DJ", new Ordinary("Đ") },
         { "dj", new Ordinary("đ") },
         { "L", new Ordinary("Ł") },
         { "l", new Ordinary("ł") },
         { "NG", new Ordinary("Ŋ") },
         { "ng", new Ordinary("ŋ") },
         { "o", new Ordinary("ø") },
         { "O", new Ordinary("Ø") },
         { "OE", new Ordinary("Œ") },
         { "oe", new Ordinary("œ") },
         { "ss", new Ordinary("ß") },
         { "SS", new Ordinary("SS") },
         { "TH", new Ordinary("Þ") },
         { "th", new Ordinary("þ") },

         // Table 4: Greek Letters
         { "alpha", new Variable("α") },
         { "beta", new Variable("β") },
         { "gamma", new Variable("γ") },
         { "delta", new Variable("δ") },
         { "epsilon", new Variable("ε") },
         { "varepsilon", new Variable("ɛ") },
         { "zeta", new Variable("ζ") },
         { "eta", new Variable("η") },
         { "theta", new Variable("θ") },
         { "vartheta", new Variable("ϑ") },
         { "iota", new Variable("ι") },
         { "kappa", new Variable("κ") },
         { "lambda", new Variable("λ") },
         { "mu", new Variable("µ") },
         { "nu", new Variable("ν") },
         { "xi", new Variable("ξ") },
         { "omicron", new Variable("ο") },
         { "pi", new Variable("π") },
         { "varpi", new Variable("ϖ") },
         { "rho", new Variable("ρ") },
         { "varrho", new Variable("ϱ") },
         { "sigma", new Variable("σ") },
         { "varsigma", new Variable("ς") },
         { "tau", new Variable("τ") },
         { "upsilon", new Variable("υ") },
         { "phi", new Variable("φ") },
         { "varphi", new Variable("ϕ") },
         { "chi", new Variable("χ") },
         { "psi", new Variable("ψ") },
         { "omega", new Variable("ω") },

         { "Gamma", new Variable("Γ") },
         { "Delta", new Variable("∆") },
         { "Theta", new Variable("Θ") },
         { "Lambda", new Variable("Λ") },
         { "Xi", new Variable("Ξ") },
         { "Pi", new Variable("Π") },
         { "Sigma", new Variable("Σ") },
         { "Upsilon", new Variable("Υ") },
         { "Phi", new Variable("Φ") },
         { "Psi", new Variable("Ψ") },
         { "Omega", new Variable("Ω") },
         // (The remaining Greek majuscules can be produced with ordinary Latin letters.
         // The symbol “M”, for instance, is used for both an uppercase “m” and an uppercase “µ”.

         // Table 5: Punctuation Marks Not Found in OT
         // [Skip text mode commands]

         // Table 6: Predeﬁned LaTeX2ε Text-Mode Commands
         // [Skip text mode commands]

         // Table 7: Binary Operation Symbols
         { "pm", new BinaryOperator("±") },
         { "mp", new BinaryOperator("∓") },
         { "times", Times },
         { "div", Divide },
         { "ast", new BinaryOperator("∗") },
         { "star" , new BinaryOperator("⋆") },
         { "circ" , new BinaryOperator("◦") },
         { "bullet", new BinaryOperator("•") },
         { "cdot" , new BinaryOperator("·") },
         // +
         { "cap", new BinaryOperator("∩") },
         { "cup", new BinaryOperator("∪") },
         { "uplus", new BinaryOperator("⊎") },
         { "sqcap", new BinaryOperator("⊓") },
         { "sqcup", new BinaryOperator("⊔") },
         { "vee", "lor", new BinaryOperator("∨") },
         { "wedge", "land", new BinaryOperator("∧") },
         { "setminus", new BinaryOperator("∖") },
         { "wr", new BinaryOperator("≀") },
         // -
         { "diamond", new BinaryOperator("⋄") },
         { "bigtriangleup", new BinaryOperator("△") },
         { "bigtriangledown", new BinaryOperator("▽") },
         { "triangleleft", new BinaryOperator("◃") },
         { "triangleright", new BinaryOperator("▹") },
         { "lhd", new BinaryOperator("⊲") },
         { "rhd", new BinaryOperator("⊳") },
         { "unlhd", new BinaryOperator("⊴") },
         { "unrhd", new BinaryOperator("⊵") },
         { "oplus", new BinaryOperator("⊕") },
         { "ominus", new BinaryOperator("⊖") },
         { "otimes", new BinaryOperator("⊗") },
         { "oslash", new BinaryOperator("⊘") },
         { "odot", new BinaryOperator("⊙") },
         { "bigcirc", new BinaryOperator("◯") },
         { "dagger", new BinaryOperator("†") },
         { "ddagger", new BinaryOperator("‡") },
         { "amalg", new BinaryOperator("⨿") },

         // Table 8: Relation Symbols
         { "leq", "le", new Relation("≤") },
         { "geq", "ge", new Relation("≥") },
         { "equiv", new Relation("≡") },
         { "models", new Relation("⊧") },
         { "prec", new Relation("≺") },
         { "succ", new Relation("≻") },
         { "sim", new Relation("∼") },
         { "perp", new Relation("⟂") },
         { "preceq", new Relation("⪯") },
         { "succeq", new Relation("⪰") },
         { "simeq", new Relation("≃") },
         { "mid", new Relation("∣") },
         { "ll", new Relation("≪") },
         { "gg", new Relation("≫") },
         { "asymp", new Relation("≍") },
         { "parallel", new Relation("∥") },
         { "subset", new Relation("⊂") },
         { "supset", new Relation("⊃") },
         { "approx", new Relation("≈") },
         { "bowtie", new Relation("⋈") },
         { "subseteq", new Relation("⊆") },
         { "supseteq", new Relation("⊇") },
         { "cong", new Relation("≅") },
         { "Join", new Relation("⨝") }, // Capital J is intentional
         { "sqsubset", new Relation("⊏") },
         { "sqsupset", new Relation("⊐") },
         { "neq", "ne", new Relation("≠") },
         { "smile", new Relation("⌣") },
         { "sqsubseteq", new Relation("⊑") },
         { "sqsupseteq", new Relation("⊒") },
         { "doteq", new Relation("≐") },
         { "frown", new Relation("⌢") },
         { "in", new Relation("∈") },
         { "ni", new Relation("∋") },
         { "notin", new Relation("∉") },
         { "propto", new Relation("∝") },
         // =
         { "vdash", new Relation("⊢") },
         { "dashv", new Relation("⊣") },
         // <
         // >
         // :
         
         // Table 9: Punctuation Symbols
         // ,
         // ;
         { "colon", new Punctuation(":") }, // \colon is different from : which is a relation
         { "ldotp", new Punctuation(".") }, // Aka the full stop or decimal dot
         { "cdotp", new Punctuation("·") },

         // Table 10: Arrow Symbols 
         { "leftarrow", "gets", new Relation("←") },
         { "longleftarrow", new Relation("⟵") },
         { "uparrow", new Relation("↑") },
         { "Leftarrow", new Relation("⇐") },
         { "Longleftarrow", new Relation("⟸") },
         { "Uparrow", new Relation("⇑") },
         { "rightarrow", "to", new Relation("→") },
         { "longrightarrow", new Relation("⟶") },
         { "downarrow", new Relation("↓") },
         { "Rightarrow", new Relation("⇒") },
         { "Longrightarrow", new Relation("⟹") },
         { "Downarrow", new Relation("⇓") },
         { "leftrightarrow", new Relation("↔") },
         { "Leftrightarrow", new Relation("⇔") },
         { "updownarrow", new Relation("↕") },
         { "longleftrightarrow", new Relation("⟷") },
         { "Longleftrightarrow", "iff", new Relation("⟺") },
         { "Updownarrow", new Relation("⇕") },
         { "mapsto", new Relation("↦") },
         { "longmapsto", new Relation("⟼") },
         { "nearrow", new Relation("↗") },
         { "hookleftarrow", new Relation("↩") },
         { "hookrightarrow", new Relation("↪") },
         { "searrow", new Relation("↘") },
         { "leftharpoonup", new Relation("↼") },
         { "rightharpoonup", new Relation("⇀") },
         { "swarrow", new Relation("↙") },
         { "leftharpoondown", new Relation("↽") },
         { "rightharpoondown", new Relation("⇁") },
         { "nwarrow", new Relation("↖") },
         { "rightleftharpoons", new Relation("⇌") },
         { "leadsto", new Relation("↝") },

         // Table 11: Miscellaneous Symbols
         { "ldots", new Ordinary("…") },
         { "aleph", new Ordinary("ℵ") },
         { "hbar", new Ordinary("ℏ") },
         { "imath", new Ordinary("𝚤") },
         { "jmath", new Ordinary("𝚥") },
         { "ell", new Ordinary("ℓ") },
         { "wp", new Ordinary("℘") },
         { "Re", new Ordinary("ℜ") },
         { "Im", new Ordinary("ℑ") },
         { "mho", new Ordinary("℧") },
         { "cdots", new Ordinary("⋯") },
         // \prime is removed because Unicode has no matching character
         { "emptyset", new Ordinary("∅") },
         { "nabla", new Ordinary("∇") },
         { "surd", new Ordinary("√") },
         { "top", new Ordinary("⊤") },
         { "bot", new Ordinary("⊥") },
         { "|", "Vert", new Ordinary("‖") },
         { "angle", new Ordinary("∠") },
         // .
         { "vdots", new Ordinary("⋮") },
         { "forall", new Ordinary("∀") },
         { "exists", new Ordinary("∃") },
         { "neg", "lnot", new Ordinary("¬") },
         { "flat", new Ordinary("♭") },
         { "natural", new Ordinary("♮") },
         { "sharp", new Ordinary("♯") },
         { "backslash", new Ordinary("\\") },
         { "partial", new Ordinary("𝜕") },
         { "vert", new Ordinary("|") },
         { "ddots", new Ordinary("⋱") },
         { "infty", new Ordinary("∞") },
         { "triangle", new Ordinary("\u25B3") },

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
         { "lim", new LargeOperator("lim", null) },
         { "limsup", new LargeOperator("lim sup", null) },
         { "liminf", new LargeOperator("lim inf", null) },
         { "max", new LargeOperator("max", null) },
         { "min", new LargeOperator("min", null) },
         { "sup", new LargeOperator("sup", null) },
         { "inf", new LargeOperator("inf", null) },
         { "det", new LargeOperator("det", null) },
         { "Pr", new LargeOperator("Pr", null) },
         { "gcd", new LargeOperator("gcd", null) },
         
         // Large operators
         { "prod", new LargeOperator("\u220F", null) },
         { "coprod", new LargeOperator("\u2210", null) },
         { "sum", new LargeOperator("\u2211", null) },
         { "int", new LargeOperator("\u222B", false) },
         { "iint", new LargeOperator("\u222C", false) }, //not in iosMath
         { "iiint", new LargeOperator("\u222D", false) }, //not in iosMath
         { "iiiint", new LargeOperator("\u2A0C", false) }, //not in iosMath
         { "oint", new LargeOperator("\u222E", false) },
         { "oiint", new LargeOperator("\u222F", false) }, //not in iosMath
         { "oiiint", new LargeOperator("\u2230", false) }, //not in iosMath
         { "intclockwise", new LargeOperator("\u2231", false) }, //not in iosMath
         { "awint", new LargeOperator("\u2A11", false) }, //not in iosMath
         { "varointclockwise", new LargeOperator("\u2232", false) }, //not in iosMath
         { "ointctrclockwise", new LargeOperator("\u2233", false) }, //not in iosMath
         { "bigwedge", new LargeOperator("\u22C0", null) },
         { "bigvee", new LargeOperator("\u22C1", null) },
         { "bigcap", new LargeOperator("\u22C2", null) },
         { "bigcup", new LargeOperator("\u22C3", null) },
         { "bigbot", new LargeOperator("\u27D8", null) }, //not in iosMath
         { "bigtop", new LargeOperator("\u27D9", null) }, //not in iosMath
         { "bigodot", new LargeOperator("\u2A00", null) },
         { "bigoplus", new LargeOperator("\u2A01", null) },
         { "bigotimes", new LargeOperator("\u2A02", null) },
         { "bigcupdot", new LargeOperator("\u2A03", null) }, //not in iosMath
         { "biguplus", new LargeOperator("\u2A04", null) },
         { "bigsqcap", new LargeOperator("\u2A05", null) }, //not in iosMath
         { "bigsqcup", new LargeOperator("\u2A06", null) },
         { "bigtimes", new LargeOperator("\u2A09", null) }, //not in iosMath
         
         
         
         // Spacing
         { ",", new Space(Structures.Space.ShortSpace) },
         { ":", ">", new Space(Structures.Space.MediumSpace) },
         { ";", new Space(Structures.Space.LongSpace) },
         { "!", new Space(-Structures.Space.ShortSpace) },
         { "enspace", new Space(Structures.Space.EmWidth / 2) },
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
  }
}
