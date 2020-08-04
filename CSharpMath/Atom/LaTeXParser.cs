using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CSharpMath.Atom {
  using Atoms;
  using Space = Atoms.Space;
  using Structures;
  using static Structures.Result;
  using InvalidCodePathException = Structures.InvalidCodePathException;
  public class LaTeXParser {

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces",
      Justification = "This is a marker interface to enable compile-time type checking")]
#pragma warning disable CA1034 // Nested types should not be visible
    // Justification: Implementation details exposed for extensibility
    public interface IEnvironment { }
    public class TableEnvironment : IEnvironment {
      public TableEnvironment(string? name) => Name = name;
      public string? Name { get; set; }
      public bool Ended { get; set; }
      public int NRows { get; set; }
      public string? ArrayAlignments { get; set; }
    }
    public class InnerEnvironment : IEnvironment {
      public Boundary? RightBoundary { get; set; }
    }
#pragma warning restore CA1034 // Nested types should not be visible
    public string Chars { get; }
    public int NextChar { get; private set; }
    public bool TextMode { get; set; } //_spacesAllowed in iosMath
    public FontStyle CurrentFontStyle { get; set; }
    public Stack<IEnvironment> Environments { get; } = new Stack<IEnvironment>();
    public LaTeXParser(string str) {
      Chars = str;
      CurrentFontStyle = FontStyle.Default;
    }
    public Result<MathList> Build() => BuildInternal(false);
    public char ReadChar() => Chars[NextChar++];
    public void UndoReadChar() =>
      _ = NextChar == 0
      ? throw new InvalidCodePathException("Can't unlook below character 0")
      : NextChar--;
    public bool HasCharacters => NextChar < Chars.Length;
    public Result<MathList> ReadArgument(MathList? appendTo = null) => BuildInternal(true, r: appendTo);
    public Result<MathList?> ReadArgumentOptional(MathList? appendTo = null) =>
      ReadCharIfAvailable('[')
      ? BuildInternal(false, ']', r: appendTo).Bind(mathList => (MathList?)mathList)
      : (MathList?)null;
    public Result<MathList> ReadUntil(char stopChar, MathList? appendTo = null) =>
      BuildInternal(false, stopChar, r: appendTo);
    // TODO: Example
    //https://phabricator.wikimedia.org/T99369
    //https://phab.wmfusercontent.org/file/data/xsimlcnvo42siudvwuzk/PHID-FILE-bdcqexocj5b57tj2oezn/math_rendering.png
    //dt, \text{d}t, \partial t, \nabla\psi \\ \underline\overline{dy/dx, \text{d}y/\text{d}x, \frac{dy}{dx}, \frac{\text{d}y}{\text{d}x}, \frac{\partial^2}{\partial x_1\partial x_2}y} \\ \prime,
    private Result<MathList> BuildInternal(bool oneCharOnly, char stopChar = '\0', MathList? r = null) {
      if (oneCharOnly && stopChar > '\0') {
        throw new InvalidCodePathException("Cannot set both oneCharOnly and stopChar");
      }
      r ??= new MathList();
      MathAtom? prevAtom = null;
      while (HasCharacters) {
        MathAtom? atom = null;
        if (Chars[NextChar] == stopChar && stopChar > '\0') {
          NextChar++;
          return r;
        }
        var ((handler, splitIndex), error) = LaTeXSettings.Commands.TryLookup(Chars.AsSpan(NextChar));
        if (error != null) {
          NextChar++; // Point to the start of the erroneous command
          return error;
        }
        NextChar += splitIndex;

        (MathAtom?, MathList?) handlerResult;
        (handlerResult, error) = handler(this, r, stopChar);
        if (error != null) return error;

        switch (handlerResult) {
          case ({ } /* dummy */, { } atoms): // Atoms producer (pre-styled)
            r.Append(atoms);
            prevAtom = r.Atoms.LastOrDefault();
            if (oneCharOnly)
              return r;
            else continue;
          case (null, { } @return): // Environment ender
            return @return;
          case (null, null): // Atom modifier
            continue;
          case ({ } resultAtom, null): // Atom producer
            atom = resultAtom;
            break;
        }
        atom.FontStyle = CurrentFontStyle;
        r.Add(atom);
        prevAtom = atom;
        if (oneCharOnly) {
          return r; // we consumed our character.
        }
      }
      return stopChar switch
      {
        '\0' => r,
        '}' => "Missing closing brace",
        _ => "Expected character not found: " + stopChar.ToStringInvariant(),
      };
    }

    public string ReadString() {
      var builder = new StringBuilder();
      while (HasCharacters) {
        var ch = ReadChar();
        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
          builder.Append(ch.ToStringInvariant());
        } else {
          UndoReadChar();
          break;
        }
      }
      return builder.ToString();
    }

    public Result<Color> ReadColor() {
      if (!ReadCharIfAvailable('{')) {
        return "Missing {";
      }
      SkipSpaces();
      var index = NextChar;
      var length = 0;
      while (HasCharacters) {
        var ch = ReadChar();
        if (char.IsLetterOrDigit(ch) || ch == '#') {
          length++;
        } else {
          // we went too far
          UndoReadChar();
          break;
        }
      }
      var str = Chars.Substring(index, length);
      if (LaTeXSettings.ParseColor(str) is Color color) {
        SkipSpaces();
        if (!ReadCharIfAvailable('}'))
          return "Missing }";
        return color;
      } else {
        return "Invalid color: " + str;
      }
    }

    public void SkipSpaces() {
      while (HasCharacters) {
        var ch = ReadChar();
        if (char.IsWhiteSpace(ch) || char.IsControl(ch)) {
          continue;
        } else {
          UndoReadChar();
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

    /// <summary>Advances <see cref="NextChar"/> if <paramref name="ch"/> is available.</summary>
    /// <returns>Whether the char was read.</returns>
    public bool ReadCharIfAvailable(char ch) {
      AssertNotSpace(ch);
      SkipSpaces();
      if (HasCharacters) {
        var c = ReadChar();
        AssertNotSpace(c);
        if (c == ch) {
          return true;
        } else {
          UndoReadChar();
          return false;
        }
      }
      return false;
    }

    public Result<string> ReadEnvironment() {
      if (!ReadCharIfAvailable('{')) {
        return Err("Missing {");
      }
      SkipSpaces();
      var env = ReadString();
      SkipSpaces();
      if (!ReadCharIfAvailable('}')) {
        return Err("Missing }");
      }
      return Ok(env);
    }
    public Result<Structures.Space> ReadSpace() {
      SkipSpaces();
      var sb = new StringBuilder();
      while (HasCharacters) {
        var ch = ReadChar();
        if (char.IsDigit(ch) || ch == '.' || ch == '-' || ch == '+') {
          sb.Append(ch);
        } else {
          UndoReadChar();
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
        unit[i] = ReadChar();
      }
      return Structures.Space.Create(length, new string(unit), TextMode);
    }
    public Result<Boundary> ReadDelimiter(string commandName) {
      if (!HasCharacters) {
        return @"Missing delimiter for \" + commandName;
      }
      SkipSpaces();
      var ((result, splitIndex), error) = LaTeXSettings.BoundaryDelimiters.TryLookup(Chars.AsSpan(NextChar));
      if (error != null) {
        NextChar++; // Point to the start of the erroneous command
        return error;
      }
      NextChar += splitIndex;
      return result;
    }

    private static readonly Dictionary<string, (string left, string right)?> _matrixEnvironments =
      new Dictionary<string, (string left, string right)?> {
        { "matrix",  null } ,
        { "pmatrix", ("(", ")") } ,
        { "bmatrix", ("[", "]") },
        { "Bmatrix", ("{", "}") },
        { "vmatrix", ("|", "|") },
        { "Vmatrix", ("‖", "‖") }
      };
    public Result<MathAtom> ReadTable
      (string? name, MathList? firstList, bool isRow, char stopChar) {
      var environment = new TableEnvironment(name);
      Environments.Push(environment);
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
        if (!ReadCharIfAvailable('{')) {
          return "Missing array alignment";
        }
        var builder = new StringBuilder();
        var done = false;
        while (HasCharacters && !done) {
          var ch = ReadChar();
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
              return $"Invalid character '{ch}' encountered while parsing array alignments";
          }
        }
        if (!done) {
          return "Missing }";
        }
      }
      while (HasCharacters && !environment.Ended) {
        var (list, error) = BuildInternal(false, stopChar);
        if (error != null) return error;
        rows[currentRow].Add(list);
        currentColumn++;
        if (environment.NRows > currentRow) {
          currentRow = environment.NRows;
          rows.Add(new List<MathList>());
          currentColumn = 0;
        }
        // The } in \begin{matrix} is not stopChar so this line is not written in the while-condition
        if (stopChar != '\0' && Chars[NextChar - 1] == stopChar) break;
      }
      if (environment.Name != null && !environment.Ended) {
        return $@"Missing \end for \begin{{{environment.Name}}}";
      }

      // We have finished parsing the table, now interpret the environment
      name = environment.Name;
      var arrayAlignments = environment.ArrayAlignments;
      // Table environments with { Name: null } may have been popped by \right
      if (Environments.PeekOrDefault() == environment)
        Environments.Pop();

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
              new Boundary(left),
              new MathList(table),
              new Boundary(right)
            ),
            null => table
          };
        case "array":
          if (arrayAlignments is null)
            throw new InvalidCodePathException("arrayAlignments is null despite array environment");
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 18;
          for (int i = 0, j = 0; i < arrayAlignments.Length && j < table.NColumns; i++, j++) {
            // TODO: vertical lines in array currently unsupported
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
            return name + " environment can only have 2 columns";
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
            return name + " environment can only have 1 column";
          }
          table.InterRowAdditionalSpacing = 1;
          table.InterColumnSpacing = 0;
          table.SetAlignment(ColumnAlignment.Center, 0);
          return table;
        case "eqnarray":
          if (table.NColumns != 3) {
            return name + " must have exactly 3 columns";
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
              new Boundary("{"),
              new MathList(new Space(Structures.Space.ShortSpace), table),
              Boundary.Empty
            );
          }
        default:
          return "Unknown environment " + name;
      }
    }

    public static Result<MathList> MathListFromLaTeX(string str) {
      var builder = new LaTeXParser(str);
      return builder.Build().Match(Ok,
        error => Err(HelpfulErrorMessage(error, builder.Chars, builder.NextChar)));
    }

    public static string HelpfulErrorMessage(string error, string source, int right) {
      if (right <= 0) right = 1;
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

    static string BoundaryToLaTeX(Boundary delimiter) =>
      LaTeXSettings.BoundaryDelimitersReverse.TryGetValue(delimiter, out var command)
      ? command
      : delimiter.Nucleus ?? "";

    private static void MathListToLaTeX
      (MathList mathList, StringBuilder builder, FontStyle outerFontStyle) {
      static bool MathAtomToLaTeX(MathAtom atom, StringBuilder builder,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? command) {
        if (LaTeXSettings.CommandForAtom(atom) is string name) {
          command = name;
          builder.Append(name);
          if (name.AsSpan().StartsWithInvariant(@"\"))
            builder.Append(" ");
          return true;
        }
        command = null;
        return false;
      }

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
            builder.Append(@"\").Append(LaTeXSettings.FontStyles.SecondToFirst[atom.FontStyle]).Append("{");
          }
        }
        currentFontStyle = atom.FontStyle;
        switch (atom) {
          case Comment { Nucleus: var comment }:
            builder.Append('%').Append(comment).Append('\n');
            break;
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
                  ({ Nucleus: null }, { Nucleus: null }) => "atop",
                  ({ Nucleus: "(" }, { Nucleus: ")" }) => "choose",
                  ({ Nucleus: "{" }, { Nucleus: "}" }) => "brace",
                  ({ Nucleus: "[" }, { Nucleus: "]" }) => "brack",
                  (var left, var right) => $"atopwithdelims{BoundaryToLaTeX(left)}{BoundaryToLaTeX(right)}",
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
          case Inner { LeftBoundary: { Nucleus: null }, InnerList: var list, RightBoundary: { Nucleus: null } }:
            MathListToLaTeX(list, builder, currentFontStyle);
            break;
          case Inner { LeftBoundary: { Nucleus: "〈" }, InnerList: var list, RightBoundary: { Nucleus: "|" } }:
            builder.Append(@"\Bra{");
            MathListToLaTeX(list, builder, currentFontStyle);
            builder.Append("}");
            break;
          case Inner { LeftBoundary: { Nucleus: "|" }, InnerList: var list, RightBoundary: { Nucleus: "〉" } }:
            builder.Append(@"\Ket{");
            MathListToLaTeX(list, builder, currentFontStyle);
            builder.Append("}");
            break;
          case Inner { LeftBoundary: var left, InnerList: var list, RightBoundary: var right }:
            builder.Append(@"\left").Append(BoundaryToLaTeX(left)).Append(' ');
            MathListToLaTeX(list, builder, currentFontStyle);
            builder.Append(@"\right").Append(BoundaryToLaTeX(right)).Append(' ');
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
            MathAtomToLaTeX(accent, builder, out _);
            builder.Append("{");
            MathListToLaTeX(accent.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case LargeOperator op:
            if (MathAtomToLaTeX(op, builder, out var command)) {
              if (!(LaTeXSettings.AtomForCommand(command) is LargeOperator originalOperator))
                throw new InvalidCodePathException("original operator not found!");
              if (originalOperator.Limits == op.Limits)
                break;
            } else {
              builder.Append($@"\operatorname{{{op.Nucleus}}} ");
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
          case Colored colored:
            builder.Append(@"\color{");
            LaTeXSettings.ColorToString(colored.Color, builder)
              .Append("}{");
            MathListToLaTeX(colored.InnerList, builder, currentFontStyle);
            builder.Append("}");
            break;
          case ColorBox colorBox:
            builder.Append(@"\colorbox{");
            LaTeXSettings.ColorToString(colorBox.Color, builder)
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
          case var _ when MathAtomToLaTeX(atom, builder, out _):
            break;
          case Atoms.Space space:
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
