using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atom {
  using Atoms;
  using InvalidCodePathException = Structures.InvalidCodePathException;
  public class LaTeXParser {
    interface IEnvironment { }
    class TableEnvironment : IEnvironment {
      public TableEnvironment(string? name) => Name = name;
      public string? Name { get; set; }
      public bool Ended { get; set; }
      public int NRows { get; set; }
      public string? ArrayAlignments { get; set; }
    }
    class InnerEnvironment : IEnvironment {
      public Boundary? RightBoundary { get; set; }
    }
    public string Chars { get; }
    public int CurrentChar { get; private set; }
    private bool _textMode; //_spacesAllowed in iosMath
    private FontStyle _currentFontStyle;
    private readonly Stack<IEnvironment> _environments = new Stack<IEnvironment>();
    public string? Error { get; private set; }
    public LaTeXParser(string str) {
      Chars = str;
      _currentFontStyle = FontStyle.Default;
    }
    public MathList? Build() {
      var r = BuildInternal(false);
      if (HasCharacters && Error == null) {
        SetError("Error; most likely mismatched braces.");
      }
      return Error != null ? null : r;
    }
    private char GetNextCharacter() => Chars[CurrentChar++];
    private void UnlookCharacter() =>
      _ = CurrentChar == 0
      ? throw new InvalidCodePathException("Can't unlook below character 0")
      : CurrentChar--;
    private bool HasCharacters => CurrentChar < Chars.Length;
    private MathList? BuildInternal(bool oneCharOnly, char stopChar = '\0', MathList? r = null) {
      if (oneCharOnly && stopChar > '\0') {
        throw new InvalidCodePathException("Cannot set both oneCharOnly and stopChar");
      }
      r ??= new MathList();
      MathAtom? prevAtom = null;
      while (HasCharacters) {
        if (Error != null) {
          return null;
        }
        MathAtom atom;
        switch (GetNextCharacter()) {
          case var ch when oneCharOnly && (ch == '^' || ch == '}' || ch == '_' || ch == '&'):
            SetError($"{ch} cannot appear as an argument to a command");
            return r;
          case var ch when stopChar > '\0' && ch == stopChar:
            return r;
          case '^':
            if (prevAtom == null || prevAtom.Superscript.IsNonEmpty() || !prevAtom.ScriptsAllowed) {
              prevAtom = new Ordinary(string.Empty);
              r.Add(prevAtom);
            }
            // this is a superscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            this.BuildInternal(true, r: prevAtom.Superscript);
            if (Error != null) return null;
            continue;
          case '_':
            if (prevAtom == null || prevAtom.Subscript.IsNonEmpty() || !prevAtom.ScriptsAllowed) {
              prevAtom = new Ordinary(string.Empty);
              r.Add(prevAtom);
            }
            // this is a subscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            this.BuildInternal(true, r: prevAtom.Subscript);
            if (Error != null) return null;
            continue;
          case '{':
            MathList? sublist;
            if (_environments.PeekOrDefault() is TableEnvironment { Name: null }) {
              // \\ or \cr which do not have a corrosponding \end
              var oldEnv = _environments.Pop();
              sublist = BuildInternal(false, '}');
              _environments.Push(oldEnv);
            } else {
              sublist = BuildInternal(false, '}');
            }
            if (sublist == null) return null;
            prevAtom = sublist.Atoms.LastOrDefault();
            r.Append(sublist);
            if (oneCharOnly) {
              return r;
            }
            continue;
#warning TODO Example
          //https://phabricator.wikimedia.org/T99369
          //https://phab.wmfusercontent.org/file/data/xsimlcnvo42siudvwuzk/PHID-FILE-bdcqexocj5b57tj2oezn/math_rendering.png
          //dt, \text{d}t, \partial t, \nabla\psi \\ \underline\overline{dy/dx, \text{d}y/\text{d}x, \frac{dy}{dx}, \frac{\text{d}y}{\text{d}x}, \frac{\partial^2}{\partial x_1\partial x_2}y} \\ \prime,
          case '}' when oneCharOnly || stopChar != '\0':
            throw new InvalidCodePathException("This should have been handled before.");
          case '}':
            SetError("Missing opening brace");
            return null;
          case '\\':
            var command = ReadCommand();
            var done = StopCommand(command, r, stopChar);
            if (done != null) {
              return done;
            }
            if (Error != null) {
              return null;
            }
            if (ApplyModifier(command, prevAtom)) {
              continue;
            }
            if (LaTeXSettings.FontStyles.TryGetValue(command, out var fontStyle)) {
              var oldSpacesAllowed = _textMode;
              var oldFontStyle = _currentFontStyle;
              _textMode = (command == "text");
              _currentFontStyle = fontStyle;
              var childList = BuildInternal(true);
              if (childList == null) return null;
              _currentFontStyle = oldFontStyle;
              _textMode = oldSpacesAllowed;
              prevAtom = childList.Atoms.LastOrDefault();
              r.Append(childList);
              if (oneCharOnly) {
                return r;
              }
              continue;
            }
            switch (AtomForCommand(command, stopChar)) {
              case null:
                SetError(Error ?? "Internal error");
                return null;
              case var a:
                atom = a;
                break;
            }
            break;
          case '&': // column separation in tables
            if (_environments.PeekOrDefault() is TableEnvironment) {
              return r;
            }
            var table = BuildTable(null, r, false, stopChar);
            if (table == null) return null;
            return new MathList(table);
          case '\'': // this case is NOT in iosMath
            int i = 1;
            while (ExpectCharacter('\'')) i++;
            atom = new Prime(i);
            break;
          case ' ' when _textMode:
            atom = new Ordinary(" ");
            break;
          case var ch when ch <= sbyte.MaxValue:
            if (LaTeXSettings.ForAscii((sbyte)ch) is MathAtom asciiAtom)
              atom = asciiAtom;
            else continue; // Ignore ASCII spaces and control characters
            break;
          case var ch:
            // not a recognized character, display it directly
            atom = new Ordinary(ch.ToStringInvariant());
            break;
        }
        atom.FontStyle = _currentFontStyle;
        r.Add(atom);
        prevAtom = atom;
        if (oneCharOnly) {
          return r; // we consumed our character.
        }
      }
      if (stopChar > 0) {
        if (stopChar == '}') {
          SetError("Missing closing brace");
        } else {
          // we never found our stop character.
          SetError("Expected character not found: " + stopChar.ToStringInvariant());
        }
      }
      return r;
    }

    private string ReadString() {
      var builder = new StringBuilder();
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
          builder.Append(ch.ToStringInvariant());
        } else {
          UnlookCharacter();
          break;
        }
      }
      return builder.ToString();
    }

    private Structures.Color? ReadColor() {
      if (!ExpectCharacter('{')) {
        SetError("Missing {");
        return null;
      }
      SkipSpaces();
      var builder = new StringBuilder();
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if (char.IsLetterOrDigit(ch) || ch == '#') {
          builder.Append(ch);
        } else {
          // we went too far
          UnlookCharacter();
          break;
        }
      }
      var str = builder.ToString();
      if (!(Structures.Color.Create(str.AsSpan()) is { } color)) {
        SetError("Invalid color: " + str);
        return null;
      }
      SkipSpaces();
      if (!ExpectCharacter('}')) {
        SetError("Missing }");
        return null;
      }
      return color;
    }

    private void SkipSpaces() {
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if (char.IsWhiteSpace(ch) || char.IsControl(ch)) {
          continue;
        } else {
          UnlookCharacter();
          return;
        }
      }
    }

    private static void AssertNotSpace(char ch) {
      if (char.IsWhiteSpace(ch) || char.IsControl(ch)) {
        //throw since this is not normal
        throw new InvalidOperationException("Expected non space character; found " + ch);
      }
    }

    private bool ExpectCharacter(char ch) {
      AssertNotSpace(ch);
      SkipSpaces();
      if (HasCharacters) {
        var c = GetNextCharacter();
        AssertNotSpace(c);
        if (c == ch) {
          return true;
        } else {
          UnlookCharacter();
          return false;
        }
      }
      return false;
    }


    //static readonly char[] _singleCharCommands = @"{}$#%_| ,:>;!\".ToCharArray();
    private string ReadCommand() {
      if (HasCharacters) {
        var ch = GetNextCharacter();
        if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z')) {
          return ch.ToStringInvariant();
        } else {
          UnlookCharacter();
        }
      }
      return ReadString();
    }

    private string? ReadDelimiter() {
      SkipSpaces();
      while (HasCharacters) {
        var ch = GetNextCharacter();
        AssertNotSpace(ch);
        if (ch == '\\') {
          // a command
          var command = ReadCommand();
          if (command == "|") {
            return @"||";
          }
          return command;
        }
        return ch.ToStringInvariant();
      }
      return null;
    }

    private string? ReadEnvironment() {
      if (!ExpectCharacter('{')) {
        SetError("Missing {");
        return null;
      }
      SkipSpaces();
      var env = ReadString();
      SkipSpaces();
      if (!ExpectCharacter('}')) {
        SetError("Missing }");
        return null;
      }
      return env;
    }

    private Structures.Result<Structures.Space> ReadSpace() {
      SkipSpaces();
      var sb = new StringBuilder();
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if (char.IsDigit(ch) || ch == '.' || ch == '-' || ch == '+') {
          sb.Append(ch);
        } else {
          UnlookCharacter();
          break;
        }
      }
      var length = sb.ToString();
      if (string.IsNullOrEmpty(length)) {
        return "Expected length value";
      }
      SkipSpaces();
      var unit = new char[2];
      for (int i = 0; i < 2 && HasCharacters; i++) {
        unit[i] = GetNextCharacter();
      }
      return Structures.Space.Create(length, new string(unit), _textMode);
    }
    private Boundary? BoundaryAtomForDelimiterType(string delimiterType) {
      var delim = ReadDelimiter();
      if (delim == null) {
        SetError("Missing delimiter for " + delimiterType);
        return null;
      }
      if (!LaTeXSettings.BoundaryDelimiters.TryGetValue(delim, out var boundary)) {
        SetError(@"Invalid delimiter for \" + delimiterType + ": " + delim);
      }
      return boundary;
    }

    private MathAtom? AtomForCommand(string command, char stopChar) {
      switch (LaTeXSettings.AtomForCommand(command)) {
        case Accent accent:
          var innerList = BuildInternal(true);
          if (innerList is null) return null;
          return new Accent(accent.Nucleus, innerList);
        case MathAtom atom:
          return atom;
      }
      switch (command) {
        case "frac":
          var numerator = BuildInternal(true);
          if (numerator is null) return null;
          var denominator = BuildInternal(true);
          if (denominator is null) return null;
          return new Fraction(numerator, denominator);
        case "binom":
          numerator = BuildInternal(true);
          if (numerator is null) return null;
          denominator = BuildInternal(true);
          if (denominator is null) return null;
          return new Fraction(numerator, denominator, false) {
            LeftDelimiter = "(",
            RightDelimiter = ")"
          };
        case "sqrt":
          var degree = ExpectCharacter('[') ? BuildInternal(false, ']') : new MathList();
          if (degree is null) return null;
          var radicand = BuildInternal(true);
          return radicand != null ? new Radical(degree, radicand) : null;
        case "left":
          var leftBoundary = BoundaryAtomForDelimiterType("left");
          if (!(leftBoundary is Boundary left)) return null;
          _environments.Push(new InnerEnvironment());
          var innerList = BuildInternal(false, stopChar);
          if (innerList is null) return null;
          if (!(_environments.PeekOrDefault() is InnerEnvironment { RightBoundary: { } right })) {
            SetError($@"Missing \right for \left with delimiter {leftBoundary}");
            return null;
          }
          _environments.Pop();
          return new Inner(left, innerList, right);
        case "overline":
          innerList = BuildInternal(true);
          if (innerList is null) return null;
          return new Overline(innerList);
        case "underline":
          innerList = BuildInternal(true);
          if (innerList is null) return null;
          return new Underline(innerList);
        case "begin":
          var env = ReadEnvironment();
          if (env == null) {
            return null;
          }
          return BuildTable(env, null, false, stopChar);
        case "color":
          return (ReadColor()) switch
          {
            { } color when BuildInternal(true) is { } ml => new Color(color, ml),
            _ => null,
          };
        case "colorbox":
          return (ReadColor()) switch
          {
            { } color when BuildInternal(true) is { } ml => new ColorBox(color, ml),
            _ => null,
          };
        case "prime":
          SetError(@"\prime won't be supported as Unicode has no matching character. Use ' instead.");
          return null;
        case "kern":
        case "hskip":
          if (_textMode) {
            var (space, error) = ReadSpace();
            if (error != null) {
              SetError(error);
              return null;
            } else return new Space(space);
          }
          SetError($@"\{command} is not allowed in math mode");
          return null;
        case "mkern":
        case "mskip":
          if (!_textMode) {
            var (space, error) = ReadSpace();
            if (error != null) {
              SetError(error);
              return null;
            } else return new Space(space);
          }
          SetError($@"\{command} is not allowed in text mode");
          return null;
        case "raisebox":
          if (!ExpectCharacter('{')) { SetError("Expected {"); return null; }
          var (raise, err) = ReadSpace();
          if (err != null) {
            SetError(err);
            return null;
          }
          if (!ExpectCharacter('}')) { SetError("Expected }"); return null; }
          innerList = BuildInternal(true);
          if (innerList is null) return null;
          return new RaiseBox(raise, innerList);
        case "TeX":
          return TeX;
        case "operatorname":
          if (!ExpectCharacter('{')) { SetError("Expected {"); return null; }
          var operatorname = ReadString();
          if (!ExpectCharacter('}')) { SetError("Expected }"); return null; }
          return new LargeOperator(operatorname, null);
        default:
          SetError("Invalid command \\" + command);
          return null;
      }
    }

    private static readonly Dictionary<string, (string left, string right)?> fractionCommands =
      new Dictionary<string, (string, string)?> {
        { "over", null },
        { "atop", null },
        { "choose", ("(", ")") },
        { "brack", ("[", "]") },
        { "brace", ("{", "}") }
      };

    //should be \textrm instead of \text
    private static readonly MathAtom TeX = new Inner(Boundary.Empty,
      MathListFromLaTeX(@"\text{T\kern-.1667em\raisebox{-.5ex}{E}\kern-.125emX}")
      .Match(mathList => mathList, e =>
        throw new FormatException(@"A syntax error is present in the definition of \TeX.")),
      Boundary.Empty);

    private MathList? StopCommand(string command, MathList list, char stopChar) {
      switch (command) {
        case "right":
          while (_environments.PeekOrDefault() is TableEnvironment table)
            if (table.Name is null) {
              table.Ended = true;
              _environments.Pop(); // Get out of \\ or \cr before looking for \right
            } else {
              SetError($"Missing \\end{{{table.Name}}}");
              return null;
            }
          if (!(_environments.PeekOrDefault() is InnerEnvironment inner)) {
            SetError("Missing \\left");
            return null;
          }
          inner.RightBoundary = BoundaryAtomForDelimiterType("right");
          if (inner.RightBoundary == null) {
            return null;
          }
          return list;
        case var _ when fractionCommands.ContainsKey(command):
          var denominator = BuildInternal(false, stopChar);
          if (denominator is null) return null;
          var fraction = new Fraction(list, denominator, command == "over");
          if (fractionCommands[command] is (var left, var right)) {
            fraction.LeftDelimiter = left;
            fraction.RightDelimiter = right;
          };
          return new MathList(fraction);
        case "\\":
        case "cr":
          if (!(_environments.PeekOrDefault() is TableEnvironment environment)) {
            var table = BuildTable(null, list, true, stopChar);
            if (table == null) return null;
            return new MathList(table);
          } else {
            // stop the current list and increment the row count
            environment.NRows++;
            return list;
          }
        case "end":
          if (!(_environments.PeekOrDefault() is TableEnvironment endEnvironment)) {
            SetError(@"Missing \begin");
            return null;
          }
          var env = ReadEnvironment();
          if (env == null) {
            return null;
          }
          if (env != endEnvironment.Name) {
            SetError($"Begin environment name {endEnvironment.Name} does not match end environment name {env}");
            return null;
          }
          endEnvironment.Ended = true;
          return list;
      }
      return null;
    }
    private bool ApplyModifier(string modifier, MathAtom? atom) {
      switch (modifier) {
        case "limits":
          if (atom is LargeOperator limitsOp) {
            limitsOp.Limits = true;
          } else {
            SetError(@"\limits can only be applied to an operator");
          }
          return true;
        case "nolimits":
          if (atom is LargeOperator noLimitsOp) {
            noLimitsOp.Limits = false;
          } else {
            SetError(@"\nolimits can only be applied to an operator");
          }
          return true;
      }
      return false;
    }

    private void SetError(string error) => Error ??= error;

    private static readonly Dictionary<string, (string left, string right)?> _matrixEnvironments =
      new Dictionary<string, (string left, string right)?> {
        { "matrix",  null } ,
        { "pmatrix", ("(", ")") } ,
        { "bmatrix", ("[", "]") },
        { "Bmatrix", ("{", "}") },
        { "vmatrix", ("vert", "vert") },
        { "Vmatrix", ("Vert", "Vert") }
      };
    private MathAtom? BuildTable
      (string? name, MathList? firstList, bool isRow, char stopChar) {
      var environment = new TableEnvironment(name);
      _environments.Push(environment);
      int currentRow = 0;
      int currentColumn = 0;
      var rows = new List<List<MathList>> { new List<MathList>() };
      if (firstList != null) {
        rows[currentRow].Add(firstList);
        if (isRow) {
          environment.NRows++;
          currentRow++;
          rows.Add(new List<MathList>());
        } else {
          currentColumn++;
        }
      }
      if (environment.Name == "array") {
        if (!ExpectCharacter('{')) {
          SetError("Missing array alignment");
          return null;
        }
        var builder = new StringBuilder();
        var done = false;
        while (HasCharacters && !done) {
          var ch = GetNextCharacter();
          switch (ch) {
            case 'l':
            case 'c':
            case 'r':
            case '|':
              builder.Append(ch);
              break;
            case '}':
              environment.ArrayAlignments = builder.ToString();
              done = true;
              break;
            default:
              SetError($"Invalid character '{ch}' encountered while parsing array alignments");
              return null;
          }
        }
        if (!done) {
          SetError("Missing }");
          return null;
        }
      }
      while (HasCharacters && !environment.Ended) {
        var list = BuildInternal(false, stopChar);
        if (list == null) {
          return null;
        }
        rows[currentRow].Add(list);
        currentColumn++;
        if (environment.NRows > currentRow) {
          currentRow = environment.NRows;
          rows.Add(new List<MathList>());
          currentColumn = 0;
        }
        // The } in \begin{matrix} is not stopChar so this line is not written in the while-condition
        if (stopChar != '\0' && Chars[CurrentChar - 1] == stopChar) break;
      }
      if (environment.Name != null && !environment.Ended) {
        SetError($@"Missing \end for \begin{{{environment.Name}}}");
        return null;
      }

      // We have finished parsing the table, now interpret the environment
      name = environment.Name;
      var arrayAlignments = environment.ArrayAlignments;
      // Table environments with { Name: null } may have been popped by \right
      if (_environments.PeekOrDefault() == environment)
        _environments.Pop();

      var table = new Table(name, rows);
      switch (name) {
        case null:
          table.InterRowAdditionalSpacing = 1;
          for (int i = 0; i < table.NColumns; i++) {
            table.SetAlignment(ColumnAlignment.Left, i);
          }
          return table;
        case var _ when _matrixEnvironments.TryGetValue(name, out var delimiters):
          table.Environment = "matrix"; // TableEnvironment is set to matrix as delimiters are converted to latex outside the table.
          table.InterColumnSpacing = 18;

          var style = new Style(LineStyle.Text);
          foreach (var row in table.Cells) {
            foreach (var cell in row) {
              cell.Insert(0, style);
            }
          }
          return delimiters switch
          {
            (var left, var right) => new Inner(
              LaTeXSettings.BoundaryDelimiters[left],
              new MathList(table),
              LaTeXSettings.BoundaryDelimiters[right]
            ),
            null => table
          };
        case "array":
          if (arrayAlignments is null)
            throw new InvalidCodePathException("arrayAlignments is null despite array environment");
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 18;
          for (int i = 0, j = 0; i < arrayAlignments.Length && j < table.NColumns; i++, j++) {
#warning vertical lines in array currently unsupported
            while (arrayAlignments[i] == '|') i++;
            table.SetAlignment(arrayAlignments[i] switch
            {
              'l' => ColumnAlignment.Left,
              'c' => ColumnAlignment.Center,
              'r' => ColumnAlignment.Right,
              _ => throw new InvalidCodePathException("Invalid characters were not filtered")
            }, j);
          }
          return table;
        case "eqalign":
        case "split":
        case "aligned":
          if (table.NColumns != 2) {
            SetError(name + " environment can only have 2 columns");
            return null;
          } else {
            // add a spacer before each of the second column elements, in order to create the correct spacing for "=" and other relations.
            var spacer = new Ordinary(string.Empty);
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
            SetError(name + " environment can only have 1 column");
            return null;
          }
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 0;
          table.SetAlignment(ColumnAlignment.Center, 0);
          return table;
        case "eqnarray":
          if (table.NColumns != 3) {
            SetError(name + " must have exactly 3 columns");
            return null;
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
            SetError("cases environment must have 1 to 2 columns");
            return null;
          } else {
            table.Environment = "array";
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
            return new Inner(
              LaTeXSettings.BoundaryDelimiters["{"],
              new MathList(new Space(Structures.Space.ShortSpace), table),
              Boundary.Empty
            );
          }
        default:
          SetError("Unknown environment " + name);
          return null;
      }
    }

    public static Structures.Result<MathList> MathListFromLaTeX(string str) {
      var builder = new LaTeXParser(str);
      var list = builder.Build();
      return builder.Error is { } error
      ? Structures.Result.Err(HelpfulErrorMessage(error, builder.Chars, builder.CurrentChar))
      : list != null
      ? Structures.Result.Ok(list)
      : throw new InvalidCodePathException("Both error and list are null?");
    }

    public static string HelpfulErrorMessage(string error, string source, int right) {
      // Just like Xunit's helpful error message in Assert.Equal(string, string)
      const string dots = "···";
      const int lookbehind = 20;
      const int lookahead = 41;
      var sb = new StringBuilder("Error: ").Append(error);
      sb.Append('\n');
      var left = right - 1;
      var startIsFarAway = left > lookbehind;
      if (startIsFarAway)
        sb.Append(dots).Append(source, left - lookbehind, lookbehind);
      else sb.Append(source, 0, left);
      var endIsFarAway = left < source.Length - lookahead;
      if (endIsFarAway)
        sb.Append(source, left, lookahead).Append(dots);
      else sb.Append(source, left, source.Length - left);
      sb.Append('\n');
      if (startIsFarAway)
        sb.Append(' ', lookbehind + dots.Length);
      else sb.Append(' ', left);
      sb.Append("↑ (pos ").Append(right).Append(')');
      return sb.ToString();
    }

    // ^ LaTeX -> Math atoms
    // v Math atoms -> LaTeX

    public static string EscapeAsLaTeX(string literal) =>
      new StringBuilder(literal)
      .Replace("{", @"\{")
      .Replace("}", @"\}")
      .Replace(@"\", @"\backslash ")
      .Replace("#", @"\#")
      .Replace("$", @"\$")
      .Replace("%", @"\%")
      .Replace("&", @"\&")
      .Replace("^", @"\textasciicircum ")
      .Replace("_", @"\_")
      .Replace("~", @"\textasciitilde ")
      .ToString();

    private static void MathListToLaTeX
      (MathList mathList, StringBuilder builder, FontStyle outerFontStyle) {
      if (mathList is null) throw new ArgumentNullException(nameof(mathList));
      if (mathList.IsEmpty()) return;
      var currentFontStyle = outerFontStyle;
      foreach (var atom in mathList) {
        if (currentFontStyle != atom.FontStyle) {
          if (currentFontStyle != outerFontStyle) {
            // close the previous font style
            builder.Append("}");
          }
          if (atom.FontStyle != outerFontStyle) {
            // open a new font style
            builder.Append(@"\").Append(LaTeXSettings.FontStyles[atom.FontStyle]).Append("{");
          }
        }
        currentFontStyle = atom.FontStyle;
        switch (atom) {
          case Fraction fraction:
            if (fraction.HasRule) {
              builder.Append(@"\frac{");
              MathListToLaTeX(fraction.Numerator, builder, currentFontStyle);
              builder.Append("}{");
              MathListToLaTeX(fraction.Denominator, builder, currentFontStyle);
              builder.Append("}");
            } else {
              builder.Append("{");
              MathListToLaTeX(fraction.Numerator, builder, currentFontStyle);
              builder.Append(@" \").Append(
                (fraction.LeftDelimiter, fraction.RightDelimiter) switch
                {
                  (null, null) => "atop",
                  ("(", ")") => "choose",
                  ("{", "}") => "brace",
                  ("[", "]") => "brack",
                  (var left, var right) => $"atopwithdelims{left}{right}",
                }).Append(" ");
              MathListToLaTeX(fraction.Denominator, builder, currentFontStyle);
              builder.Append("}");
            }
            break;
          case Radical radical:
            builder.Append(@"\sqrt");
            if (radical.Degree.IsNonEmpty()) {
              builder.Append('[');
              MathListToLaTeX(radical.Degree, builder, currentFontStyle);
              builder.Append(']');
            }
            builder.Append('{');
            MathListToLaTeX(radical.Radicand, builder, currentFontStyle);
            builder.Append('}');
            break;
          case Inner inner:
            if (inner.LeftBoundary == Boundary.Empty && inner.RightBoundary == Boundary.Empty) {
              builder.Append('{');
              MathListToLaTeX(inner.InnerList, builder, currentFontStyle);
              builder.Append('}');
            } else {
              static string BoundaryToLaTeX(Boundary delimiter) {
                var command = LaTeXSettings.BoundaryDelimiters[delimiter];
                if (command == null) {
                  return string.Empty;
                }
                if ("()[]<>|./".Contains(command) && command.Length == 1)
                  return command;
                if (command == "||") {
                  return @"\|";
                } else {
                  return @"\" + command;
                }
              }
              builder.Append(@"\left").Append(BoundaryToLaTeX(inner.LeftBoundary)).Append(' ');
              MathListToLaTeX(inner.InnerList, builder, currentFontStyle);
              builder.Append(@"\right").Append(BoundaryToLaTeX(inner.RightBoundary)).Append(' ');
            }
            break;
          case Table table:
            if (table.Environment != null) {
              builder.Append(@"\begin{" + table.Environment + "}");
            }
            if (table.Environment == "array") {
              builder.Append('{');
              foreach (var alignment in table.Alignments)
                builder.Append(alignment switch
                {
                  ColumnAlignment.Left => 'l',
                  ColumnAlignment.Right => 'r',
                  _ => 'c'
                });
              builder.Append('}');
            }
            for (int i = 0; i < table.NRows; i++) {
              var row = table.Cells[i];
              for (int j = 0; j < row.Count; j++) {
                var cell = row[j];
                if (table.Environment == "matrix"
                    && cell.Count >= 1
                    && cell[0] is Style) {
                  // remove the first atom.
                  cell = cell.Slice(1, cell.Count - 1);
                }
                if (table.Environment switch
                {
                  "eqalign" => true,
                  "aligned" => true,
                  "split" => true,
                  _ => false
                }
                    && j == 1
                    && cell.Count >= 1
                    && cell[0] is Ordinary ord
                    && string.IsNullOrEmpty(ord.Nucleus)) {
                  // empty nucleus added for spacing. Remove it.
                  cell = cell.Slice(1, cell.Count - 1);
                }
                MathListToLaTeX(cell, builder, currentFontStyle);
                if (j < row.Count - 1) {
                  builder.Append("&");
                }
              }
              if (i < table.NRows - 1) {
                builder.Append(@"\\ ");
              }
            }
            if (table.Environment != null) {
              builder.Append(@"\end{")
                .Append(table.Environment)
                .Append("}");
            }
            break;
          case Overline over:
            builder.Append(@"\overline{");
            MathListToLaTeX(over.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case Underline under:
            builder.Append(@"\underline{");
            MathListToLaTeX(under.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case Accent accent:
            builder.Append(@"\")
              .Append(LaTeXSettings.CommandForAtom(accent))
              .Append("{");
            MathListToLaTeX(accent.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case LargeOperator op:
            var command = LaTeXSettings.CommandForAtom(op);
            if (command == null) {
              builder.Append($@"\operatorname{{{op.Nucleus}}} ");
            } else {
              builder.Append($@"\{command} ");
              if (!(LaTeXSettings.AtomForCommand(command) is LargeOperator originalOperator))
                throw new InvalidCodePathException("original operator not found!");
              if (originalOperator.Limits == op.Limits)
                break;
            }
            switch (op.Limits) {
              case true:
                builder.Append(@"\limits ");
                break;
              case false:
                if (!op.ForceNoLimits) builder.Append(@"\nolimits ");
                break;
              case null:
                break;
            }
            break;
          case Color color:
            builder.Append(@"\color{")
              .Append(color.Colour)
              .Append("}{");
            MathListToLaTeX(color.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case ColorBox colorBox:
            builder.Append(@"\colorbox{")
              .Append(colorBox.Colour)
              .Append("}{");
            MathListToLaTeX(colorBox.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case Prime prime:
            builder.Append('\'', prime.Length);
            break;
          case RaiseBox r:
            builder.Append(@"\raisebox{")
              .Append(r.Raise.Length.ToStringInvariant("0.0####"))
              .Append(r.Raise.IsMu ? "mu" : "pt")
              .Append("}{");
            MathListToLaTeX(r.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case var _ when LaTeXSettings.CommandForAtom(atom) is string name:
            builder.Append(@"\").Append(name).Append(" ");
            break;
          case Space space:
            var intSpace = (int)space.Length;
            if (space.IsMu)
              builder.Append(@"\mkern")
                .Append(space.Length.ToStringInvariant("0.0####"))
                .Append("mu");
            else
              builder.Append(@"\kern")
                .Append(space.Length.ToStringInvariant("0.0####"))
                .Append("pt");
            break;
          case { Nucleus: null }:
          case { Nucleus: "" }:
            builder.Append("{}");
            break;
          case { Nucleus: "\u2236" }:
            builder.Append(":");
            break;
          case { Nucleus: "\u2212" }:
            builder.Append("-");
            break;
          case { Nucleus: var aNucleus }:
            builder.Append(aNucleus);
            break;
        }
        static void AppendScript
          (StringBuilder builder, MathList script, char scriptChar, FontStyle currentFontStyle) {
          if (script.IsNonEmpty()) {
            builder.Append(scriptChar).Append('{');
            var lengthBeforeScript = builder.Length;
            MathListToLaTeX(script, builder, currentFontStyle);
            if (lengthBeforeScript + 1 == builder.Length)
              builder.Remove(lengthBeforeScript - 1, 1); // Remove { if script is only 1 char
            else
              builder.Append('}');
          }
        }
        AppendScript(builder, atom.Subscript, '_', currentFontStyle);
        AppendScript(builder, atom.Superscript, '^', currentFontStyle);
      }
      if (currentFontStyle != outerFontStyle) {
        builder.Append("}");
      }
    }
    public static StringBuilder MathListToLaTeX(MathList mathList, StringBuilder? sb = null) {
      sb ??= new StringBuilder();
      MathListToLaTeX(mathList, sb, FontStyle.Default);
      return sb;
    }
  }
}
