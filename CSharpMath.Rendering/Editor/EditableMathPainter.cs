using System;
using System.Collections.Generic;
using System.Drawing;
#warning Proper private/public scope
namespace CSharpMath.Rendering {
  using Atoms;
  using Constants;
  using Editor;
  using Enumerations;
  using Interfaces;
  using Color = Structures.Color;

  public abstract class EditableMathPainter<TCanvas, TColor, TButton, TLayout> : Painter<TCanvas, EditableMathSource, TColor> where TButton : class, IButton where TLayout : IButtonLayout<TButton, TLayout> {
    protected EditableMathPainter(MathKeyboardView<TButton, TLayout> keyboard, float fontSize = DefaultFontSize * 3 / 2) : base(fontSize) {
      this.keyboard = keyboard;
      if (keyboard != null) {
        keyboard.leftPressed += MoveCursorLeft;
        keyboard.rightPressed += MoveCursorRight;
      }
      caretView = new CaretView<Fonts, Glyph>(fontSize) { showHandle = true };
      Source = new EditableMathSource(new MathList());
      if (keyboard?.Tabs != null)
        foreach (var tab in keyboard.Tabs) {
          tab.insertText = InsertText;
          tab.delete = DeleteBackwards;
        }
    }
    
    public bool HasText => MathList?.Atoms?.Count > 0;
    public Color SelectColor { get; set; }
    public MathListIndex InsertionIndex { get; set; }
    public override IDisplay<Fonts, Glyph> Display => _display;
    public MathList MathList => Source.MathList;
    public event Action RedrawRequested;
    protected IDisplay<Fonts, Glyph> _display;
    protected void UpdateDisplay() {
      var position = _display?.Position ?? default;
      _display = FrontEnd.TypesettingContextExtensions.CreateLine(TypesettingContext.Instance, MathList, Fonts, LineStyle);
      _display.Position = position;
    }

    private readonly CaretView<Fonts, Glyph> caretView;
    private readonly List<MathListIndex> highlighted;
    private readonly MathKeyboardView<TButton, TLayout> keyboard;

    protected override void SetRedisplay() {
      //EditableMathView.PainterSupplier sets MathList.Atoms to null
      InsertionIndex = MathListIndex.Level0Index(MathList?.Atoms.Count ?? 0);
      InsertionPointChanged();
    }

    protected override RectangleF? MeasureCore(float canvasWidth) =>
      _display.DisplayBounds;

    public override void Draw(TCanvas canvas, TextAlignment alignment, Thickness padding = default, float offsetX = 0, float offsetY = 0) {
      UpdateDisplay();
      var c = WrapCanvas(canvas);
      DrawCore(c, _display, IPainterExtensions.GetDisplayPosition(_display.Width, _display.Ascent, _display.Descent, FontSize, CoordinatesFromBottomLeftInsteadOfTopLeft, c.Width, c.Height, alignment, padding, offsetX, offsetY));
    }
    protected override void DrawAfterSuccess(ICanvas c) {
      base.DrawAfterSuccess(c);
      if (!caretView.showHandle) return;
      var path = c.GetPath();
      PointF? Point(int index) =>
        _display.PointForIndex(TypesettingContext.Instance, MathListIndex.Level0Index(index));
      if (!(_display.PointForIndex(TypesettingContext.Instance, InsertionIndex) is PointF cursorPosition))
        return;
      cursorPosition.Y *= -1; //inverted canvas, blah blah
      var point = caretView.handle.InitialPoint.Plus(cursorPosition);
      path.BeginRead(1);
      path.Foreground = caretView.handle.ActualColor;
      path.MoveTo(point.X, point.Y);
      point = caretView.handle.NextPoint1.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      point = caretView.handle.NextPoint2.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      point = caretView.handle.NextPoint3.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      point = caretView.handle.FinalPoint.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      path.CloseContour();
      path.EndRead();
    }

