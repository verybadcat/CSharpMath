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
    public static bool ForbidMultipleRadicals { get; set; }

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
          tab.InsertText += InsertText;
          tab.Delete += DeleteBackwards;
        }
    }
    
    public Lazy<string> LaTeX => Source.LaTeX;
    public Color SelectColor { get; set; }
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
          atom.Nucleus = Constants.Symbols.WhiteSquare;
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
        atom.Nucleus = Symbols.BlackSquare;
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
          atom.Nucleus = Symbols.BlackSquare;
          // TODO - disable caret
        }
      }

      SetKeyboardMode();

      /* Find the insert point rect and create a caretView to draw the caret at this position. */

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


    public void RemovePlaceholderIfPresent() {
      var current = MathList.AtomAt(InsertionIndex);
      if (current?.AtomType is MathAtomType.Placeholder)
        // Remove this element - the inserted text replaces the placeholder
        MathList.RemoveAt(InsertionIndex);
    }



    public static bool IsNumeric(char c) => c is '.' || (c >= '0' && c <= '9');



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

  }
}