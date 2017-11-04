using CSharpMath.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMath.Atoms.Factories {
  internal class MathListBuilder {
    private string _error;
    private char[] _chars;
    private int _length;
    private int _currentChar;
    private bool _spacesAllowed;
    private FontStyle _currentFontStyle;
    public MathListBuilder(string str) {
      _chars = str.ToCharArray();
      _currentFontStyle = FontStyle.Default;
      _length = str.Length;
    }
    public MathList Build() {
      var r = this.BuildInternal(false);
      if (HasCharacters && _error!=null) {
        _error = "Error; most likely mismatched braces " + new string(_chars);
      }
      if (_error!=null) {
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


    private MathList BuildInternal(bool oneCharOnly)
      => BuildInternal(oneCharOnly, (char)0);

    private MathList BuildInternal(bool oneCharOnly, char stopChar) {
      if (oneCharOnly && stopChar > 0) {
        throw new InvalidOperationException("Cannot set both oneCharOnly and stopChar");
      }
      var r = new MathList();
      IMathAtom prevAtom = null;
      while (HasCharacters) {
        if (_error!=null) {
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
        if (ch == '^') {
          if (prevAtom == null || prevAtom.Superscript!=null || !prevAtom.ScriptsAllowed) {
            prevAtom = MathAtoms.Create(MathAtomType.Ordinary, "");
            r.AddAtom(prevAtom);
          }
          // this is a subscript for the previous atom.
          // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
          prevAtom.Superscript = this.BuildInternal(true);
          continue;
        }
        if (ch == '_') {
          if (prevAtom == null || prevAtom.Subscript != null || !prevAtom.ScriptsAllowed) {
            prevAtom = MathAtoms.Create(MathAtomType.Ordinary, "");
            r.AddAtom(prevAtom);
          }
          // this is a subscript for the previous atom.
          // note, if the next char is StopChar, it will be consumed and doesn't count as stop.
          prevAtom.Subscript = this.BuildInternal(true);
          continue;
        }
        if (ch == '{') {
          var sublist = BuildInternal(false, '}');
          prevAtom = sublist.Atoms.Last();
          r.Append(sublist);
          if (oneCharOnly) {
            return r;
          }
          continue;
        }
        if (ch == '}') {
          if (stopChar == 0) {
            throw new InvalidOperationException("This should have been handled before.");
          }
          _error = "Mismatched braces";
          return null;
        }
        if (ch == '\\') {
          string command = ReadCommand();
          var done = StopCommand(command, r, stopChar);
          if (done!=null) {
            return done;
          }
          if (_error!=null) {
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
            var sublist = BuildInternal(true);
            _currentFontStyle = oldFontStyle;
            _spacesAllowed = oldSpacesAllowed;
            prevAtom = sublist.Atoms.Last();
            r.Append(sublist);
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
        }
      }
      return r;
    }

  }
}
