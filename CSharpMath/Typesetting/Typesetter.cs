using System;
using System.Collections.Generic;
using CSharpMath.Atoms;
using CSharpMath.Atoms.Atom;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using ColorAtom = CSharpMath.Atoms.Atom.Color;
using Color = CSharpMath.Structures.Color;
using System.Drawing;
using System.Linq;
using CSharpMath.TypesetterInternal;

namespace CSharpMath {
  public class Typesetter<TFont, TGlyph> where TFont: IFont<TGlyph> {
    internal readonly TFont _font;
    internal readonly TypesettingContext<TFont, TGlyph> _context;
    internal FontMathTable<TFont, TGlyph> _mathTable => _context.MathTable;
    internal TFont _styleFont;
    internal LineStyle _style;
    internal readonly bool _cramped;
    internal readonly bool _spaced;
    internal readonly List<IDisplay<TFont, TGlyph>> _displayAtoms =
      new List<IDisplay<TFont, TGlyph>>();
    internal PointF _currentPosition; // the Y axis is NOT inverted in the typesetter.
    internal readonly AttributedString<TFont, TGlyph> _currentLine;
    internal Range _currentLineIndexRange = Range.NotFound;
    internal readonly List<MathAtom> _currentAtoms = new List<MathAtom>();
    internal const int _delimiterFactor = 901;
    internal const int _delimiterShortfallPoints = 5;
    private LineStyle _scriptStyle => _style switch
    {
      LineStyle.Display => LineStyle.Script,
      LineStyle.Text => LineStyle.Script,
      LineStyle.Script => LineStyle.ScriptScript,
      LineStyle.ScriptScript => LineStyle.ScriptScript,
      _ => throw new ArgumentOutOfRangeException(nameof(_style))
    };
    private LineStyle _fractionStyle =>
      _style == LineStyle.ScriptScript ? _style : _style + 1;
    private bool _subscriptCramped => true;
    private bool _superscriptCramped => _cramped;
    private float _superscriptShiftUp =>
      _cramped
      ? _mathTable.SuperscriptShiftUpCramped(_styleFont)
      : _mathTable.SuperscriptShiftUp(_styleFont);
    internal Typesetter(TFont font, TypesettingContext<TFont, TGlyph> context,
      LineStyle style, bool cramped, bool spaced) {
      _font = font;
      _context = context;
      _style = style;
      _styleFont = _context.MathFontCloner.Invoke(font, context.MathTable.GetStyleSize(style, font));
      _cramped = cramped;
      _spaced = spaced;
      _currentLine = new AttributedString<TFont, TGlyph>();
    }
    public static ListDisplay<TFont, TGlyph> CreateLine
      (MathList list, TFont font, TypesettingContext<TFont, TGlyph> context, LineStyle style) =>
      Typesetter<TFont, TGlyph>.CreateLine(list.FinalizedList(), font, context, style, false);

