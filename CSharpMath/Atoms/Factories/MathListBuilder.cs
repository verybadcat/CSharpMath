using CSharpMath.Enumerations;
using CSharpMath.Environment;
using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atoms {
  internal class MathListBuilder {
    private string _error;
    private char[] _chars;
    private int _length;
    private int _currentChar;
    private bool _spacesAllowed;
    private FontStyle _currentFontStyle;
    private EnvironmentProperties _currentEnvironment;
    private Inner _currentInnerAtom;
    public MathListBuilder(string str) {
      _chars = str.ToCharArray();
      _currentFontStyle = FontStyle.Default;
      _length = str.Length;
    }
    public IMathList Build() {
      var r = this.BuildInternal(false);
      if (HasCharacters && _error != null) {
        _error = "Error; most likely mismatched braces " + new string(_chars);
      }
      if (_error != null) {
        return null;
      }
      return r;
    }

    private char GetNextCharacter() {
      var r = _chars[_currentChar];
      _currentChar++;
      return r;
    }

    private void unlookCharacter() {
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
        var ch = GetNextCharacter();
        if (oneCharOnly) {
          if (ch == '^' || ch == '}' || ch == '_' || ch == '&') {
            // this is not the character we are looking for. They are for the caller to look at.
            unlookCharacter();
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
              r.AddAtom(prevAtom);
            }
            // this is a subscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            prevAtom.Superscript = this.BuildInternal(true);
            continue;
          case '_':
            if (prevAtom == null || prevAtom.Subscript != null || !prevAtom.ScriptsAllowed) {
              prevAtom = MathAtoms.Create(MathAtomType.Ordinary, "");
              r.AddAtom(prevAtom);
            }
            // this is a subscript for the previous atom.
            // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
            prevAtom.Subscript = this.BuildInternal(true);
            continue;
          case '{':
            var sublist = BuildInternal(false, '}');
            prevAtom = sublist.Atoms.Last();
            r.Append(sublist);
            if (oneCharOnly) {
              return r;
            }
            continue;

          case '}':
            if (stopChar == 0) {
              throw new InvalidOperationException("This should have been handled before.");
            }
            _error = "Mismatched braces";
            return null;

          case '\\':
            string command = ReadCommand();
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
              _currentFontStyle = fontStyle;
              var childList = BuildInternal(true);
              _currentFontStyle = oldFontStyle;
              _spacesAllowed = oldSpacesAllowed;
              prevAtom = childList.Atoms.Last();
              r.Append(childList);
              if (oneCharOnly) {
                return r;
              }
              continue;
            }
            atom = AtomForCommand(command);
            if (atom == null) {
              _error = "Internal error";
              return null;
            }
            break;
          case '&': { // column separation in tables
              if (_currentEnvironment != null) {
                return r;
              }
              var table = BuildTable(null, r, false);
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
        r.AddAtom(atom);
        prevAtom = atom;
        if (oneCharOnly) {
          return r; // we consumed our character.
        }
      }
      if (stopChar > 0) {
        if (stopChar == '}') {
          _error = "Missing closing brace";
        } else {
          // we never found our stop character.
          _error = "Expected character not found: " + stopChar.ToString();
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
          unlookCharacter();
          break;
        }
      }
      return builder.ToString();
    }

    private string ReadColor() {
      if (!(ExpectCharacter('{'))) {
        _error = "Missing {";
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
          unlookCharacter();
          break;
        }
      }
      if (!ExpectCharacter('}')) {
        _error = "Missing }";
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
          unlookCharacter();
          return;
        }
      }
    }

    private void AssertNotSpace(char ch) {
      if (ch < 0x21 || ch > 0x7E) {
        _error = "Expected non space character; found " + ch;
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
          unlookCharacter();
          return false;
        }
      }
      return false;
    }


    private string ReadCommand() {
      char[] singleCharCommands = @"{}$#%_| ,>;!\\".ToCharArray();
      if (HasCharacters) {
        var ch = GetNextCharacter();
        if (singleCharCommands.Contains(ch)) {
          return ch.ToString();
        } else {
          unlookCharacter();
        }
      }
      return ReadString();
    }

    private string ReadDelimiter() {
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

    private string ReadEnvironment() {
      if (ExpectCharacter('{')) {
        _error = "Missing {";
        return null;
      }
      SkipSpaces();
      var env = ReadString();
      if (!ExpectCharacter('}')) {
        _error = "Missing }";
        return null;
      }
      return env;
    }

    private IMathAtom AtomForCommand(string command) {
      var atom = MathAtoms.ForLatexSymbolName(command);
      if (atom != null) {
        return atom;
      }
      switch (command) {
        case "frac":
          var fraction = new Fraction();
          fraction.Numerator = BuildInternal(true);
          fraction.Denominator = BuildInternal(true);
          return fraction;
        case "binom":
          var frac = new Fraction(false);
          frac.Numerator = BuildInternal(true);
          frac.Denominator = BuildInternal(true);
          frac.LeftDelimiter = "(";
          frac.RightDelimiter = ")";
          return frac;
        case "sqrt":
          var rad = new Radical();
          var ch = GetNextCharacter();
          if (ch == '[') {
            rad.Degree = BuildInternal(false, ']');
            rad.Radicand = BuildInternal(true);
          } else {
            unlookCharacter();
            rad.Radicand = BuildInternal(true);
          }
          return rad;
        case "left":
          var oldInner = _currentInnerAtom;
          _currentInnerAtom = new Inner();
          _currentInnerAtom.LeftBoundary = MathAtoms.BoundaryAtom("left");
          if (_currentInnerAtom.LeftBoundary == null) {
            return null;
          }
          _currentInnerAtom.InnerList = BuildInternal(false);
          if (_currentInnerAtom.RightBoundary == null) {
            _error = "Missing \\right";
            return null;
          }
          var newInner = _currentInnerAtom;
          _currentInnerAtom = oldInner;
          return newInner;
        case "overline":
          var over = new Overline();
          over.InnerList = BuildInternal(true);
          return over;
        case "underline":
          var under = new Underline();
          under.InnerList = BuildInternal(true);
          return under;
        case "begin":
          var env = ReadEnvironment();
          if (env == null) {
            return null;
          }
          var table = BuildTable(env, null, false);
          return table;
        case "color":
          var mathColor = new MathColor();
          mathColor.ColorString = ReadColor();
          mathColor.InnerList = BuildInternal(true);
          return mathColor;
        default:
          _error = "Invalid command \\" + command;
          return null;
      }
    }

    private static Dictionary<string, String[]> fractionCommands = new Dictionary<string, string[]> {
      {"over", new string[0] },
      {"atop", new string[0] },
      {"choose", new string[]{"(", ")" } },
      {"brack", new string[]{"[", "]"} },
      {"brace", new string[]{"{", "}"} }
    };

    private IMathList StopCommand(string command, IMathList list, char stopChar) {
      if (command == "right") {
        if (_currentInnerAtom == null) {
          _error = "Missing \\left";
          return null;
        }
        return list;
      }
      if (fractionCommands.ContainsKey(command)) {
        bool rule = (command == "over");
        var fraction = new Fraction(rule);
        string[] delimiters = fractionCommands[command];
        if (delimiters.Count() == 2) {
          fraction.LeftDelimiter = delimiters[0];
          fraction.RightDelimiter = delimiters[1];
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
          return MathLists.WithAtoms(table);
        } else {
          // stop the current list and increment the row count
          _currentEnvironment.NRows++;
          return list;
        }
      } else if (command == "end") {
        if (_currentEnvironment == null) {
          _error = @"Missing \begin";
          return null;
        }
        _currentEnvironment.Ended = true;
        return list;
      }
      return null;
    }
    private bool ApplyModifier(string modifier, IMathAtom atom) {
      if (modifier == "limits") {
        if (atom.AtomType == MathAtomType.LargeOperator) {
          var op = (LargeOperator)atom;
          op.Limits = true;
        } else {
          _error = "nolimits can only be applied to an operator.";
        }
        return true;
      }else if (modifier == "nolimits") {
        if (atom is LargeOperator) {
          var op = (LargeOperator)atom;
          op.Limits = false;
        }
        return true;
      }
      return false;
    }

    private void setError(string message) {
      if (_error == null) {
        _error = message;
      }
    }

    private IMathAtom BuildTable(string environment, IMathList firstList, bool isRow) {
      var oldEnv = _currentEnvironment;
      _currentEnvironment = new EnvironmentProperties("env");
      int currentRow = 0;
      int currentColumn = 0;
      List<List<IMathList>> rows = new List<List<IMathList>>();
      rows.Add(new List<IMathList>());
      if (firstList!=null) {
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
      if (_currentEnvironment.Name!=null && !_currentEnvironment.Ended) {
        _error = @"Missing \end";
        return null;
      }
      IMathAtom table = MathAtoms.Table(_currentEnvironment.Name, rows, out string errorMessage);
      if (table == null && errorMessage!=null) {
        _error = errorMessage;
        return null;
      }
      _currentEnvironment = oldEnv;
      return table;
    }

    private Dictionary<int, string> spaceToCommands { get; } = new Dictionary<int, string> {
      {3, "," },
      {4, ">" },
      {5, ";" },
      {-3, "!" },
      {18, "quad" },
      {36, "qquad" }
    };

    private Dictionary<LineStyle, string> styleToCommands { get; } = new Dictionary<LineStyle, string> {
      {LineStyle.Display, "displaystyle" },
      {LineStyle.Text, "textstyle" },
      {LineStyle.Script, "scriptstyle" },
      {LineStyle.ScriptScript, "scriptscriptstyle" }
    };
    
    
  }
}