    private bool isEditing;
    public void StartEditing() {
      if (isEditing) return;
      isEditing = true;
      if (InsertionIndex is null)
        InsertionIndex = MathListIndex.Level0Index(MathList.Atoms.Count);
      //keyboard.StartedEditing(textView);
      InsertionPointChanged();
      BeginEditing?.Invoke(this, EventArgs.Empty);
    }
    public void FinishEditing() {
      if (!isEditing) return;
      isEditing = false;
      //keyboard.FinishedEditing(textView);
      InsertionPointChanged();
      EndEditing?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler BeginEditing;
    public event EventHandler EndEditing;
    public event EventHandler TextModified;
    public event EventHandler ReturnPressed;

    public void Tap(PointF point) {
      point.Y *= -1; //inverted canvas, blah blah
      if (!isEditing) {
        InsertionIndex = null;
        caretView.showHandle = true;
        StartEditing();
      } else {
        // If already editing move the cursor and show handle
        InsertionIndex = ClosestIndexToPoint(point) ??
          MathListIndex.Level0Index(MathList.Atoms.Count);
        caretView.showHandle = true;
        InsertionPointChanged();
      }
    }

    public MathListIndex ClosestIndexToPoint(PointF point) {
      UpdateDisplay();
      // no mathlist, so can't figure it out.
      if (MathList is null) return null;
      return _display.IndexForPoint(TypesettingContext.Instance, point);
    }

    public void Clear() {
      MathList.Clear();
      InsertionPointChanged();
    }

    public void MoveCaretToPoint(PointF point) {
      InsertionIndex = ClosestIndexToPoint(point);
      caretView.showHandle = true;
      InsertionPointChanged();
    }

    public PointF? CaretRectForIndex(MathListIndex index) {
      UpdateDisplay();
      // no mathlist so we can't figure it out.
      if (_display is null) return PointF.Empty;
      return _display.PointForIndex(TypesettingContext.Instance, index);
    }

    public static void ClearPlaceholders(Interfaces.IMathList mathList) {
      foreach (var atom in (IList<IMathAtom>)mathList?.Atoms ?? Array.Empty<IMathAtom>()) {
        if (atom.AtomType is Enumerations.MathAtomType.Placeholder)
          atom.Nucleus = Constants.Symbols.WhiteSquare.ToString();
        if (atom.Superscript is Interfaces.IMathList super)
          ClearPlaceholders(super);
        if (atom.Subscript is Interfaces.IMathList sub)
          ClearPlaceholders(sub);
        if (atom is Radical rad) {
          ClearPlaceholders(rad.Degree);
          ClearPlaceholders(rad.Radicand);
        }
        if (atom is Fraction frac) {
          ClearPlaceholders(frac.Numerator);
          ClearPlaceholders(frac.Denominator);
        }
      }
    }

    /// <summary>
    /// Helper method to update caretView when insertion point/selection changes.
    /// </summary>
    public void InsertionPointChanged() {
      // If not in editing mode, we don't show the caret.
      if (!isEditing) {
#warning INCOMPLETE: REVISIT
        /*
        [_caretView removeFromSuperview];
        self.cancelImage.hidden = YES;
        */
        return;
      }
      ClearPlaceholders(MathList);
      var atom = MathList.AtomAt(InsertionIndex);
      if (atom?.AtomType is MathAtomType.Placeholder) {
        atom.Nucleus = Symbols.BlackSquare.ToString();
        if (InsertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus) {
          // If the insertion index is inside a placeholder, move it out.
          InsertionIndex = InsertionIndex.LevelDown();
        }
        // TODO - disable caret
      } else {
        var previousIndex = InsertionIndex.Previous;
        atom = MathList.AtomAt(previousIndex);
        if (atom != null && atom.AtomType is MathAtomType.Placeholder &&
           atom.Superscript is null && atom.Subscript is null) {
          InsertionIndex = previousIndex;
          atom.Nucleus = Symbols.BlackSquare.ToString();
          // TODO - disable caret
        }
      }

      SetKeyboardMode();

      /*
             Find the insert point rect and create a caretView to draw the caret at this position.
             */

      // Check that we were returned a valid position before displaying a caret there.
      if (!(CaretRectForIndex(InsertionIndex) is PointF caretPosition))
        return;

#warning INCOMPLETE: REVISIT

      /*
    // caretFrame is in the flipped coordinate system, flip it back
    _caretView.position = CGPointApplyAffineTransform(caretPosition, _flipTransform);
    if (_caretView.superview == nil) {
        [self addSubview:_caretView];
        [self setNeedsDisplay];
    }
    
    // when a caret is displayed, the X symbol should be as well
    self.cancelImage.hidden = NO;
    
    // Set up a timer to "blink" the caret.
    [_caretView delayBlink];
    [self.label setNeedsLayout];
      */
      RedrawRequested?.Invoke();
    }
    public void SetKeyboardMode() {
      keyboard.RadicalHighlighted = keyboard.ExponentHighlighted = keyboard.SquareRootHighlighted = false;
      if (InsertionIndex.HasSubIndexOfType(MathListSubIndexType.Superscript))
        keyboard.EqualsAllowed = !(keyboard.ExponentHighlighted = true);

      if (InsertionIndex.SubIndexType is MathListSubIndexType.Numerator ||
        InsertionIndex.SubIndexType is MathListSubIndexType.Denominator)
        keyboard.EqualsAllowed = false;
#warning Review
      //keyboard.FractionsAllowed = false;???

      if (InsertionIndex.SubIndexType is MathListSubIndexType.Degree)
        keyboard.RadicalHighlighted = true;
      else if (InsertionIndex.SubIndexType is MathListSubIndexType.Radicand)
        keyboard.SquareRootHighlighted = true;
    }

    // Insert a list at a given point.
    public void InsertMathList(IMathList list, PointF point) {
      var detailedIndex = ClosestIndexToPoint(point);
      // insert at the given index - but don't consider sublevels at this point
      var index = MathListIndex.Level0Index(detailedIndex.AtomIndex);
      foreach (var atom in list.Atoms) {
        MathList.Insert(index, atom);
        index = index.Next;
      }
      InsertionIndex = index; // move the index to the end of the new list.
      InsertionPointChanged();
    }

    ///<summary>If the index is in a radical, subscript, or exponent, fetches the next index after the root atom.</summary>
    public MathListIndex GetIndexAfterSpecialStructure(MathListIndex index, MathListSubIndexType type) {
      while (index.HasSubIndexOfType(type))
        index = index.LevelDown();
      //Point to just after this node.
      return index.Next;
    }

    public void RemovePlaceholderIfPresent() {
      var current = MathList.AtomAt(InsertionIndex);
      if (current?.AtomType is MathAtomType.Placeholder)
        // Remove this element - the inserted text replaces the placeholder
        MathList.RemoveAt(InsertionIndex);
    }

    /// <returns>True if updated</returns>
    public bool UpdatePlaceholderIfPresent(IMathAtom atom) {
      var current = MathList.AtomAt(InsertionIndex);
      if (current?.AtomType is MathAtomType.Placeholder) {
        if (current.Superscript is IMathList super)
          atom.Superscript = super;
        if (current.Subscript is IMathList sub)
          atom.Subscript = sub;
        //Remove the placeholder and replace with atom.
        MathList.RemoveAt(InsertionIndex);
        MathList.Insert(InsertionIndex, atom);
        return true;
      }
      return false;
    }

    public static MathAtom AtomForCharacter(char c) {
      // Get the basic conversion from MathAtoms, and then special case unicode characters and latex special characters.
      switch (c) {
        //https://github.com/kostub/MathEditor/blob/61f67c6224000c224e252f6eeba483003f11d3d5/mathEditor/editor/MTEditableMathLabel.m#L414
        case Symbols.Multiplication:
        case '*':
          return MathAtoms.Times;
        case Symbols.SquareRoot:
          return MathAtoms.PlaceholderSquareRoot;
        case Symbols.Infinity:
        case Symbols.Degree:
        case Symbols.Angle:
          return MathAtoms.Create(MathAtomType.Ordinary, c);
        case Symbols.Division:
        case '/':
          return MathAtoms.Divide;
        case Symbols.FractionSlash:
          return MathAtoms.PlaceholderFraction;
        case '{':
          return MathAtoms.Create(MathAtomType.Open, c);
        case '}':
          return MathAtoms.Create(MathAtomType.Close, c);
        case Symbols.GreaterEqual:
        case Symbols.LessEqual:
          return MathAtoms.Create(MathAtomType.Relation, c);
        case var _ when c >= UnicodeFontChanger.UnicodeGreekLowerStart && c <= UnicodeFontChanger.UnicodeGreekLowerEnd:
          // All greek letters are rendered as variables.
          return MathAtoms.Create(MathAtomType.Variable, c);
        case var _ when c >= UnicodeFontChanger.UnicodeGreekUpperStart && c <= UnicodeFontChanger.UnicodeGreekUpperEnd:
          // Including capital greek letters
          return MathAtoms.Create(MathAtomType.Variable, c);
        case var _ when c < '\x21' || c > '\x7E':
        case '\'':
        case '~':
          // Not ascii
          return null;
        case var _ when MathAtoms.ForCharacter(c) is MathAtom atom:
          return atom;
        default:
          //Just an ordinary character
          return MathAtoms.Create(MathAtomType.Ordinary, c);
      }
    }

    public static bool IsNumeric(char c) => c is '.' || (c >= '0' && c <= '9');

    public MathListIndex GetOutOfRadical(MathListIndex index) {
      if (index.HasSubIndexOfType(MathListSubIndexType.Degree))
        index = GetIndexAfterSpecialStructure(index, MathListSubIndexType.Degree);
      if (index.HasSubIndexOfType(MathListSubIndexType.Radicand))
        index = GetIndexAfterSpecialStructure(index, MathListSubIndexType.Radicand);
      return index;
    }

    public void InsertText(string str) {
      void HandleExponentButton() {
        if (InsertionIndex.HasSubIndexOfType(MathListSubIndexType.Superscript))
          // The index is currently inside an exponent. The exponent button gets it out of the exponent and move forward.
          InsertionIndex = GetIndexAfterSpecialStructure(InsertionIndex, MathListSubIndexType.Superscript);
        else {
          //Not in an exponent. Add one.
          if (!InsertionIndex.AtBeginningOfLine) {
            var a = MathList.AtomAt(InsertionIndex.Previous);
            if (a.Superscript is null) {
              a.Superscript = MathAtoms.PlaceholderList;
              InsertionIndex = InsertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
            } else if (InsertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus)
              // If we are already inside the nucleus, then we come out and go up to the superscript
              InsertionIndex = InsertionIndex.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(a.Superscript.Atoms.Count), MathListSubIndexType.Superscript);
            else
              InsertionIndex = InsertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Superscript.Atoms.Count), MathListSubIndexType.Superscript);
          } else {
            // Create an empty atom and move the insertion index up.
            var emptyAtom = MathAtoms.Placeholder;
            emptyAtom.Superscript = MathAtoms.PlaceholderList;
            if (!UpdatePlaceholderIfPresent(emptyAtom))
              // If the placeholder hasn't been updated then insert it.
              MathList.Insert(InsertionIndex, emptyAtom);
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
          }
        }
      }