    private static ListDisplay<TFont, TGlyph> CreateLine(
      MathList list, TFont font, TypesettingContext<TFont, TGlyph> context,
      LineStyle style, bool cramped, bool spaced = false) {
      List<MathAtom> _PreprocessMathList() {
        MathAtom? prevNode = null;
        var r = new List<MathAtom>();
        foreach (var atom in list.Atoms) {
          // we do not use a switch statement on AtomType here as we may be changing said type.
          if (atom.AtomType == MathAtomType.Variable || atom.AtomType == MathAtomType.Number) {
            // These are not a TeX type nodes. TeX does this during parsing the input.
            // switch to using the font specified in the atom
            var newFont = context.ChangeFont(atom.Nucleus, atom.FontStyle);
            // we convert it to ordinary
            atom.AtomType = MathAtomType.Ordinary;
            atom.Nucleus = newFont;
          }
          if (atom.AtomType == MathAtomType.Ordinary || atom.AtomType == MathAtomType.UnaryOperator) {
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
          }
          // TODO: add italic correction here or in second pass?
          prevNode = atom;
          r.Add(prevNode);
        }
        return r;
      }
      var typesetter = new Typesetter<TFont, TGlyph>(font, context, style, cramped, spaced);
      typesetter.CreateDisplayAtoms(_PreprocessMathList());
      return new ListDisplay<TFont, TGlyph>(typesetter._displayAtoms.ToArray());
    }
    private void CreateDisplayAtoms(List<MathAtom> preprocessedAtoms) {
      MathAtom? prevNode = null;
      var prevType = MathAtomType.MinValue;
      foreach (var atom in preprocessedAtoms) {
        switch (atom.AtomType) {
          case MathAtomType.Number:
          case MathAtomType.Variable:
          case MathAtomType.UnaryOperator:
            throw new InvalidCodePathException
              ($"Type {atom.AtomType} should have been removed by preprocessing");
          case MathAtomType.Boundary:
            throw new InvalidCodePathException("A boundary atom should never be inside a mathlist");
          case MathAtomType.Space: {
              AddDisplayLine(false);
              var space = (Space)atom;
              _currentPosition.X += space.ActualLength(_mathTable, _font);
              continue;
            }
          case MathAtomType.Style: {
              // stash the existing layout
              AddDisplayLine(false);
              var style = (Style)atom;
              _style = style.LineStyle;
              _styleFont =
                _context.MathFontCloner.Invoke(_font, _mathTable.GetStyleSize(_style, _font));
              // We need to preserve the prevNode for any inter-element space changes,
              // so we skip to the next node.
              continue;
            }
          case MathAtomType.Color: {
              AddDisplayLine(false);
              AddInterElementSpace(prevNode, MathAtomType.Color);
              var color = (ColorAtom)atom;
              var colorDisplay = CreateLine(color.InnerList, _font, _context, _style);
              colorDisplay.SetTextColorRecursive(Color.Create(color.ColorString.AsSpan()));
              colorDisplay.Position = _currentPosition;
              _currentPosition.X += colorDisplay.Width;
              _displayAtoms.Add(colorDisplay);
              break;
            }
          case MathAtomType.Radical: {
              AddDisplayLine(false);
              var rad = (Radical)atom;
              // Radicals are considered as Ord in rule 16.
              AddInterElementSpace(prevNode, MathAtomType.Ordinary);
              var displayRad = MakeRadical(rad.Radicand, rad.IndexRange);
              if (rad.Degree != null) {
                // add the degree to the radical
                displayRad.SetDegree(
                  CreateLine(rad.Degree, _styleFont, _context, LineStyle.Script),
                  _styleFont, _mathTable);
              }
              _displayAtoms.Add(displayRad);
              _currentPosition.X += displayRad.Width;

              if (atom.Superscript != null || atom.Subscript != null) {
                MakeScripts(atom, displayRad, rad.IndexRange.Location, 0);
              }
              break;
            }
          case MathAtomType.Fraction: {
              AddDisplayLine(false);
              var fraction = (Fraction)atom;
              AddInterElementSpace(prevNode, atom.AtomType);
              var fractionDisplay = MakeFraction(fraction);
              _displayAtoms.Add(fractionDisplay);
              _currentPosition.X += fractionDisplay.Width;
              if (atom.Superscript != null || atom.Subscript != null) {
                MakeScripts(atom, fractionDisplay, fraction.IndexRange.Location, 0);
              }
              break;
            }
          case MathAtomType.Inner: {
              AddDisplayLine(false);
              AddInterElementSpace(prevNode, atom.AtomType);
              var inner = (Inner)atom;
              ListDisplay<TFont, TGlyph> innerDisplay;
              if (inner.LeftBoundary != null || inner.RightBoundary != null) {
                innerDisplay = _MakeLeftRight(inner);
              } else {
                innerDisplay = CreateLine(inner.InnerList, _font, _context, _style, _cramped);
              }
              innerDisplay.Position = _currentPosition;
              _currentPosition.X += innerDisplay.Width;
              _displayAtoms.Add(innerDisplay);
              if (atom.Subscript != null || atom.Superscript != null) {
                MakeScripts(atom, innerDisplay, atom.IndexRange.Location, 0);
              }
              break;
            }
          case MathAtomType.Underline: {
              AddDisplayLine(false);
              // Underline is considered as Ord in rule 16.
              AddInterElementSpace(prevNode, MathAtomType.Ordinary);
              atom.AtomType = MathAtomType.Ordinary;
              var underline = (Underline)atom;
              var innerListDisplay = Typesetter<TFont, TGlyph>.CreateLine
                (underline.InnerList, _font, _context, _style, _cramped);
              var underlineDisplay =
                new OverOrUnderlineDisplay<TFont, TGlyph>(innerListDisplay, _currentPosition) {
                LineShiftUp = -(innerListDisplay.Descent + _mathTable.UnderbarVerticalGap(_styleFont)),
                LineThickness = _mathTable.UnderbarRuleThickness(_styleFont)
              };
              _displayAtoms.Add(underlineDisplay);
              _currentPosition.X += underlineDisplay.Width;
              // add super scripts || subscripts
              if (atom.Subscript != null || atom.Superscript != null) {
                MakeScripts(atom, underlineDisplay, atom.IndexRange.Location, 0);
              }
              break;
            }
          case MathAtomType.Overline: {
              AddDisplayLine(false);
              // Overline is considered as Ord in rule 16.
              AddInterElementSpace(prevNode, MathAtomType.Ordinary);
              atom.AtomType = MathAtomType.Ordinary;

              var overline = (Overline)atom;
              var innerListDisplay = Typesetter<TFont, TGlyph>.CreateLine
                (overline.InnerList, _font, _context, _style, true);
              var overlineDisplay =
                new OverOrUnderlineDisplay<TFont, TGlyph>(innerListDisplay, _currentPosition) {
                LineShiftUp = innerListDisplay.Ascent + _mathTable.OverbarVerticalGap(_font)
                + _mathTable.OverbarRuleThickness(_font) + _mathTable.OverbarExtraAscender(_font),
                LineThickness = _mathTable.OverbarRuleThickness(_styleFont)
              };
              _displayAtoms.Add(overlineDisplay);
              _currentPosition.X += overlineDisplay.Width;
              // add super scripts || subscripts
              if (atom.Subscript != null || atom.Superscript != null) {
                MakeScripts(atom, overlineDisplay, atom.IndexRange.Location, 0);
              }
              break;
            }
          case MathAtomType.Accent: {
              AddDisplayLine(false);
              // Accent is considered as Ord in rule 16.
              AddInterElementSpace(prevNode, MathAtomType.Ordinary);
              atom.AtomType = MathAtomType.Ordinary;

              var accent = (Accent)atom;
              var accentDisplay = MakeAccent(accent);
              _displayAtoms.Add(accentDisplay);
              _currentPosition.X += accentDisplay.Width;
              // add super scripts || subscripts
              if (atom.Subscript != null || atom.Superscript != null) {
                MakeScripts(atom, accentDisplay, atom.IndexRange.Location, 0);
              }
              break;
            }
          case MathAtomType.Table: {
              AddDisplayLine(false);
              // We will consider tables as inner
              AddInterElementSpace(prevNode, MathAtomType.Inner);
              var table = (Table)atom;
              table.AtomType = MathAtomType.Inner;
              var tableDisplay = MakeTable(table);
              _displayAtoms.Add(tableDisplay);
              _currentPosition.X += tableDisplay.Width;
              break;
            }
          case MathAtomType.LargeOperator: {
              AddDisplayLine(false);
              AddInterElementSpace(prevNode, atom.AtomType);
              var op = (LargeOperator)atom;
              var opDisplay = MakeLargeOperator(op);
              _displayAtoms.Add(opDisplay);
              break;
            }
          case MathAtomType.Ordinary:
          case MathAtomType.BinaryOperator:
          case MathAtomType.Relation:
          case MathAtomType.Open:
          case MathAtomType.Close:
          case MathAtomType.Placeholder:
          case MathAtomType.Punctuation:
          case MathAtomType.Prime: {
              if (prevNode != null) {
                float interElementSpace = GetInterElementSpace(prevNode.AtomType, atom.AtomType);
                if (_currentLine.Length > 0) {
                  if (interElementSpace > 0) {
                    var run = _currentLine.Runs.Last();
                    run.GlyphInfos.Last().KernAfterGlyph = interElementSpace;
                  }
                } else {
                  _currentPosition.X += interElementSpace;
                }
              }
              var nucleusText = atom.Nucleus;
              var glyphs = _context.GlyphFinder.FindGlyphs(_font, nucleusText);
              var current = new AttributedGlyphRun<TFont, TGlyph>(
                nucleusText, glyphs, _font, atom.AtomType == MathAtomType.Placeholder);
              _currentLine.AppendGlyphRun(current);
              if (_currentLineIndexRange.Location == Range.UndefinedInt)
                _currentLineIndexRange = atom.IndexRange;
              else
                _currentLineIndexRange = new Range(_currentLineIndexRange.Location,
                  _currentLineIndexRange.Length + atom.IndexRange.Length);
              // add the fused atoms
              if (atom.FusedAtoms != null)
                _currentAtoms.AddRange(atom.FusedAtoms);
              else
                _currentAtoms.Add(atom);
              if (atom.Subscript != null || atom.Superscript != null) {
                var line = AddDisplayLine(true);
                if (line is null) throw new InvalidCodePathException("evenIfLengthIsZeo not respected");
                float delta = 0;
                if (atom.Nucleus != "") {
                  var glyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex
                    (_font, atom.Nucleus.Length - 1, atom.Nucleus);
                  delta = _context.MathTable.GetItalicCorrection(_styleFont, glyph);
                }
                if (delta > 0 && atom.Subscript == null)
                  // add a kern of delta
                  _currentPosition.X += delta;
                MakeScripts(atom, line, atom.IndexRange.End - 1, delta);
              }
              if (atom.AtomType == MathAtomType.Prime) continue; //preserve spacing of previous atom
              break;
            }
          case MathAtomType.RaiseBox:
            AddDisplayLine(false);
            var raiseBox = (RaiseBox)atom;
            var raisedDisplay =
              Typesetter<TFont, TGlyph>.CreateLine(raiseBox.InnerList, _font, _context, _style);
            var raisedPosition = _currentPosition;
            raisedPosition.Y += raiseBox.Raise.ActualLength(_mathTable, _font);
            raisedDisplay.Position = raisedPosition;
            _currentPosition.X += raisedDisplay.Width;
            _displayAtoms.Add(raisedDisplay);
            break;
          default:
            throw new InvalidCodePathException("Unknown atom type " + atom.AtomType);
        }
        prevNode = atom;
        prevType = atom.AtomType;
      }

      AddDisplayLine(false);
      if (_spaced && prevType != MathAtomType.MinValue) {
        var lastDisplay = _displayAtoms.LastOrDefault();
        if (lastDisplay != null) {
          //float space = GetInterElementSpace(prevType, MathAtomType.Close);
          //throw new NotImplementedException();
          ////       lastDisplay.Width += space;
        }
      }
    }

