using CSharpMath.Enumerations;
using CSharpMath.TableEnvironment;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atoms {
  public class MathListBuilder {
    private string _error;
    private readonly char[] _chars;
    private readonly int _length;
    private int _currentChar;
    private bool _spacesAllowed;
    private FontStyle _currentFontStyle;
    private TableEnvironmentProperties _currentEnvironment;
    private Inner _currentInnerAtom;

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

    private char? GetNextCharacter() {
      if (!HasCharacters) {
        return null;
      }
      var r = _chars[_currentChar];
      _currentChar++;
      return r;
    }

    private void UnlookCharacter() {
      if (_currentChar == 0) {
        throw new InvalidOperationException("Can't unlook below character 0");
      }
      _currentChar--;
    }

    private bool HasCharacters => _currentChar < _length;


    private IMathList BuildInternal(bool oneCharOnly)
      => BuildInternal(oneCharOnly, (char)0);

    private IMathList BuildInternal(bool oneCharOnly, char stopChar) {
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
        var ch = (char)GetNextCharacter();
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
              prevAtom = MathAtoms.Create(MathAtomType.Ordinary, "");
              r.Add(prevAtom);
            }
            // this is a superscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            prevAtom.Superscript = this.BuildInternal(true);
            continue;
          case '_':
            if (prevAtom == null || prevAtom.Subscript != null || !prevAtom.ScriptsAllowed) {
              prevAtom = MathAtoms.Create(MathAtomType.Ordinary, "");
              r.Add(prevAtom);
            }
            // this is a subscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            prevAtom.Subscript = this.BuildInternal(true);
            continue;
          case '{':
            var sublist = BuildInternal(false, '}');
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
            SetError("Mismatched braces");
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
              var oldSpacesAllowed = _spacesAllowed;
              var oldFontStyle = _currentFontStyle;
              _spacesAllowed = (command == "text");
              _currentFontStyle = fontStyle;
              var childList = BuildInternal(true);
              _currentFontStyle = oldFontStyle;
              _spacesAllowed = oldSpacesAllowed;
              prevAtom = childList.Atoms.LastOrDefault();
              r.Append(childList);
              if (oneCharOnly) {
                return r;
              }
              continue;
            }
            atom = AtomForCommand(command); 
            if (atom == null) {
              SetError(_error ?? "Internal error");
              return null;
            }
            break;
          case '&': { // column separation in tables
              if (_currentEnvironment != null) {
                return r;
              }
              var table = BuildTable(null, r, false);
              if (table == null) return null;
              return MathLists.WithAtoms(table);
            }
          default:
            if (_spacesAllowed && ch == ' ') {
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
          throw new Exception("Atom shouldn't be null");
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

    private string ReadString() {
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

    private string ReadColor() {
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

    private void SkipSpaces() {
      while (HasCharacters) {
        var ch = GetNextCharacter();
        if (ch < 0x21 || ch > 0x7e) {
          continue;
        } else {
          UnlookCharacter();
          return;
        }
      }
    }

    private void AssertNotSpace(char ch) {
      if (ch < 0x21 || ch > 0x7E) {
        SetError("Expected non space character; found " + ch);
      }
    }

    private bool ExpectCharacter(char ch) {
      AssertNotSpace(ch);
      SkipSpaces();
      if (HasCharacters) {
        var c = (char)GetNextCharacter();
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


    private string ReadCommand() {
      char[] singleCharCommands = @"{}$#%_| ,:>;!\".ToCharArray();
      if (HasCharacters) {
        var ch = (char)GetNextCharacter();
        if (singleCharCommands.Contains(ch)) {
          return ch.ToString();
        } else {
          UnlookCharacter();
        }
      }
      return ReadString();
    }

    private string ReadDelimiter() {
      SkipSpaces();
      while (HasCharacters) {
        var ch = (char)GetNextCharacter();
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

    private string ReadEnvironment() {
      if (!ExpectCharacter('{')) {
        SetError("Missing {");
        return null;
      }
      SkipSpaces();
      var env = ReadString();
      if (!ExpectCharacter('}')) {
        SetError("Missing }");
        return null;
      }
      return env;
    }

    private IMathAtom _BoundaryAtomForDelimiterType(string delimiterType) {
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

    private IMathAtom AtomForCommand(string command) {
      var atom = MathAtoms.ForLatexSymbolName(command);
      if (atom != null) {
        return atom;
      }
      var accent = MathAtoms.Accent(command);
      if (accent!=null) {
        accent.InnerList = BuildInternal(true);
        return accent;
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
          var ch = GetNextCharacter();
          if (ch == null)
            SetError("Missing argument for sqrt");
          else if (ch == '[') {
            rad.Degree = BuildInternal(false, ']');
            rad.Radicand = BuildInternal(true);
          } else {
            UnlookCharacter();
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
          _currentInnerAtom.InnerList = BuildInternal(false);
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
          var table = BuildTable(env, null, false);
          return table;
        case "color":
          return new MathColor {
            ColorString = ReadColor(),
            InnerList = BuildInternal(true)
          };
        default:
          SetError("Invalid command \\" + command);
          return null;
      }
    }

    private static Dictionary<string, (string left, string right)?> fractionCommands = new Dictionary<string, (string left, string right)?> {
      {"over", null },
      {"atop", null },
      {"choose", ("(", ")") },
      {"brack", ("[", "]") },
      {"brace", ("{", "}") }
    };

    private MathList StopCommand(string command, MathList list, char stopChar) {
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
          var table = BuildTable(null, list, true);
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
    private bool ApplyModifier(string modifier, IMathAtom atom) {
      if (modifier == "limits") {
        if (atom !=null && atom.AtomType == MathAtomType.LargeOperator) {
          var op = (LargeOperator)atom;
          op.Limits = true;
        } else {
          SetError("limits can only be applied to an operator.");
        }
        return true;
      } else if (modifier == "nolimits") {
        if (atom is LargeOperator op) {
          op.Limits = false;
        } else {
          SetError("nolimits can only be applied to an operator.");
        }
        return true;
      }
      return false;
    }

    private void SetError(string message) {
      if (_error == null) {
        SetError(message);
      }
    }

    private IMathAtom BuildTable(string environment, IMathList firstList, bool isRow) {
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
        var list = BuildInternal(false);
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

    private static Dictionary<int, string> SpaceToCommands { get; } = new Dictionary<int, string> {
      {3, "," },
      {4, ":" },
      {5, ";" },
      {-3, "!" },
      {18, "quad" },
      {36, "qquad" }
    };

    private static Dictionary<LineStyle, string> StyleToCommands { get; } = new Dictionary<LineStyle, string> {
      {LineStyle.Display, "displaystyle" },
      {LineStyle.Text, "textstyle" },
      {LineStyle.Script, "scriptstyle" },
      {LineStyle.ScriptScript, "scriptscriptstyle" }
    };

    public static string DelimiterToString(IMathAtom delimiter) {
      var command = MathAtoms.DelimiterName(delimiter);
      if (command == null) {
        return "";
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
              builder.Append(@"\" + MathAtoms.AccentName(accent) + "{" + MathListToString(accent.InnerList) + "}");
              break;
            }
          case MathAtomType.LargeOperator: {
              var op = (LargeOperator)atom;
              var command = MathAtoms.LatexSymbolNameForAtom(atom);
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
              var intSpace = (int)space.Space;
              if (SpaceToCommands.ContainsKey(intSpace) && intSpace == space.Space) {
                var command = SpaceToCommands[intSpace];
                builder.Append(@"\" + command + " ");
              } else {
                builder.Append(@"\mkern" + space.Space.ToString("0.0") + "mu");
              }
              break;
            }
          case MathAtomType.Style: {
              var style = (IMathStyle)atom;
              var command = StyleToCommands[style.Style];
              builder.Append(@"\" + command + " ");
              break;
            }
          case MathAtomType.Color: {
              var color = (IMathColor)atom;
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
                var command = MathAtoms.LatexSymbolNameForAtom(atom);
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