      void HandleRadical(bool degreeButtonPressed) {
        var current = InsertionIndex;
        if ((current.HasSubIndexOfType(MathListSubIndexType.Degree) || current.HasSubIndexOfType(MathListSubIndexType.Radicand)) && MathList.Atoms[current.AtomIndex] is Radical rad)
          if (degreeButtonPressed)
            if (rad.Degree is null) {
              rad.Degree = MathAtoms.PlaceholderList;
              InsertionIndex = current.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
            } else if (current.HasSubIndexOfType(MathListSubIndexType.Radicand))
              // The radical the cursor is at has a degree. If the cursor is in the radicand, move the cursor to the degree
              InsertionIndex = current.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
            else
              // If the cursor is at the degree, get out of the radical
              InsertionIndex = GetOutOfRadical(current);
          else if (current.HasSubIndexOfType(MathListSubIndexType.Degree))
            // If the radical the cursor at has a degree, and the cursor is at the degree, move the cursor to the radicand.
            InsertionIndex = current.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
          else
            // If the cursor is at the radicand, get out of the radical.
            InsertionIndex = GetOutOfRadical(current);
        else if (degreeButtonPressed) {
          rad = MathAtoms.PlaceholderRadical;
          MathList.Insert(current, rad);
          InsertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
        } else {
          rad = MathAtoms.PlaceholderSquareRoot;
          MathList.Insert(current, rad);
          InsertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
        }
      }

