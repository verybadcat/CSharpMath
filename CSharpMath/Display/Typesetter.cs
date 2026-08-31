using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Atom;
using CSharpMath.Atom.Atoms;
using CSharpMath.Display.Displays;
using CSharpMath.Display.FrontEnd;

namespace CSharpMath.Display {
  public static class Typesetter {
    public static ListDisplay<TFont, TGlyph> CreateLine<TFont, TGlyph>
      (MathList list, TFont font, TypesettingContext<TFont, TGlyph> context, LineStyle style)
      where TFont : IFont<TGlyph> =>
      list is null ? throw new ArgumentNullException(nameof(list))
      : Typesetter<TFont, TGlyph>.CreateLine(list.Clone(true), font, context, style, false);
    public static bool UnicodeLengthIsOne(string? str) => str?.Length switch {
      1 => true,
      2 when char.IsHighSurrogate(str[0]) && char.IsLowSurrogate(str[1]) => true,
      _ => false
    };
    internal static MathAtom SpacingAtom(MathAtom atom) => atom switch {
      LargeDelimiter large => SpacingAtom(large.Nucleus, large.MathClass),
      Stack stack => SpacingAtom(stack.Nucleus, stack.DisplayClassType),
      _ => atom
    };
    private static MathAtom SpacingAtom(string nucleus, Type mathClass) =>
      mathClass == typeof(Open) ? new Open(nucleus) :
      mathClass == typeof(Close) ? new Close(nucleus) :
      mathClass == typeof(Relation) ? new Relation(nucleus) :
      mathClass == typeof(BinaryOperator) ? new BinaryOperator(nucleus) :
      new Ordinary(nucleus);
    private static TGlyph FindVariantGlyph<TFont, TGlyph>(FontMathTable<TFont, TGlyph> mathTable,
      IGlyphBoundsProvider<TFont, TGlyph> boundsProvider, TFont styleFont, TGlyph rawGlyph,
      float targetWidth, out float glyphAscent, out float glyphDescent, out float glyphWidth)
      where TFont : IFont<TGlyph> {
      var (glyphs, nGlyphs) = mathTable.GetHorizontalVariantsForGlyph(rawGlyph);
      if (nGlyphs == 0)
        throw new InvalidCodePathException("Incorrect GetHorizontalVariantsForGlyph implementation. " +
          "There should always be at least one variant -- the glyph itself");

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
        } else {
          currentGlyph = glyph;
          glyphWidth = advance;
          glyphAscent = ascent;
          glyphDescent = descent;
        }
      }
      return currentGlyph;
    }
    public static GlyphDisplay<TFont, TGlyph> CreateAccentGlyphDisplay<TFont, TGlyph>
      (ListDisplay<TFont, TGlyph> accentee, TGlyph accenteeSingleGlyph, TGlyph accent,
       TypesettingContext<TFont, TGlyph> context, TFont styleFont, Range atomRange)
      where TFont : IFont<TGlyph> {
      if (accentee is null) throw new ArgumentNullException(nameof(accentee));
      if (context is null) throw new ArgumentNullException(nameof(context));
      var accenteeWidth = accentee.Width;
      var accentGlyph =
        FindVariantGlyph(context.MathTable, context.GlyphBoundsProvider, styleFont, accent,
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
      return new GlyphDisplay<TFont, TGlyph>(
        accentGlyph, atomRange, styleFont, glyphAscent, glyphDescent, glyphWidth) {
        Position = accentPosition
      };
    }
  }
  public class Typesetter<TFont, TGlyph> where TFont : IFont<TGlyph> {
    internal readonly TFont _font;
    internal readonly TypesettingContext<TFont, TGlyph> _context;
    internal readonly FontMathTable<TFont, TGlyph> _mathTable;
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
    private LineStyle _scriptStyle => _style switch {
      LineStyle.Display => LineStyle.Script,
      LineStyle.Text => LineStyle.Script,
      LineStyle.Script => LineStyle.ScriptScript,
      LineStyle.ScriptScript => LineStyle.ScriptScript,
      _ => throw new
        System.ComponentModel.InvalidEnumArgumentException(nameof(_style), (int)_style, typeof(LineStyle))
    };
    private LineStyle _fractionStyle => _style == LineStyle.ScriptScript ? _style : _style + 1;
    private const bool _subscriptCramped = true;
    private bool _superscriptCramped => _cramped;
    private float _superscriptShiftUp =>
      _cramped
      ? _mathTable.SuperscriptShiftUpCramped(_styleFont)
      : _mathTable.SuperscriptShiftUp(_styleFont);
    internal Typesetter(TFont font, TypesettingContext<TFont, TGlyph> context,
      LineStyle style, bool cramped, bool spaced) {
      _font = font;
      _context = context;
      _mathTable = context.MathTable;
      _style = style;
      _styleFont = _context.MathFontCloner.Invoke(font, context.MathTable.GetStyleSize(style, font));
      _cramped = cramped;
      _spaced = spaced;
      _currentLine = new AttributedString<TFont, TGlyph>();
    }
    internal static ListDisplay<TFont, TGlyph> CreateLine(
      MathList list, TFont font, TypesettingContext<TFont, TGlyph> context,
      LineStyle style, bool cramped, bool spaced = false) {
      // NOTE: The 3 atom types that use continue; below, aka [Comment, Space, Style], correspond to non-displayed atom types
      // in MathList.Clone(true). Update that if-condition and add a test in Issue213() if more such atom types are added.
      // Otherwise, using these atoms between = (Relation) and - (BinaryOperator) will cause an exception from invalid spacing.

      List<MathAtom> _PreprocessMathList() {
        MathAtom? prevAtom = null;
        var r = new List<MathAtom>();
        foreach (var atom in list.Atoms) {
          if (atom is Comment) continue;
          // These are not a TeX type nodes. TeX does this during parsing the input.
          // switch to using the font specified in the atom and convert it to ordinary
          var newAtom = atom switch {
            Variable v => v.ToOrdinary(UnicodeFontChanger.ChangeFont),
            Number n => n.ToOrdinary(UnicodeFontChanger.ChangeFont),
            // TeX treats unary operators as Ordinary. So will we.
            UnaryOperator u => u.ToOrdinary(),
            _ => atom
          };
          // This is Rule 14 to merge ordinary characters, but only within one font
          // style: the fused run is stamped with a single face (iosMath 76fd773).
          if (newAtom is Ordinary && prevAtom is Ordinary o && o.Superscript.IsEmpty() && o.Subscript.IsEmpty()
            && o.FontStyle == newAtom.FontStyle) {
            prevAtom.Fuse(newAtom);
            // skip the current node as we fused it
            continue;
          }
          // TODO: add italic correction here or in second pass?
          prevAtom = newAtom;
          r.Add(newAtom);
        }
        return r;
      }
      var typesetter = new Typesetter<TFont, TGlyph>(font, context, style, cramped, spaced);
      typesetter.CreateDisplayAtoms(_PreprocessMathList());
      return new ListDisplay<TFont, TGlyph>(typesetter._displayAtoms.ToArray());
    }
    private void CreateDisplayAtoms(List<MathAtom> preprocessedAtoms) {
      MathAtom? prevAtom = null;
      foreach (var atom in preprocessedAtoms) {
        switch (atom) {
          case Number _:
          case Variable _:
          case UnaryOperator _:
          case Comment _:
            throw new InvalidCodePathException
              ($"Type {atom.TypeName} should have been removed by preprocessing");
          case Space space:
            AddDisplayLine(false);
            _currentPosition.X += space.ActualLength(_mathTable, _font);
            continue;
          case Style style:
            // stash the existing layout
            AddDisplayLine(false);
            _style = style.LineStyle;
            _styleFont =
              _context.MathFontCloner.Invoke(_font, _mathTable.GetStyleSize(_style, _font));
            // We need to preserve the prevAtom for any inter-element space changes,
            // so we skip to the next node.
            continue;
          case Colored colored:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, colored);
            var colorDisplay = CreateLine(colored.InnerList, _font, _context, _style, false);
            colorDisplay.SetTextColorRecursive(colored.Color);
            colorDisplay.Position = _currentPosition;
            _currentPosition.X += colorDisplay.Width;
            _displayAtoms.Add(colorDisplay);
            break;
          case ColorBox colorBox:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, colorBox);
            colorDisplay = CreateLine(colorBox.InnerList, _font, _context, _style, false);
            colorDisplay.BackColor = colorBox.Color;
            colorDisplay.Position = _currentPosition;
            _currentPosition.X += colorDisplay.Width;
            _displayAtoms.Add(colorDisplay);
            break;
          case Group group:
            AddDisplayLine(false);
            // Spaced as Ordinary; lay out the sub-mlist with a fresh recursion so any
            // interior style node is scoped to the group. Keep the nested ListDisplay
            // as the group's composite nucleus so scripts use the complete box metrics.
            AddInterElementSpace(prevAtom, group);
            var groupInnerDisplay =
              CreateLine(group.InnerList, _font, _context, _style, _cramped);
            groupInnerDisplay.Position = _currentPosition;
            groupInnerDisplay.SetRangeOverride(atom.IndexRange);
            _displayAtoms.Add(groupInnerDisplay);
            _currentPosition.X += groupInnerDisplay.Width;
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              // Scripts attach after the whole group.
              MakeScripts(atom, groupInnerDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Box box:
            AddDisplayLine(false);
            // Box spacing class is Ordinary.
            AddInterElementSpace(prevAtom, box);
            var boxChildDisplay = CreateLine(box.InnerList, _font, _context, _style, false);
            var boxDisplay = new BoxDisplay<TFont, TGlyph>(boxChildDisplay, box.KeepWidth,
              box.KeepHeight, box.KeepDepth, box.DrawChild, box.HAlign, box.StrikeStyle,
              _mathTable.FractionRuleThickness(_styleFont),
              0.55f * _mathTable.AccentBaseHeight(_styleFont),
              atom.IndexRange) {
              Position = _currentPosition
            };
            _displayAtoms.Add(boxDisplay);
            _currentPosition.X += boxDisplay.Width;
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, boxDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Stack stack:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, stack);
            var stackDisplay = MakeStack(stack, atom.IndexRange);
            _displayAtoms.Add(stackDisplay);
            _currentPosition.X += stackDisplay.Width;
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, stackDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Radical rad:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, rad);
            var displayRad = MakeRadical(rad.Radicand, rad.IndexRange);
            if (rad.Degree.IsNonEmpty()) {
              // add the degree to the radical
              displayRad.SetDegree(
                CreateLine(rad.Degree, _styleFont, _context, LineStyle.Script, false),
                _styleFont, _mathTable);
            }
            _displayAtoms.Add(displayRad);
            _currentPosition.X += displayRad.Width;

            if (atom.Superscript.IsNonEmpty() || atom.Subscript.IsNonEmpty()) {
              MakeScripts(atom, displayRad, rad.IndexRange.Location, 0);
            }
            break;
          case Fraction fraction:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, fraction);
            var fractionDisplay = MakeFraction(fraction);
            _displayAtoms.Add(fractionDisplay);
            _currentPosition.X += fractionDisplay.Width;
            if (atom.Superscript.IsNonEmpty() || atom.Subscript.IsNonEmpty()) {
              MakeScripts(atom, fractionDisplay, fraction.IndexRange.Location, 0);
            }
            break;
          case Inner inner:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, inner);
            IDisplay<TFont, TGlyph> innerDisplay;
            if (inner.LeftBoundary != Boundary.Empty || inner.RightBoundary != Boundary.Empty) {
              innerDisplay = MakeInner(inner, atom.IndexRange);
            } else {
              innerDisplay = CreateLine(inner.InnerList, _font, _context, _style, _cramped);
            }
            innerDisplay.Position = _currentPosition;
            _currentPosition.X += innerDisplay.Width;
            _displayAtoms.Add(innerDisplay);
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, innerDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Underline underline:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, underline);
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
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, underlineDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Overline overline:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, overline);
            innerListDisplay = Typesetter<TFont, TGlyph>.CreateLine
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
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, overlineDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case UnderAnnotation underAnnotation:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, underAnnotation);
            innerListDisplay = Typesetter<TFont, TGlyph>.CreateLine
              (underAnnotation.InnerList, _font, _context, _style, true);
            var underAnnotationDisplay = MakeUnderAnnotation(underAnnotation, atom.IndexRange);
            _displayAtoms.Add(underAnnotationDisplay);
            _currentPosition.X += underAnnotationDisplay.Width;
            // add super scripts || subscripts
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, underAnnotationDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Accent accent:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, accent);

            var accentDisplay = MakeAccent(accent);
            _displayAtoms.Add(accentDisplay);
            _currentPosition.X += accentDisplay.Width;
            // add super scripts || subscripts
            if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
              MakeScripts(atom, accentDisplay, atom.IndexRange.Location, 0);
            }
            break;
          case Table table:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, table);
            var tableDisplay = MakeTable(table);
            _displayAtoms.Add(tableDisplay);
            _currentPosition.X += tableDisplay.Width;
            break;
          case LargeOperator op:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, op);
            var opDisplay = MakeLargeOperator(op);
            _displayAtoms.Add(opDisplay);
            break;
          case RaiseBox raiseBox:
            AddDisplayLine(false);
            var raisedDisplay =
              CreateLine(raiseBox.InnerList, _font, _context, _style, false);
            var raisedPosition = _currentPosition;
            raisedPosition.Y += raiseBox.Raise.ActualLength(_mathTable, _font);
            raisedDisplay.Position = raisedPosition;
            _currentPosition.X += raisedDisplay.Width;
            _displayAtoms.Add(raisedDisplay);
            break;
          case LargeDelimiter large:
            AddDisplayLine(false);
            AddInterElementSpace(prevAtom, Typesetter.SpacingAtom(large));
            if (large.Nucleus.Length == 0) {
              var emptyDisplay = new GlyphDisplay<TFont, TGlyph>(default!, large.IndexRange,
                _styleFont, 0, 0, 0) { Position = _currentPosition };
              _displayAtoms.Add(emptyDisplay);
              if (large.Subscript.IsNonEmpty() || large.Superscript.IsNonEmpty())
                MakeScripts(large, emptyDisplay, large.IndexRange.Location, 0);
            } else {
              var height = (large.Size switch {
                LargeDelimiter.DelimiterSize.Size1 => 1.2f,
                LargeDelimiter.DelimiterSize.Size2 => 1.623f,
                LargeDelimiter.DelimiterSize.Size3 => 2.047f,
                _ => 2.470f
              }) * _styleFont.PointSize;
              var display = FindGlyphForBoundary(large.Nucleus, height, large.IndexRange);
              display.Position = _currentPosition;
              _currentPosition.X += display.Width;
              _displayAtoms.Add(display);
              if (large.Subscript.IsNonEmpty() || large.Superscript.IsNonEmpty())
                MakeScripts(large, display, large.IndexRange.Location, 0);
            }
            break;
          case Ordinary _:
          case BinaryOperator _:
          case Relation _:
          case Open _:
          case Close _:
          case Placeholder _:
          case Punctuation _: {
              if (prevAtom != null) {
                float interElementSpace =
                  InterElementSpaces.Get(Typesetter.SpacingAtom(prevAtom), Typesetter.SpacingAtom(atom), _style, _styleFont, _mathTable);
                if (_currentLine.Length > 0) {
                  if (interElementSpace > 0) {
                    _currentLine.Runs.Last().GlyphInfos.Last().KernAfterGlyph = interElementSpace;
                  }
                } else {
                  _currentPosition.X += interElementSpace;
                }
              }
              var nucleusText = atom.Nucleus;
              var glyphs = _context.GlyphFinder.FindGlyphs(_font, nucleusText);
              var current = new AttributedGlyphRun<TFont, TGlyph>(
                nucleusText, glyphs, _font, atom is Placeholder, (atom as Placeholder)?.Color);
              _currentLine.AppendGlyphRun(current);
              if (_currentLineIndexRange.Location == Range.UndefinedInt)
                _currentLineIndexRange = atom.IndexRange;
              else
                _currentLineIndexRange += atom.IndexRange;
              // add the fused atoms
              if (atom.FusedAtoms != null)
                _currentAtoms.AddRange(atom.FusedAtoms);
              else
                _currentAtoms.Add(atom);
              if (atom.Subscript.IsNonEmpty() || atom.Superscript.IsNonEmpty()) {
                var line = AddDisplayLine(true);
                if (line is null) throw new InvalidCodePathException("evenIfLengthIsZero not respected");
                float delta = 0;
                if (atom.Nucleus.Length > 0) {
                  var glyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex
                    (_font, atom.Nucleus.Length - 1, atom.Nucleus);
                  delta = _context.MathTable.GetItalicCorrection(_styleFont, glyph);
                }
                if (delta > 0 && atom.Subscript.IsEmpty())
                  // add a kern of delta
                  _currentPosition.X += delta;
                MakeScripts(atom, line, atom.IndexRange.End - 1, delta);
              }
              break;
            }
          default:
            throw new InvalidCodePathException("Unknown atom type " + atom.TypeName);
        }
        prevAtom = atom;
      }

      AddDisplayLine(false);
      if (_spaced && prevAtom != null) {
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
        CreateLine(accent.InnerList, _font, _context, _style, true);
      if (accent.Nucleus.Length == 0) {
        //no accent
        return accentee;
      }

      var accenteeSingleGlyph = _context.GlyphFinder.EmptyGlyph;
      if (accent.InnerList?.Atoms.Count == 1
        && accent.InnerList.Atoms[0] is MathAtom innerAtom
        && Typesetter.UnicodeLengthIsOne(innerAtom.Nucleus)
        && innerAtom.Superscript.IsEmpty()
        && innerAtom.Subscript.IsEmpty()) {
        // Only one single Unicode character is allowed to be an accent
        accenteeSingleGlyph =
          _context.GlyphFinder.FindGlyphForCharacterAtIndex
            (_font, innerAtom.Nucleus.Length - 1, innerAtom.Nucleus);
        if (accent.Subscript.IsNonEmpty() || accent.Superscript.IsNonEmpty()) {
          // Attach the super/subscripts to the accentee instead of the accent.
          innerAtom.Subscript.Append(accent.Subscript);
          innerAtom.Superscript.Append(accent.Superscript);
          accent.Subscript.Clear();
          accent.Superscript.Clear();
          // Remake the accentee (now with sub/superscripts)
          // Note: Latex adjusts the heights in case the height of the char is different
          // in non-cramped mode. However this shouldn't be the case since cramping
          // only affects fractions and superscripts. We skip adjusting the heights.
          accentee = CreateLine(accent.InnerList, _font, _context, _style, _cramped);
        }
      }

      var display = new AccentDisplay<TFont, TGlyph>(
        Typesetter.CreateAccentGlyphDisplay(
          accentee, accenteeSingleGlyph,
          _context.GlyphFinder.FindGlyphForCharacterAtIndex(
            _font, accent.Nucleus.Length - 1, accent.Nucleus
          ),
          _context, _styleFont, accent.IndexRange), accentee);
      // WJWJWJ -- In the display, the position is the Accentee position.
      // Is that correct, or should we be setting it here?
      // (Happypig375 edit: That should be correct but _currentPosition
      // should have been added like below.)
      display.Position = display.Position.Plus(_currentPosition);
      return display;
    }


    private void MakeScripts(MathAtom atom, IDisplay<TFont, TGlyph> display, int index, float delta) {
      float superscriptShiftUp = 0;
      float subscriptShiftDown = 0;
      display.HasScript = true;
      if (!(display is TextLineDisplay<TFont, TGlyph>)) {
        var scriptFontSize = _mathTable.GetStyleSize(_scriptStyle, _font);
        var scriptFont = _context.MathFontCloner.Invoke(_font, scriptFontSize);
        superscriptShiftUp = display.Ascent - _context.MathTable.SuperscriptShiftUp(scriptFont);
        subscriptShiftDown = display.Descent + _context.MathTable.SubscriptBaselineDropMin(scriptFont);
      }
      if (atom.Superscript.IsEmpty()) {
        if (atom.Subscript.IsEmpty())
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
      if (atom.Subscript.IsEmpty()) {
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

    private void AddInterElementSpace(MathAtom? prev, MathAtom current) =>
      _currentPosition.X +=
        prev != null ? InterElementSpaces.Get(Typesetter.SpacingAtom(prev), Typesetter.SpacingAtom(current), _style, _styleFont, _mathTable)
        : _spaced ? InterElementSpaces.Get(new Open(""), Typesetter.SpacingAtom(current), _style, _styleFont, _mathTable)
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
    private RadicalDisplay<TFont, TGlyph> MakeRadical(MathList radicand, Range range) {
      IGlyphDisplay<TFont, TGlyph> _GetRadicalGlyph(float radicalHeight) {
        // TODO: something related to GlyphFinder.FindGlyph
        var radicalGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, "\u221A");
        var glyph = FindGlyph(radicalGlyph, radicalHeight,
          out float glyphAscent, out float glyphDescent, out float glyphWidth);

        return
          glyphAscent + glyphDescent < radicalHeight
          // the glyphs are not big enough, so we construct one using extenders
          && ConstructGlyph(radicalGlyph, radicalHeight) is IGlyphDisplay<TFont, TGlyph> constructed
          ? constructed
          : new GlyphDisplay<TFont, TGlyph>
            (glyph, Range.NotFound, _styleFont, glyphAscent, glyphDescent, glyphWidth);
      }
      var innerDisplay = CreateLine(radicand, _font, _context, _style, true);
      var radicalVerticalGap =
        _style == LineStyle.Display
        ? _mathTable.RadicalDisplayStyleVerticalGap(_styleFont)
        : _mathTable.RadicalVerticalGap(_styleFont);
      var radicalRuleThickness = _mathTable.RadicalRuleThickness(_styleFont);
      var radicalHeight =
        innerDisplay.Ascent + innerDisplay.Descent + radicalVerticalGap + radicalRuleThickness;
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
        - (innerDisplay.Ascent + innerDisplay.Descent + radicalVerticalGap + radicalRuleThickness);
      if (delta > 0) {
        radicalVerticalGap += delta / 2;
      }
      // we need to shift the radical glyph up, to coincide with the baseline of inner.
      // The new ascent of the radical glyph should be thickness + adjusted clearance + h(inner)
      var radicalAscent = radicalRuleThickness + radicalVerticalGap + innerDisplay.Ascent;
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

    private IDisplay<TFont, TGlyph> MakeFraction(Fraction fraction) {
      // Style override: temporarily swap _style for \dfrac/\tfrac/\cfrac.
      var savedStyle = _style;
      bool didOverrideStyle = false;
      if (fraction.StyleOverride != FractionStyle.Auto) {
        var overrideStyle = fraction.StyleOverride switch {
          FractionStyle.Display => LineStyle.Display,
          FractionStyle.Text => LineStyle.Text,
          _ => _style
        };
        if (overrideStyle != _style) {
          _style = overrideStyle;
          _styleFont = _context.MathFontCloner.Invoke(_font, _mathTable.GetStyleSize(_style, _font));
          didOverrideStyle = true;
        }
      }

      float _NumeratorShiftUp(bool hasRule) =>
        (hasRule, _style) switch {
          (true, LineStyle.Display) => _mathTable.FractionNumeratorDisplayStyleShiftUp(_styleFont),
          (true, _) => _mathTable.FractionNumeratorShiftUp(_styleFont),
          (false, LineStyle.Display) => _mathTable.StackTopDisplayStyleShiftUp(_styleFont),
          (false, _) => _mathTable.StackTopShiftUp(_styleFont)
        };
      float _NumeratorGapMin() =>
        _style == LineStyle.Display
        ? _mathTable.FractionNumDisplayStyleGapMin(_styleFont)
        : _mathTable.FractionNumeratorGapMin(_styleFont);

      float _DenominatorShiftDown(bool hasRule) =>
        (hasRule, _style) switch {
          (true, LineStyle.Display) => _mathTable.FractionDenominatorDisplayStyleShiftDown(_styleFont),
          (true, _) => _mathTable.FractionDenominatorShiftDown(_styleFont),
          (false, LineStyle.Display) => _mathTable.StackBottomDisplayStyleShiftDown(_styleFont),
          (false, _) => _mathTable.StackBottomShiftDown(_styleFont)
        };
      float _DenominatorGapMin() =>
        _style == LineStyle.Display
        ? _mathTable.FractionDenomDisplayStyleGapMin(_styleFont)
        : _mathTable.FractionDenominatorGapMin(_styleFont);
      float _StackGapMin() =>
        _style == LineStyle.Display
        ? _mathTable.StackDisplayStyleGapMin(_styleFont)
        : _mathTable.StackGapMin(_styleFont);
      float _FractionDelimiterHeight() =>
          _style == LineStyle.Display
          ? _mathTable.FractionDelimiterDisplayStyleSize(_styleFont)
          : _mathTable.FractionDelimiterSize(_styleFont);

      var numeratorDisplay =
          CreateLine(fraction.Numerator, _font, _context, _fractionStyle, false);
      var denominatorDisplay =
        CreateLine(fraction.Denominator, _font, _context, _fractionStyle, true);

      if (fraction.IsContinuedFraction) {
        // Apply cfrac strut floors to the operand boxes *before* numeratorShiftUp
        // and denominatorShiftDown are computed. AMSMath's strut is a floor on
        // the operand box, not on the shift.
        float strutHeight = 0.85f * _styleFont.PointSize;
        float strutDepth = 0.35f * _styleFont.PointSize;
        numeratorDisplay.SetOverrideMetrics(
          Math.Max(strutHeight, numeratorDisplay.Ascent),
          Math.Max(strutDepth, numeratorDisplay.Descent),
          numeratorDisplay.Width);
        denominatorDisplay.SetOverrideMetrics(
          Math.Max(strutHeight, denominatorDisplay.Ascent),
          Math.Max(strutDepth, denominatorDisplay.Descent),
          denominatorDisplay.Width);
      }

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
        if (distanceFromNumeratorToBar < _NumeratorGapMin()) {
          numeratorShiftUp += (_NumeratorGapMin() - distanceFromNumeratorToBar);
        }
        // now, do the same for the denominator
        var distanceFromDenominatorToBar =
          barLocation - barThickness / 2 - (denominatorDisplay.Ascent - denominatorShiftDown);
        if (distanceFromDenominatorToBar < _DenominatorGapMin()) {
          denominatorShiftDown += _DenominatorGapMin() - distanceFromDenominatorToBar;
        }
      } else {
        float clearance =
          numeratorShiftUp - numeratorDisplay.Descent
          - (denominatorDisplay.Ascent - denominatorShiftDown);
        float minClearance = _StackGapMin();
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
        LinePosition = barLocation,
        NumeratorAlignment = fraction.NumeratorAlignment
      };
      display.UpdateNumeratorAndDenominatorPositions();

      IDisplay<TFont, TGlyph> result;
      // Add delimiters to fraction display
      if (fraction.LeftDelimiter == Boundary.Empty && fraction.RightDelimiter == Boundary.Empty) {
        result = display;
      } else {
        var glyphHeight = _FractionDelimiterHeight();
        var position = new PointF();
        var innerGlyphs = new List<IDisplay<TFont, TGlyph>>();
        if (fraction.LeftDelimiter.Nucleus?.Length > 0) {
          var leftGlyph = FindGlyphForBoundary(fraction.LeftDelimiter.Nucleus, glyphHeight);
          leftGlyph.Position = position;
          innerGlyphs.Add(leftGlyph);
          position.X += leftGlyph.Width;
        }
        display.Position = position;
        position.X += display.Width;
        innerGlyphs.Add(display);
        if (fraction.RightDelimiter.Nucleus?.Length > 0) {
          var rightGlyph = FindGlyphForBoundary(fraction.RightDelimiter.Nucleus, glyphHeight);
          rightGlyph.Position = position;
          innerGlyphs.Add(rightGlyph);
          position.X += rightGlyph.Width;
        }
        result = new ListDisplay<TFont, TGlyph>(innerGlyphs) {
          Position = _currentPosition
        };
      }

      if (fraction.IsContinuedFraction) {
        // Wrap with a 3mu thinspace on both sides (amsmath \cfrac). The wrapper is a
        // plain positioned container; its metrics are the child's plus the padding.
        float thinspace = 3 * _mathTable.MuUnit(_styleFont);
        var wrapped = new ListDisplay<TFont, TGlyph>(new[] { result });
        result.Position = new PointF(thinspace, 0);
        wrapped.Position = _currentPosition;
        wrapped.SetOverrideMetrics(result.Ascent, result.Descent, result.Width + 2 * thinspace);
        result = wrapped;
      }

      if (didOverrideStyle) {
        _style = savedStyle;
        _styleFont = _context.MathFontCloner.Invoke(_font, _mathTable.GetStyleSize(_style, _font));
      }
      return result;
    }

    /// <summary>Lays out a generic over/under stack (\overrightarrow, \overbrace,
    /// \overset, …). The base is centered; over/under rows are centered above/below
    /// it using stretch-stack gaps for extensible rows and operator-limit gaps for
    /// MathList rows.</summary>
    private IDisplay<TFont, TGlyph> MakeStack(Stack stack, Range range) {
      var baseDisplay = CreateLine(stack.InnerList, _font, _context, _style, _cramped);
      float targetWidth = baseDisplay.Width;

      IDisplay<TFont, TGlyph>? BuildRow(StackConstruction? construction, bool over) {
        if (construction == null) return null;
        if (construction is StackConstruction.Extensible extensible) {
          return BuildHorizontalExtensibleDisplay(extensible.Glyph, targetWidth, range);
        }
        if (construction is StackConstruction.MathListRow mathList) {
          return CreateLine(mathList.List, _font, _context, _scriptStyle,
            over ? _superscriptCramped : _subscriptCramped);
        }
        throw new InvalidCodePathException("Unknown stack construction kind");
      }

      var overDisplay = BuildRow(stack.Over, true);
      var underDisplay = BuildRow(stack.Under, false);

      // MathList rows use the operator-limit gap; stretchy rows use stretch-stack gaps.
      float overGap =
        stack.Over is StackConstruction.MathListRow && overDisplay != null
        ? Math.Max(_mathTable.UpperLimitGapMin(_styleFont),
                   _mathTable.UpperLimitBaselineRiseMin(_styleFont) - overDisplay.Descent)
        : _mathTable.StretchStackGapAboveMin(_styleFont);
      float underGap =
        stack.Under is StackConstruction.MathListRow && underDisplay != null
        ? Math.Max(_mathTable.LowerLimitGapMin(_styleFont),
                   _mathTable.LowerLimitBaselineDropMin(_styleFont) - underDisplay.Ascent)
        : _mathTable.StretchStackGapBelowMin(_styleFont);

      float totalWidth = Math.Max(baseDisplay.Width,
        Math.Max(overDisplay?.Width ?? 0, underDisplay?.Width ?? 0));

      baseDisplay.Position = new PointF((totalWidth - baseDisplay.Width) / 2, 0);
      if (overDisplay != null) {
        overDisplay.Position = new PointF(
          (totalWidth - overDisplay.Width) / 2,
          baseDisplay.Ascent + overGap + overDisplay.Descent);
      }
      if (underDisplay != null) {
        underDisplay.Position = new PointF(
          (totalWidth - underDisplay.Width) / 2,
          -(baseDisplay.Descent + underGap + underDisplay.Ascent));
      }

      return new StackDisplay<TFont, TGlyph>(baseDisplay, overDisplay, underDisplay, range) {
        Position = _currentPosition,
        Width = totalWidth,
        Ascent = baseDisplay.Ascent +
          (overDisplay != null ? overGap + overDisplay.Ascent + overDisplay.Descent : 0),
        Descent = baseDisplay.Descent +
          (underDisplay != null ? underGap + underDisplay.Ascent + underDisplay.Descent : 0)
      };
    }

    /// <summary>Finds the smallest horizontal variant whose width covers minWidth, or the
    /// largest available; falls back to the horizontal glyph assembly.</summary>
    private IDisplay<TFont, TGlyph>? BuildHorizontalExtensibleDisplay(
      string capGlyphText, float targetWidth, Range range) {
      var capGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, capGlyphText);
      if (_context.GlyphFinder.GlyphIsEmpty(capGlyph)) return null;
      var (variantsEnumerable, nVariants) = _mathTable.GetHorizontalVariantsForGlyph(capGlyph);
      var variants = variantsEnumerable.ToArray();
      if (nVariants == 0) {
        // Assembly-only glyph: no preset variants, go straight to the assembly.
        if (ConstructHorizontalGlyph(capGlyph, targetWidth, range) is IGlyphDisplay<TFont, TGlyph> assembledOnly) {
          return assembledOnly;
        }
        // No assembly either; render the bare glyph.
        using var fallbackArray = new RentedArray<TGlyph>(capGlyph);
        var fallbackBox = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, fallbackArray.Result, 1).Single();
        var (fallbackAdvances, fallbackTotal) = _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, fallbackArray.Result, 1);
        fallbackBox.GetAscentDescentWidth(out float fa, out float fd, out _);
        return new GlyphDisplay<TFont, TGlyph>(capGlyph, range, _styleFont, fa, fd, fallbackTotal);
      }
      var boundingBoxes = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, variants, nVariants).ToArray();
      var (advancesEnumerable, _) = _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, variants, nVariants);
      var advances = advancesEnumerable.ToArray();
      TGlyph bestGlyph = default!;
      float bestAscent = 0, bestDescent = 0, bestWidth = float.NegativeInfinity;
      for (int i = 0; i < nVariants; i++) {
        boundingBoxes[i].GetAscentDescentWidth(out bestAscent, out bestDescent, out _);
        bestGlyph = variants[i];
        bestWidth = advances[i];
        if (bestWidth >= targetWidth) break;
      }
      if (bestWidth >= targetWidth) {
        return new GlyphDisplay<TFont, TGlyph>(bestGlyph, range, _styleFont, bestAscent, bestDescent, bestWidth);
      }
      // No variant covers the width; try the font-supplied horizontal assembly.
      if (ConstructHorizontalGlyph(capGlyph, targetWidth, range)
          is IGlyphDisplay<TFont, TGlyph> assembled) {
        return assembled;
      }
      // Saturation: use the largest available variant.
      return new GlyphDisplay<TFont, TGlyph>(bestGlyph, range, _styleFont, bestAscent, bestDescent, bestWidth);
    }

    private InnerDisplay<TFont, TGlyph> MakeInner(Inner inner, Range range) {
      if (inner.LeftBoundary == Boundary.Empty && inner.RightBoundary == Boundary.Empty) {
        throw new InvalidCodePathException("Inner should have a boundary to call this function.");
      }
      var innerListDisplay = CreateLine(inner.InnerList, _font, _context, _style, _cramped, true);
      float axisHeight = _mathTable.AxisHeight(_styleFont);
      // delta is the max distance from the axis.
      float delta =
        Math.Max(innerListDisplay.Ascent - axisHeight, innerListDisplay.Descent + axisHeight);
      var d1 = delta / 500 * _delimiterFactor; // This represents atleast 90% of the formula
      float d2 = 2 * delta - _delimiterShortfallPoints; // This represents a shortfall of 5pt
      // The size of the delimiter glyph should cover at least 90% of the formula or
      // be at most 5pt short.
      float glyphHeight = Math.Max(d1, d2);

      var leftGlyph =
        inner.LeftBoundary is Boundary { Nucleus: var left } && left?.Length > 0
        ? FindGlyphForBoundary(left, glyphHeight)
        : null;

      var rightGlyph =
        inner.RightBoundary is Boundary { Nucleus: var right } && right?.Length > 0
        ? FindGlyphForBoundary(right, glyphHeight)
        : null;
      return new InnerDisplay<TFont, TGlyph>(innerListDisplay, leftGlyph, rightGlyph, range);
    }

    private IGlyphDisplay<TFont, TGlyph> FindGlyphForBoundary(
      string delimiter, float glyphHeight, Range? range = null) {
      var leftGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, delimiter);
      var glyph = FindGlyph(leftGlyph, glyphHeight,
        out float glyphAscent, out float glyphDescent, out float glyphWidth);
      var displayRange = range ?? Range.NotFound;
      IGlyphDisplay<TFont, TGlyph> glyphDisplay;
      if (glyphAscent + glyphDescent < glyphHeight
        && ConstructGlyph(leftGlyph, glyphHeight) is GlyphConstructionDisplay<TFont, TGlyph> constructed) {
        constructed.Range = displayRange;
        glyphDisplay = constructed;
      } else {
        glyphDisplay = new GlyphDisplay<TFont, TGlyph>
          (glyph, displayRange, _styleFont, glyphAscent, glyphDescent, glyphWidth);
      }
      // Center the glyph on the axis
      var shiftDown =
        0.5f * (glyphDisplay.Ascent - glyphDisplay.Descent)
        - _mathTable.AxisHeight(_styleFont);
      glyphDisplay.ShiftDown = shiftDown;
      return glyphDisplay;
    }

    private UnderAnnotationDisplay<TFont, TGlyph> MakeUnderAnnotation(UnderAnnotation underAnnotation, Range range) {

      var innerListDisplay = CreateLine(underAnnotation.InnerList, _font, _context, _style, _cramped, true);

      ListDisplay<TFont, TGlyph>? underListDisplay = null;
      if (underAnnotation.UnderList is { Count: > 0 }) {
        underListDisplay = CreateLine(underAnnotation.UnderList, _font, _context, _scriptStyle, _subscriptCramped, true);
      }

      float axisHeight = _mathTable.AxisHeight(_styleFont);

      var annotationSingleGlyph = _context.GlyphFinder.FindGlyphForCharacterAtIndex(_font, 0, underAnnotation.Nucleus);

      var glyph = FindHorizontalGlyph(annotationSingleGlyph, innerListDisplay.Width,
        out float glyphAscent, out float glyphDescent, out float glyphWidth);

      var lineShiftUp = innerListDisplay.Descent;
      glyphDescent += lineShiftUp;

      var glyphDisplay =
      innerListDisplay.Width > glyphWidth ? ConstructHorizontalGlyph(annotationSingleGlyph,
        innerListDisplay.Width, Range.NotFound) as IGlyphDisplay<TFont, TGlyph>
         :
        new GlyphDisplay<TFont, TGlyph>
          (glyph, Range.NotFound, _styleFont, glyphAscent, glyphDescent, glyphWidth);

      glyphDisplay!.Position = new PointF(_currentPosition.X, glyphDisplay!.Position.Y - lineShiftUp);

      var delta = (glyphDisplay.Width - innerListDisplay.Width) / 2;
      innerListDisplay.Position = new PointF(_currentPosition.X + delta, _currentPosition.Y);

      var glArray = new RentedArray<TGlyph>(annotationSingleGlyph);
      var boundingBox = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, glArray.Result, 1).Single();

      float underListBasedDescent = 0;
      if (underListDisplay is not null) {
        var delta1 = (glyphDisplay.Width - underListDisplay.Width) / 2;
        underListBasedDescent = axisHeight + underListDisplay.Ascent + boundingBox.Height;
        underListDisplay.Position = new PointF(_currentPosition.X + delta1, glyphDisplay!.Position.Y - underListBasedDescent);
      }

      return new UnderAnnotationDisplay<TFont, TGlyph>(innerListDisplay, underListDisplay, glyphDisplay!, underListBasedDescent, _currentPosition);
    }

    private HorizontalGlyphConstructionDisplay<TFont, TGlyph>? ConstructHorizontalGlyph(
      TGlyph glyph, float glyphWidth, Range range) {
      var parts = _mathTable.GetHorizontalGlyphAssembly(glyph, _styleFont);
      if (parts is null) return null;
      var partList = parts.ToList();
      if (partList.Count == 0) return null;
      ValidateAssemblyParts(partList);
      var glyphs = new List<TGlyph>();
      var offsets = new List<float>();
      float width = ConstructHorizontalGlyphWithParts(partList, glyphWidth, glyphs, offsets);
      float glyphAscent = 0, glyphDescent = 0;
      var bounds = _context.GlyphBoundsProvider
        .GetBoundingRectsForGlyphs(_styleFont, glyphs, glyphs.Count);
      foreach (var boundingBox in bounds) {
        boundingBox.GetAscentDescentWidth(out var ascent, out var descent, out _);
        glyphAscent = Math.Max(glyphAscent, ascent);
        glyphDescent = Math.Max(glyphDescent, descent);
      }
      return new HorizontalGlyphConstructionDisplay<TFont, TGlyph>
        (glyphs, offsets, _styleFont, glyphAscent, glyphDescent, width) { Range = range };
    }

    private float ConstructHorizontalGlyphWithParts(IEnumerable<GlyphPart<TGlyph>> parts,
      float glyphWidth, List<TGlyph> glyphs, List<float> offsets) {
      var partList = parts.ToList();
      var hasExtender = partList.Any(part => part.IsExtender);
      float previousMinWidth = float.NegativeInfinity;
      float previousMaxWidth = float.NegativeInfinity;
      for (int nExtenders = 0; ; nExtenders++) {
        glyphs.Clear();
        offsets.Clear();
        GlyphPart<TGlyph>? prevPart = null;
        float minDistance = _mathTable.MinConnectorOverlap(_styleFont);
        float minOffset = 0;
        float maxDelta = float.MaxValue;
        foreach (var part in partList) {
          var repeats = 1;
          if (part.IsExtender) {
            repeats = nExtenders;
          }
          for (int i = 0; i < repeats; i++) {
            glyphs.Add(part.Glyph);
            if (prevPart != null) {
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
        float minWidth = minOffset + prevPart.FullAdvance;
        float maxWidth = minWidth + maxDelta * (glyphs.Count - 1);
        if (!IsFinite(minWidth) || !IsFinite(maxWidth))
          throw new InvalidCodePathException("Glyph assembly produced a non-finite width.");
        if (nExtenders >= 3 && minWidth < glyphWidth
          && !(minWidth > previousMinWidth || maxWidth > previousMaxWidth))
          throw new InvalidCodePathException("Glyph assembly made no progress while adding extenders.");
        previousMinWidth = minWidth;
        previousMaxWidth = maxWidth;
        if (!hasExtender && minWidth < glyphWidth)
          throw new InvalidCodePathException("Glyph assembly has no extender for the requested size.");
        if (minWidth >= glyphWidth) {
          // we are done
          return minWidth;
        }
        if (glyphWidth <= maxWidth) {
          // spread the delta equally among all the connecters
          float delta = glyphWidth - minWidth;
          float dDelta = delta / (glyphs.Count - 1);
          float lastOffset = 0;
          for (int i = 0; i < offsets.Count; i++) {
            float offset = offsets[i] + i * dDelta;
            offsets[i] = offset;
            lastOffset = offset;
          }
          // we are done
          return lastOffset + prevPart.FullAdvance;
        }
      }
    }

    private GlyphConstructionDisplay<TFont, TGlyph>? ConstructGlyph(TGlyph glyph, float glyphHeight) {
      var parts = _mathTable.GetVerticalGlyphAssembly(glyph, _styleFont);
      if (parts is null) return null;
      var partList = parts.ToList();
      if (partList.Count == 0) return null;
      ValidateAssemblyParts(partList);
      var glyphs = new List<TGlyph>();
      var offsets = new List<float>();
      float height = ConstructGlyphWithParts(partList, glyphHeight, glyphs, offsets);
      using var singleGlyph = new RentedArray<TGlyph>(glyphs[0]);
      // descent:0 because it's up to the rendering to adjust the display glyph up or down by setting ShiftDown
      return new GlyphConstructionDisplay<TFont, TGlyph>
        (glyphs, offsets, _styleFont, height, 0, _context.GlyphBoundsProvider
          .GetAdvancesForGlyphs(_styleFont, singleGlyph.Result, 1).Total);
    }

    private float ConstructGlyphWithParts(IEnumerable<GlyphPart<TGlyph>> parts,
      float glyphHeight, List<TGlyph> glyphs, List<float> offsets) {
      var partList = parts.ToList();
      var hasExtender = partList.Any(part => part.IsExtender);
      float previousMinHeight = float.NegativeInfinity;
      float previousMaxHeight = float.NegativeInfinity;
      for (int nExtenders = 0; ; nExtenders++) {
        glyphs.Clear();
        offsets.Clear();
        GlyphPart<TGlyph>? prevPart = null;
        float minDistance = _mathTable.MinConnectorOverlap(_styleFont);
        float minOffset = 0;
        float maxDelta = float.MaxValue;
        foreach (var part in partList) {
          var repeats = 1;
          if (part.IsExtender) {
            repeats = nExtenders;
          }
          for (int i = 0; i < repeats; i++) {
            glyphs.Add(part.Glyph);
            if (prevPart != null) {
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
        if (!IsFinite(minHeight) || !IsFinite(maxHeight))
          throw new InvalidCodePathException("Glyph assembly produced a non-finite height.");
        if (nExtenders >= 3 && minHeight < glyphHeight
          && !(minHeight > previousMinHeight || maxHeight > previousMaxHeight))
          throw new InvalidCodePathException("Glyph assembly made no progress while adding extenders.");
        previousMinHeight = minHeight;
        previousMaxHeight = maxHeight;
        if (!hasExtender && minHeight < glyphHeight)
          throw new InvalidCodePathException("Glyph assembly has no extender for the requested size.");
        if (minHeight >= glyphHeight) {
          // we are done
          return minHeight;
        }
        if (glyphHeight <= maxHeight) {
          // spread the delta equally among all the connecters
          float delta = glyphHeight - minHeight;
          float dDelta = delta / (glyphs.Count - 1);
          float lastOffset = 0;
          for (int i = 0; i < offsets.Count; i++) {
            float offset = offsets[i] + i * dDelta;
            offsets[i] = offset;
            lastOffset = offset;
          }
          // we are done
          return lastOffset + prevPart.FullAdvance;
        }
      }
    }

    private static void ValidateAssemblyParts(IReadOnlyCollection<GlyphPart<TGlyph>> parts) {
      if (parts.Any(part => !IsFinite(part.FullAdvance) || part.FullAdvance < 0
        || !IsFinite(part.StartConnectorLength) || part.StartConnectorLength < 0
        || !IsFinite(part.EndConnectorLength) || part.EndConnectorLength < 0
        || (part.IsExtender && !(part.FullAdvance > 0))))
        throw new InvalidCodePathException("Glyph assembly contains invalid metrics.");
    }
    private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private TGlyph FindGlyph(TGlyph rawGlyph, float height,
      out float glyphAscent, out float glyphDescent, out float glyphWidth) {
      // in iosMath.
      glyphAscent = glyphDescent = glyphWidth = float.NaN;
      var (variants, nVariants) = _mathTable.GetVerticalVariantsForGlyph(rawGlyph);
      if (nVariants == 0) {
        using var rawGlyphArray = new RentedArray<TGlyph>(rawGlyph);
        var rect = _context.GlyphBoundsProvider
          .GetBoundingRectsForGlyphs(_styleFont, rawGlyphArray.Result, 1).Single();
        rect.GetAscentDescentWidth(out glyphAscent, out glyphDescent, out _);
        glyphWidth = _context.GlyphBoundsProvider
          .GetAdvancesForGlyphs(_styleFont, rawGlyphArray.Result, 1).Total;
        return rawGlyph;
      }
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

    private TGlyph FindHorizontalGlyph(TGlyph rawGlyph, float width,
      out float glyphAscent, out float glyphDescent, out float glyphWidth) {
      // in iosMath.
      glyphAscent = glyphDescent = glyphWidth = float.NaN;
      var (variants, nVariants) = _mathTable.GetHorizontalVariantsForGlyph(rawGlyph);
      if (nVariants == 0) {
        using var rawGlyphArray = new RentedArray<TGlyph>(rawGlyph);
        var rect = _context.GlyphBoundsProvider
          .GetBoundingRectsForGlyphs(_styleFont, rawGlyphArray.Result, 1).Single();
        rect.GetAscentDescentWidth(out glyphAscent, out glyphDescent, out _);
        glyphWidth = _context.GlyphBoundsProvider
          .GetAdvancesForGlyphs(_styleFont, rawGlyphArray.Result, 1).Total;
        return rawGlyph;
      }
      var rects =
        _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs(_styleFont, variants, nVariants);
      var advances =
        _context.GlyphBoundsProvider.GetAdvancesForGlyphs(_styleFont, variants, nVariants).Advances;
      foreach (var (rect, advance, variant) in rects.Zip(advances, variants, ValueTuple.Create)) {
        rect.GetAscentDescentWidth(out glyphAscent, out glyphDescent, out glyphWidth);
        if (glyphWidth >= width) {
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
      // Cells inherit the surrounding style unless the env pins one (matrix/cases ->
      // Text, smallmatrix -> Script); see _CellStyleForTable.
      var cellStyle = _CellStyleForTable(table);
      foreach (var row in table.Cells) {
        var colDispalys = new List<ListDisplay<TFont, TGlyph>>();
        r.Add(colDispalys);
        for (int i = 0; i < row.Count; i++) {
          var disp = CreateLine(row[i], _font, _context, cellStyle, false);
          columnWidths[i] = Math.Max(disp.Width, columnWidths[i]);
          colDispalys.Add(disp);
        }
      }
      return r;
    }
    /// <summary>The line style the cells of this table actually render in. Some envs
    /// pin every cell to a fixed style via table.CellStyle; the rest leave it null and
    /// render in the surrounding _style.</summary>
    private LineStyle _CellStyleForTable(Table table) => table.CellStyle ?? _style;

    /// <summary>The font size of a style relative to this table's base font.</summary>
    private float CellStyleFontSize(Table table) =>
      _context.MathTable.GetStyleSize(_CellStyleForTable(table), _font);

    private IDisplay<TFont, TGlyph> MakeTable(Table table) {
      int nColumns = table.NColumns;
      if (nColumns == 0 || table.NRows == 0) {
        //Empty table
        var emptyTable = new ListDisplay<TFont, TGlyph>(Array.Empty<IDisplay<TFont, TGlyph>>());
        return emptyTable;
      }
      bool hasRules =
        table.VerticalLines.Any(v => v > 0) || table.HorizontalLines.Any(h => h > 0);

      var columnWidths = new float[nColumns];
      var displays = TypesetCells(table, columnWidths);
      float[]? columnOffsets = null;
      List<List<float>>? verticalRuleXs = null;
      if (!hasRules) {
        var rowDisplays = new List<ListDisplay<TFont, TGlyph>>();
        foreach (var row in displays) {
          rowDisplays.Add(MakeRowWithColumns(row, table, columnWidths));
        }
        // position all the rows
        PositionRows(rowDisplays, table);
        return new ListDisplay<TFont, TGlyph>(rowDisplays.ToArray()) {
          // Range is set here in the objective C code.
          Position = _currentPosition
        };
      }

      // Array with rules: compute per-column start offsets shared by cells and
      // vertical rules so they cannot drift.
      const float rulePaddingMultiplier = 0.2f;   // content↔rule clearance
      const float ruleGapMultiplier = 0.2f;       // ≈ \doublerulesep (2pt at 10pt)
      float thickness = _mathTable.FractionRuleThickness(_styleFont);
      float padding = rulePaddingMultiplier * _styleFont.PointSize;
      float ruleGap = ruleGapMultiplier * _styleFont.PointSize;
      float cellStyleMuUnit = CellStyleFontSize(table) / 18f;

      columnOffsets = new float[nColumns];
      verticalRuleXs = new List<List<float>>();
      float x = 0;
      for (int boundary = 0; boundary <= nColumns; boundary++) {
        float gapBase = boundary == 0 || boundary == nColumns
          ? 0 : table.InterColumnSpacing * cellStyleMuUnit;
        int count = boundary < table.VerticalLines.Count ? table.VerticalLines[boundary] : 0;
        var ruleXs = new List<float>();
        if (count > 0) {
          // No padding outside the outermost rules so they sit flush at the box edges.
          float padLeft = boundary == 0 ? 0 : padding;
          float padRight = boundary == nColumns ? 0 : padding;
          float ruleAreaStart = x + padLeft + gapBase / 2;
          for (int k = 0; k < count; k++) {
            ruleXs.Add(ruleAreaStart + k * (thickness + ruleGap) + thickness / 2);
          }
          float ruleBlock = count * thickness + (count - 1) * ruleGap;
          x += gapBase + padLeft + padRight + ruleBlock;
        } else {
          x += gapBase;
        }
        verticalRuleXs.Add(ruleXs);
        if (boundary < nColumns) {
          columnOffsets[boundary] = x;
          x += columnWidths[boundary];
        }
      }
      float contentWidth = x;

      var ruledRowDisplays = new List<IDisplay<TFont, TGlyph>>();
      foreach (var row in displays) {
        ruledRowDisplays.Add(MakeRuledRowWithColumns(row, table, columnWidths, columnOffsets));
      }

      // position all the rows
      PositionRows(ruledRowDisplays.Cast<ListDisplay<TFont, TGlyph>>().ToList(), table);

      // Vertical rules span the shared frame (frameBot..frameTop); horizontals span
      // the full content width so outer rules meet at the corners by construction.
      float contentTop = float.NegativeInfinity;
      float contentBot = float.PositiveInfinity;
      foreach (var rowDisplay in ruledRowDisplays) {
        contentTop = Math.Max(contentTop, rowDisplay.Position.Y + rowDisplay.Ascent);
        contentBot = Math.Min(contentBot, rowDisplay.Position.Y - rowDisplay.Descent);
      }
      float frameTop = contentTop + padding;
      float frameBot = contentBot - padding;

      for (int boundary = 0; boundary < verticalRuleXs.Count; boundary++) {
        foreach (var ruleX in verticalRuleXs[boundary]) {
          ruledRowDisplays.Add(new RuleDisplay<TFont, TGlyph>(
            new PointF(ruleX, frameBot), frameBot < frameTop ? frameTop - frameBot : 0,
            thickness, vertical: true, Range.NotFound));
        }
      }
      int nRows = table.NRows;
      for (int b = 0; b < table.HorizontalLines.Count && b <= nRows; b++) {
        int count = table.HorizontalLines[b];
        if (count == 0) continue;
        float y0;
        if (b == 0) {
          y0 = frameTop;
        } else if (b >= nRows) {
          y0 = frameBot;
        } else {
          var above = ruledRowDisplays[b - 1];   // upper row (higher y)
          var below = ruledRowDisplays[b];       // lower row
          float gapBot = above.Position.Y - above.Descent;
          float gapTop = below.Position.Y + below.Ascent;
          y0 = (gapTop + gapBot) / 2;
        }
        for (int k = 0; k < count; k++) {
          float yk = b switch {
            0 => y0 - k * (thickness + ruleGap),           // stack downward
            _ when b >= nRows => y0 + k * (thickness + ruleGap), // stack upward
            _ => y0 + (k % 2 == 0 ? 1 : -1) * ((k + 1) / 2) * (thickness + ruleGap), // symmetric
          };
          ruledRowDisplays.Add(new RuleDisplay<TFont, TGlyph>(
            new PointF(0, yk), contentWidth, thickness, vertical: false, Range.NotFound));
        }
      }

      return new ListDisplay<TFont, TGlyph>(ruledRowDisplays.ToArray()) {
        Position = _currentPosition
      };
    }

    /// <summary>Like MakeRowWithColumns but using precomputed shared column offsets.</summary>
    private ListDisplay<TFont, TGlyph> MakeRuledRowWithColumns(
      List<ListDisplay<TFont, TGlyph>> row, Table table, float[] columnWidths, float[] columnOffsets) {
      Range rowRange = Range.NotFound;
      for (int i = 0; i < row.Count; i++) {
        var entry = row[i];
        var alignment = table.GetAlignment(i);
        var cellPosition = columnOffsets[i];
        switch (alignment) {
          case ColumnAlignment.Right:
            cellPosition += (columnWidths[i] - entry.Width);
            break;
          case ColumnAlignment.Center:
            cellPosition += (columnWidths[i] - entry.Width) / 2;
            break;
        }
        entry.Position = new PointF(cellPosition, 0);
        rowRange += entry.Range;
      }
      var ruled = new ListDisplay<TFont, TGlyph>(row.ToArray());
      if (rowRange != Range.NotFound) {
        ruled.SetRangeOverride(rowRange);
      }
      return ruled;
    }

    private ListDisplay<TFont, TGlyph> MakeRowWithColumns
      (List<ListDisplay<TFont, TGlyph>> row, Table table, float[] columnWidths) {
      float columnStart = 0;
      Range rowRange = Range.NotFound;
      float cellStyleMuUnit = CellStyleFontSize(table) / 18f;
      for (int i = 0; i < row.Count; i++) {
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
        columnStart += (columnWidth + table.InterColumnSpacing * cellStyleMuUnit);
      }
      return new ListDisplay<TFont, TGlyph>(row.ToArray());
    }

    private const float jotMultiplier = 0.3f;
    private const float lineSkipMultiplier = 0.1f;
    private const float lineSkipLimitMultiplier = 0;
    private const float baseLineSkipMultiplier = 1.2f;

    private void PositionRows(List<ListDisplay<TFont, TGlyph>> rows, Table table) {
      float currPos = 0;
      // Row leading tracks the cell-content style, not the surrounding style: a
      // styled table is a self-contained vbox whose internal baseline grid is fixed
      // in the cell style before it is placed into a smaller context.
      float cellStyleFontSize = CellStyleFontSize(table);
      float openUp = table.InterRowAdditionalSpacing * jotMultiplier * cellStyleFontSize;
      float baselineSkip = openUp + baseLineSkipMultiplier * cellStyleFontSize;
      float lineSkip = openUp + lineSkipMultiplier * cellStyleFontSize;
      float lineSkipLimit = openUp + lineSkipLimitMultiplier * cellStyleFontSize;
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
          using (var glyphsArray = new RentedArray<TGlyph>(glyph)) {
            var boundingBox = _context.GlyphBoundsProvider.GetBoundingRectsForGlyphs
              (_styleFont, glyphsArray.Result, 1).Single();
            var width = _context.GlyphBoundsProvider.GetAdvancesForGlyphs
              (_styleFont, glyphsArray.Result, 1).Total;
            boundingBox.GetAscentDescentWidth(out float ascent, out float descent, out _);
            var shiftDown = 0.5 * (ascent - descent) - _mathTable.AxisHeight(_styleFont);
            if (op.Subscript.IsNonEmpty() && !(op.Limits ?? _style == LineStyle.Display))
              // remove italic correction in this case
              width -= delta;
            var glyphDisplay =
              new GlyphDisplay<TFont, TGlyph>(glyph, op.IndexRange, _styleFont, ascent, descent, width) {
                ShiftDown = (float)shiftDown,
                Position = _currentPosition
              };
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
      if (op.Subscript.IsEmpty() && op.Superscript.IsEmpty()) {
        _currentPosition.X += display.Width;
        return display;
      }
      if (op.Limits ?? _style == LineStyle.Display) {
        ListDisplay<TFont, TGlyph>? superscript = null;
        ListDisplay<TFont, TGlyph>? subscript = null;
        if (op.Superscript.IsNonEmpty()) {
          superscript =
            CreateLine(op.Superscript, _font, _context, _scriptStyle, _superscriptCramped);
        }
        if (op.Subscript.IsNonEmpty()) {
          subscript =
            CreateLine(op.Subscript, _font, _context, _scriptStyle, _subscriptCramped);
        }
        var opsDisplay = new LargeOpLimitsDisplay<TFont, TGlyph>(
          display,
          superscript,
          superscript == null ? 0
          : Math.Max(_mathTable.UpperLimitGapMin(_styleFont),
                     _mathTable.UpperLimitBaselineRiseMin(_styleFont) - superscript.Descent),
          subscript,
          subscript == null ? 0
          : Math.Max(_mathTable.LowerLimitGapMin(_styleFont),
                     _mathTable.LowerLimitBaselineDropMin(_styleFont) - subscript.Ascent),
          delta / 2,
          0
        ) {
          Position = _currentPosition
        };
        _currentPosition.X += opsDisplay.Width;
        return opsDisplay;
      }
      _currentPosition.X += display.Width;
      MakeScripts(op, display, op.IndexRange.Location, delta);
      return display;
    }
  }
}
