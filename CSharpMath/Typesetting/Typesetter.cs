using System;
using System.Collections.Generic;
using System.Text;
using CSharpMath.Atoms;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.Display;
using CSharpMath.Interfaces;
using System.Drawing;
using System.Linq;
using CSharpMath.TypesetterInternal;

namespace CSharpMath {
  public class Typesetter {
    private MathFont _font;
    private MathFont _styleFont;
    private LineStyle _style;
    private bool _cramped;
    private bool _spaced;
    private List<DisplayBase> _displayAtoms = new List<DisplayBase>();
    private PointF _currentPosition;
    private AttributedString _currentLine;
    private Range _currentLineIndexRange = Range.NotFoundRange;
    private List<IMathAtom> _currentAtoms = new List<IMathAtom>();

    public Typesetter(MathFont font, LineStyle style, bool cramped, bool spaced) {
      _font = font;
      _style = style;
      _cramped = cramped;
      _spaced = spaced;
    }

    public static MathListDisplay CreateLine(IMathList list, MathFont font, LineStyle style) {
      var finalized = list.FinalizedList();
      return _CreateLine(finalized, font, style, false);
    }

    private static MathListDisplay _CreateLine(
      IMathList list, MathFont font,
      LineStyle style, bool cramped, bool spaced = false) {
      var preprocessedAtoms = _PreprocessMathList(list);
      var typesetter = new Typesetter(font, style, cramped, spaced);
      typesetter._CreateDisplayAtoms(preprocessedAtoms);
      var lastAtom = list.Atoms.Last();
      var line = new MathListDisplay(typesetter._displayAtoms.ToArray(), 
        new Range(0, lastAtom.IndexRange.End));
      return line;
    }

    private void _CreateDisplayAtoms(List<IMathAtom> preprocessedAtoms) {
      IMathAtom prevNode = null;
      MathAtomType prevType = MathAtomType.MinValue;
      foreach (var atom in preprocessedAtoms) {
        switch (atom.AtomType) {
          case MathAtomType.Number:
          case MathAtomType.Variable:
          case MathAtomType.UnaryOperator:
            throw new InvalidOperationException($"Type {atom.AtomType} should have been removed by preprocessing");
          case MathAtomType.Boundary:
            throw new InvalidOperationException("A bounadry atom should never be inside a mathlist");
          case MathAtomType.Space:
            AddDisplayLine(false);
            var space = atom as MathSpace;
            _currentPosition.X += space.Space * _styleFont.MathTable.MuUnit;
            continue;
          case MathAtomType.Style:
            // stash the existing layout
            AddDisplayLine(false);
            var style = atom as IMathStyle;
            _style = style.Style;
            // We need to preserve the prevNode for any inter-element space changes,
            // so we skip to the next node.
            continue;
          case MathAtomType.Color:
            AddDisplayLine(false);
            var color = atom as IMathColor;
            var display = CreateLine(color.InnerList, _font, _style);
            display.LocalTextColor = ColorExtensions.From6DigitHexString(color.ColorString);
            break;
          case MathAtomType.Radical:
            AddDisplayLine(false);
            var rad = atom as IRadical;
            // Radicals are considered as Ord in rule 16.
            AddInterElementSpace(prevNode, MathAtomType.Ordinary);
            var displayRad = MakeRadical(rad.Radicand, rad.IndexRange);
            throw new NotImplementedException();
          // break;
          case MathAtomType.Ordinary:
          case MathAtomType.BinaryOperator:
          case MathAtomType.Relation:
          case MathAtomType.Open:
          case MathAtomType.Close:
          case MathAtomType.Placeholder:
          case MathAtomType.Punctuation:
            if (prevNode != null) {
              float interElementSpace = GetInterElementSpace(prevNode.AtomType, atom.AtomType);
              if (_currentLine.Length > 0) {
                if (interElementSpace > 0) {
                  // add a kerning of that space to the previous character.
                  // iosMath uses [NSString rangeOfComposedCharacterSequenceAtIndex: xxx] here.
                }
              } else {
                _currentPosition.X += interElementSpace;
              }
            }
            AttributedString current = null;
            if (atom.AtomType == MathAtomType.Placeholder) {
              current = new AttributedString(atom.Nucleus, _placeholderColor);
            } else {
              current = new AttributedString(atom.Nucleus, Color.Transparent);
            }
            _currentLine = AttributedStringExtensions.Combine(_currentLine, current);
            if (_currentLineIndexRange.Location == Range.NotFound) {
              _currentLineIndexRange = atom.IndexRange;
            } else {
              _currentLineIndexRange.Length += atom.IndexRange.Length;
            }
            // add the fused atoms
            if (atom.FusedAtoms != null) {
              _currentAtoms.AddRange(atom.FusedAtoms);
            } else {
              _currentAtoms.Add(atom);
            }
            if (atom.Subscript != null || atom.Superscript != null) {
              throw new NotImplementedException();
            }
            break;
        }

      }

      AddDisplayLine(false);
      if (_spaced && prevType!=MathAtomType.MinValue) {
        var lastDisplay = _displayAtoms.LastOrDefault();
        if (lastDisplay!=null) {
          float space = GetInterElementSpace(prevType, MathAtomType.Close);
          lastDisplay.Width += space;
        }
      }
    }

    private static Color _placeholderColor => Color.Blue;

