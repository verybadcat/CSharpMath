using System;
using System.Collections.Generic;
using CSharpMath.Atoms;
using CSharpMath.Display;
using CSharpMath.Display.Text;
using CSharpMath.Enumerations;
using CSharpMath.FrontEnd;
using CSharpMath.Interfaces;
using Color = CSharpMath.Structures.Color;
using System.Drawing;
using System.Linq;
using CSharpMath.TypesetterInternal;

namespace CSharpMath {
  public class Typesetter<TFont, TGlyph>
    where TFont: MathFont<TGlyph> {
    internal readonly TFont _font;
    internal readonly TypesettingContext<TFont, TGlyph> _context;
    internal FontMathTable<TFont, TGlyph> _mathTable => _context.MathTable;
    internal TFont _styleFont;
    internal LineStyle _style;
    internal readonly bool _cramped;
    internal readonly bool _spaced;
    internal readonly List<IDisplay<TFont, TGlyph>> _displayAtoms = new List<IDisplay<TFont, TGlyph>>();
    internal PointF _currentPosition; // the Y axis is NOT inverted in the typesetter.
    internal readonly AttributedString<TFont, TGlyph> _currentLine;
    internal Range _currentLineIndexRange = Range.NotFoundRange;
    internal readonly List<IMathAtom> _currentAtoms = new List<IMathAtom>();
    internal const int _delimiterFactor = 901;
    internal const int _delimiterShortfallPoints = 5;

    private LineStyle _scriptStyle {
      get {
        switch (_style) {
          case LineStyle.Display:
          case LineStyle.Text:
            return LineStyle.Script;
          case LineStyle.Script:
          case LineStyle.ScriptScript:
            return LineStyle.ScriptScript;
          default:
            throw new ArgumentOutOfRangeException(nameof(_style));
        }
      }
    }

    private LineStyle _fractionStyle {
      get {
        if (_style == LineStyle.ScriptScript) {
          return _style;
        }
        return _style + 1;
      }
    }

    private bool _subscriptCramped => true;

    private bool _superscriptCramped => _cramped;

    private float _superscriptShiftUp {
      get {
        if (_cramped) {
          return _mathTable.SuperscriptShiftUpCramped(_styleFont);
        }
        return _mathTable.SuperscriptShiftUp(_styleFont);
      }
    }

    internal Typesetter(TFont font, TypesettingContext<TFont, TGlyph> context, LineStyle style, bool cramped, bool spaced) {
      _font = font;
      _context = context;
      _style = style;
      _styleFont = _context.MathFontCloner.Invoke(font, context.MathTable.GetStyleSize(style, font));
      _cramped = cramped;
      _spaced = spaced;
      _currentLine = new AttributedString<TFont, TGlyph>();
    }

    public static ListDisplay<TFont, TGlyph> CreateLine(IMathList list, TFont font, TypesettingContext<TFont, TGlyph> context, LineStyle style) {
      var finalized = list.FinalizedList();
      return _CreateLine(finalized, font, context, style, false);
    }

    private static ListDisplay<TFont, TGlyph> _CreateLine(
      IMathList list, TFont font, TypesettingContext<TFont, TGlyph> context,
      LineStyle style, bool cramped, bool spaced = false) {
      var preprocessedAtoms = _PreprocessMathList(list, context);
      var typesetter = new Typesetter<TFont, TGlyph>(font, context, style, cramped, spaced);
      typesetter._CreateDisplayAtoms(preprocessedAtoms);
      var line = new ListDisplay<TFont, TGlyph>(typesetter._displayAtoms.ToArray());
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
            throw new InvalidCodePathException($"Type {atom.AtomType} should have been removed by preprocessing");
          case MathAtomType.Boundary:
            throw new InvalidCodePathException("A boundary atom should never be inside a mathlist");
          case MathAtomType.Space:
            AddDisplayLine(false);
            var space = atom as ISpace;
            _currentPosition.X += space.ActualLength(_mathTable, _font);
            continue;
          case MathAtomType.Style:
            // stash the existing layout
            AddDisplayLine(false);
            var style = atom as IStyle;
            _style = style.LineStyle;
            _styleFont = _context.MathFontCloner.Invoke(_font, _mathTable.GetStyleSize(_style, _font));
            // We need to preserve the prevNode for any inter-element space changes,
            // so we skip to the next node.
            continue;
          case MathAtomType.Color:
            AddDisplayLine(false);
            AddInterElementSpace(prevNode, MathAtomType.Color);
            var color = atom as IColor;
            var colorDisplay = CreateLine(color.InnerList, _font, _context, _style);
            colorDisplay.SetTextColorRecursive(Color.Create(color.ColorString));
            colorDisplay.Position = _currentPosition;
            _currentPosition.X += colorDisplay.Width;
            _displayAtoms.Add(colorDisplay);
            break;
          case MathAtomType.Radical:
            AddDisplayLine(false);
            var rad = atom as IRadical;
            // Radicals are considered as Ord in rule 16.
            AddInterElementSpace(prevNode, MathAtomType.Ordinary);
            var displayRad = MakeRadical(rad.Radicand, rad.IndexRange);
            if (rad.Degree != null) {
              // add the degree to the radical
              var degree = CreateLine(rad.Degree, _font, _context, LineStyle.Script);
              displayRad.SetDegree(degree, _styleFont, _mathTable);
            }
            _displayAtoms.Add(displayRad);
            _currentPosition.X += displayRad.Width;

            if (atom.Superscript != null || atom.Subscript != null) {
              MakeScripts(atom, displayRad, rad.IndexRange.Location, 0);
            }
            break;
          case MathAtomType.Fraction:
            AddDisplayLine(false);
            var fraction = atom as IFraction;
            AddInterElementSpace(prevNode, atom.AtomType);
            var fractionDisplay = MakeFraction(fraction);
            _displayAtoms.Add(fractionDisplay);
            _currentPosition.X += fractionDisplay.Width;
            if (atom.Superscript != null || atom.Subscript != null) {
              MakeScripts(atom, fractionDisplay, fraction.IndexRange.Location, 0);
            }
            break;
          case MathAtomType.Inner:
            AddDisplayLine(false);
            AddInterElementSpace(prevNode, atom.AtomType);
            var inner = atom as IMathInner;
            ListDisplay<TFont, TGlyph> innerDisplay;
            if (inner.LeftBoundary != null || inner.RightBoundary != null) {
              innerDisplay = _MakeLeftRight(inner);
            } else {
              innerDisplay = _CreateLine(inner.InnerList, _font, _context, _style, _cramped);
            }
            innerDisplay.Position = _currentPosition;
            _currentPosition.X += innerDisplay.Width;
            _displayAtoms.Add(innerDisplay);
            if (atom.Subscript != null || atom.Superscript != null) {
              MakeScripts(atom, innerDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case MathAtomType.Group:
            AddDisplayLine(false);
            //no inserting space here as group has no inter-element space
            var group = atom as Group;
            ListDisplay<TFont, TGlyph> groupDisplay =
              _CreateLine(group.InnerList, _font, _context, _style, _cramped);
            groupDisplay.Position = _currentPosition;
            _currentPosition.X += groupDisplay.Width;
            _displayAtoms.Add(groupDisplay);
            if (atom.Subscript != null || atom.Superscript != null) {
              MakeScripts(atom, groupDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case MathAtomType.Underline:
            AddDisplayLine(false);
            // Underline is considered as Ord in rule 16.
            AddInterElementSpace(prevNode, MathAtomType.Ordinary);
            atom.AtomType = MathAtomType.Ordinary;
            var under = atom as IUnderline;
            var underlineDisplay = MakeUnderline(under);
            _displayAtoms.Add(underlineDisplay);
            _currentPosition.X += underlineDisplay.Width;
                // add super scripts || subscripts
                if (atom.Subscript != null || atom.Superscript != null) {
                    MakeScripts(atom, underlineDisplay, atom.IndexRange.Location, 0);
                }
                break;
                
          case MathAtomType.Overline:
            AddDisplayLine(false);
            // Overline is considered as Ord in rule 16.
            AddInterElementSpace(prevNode, MathAtomType.Ordinary);
            atom.AtomType = MathAtomType.Ordinary;

            var over = atom as IOverline;
            var overlineDisplay = MakeOverline(over);
            _displayAtoms.Add(overlineDisplay);
            _currentPosition.X += overlineDisplay.Width;
            // add super scripts || subscripts
            if (atom.Subscript != null || atom.Superscript != null) {
              MakeScripts(atom, overlineDisplay, atom.IndexRange.Location, 0);
            }
            break;
                
          case MathAtomType.Accent:
            AddDisplayLine(false);
            // Accent is considered as Ord in rule 16.
            AddInterElementSpace(prevNode, MathAtomType.Ordinary);
            atom.AtomType = MathAtomType.Ordinary;

            var accent = atom as IAccent;
            var accentDisplay = MakeAccent(accent);
            _displayAtoms.Add(accentDisplay);
            _currentPosition.X += accentDisplay.Width;
            // add super scripts || subscripts
            if (atom.Subscript != null || atom.Superscript != null) {
              MakeScripts(atom, accentDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case MathAtomType.Table:
            AddDisplayLine(false);
            // We will consider tables as inner
            AddInterElementSpace(prevNode, MathAtomType.Inner);
            var table = atom as Table;
            table.AtomType = MathAtomType.Inner;
            var tableDisplay = MakeTable(table);
            _displayAtoms.Add(tableDisplay);
            _currentPosition.X += tableDisplay.Width;
            break;
          case MathAtomType.LargeOperator:
            AddDisplayLine(false);
            AddInterElementSpace(prevNode, atom.AtomType);
            var op = atom as LargeOperator;
            var opDisplay = MakeLargeOperator(op);
            _displayAtoms.Add(opDisplay);
            break;
          case MathAtomType.Ordinary:
          case MathAtomType.BinaryOperator:
          case MathAtomType.Relation:
          case MathAtomType.Open:
          case MathAtomType.Close:
          case MathAtomType.Placeholder:
          case MathAtomType.Punctuation:
          case MathAtomType.Prime:
            if (prevNode != null) {
              float interElementSpace = GetInterElementSpace(prevNode.AtomType, atom.AtomType);
              if (_currentLine.Length > 0) {
                if (interElementSpace > 0) {
                  var run = _currentLine.Runs.Last();
                  run.KernedGlyphs.Last().KernAfterGlyph = interElementSpace;
                }
              } else {
                _currentPosition.X += interElementSpace;
              }
            }
            AttributedGlyphRun<TFont, TGlyph> current = null;
            var nucleusText = atom.Nucleus;
            var glyphs = _context.GlyphFinder.FindGlyphs(_font, nucleusText);
            current = AttributedGlyphRuns.Create(nucleusText, glyphs, _font, atom.AtomType == MathAtomType.Placeholder);
            _currentLine.AppendGlyphRun(current);
            if (_currentLineIndexRange.Location == Range.UndefinedInt) {
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
              var line = AddDisplayLine(true);
              float delta = 0;
              if (atom.Nucleus.IsNonEmpty()) {
                TGlyph glyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, atom.Nucleus.Length - 1, atom.Nucleus);
                delta = _context.MathTable.GetItalicCorrection(_styleFont, glyph);
              }
              if (delta > 0 && atom.Subscript == null) {
                // add a kern of delta
                _currentPosition.X += delta;
              }
              MakeScripts(atom, line, atom.IndexRange.End - 1, delta);
            }
            if (atom.AtomType == MathAtomType.Prime) continue; //preserve spacing of previous atom
            break;
          case var _ when atom is Atoms.Extension.I_ExtensionAtom ext:
            Display.Extension._Typesetter.CreateDisplayAtom(this, ext);
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

    private OverOrUnderlineDisplay<TFont, TGlyph> MakeUnderline(IUnderline underline) {
      var inner = underline.InnerList;
      var innerListDisplay = _CreateLine(inner, _font, _context, _style, _cramped);
      return new OverOrUnderlineDisplay<TFont, TGlyph>(innerListDisplay, _currentPosition) {
        LineShiftUp = -(innerListDisplay.Descent + _mathTable.UnderbarVerticalGap(_styleFont)),
        LineThickness = _mathTable.UnderbarRuleThickness(_styleFont)
      };
    }

    private OverOrUnderlineDisplay<TFont, TGlyph> MakeOverline(IOverline overline) {
      var inner = overline.InnerList;
      var innerListDisplay = _CreateLine(inner, _font, _context, _style, true);
      return new OverOrUnderlineDisplay<TFont, TGlyph>(innerListDisplay, _currentPosition) {
        LineShiftUp = innerListDisplay.Ascent + _mathTable.OverbarVerticalGap(_font)
        + _mathTable.OverbarRuleThickness(_font) + _mathTable.OverbarExtraAscender(_font),
        LineThickness = _mathTable.OverbarRuleThickness(_styleFont)
      };
    }

    private IDisplay<TFont, TGlyph> MakeAccent(IAccent accent) {

      var accentee = _CreateLine(accent.InnerList, _font, _context, _style, true);
      if (accent.Nucleus.Length == 0) {
        //no accent
        return accentee;
      }
      
      TGlyph accenteeSingleGlyph;
      GlyphDisplay<TFont, TGlyph> accentGlyphDisplay;
      var isSingleCharAccent = _IsSingleCharAccent(accent);
      if (isSingleCharAccent) {
        var innerNucleus = accent.InnerList.Atoms[0].Nucleus;
        accenteeSingleGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, innerNucleus.Length - 1, innerNucleus);
      } else accenteeSingleGlyph = _context.GlyphFinder.EmptyGlyph;
      accentGlyphDisplay = CreateAccentGlyphDisplay(accentee, accenteeSingleGlyph, _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, accent.Nucleus.Length - 1, accent.Nucleus), _context, _font, _styleFont, accent.IndexRange);
      if (isSingleCharAccent && (accent.Subscript != null || accent.Superscript != null)) {
        var innerAtom = accent.InnerList.Atoms[0];
        // Attach the super/subscripts to the accentee instead of the accent.
        innerAtom.Subscript = accent.Subscript;
        innerAtom.Superscript = accent.Superscript;
        accent.Subscript = null;
        accent.Superscript = null;
        // Remake the accentee (now with sub/superscripts)
        // Note: Latex adjusts the heights in case the height of the char is different in non-cramped mode. However this shouldn't be the case since cramping
        // only affects fractions and superscripts. We skip adjusting the heights.
        accentee = _CreateLine(accent.InnerList, _font, _context, _style, _cramped);
      }

      var display = new AccentDisplay<TFont, TGlyph>(accentGlyphDisplay, accentee);
      // WJWJWJ -- In the display, the position is the Accentee position. Is that correct, or should we be
      // setting it here? (Happypig375 edit: That should be correct but _currentPosition should have been added like below.)
      display.Position = display.Position.Plus(_currentPosition);
      return display;
    }

    public static GlyphDisplay<TFont, TGlyph> CreateAccentGlyphDisplay(ListDisplay<TFont, TGlyph> accentee, TGlyph accenteeSingleGlyph, TGlyph accent, TypesettingContext<TFont, TGlyph> context, TFont normalFont, TFont styleFont, Range atomRange) {
      var accenteeWidth = accentee.Width;
      TGlyph accentGlyph = _FindVariantGlyph(context.MathTable, context.GlyphBoundsProvider, styleFont, accent, accenteeWidth, out float glyphAscent, out float glyphDescent, out float glyphWidth);
      var delta = Math.Min(accentee.Ascent, context.MathTable.AccentBaseHeight(styleFont));
      float skew = _GetSkew(context.MathTable, context.GlyphFinder, normalFont, styleFont, accenteeWidth, accentGlyph, accenteeSingleGlyph);
      var height = accentee.Ascent - delta;
      var accentPosition = new PointF(skew, height);
      return new GlyphDisplay<TFont, TGlyph>(accentGlyph, atomRange, styleFont) {
        Ascent = glyphAscent,
        Descent = glyphDescent,
        Width = glyphWidth,
        Position = accentPosition
      };
    }


    private static float _GetSkew(FontMathTable<TFont, TGlyph> mathTable, IGlyphFinder<TFont, TGlyph> glyphFinder, TFont normalFont, TFont styleFont, float accenteeWidth, TGlyph accentGlyph, TGlyph accenteeSingleGlyph) {
      float accentAdjustment = mathTable.GetTopAccentAdjustment(styleFont, accentGlyph);
      float accenteeAdjustment;
      if (glyphFinder.GlyphIsEmpty(accenteeSingleGlyph)) {
        accenteeAdjustment = accenteeWidth / 2;
      }
      else {
        accenteeAdjustment = mathTable.GetTopAccentAdjustment(styleFont, accenteeSingleGlyph);
      }
      return accenteeAdjustment - accentAdjustment;
    }

    private static TGlyph _FindVariantGlyph(FontMathTable<TFont, TGlyph> mathTable, IGlyphBoundsProvider<TFont, TGlyph> boundsProvider, TFont styleFont, TGlyph rawGlyph, float targetWidth, out float glyphAscent, out float glyphDescent, out float glyphWidth) {
      var glyphs = mathTable.GetHorizontalVariantsForGlyph(rawGlyph);
      int nGlyphs = glyphs.Length;
      if (nGlyphs == 0) {
        throw new ArgumentException("There should always be at least one variant -- the glyph itself");
      }
      var currentGlyph = glyphs[0];

      var boundingBoxes = boundsProvider.GetBoundingRectsForGlyphs(styleFont, glyphs);
      var (Advances, _) = boundsProvider.GetAdvancesForGlyphs(styleFont, glyphs);


      glyphAscent = float.NaN;  // These NaN values should never be returned. We have to set them to keep the compiler happy.
      glyphDescent = float.NaN;
      glyphWidth = float.NaN;
      for (int i = 0; i < nGlyphs; i++) {
        var bounds = boundingBoxes[i];
        var advance = Advances[i];
        bounds.GetAscentDescentWidth(out float ascent, out float descent, out float _);
        var width = bounds.Right;
        if (width > targetWidth) {

          if (i == 0) {
            // glyph dimensions are not yet set
            glyphWidth = advance;
            glyphAscent = ascent;
            glyphDescent = descent;
          }
          return currentGlyph;
        }
        else {
          currentGlyph = glyphs[i];
          glyphWidth = advance;
          glyphAscent = ascent;
          glyphDescent = descent;

        }
      }
      return currentGlyph;
    }

    public static bool UnicodeLengthIsOne(string str) =>
      (str.Length == 1) || (str.Length == 2 && char.IsHighSurrogate(str[0]) && char.IsLowSurrogate(str[1]));

    private static bool _IsSingleCharAccent(IAccent accent) {
      if (accent.InnerList.Atoms.Count!=1) {
        return false;
      }
      var innerAtom = accent.InnerList.Atoms[0];
      // WJWJWJ (Happypig375 edit): This is the only usage of iosMath/lib/MTUnicode.h and iosMath/lib/MTUnicode.m,
      //                            I converted it into a static method named UnicodeLengthIsOne
      return UnicodeLengthIsOne(innerAtom.Nucleus) && innerAtom.Superscript is null && innerAtom.Subscript is null;
    }

    private void MakeScripts(IMathAtom atom, IDisplay<TFont, TGlyph> display, int index, float delta) {
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
          throw new InvalidCodePathException($"MakeScripts was called when both supercript and subscript of atom were null.");
        var line = display as TextLineDisplay<TFont, TGlyph>;
        var subscript = _CreateLine(atom.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
        subscript.MyLinePosition = LinePosition.Subscript;
        subscript.IndexInParent = index;
        subscriptShiftDown = Math.Max(subscriptShiftDown, _mathTable.SubscriptShiftDown(_styleFont));
        subscriptShiftDown = Math.Max(subscriptShiftDown, subscript.Ascent - _mathTable.SubscriptTopMax(_styleFont));
        subscript.Position = new PointF(_currentPosition.X, _currentPosition.Y - subscriptShiftDown);
        _displayAtoms.Add(subscript);
        _currentPosition.X += subscript.Width + _mathTable.SpaceAfterScript(_styleFont);
        return;
      }

      // If we get here, superscript is not null
      var superscript = _CreateLine(atom.Superscript, _font, _context, _scriptStyle, _superscriptCramped);
      superscript.MyLinePosition = LinePosition.Supersript;
      superscript.IndexInParent = index;
      superscriptShiftUp = Math.Max(superscriptShiftUp, _superscriptShiftUp);
      superscriptShiftUp = Math.Max(superscriptShiftUp, superscript.Descent + _mathTable.SuperscriptBottomMin(_styleFont));
      if (atom.Subscript == null) {
        superscript.Position = new PointF(_currentPosition.X, _currentPosition.Y + superscriptShiftUp);
        _displayAtoms.Add(superscript);
        _currentPosition.X += superscript.Width + _mathTable.SpaceAfterScript(_styleFont);
        return;
      }
      // If we get here, we have both a superscript and a subscript.
      var subscriptB = _CreateLine(atom.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
      subscriptB.MyLinePosition = LinePosition.Subscript;
      subscriptB.IndexInParent = index;
      subscriptShiftDown = Math.Max(subscriptShiftDown, _mathTable.SubscriptShiftDown(_styleFont));

      // joint positioning of subscript and superscript

      var subSuperScriptGap = (superscriptShiftUp - superscript.Descent + (subscriptShiftDown - subscriptB.Ascent));
      var gapShortfall = _mathTable.SubSuperscriptGapMin(_styleFont) - subSuperScriptGap;
      if (gapShortfall > 0) {
        subscriptShiftDown += gapShortfall;
        var superscriptBottomDelta = _mathTable.SuperscriptBottomMaxWithSubscript(_styleFont) - (superscriptShiftUp - superscript.Descent);
        if (superscriptBottomDelta > 0) {
          superscriptShiftUp += superscriptBottomDelta;
          subscriptShiftDown -= superscriptBottomDelta;
        }
      }
      // the delta is the italic correction above that shift superscript position.
      superscript.Position = new PointF(_currentPosition.X + delta, _currentPosition.Y + superscriptShiftUp);
      _displayAtoms.Add(superscript);
      subscriptB.Position = new PointF(_currentPosition.X, _currentPosition.Y - subscriptShiftDown);
      _displayAtoms.Add(subscriptB);
      _currentPosition.X += Math.Max(superscript.Width + delta, subscriptB.Width) + _mathTable.SpaceAfterScript(_styleFont);
    }

    private static Atoms.Color _placeholderColor => new Atoms.Color {
      ColorString = "#0000ff" // blue
    };

    private static Atoms.Color _blackColor => new Atoms.Color {
      ColorString = "#000000"
    };

    private float GetInterElementSpace(MathAtomType left, MathAtomType right) {
      //no inter-element space for group
      if (left == MathAtomType.Group || right == MathAtomType.Group || right == MathAtomType.Prime)
        return 0;
      var leftIndex = GetInterElementSpaceArrayIndexForType(left, true);
      var rightIndex = GetInterElementSpaceArrayIndexForType(right, false);
      var spaces = InterElementSpaces.Spaces;
      var spaceArray = spaces[leftIndex];
      var spaceType = spaceArray[rightIndex];
      if (spaceType == InterElementSpaceType.Invalid)
        throw new InvalidCodePathException($"Invalid space between {left} and {right}");
      var multiplier = spaceType.SpacingInMu(_style);
      if (multiplier > 0) {
        return multiplier * _mathTable.MuUnit(_styleFont);
      }
      return 0;
    }

    private void AddInterElementSpace(IMathAtom prevNode, MathAtomType currentType) {
      float space = 0;
      if (prevNode != null) {
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
          throw new InvalidCodePathException("Inter-element space undefined for radical on the right. Treat radical as ordinary.");
      }
      var extResult = Display.Extension._Typesetter.GetInterElementSpaceArrayIndexForType(atomType);
      if (extResult != -1) return extResult;
      throw new InvalidCodePathException($"Inter-element space undefined for atom type {atomType}");
    }

    internal TextLineDisplay<TFont, TGlyph> AddDisplayLine(bool evenIfLengthIsZero) {
      if (evenIfLengthIsZero || (_currentLine != null && _currentLine.Length > 0)) {
        _currentLine.SetFont(_styleFont);
        var displayAtom = TextLineDisplays.Create(_currentLine, _currentLineIndexRange, _context, _currentAtoms);
        displayAtom.Position = _currentPosition;
        _displayAtoms.Add(displayAtom);
        _currentPosition.X += displayAtom.Width;
        _currentLine.Clear();
        _currentAtoms.Clear();
        _currentLineIndexRange = Ranges.NotFound;
        return displayAtom;
      }
      return null;
    }

    private static List<IMathAtom> _PreprocessMathList(IMathList list, TypesettingContext<TFont, TGlyph> context) {
      IMathAtom prevNode = null;
      var r = new List<IMathAtom>();
      foreach (IMathAtom atom in list.Atoms) {
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
    [Obsolete("Is any code using this?", true)]
    private string _ChangeFont(string input, FontStyle style) {
      return _context.ChangeFont(input, style);
    }

    private float _radicalVerticalGap(TFont font) =>
      _style == LineStyle.Display ? _mathTable.RadicalDisplayStyleVerticalGap(font) : _mathTable.RadicalVerticalGap(font);

    private RadicalDisplay<TFont, TGlyph> MakeRadical(IMathList radicand, Range range) {
      var innerDisplay = _CreateLine(radicand, _font, _context, _style, true);
      var clearance = _radicalVerticalGap(_styleFont);
      var radicalRuleThickness = _mathTable.RadicalRuleThickness(_styleFont);
      var radicalHeight = innerDisplay.Ascent + innerDisplay.Descent + clearance + radicalRuleThickness;

      IDownshiftableDisplay<TFont, TGlyph> glyph = _GetRadicalGlyph(radicalHeight);
      // Note this is a departure from Latex. Latex assumes that glyphAscent == thickness.
      // Open type math makes no such assumption, and ascent and descent are independent of the thickness.
      // Latex computes delta as descent - (h(inner) + d(inner) + clearance)
      // but since we may not have ascent == thickness, we modify the delta calculation slightly.
      // If the font designer followes Latex conventions, it will be identical.
      var descent = glyph.Descent;
      var ascent = glyph.Ascent;
      var delta = (glyph.Descent + glyph.Ascent) - (innerDisplay.Ascent + innerDisplay.Descent + clearance + radicalRuleThickness);
      if (delta > 0) {
        clearance += delta / 2;
      }
      // we need to shift the radical glyph up, to coincide with the baseline of inner.
      // The new ascent of the radical glyph should be thickness + adjusted clearance + h(inner)
      var radicalAscent = radicalRuleThickness + clearance + innerDisplay.Ascent;
      var shiftUp = radicalAscent - glyph.Ascent;   // Note: if the font designer followed latex conventions, this is the same as glyphAscent == thickness.
      glyph.ShiftDown = -shiftUp;

      var radical = new RadicalDisplay<TFont, TGlyph>(innerDisplay, glyph, _currentPosition, range) {
        Ascent = radicalAscent + _mathTable.RadicalExtraAscender(_styleFont),
        TopKern = _mathTable.RadicalExtraAscender(_styleFont),
        LineThickness = radicalRuleThickness,

        Descent = Math.Max(glyph.Ascent + glyph.Descent - radicalAscent, innerDisplay.Descent),
        Width = glyph.Width + innerDisplay.Width
      };
      return radical;
    }

    private float _NumeratorShiftUp(bool hasRule) {
      if (hasRule) {
        if (_style == LineStyle.Display) {
          return _mathTable.FractionNumeratorDisplayStyleShiftUp(_styleFont);
        }
        return _mathTable.FractionNumeratorShiftUp(_styleFont);
      }
      if (_style == LineStyle.Display) {
        return _mathTable.StackTopDisplayStyleShiftUp(_styleFont);
      }
      return _mathTable.StackTopShiftUp(_styleFont);
    }

    private float _NumeratorGapMin {
      get {
        if (_style == LineStyle.Display) {
          return _mathTable.FractionNumDisplayStyleGapMin(_styleFont);
        }
        return _mathTable.FractionNumeratorGapMin(_styleFont);
      }
    }

    private float _DenominatorShiftDown(bool hasRule) {
      if (hasRule) {
        if (_style == LineStyle.Display) {
          return _mathTable.FractionDenominatorDisplayStyleShiftDown(_styleFont);
        }
        return _mathTable.FractionDenominatorShiftDown(_styleFont);
      }
      if (_style == LineStyle.Display) {
        return _mathTable.StackBottomDisplayStyleShiftDown(_styleFont);
      }
      return _mathTable.StackBottomShiftDown(_styleFont);
    }

    private float _DenominatorGapMin {
      get {
        if (_style == LineStyle.Display) {
          return _mathTable.FractionDenomDisplayStyleGapMin(_styleFont);
        }
        return _mathTable.FractionDenominatorGapMin(_styleFont);
      }
    }

    private float _StackGapMin {
      get {
        if (_style == LineStyle.Display) {
          return _mathTable.StackDisplayStyleGapMin(_styleFont);
        }
        return _mathTable.StackGapMin(_styleFont);
      }
    }

    private float _FractionDelimiterHeight {
      get {
        if (_style == LineStyle.Display) {
          return _mathTable.FractionDelimiterDisplayStyleSize(_styleFont);
        }
        return _mathTable.FractionDelimiterSize(_styleFont);
      }
    }

    private IDisplay<TFont, TGlyph> MakeFraction(IFraction fraction) {
      var numeratorDisplay = _CreateLine(fraction.Numerator, _font, _context, _fractionStyle, false);
      var denominatorDisplay = _CreateLine(fraction.Denominator, _font, _context, _fractionStyle, true);

      var numeratorShiftUp = _NumeratorShiftUp(fraction.HasRule);
      var denominatorShiftDown = _DenominatorShiftDown(fraction.HasRule);
      var barLocation = _mathTable.AxisHeight(_styleFont);
      var barThickness = (fraction.HasRule) ? _mathTable.FractionRuleThickness(_styleFont) : 0;

      if (fraction.HasRule) {
        // this is the difference between the lowest portion of the numerator and the top edge of the fraction bar.
        var distanceFromNumeratorToBar = (numeratorShiftUp - numeratorDisplay.Descent) - (barLocation + barThickness / 2);
        // The distance should be at least displayGap
        if (distanceFromNumeratorToBar < _NumeratorGapMin) {
          numeratorShiftUp += (_NumeratorGapMin - distanceFromNumeratorToBar);
        }
        // now, do the same for the denominator
        var distanceFromDenominatorToBar = (barLocation - barThickness / 2) - (denominatorDisplay.Ascent - denominatorShiftDown);
        if (distanceFromDenominatorToBar < _DenominatorGapMin) {
          denominatorShiftDown += (_DenominatorGapMin - distanceFromDenominatorToBar);
        }
      } else {
        float clearance = (numeratorShiftUp - numeratorDisplay.Descent) - (denominatorDisplay.Ascent - denominatorShiftDown);
        float minClearance = _StackGapMin;
        if (clearance < minClearance) {
          numeratorShiftUp += (minClearance - clearance / 2);
          denominatorShiftDown += (minClearance - clearance) / 2;
        }
      }

      var display = new FractionDisplay<TFont, TGlyph>(numeratorDisplay, denominatorDisplay, _currentPosition, fraction.IndexRange) {
        NumeratorUp = numeratorShiftUp,
        DenominatorDown = denominatorShiftDown,
        LineThickness = barThickness,
        LinePosition = barLocation
      };
      display.UpdateNumeratorAndDenominatorPositions();

      if (fraction.LeftDelimiter == null && fraction.RightDelimiter == null) {
        return display;
      }
      return _AddDelimitersToFractionDisplay(display, fraction);
    }

    private ListDisplay<TFont, TGlyph> _AddDelimitersToFractionDisplay(FractionDisplay<TFont, TGlyph> display, IFraction fraction) {
      var glyphHeight = _FractionDelimiterHeight;
      var position = new PointF();
      var innerGlyphs = new List<IDisplay<TFont, TGlyph>>();
      if (fraction.LeftDelimiter.IsNonEmpty()) {
        var leftGlyph = _FindGlyphForBoundary(fraction.LeftDelimiter, glyphHeight);
        leftGlyph.Position = position;
        innerGlyphs.Add(leftGlyph);
        position.X += leftGlyph.Width;
      }
      display.Position = position;
      position.X += display.Width;
      innerGlyphs.Add(display);
      if (fraction.RightDelimiter.IsNonEmpty()) {
        var rightGlyph = _FindGlyphForBoundary(fraction.RightDelimiter, glyphHeight);
        rightGlyph.Position = position;
        innerGlyphs.Add(rightGlyph);
        position.X += rightGlyph.Width;
      }
      var innerDisplay = new ListDisplay<TFont, TGlyph>(innerGlyphs.ToArray()) {
        Position = _currentPosition
      };
      return innerDisplay;
    }

    private ListDisplay<TFont, TGlyph> _MakeLeftRight(IMathInner inner) {
      if (inner.LeftBoundary == null && inner.RightBoundary == null) {
        throw new InvalidOperationException("Inner should have a boundary to call this function.");
      }
      var innerListDisplay = _CreateLine(inner.InnerList, _font, _context, _style, _cramped, true);
      float axisHeight = _mathTable.AxisHeight(_styleFont);
      // delta is the max distance from the axis.
      float delta = Math.Max(innerListDisplay.Ascent - axisHeight, innerListDisplay.Descent + axisHeight);
      var d1 = (delta / 500) * _delimiterFactor;
      float d2 = 2 * delta - _delimiterShortfallPoints;
      float glyphHeight = Math.Max(d1, d2);

      var innerElements = new List<IDisplay<TFont, TGlyph>>();
      var innerPosition = new PointF();
      if (inner.LeftBoundary != null && inner.LeftBoundary.Nucleus.IsNonEmpty()) {
        var leftGlyph = _FindGlyphForBoundary(inner.LeftBoundary.Nucleus, glyphHeight);
        leftGlyph.Position = innerPosition;
        innerPosition.X += leftGlyph.Width;
        innerElements.Add(leftGlyph);
      }
      innerListDisplay.Position = innerPosition;
      innerPosition.X += innerListDisplay.Width;
      innerElements.Add(innerListDisplay);

      if (inner.RightBoundary != null && inner.RightBoundary.Nucleus.Length > 0) {
        var rightGlyph = _FindGlyphForBoundary(inner.RightBoundary.Nucleus, glyphHeight);
        rightGlyph.Position = innerPosition;
        innerPosition.X += rightGlyph.Width;
        innerElements.Add(rightGlyph);
      }
      var innerArrayDisplay = new ListDisplay<TFont, TGlyph>(innerElements.ToArray());
      return innerArrayDisplay;
    }

    private Range _RangeOfComposedCharacterSequenceAtIndex(int index) {
      // This will likely change once we start dealing with fonts and weird characters.
      return new Range(index, 1);
    }


    private IDownshiftableDisplay<TFont, TGlyph> _FindGlyphForBoundary(string delimiter, float glyphHeight) {
      TGlyph leftGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, delimiter);
      TGlyph glyph = _FindGlyph(leftGlyph, glyphHeight, out float glyphAscent, out float glyphDescent, out float glyphWidth);
      IDownshiftableDisplay<TFont, TGlyph> glyphDisplay = null;
      if (glyphAscent + glyphDescent < glyphHeight) {
        glyphDisplay = _ConstructGlyph(leftGlyph, glyphHeight);
      }
      if (glyphDisplay == null) {
        glyphDisplay = new GlyphDisplay<TFont, TGlyph>(glyph, Range.NotFoundRange, _styleFont) {
          Ascent = glyphAscent, // 26
          Descent = glyphDescent,// 18
          Width = glyphWidth
        };
      }
      // Center the glyph on the axis
      var shiftDown = 0.5f * (glyphDisplay.Ascent - glyphDisplay.Descent) - _mathTable.AxisHeight(_styleFont);
      glyphDisplay.ShiftDown = shiftDown;
      return glyphDisplay;
    }

    private IDownshiftableDisplay<TFont, TGlyph> _GetRadicalGlyph(float radicalHeight) {
      TGlyph radicalGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, Constants.Symbols.SquareRoot);
      TGlyph glyph = _FindGlyph(radicalGlyph, radicalHeight, out float glyphAscent, out float glyphDescent, out float glyphWidth);

      IDownshiftableDisplay<TFont, TGlyph> glyphDisplay = null;
      if (glyphAscent + glyphDescent < radicalHeight) {
        // the glyphs are not big enough, so we construct one using extenders
        glyphDisplay = _ConstructGlyph(radicalGlyph, radicalHeight);
      }
      if (glyphDisplay == null) {
        glyphDisplay = new GlyphDisplay<TFont, TGlyph>(glyph, Range.NotFoundRange, _styleFont) {
          Ascent = glyphAscent,
          Descent = glyphDescent,
          Width = glyphWidth
        };
      }
      return glyphDisplay;
    }

    private GlyphConstructionDisplay<TFont, TGlyph> _ConstructGlyph(TGlyph glyph, float glyphHeight) {
      GlyphPart<TGlyph>[] parts = _mathTable.GetVerticalGlyphAssembly(glyph, _styleFont);
      if (parts.IsEmpty()) {
        return null;
      }
      List<TGlyph> glyphs = new List<TGlyph>();
      List<float> offsets = new List<float>();
      float height = _ConstructGlyphWithParts(parts, glyphHeight, glyphs, offsets);
      TGlyph firstGlyph = glyphs[0];
      float width = _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, new TGlyph[] { firstGlyph }).Total;
      var display = new GlyphConstructionDisplay<TFont, TGlyph>(glyphs, offsets, _styleFont) {
        Width = width,
        Ascent = height,
        Descent = 0 // it's up to the rendering to adjust the display glyph up or down
      };
      return display;
    }

    private float _ConstructGlyphWithParts(GlyphPart<TGlyph>[] parts, float glyphHeight, List<TGlyph> glyphs, List<float> offsets) {
      for (int nExtenders = 0; ; nExtenders++) {
        glyphs.Clear();
        offsets.Clear();
        GlyphPart<TGlyph> prevPart = null;
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
              minOffset = minOffset + minOffsetDelta;
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
      TGlyph[] variants = _mathTable.GetVerticalVariantsForGlyph(rawGlyph);
      var nVariants = variants.Length;
      var glyph = rawGlyph;
      var rects = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, variants);
      var advances = _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, variants).Advances;
      int i = 0;
      do {
        var rect = rects[i];
        rect.GetAscentDescentWidth(out glyphAscent, out glyphDescent, out glyphWidth);
        if (glyphAscent + glyphDescent >= height) {
          glyphWidth = advances[i];
          return variants[i];
        }
        i++;
      } while (i < nVariants);
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
        ListDisplay<TFont, TGlyph> emptyTable = new ListDisplay<TFont, TGlyph>(new IDisplay<TFont, TGlyph>[0]) {
        };
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

    private ListDisplay<TFont, TGlyph> MakeRowWithColumns(List<ListDisplay<TFont, TGlyph>> row, Table table, float[] columnWidths) {
      float columnStart = 0;
      Range rowRange = Ranges.NotFound;
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
        rowRange = Ranges.Union(rowRange, entry.Range);
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

      foreach (var display in rows) {
        display.Position = new PointF(display.Position.X, display.Position.Y - shiftDown);
      }
    }

    private IDisplay<TFont, TGlyph> MakeLargeOperator(LargeOperator op) {
      bool limits = op.Limits ?? _style == LineStyle.Display;
      float delta = 0;
      if (op.Nucleus.Length == 1) {
        var glyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, op.Nucleus);
        if (_style == LineStyle.Display && !(_context.GlyphFinder.GlyphIsEmpty(glyph))) {
          // Enlarge the character in display style.
          glyph = _mathTable.GetLargerGlyph(_styleFont, glyph);
        }
        delta = _mathTable.GetItalicCorrection(_styleFont, glyph);
        var glyphArray = new TGlyph[] { glyph };
        var boundingBoxArray = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, glyphArray);
        var boundingBox = boundingBoxArray[0];
        var width = _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, glyphArray).Advances[0];
        boundingBox.GetAscentDescentWidth(out float ascent, out float descent, out float _);
        var shiftDown = 0.5 * (ascent - descent) - _mathTable.AxisHeight(_styleFont);
        var glyphDisplay = new GlyphDisplay<TFont, TGlyph>(glyph, op.IndexRange, _styleFont) {
          Ascent = ascent,
          Descent = descent,
          Width = width
        };
        if (op.Subscript!=null && !limits) {
          // remove italic correction in this case
          glyphDisplay.Width -= delta;
        }
        glyphDisplay.ShiftDown = (float)shiftDown;
        glyphDisplay.Position = _currentPosition;
        return AddLimitsToDisplay(glyphDisplay, op, delta);
      } else {
        // create a regular node.
        var glyphs = _context.GlyphFinder.FindGlyphs(_font, op.Nucleus);
        var glyphRun = AttributedGlyphRuns.Create(op.Nucleus, glyphs, _styleFont, false);
        var run = new TextRunDisplay<TFont, TGlyph>(glyphRun, op.IndexRange, _context);
        var runs = new List<TextRunDisplay<TFont, TGlyph>>{ run };
        var atoms = new List<IMathAtom> { op };
        var line = new TextLineDisplay<TFont, TGlyph>(runs, atoms) {
          Position = _currentPosition
        };
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
        ListDisplay<TFont, TGlyph> superscript = null;
        ListDisplay<TFont, TGlyph> subscript = null;
        if (op.Superscript!=null) {
          superscript = _CreateLine(op.Superscript, _font, _context, _scriptStyle, _superscriptCramped);
        }
        if (op.Subscript!=null) {
          subscript = _CreateLine(op.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
        }
        var opsDisplay = new LargeOpLimitsDisplay<TFont, TGlyph>(display, superscript, subscript, delta / 2, 0) {
          Position = _currentPosition
        };
        if (superscript!=null) {
          var upperGap = Math.Max(_mathTable.UpperLimitGapMin(_styleFont),
                                  _mathTable.UpperLimitBaselineRiseMin(_styleFont)-superscript.Descent);
          opsDisplay.SetUpperLimitGap(upperGap);
        }
        if (subscript!=null) {
          var lowerGap = Math.Max(_mathTable.LowerLimitGapMin(_styleFont), 
                                  _mathTable.LowerLimitBaselineDropMin(_styleFont) - subscript.Ascent);
          opsDisplay.SetLowerLimitGap(lowerGap);
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
