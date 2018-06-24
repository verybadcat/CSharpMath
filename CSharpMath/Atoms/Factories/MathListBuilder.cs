using CSharpMath.Enumerations;
using CSharpMath.TableEnvironment;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathListBuilder {
    internal string _error;
    internal readonly char[] _chars;
    internal readonly int _length;
    internal int _currentChar;
    internal bool _textMode; //_spacesAllowed in iosMath
    internal FontStyle _currentFontStyle;
    internal TableEnvironmentProperties _currentEnvironment;
    internal Inner _currentInnerAtom;

    public string Error => _error;

    public MathListBuilder(string str) {
      _chars = str.ToCharArray();
      _currentFontStyle = FontStyle.Default;
      _length = str.Length;
    }
    public IMathList Build() {
      var r = BuildInternal(false);
      if (HasCharacters && _error == null) {
        SetError("Error; most likely mismatched braces " + new string(_chars));
      }
      if (_error != null) {
        return null;
      }
      return r;
    }

    internal char GetNextCharacter() => _chars[_currentChar++];

    internal void UnlookCharacter() => _ = _currentChar == 0 ? throw new InvalidOperationException("Can't unlook below character 0") : _currentChar--;

    internal bool HasCharacters => _currentChar < _length;


    internal IMathList BuildInternal(bool oneCharOnly)
      => BuildInternal(oneCharOnly, (char)0);

    internal IMathList BuildInternal(bool oneCharOnly, char stopChar) {
      if (oneCharOnly && stopChar > 0) {
        throw new InvalidOperationException("Cannot set both oneCharOnly and stopChar");
      }
      var r = new MathList();
      IMathAtom prevAtom = null;
      while (HasCharacters) {
        if (_error != null) {
          return null;
        }
        IMathAtom atom = null;
        var ch = GetNextCharacter();
        if (oneCharOnly) {
          if (ch == '^' || ch == '}' || ch == '_' || ch == '&') {
            // this is not the character we are looking for. They are for the caller to look at.
            UnlookCharacter();
            return r;
          }
        }
        if (stopChar > 0 && ch == stopChar) {
          return r;
        }
        switch (ch) {
          case '^':
            if (prevAtom == null || prevAtom.Superscript != null || !prevAtom.ScriptsAllowed) {
              prevAtom = MathAtoms.Create(MathAtomType.Ordinary, string.Empty);
              r.Add(prevAtom);
            }
            // this is a superscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            prevAtom.Superscript = this.BuildInternal(true);
            continue;
          case '_':
            if (prevAtom == null || prevAtom.Subscript != null || !prevAtom.ScriptsAllowed) {
              prevAtom = MathAtoms.Create(MathAtomType.Ordinary, string.Empty);
              r.Add(prevAtom);
            }
            // this is a subscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            prevAtom.Subscript = this.BuildInternal(true);
            continue;
          case '{':
            IMathList sublist;
            if (_currentEnvironment != null && _currentEnvironment.Name == null) {
              // \\ or \cr which do not have a corrosponding \end
              var oldEnv = _currentEnvironment;
              _currentEnvironment = null;
              sublist = BuildInternal(false, '}');
              _currentEnvironment = oldEnv;
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
          case '}':
            if (oneCharOnly || stopChar != 0) {
              throw new InvalidOperationException("This should have been handled before.");
            }
            SetError("Missing opening brace");
            return null;

          case '\\':
            var command = ReadCommand();
            var done = StopCommand(command, r, stopChar);
            if (done != null) {
              return done;
            }
            if (_error != null) {
              return null;
            }
            if (ApplyModifier(command, prevAtom)) {
              continue;
            }
            var fontStyleQ = MathAtoms.FontStyle(command);
            if (fontStyleQ.HasValue) {
              var fontStyle = fontStyleQ.Value;
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
            atom = AtomForCommand(command, stopChar); 
            if (atom == null) {
              SetError(_error ?? "Internal error");
              return null;
            }
            break;
          case '&': { // column separation in tables
              if (_currentEnvironment != null) {
                return r;
              }
              var table = BuildTable(null, r, false, stopChar); 
              if (table == null) return null;
              return MathLists.WithAtoms(table);
            }
          case '\'': // this case is NOT in iosMath
            int i = 1;
            while (ExpectCharacter('\'')) i++;
            Append: switch (i) {
              //glyphs are already superscripted
              //pick appropriate codepoint depending on number of primes
              case 1:
                atom = MathAtoms.Create(MathAtomType.Ordinary, "\u2032");
                break;
              case 2:
                atom = MathAtoms.Create(MathAtomType.Ordinary, "\u2033");
                break;
              case 3:
                atom = MathAtoms.Create(MathAtomType.Ordinary, "\u2034");
                break;
              case 4:
                atom = MathAtoms.Create(MathAtomType.Ordinary, "\u2057");
                break;
              default:
                r.Add(MathAtoms.Create(MathAtomType.Ordinary, "\u2057"));
                r.Add(new Space(-2.5f, true));
                i -= 4;
                goto Append;
            }
            break;
          default:
            if (_textMode && ch == ' ') {
              atom = MathAtoms.ForLatexSymbolName(" ");
            } else {
              atom = MathAtoms.ForCharacter(ch);
              if (atom == null) {
                // not a recognized character
                continue;
              }
            }
            break;
        }
        if (atom == null) {
          throw new ArgumentNullException("Atom shouldn't be null");
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
          SetError("Expected character not found: " + stopChar.ToString());
        }
      }
      return r;
    }

    internal string ReadString() {
      var builder = new StringBuilder();
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z')) {
          builder.Append(ch.ToString());
        } else {
          UnlookCharacter();
          break;
        }
      }
      return builder.ToString();
    }

    internal string ReadColor() {
      if (!(ExpectCharacter('{'))) {
        SetError("Missing {");
        return null;
      }
      SkipSpaces();
      var builder = new StringBuilder();
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if (ch == '#' || (ch >= 'A' && ch <= 'F') || (ch >= 'A' && ch <= 'f') || (ch >= '0' && ch <= '9')) {
          builder.Append(ch);
        } else {
          // we went too far
          UnlookCharacter();
          break;
        }
      }
      if (!ExpectCharacter('}')) {
        SetError("Missing }");
        return null;
      }
      return builder.ToString();
    }

    internal void SkipSpaces() {
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

    internal void AssertNotSpace(char ch) {
      if (char.IsWhiteSpace(ch) || char.IsControl(ch)) {
        //throw since this is not normal
        throw new InvalidOperationException("Expected non space character; found " + ch);
      }
    }

    internal bool ExpectCharacter(char ch) {
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
    internal string ReadCommand() {
      if (HasCharacters) {
        var ch = GetNextCharacter();
        if ((ch < 'a' || ch > 'z') && (ch < 'A' || ch > 'Z')) {
          return ch.ToString();
        } else {
          UnlookCharacter();
        }
      }
      return ReadString();
    }

    internal string ReadDelimiter() {
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
        return ch.ToString();
      }
      return null;
    }

    internal string ReadEnvironment() {
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

    internal IMathAtom _BoundaryAtomForDelimiterType(string delimiterType) {
      string delim = ReadDelimiter();
      if (delim == null) {
        SetError("Missing delimiter for " + delimiterType);
        return null;
      }
      var boundary = MathAtoms.BoundaryAtom(delim);
      if (boundary == null) {
        SetError(@"Invalid delimiter for \" + delimiterType + ": " + delim);
      }
      return boundary;
    }

    internal IMathAtom AtomForCommand(string command, char stopChar) {
      var atom = MathAtoms.ForLatexSymbolName(command);
      if(atom is Accent accent) {
        accent.InnerList = BuildInternal(true);
        return accent;
      }
      if (atom != null) {
        return atom;
      }
      switch (command) {
        case "frac":
          return new Fraction {
            Numerator = BuildInternal(true),
            Denominator = BuildInternal(true)
          };
        case "binom":
          return new Fraction(false) {
            Numerator = BuildInternal(true),
            Denominator = BuildInternal(true),
            LeftDelimiter = "(",
            RightDelimiter = ")"
          };
        case "sqrt":
          var rad = new Radical();
          if (ExpectCharacter('[')) {
            rad.Degree = BuildInternal(false, ']');
            rad.Radicand = BuildInternal(true);
          } else {
            rad.Radicand = BuildInternal(true);
          }
          return rad;
        case "left":
          var oldInner = _currentInnerAtom;
          _currentInnerAtom = new Inner {
            LeftBoundary = _BoundaryAtomForDelimiterType("left")
          };
          if (_currentInnerAtom.LeftBoundary == null) {
            return null;
          }
          _currentInnerAtom.InnerList = BuildInternal(false, stopChar); 
          if (_currentInnerAtom.RightBoundary == null) {
            SetError("Missing \\right");
            return null;
          }
          var newInner = _currentInnerAtom;
          _currentInnerAtom = oldInner;
          return newInner;
        case "overline":
          return new Overline {
            InnerList = BuildInternal(true)
          };
        case "underline":
          return new Underline() {
              InnerList = BuildInternal(true)
          };
        case "begin":
          var env = ReadEnvironment();
          if (env == null) {
            return null;
          }
          var table = BuildTable(env, null, false, stopChar); 
          return table;
        case "color":
          return new Color {
            ColorString = ReadColor(),
            InnerList = BuildInternal(true)
          };
        default:
          var extResult = Extension._MathListBuilder.AtomForCommand(this, command);
          if (extResult != null) return extResult;
          SetError("Invalid command \\" + command);
          return null;
      }
    }

    internal static Dictionary<string, (string left, string right)?> fractionCommands = new Dictionary<string, (string left, string right)?> {
      {"over", null },
      {"atop", null },
      {"choose", ("(", ")") },
      {"brack", ("[", "]") },
      {"brace", ("{", "}") }
    };

    internal MathList StopCommand(string command, MathList list, char stopChar) {
      if (command == "right") {
        if (_currentInnerAtom == null) {
          SetError("Missing \\left");
          return null;
        }
        _currentInnerAtom.RightBoundary = _BoundaryAtomForDelimiterType("right");
        if (_currentInnerAtom.RightBoundary == null) {
          return null;
        }
        return list;
      }
      if (fractionCommands.ContainsKey(command)) {
        bool rule = (command == "over");
        var fraction = new Fraction(rule);
        (string left, string right)? delimiters = fractionCommands[command];
        if (delimiters != null) {
          fraction.LeftDelimiter = delimiters.Value.left;
          fraction.RightDelimiter = delimiters.Value.right;
        }
        fraction.Numerator = list;
        fraction.Denominator = BuildInternal(false, stopChar);
        if (_error != null) {
          return null;
        }
        var fracList = MathLists.WithAtoms(fraction);
        return fracList;
      } else if (command == "\\" || command == "cr") {
        if (_currentEnvironment == null) {
          var table = BuildTable(null, list, true, stopChar); 
          if (table == null) return null;
          return MathLists.WithAtoms(table);
        } else {
          // stop the current list and increment the row count
          _currentEnvironment.NRows++;
          return list;
        }
      } else if (command == "end") {
        if (_currentEnvironment == null) {
          SetError(@"Missing \begin");
          return null;
        }
        var env = ReadEnvironment();
        if (env == null) {
          return null;
        }
        if (env!=_currentEnvironment.Name) {
          SetError($"Begin environment name {_currentEnvironment.Name} does not match end environment name {env}");
          return null;
        }
        _currentEnvironment.Ended = true;
        return list;
      }
      return null;
    }
    internal bool ApplyModifier(string modifier, IMathAtom atom) {
      if (modifier == "limits") {
        if (atom !=null && atom.AtomType == MathAtomType.LargeOperator) {
          var op = (LargeOperator)atom;
          op.Limits = true;
        } else {
          SetError(@"\limits can only be applied to an operator");
        }
        return true;
      } else if (modifier == "nolimits") {
        if (atom is LargeOperator op) {
          op.Limits = false;
        } else {
          SetError(@"\nolimits can only be applied to an operator");
        }
        return true;
      }
      return false;
    }

    internal void SetError(string message) {
      if (_error == null) {
        _error = message;
      }
    }

    internal IMathAtom BuildTable(string environment, IMathList firstList, bool isRow, char stopChar) { 
      var oldEnv = _currentEnvironment;
      _currentEnvironment = new TableEnvironmentProperties(environment);
      int currentRow = 0;
      int currentColumn = 0;
      List<List<IMathList>> rows = new List<List<IMathList>> { new List<IMathList>() };
      if (firstList != null) {
        rows[currentRow].Add(firstList);
        if (isRow) {
          _currentEnvironment.NRows++;
          currentRow++;
          rows.Add(new List<IMathList>());
        } else {
          currentColumn++;
        }
      }
      while (HasCharacters && !(_currentEnvironment.Ended)) {
        var list = BuildInternal(false, stopChar); 
        if (list == null) {
          return null;
        }
        rows[currentRow].Add(list);
        currentColumn++;
        if (_currentEnvironment.NRows > currentRow) {
          currentRow = _currentEnvironment.NRows;
          rows.Add(new List<IMathList>());
          currentColumn = 0;
        }
      }
      if (_currentEnvironment.Name != null && !_currentEnvironment.Ended) {
        SetError(@"Missing \end");
        return null;
      }
      IMathAtom table = MathAtoms.Table(_currentEnvironment.Name, rows, out string errorMessage);
      if (table == null && errorMessage != null) {
        SetError(errorMessage);
        return null;
      }
      _currentEnvironment = oldEnv;
      return table;
    }

// ^ LaTeX -> Math atoms
// v Math atoms -> LaTeX

    internal static Dictionary<int, string> SpaceToCommands { get; } = new Dictionary<int, string> {
      {3, "," },
      {4, ":" },
      {5, ";" },
      {-3, "!" },
      {18, "quad" },
      {36, "qquad" }
    };

    internal static Dictionary<LineStyle, string> StyleToCommands { get; } = new Dictionary<LineStyle, string> {
      {LineStyle.Display, "displaystyle" },
      {LineStyle.Text, "textstyle" },
      {LineStyle.Script, "scriptstyle" },
      {LineStyle.ScriptScript, "scriptscriptstyle" }
    };

    public static string DelimiterToString(IMathAtom delimiter) {
      var command = MathAtoms.DelimiterName(delimiter);
      if (command == null) {
        return string.Empty;
      }
      var singleChars = @"()[]<>|./";
      if (singleChars.IndexOf(command, StringComparison.OrdinalIgnoreCase) >= 0 && command.Length == 1) {
        return command;
      }
      if (command == "||") {
        return @"\|";
      } else {
        return @"\" + command;
      }
    }

    public static string MathListToString(IMathList mathList) {
      var builder = new StringBuilder();
      var currentFontStyle = FontStyle.Default;
      foreach (var atom in mathList) {
        if (currentFontStyle != atom.FontStyle) {
          if (currentFontStyle != FontStyle.Default) {
            // close the previous font style
            builder.Append("}");
          }
          if (atom.FontStyle != FontStyle.Default) {
            // open a new font style
            var fontStyleName = atom.FontStyle.FontName();
            builder.Append(@"\" + fontStyleName + "{");
          }
        }
        currentFontStyle = atom.FontStyle;
        switch (atom.AtomType) {
          case MathAtomType.Fraction: {
              IFraction fraction = (IFraction)atom;
              var numerator = MathListToString(fraction.Numerator);
              var denominator = MathListToString(fraction.Denominator);
              if (fraction.HasRule) {
                builder.Append(@"\frac{" + numerator + "}{" + denominator + "}");
              } else {
                string command = null;
                if (fraction.LeftDelimiter == null && fraction.RightDelimiter == null) {
                  command = "atop";
                } else if (fraction.LeftDelimiter == "(" && fraction.RightDelimiter == ")") {
                  command = "choose";
                } else if (fraction.LeftDelimiter == "{" && fraction.RightDelimiter == "}") {
                  command = "brace";
                } else if (fraction.LeftDelimiter == "[" && fraction.RightDelimiter == "]") {
                  command = "brack";
                } else {
                  command = $"atopwithdelims{fraction.LeftDelimiter}{fraction.RightDelimiter}";
                }
                builder.Append("{" + numerator + @" \" + command + " " + denominator + "}");
              }
            }
            break;
          case MathAtomType.Radical: {
              builder.Append(@"\sqrt");
              var radical = (IRadical)atom;
              if (radical.Degree != null) {
                builder.Append($"[{MathListToString(radical.Degree)}]");
              }
              builder.Append("{" + MathListToString(radical.Radicand) + "}");
              break;
            }
          case MathAtomType.Inner: {
              var inner = (IMathInner)atom;
              if (inner.LeftBoundary == null && inner.RightBoundary == null) {
                builder.Append("{" + MathListToString(inner.InnerList) + "}");
              } else {
                if (inner.LeftBoundary == null) {
                  builder.Append(@"\left. ");
                } else {
                  builder.Append(@"\left" + DelimiterToString(inner.LeftBoundary) + " ");
                }
                builder.Append(MathListToString(inner.InnerList));
                if (inner.RightBoundary == null) {
                  builder.Append(@"\right. ");
                } else {
                  builder.Append(@"\right" + DelimiterToString(inner.RightBoundary) + " ");

                }
              }
              break;
            }
          case MathAtomType.Table: {
              var table = (IMathTable)atom;
              if (table.Environment != null) {
                builder.Append(@"\begin{" + table.Environment + "}");
              }
              for (int i = 0; i < table.NRows; i++) {
                var row = table.Cells[i];
                for (int j = 0; j < row.Count; j++) {
                  var cell = row[j];
                  if (table.Environment == "matrix") {
                    if (cell.Count >= 1 && cell[0].AtomType == MathAtomType.Style) {
                      // remove the first atom.
                      var atoms = cell.Atoms.GetRange(1, cell.Count - 1);
                      cell = MathLists.WithAtoms(atoms.ToArray());
                    }
                  }
                  if (table.Environment == "eqalign" || table.Environment == "aligned" || table.Environment == "split") {
                    if (j == 1 && cell.Count >= 1 && cell[0].AtomType == MathAtomType.Ordinary && string.IsNullOrEmpty(cell[0].Nucleus)) {
                      // empty nucleus added for spacing. Remove it.
                      var atoms = cell.Atoms.GetRange(1, cell.Count - 1);
                      cell = MathLists.WithAtoms(atoms.ToArray());
                    }
                  }
                  builder.Append(MathListToString(cell));
                  if (j < row.Count - 1) {
                    builder.Append("&");
                  }
                }
                if (i < table.NRows - 1) {
                  builder.Append(@"\\ ");
                }
              }
              if (table.Environment != null) {
                builder.Append(@"\end{" + table.Environment + "}");
              }
              break;
            }
          case MathAtomType.Overline: {
              var over = (IOverline)atom;
              builder.Append(@"\overline{" + MathListToString(over.InnerList) + "}");
              break;
            }
          case MathAtomType.Underline: {
              var under = (IUnderline)atom;
              builder.Append(@"\underline{" + MathListToString(under.InnerList) + "}");
              break;
            }
          case MathAtomType.Accent: {
              var accent = (IAccent)atom;
              var list = accent.InnerList;
              accent.InnerList = null; //for lookup
              builder.Append(@"\" + MathAtoms.Commands[(MathAtom)atom] + "{" + MathListToString(list) + "}");
              break;
            }
          case MathAtomType.LargeOperator: {
              var op = (LargeOperator)atom;
              var command = MathAtoms.LatexSymbolNameForAtom(op);
              LargeOperator originalOperator = (LargeOperator)MathAtoms.ForLatexSymbolName(command);
              builder.Append($@"\{command} ");
              if (originalOperator.Limits != op.Limits) {
                if (op.Limits) {
                  builder.Append(@"\limits ");
                } else {
                  builder.Append(@"\nolimits ");
                }
              }
              break;
            }
          case MathAtomType.Space: {
              var space = (ISpace)atom;
              var intSpace = (int)space.Length;
              if (SpaceToCommands.ContainsKey(intSpace) && intSpace == space.Length) {
                var command = SpaceToCommands[intSpace];
                builder.Append(@"\" + command + " ");
              } else {
                if (space.IsMu)
                  builder.Append(@"\mkern" + space.Length.ToString("0.0") + "mu");
                else
                  builder.Append(@"\kern" + space.Length.ToString("0.0") + "pt");
              }
              break;
            }
          case MathAtomType.Style: {
              var style = (IStyle)atom;
              var command = StyleToCommands[style.LineStyle];
              builder.Append(@"\" + command + " ");
              break;
            }
          case MathAtomType.Color: {
              var color = (IColor)atom;
              builder.Append($@"\color{{{color.ColorString}}}{{{MathListToString(color.InnerList)}}}");
              break;
            }
          default: {
              var aNucleus = atom.Nucleus;
              if (String.IsNullOrEmpty(aNucleus)) {
                builder.Append(@"{}");
              } else if (aNucleus == "\u2236") {
                builder.Append(":");
              } else if (aNucleus == "\u2212") {
                builder.Append("-");
              } else {
                var command = MathAtoms.LatexSymbolNameForAtom((MathAtom)atom);
                if (command == null) {
                  builder.Append(aNucleus);
                } else {
                  builder.Append(@"\" + command + " ");
                }
              }
              break;
            }

        }
        if (atom.Superscript != null) {
          builder.Append(@"^{" + MathListToString(atom.Superscript) + "}");
        }
        if (atom.Subscript!=null) {
          builder.Append(@"_{" + MathListToString(atom.Subscript) + "}");
        }
      }
      if (currentFontStyle!=FontStyle.Default) {
        builder.Append("}");
      }
      return builder.ToString();
    }
  }
}