    private IDisplay<TFont, TGlyph> MakeAccent(Accent accent) {

      var accentee =
        CreateLine(accent.InnerList ?? new MathList(), _font, _context, _style, true);
      if (accent.Nucleus.Length == 0) {
        //no accent
        return accentee;
      }

      var accenteeSingleGlyph = _context.GlyphFinder.EmptyGlyph;
      if (accent.InnerList?.Atoms.Count == 1
        && accent.InnerList.Atoms[0] is MathAtom innerAtom
        && UnicodeLengthIsOne(innerAtom.Nucleus)
        && innerAtom.Superscript is null
        && innerAtom.Subscript is null) {
        // Only one single Unicode character is allowed to be an accent
        accenteeSingleGlyph =
          _context.GlyphFinder.FindGlyphForCharacterAtIndex
            (_font, innerAtom.Nucleus.Length - 1, innerAtom.Nucleus);
        if (accent.Subscript != null || accent.Superscript != null) {
          // Attach the super/subscripts to the accentee instead of the accent.
          innerAtom.Subscript = accent.Subscript;
          innerAtom.Superscript = accent.Superscript;
          accent.Subscript = null;
          accent.Superscript = null;
          // Remake the accentee (now with sub/superscripts)
          // Note: Latex adjusts the heights in case the height of the char is different
          // in non-cramped mode. However this shouldn't be the case since cramping
          // only affects fractions and superscripts. We skip adjusting the heights.
          accentee = CreateLine(accent.InnerList, _font, _context, _style, _cramped);
        }
      }

      var display = new AccentDisplay<TFont, TGlyph>(
        Typesetter<TFont, TGlyph>.CreateAccentGlyphDisplay(
          accentee, accenteeSingleGlyph,
          _context.GlyphFinder.FindGlyphForCharacterAtIndex(
            _font, accent.Nucleus.Length - 1, accent.Nucleus
          ),
          _context, _font, _styleFont, accent.IndexRange), accentee);
      // WJWJWJ -- In the display, the position is the Accentee position.
      // Is that correct, or should we be setting it here?
      // (Happypig375 edit: That should be correct but _currentPosition
      // should have been added like below.)
      display.Position = display.Position.Plus(_currentPosition);
      return display;
    }

    public static GlyphDisplay<TFont, TGlyph> CreateAccentGlyphDisplay
      (ListDisplay<TFont, TGlyph> accentee, TGlyph accenteeSingleGlyph, TGlyph accent,
       TypesettingContext<TFont, TGlyph> context, TFont normalFont, TFont styleFont, Range atomRange) {
      var accenteeWidth = accentee.Width;
      var accentGlyph =
        _FindVariantGlyph(context.MathTable, context.GlyphBoundsProvider, styleFont, accent,
          accenteeWidth, out float glyphAscent, out float glyphDescent, out float glyphWidth);
      var delta = Math.Min(accentee.Ascent, context.MathTable.AccentBaseHeight(styleFont));
      float accentAdjustment = context.MathTable.GetTopAccentAdjustment(styleFont, accentGlyph);
      float accenteeAdjustment =
        context.GlyphFinder.GlyphIsEmpty(accenteeSingleGlyph)
        ? accenteeWidth / 2
        : context.MathTable.GetTopAccentAdjustment(styleFont, accenteeSingleGlyph);
      float skew = accenteeAdjustment - accentAdjustment;
      var height = accentee.Ascent - delta;
      var accentPosition = new PointF(skew, height);
      return new GlyphDisplay<TFont, TGlyph>(accentGlyph, atomRange, styleFont) {
        Ascent = glyphAscent,
        Descent = glyphDescent,
        Width = glyphWidth,
        Position = accentPosition
      };
    }

