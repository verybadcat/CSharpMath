using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CSharpMath.Atom {
  using Atoms;
  using Structures;
  using Space = Atoms.Space;
  //https://mirror.hmc.edu/ctan/macros/latex/contrib/unicode-math/unimath-symbols.pdf
  public static class LaTeXSettings {
    static readonly Dictionary<Boundary, string> boundaryDelimitersReverse = new Dictionary<Boundary, string>();
    public static IReadOnlyDictionary<Boundary, string> BoundaryDelimitersReverse => boundaryDelimitersReverse;
    public static LaTeXCommandDictionary<Boundary> BoundaryDelimiters { get; } =
      new LaTeXCommandDictionary<Boundary>(
        consume => {
          if (consume.IsEmpty) throw new InvalidCodePathException("Unexpected empty " + nameof(consume));
          if (char.IsHighSurrogate(consume[0])) {
            if (consume.Length == 1)
              return "Unexpected single high surrogate without its counterpart";
            if (!char.IsLowSurrogate(consume[1]))
              return "Low surrogate not found after high surrogate";
            return "Invalid delimiter " + consume.Slice(0, 2).ToString();
          } else {
            if (char.IsLowSurrogate(consume[0]))
              return "Unexpected low surrogate without its counterpart";
            return "Invalid delimiter " + consume[0];
          }
        },
        command => "Invalid delimiter " + command.ToString(),
        (key, value) => {
          if (!boundaryDelimitersReverse.ContainsKey(value))
            boundaryDelimitersReverse.Add(value, key);
        })
      {
        { @".", Boundary.Empty }, // . means no delimiter

        // Table 14: Delimiters
        { @"(", new Boundary("(") },
        { @")", new Boundary(")") },
        { @"\uparrow", "↑", new Boundary("↑") },
        { @"\Uparrow", "⇑", new Boundary("⇑") },
        { @"[", new Boundary("[") },
        { @"]", new Boundary("]") },
        { @"\downarrow", "↓", new Boundary("↓") },
        { @"\Downarrow", "⇓", new Boundary("⇓") },
        { @"\{", @"\lbrace", new Boundary("{") },
        { @"\}", @"\rbrace", new Boundary("}") },
        { @"\updownarrow", "↕", new Boundary("↕") },
        { @"\Updownarrow", "⇕", new Boundary("⇕") },
        { @"\lfloor", "⌊", new Boundary("⌊") },
        { @"\rfloor", "⌋", new Boundary("⌋") },
        { @"\lceil", "⌈", new Boundary("⌈") },
        { @"\rceil", "⌉", new Boundary("⌉") },
        { @"<", @"\langle", "〈", new Boundary("〈") },
        { @">", @"\rangle", "〉", new Boundary("〉") },
        { @"/", new Boundary("/") },
        { @"\\", @"backslash", new Boundary("\\") },
        { @"|", @"\vert", new Boundary("|") },
        { @"\|", @"\Vert", "‖", new Boundary("‖") },

        // Table 15: Large Delimiters
        // { @"\lmoustache", "⎰", new Boundary("⎰") }, // Glyph not in Latin Modern Math
        // { @"\rmoustache", "⎱", new Boundary("⎱") }, // Glyph not in Latin Modern Math
        { @"\rgroup", "⟯", new Boundary("⟯") },
        { @"\lgroup", "⟮", new Boundary("⟮") },
        { @"\arrowvert", new Boundary("|") }, // unsure, copied from \vert
        { @"\Arrowvert", new Boundary("‖") }, // unsure, copied from \Vert
        { @"\bracevert", new Boundary("|") }, // unsure, copied from \vert

        // Table 19: AMS Delimiters
        { @"\ulcorner", "⌜", new Boundary("⌜") },
        { @"\urcorner", "⌝", new Boundary("⌝") },
        { @"\llcorner", "⌞", new Boundary("⌞") },
        { @"\lrcorner", "⌟", new Boundary("⌟") },
      };

    static readonly MathAtom? Dummy = Placeholder;
    public static Result<(MathAtom? Atom, MathList? Return)> Ok(MathAtom? atom) => Result.Ok((atom, (MathList?)null));
    public static Result<(MathAtom? Atom, MathList? Return)> OkStyled(MathList styled) => Result.Ok((Dummy, (MathList?)styled));
    public static Result<(MathAtom? Atom, MathList? Return)> OkStop(MathList @return) => Result.Ok(((MathAtom?)null, (MathList?)@return));
    public static ResultImplicitError Err(string error) => Result.Err(error);
    // Lock this object in tests in case threading exceptions happen between command reading and writing
    public static LaTeXCommandDictionary<Func<LaTeXParser, MathList, char, Result<(MathAtom? Atom, MathList? Return)>>> Commands { get; } =
      new LaTeXCommandDictionary<Func<LaTeXParser, MathList, char, Result<(MathAtom? Atom, MathList? Return)>>>(consume => {
        if (consume.IsEmpty) throw new ArgumentException("Unexpected empty " + nameof(consume));
        if (char.IsHighSurrogate(consume[0])) {
          if (consume.Length == 1 || !char.IsLowSurrogate(consume[1]))
            return "Low surrogate not found after high surrogate";
          var atom = new Ordinary(consume.Slice(0, 2).ToString());
          return ((parser, accumulate, stopChar) => Ok(atom), 2);
        } else {
          if (char.IsLowSurrogate(consume[0]))
            return "High surrogate not found before low surrogate";
          var atom = new Ordinary(consume[0].ToStringInvariant());
          return ((parser, accumulate, stopChar) => Ok(atom), 1);
        }
      }, command => "Invalid command " + command.ToString()) {
        #region Atom producers
        { Enumerable.Range(0, 33).Concat(new[] { 127 }).Select(c => ((char)c).ToStringInvariant()),
          _ => (parser, accumulate, stopChar) => {
          if (parser.TextMode) {
            parser.SkipSpaces(); // Multiple spaces are collapsed into one in text mode
            return Ok(new Ordinary(" "));
          } else return Ok(null);
        } },
        { "%", (parser, accumulate, stopChar) => {
          var index = parser.NextChar;
          var length = 0;
          while (parser.HasCharacters) {
            switch (parser.ReadChar()) {
              // https://en.wikipedia.org/wiki/Newline#Unicode
              case '\u000A':
              case '\u000B':
              case '\u000C':
              case '\u0085':
              case '\u2028':
              case '\u2029':
                goto exitWhile;
              case '\u000D':
                if (parser.HasCharacters && parser.ReadChar() != '\u000A')
                  parser.UndoReadChar();
                goto exitWhile;
              default:
                length++;
                break;
            }
          }
          exitWhile:
          return Ok(new Comment(parser.Chars.Substring(index, length)));
        } },
        { @"\frac", (parser, accumulate, stopChar) =>
          parser.ReadArgument().Bind(numerator =>
            parser.ReadArgument().Bind(denominator =>
              Ok(new Fraction(numerator, denominator)))) },
        { @"\binom", (parser, accumulate, stopChar) =>
          parser.ReadArgument().Bind(numerator =>
            parser.ReadArgument().Bind(denominator =>
              Ok(new Fraction(numerator, denominator, false) {
                LeftDelimiter = new Boundary("("),
                RightDelimiter = new Boundary(")")
              }))) },
        { @"\sqrt", (parser, accumulate, stopChar) =>
          parser.ReadArgumentOptional().Bind(degree =>
            parser.ReadArgument().Bind(radicand =>
              Ok(new Radical(degree ?? new MathList(), radicand)))) },
        { @"\left", (parser, accumulate, stopChar) =>
          parser.ReadDelimiter("left").Bind(left => {
            parser.Environments.Push(new LaTeXParser.InnerEnvironment());
            return parser.ReadUntil(stopChar).Bind(innerList => {
              if (!(parser.Environments.PeekOrDefault() is
                LaTeXParser.InnerEnvironment { RightBoundary: { } right })) {
                return Err($@"Missing \right for \left with delimiter {left}");
              }
              parser.Environments.Pop();
              return Ok(new Inner(left, innerList, right));
            });
          }) },
        { @"\overline", (parser, accumulate, stopChar) =>
          parser.ReadArgument().Bind(mathList => Ok(new Overline(mathList))) },
        { @"\underline", (parser, accumulate, stopChar) =>
          parser.ReadArgument().Bind(mathList => Ok(new Underline(mathList))) },
        { @"\begin", (parser, accumulate, stopChar) =>
          parser.ReadEnvironment().Bind(env =>
            parser.ReadTable(env, null, false, stopChar)).Bind(Ok) },
        { @"\color", (parser, accumulate, stopChar) =>
          parser.ReadColor().Bind(
            color => parser.ReadArgument().Bind(
            colored => Ok(new Colored(color, colored)))) },
        { @"\colorbox", (parser, accumulate, stopChar) =>
          parser.ReadColor().Bind(
            color => parser.ReadArgument().Bind(
            colored => Ok(new ColorBox(color, colored)))) },
        { @"\prime", (parser, accumulate, stopChar) =>
          Err(@"\prime won't be supported as Unicode has no matching character. Use ' instead.") },
        { @"\kern", (parser, accumulate, stopChar) =>
          parser.TextMode ? parser.ReadSpace().Bind(kern => Ok(new Space(kern))) : @"\kern is not allowed in math mode" },
        { @"\hskip", (parser, accumulate, stopChar) =>
//TODO \hskip and \mskip: Implement plus and minus for expansion
          parser.TextMode ? parser.ReadSpace().Bind(skip => Ok(new Space(skip))) : @"\hskip is not allowed in math mode" },
        { @"\mkern", (parser, accumulate, stopChar) =>
          !parser.TextMode ? parser.ReadSpace().Bind(kern => Ok(new Space(kern))) : @"\mkern is not allowed in text mode" },
        { @"\mskip", (parser, accumulate, stopChar) =>
          !parser.TextMode ? parser.ReadSpace().Bind(skip => Ok(new Space(skip))) : @"\mskip is not allowed in text mode" },
        { @"\raisebox", (parser, accumulate, stopChar) => {
          if (!parser.ReadCharIfAvailable('{')) return "Expected {";
          return parser.ReadSpace().Bind(raise => {
            if (!parser.ReadCharIfAvailable('}')) return "Expected }";
            return parser.ReadArgument().Bind(innerList =>
              Ok(new RaiseBox(raise, innerList)));
          });
        } },
        { @"\operatorname", (parser, accumulate, stopChar) => {
          if (!parser.ReadCharIfAvailable('{')) return "Expected {";
          var operatorname = parser.ReadString();
          if (!parser.ReadCharIfAvailable('}')) return "Expected }";
          return Ok(new LargeOperator(operatorname, null));
        } },
        // Bra and Ket implementations are derived from Donald Arseneau's braket LaTeX package.
        // See: https://www.ctan.org/pkg/braket
        { @"\Bra", (parser, accumulate, stopChar) =>
          parser.ReadArgument().Bind(innerList =>
            Ok(new Inner(new Boundary("〈"), innerList, new Boundary("|")))) },
        { @"\Ket", (parser, accumulate, stopChar) =>
          parser.ReadArgument().Bind(innerList =>
            Ok(new Inner(new Boundary("|"), innerList, new Boundary("〉")))) },
        #endregion Atom producers
        #region Atom modifiers
        { @"^", (parser, accumulate, stopChar) => {
          var prevAtom = accumulate.Last;
          if (prevAtom == null || prevAtom.Superscript.IsNonEmpty() || !prevAtom.ScriptsAllowed) {
            prevAtom = new Ordinary(string.Empty);
            accumulate.Add(prevAtom);
          }
          // this is a superscript for the previous atom.
          // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
          return parser.ReadArgument(prevAtom.Superscript).Bind(_ => Ok(null));
        } },
        { @"_", (parser, accumulate, stopChar) => {
          var prevAtom = accumulate.Last;
          if (prevAtom == null || prevAtom.Subscript.IsNonEmpty() || !prevAtom.ScriptsAllowed) {
            prevAtom = new Ordinary(string.Empty);
            accumulate.Add(prevAtom);
          }
          // this is a subscript for the previous atom.
          // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
          return parser.ReadArgument(prevAtom.Subscript).Bind(_ => Ok(null));
        } },
        { @"{", (parser, accumulate, stopChar) => {
            if (parser.Environments.PeekOrDefault() is LaTeXParser.TableEnvironment { Name: null }) {
              // \\ or \cr which do not have a corresponding \end
              var oldEnv = parser.Environments.Pop();
              return parser.ReadUntil('}').Bind(sublist => {
                parser.Environments.Push(oldEnv);
                return OkStyled(sublist);
              });
            } else {
              return parser.ReadUntil('}').Bind(OkStyled);
            }
        } },
        { @"}", (parser, accumulate, stopChar) => "Missing opening brace" },
        { @"\limits", (parser, accumulate, stopChar) => {
          if (accumulate.Last is LargeOperator largeOp) {
            largeOp.Limits = true;
            return Ok(null);
          } else return @"\limits can only be applied to an operator";
        } },
        { @"\nolimits", (parser, accumulate, stopChar) => {
          if (accumulate.Last is LargeOperator largeOp) {
            largeOp.Limits = false;
            return Ok(null);
          } else return @"\nolimits can only be applied to an operator";
        } },
        #endregion Atom modifiers
        #region Environment enders
        { @"&", (parser, accumulate, stopChar) => // column separation in tables
          parser.Environments.PeekOrDefault() is LaTeXParser.TableEnvironment
          ? OkStop(accumulate)
          : parser.ReadTable(null, accumulate, false, stopChar).Bind(table => OkStop(new MathList(table))) },
        { @"\over", (parser, accumulate, stopChar) =>
          parser.ReadUntil(stopChar).Bind(denominator =>
            OkStop(new MathList(new Fraction(accumulate, denominator)))) },
        { @"\atop", (parser, accumulate, stopChar) =>
          parser.ReadUntil(stopChar).Bind(denominator =>
            OkStop(new MathList(new Fraction(accumulate, denominator, false)))) },
        { @"\choose", (parser, accumulate, stopChar) =>
          parser.ReadUntil(stopChar).Bind(denominator =>
            OkStop(new MathList(new Fraction(accumulate, denominator, false) { LeftDelimiter = new Boundary("("), RightDelimiter = new Boundary(")") }))) },
        { @"\brack", (parser, accumulate, stopChar) =>
          parser.ReadUntil(stopChar).Bind(denominator =>
            OkStop(new MathList(new Fraction(accumulate, denominator, false) { LeftDelimiter = new Boundary("["), RightDelimiter = new Boundary("]") }))) },
        { @"\brace", (parser, accumulate, stopChar) =>
          parser.ReadUntil(stopChar).Bind(denominator =>
            OkStop(new MathList(new Fraction(accumulate, denominator, false) { LeftDelimiter = new Boundary("{"), RightDelimiter = new Boundary("}") }))) },
        { @"\atopwithdelims", (parser, accumulate, stopChar) =>
          parser.ReadDelimiter(@"atopwithdelims").Bind(left =>
            parser.ReadDelimiter(@"atopwithdelims").Bind(right =>
              parser.ReadUntil(stopChar).Bind(denominator =>
                OkStop(new MathList(new Fraction(accumulate, denominator, false) { LeftDelimiter = left, RightDelimiter = right }))))) },
        { @"\right", (parser, accumulate, stopChar) => {
          while (parser.Environments.PeekOrDefault() is LaTeXParser.TableEnvironment table)
            if (table.Name is null) {
              table.Ended = true;
              parser.Environments.Pop(); // Get out of \\ or \cr before looking for \right
            } else {
              return $"Missing \\end{{{table.Name}}}";
            }
          if (!(parser.Environments.PeekOrDefault() is LaTeXParser.InnerEnvironment inner)) {
            return "Missing \\left";
          }
          var (boundary, error) = parser.ReadDelimiter("right");
          if (error != null) return error;
          inner.RightBoundary = boundary;
          return OkStop(accumulate);
        } },
        { @"\\", @"\cr", (parser, accumulate, stopChar) => {
          if (!(parser.Environments.PeekOrDefault() is LaTeXParser.TableEnvironment environment)) {
            return parser.ReadTable(null, accumulate, true, stopChar).Bind(table => OkStop(new MathList(table)));
          } else {
            // stop the current list and increment the row count
            environment.NRows++;
            return OkStop(accumulate);
          }
        } },
        { @"\end", (parser, accumulate, stopChar) => {
          if (!(parser.Environments.PeekOrDefault() is LaTeXParser.TableEnvironment endEnvironment)) {
            return @"Missing \begin";
          }
          return parser.ReadEnvironment().Bind(env => {
            if (env != endEnvironment.Name) {
              return $"Begin environment name {endEnvironment.Name} does not match end environment name {env}";
            }
            endEnvironment.Ended = true;
            return OkStop(accumulate);
          });
        } },
        #endregion Environment enders
      };
    public static MathAtom Times => new BinaryOperator("×");
    public static MathAtom Divide => new BinaryOperator("÷");
    public static MathAtom Placeholder => new Placeholder("\u25A1");
    public static MathList PlaceholderList => new MathList { Placeholder };

    public static AliasBiDictionary<string, FontStyle> FontStyles { get; } =
      new AliasBiDictionary<string, FontStyle>((command, fontStyle) => {
        Commands.Add(@"\" + command, (parser, accumulate, stopChar) => {
          var oldSpacesAllowed = parser.TextMode;
          var oldFontStyle = parser.CurrentFontStyle;
          parser.TextMode = command == "text";
          parser.CurrentFontStyle = fontStyle;
          var readsToEnd =
            !command.AsSpan().StartsWithInvariant("math")
            && !command.AsSpan().StartsWithInvariant("text");
          return (readsToEnd ? parser.ReadUntil(stopChar, accumulate) : parser.ReadArgument()).Bind(r => {
            parser.CurrentFontStyle = oldFontStyle;
            parser.TextMode = oldSpacesAllowed;
            if (readsToEnd)
              return OkStop(accumulate);
            else return OkStyled(r);
          });
        });
      }) {
        { "mathnormal", FontStyle.Default },
        { "mathrm", "rm", "text", FontStyle.Roman },
        { "mathbf", "bf", FontStyle.Bold },
        { "mathcal", "cal", FontStyle.Caligraphic },
        { "mathtt", "tt", FontStyle.Typewriter },
        { "mathit", "it", "mit", FontStyle.Italic },
        { "mathsf", "sf", FontStyle.SansSerif },
        { "mathfrak", "frak", FontStyle.Fraktur },
        { "mathbb", "bb", FontStyle.Blackboard },
        { "mathbfit", "bm", FontStyle.BoldItalic },
      };

    public static Color? ParseColor(string? hexOrName) {
      if (hexOrName == null) return null;
      if (hexOrName.StartsWith("#", StringComparison.Ordinal)) {
        var hex = hexOrName.Substring(1);
        return
          (hex.Length, int.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var i)) switch
          {
            (8, true) => Color.FromArgb(i),
            (6, true) => Color.FromArgb(unchecked((int)0xff000000) + i),
            _ => null
          };
      }
#pragma warning disable CA1308 // Normalize strings to uppercase
      if (PredefinedColors.FirstToSecond.TryGetValue(hexOrName.ToLowerInvariant(), out var predefined))
        return predefined;
#pragma warning restore CA1308 // Normalize strings to uppercase
      return null;
    }
    public static StringBuilder ColorToString(Color color, StringBuilder sb) {
      if (PredefinedColors.SecondToFirst.TryGetValue(color, out var outString)) {
        return sb.Append(outString);
      } else {
        sb.Append('#');
        if (color.A != 255)
          sb.Append(color.A.ToStringInvariant("X2"));
        return sb.Append(color.R.ToStringInvariant("X2"))
                 .Append(color.G.ToStringInvariant("X2"))
                 .Append(color.B.ToStringInvariant("X2"));
      }
    }
    //https://en.wikibooks.org/wiki/LaTeX/Colors#Predefined_colors
    public static AliasBiDictionary<string, Color> PredefinedColors { get; } =
      new AliasBiDictionary<string, Color> {
        { "black", Color.FromArgb(0, 0, 0) },
        { "blue", Color.FromArgb(0, 0, 255) },
        { "brown", Color.FromArgb(150, 75, 0) },
        { "cyan", Color.FromArgb(0, 255, 255) },
        { "darkgray", Color.FromArgb(128, 128, 128) },
        { "gray", Color.FromArgb(169, 169, 169) },
        { "green", Color.FromArgb(0, 128, 0) },
        { "lightgray", Color.FromArgb(211, 211, 211) },
        { "lime", Color.FromArgb(0, 255, 0) },
        { "magenta", Color.FromArgb(255, 0, 255) },
        { "olive", Color.FromArgb(128, 128, 0) },
        { "orange", Color.FromArgb(255, 128, 0) },
        { "pink", Color.FromArgb(255, 192, 203) },
        { "purple", Color.FromArgb(128, 0, 128) },
        { "red", Color.FromArgb(255, 0,0) },
        { "teal", Color.FromArgb(0, 128, 128) },
        { "violet", Color.FromArgb(128, 0, 255) },
        { "white", Color.FromArgb(255, 255, 255) },
        { "yellow", Color.FromArgb(255, 255, 0) }
      };

    public static MathAtom? AtomForCommand(string symbolName) =>
      CommandSymbols.FirstToSecond.TryGetValue(
        symbolName ?? throw new ArgumentNullException(nameof(symbolName)),
        out var symbol) ? symbol.Clone(false) : null;

    public static string? CommandForAtom(MathAtom atom) {
      var atomWithoutScripts = atom.Clone(false);
      atomWithoutScripts.Superscript.Clear();
      atomWithoutScripts.Subscript.Clear();
      if (atomWithoutScripts is IMathListContainer container)
        foreach (var list in container.InnerLists)
          list.Clear();
      return CommandSymbols.SecondToFirst.TryGetValue(atomWithoutScripts, out var name) ? name : null;
    }

    public static AliasBiDictionary<string, MathAtom> CommandSymbols { get; } =
      new AliasBiDictionary<string, MathAtom>((command, atom) =>
        Commands.Add(command, (parser, accumulate, stopChar) =>
          atom is Accent accent
          ? parser.ReadArgument().Bind(accentee => Ok(new Accent(accent.Nucleus, accentee)))
          : Ok(atom.Clone(false)))) {
        // Custom additions
        { @"\diameter", "\u2300", new Ordinary("\u2300") },
        { @"\npreccurlyeq", "⋠", new Relation("⋠") },
        { @"\nsucccurlyeq", "⋡", new Relation("⋡") },
        { @"\iint", "∬", new LargeOperator("∬", false) },
        { @"\iiint", "∭", new LargeOperator("∭", false) },
        { @"\iiiint", "⨌", new LargeOperator("⨌", false) },
        { @"\oiint", "∯", new LargeOperator("∯", false) },
        { @"\oiiint", "∰", new LargeOperator("∰", false) },
        { @"\intclockwise", "∱", new LargeOperator("∱", false) },
        { @"\awint", "⨑", new LargeOperator("⨑", false) },
        { @"\varointclockwise", "∲", new LargeOperator("∲", false) },
        { @"\ointctrclockwise", "∳", new LargeOperator("∳", false) },
        { @"\bigbot", "⟘", new LargeOperator("⟘", null) },
        { @"\bigtop", "⟙", new LargeOperator("⟙", null) },
        { @"\bigcupdot", "⨃", new LargeOperator("⨃", null) },
        { @"\bigsqcap", "⨅", new LargeOperator("⨅", null) },
        { @"\bigtimes", "⨉", new LargeOperator("⨉", null) },
        { @"\arsinh", new LargeOperator("arsinh", false, true) },
        { @"\arcosh", new LargeOperator("arcosh", false, true) },
        { @"\artanh", new LargeOperator("artanh", false, true) },
        { @"\arccot", new LargeOperator("arccot", false, true) },
        { @"\arcoth", new LargeOperator("arcoth", false, true) },
        { @"\arcsec", new LargeOperator("arcsec", false, true) },
        { @"\sech", new LargeOperator("sech", false, true) },
        { @"\arsech", new LargeOperator("arsech", false, true) },
        { @"\arccsc", new LargeOperator("arccsc", false, true) },
        { @"\csch", new LargeOperator("csch", false, true) },
        { @"\arcsch", new LargeOperator("arcsch", false, true) },
        // Use escape sequence for combining characters
        { @"\overbar", "\u0305", new Accent("\u0305") },
        { @"\ovhook", "\u0309", new Accent("\u0309") },
        { @"\ocirc", "\u030A", new Accent("\u030A") },
        { @"\leftharpoonaccent", "\u20D0", new Accent("\u20D0") },
        { @"\rightharpoonaccent", "\u20D1", new Accent("\u20D1") },
        { @"\vertoverlay", "\u20D2", new Accent("\u20D2") },
        { @"\dddot", "\u20DB", new Accent("\u20DB") },
        { @"\ddddot", "\u20DC", new Accent("\u20DC") },
        { @"\widebridgeabove", "\u20E9", new Accent("\u20E9") },
        { @"\asteraccent", "\u20F0", new Accent("\u20F0") },
        { @"\threeunderdot", "\u20E8", new Accent("\u20E8") },
        { @"\TeX", new Inner(Boundary.Empty, new MathList(
            new Variable("T") { FontStyle = FontStyle.Roman },
            new Space(-1/6f * Structures.Space.EmWidth) { FontStyle = FontStyle.Roman },
            new RaiseBox(-1/2f * Structures.Space.ExHeight,
              new MathList(new Variable("E") { FontStyle = FontStyle.Roman })
            ) { FontStyle = FontStyle.Roman },
            new Space(-1/8f * Structures.Space.EmWidth) { FontStyle = FontStyle.Roman },
            new Variable("X") { FontStyle = FontStyle.Roman }
          ), Boundary.Empty) },

        // Delimiters outside \left or \right
        { @"(", new Open("(") },
        { @")", new Close(")") },
        { @"[", new Open("[") },
        { @"]", new Close("]") },
        { @"\lceil", "⌈", new Open("⌈") },
        { @"\rceil", "⌉", new Close("⌉") },
        { @"\lfloor", "⌊", new Open("⌊") },
        { @"\rfloor", "⌋", new Close("⌋") },
        { @"\langle", "〈", new Open("〈") },
        { @"\rangle", "〉", new Close("〉") },
        { @"\lgroup", "⟮", new Open("⟮") },
        { @"\rgroup", "⟯", new Close("⟯") },
        { @"\ulcorner", "⌜", new Open("⌜") },
        { @"\urcorner", "⌝", new Close("⌝") },
        { @"\llcorner", "⌞", new Open("⌞") },
        { @"\lrcorner", "⌟", new Close("⌟") },

        // Standard TeX
        { Enumerable.Range('0', 10).Select(c => ((char)c).ToStringInvariant()),
          n => new Number(n) },
        { Enumerable.Range('A', 26).Concat(Enumerable.Range('a', 26)).Select(c => ((char)c).ToStringInvariant()),
          v => new Variable(v) },
        { @"\ ", new Ordinary(" ") },
        { @"\,", "\u2006", "\u2009", new Space(Structures.Space.ShortSpace) }, // six-per-em space
        { @"\:", @"\>", "\u205F", new Space(Structures.Space.MediumSpace) },
        { @"\;", new Space(Structures.Space.LongSpace) },
        { @"\!", new Space(-Structures.Space.ShortSpace) },
        { @"\enspace", "\u2000", "\u2002", new Space(Structures.Space.EmWidth / 2) },
        { @"\quad", "\u2001", "\u2003", new Space(Structures.Space.EmWidth) },
        { @"\qquad", new Space(Structures.Space.EmWidth * 2) },
        { @"\displaystyle", new Style(LineStyle.Display) },
        { @"\textstyle", new Style(LineStyle.Text) },
        { @"\scriptstyle", new Style(LineStyle.Script) },
        { @"\scriptscriptstyle", new Style(LineStyle.ScriptScript) },

        // The gensymb package for LaTeX2ε: http://mirrors.ctan.org/macros/latex/contrib/was/gensymb.pdf
        { @"\degree", "°", new Ordinary("°") },
        { @"\celsius", "℃", new Ordinary("℃") },
        { @"\perthousand", "‰", new Ordinary("‰") },
        { @"\ohm", "Ω", new Ordinary("Ω") },
        { @"\micro", "µ", new Ordinary("µ") },

        // ASCII characters without special properties (Not a special Command or CommandSymbol)
        // AMSMath: Although / is (semantically speaking) of class 2: Binary Operator,
        // we write k/2 with no space around the slash rather than k / 2.
        // And compare p|q -> p|q (no space) with p\mid q -> p | q (class-3 spacing).
        { @"/", new Ordinary("/") },
        { @"@", new Ordinary("@") },
        { @"`", new Ordinary("`") },
        { @"|", new Ordinary("|") },

        // LaTeX Symbol List: https://rpi.edu/dept/arc/training/latex/LaTeX_symbols.pdf
        // (Included in the same folder as this file)
        // Shorter list: https://www.andy-roberts.net/res/writing/latex/symbols.pdf

        // Command <-> Unicode: https://www.johndcook.com/unicode_latex.html
        // Unicode char lookup: https://unicode-table.com/en/search/
        // Reference LaTeX output for glyph: https://www.codecogs.com/latex/eqneditor.php
        // Look at what glyphs are in a font: https://github.com/fontforge/fontforge

        // Following tables are from the LaTeX Symbol List
        // Table 1: Escapable “Special” Characters
        { @"\$", new Ordinary("$") },
        { @"\%", new Ordinary("%") },
        { @"\_", new Ordinary("_") },
        { @"\}", @"\rbrace", new Close("}") },
        { @"\&", new Ordinary("&") },
        { @"\#", new Ordinary("#") },
        { @"\{", @"\lbrace", new Open("{") },

        // Table 2: LaTeX2ε Commands Deﬁned to Work in Both Math and Text Mode
        // \$ is defined in Table 1
        { @"\P", "¶", new Ordinary("¶") },
        { @"\S", "§", new Ordinary("§") },
        // \_ is defined in Table 1
        { @"\copyright", "©", new Ordinary("©") },
        { @"\dag", new Ordinary("†") },
        { @"\ddag", new Ordinary("‡") },
        { @"\dots", new Ordinary("…") },
        { @"\pounds", "£", new Ordinary("£") },
        // \{ is defined in Table 1
        // \} is defined in Table 1

        // Table 3: Non-ASCII Letters (Excluding Accented Letters)
        { @"\aa", "å", new Ordinary("å") },
        { @"\AA", @"\angstrom", "Å", new Ordinary("Å") },
        { @"\AE", "Æ", new Ordinary("Æ") },
        { @"\ae", "æ", new Ordinary("æ") },
        { @"\DH", "Ð", new Ordinary("Ð") },
        { @"\dh", "ð", new Ordinary("ð") },
        { @"\DJ", "Đ", new Ordinary("Đ") },
        //{ @"\dj", "đ", new Ordinary("đ") }, // Glyph not in Latin Modern Math
        { @"\L", "Ł", new Ordinary("Ł") },
        { @"\l", "ł", new Ordinary("ł") },
        { @"\NG", "Ŋ", new Ordinary("Ŋ") },
        { @"\ng", "ŋ", new Ordinary("ŋ") },
        { @"\o", "ø", new Ordinary("ø") },
        { @"\O", "Ø", new Ordinary("Ø") },
        { @"\OE", "Œ", new Ordinary("Œ") },
        { @"\oe", "œ", new Ordinary("œ") },
        { @"\ss", "ß", new Ordinary("ß") },
        { @"\SS", new Ordinary("SS") },
        { @"\TH", "Þ", new Ordinary("Þ") },
        { @"\th", "þ", new Ordinary("þ") },

        // Table 4: Greek Letters
        { @"\alpha", "α", new Variable("α") },
        { @"\beta", "β", new Variable("β") },
        { @"\gamma", "γ", new Variable("γ") },
        { @"\delta", "δ", new Variable("δ") },
        { @"\epsilon", "ϵ", new Variable("ϵ") },
        { @"\varepsilon", "ε", new Variable("ε") },
        { @"\zeta", "ζ", new Variable("ζ") },
        { @"\eta", "η", new Variable("η") },
        { @"\theta", "θ", new Variable("θ") },
        { @"\vartheta", "ϑ", new Variable("ϑ") },
        { @"\iota", "ι", new Variable("ι") },
        { @"\kappa", "κ", new Variable("κ") },
        { @"\lambda", "λ", new Variable("λ") },
        { @"\mu", "μ", new Variable("μ") },
        { @"\nu", "ν", new Variable("ν") },
        { @"\xi", "ξ", new Variable("ξ") },
        { @"\omicron", "ο", new Variable("ο") },
        { @"\pi", "π", new Variable("π") },
        { @"\varpi", "ϖ", new Variable("ϖ") },
        { @"\rho", "ρ", new Variable("ρ") },
        { @"\varrho", "ϱ", new Variable("ϱ") },
        { @"\sigma", "σ", new Variable("σ") },
        { @"\varsigma", "ς", new Variable("ς") },
        { @"\tau", "τ", new Variable("τ") },
        { @"\upsilon", "υ", new Variable("υ") },
        { @"\phi", "ϕ", new Variable("ϕ") }, // Don't be fooled by Visual Studio!
        { @"\varphi", "φ", new Variable("φ") }, // The Visual Studio font is wrong!
        { @"\chi", "χ", new Variable("χ") },
        { @"\psi", "ψ", new Variable("ψ") },
        { @"\omega", "ω", new Variable("ω") },

        { @"\Gamma", "Γ", new Variable("Γ") },
        { @"\Delta", "Δ", new Variable("Δ") },
        { @"\Theta", "Θ", new Variable("Θ") },
        { @"\Lambda", "Λ", new Variable("Λ") },
        { @"\Xi", "Ξ", new Variable("Ξ") },
        { @"\Pi", "Π", new Variable("Π") },
        { @"\Sigma", "Σ", new Variable("Σ") },
        { @"\Upsilon", "Υ", new Variable("Υ") },
        { @"\Phi", "Φ", new Variable("Φ") },
        { @"\Psi", "Ψ", new Variable("Ψ") },
        { @"\Omega", "Ω", new Variable("Ω") },
        // (The remaining Greek majuscules can be produced with ordinary Latin letters.
        // The symbol “M”, for instance, is used for both an uppercase “m” and an uppercase “µ”.

        // Table 5: Punctuation Marks Not Found in OT
        { @"\guillemotleft", "«", new Punctuation("«") },
        { @"\guillemotright", "»", new Punctuation("»") },
        { @"\guilsinglleft", "‹", new Punctuation("‹") },
        { @"\guilsinglright", "›", new Punctuation("›") },
        { @"\quotedblbase", "„", new Punctuation("„") },
        { @"\quotesinglbase", "‚", new Punctuation("‚") }, // This is not the comma
        { "\"", @"\textquotedbl", new Punctuation("\"") },

        // Table 6: Predeﬁned LaTeX2ε Text-Mode Commands
        // [Skip text mode commands]

        // Table 7: Binary Operation Symbols
        { @"\pm", "±", new BinaryOperator("±") },
        { @"\mp", "∓", new BinaryOperator("∓") },
        { @"\times", Times },
        { @"\div", Divide },
        { @"\ast", "∗", new BinaryOperator("∗") },
        { @"*", new BinaryOperator("*") }, // ADDED: For consistency with \ast
        { @"\star", "⋆", new BinaryOperator("⋆") },
        { @"\circ", "◦", new BinaryOperator("◦") },
        { @"\bullet", "•", new BinaryOperator("•") },
        { @"\cdot", "·", new BinaryOperator("·") },
        { @"+", new BinaryOperator("+") },
        { @"\cap", "∩", new BinaryOperator("∩") },
        { @"\cup", "∪", new BinaryOperator("∪") },
        { @"\uplus", "⊎", new BinaryOperator("⊎") },
        { @"\sqcap", "⊓", new BinaryOperator("⊓") },
        { @"\sqcup", "⊔", new BinaryOperator("⊔") },
        { @"\vee", @"\lor", "∨", new BinaryOperator("∨") },
        { @"\wedge", @"\land", "∧", new BinaryOperator("∧") },
        { @"\setminus", "∖", new BinaryOperator("∖") },
        { @"\wr", "≀", new BinaryOperator("≀") },
        { @"-", "−", new BinaryOperator("−") }, // Convert hyphen to math minus
        { @"\diamond", "⋄", new BinaryOperator("⋄") },
        { @"\bigtriangleup", "△", new BinaryOperator("△") },
        { @"\bigtriangledown", "▽", new BinaryOperator("▽") },
        { @"\triangleleft", "◁", new BinaryOperator("◁") }, // Latin Modern Math doesn't have ◃
        { @"\triangleright", "▷", new BinaryOperator("▷") }, // Latin Modern Math doesn't have ▹
        { @"\lhd", "⊲", new BinaryOperator("⊲") },
        { @"\rhd", "⊳", new BinaryOperator("⊳") },
        { @"\unlhd", "⊴", new BinaryOperator("⊴") },
        { @"\unrhd", "⊵", new BinaryOperator("⊵") },
        { @"\oplus", "⊕", new BinaryOperator("⊕") },
        { @"\ominus", "⊖", new BinaryOperator("⊖") },
        { @"\otimes", "⊗", new BinaryOperator("⊗") },
        { @"\oslash", "⊘", new BinaryOperator("⊘") },
        { @"\odot", "⊙", new BinaryOperator("⊙") },
        { @"\bigcirc", "◯", new BinaryOperator("◯") },
        { @"\dagger", "†", new BinaryOperator("†") },
        { @"\ddagger", "‡", new BinaryOperator("‡") },
        { @"\amalg", "⨿", new BinaryOperator("⨿") },

        // Table 8: Relation Symbols
        { @"\leq", @"\le", "≤", new Relation("≤") },
        { @"\geq", @"\ge", "≥", new Relation("≥") },
        { @"\equiv", "≡", new Relation("≡") },
        { @"\models", "⊧", new Relation("⊧") },
        { @"\prec", "≺", new Relation("≺") },
        { @"\succ", "≻", new Relation("≻") },
        { @"\sim", "∼", new Relation("∼") },
        { @"\perp", "⟂", new Relation("⟂") },
        { @"\preceq", "⪯", new Relation("⪯") },
        { @"\succeq", "⪰", new Relation("⪰") },
        { @"\simeq", "≃", new Relation("≃") },
        { @"\mid", "∣", new Relation("∣") },
        { @"\ll", "≪", new Relation("≪") },
        { @"\gg", "≫", new Relation("≫") },
        { @"\asymp", "≍", new Relation("≍") },
        { @"\parallel", "∥", new Relation("∥") },
        { @"\subset", "⊂", new Relation("⊂") },
        { @"\supset", "⊃", new Relation("⊃") },
        { @"\approx", "≈", new Relation("≈") },
        { @"\bowtie", "⋈", new Relation("⋈") },
        { @"\subseteq", "⊆", new Relation("⊆") },
        { @"\supseteq", "⊇", new Relation("⊇") },
        { @"\cong", "≅", new Relation("≅") },
        // Latin Modern Math doesn't have ⨝ so we copy the one from \bowtie
        { @"\Join", "⨝", new Relation("⋈") }, // Capital J is intentional
        { @"\sqsubset", "⊏", new Relation("⊏") },
        { @"\sqsupset", "⊐", new Relation("⊐") },
        { @"\neq", @"\ne", "≠", new Relation("≠") },
        { @"\smile", "⌣", new Relation("⌣") },
        { @"\sqsubseteq", "⊑", new Relation("⊑") },
        { @"\sqsupseteq", "⊒", new Relation("⊒") },
        { @"\doteq", "≐", new Relation("≐") },
        { @"\frown", "⌢", new Relation("⌢") },
        { @"\in", "∈", new Relation("∈") },
        { @"\ni", "∋", new Relation("∋") },
        { @"\notin", "∉", new Relation("∉") },
        { @"\propto", "∝", new Relation("∝") },
        { @"=", new Relation("=") },
        { @"\vdash", "⊢", new Relation("⊢") },
        { @"\dashv", "⊣", new Relation("⊣") },
        { @"<", new Relation("<") },
        { @">", new Relation(">") },
        { @":", "∶", new Relation("∶") }, // Colon is a ratio. Regular colon is \colon
        
        // Table 9: Punctuation Symbols
        { @",", new Punctuation(",") },
        { @";", new Punctuation(";") },
        { @"\colon", new Punctuation(":") }, // \colon is different from : which is a relation
        { @"\ldotp", new Punctuation(".") }, // Aka the full stop or decimal dot
        { @"\cdotp", new Punctuation("·") },
        { @"!", new Punctuation("!") }, // ADDED: According to https://latex.wikia.org/wiki/List_of_LaTeX_symbols#Class_6_.28Pun.29_symbols:_postfix_.2F_punctuation
        { @"?", new Punctuation("?") }, // ADDED: According to https://latex.wikia.org/wiki/List_of_LaTeX_symbols#Class_6_.28Pun.29_symbols:_postfix_.2F_punctuation
        
        // Table 10: Arrow Symbols 
        { @"\leftarrow", @"\gets", "←", new Relation("←") },
        { @"\longleftarrow", "⟵", new Relation("⟵") },
        { @"\uparrow", "↑", new Relation("↑") },
        { @"\Leftarrow", "⇐", new Relation("⇐") },
        { @"\Longleftarrow", "⟸", new Relation("⟸") },
        { @"\Uparrow", "⇑", new Relation("⇑") },
        { @"\rightarrow", @"\to", "→", new Relation("→") },
        { @"\longrightarrow", "⟶", new Relation("⟶") },
        { @"\downarrow", "↓", new Relation("↓") },
        { @"\Rightarrow", "⇒", new Relation("⇒") },
        { @"\Longrightarrow", "⟹", new Relation("⟹") },
        { @"\Downarrow", "⇓", new Relation("⇓") },
        { @"\leftrightarrow", "↔", new Relation("↔") },
        { @"\Leftrightarrow", "⇔", new Relation("⇔") },
        { @"\updownarrow", "↕", new Relation("↕") },
        { @"\longleftrightarrow", "⟷", new Relation("⟷") },
        { @"\Longleftrightarrow", @"\iff", "⟺", new Relation("⟺") },
        { @"\Updownarrow", "⇕", new Relation("⇕") },
        { @"\mapsto", "↦", new Relation("↦") },
        { @"\longmapsto", "⟼", new Relation("⟼") },
        { @"\nearrow", "↗", new Relation("↗") },
        { @"\hookleftarrow", "↩", new Relation("↩") },
        { @"\hookrightarrow", "↪", new Relation("↪") },
        { @"\searrow", "↘", new Relation("↘") },
        { @"\leftharpoonup", "↼", new Relation("↼") },
        { @"\rightharpoonup", "⇀", new Relation("⇀") },
        { @"\swarrow", "↙", new Relation("↙") },
        { @"\leftharpoondown", "↽", new Relation("↽") },
        { @"\rightharpoondown", "⇁", new Relation("⇁") },
        { @"\nwarrow", "↖", new Relation("↖") },
        { @"\rightleftharpoons", "⇌", new Relation("⇌") },
        { @"\leadsto", new Relation("⇝") }, // same as \rightsquigarrow

        // Table 11: Miscellaneous Symbols
        { @"\ldots", "…", new Punctuation("…") }, // CHANGED: Not Ordinary for consistency with \cdots, \vdots and \ddots
        { @"\aleph", "ℵ", new Ordinary("ℵ") },
        { @"\hbar", "ℏ", new Ordinary("ℏ") },
        { @"\imath", "𝚤", new Ordinary("𝚤") },
        { @"\jmath", "𝚥", new Ordinary("𝚥") },
        { @"\ell", "ℓ", new Ordinary("ℓ") },
        { @"\wp", "℘", new Ordinary("℘") },
        { @"\Re", "ℜ", new Ordinary("ℜ") },
        { @"\Im", "ℑ", new Ordinary("ℑ") },
        { @"\mho", "℧", new Ordinary("℧") },
        { @"\cdots", @"\dotsb", "⋯", new Ordinary("⋯") }, // CHANGED: Not Ordinary according to https://latex.wikia.org/wiki/List_of_LaTeX_symbols#Class_6_.28Pun.29_symbols:_postfix_.2F_punctuation
        // \prime is removed because Unicode has no matching character
        { @"\emptyset", "∅", new Ordinary("∅") },
        { @"\nabla", "∇", new Ordinary("∇") },
        { @"\surd", "√", new Ordinary("√") },
        { @"\top", "⊤", new Ordinary("⊤") },
        { @"\bot", "⊥", new Ordinary("⊥") },
        { @"\|", @"\Vert", "‖", new Ordinary("‖") },
        { @"\angle", "∠", new Ordinary("∠") },
        { @".", new Number(".") }, // CHANGED: Not punctuation for easy parsing of numbers
        { @"\vdots", "⋮", new Punctuation("⋮") }, // CHANGED: Not Ordinary according to https://latex.wikia.org/wiki/List_of_LaTeX_symbols#Class_6_.28Pun.29_symbols:_postfix_.2F_punctuation
        { @"\forall", "∀", new Ordinary("∀") },
        { @"\exists", "∃", new Ordinary("∃") },
        { @"\neg", "lnot", new Ordinary("¬") },
        { @"\flat", "♭", new Ordinary("♭") },
        { @"\natural", "♮", new Ordinary("♮") },
        { @"\sharp", "♯", new Ordinary("♯") },
        { @"\backslash", new Ordinary("\\") },
        { @"\partial", "𝜕", new Ordinary("𝜕") },
        { @"\vert", new Ordinary("|") },
        { @"\ddots", "⋱", new Punctuation("⋱") }, // CHANGED: Not Ordinary according to https://latex.wikia.org/wiki/List_of_LaTeX_symbols#Class_6_.28Pun.29_symbols:_postfix_.2F_punctuation
        { @"\infty", "∞", new Ordinary("∞") },
        { @"\Box", "□", new Ordinary("□") }, // same as \square
        { @"\Diamond", new Ordinary("◊") }, // same as \lozenge
        { @"\triangle", new Ordinary("△") }, // same as \bigtriangleup
        { @"\clubsuit", "♣", new Ordinary("♣") },
        { @"\diamondsuit", "♢", new Ordinary("♢") },
        { @"\heartsuit", "♡", new Ordinary("♡") },
        { @"\spadesuit", "♠", new Ordinary("♠") },

        // Table 12: Variable-sized Symbols 
        { @"\sum", "∑", new LargeOperator("∑", null) },
        { @"\prod", "∏", new LargeOperator("∏", null) },
        { @"\coprod", "∐", new LargeOperator("∐", null) },
        { @"\int", "∫", new LargeOperator("∫", false) },
        { @"\oint", "∮", new LargeOperator("∮", false) },
        { @"\bigcap", "⋂", new LargeOperator("⋂", null) },
        { @"\bigcup", "⋃", new LargeOperator("⋃", null) },
        { @"\bigsqcup", "⨆", new LargeOperator("⨆", null) },
        { @"\bigvee", "⋁", new LargeOperator("⋁", null) },
        { @"\bigwedge", "⋀", new LargeOperator("⋀", null) },
        { @"\bigodot", "⨀", new LargeOperator("⨀", null) },
        { @"\bigoplus", "⨁", new LargeOperator("⨁", null) },
        { @"\bigotimes", "⨂", new LargeOperator("⨂", null) },
        { @"\biguplus", "⨄", new LargeOperator("⨄", null) },

        // Table 13: Log-like Symbols 
        { @"\arccos", new LargeOperator("arccos", false, true) },
        { @"\arcsin", new LargeOperator("arcsin", false, true) },
        { @"\arctan", new LargeOperator("arctan", false, true) },
        { @"\arg", new LargeOperator("arg", false, true) },
        { @"\cos", new LargeOperator("cos", false, true) },
        { @"\cosh", new LargeOperator("cosh", false, true) },
        { @"\cot", new LargeOperator("cot", false, true) },
        { @"\coth", new LargeOperator("coth", false, true) },
        { @"\csc", new LargeOperator("csc", false, true) },
        { @"\deg", new LargeOperator("deg", false, true) },
        { @"\det", new LargeOperator("det", null) },
        { @"\dim", new LargeOperator("dim", false, true) },
        { @"\exp", new LargeOperator("exp", false, true) },
        { @"\gcd", new LargeOperator("gcd", null) },
        { @"\hom", new LargeOperator("hom", false, true) },
        { @"\inf", new LargeOperator("inf", null) },
        { @"\ker", new LargeOperator("ker", false, true) },
        { @"\lg", new LargeOperator("lg", false, true) },
        { @"\lim", new LargeOperator("lim", null) },
        { @"\liminf", new LargeOperator("lim inf", null) },
        { @"\limsup", new LargeOperator("lim sup", null) },
        { @"\ln", new LargeOperator("ln", false, true) },
        { @"\log", new LargeOperator("log", false, true) },
        { @"\max", new LargeOperator("max", null) },
        { @"\min", new LargeOperator("min", null) },
        { @"\Pr", new LargeOperator("Pr", null) },
        { @"\sec", new LargeOperator("sec", false, true) },
        { @"\sin", new LargeOperator("sin", false, true) },
        { @"\sinh", new LargeOperator("sinh", false, true) },
        { @"\sup", new LargeOperator("sup", null) },
        { @"\tan", new LargeOperator("tan", false, true) },
        { @"\tanh", new LargeOperator("tanh", false, true) },

        // Table 14: Delimiters
        // Table 15: Large Delimiters
        // [See BoundaryDelimiters dictionary above]

        // Table 16: Math-Mode Accents
        // Use escape sequence for combining characters
        { @"\hat", "\u0302", new Accent("\u0302") },  // In our implementation hat and widehat behave the same.
        { @"\acute", "\u0301", new Accent("\u0301") },
        { @"\bar", "\u0304", new Accent("\u0304") },
        { @"\dot", "\u0307", new Accent("\u0307") },
        { @"\breve", "\u0306", new Accent("\u0306") },
        { @"\check", "\u030C", new Accent("\u030C") },
        { @"\grave", "\u0300", new Accent("\u0300") },
        { @"\vec", "\u20D7", new Accent("\u20D7") },
        { @"\ddot", "\u0308", new Accent("\u0308") },
        { @"\tilde", "\u0303", new Accent("\u0303") }, // In our implementation tilde and widetilde behave the same.

        // Table 17: Some Other Constructions
        { @"\widehat", new Accent("\u0302") },
        { @"\widetilde", new Accent("\u0303") },
        // TODO: implement \overleftarrow, \overrightarrow, \overbrace, \underbrace
        // \overleftarrow{}
        // \overrightarrow{}
        // \overline{}
        // \underline{}
        // \overbrace{}
        // \underbrace{}
        // \sqrt{}
        // \sqrt[]{}
        { @"'", "′", new Ordinary("′") },
        { @"''", "″", new Ordinary("″") }, // ADDED: Custom addition
        { @"'''", "‴", new Ordinary("‴") }, // ADDED: Custom addition
        { @"''''", "⁗", new Ordinary("⁗") }, // ADDED: Custom addition
        // \frac{}{}

        // Table 18: textcomp Symbols
        // [Skip text mode commands]

        // Table 19: AMS Delimiters
        // [See BoundaryDelimiters dictionary above]

        // Table 20: AMS Arrows
        //{ @"\dashrightarrow", "⇢", new Relation("⇢") }, // Glyph not in Latin Modern Math
        //{ @"\dashleftarrow", "⇠", new Relation("⇠") }, // Glyph not in Latin Modern Math
        { @"\leftleftarrows", "⇇", new Relation("⇇") },
        { @"\leftrightarrows", "⇆", new Relation("⇆") },
        { @"\Lleftarrow", "⇚", new Relation("⇚") },
        { @"\twoheadleftarrow", "↞", new Relation("↞") },
        { @"\leftarrowtail", "↢", new Relation("↢") },
        { @"\looparrowleft", "↫", new Relation("↫") },
        { @"\leftrightharpoons", "⇋", new Relation("⇋") },
        { @"\curvearrowleft", "↶", new Relation("↶") },
        { @"\circlearrowleft", "↺", new Relation("↺") },
        { @"\Lsh", "↰", new Relation("↰") },
        { @"\upuparrows", "⇈", new Relation("⇈") },
        { @"\upharpoonleft", "↿", new Relation("↿") },
        { @"\downharpoonleft", "⇃", new Relation("⇃") },
        { @"\multimap", "⊸", new Relation("⊸") },
        { @"\leftrightsquigarrow", "↭", new Relation("↭") },
        { @"\rightrightarrows", "⇉", new Relation("⇉") },
        { @"\rightleftarrows", "⇄", new Relation("⇄") },
        // Duplicate entry in LaTeX Symbol list: \rightrightarrows
        // Duplicate entry in LaTeX Symbol list: \rightleftarrows
        { @"\twoheadrightarrow", "↠", new Relation("↠") },
        { @"\rightarrowtail", "↣", new Relation("↣") },
        { @"\looparrowright", "↬", new Relation("↬") },
        // \rightleftharpoons defined in Table 10
        { @"\curvearrowright", "↷", new Relation("↷") },
        { @"\circlearrowright", "↻", new Relation("↻") },
        { @"\Rsh", "↱", new Relation("↱") },
        { @"\downdownarrows", "⇊", new Relation("⇊") },
        { @"\upharpoonright", "↾", new Relation("↾") },
        { @"\downharpoonright", "⇂", new Relation("⇂") },
        { @"\rightsquigarrow", "⇝", new Relation("⇝") },

        // Table 21: AMS Negated Arrows
        { @"\nleftarrow", "↚", new Relation("↚") },
        { @"\nrightarrow", "↛", new Relation("↛") },
        { @"\nLeftarrow", "⇍", new Relation("⇍") },
        { @"\nRightarrow", "⇏", new Relation("⇏") },
        { @"\nleftrightarrow", "↮", new Relation("↮") },
        { @"\nLeftrightarrow", "⇎", new Relation("⇎") },

        // Table 22: AMS Greek 
        // { @"\digamma", "ϝ", new Variable("ϝ") }, // Glyph not in Latin Modern Math
        { @"\varkappa", "ϰ", new Variable("ϰ") },

        // Table 23: AMS Hebrew
        { @"\beth", "ℶ", new Ordinary("ℶ") },
        { @"\daleth", "ℸ", new Ordinary("ℸ") },
        { @"\gimel", "ℷ", new Ordinary("ℷ") },

        // Table 24: AMS Miscellaneous
        // \hbar defined in Table 11
        { @"\hslash", new Ordinary("ℏ") }, // Same as \hbar
        { @"\vartriangle", "▵", new Ordinary("△") }, // ▵ not in Latin Modern Math
        { @"\triangledown", "▿", new Ordinary("▽") }, // ▿ not in Latin Modern Math
        { @"\square", Placeholder },
        { @"\lozenge", "◊", new Ordinary("◊") },
        // { @"\circledS", "Ⓢ", new Ordinary("Ⓢ") }, // Glyph not in Latin Modern Math
        // \angle defined in Table 11
        { @"\measuredangle", "∡", new Ordinary("∡") },
        { @"\nexists", "∄", new Ordinary("∄") },
        // \mho defined in Table 11
        // { @"\Finv", "Ⅎ", new Ordinary("Ⅎ") }, // Glyph not in Latin Modern Math
        // { @"\Game", "⅁", new Ordinary("⅁") }, // Glyph not in Latin Modern Math
        { @"\Bbbk", new Ordinary("𝐤") }, // "𝐤" should make make a Variable so it's not a key here
        { @"\backprime", "‵", new Ordinary("‵") },
        { @"\varnothing", new Ordinary("∅") }, // Same as \emptyset
        { @"\blacktriangle", "▴", "▲", new Ordinary("▲") }, // ▴ not in Latin Modern Math
        { @"\blacktriangledown", "▾", "▼", new Ordinary("▼") }, // ▾ not in Latin Modern Math
        { @"\blacksquare", "▪", new Ordinary("▪") },
        { @"\blacklozenge", "⧫", "♦", new Ordinary("♦") }, // ⧫ not in Latin Modern Math
        { @"\bigstar", "★", new Ordinary("⋆") }, // ★ not in Latin Modern Math
        { @"\sphericalangle", "∢", new Ordinary("∢") },
        { @"\complement", "∁", new Ordinary("∁") },
        { @"\eth", new Ordinary("ð") }, // Same as \dh
        { @"\diagup", "╱", new Ordinary("/") }, // ╱ not in Latin Modern Math
        { @"\diagdown", "╲", new Ordinary("\\") }, // ╲ not in Latin Modern Math

        // Table 25: AMS Commands Deﬁned to Work in Both Math and Text Mode
        { @"\checkmark", "✓", new Ordinary("✓") },
        { @"\circledR", "®", new Ordinary("®") },
        { @"\maltese", "✠", new Ordinary("✠") },

        // Table 26: AMS Binary Operators
        { @"\dotplus", "∔", new BinaryOperator("∔") },
        { @"\smallsetminus", new BinaryOperator("∖") }, // Same as \setminus
        { @"\Cap", "⋒", new BinaryOperator("⋒") },
        { @"\Cup", "⋓", new BinaryOperator("⋓") },
        { @"\barwedge", "⌅", new BinaryOperator("⌅") },
        { @"\veebar", "⊻", new BinaryOperator("⊻") },
        // { @"\doublebarwedge", "⩞", new BinaryOperator("⩞") }, //Glyph not in Latin Modern Math
        { @"\boxminus", "⊟", new BinaryOperator("⊟") },
        { @"\boxtimes", "⊠", new BinaryOperator("⊠") },
        { @"\boxdot", "⊡", new BinaryOperator("⊡") },
        { @"\boxplus", "⊞", new BinaryOperator("⊞") },
        { @"\divideontimes", "⋇", new BinaryOperator("⋇") },
        { @"\ltimes", "⋉", new BinaryOperator("⋉") },
        { @"\rtimes", "⋊", new BinaryOperator("⋊") },
        { @"\leftthreetimes", "⋋", new BinaryOperator("⋋") },
        { @"\rightthreetimes", "⋌", new BinaryOperator("⋌") },
        { @"\curlywedge", "⋏", new BinaryOperator("⋏") },
        { @"\curlyvee", "⋎", new BinaryOperator("⋎") },
        { @"\circleddash", "⊝", new BinaryOperator("⊝") },
        { @"\circledast", "⊛", new BinaryOperator("⊛") },
        { @"\circledcirc", "⊚", new BinaryOperator("⊚") },
        { @"\centerdot", new BinaryOperator("·") }, // Same as \cdot
        { @"\intercal", "⊺", new BinaryOperator("⊺") },

        // Table 27: AMS Binary Relations
        { @"\leqq", "≦", new Relation("≦") },
        { @"\leqslant", "⩽", new Relation("⩽") },
        { @"\eqslantless", "⪕", new Relation("⪕") },
        { @"\lesssim", "≲", new Relation("≲") },
        { @"\lessapprox", "⪅", new Relation("⪅") },
        { @"\approxeq", "≊", new Relation("≊") },
        { @"\lessdot", "⋖", new Relation("⋖") },
        { @"\lll", "⋘", new Relation("⋘") },
        { @"\lessgtr", "≶", new Relation("≶") },
        { @"\lesseqgtr", "⋚", new Relation("⋚") },
        { @"\lesseqqgtr", "⪋", new Relation("⪋") },
        { @"\doteqdot", "≑", new Relation("≑") },
        { @"\risingdotseq", "≓", new Relation("≓") },
        { @"\fallingdotseq", "≒", new Relation("≒") },
        { @"\backsim", "∽", new Relation("∽") },
        { @"\backsimeq", "⋍", new Relation("⋍") },
        // { @"\subseteqq", "⫅", new Relation("⫅") }, // Glyph not in Latin Modern Math
        { @"\Subset", "⋐", new Relation("⋐") },
        // \sqsubset is defined in Table 8
        { @"\preccurlyeq", "≼", new Relation("≼") },
        { @"\curlyeqprec", "⋞", new Relation("⋞") },
        { @"\precsim", "≾", new Relation("≾") },
        // { @"\precapprox", "⪷", new Relation("⪷") }, // Glyph not in Latin Modern Math
        { @"\vartriangleleft", new Relation("⊲") }, // Same as \lhd
        { @"\trianglelefteq", new Relation("⊴") }, // Same as \unlhd
        { @"\vDash", "⊨", new Relation("⊨") },
        { @"\Vvdash", "⊪", new Relation("⊪") },
        { @"\smallsmile", new Relation("⌣") }, // Same as \smile
        { @"\smallfrown", new Relation("⌢") }, // Same as \frown
        { @"\bumpeq", "≏", new Relation("≏") },
        { @"\Bumpeq", "≎", new Relation("≎") },
        { @"\geqq", "≧", new Relation("≧") },
        { @"\geqslant", "⩾", new Relation("⩾") },
        { @"\eqslantgtr", "⪖", new Relation("⪖") },
        { @"\gtrsim", "≳", new Relation("≳") },
        { @"\gtrapprox", "⪆", new Relation("⪆") },
        { @"\gtrdot", "⋗", new Relation("⋗") },
        { @"\ggg", "⋙", new Relation("⋙") },
        { @"\gtrless", "≷", new Relation("≷") },
        { @"\gtreqless", "⋛", new Relation("⋛") },
        { @"\gtreqqless", "⪌", new Relation("⪌") },
        { @"\eqcirc", "≖", new Relation("≖") },
        { @"\circeq", "≗", new Relation("≗") },
        { @"\triangleq", "≜", new Relation("≜") },
        { @"\thicksim", new Relation("∼") }, // Same as \sim
        { @"\thickapprox", new Relation("≈") }, // Same as \approx
        // { @"\supseteqq", "⫆", new Relation("⫆") }, // Glyph not in Latin Modern Math
        { @"\Supset", "⋑", new Relation("⋑") },
        // \sqsupset is defined in Table 8
        { @"\succcurlyeq", "≽", new Relation("≽") },
        { @"\curlyeqsucc", "⋟", new Relation("⋟") },
        { @"\succsim", "≿", new Relation("≿") },
        // { @"\succapprox", "⪸", new Relation("⪸") }, // Glyph not in Latin Modern Math
        { @"\vartriangleright", new Relation("⊳") }, // Same as \rhd
        { @"\trianglerighteq", new Relation("⊵") }, // Same as \unrhd
        { @"\Vdash", "⊩", new Relation("⊩") },
        { @"\shortmid", new Relation("∣") }, // Same as \mid
        { @"\shortparallel", new Relation("∥") }, // Same as \parallel
        { @"\between", "≬", new Relation("≬") },
        // { @"\pitchfork", "⋔", new Relation("⋔") }, // Glyph not in Latin Modern Math
        { @"\varpropto", new Relation("∝") }, // Same as \propto
        { @"\blacktriangleleft", "◀", "◂", new Relation("◀") }, // ◂ not in Latin Modern Math
        { @"\therefore", "∴", new Relation("∴") },
        // { @"\backepsilon", "϶", new Relation("϶") }, // Glyph not in Latin Modern Math
        { @"\blacktriangleright", "▶", "▸", new Relation("▶") }, // ▸ not in Latin Modern Math
        { @"\because", "∵", new Relation("∵") },

        // Table 28: AMS Negated Binary Relations
        // U+0338, an overlapping slant, is used as a workaround when Unicode has no matching character
        { @"\nless", "≮", new Relation("≮") },
        { @"\nleq", "≰", new Relation("≰") },
        { @"\nleqslant", "⩽\u0338", new Relation("⩽\u0338") },
        { @"\nleqq", "≦\u0338", new Relation("≦\u0338") },
        { @"\lneq", "⪇", new Relation("⪇") },
        { @"\lneqq", "≨", new Relation("≨") },
        // \lvertneqq -> ≨ + U+FE00 (Variation Selector 1) Not dealing with variation selectors, thank you very much
        { @"\lnsim", "⋦", new Relation("⋦") },
        { @"\lnapprox", "⪉", new Relation("⪉") },
        { @"\nprec", "⊀", new Relation("⊀") },
        { @"\npreceq", "⪯\u0338", new Relation("⪯\u0338") },
        { @"\precnsim", "⋨", new Relation("⋨") },
        // { @"\precnapprox", "⪹", new Relation("⪹") }, // Glyph not in Latin Modern Math
        { @"\nsim", "≁", new Relation("≁") },
        { @"\nshortmid", new Relation("∤") }, // Same as \nmid
        { @"\nmid", "∤", new Relation("∤") },
        { @"\nvdash", "⊬", new Relation("⊬") },
        { @"\nvDash", "⊭", new Relation("⊭") },
        { @"\ntriangleleft", "⋪", new Relation("⋪") },
        { @"\ntrianglelefteq", "⋬", new Relation("⋬") },
        { @"\nsubseteq", "⊈", new Relation("⊈") },
        { @"\subsetneq", "⊊", new Relation("⊊") },
        // \varsubsetneq -> ⊊ + U+FE00 (Variation Selector 1) Not dealing with variation selectors, thank you very much
        // { @"\subsetneqq", "⫋", new Relation("⫋") }, // Glyph not in Latin Modern Math
        // \varsubsetneqq -> ⫋ + U+FE00 (Variation Selector 1) Not dealing with variation selectors, thank you very much
        { @"\ngtr", "≯", new Relation("≯") },
        { @"\ngeq", "≱", new Relation("≱") },
        { @"\ngeqslant", "⩾\u0338", new Relation("⩾\u0338") },
        { @"\ngeqq", "≧\u0338", new Relation("≧\u0338") },
        { @"\gneq", "⪈", new Relation("⪈") },
        { @"\gneqq", "≩", new Relation("≩") },
        // \gvertneqq -> ≩ + U+FE00 (Variation Selector 1) Not dealing with variation selectors, thank you very much
        { @"\gnsim", "⋧", new Relation("⋧") },
        { @"\gnapprox", "⪊", new Relation("⪊") },
        { @"\nsucc", "⊁", new Relation("⊁") },
        { @"\nsucceq", "⪰\u0338", new Relation("⪰\u0338") },
        // Duplicate entry in LaTeX Symbol list: \nsucceq
        { @"\succnsim", "⋩", new Relation("⋩") },
        // { @"\succnapprox", "⪺", new Relation("⪺") }, // Glyph not in Latin Modern Math
        { @"\ncong", "≇", new Relation("≇") },
        { @"\nshortparallel", new Relation("∦") }, // Same as \nparallel
        { @"\nparallel", "∦", new Relation("∦") },
        { @"\nVdash", "⊮", new Relation("⊮") }, // Error in LaTeX Symbol list: defined as \nvDash which duplicates above
        { @"\nVDash", "⊯", new Relation("⊯") },
        { @"\ntriangleright", "⋫", new Relation("⋫") },
        { @"\ntrianglerighteq", "⋭", new Relation("⋭") },
        { @"\nsupseteq", "⊉", new Relation("⊉") },
        // { @"\nsupseteqq", "⫆\u0338", new Relation("⫆\u0338") }, // Glyph not in Latin Modern Math
        { @"\supsetneq", "⊋", new Relation("⊋") },
        // \varsupsetneq -> ⊋ + U+FE00 (Variation Selector 1) Not dealing with variation selectors, thank you very much
        // { @"\supsetneqq", "⫌", new Relation("⫌") }, // Glyph not in Latin Modern Math
        // \varsupsetneqq -> ⫌ + U+FE00 (Variation Selector 1) Not dealing with variation selectors, thank you very much
      };
  }
}