using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Rendering {
  using Atoms;
  using CSharpMath.Interfaces;
  using Editor;
  public abstract class EditableMathPainter<TCanvas, TColor, TButton, TTextView> : MathPainter<TCanvas, TColor> where TButton : IButton where TTextView : class, ITextView {
    public EditableMathPainter(float fontSize = DefaultFontSize * 3 / 2) : base(fontSize) { }

    readonly CaretView<Fonts, Glyph> caretView;
    readonly List<MathListIndex> highlighted;
    readonly MathKeyboardView<TButton, TTextView> keyboard;
    TTextView textView;
    MathListIndex insertionIndex;
    protected override void SetRedisplay() {
      base.SetRedisplay();
      insertionIndex = MathListIndex.Level0Index(MathList.Atoms.Count);
      insertionPointChanged();
    }

    protected override void DrawAfterSuccess(ICanvas c) {
      base.DrawAfterSuccess(c);
      if (!caretView.showHandle) return;
      var path = c.GetPath();
      var point = caretView.handle.InitialPoint;
      path.BeginRead(1);
      path.Foreground = caretView.handle.ActualColor;
      path.MoveTo(point.X, point.Y);
      point = caretView.handle.NextPoint1;
      path.LineTo(point.X, point.Y);
      point = caretView.handle.NextPoint2;
      path.LineTo(point.X, point.Y);
      point = caretView.handle.NextPoint3;
      path.LineTo(point.X, point.Y);
      point = caretView.handle.FinalPoint;
      path.LineTo(point.X, point.Y);
      path.CloseContour();
      path.EndRead();
    }

    bool isEditing;
    public void StartEditing() {
      if (isEditing) return;
      isEditing = true;
      if (insertionIndex is null)
        insertionIndex = MathListIndex.Level0Index(MathList.Atoms.Count);
      keyboard.StartedEditing(textView);
      insertionPointChanged();
      BeginEditing?.Invoke(this, EventArgs.Empty);
    }
    public void FinishEditing() {
      if (!isEditing) return;
      isEditing = false;
      keyboard.FinishedEditing(textView);
      insertionPointChanged();
      EndEditing?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler BeginEditing;
    public event EventHandler EndEditing;

    public void Tap(PointF point) {
      if (!isEditing) {
        insertionIndex = null;
        caretView.showHandle = false;
        startEditing();
      } else {
        // If already editing move the cursor and show handle
        insertionIndex = ClosestIndexToPoint(point) ??
          MathListIndex.Level0Index(MathList.Atoms.Count);
        caretView.showHandle = false;
        insertionPointChanged();
      }
    }

    public MathListIndex ClosestIndexToPoint(PointF point){
      UpdateDisplay();
      // no mathlist, so can't figure it out.
      if (MathList is null) return null;
      var displayPoint = new PointF(point.X - _display.Position.X, point.Y - _display.Position.Y);
      return _display.IndexForPoint(TypesettingContext.Instance, displayPoint);
    }

    public void Clear() {
      MathList = new MathList();
      insertionPointChanged();
    }

    public void MoveCaretToPoint(PointF point){
      insertionIndex = ClosestIndexToPoint(point);
      caretView.showHandle = false;
      insertionPointChanged();
    }

    public PointF? CaretRectForIndex(MathListIndex index){
      UpdateDisplay();
      // no mathlist so we can't figure it out.
      if (_display is null) return PointF.Empty;
      return _display.PointForIndex(TypesettingContext.Instance, index);
    }

    public static void ClearPlaceholders(Interfaces.IMathList mathList) {
      foreach (var atom in mathList.Atoms) {
        if (atom.AtomType is Enumerations.MathAtomType.Placeholder)
          atom.Nucleus = Constants.Symbols.WhiteSquare.ToString();
        if (atom.Superscript is Interfaces.IMathList super)
          ClearPlaceholders(super);
        if (atom.Subscript is Interfaces.IMathList sub)
          ClearPlaceholders(sub);
        if(atom is Radical rad) {
          ClearPlaceholders(rad.Degree);
          ClearPlaceholders(rad.Radicand);
        }
        if(atom is Fraction frac) {
          ClearPlaceholders(frac.Numerator);
          ClearPlaceholders(frac.Denominator);
        }
      }
    }

    /// <summary>
    /// Helper method to update caretView when insertion point/selection changes.
    /// </summary>
    public void insertionPointChanged() {
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
      var atom = MathList.AtomAt(insertionIndex);
      if (atom.AtomType is Enumerations.MathAtomType.Placeholder) {
        atom.Nucleus = Constants.Symbols.BlackSquare.ToString();
        if (insertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus) {
          // If the insertion index is inside a placeholder, move it out.
          insertionIndex = insertionIndex.LevelDown();
        }
        // TODO - disable caret
      } else {
        var previousIndex = insertionIndex.Previous;
        atom = MathList.AtomAt(insertionIndex);
        if (atom.AtomType is Enumerations.MathAtomType.Placeholder &&
           atom.Superscript is null && atom.Subscript is null) {
          insertionIndex = previousIndex;
          atom.Nucleus = Constants.Symbols.BlackSquare.ToString();
          // TODO - disable caret
        }
      }

      SetKeyboardMode();

      /*
             Find the insert point rect and create a caretView to draw the caret at this position.
             */

      // Check tht we were returned a valid position before displaying a caret there.
      if (!(CaretRectForIndex(insertionIndex) is PointF caretPosition))
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
    }
    public void SetKeyboardMode() {
      keyboard.RadicalHighlighted = keyboard.ExponentHighlighted = keyboard.SquareRootHighlighted = false;
      if (insertionIndex.HasSubIndexOfType(MathListSubIndexType.Superscript))
        keyboard.EqualsAllowed = !(keyboard.ExponentHighlighted = true);

      if (insertionIndex.SubIndexType is MathListSubIndexType.Numerator ||
        insertionIndex.SubIndexType is MathListSubIndexType.Denominator)
        keyboard.EqualsAllowed = false;
#warning Review
      //keyboard.FractionsAllowed = false;???

      if (insertionIndex.SubIndexType is MathListSubIndexType.Degree)
        keyboard.RadicalHighlighted = true;
      else if (insertionIndex.SubIndexType is MathListSubIndexType.Radicand)
        keyboard.SquareRootHighlighted = true;
    }

    // Insert a list at a given point.
    public void InsertMathList(IMathList list, PointF point){
      var detailedIndex = ClosestIndexToPoint(point);
      // insert at the given index - but don't consider sublevels at this point
      var index = MathListIndex.Level0Index(detailedIndex.AtomIndex);
      foreach(var atom in list.Atoms){
        MathList.Insert(index, atom);
        index = index.Next;
      }
      insertionIndex = index; // move the index to the end of the new list.
      insertionPointChanged();
    }

    public void highlightCharacterAtIndex(MathListIndex index);
    public void clearHighlights();
    public void enableTap(bool enable);

    public RectangleF mathDisplaySize { get; }
  }
}