    private static TGlyph _FindVariantGlyph(FontMathTable<TFont, TGlyph> mathTable,
      IGlyphBoundsProvider<TFont, TGlyph> boundsProvider, TFont styleFont, TGlyph rawGlyph,
      float targetWidth, out float glyphAscent, out float glyphDescent, out float glyphWidth) {
      var (glyphs, nGlyphs) = mathTable.GetHorizontalVariantsForGlyph(rawGlyph);
      if (nGlyphs == 0)
        throw new ArgumentException("There should always be at least one variant -- the glyph itself");

      var boundingBoxes = boundsProvider.GetBoundingRectsForGlyphs(styleFont, glyphs, nGlyphs);
      var (advances, _) = boundsProvider.GetAdvancesForGlyphs(styleFont, glyphs, nGlyphs);
      TGlyph currentGlyph = default!;
      // These NaN values should never be returned. We have to set them to keep the compiler happy.
      glyphAscent = float.NaN; 
      glyphDescent = float.NaN;
      glyphWidth = float.NaN;
      foreach (var (advance, bounds, glyph) in advances.Zip(boundingBoxes, glyphs, ValueTuple.Create)) {
        bounds.GetAscentDescentWidth(out float ascent, out float descent, out float _);
        var width = bounds.Right;
        if (width > targetWidth) {
          if (glyphAscent is float.NaN) {
            // glyph dimensions are not yet set
            glyphWidth = advance;
            glyphAscent = ascent;
            glyphDescent = descent;
          }
          return glyph;
        }
        else {
          currentGlyph = glyph;
          glyphWidth = advance;
          glyphAscent = ascent;
          glyphDescent = descent;
        }
      }
      return currentGlyph;
    }

    public static bool UnicodeLengthIsOne(string str) =>
      (str.Length == 1)
      || (str.Length == 2 && char.IsHighSurrogate(str[0]) && char.IsLowSurrogate(str[1]));