    private float GetInterElementSpace(MathAtomType left, MathAtomType right) {
      var leftIndex = GetInterElementSpaceArrayIndexForType(left, true);
      var rightIndex = GetInterElementSpaceArrayIndexForType(right, false);
      var spaces = InterElementSpaces.Spaces;
      var spaceArray = spaces[leftIndex];
      var spaceType = spaceArray[rightIndex];
      Assertions.Assert(spaceType != InterElementSpaceType.Invalid, $"Invalid space between {left} and {right}");
      var multiplier = spaceType.SpacingInMu(_style);
      if (multiplier > 0) {
        return multiplier * _styleFont.MathTable.MuUnit;
      }
      return 0;
    }

    private void AddInterElementSpace(IMathAtom prevNode, MathAtomType currentType) {
      float space = 0;
      if (prevNode!=null) {
        space = GetInterElementSpace(prevNode.AtomType, currentType);
      } else if (_spaced) {
        space = GetInterElementSpace(MathAtomType.Open, currentType);
      }
      _currentPosition.X += space;
    }


    

    private int GetInterElementSpaceArrayIndexForType(MathAtomType atomType, bool row) {
      switch (atomType) {
        case MathAtomType.Color:
        case MathAtomType.Placeholder:
        case MathAtomType.Ordinary:
          return 0;
        case MathAtomType.LargeOperator:
          return 1;
        case MathAtomType.BinaryOperator:
          return 2;
        case MathAtomType.Relation:
          return 3;
        case MathAtomType.Open:
          return 4;
        case MathAtomType.Close:
          return 5;
        case MathAtomType.Punctuation:
          return 6;
        case MathAtomType.Fraction:
        case MathAtomType.Inner:
          return 7;
        case MathAtomType.Radical:
          if (row) {
            return 8;
          }
          throw new InvalidOperationException("Inter-element space undefined for radical on the right. Treat radical as ordinary.");
      }
      throw new InvalidOperationException($"Inter-element space undefined for atom type {atomType}");
    }

    private TextLineDisplay AddDisplayLine(bool evenIfLengthIsZero) {
      if (evenIfLengthIsZero || (_currentLine!=null && _currentLine.Length > 0)) {
        _currentLine.Font = _styleFont;
        var displayAtom = new TextLineDisplay(_currentLine, _currentPosition, _currentLineIndexRange, _styleFont, _currentAtoms);
        _displayAtoms.Add(displayAtom);
        _currentPosition.X += displayAtom.Width;
        _currentLine = new AttributedString();
        _currentAtoms = new List<IMathAtom>();
        _currentLineIndexRange = Ranges.NotFound;
        return displayAtom;
      }
      return null;
    }

    private static List<IMathAtom> _PreprocessMathList(IMathList list) {
      IMathAtom prevNode = null;
      var r = new List<IMathAtom>();
      foreach (IMathAtom atom in list.Atoms) {
        switch (atom.AtomType) {
          case MathAtomType.Variable:
          case MathAtomType.Number:
            // These are not a TeX type nodes. TeX does this during parsing the input.
            // switch to using the font specified in the atom
            var newFont = _ChangeFont(atom.Nucleus, atom.FontStyle);
            // we convert it to ordinary
            atom.AtomType = MathAtomType.Ordinary;
            atom.Nucleus = newFont;
            break;
          case MathAtomType.UnaryOperator:
          case MathAtomType.Ordinary:
            // TeX treats unary operators as Ordinary. So will we.
            atom.AtomType = MathAtomType.Ordinary;
            // This is Rule 14 to merge ordinary characters.
            // combine ordinary atoms together
            if (prevNode != null && prevNode.AtomType == MathAtomType.Ordinary
              && prevNode.Superscript == null && prevNode.Subscript == null) {
              prevNode.Fuse(atom);
              // skip the current node as we fused it
              continue;
            }
            break;
        }
        // TODO: add italic correction here or in second pass?
        prevNode = atom;
        r.Add(prevNode);
        break;
      }
      return r;
    }
    private static string _ChangeFont(string input, FontStyle style) {
      var builder = new StringBuilder();
      var inputChars = input.ToCharArray();
      foreach (var inputChar in inputChars) {
        var unicode = _StyleCharacter(inputChar, style);
        builder.Append(unicode);
      }
      return builder.ToString();
    }

    private static float GetStyleSize(LineStyle style, MathFont font) {
      float original = font.PointSize;
      switch (style) {
        case LineStyle.Script:
          return original * font.MathTable.ScriptScaleDown;
        case LineStyle.ScriptScript:
          return original * font.MathTable.ScriptScriptScaleDown;
        default:
          return original;
      }
    }

    private static char _StyleCharacter(char inputChar, FontStyle style) {
      // TODO: deal with fonts here. The Objective c app uses 32-bit characters
      // for this, which probably means this method needs to return 32-bit characters,
      // with resulting changes cascading from there.
      return inputChar;
    }

    private float _radicalVerticalGap => (_style == LineStyle.Display)
      ? _styleFont.MathTable.RadicalDisplayStyleVerticalGap
      : _styleFont.MathTable.RadicalVerticalGap;

    private RadicalDisplay MakeRadical(IMathList radicand, Range range) {
      var innerDisplay = _CreateLine(radicand, _font, _style, true);
      throw new NotImplementedException();
    }
    
  }
}