      void HandleSubscriptButton() {
        if (InsertionIndex.HasSubIndexOfType(MathListSubIndexType.Subscript))
          // The index is currently inside an subscript. The subscript button gets it out of the subscript and move forward.
          InsertionIndex = GetIndexAfterSpecialStructure(InsertionIndex, MathListSubIndexType.Subscript);
        else {
          //Not in a subscript. Add one.
          if (!InsertionIndex.AtBeginningOfLine) {
            var a = MathList.AtomAt(InsertionIndex.Previous);
            if (a.Subscript is null) {
              a.Subscript = MathAtoms.PlaceholderList;
              InsertionIndex = InsertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
            } else if (InsertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus)
              // If we are already inside the nucleus, then we come out and go down to the subscript
              InsertionIndex = InsertionIndex.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(a.Subscript.Atoms.Count), MathListSubIndexType.Subscript);
            else
              InsertionIndex = InsertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Subscript.Atoms.Count), MathListSubIndexType.Subscript);
          } else {
            // Create an empty atom and move the insertion index up.
            var emptyAtom = MathAtoms.Placeholder;
            emptyAtom.Subscript = MathAtoms.PlaceholderList;
            if (!UpdatePlaceholderIfPresent(emptyAtom))
              // If the placeholder hasn't been updated then insert it.
              MathList.Insert(InsertionIndex, emptyAtom);
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
          }
        }
      }

      void HandleSlashButton() {
        // special / handling - makes the thing a fraction
        var numerator = new MathList();
        var current = InsertionIndex;
        for (; !current.AtBeginningOfLine; current = current.Previous) {
          var a = MathList.AtomAt(current.Previous);
          if (a.AtomType != Enumerations.MathAtomType.Number && a.AtomType != Enumerations.MathAtomType.Variable)
            //We don't put this atom on the fraction
            break;
          else
            //Add the number to the beginning of the list
            numerator.Insert(0, a);
        }
        if (current.AtomIndex == InsertionIndex.AtomIndex) {
          // so we didn't really find any numbers before this, so make the numerator 1
          numerator.Add(AtomForCharacter('1'));
          if (!current.AtBeginningOfLine) {
            var prevAtom = MathList.AtomAt(current.Previous);
            if (prevAtom.AtomType is MathAtomType.Fraction) {
              //Add a times symbol
              MathList.Insert(current, MathAtoms.Times);
              current = current.Next;
            }
          }
        } else
          // delete stuff in the Mathlist from current to insertionIndex
          MathList.RemoveAtoms(new MathListRange(current, InsertionIndex.AtomIndex - current.AtomIndex));

        //Create the fraction
        var frac = new Fraction { Numerator = numerator, Denominator = MathAtoms.PlaceholderList };

        //Insert it
        MathList.Insert(current, frac);
        //Update the insertion index to go the denominator
        InsertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Denominator);
      }

      void InsertParens() {
        MathList.Insert(InsertionIndex, AtomForCharacter('('));
        InsertionIndex = InsertionIndex.Next;
        MathList.Insert(InsertionIndex, AtomForCharacter(')'));
        // Don't go to the next insertion index, to start inserting before the close parens.
      }
      void InsertAbsValue() {
        MathList.Insert(InsertionIndex, AtomForCharacter('|'));
        InsertionIndex = InsertionIndex.Next;
        MathList.Insert(InsertionIndex, AtomForCharacter('|'));
        // Don't go to the next insertion index, to start inserting before the second absolute value
      }

      if (str is null || str is "" || str is "\n")
        return;
      if (str is "\n") {
        ReturnPressed?.Invoke(this, EventArgs.Empty);
        return;
      }
      var ch = str[0];
      var atom = str.Length > 1 ? MathAtoms.ForLatexSymbolName(str) : AtomForCharacter(ch);
      if (InsertionIndex.SubIndexType is MathListSubIndexType.Denominator && atom.AtomType is MathAtomType.Relation)
        // pull the insertion index out
        InsertionIndex = InsertionIndex.LevelDown().Next;
      switch (ch) {
        case '^':
          // Special ^ handling - adds an exponent
          HandleExponentButton();
          break;
        case Symbols.SquareRoot:
          HandleRadical(false);
          break;
        case Symbols.CubeRoot:
          HandleRadical(true);
          break;
        case '_':
          HandleSubscriptButton();
          break;
        case '/':
          HandleSlashButton();
          break;
        case var _ when str is "()":
          RemovePlaceholderIfPresent();
          InsertParens();
          break;
        case var _ when str is "||":
          RemovePlaceholderIfPresent();
          InsertAbsValue();
          break;
        case var _ when atom is IMathAtom a:
          if (!UpdatePlaceholderIfPresent(a))
            // If a placeholder wasn't updated then insert the new element.
            MathList.Insert(InsertionIndex, a);
          if (atom.AtomType is MathAtomType.Fraction)
            // go to the numerator
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
          else
            InsertionIndex = InsertionIndex.Next;
          break;
      }
      InsertionPointChanged();

      // If trig function, insert parens after
      switch (str) {
        case "sin":
        case "cos":
        case "tan":
        case "sec":
        case "csc":
        case "cot":
          InsertParens();
          break;
      }
      TextModified?.Invoke(this, EventArgs.Empty);
    }

    public void DeleteBackwards() {
      // delete the last atom from the list
      var prevIndex = InsertionIndex.Previous;
      if (HasText && !(prevIndex is null)) {
        MathList.RemoveAt(prevIndex);
        if (prevIndex.FinalSubIndexType is MathListSubIndexType.Nucleus) {
          // it was in the nucleus and we removed it, get out of the nucleus and get in the nucleus of the previous one.
          var downIndex = prevIndex.LevelDown();
          prevIndex = downIndex.Previous is MathListIndex downPrev
            ? downPrev.LevelUpWithSubIndex(MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus)
            : downIndex;
        }
        InsertionIndex = prevIndex;
        if (InsertionIndex.AtBeginningOfLine && InsertionIndex.SubIndexType != MathListSubIndexType.None) {
          // We have deleted to the beginning of the line and it is not the outermost line
          var atom = MathList.AtomAt(InsertionIndex);
          if (atom is null) {
            // add a placeholder if we deleted everything in the list
            atom = MathAtoms.Placeholder;
            // mark the placeholder as selected since that is the current insertion point.
            atom.Nucleus = Symbols.BlackSquare.ToString();
            MathList.Insert(InsertionIndex, atom);
          }
        }

        InsertionPointChanged();
        TextModified?.Invoke(this, EventArgs.Empty);
      }
    }

    public void SelectCharacterAtIndex(MathListIndex index) {
      UpdateDisplay();
      if (_display is null)
        // no mathlist so we can't figure it out.
        return;
      // setup highlights before drawing the MTLine
      _display.HighlightCharacterAt(index, SelectColor);
    }
    public void ClearHighlights() {
      UpdateDisplay();
    }

    public void MoveCursorLeft(object sender, EventArgs e) {
      if (InsertionIndex is null) {
        InsertionIndex = MathListIndex.Level0Index(MathList?.Atoms?.Count ?? 0);
        return;
      }
      if (InsertionIndex.AtBeginningOfLine)
        switch (InsertionIndex.FinalSubIndexType) {
          case MathListSubIndexType.Degree:
          case MathListSubIndexType.Numerator:
          case MathListSubIndexType.Nucleus:
          case MathListSubIndexType.Superscript:
          case MathListSubIndexType.Subscript:
          default:
            InsertionIndex = InsertionIndex.LevelDown() ?? InsertionIndex;
            break;
          case MathListSubIndexType.Radicand:
            var radicalIndex = InsertionIndex.LevelDown();
            if (MathList.AtomAt(radicalIndex) is IRadical rad)
              if (rad.Degree is IMathList deg)
                InsertionIndex = radicalIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(deg.Count), MathListSubIndexType.Degree);
              else
                goto default;
            else
              throw new InvalidCodePathException($"Radical not found at {radicalIndex}");
            break;
          case MathListSubIndexType.Denominator:
            var fracIndex = InsertionIndex.LevelDown();
            if (MathList.AtomAt(fracIndex) is IFraction frac)
              InsertionIndex = fracIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(frac.Numerator.Count), MathListSubIndexType.Numerator);
            else
              throw new InvalidCodePathException($"Fraction not found at {fracIndex}");
            break;
        }
      else if(InsertionIndex.Previous is MathListIndex prev)
        switch (MathList.AtomAt(prev)) {
          case var a when a.Subscript != null:
            InsertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Subscript.Count), MathListSubIndexType.Subscript);
            break;
          case var a when a.Superscript != null:
            InsertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Superscript.Count), MathListSubIndexType.Superscript);
            break;
          case IRadical rad:
            InsertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(rad.Radicand.Count), MathListSubIndexType.Radicand);
            break;
          case IFraction frac:
            InsertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(frac.Denominator.Count), MathListSubIndexType.Denominator);
            break;
          default:
            InsertionIndex = prev;
            break;
        }
      InsertionPointChanged();
      RedrawRequested?.Invoke();
    }

    public void MoveCursorRight(object sender, EventArgs e) {
      if (InsertionIndex is null) {
        InsertionIndex = MathListIndex.Level0Index(MathList?.Atoms?.Count ?? 0);
        return;
      }
      var next = InsertionIndex.Next;
      switch (MathList.AtomAt(next)) {
        case null:
          switch (InsertionIndex.FinalSubIndexType) {
            case MathListSubIndexType.Radicand:
            case MathListSubIndexType.Denominator:
            case MathListSubIndexType.Nucleus:
            case MathListSubIndexType.Superscript:
            case MathListSubIndexType.Subscript:
            default:
              InsertionIndex = InsertionIndex.LevelDown()?.Next ?? InsertionIndex;
              break;
            case MathListSubIndexType.Degree:
              var radicalIndex = InsertionIndex.LevelDown();
              if (MathList.AtomAt(radicalIndex) is IRadical)
                InsertionIndex = radicalIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
              else
                throw new InvalidCodePathException($"Radical not found at {radicalIndex}");
              break;
            case MathListSubIndexType.Numerator:
              var fracIndex = InsertionIndex.LevelDown();
              if (MathList.AtomAt(fracIndex) is IFraction)
                InsertionIndex = fracIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Denominator);
              else
                throw new InvalidCodePathException($"Fraction not found at {fracIndex}");
              break;
          }
          break;
        case IFraction frac:
          InsertionIndex = next.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
          break;
        case IRadical rad:
          if(rad.Degree is IMathList)
            InsertionIndex = next.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
          else
            InsertionIndex = next.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
          break;
        case var a when a.Superscript != null:
          InsertionIndex = next.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
          break;
        case var a when a.Subscript != null:
          InsertionIndex = next.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
          break;
        default:
          InsertionIndex = next;
          break;
      }
      InsertionPointChanged();
      RedrawRequested?.Invoke();
    }
  }
}