    private void MakeScripts(MathAtom atom, IDisplay<TFont, TGlyph> display, int index, float delta) {
      float superscriptShiftUp = 0;
      float subscriptShiftDown = 0;
      display.HasScript = true;
      if (!(display is TextLineDisplay<TFont, TGlyph>)) {
        float scriptFontSize = _mathTable.GetStyleSize(_scriptStyle, _font);
        TFont scriptFont = _context.MathFontCloner.Invoke(_font, scriptFontSize);
        superscriptShiftUp = display.Ascent - _context.MathTable.SuperscriptShiftUp(scriptFont);
        subscriptShiftDown = display.Descent + _context.MathTable.SubscriptBaselineDropMin(scriptFont);
      }
      if (atom.Superscript == null) {
        if(atom.Subscript == null)
          throw new InvalidCodePathException
            ($"MakeScripts was called when both supercript and subscript of atom were null.");
        var subscript = CreateLine(atom.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
        subscript.LinePosition = LinePosition.Subscript;
        subscript.IndexInParent = index;
        subscriptShiftDown =
          Math.Max(subscriptShiftDown, _mathTable.SubscriptShiftDown(_styleFont));
        subscriptShiftDown =
          Math.Max(subscriptShiftDown, subscript.Ascent - _mathTable.SubscriptTopMax(_styleFont));
        subscript.Position = new PointF(_currentPosition.X, _currentPosition.Y - subscriptShiftDown);
        _displayAtoms.Add(subscript);
        _currentPosition.X += subscript.Width + _mathTable.SpaceAfterScript(_styleFont);
        return;
      }

      // If we get here, superscript is not null
      var superscript =
        CreateLine(atom.Superscript, _font, _context, _scriptStyle, _superscriptCramped);
      superscript.LinePosition = LinePosition.Superscript;
      superscript.IndexInParent = index;
      superscriptShiftUp = Math.Max(superscriptShiftUp, _superscriptShiftUp);
      superscriptShiftUp = Math.Max(superscriptShiftUp,
        superscript.Descent + _mathTable.SuperscriptBottomMin(_styleFont));
      if (atom.Subscript == null) {
        superscript.Position = new PointF(_currentPosition.X, _currentPosition.Y + superscriptShiftUp);
        _displayAtoms.Add(superscript);
        _currentPosition.X += superscript.Width + _mathTable.SpaceAfterScript(_styleFont);
        return;
      }
      // If we get here, we have both a superscript and a subscript.
      var subscriptB = CreateLine(atom.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
      subscriptB.LinePosition = LinePosition.Subscript;
      subscriptB.IndexInParent = index;
      subscriptShiftDown = Math.Max(subscriptShiftDown, _mathTable.SubscriptShiftDown(_styleFont));

      // joint positioning of subscript and superscript

      var subSuperScriptGap =
        superscriptShiftUp - superscript.Descent + (subscriptShiftDown - subscriptB.Ascent);
      var gapShortfall = _mathTable.SubSuperscriptGapMin(_styleFont) - subSuperScriptGap;
      if (gapShortfall > 0) {
        subscriptShiftDown += gapShortfall;
        var superscriptBottomDelta =
          _mathTable.SuperscriptBottomMaxWithSubscript(_styleFont)
          - (superscriptShiftUp - superscript.Descent);
        if (superscriptBottomDelta > 0) {
          superscriptShiftUp += superscriptBottomDelta;
          subscriptShiftDown -= superscriptBottomDelta;
        }
      }
      // the delta is the italic correction above that shift superscript position.
      superscript.Position =
        new PointF(_currentPosition.X + delta, _currentPosition.Y + superscriptShiftUp);
      _displayAtoms.Add(superscript);
      subscriptB.Position =
        new PointF(_currentPosition.X, _currentPosition.Y - subscriptShiftDown);
      _displayAtoms.Add(subscriptB);
      _currentPosition.X +=
        Math.Max(superscript.Width + delta, subscriptB.Width)
        + _mathTable.SpaceAfterScript(_styleFont);
    }
    private float GetInterElementSpace(MathAtomType left, MathAtomType right) {
      static int GetInterElementSpaceArrayIndexForType(MathAtomType atomType, bool row) =>
        atomType switch
        {
          MathAtomType.Color => 0,
          MathAtomType.Placeholder => 0,
          MathAtomType.Ordinary => 0,
          MathAtomType.RaiseBox => 0,
          MathAtomType.LargeOperator => 1,
          MathAtomType.BinaryOperator => 2,
          MathAtomType.Relation => 3,
          MathAtomType.Open => 4,
          MathAtomType.Close => 5,
          MathAtomType.Punctuation => 6,
          MathAtomType.Fraction => 7,
          MathAtomType.Inner => 7,
          MathAtomType.Radical when row => 8,
          MathAtomType.Radical =>
            throw new InvalidCodePathException
              ("Inter-element space undefined for radical on the right. Treat radical as ordinary."),
          _ =>
            throw new InvalidCodePathException
              ($"Inter-element space undefined for atom type {atomType}")
        };
      if (right == MathAtomType.Prime)
        return 0;
      var leftIndex = GetInterElementSpaceArrayIndexForType(left, true);
      var rightIndex = GetInterElementSpaceArrayIndexForType(right, false);
      var spaceType = InterElementSpaces.Spaces[leftIndex][rightIndex];
      if (spaceType == InterElementSpaceType.Invalid)
        throw new InvalidCodePathException($"Invalid space between {left} and {right}");
      var multiplier = spaceType.SpacingInMu(_style);
      return multiplier > 0 ? multiplier * _mathTable.MuUnit(_styleFont) : 0;
    }

    private void AddInterElementSpace(MathAtom? prevNode, MathAtomType currentType) =>
      _currentPosition.X +=
        prevNode != null ? GetInterElementSpace(prevNode.AtomType, currentType)
        : _spaced ? GetInterElementSpace(MathAtomType.Open, currentType)
        : 0;
    internal TextLineDisplay<TFont, TGlyph>? AddDisplayLine(bool evenIfLengthIsZero) {
      if (evenIfLengthIsZero || (_currentLine != null && _currentLine.Length > 0)) {
        _currentLine.SetFont(_styleFont);
        var displayAtom = new TextLineDisplay<TFont, TGlyph>(
          _currentLine, _currentLineIndexRange, _context, _currentAtoms.ToArray(), _currentPosition);
        _displayAtoms.Add(displayAtom);
        _currentPosition.X += displayAtom.Width;
        _currentLine.Clear();
        _currentAtoms.Clear();
        _currentLineIndexRange = Range.NotFound;
        return displayAtom;
      }
      return null;
    }
    private float _radicalVerticalGap(TFont font) =>
      _style == LineStyle.Display
      ? _mathTable.RadicalDisplayStyleVerticalGap(font)
      : _mathTable.RadicalVerticalGap(font);
    private RadicalDisplay<TFont, TGlyph> MakeRadical(MathList radicand, Range range) {
      var innerDisplay = CreateLine(radicand, _font, _context, _style, true);
      var clearance = _radicalVerticalGap(_styleFont);
      var radicalRuleThickness = _mathTable.RadicalRuleThickness(_styleFont);
      var radicalHeight =
        innerDisplay.Ascent + innerDisplay.Descent + clearance + radicalRuleThickness;
      var glyph = _GetRadicalGlyph(radicalHeight);
      // Note this is a departure from LaTeX. LaTeX assumes that glyphAscent == thickness.
      // Open type math makes no such assumption,
      // and ascent and descent are independent of the thickness.
      // LaTeX computes delta as descent - (h(inner) + d(inner) + clearance)
      // but since we may not have ascent == thickness, we modify the delta calculation slightly.
      // If the font designer followes LaTeX conventions, it will be identical.
      var descent = glyph.Descent;
      var ascent = glyph.Ascent;
      var delta = descent + ascent
        - (innerDisplay.Ascent + innerDisplay.Descent + clearance + radicalRuleThickness);
      if (delta > 0) {
        clearance += delta / 2;
      }
      // we need to shift the radical glyph up, to coincide with the baseline of inner.
      // The new ascent of the radical glyph should be thickness + adjusted clearance + h(inner)
      var radicalAscent = radicalRuleThickness + clearance + innerDisplay.Ascent;
      // Note: if the font designer followed latex conventions,
      // this is the same as glyphAscent == thickness.
      var shiftUp = radicalAscent - ascent;
      glyph.ShiftDown = -shiftUp;

      return new RadicalDisplay<TFont, TGlyph>(innerDisplay, glyph, _currentPosition, range) {
        Ascent = radicalAscent + _mathTable.RadicalExtraAscender(_styleFont),
        TopKern = _mathTable.RadicalExtraAscender(_styleFont),
        LineThickness = radicalRuleThickness,

        Descent = Math.Max(ascent + descent - radicalAscent, innerDisplay.Descent),
        Width = glyph.Width + innerDisplay.Width
      };
    }

    private float _NumeratorShiftUp(bool hasRule) =>
      (hasRule, _style) switch
      {
        (true, LineStyle.Display) => _mathTable.FractionNumeratorDisplayStyleShiftUp(_styleFont),
        (true, _) => _mathTable.FractionNumeratorShiftUp(_styleFont),
        (false, LineStyle.Display) => _mathTable.StackTopDisplayStyleShiftUp(_styleFont),
        (false, _) => _mathTable.StackTopShiftUp(_styleFont)
      };
    private float _NumeratorGapMin =>
      _style == LineStyle.Display
      ? _mathTable.FractionNumDisplayStyleGapMin(_styleFont)
      : _mathTable.FractionNumeratorGapMin(_styleFont);

    private float _DenominatorShiftDown(bool hasRule) =>
      (hasRule, _style) switch
      {
        (true, LineStyle.Display) => _mathTable.FractionDenominatorDisplayStyleShiftDown(_styleFont),
        (true, _) => _mathTable.FractionDenominatorShiftDown(_styleFont),
        (false, LineStyle.Display) => _mathTable.StackBottomDisplayStyleShiftDown(_styleFont),
        (false, _) => _mathTable.StackBottomShiftDown(_styleFont)
      };

    private float _DenominatorGapMin =>
      _style == LineStyle.Display
      ? _mathTable.FractionDenomDisplayStyleGapMin(_styleFont)
      : _mathTable.FractionDenominatorGapMin(_styleFont);

    private float _StackGapMin =>
      _style == LineStyle.Display
      ? _mathTable.StackDisplayStyleGapMin(_styleFont)
      : _mathTable.StackGapMin(_styleFont);

    private float _FractionDelimiterHeight =>
      _style == LineStyle.Display
      ? _mathTable.FractionDelimiterDisplayStyleSize(_styleFont)
      : _mathTable.FractionDelimiterSize(_styleFont);

    private IDisplay<TFont, TGlyph> MakeFraction(Fraction fraction) {
      var numeratorDisplay =
        CreateLine(fraction.Numerator ?? new MathList(), _font, _context, _fractionStyle, false);
      var denominatorDisplay =
        CreateLine(fraction.Denominator ?? new MathList(), _font, _context, _fractionStyle, true);

      var numeratorShiftUp = _NumeratorShiftUp(fraction.HasRule);
      var denominatorShiftDown = _DenominatorShiftDown(fraction.HasRule);
      var barLocation = _mathTable.AxisHeight(_styleFont);
      var barThickness = fraction.HasRule ? _mathTable.FractionRuleThickness(_styleFont) : 0;

      if (fraction.HasRule) {
        // this is the difference between the lowest portion of
        // the numerator and the top edge of the fraction bar.
        var distanceFromNumeratorToBar =
          numeratorShiftUp - numeratorDisplay.Descent - (barLocation + barThickness / 2);
        // The distance should be at least displayGap
        if (distanceFromNumeratorToBar < _NumeratorGapMin) {
          numeratorShiftUp += (_NumeratorGapMin - distanceFromNumeratorToBar);
        }
        // now, do the same for the denominator
        var distanceFromDenominatorToBar =
          barLocation - barThickness / 2 - (denominatorDisplay.Ascent - denominatorShiftDown);
        if (distanceFromDenominatorToBar < _DenominatorGapMin) {
          denominatorShiftDown += _DenominatorGapMin - distanceFromDenominatorToBar;
        }
      } else {
        float clearance =
          numeratorShiftUp - numeratorDisplay.Descent
          - (denominatorDisplay.Ascent - denominatorShiftDown);
        float minClearance = _StackGapMin;
        if (clearance < minClearance) {
          numeratorShiftUp += (minClearance - clearance / 2);
          denominatorShiftDown += (minClearance - clearance) / 2;
        }
      }

      var display = new FractionDisplay<TFont, TGlyph>
        (numeratorDisplay, denominatorDisplay, _currentPosition, fraction.IndexRange) {
        NumeratorUp = numeratorShiftUp,
        DenominatorDown = denominatorShiftDown,
        LineThickness = barThickness,
        LinePosition = barLocation
      };
      display.UpdateNumeratorAndDenominatorPositions();

      // Add delimiters to fraction display

      if (fraction.LeftDelimiter is null && fraction.RightDelimiter is null)
        return display;
      var glyphHeight = _FractionDelimiterHeight;
      var position = new PointF();
      var innerGlyphs = new List<IDisplay<TFont, TGlyph>>();
      if (fraction.LeftDelimiter != null && fraction.LeftDelimiter != "") {
        var leftGlyph = _FindGlyphForBoundary(fraction.LeftDelimiter, glyphHeight);
        leftGlyph.Position = position;
        innerGlyphs.Add(leftGlyph);
        position.X += leftGlyph.Width;
      }
      display.Position = position;
      position.X += display.Width;
      innerGlyphs.Add(display);
      if (fraction.RightDelimiter != null && fraction.RightDelimiter != "") {
        var rightGlyph = _FindGlyphForBoundary(fraction.RightDelimiter, glyphHeight);
        rightGlyph.Position = position;
        innerGlyphs.Add(rightGlyph);
        position.X += rightGlyph.Width;
      }
      return new ListDisplay<TFont, TGlyph>(innerGlyphs) {
        Position = _currentPosition
      };
    }

    private ListDisplay<TFont, TGlyph> _MakeLeftRight(Inner inner) {
      if (inner.LeftBoundary == null && inner.RightBoundary == null) {
        throw new InvalidCodePathException("Inner should have a boundary to call this function.");
      }
      var innerListDisplay = CreateLine(inner.InnerList, _font, _context, _style, _cramped, true);
      float axisHeight = _mathTable.AxisHeight(_styleFont);
      // delta is the max distance from the axis.
      float delta =
        Math.Max(innerListDisplay.Ascent - axisHeight, innerListDisplay.Descent + axisHeight);
      var d1 = delta / 500 * _delimiterFactor;
      float d2 = 2 * delta - _delimiterShortfallPoints;
      float glyphHeight = Math.Max(d1, d2);

      var innerElements = new List<IDisplay<TFont, TGlyph>>();
      var innerPosition = new PointF();
      if (inner.LeftBoundary is Boundary left && left.Nucleus != "") {
        var leftGlyph = _FindGlyphForBoundary(left.Nucleus, glyphHeight);
        leftGlyph.Position = innerPosition;
        innerPosition.X += leftGlyph.Width;
        innerElements.Add(leftGlyph);
      }
      innerListDisplay.Position = innerPosition;
      innerPosition.X += innerListDisplay.Width;
      innerElements.Add(innerListDisplay);

      if (inner.RightBoundary is Boundary right && right.Nucleus != "") {
        var rightGlyph = _FindGlyphForBoundary(right.Nucleus, glyphHeight);
        rightGlyph.Position = innerPosition;
        innerPosition.X += rightGlyph.Width;
        innerElements.Add(rightGlyph);
      }
      return new ListDisplay<TFont, TGlyph>(innerElements);
    }

    private IGlyphDisplay<TFont, TGlyph> _FindGlyphForBoundary(
      string delimiter, float glyphHeight) {
      var leftGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, delimiter);
      var glyph = _FindGlyph(leftGlyph, glyphHeight,
        out float glyphAscent, out float glyphDescent, out float glyphWidth);
      var glyphDisplay =
        glyphAscent + glyphDescent < glyphHeight
        && _ConstructGlyph(leftGlyph, glyphHeight) is IGlyphDisplay<TFont, TGlyph> constructed
        ? constructed
        : new GlyphDisplay<TFont, TGlyph>(glyph, Range.NotFound, _styleFont) {
          Ascent = glyphAscent, // 26
          Descent = glyphDescent,// 18
          Width = glyphWidth
        };
      // Center the glyph on the axis
      var shiftDown =
        0.5f * (glyphDisplay.Ascent - glyphDisplay.Descent)
        - _mathTable.AxisHeight(_styleFont);
      glyphDisplay.ShiftDown = shiftDown;
      return glyphDisplay;
    }

    private IGlyphDisplay<TFont, TGlyph> _GetRadicalGlyph(float radicalHeight) {
#warning GlyphFinder.FindGlyph
      var radicalGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, "\u221A");
      var glyph = _FindGlyph(radicalGlyph, radicalHeight,
        out float glyphAscent, out float glyphDescent, out float glyphWidth);

      return
        glyphAscent + glyphDescent < radicalHeight
        // the glyphs are not big enough, so we construct one using extenders
        && _ConstructGlyph(radicalGlyph, radicalHeight) is IGlyphDisplay<TFont, TGlyph> constructed
        ? constructed
        : new GlyphDisplay<TFont, TGlyph>(glyph, Range.NotFound, _styleFont) {
          Ascent = glyphAscent,
          Descent = glyphDescent,
          Width = glyphWidth
        };
    }

    private GlyphConstructionDisplay<TFont, TGlyph>? _ConstructGlyph(TGlyph glyph, float glyphHeight) {
      var parts = _mathTable.GetVerticalGlyphAssembly(glyph, _styleFont);
      if (parts is null) return null;
      var glyphs = new List<TGlyph>();
      var offsets = new List<float>();
      float height = _ConstructGlyphWithParts(parts, glyphHeight, glyphs, offsets);
      using var singleGlyph = new Structures.RentedArray<TGlyph>(glyphs[0]);
      return new GlyphConstructionDisplay<TFont, TGlyph>(glyphs, offsets, _styleFont) {
        Width = _context.GlyphBoundsProvider
          .GetAdvancesForGlyphs(_styleFont, singleGlyph.Result, 1).Total,
        Ascent = height,
        Descent = 0 // it's up to the rendering to adjust the display glyph up or down
      };
    }

    private float _ConstructGlyphWithParts(IEnumerable<GlyphPart<TGlyph>> parts,
      float glyphHeight, List<TGlyph> glyphs, List<float> offsets) {
      for (int nExtenders = 0; ; nExtenders++) {
        glyphs.Clear();
        offsets.Clear();
        GlyphPart<TGlyph>? prevPart = null;
        float minDistance = _mathTable.MinConnectorOverlap(_styleFont);
        float minOffset = 0;
        float maxDelta = float.MaxValue;
        foreach (var part in parts) {
          var repeats = 1;
          if (part.IsExtender) {
            repeats = nExtenders;
          }
          for (int i=0; i<repeats; i++) {
            glyphs.Add(part.Glyph);
            if (prevPart!=null) {
              float maxOverlap = Math.Min(prevPart.EndConnectorLength, part.StartConnectorLength);
              // the minimum amount we can add to the offset
              float minOffsetDelta = prevPart.FullAdvance - maxOverlap;
              // the maximum amount we can add to the offset
              float maxOffsetDelta = prevPart.FullAdvance - minDistance;
              maxDelta = Math.Min(maxDelta, maxOffsetDelta - minOffsetDelta);
              minOffset += minOffsetDelta;
            }
            offsets.Add(minOffset);
            prevPart = part;
          }
        }
        if (prevPart == null) {
          continue; // maybe only extenders
        }
        float minHeight = minOffset + prevPart.FullAdvance;
        float maxHeight = minHeight + maxDelta * (glyphs.Count - 1);
        if (minHeight >= glyphHeight) {
          // we are done
          return minHeight;
        }
        if (glyphHeight <= maxHeight) {
          // spread the delta equally among all the connecters
          float delta = glyphHeight - minHeight;
          float dDelta = delta / (glyphs.Count - 1);
          float lastOffset = 0;
          for (int i=0; i<offsets.Count; i++) {
            float offset = offsets[i] + i * dDelta;
            offsets[i] = offset;
            lastOffset = offset;
          }
          // we are done
          return lastOffset + prevPart.FullAdvance;
        }
      }
    }

    private TGlyph _FindGlyph(TGlyph rawGlyph, float height,
      out float glyphAscent, out float glyphDescent, out float glyphWidth) {
      // in iosMath.
      glyphAscent = glyphDescent = glyphWidth = float.NaN;
      var (variants, nVariants) = _mathTable.GetVerticalVariantsForGlyph(rawGlyph);
      var rects =
        _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, variants, nVariants);
      var advances =
        _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, variants, nVariants).Advances;
      foreach (var (rect, advance, variant) in rects.Zip(advances, variants, ValueTuple.Create)) {
        rect.GetAscentDescentWidth(out glyphAscent, out glyphDescent, out glyphWidth);
        if (glyphAscent + glyphDescent >= height) {
          glyphWidth = advance;
          return variant;
        }
      }
      if (glyphAscent is float.NaN || glyphDescent is float.NaN || glyphWidth is float.NaN)
        throw new InvalidCodePathException("glyphAscent, glyphDescent or glyphWidth is NaN.");
      return variants.Last();
    }
    private List<List<ListDisplay<TFont, TGlyph>>> TypesetCells(Table table, float[] columnWidths) {
      var r = new List<List<ListDisplay<TFont, TGlyph>>>();
      foreach(var row in table.Cells) {
        var colDispalys = new List<ListDisplay<TFont, TGlyph>>();
        r.Add(colDispalys);
        for (int i=0; i<row.Count; i++) {
          var disp = Typesetter<TFont, TGlyph>.CreateLine(row[i], _font, _context, _style);
          columnWidths[i] = Math.Max(disp.Width, columnWidths[i]);
          colDispalys.Add(disp);
        }
      }
      return r;
    }
    private IDisplay<TFont, TGlyph> MakeTable(Table table) {
      int nColumns = table.NColumns;
      if (nColumns == 0 || table.NRows == 0) {
        //Empty table
        var emptyTable = new ListDisplay<TFont, TGlyph>(new IDisplay<TFont, TGlyph>[0]);
        return emptyTable;
      }
      var columnWidths = new float[nColumns];
      var displays = TypesetCells(table, columnWidths);
      var rowDisplays = new List<ListDisplay<TFont, TGlyph>>();
      foreach (var row in displays) {
        var rowDisplay = MakeRowWithColumns(row, table, columnWidths);
        rowDisplays.Add(rowDisplay);
      }

      // position all the rows
      PositionRows(rowDisplays, table);
      return new ListDisplay<TFont, TGlyph>(rowDisplays.ToArray()) {
        // Range is set here in the objective C code.
        Position = _currentPosition
      };
    }

    private ListDisplay<TFont, TGlyph> MakeRowWithColumns
      (List<ListDisplay<TFont, TGlyph>> row, Table table, float[] columnWidths) {
      float columnStart = 0;
      Range rowRange = Range.NotFound;
      for (int i=0; i<row.Count; i++) {
        var entry = row[i];
        float columnWidth = columnWidths[i];
        var alignment = table.GetAlignment(i);
        var cellPosition = columnStart;
        switch (alignment) {
          case ColumnAlignment.Right:
            cellPosition += (columnWidth - entry.Width);
            break;
          case ColumnAlignment.Center:
            cellPosition += (columnWidth - entry.Width) / 2;
            break;
        }
        entry.Position = new PointF(cellPosition, 0);
        rowRange += entry.Range;
        columnStart += (columnWidth + table.InterColumnSpacing * _mathTable.MuUnit(_styleFont));
      }
      return new ListDisplay<TFont, TGlyph>(row.ToArray());
    }

    private const float jotMultiplier = 0.3f;
    private const float lineSkipMultiplier = 0.1f;
    private const float lineSkipLimitMultiplier = 0;
    private const float baseLineSkipMultiplier = 1.2f;

    private void PositionRows(List<ListDisplay<TFont, TGlyph>> rows, Table table) {
      float currPos = 0;
      float openUp = table.InterRowAdditionalSpacing * jotMultiplier * _styleFont.PointSize;
      float baselineSkip = openUp + baseLineSkipMultiplier * _styleFont.PointSize;
      float lineSkip = openUp + lineSkipMultiplier * _styleFont.PointSize;
      float lineSkipLimit = openUp + lineSkipMultiplier * _styleFont.PointSize;
      float prevRowDescent = 0;
      float ascent = 0;
      bool first = true;
      foreach (var display in rows) {
        if (first) {
          display.Position = new PointF();
          ascent += display.Ascent;
          first = false;
        } else {
          float skip = baselineSkip;
          if (skip - (prevRowDescent + display.Ascent) < lineSkipLimit) {
            // Rows are too close together. Space them apart further.
            skip = prevRowDescent + display.Ascent + lineSkip;
          }
          currPos -= skip;
          display.Position = new PointF(0, currPos);
        }
        prevRowDescent = display.Descent;
      }

      float descent = -currPos + prevRowDescent;
      float shiftDown = 0.5f * (ascent - descent) - _mathTable.AxisHeight(_styleFont);

      foreach (var display in rows)
        display.Position = new PointF(display.Position.X, display.Position.Y - shiftDown);
    }

    private IDisplay<TFont, TGlyph> MakeLargeOperator(LargeOperator op) {
      switch (op.Nucleus.Length) {
        case 1:
          var glyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, op.Nucleus);
          if (_style == LineStyle.Display && !_context.GlyphFinder.GlyphIsEmpty(glyph))
            // Enlarge the character in display style.
            glyph = _mathTable.GetLargerGlyph(_styleFont, glyph);
          var delta = _mathTable.GetItalicCorrection(_styleFont, glyph);
          using (var glyphsArray = new Structures.RentedArray<TGlyph>(glyph)) {
            var boundingBox = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs
              (_styleFont, glyphsArray.Result, 1).Single();
            var width = _context.GlyphBoundsProvider.GetAdvancesForGlyphs
              (_styleFont, glyphsArray.Result, 1).Total;
            boundingBox.GetAscentDescentWidth(out float ascent, out float descent, out _);
            var shiftDown = 0.5 * (ascent - descent) - _mathTable.AxisHeight(_styleFont);
            var glyphDisplay = new GlyphDisplay<TFont, TGlyph>(glyph, op.IndexRange, _styleFont) {
              Ascent = ascent,
              Descent = descent,
              Width = width
            };
            if (op.Subscript != null && !(op.Limits ?? _style == LineStyle.Display))
              // remove italic correction in this case
              glyphDisplay.Width -= delta;
            glyphDisplay.ShiftDown = (float)shiftDown;
            glyphDisplay.Position = _currentPosition;
            return AddLimitsToDisplay(glyphDisplay, op, delta);
          }

        default:
          // create a regular node.
          var glyphs = _context.GlyphFinder.FindGlyphs(_font, op.Nucleus);
          var glyphRun = new AttributedGlyphRun<TFont, TGlyph>(op.Nucleus, glyphs, _styleFont);
          var run = new TextRunDisplay<TFont, TGlyph>(glyphRun, op.IndexRange, _context);
          var runs = new List<TextRunDisplay<TFont, TGlyph>> { run };
          var line = new TextLineDisplay<TFont, TGlyph>(runs, new[] { op }, _currentPosition);
          return AddLimitsToDisplay(line, op, 0);

      }
    }

    private IDisplay<TFont, TGlyph> AddLimitsToDisplay(IDisplay<TFont, TGlyph> display,
      LargeOperator op, float delta) {
      if (op.Subscript == null && op.Superscript == null) {
        _currentPosition.X += display.Width;
        return display;
      }
      if (op.Limits ?? _style == LineStyle.Display) {
        ListDisplay<TFont, TGlyph>? superscript = null;
        ListDisplay<TFont, TGlyph>? subscript = null;
        if (op.Superscript!=null) {
          superscript =
            CreateLine(op.Superscript, _font, _context, _scriptStyle, _superscriptCramped);
        }
        if (op.Subscript!=null) {
          subscript =
            CreateLine(op.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
        }
        var opsDisplay = new LargeOpLimitsDisplay<TFont, TGlyph>
          (display, superscript, subscript, delta / 2, 0) {
          Position = _currentPosition
        };
        if (superscript!=null) {
          opsDisplay.SetUpperLimitGap(
            Math.Max(_mathTable.UpperLimitGapMin(_styleFont),
                     _mathTable.UpperLimitBaselineRiseMin(_styleFont) - superscript.Descent));
        }
        if (subscript!=null) {
          opsDisplay.SetLowerLimitGap(
            Math.Max(_mathTable.LowerLimitGapMin(_styleFont),
                     _mathTable.LowerLimitBaselineDropMin(_styleFont) - subscript.Ascent));
        }
        _currentPosition.X += opsDisplay.Width;
        return opsDisplay;
      }
      _currentPosition.X += display.Width;
      MakeScripts(op, display, op.IndexRange.Location, delta);
      return display;
    }
  }
}
