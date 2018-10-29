using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace CSharpMath.Rendering {
  using Atoms;
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
        insertionIndex = 
      }
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

    public void Clear() {
      MathList = new MathList();

    }

    public void insertionPointChanged();
    public void highlightCharacterAtIndex(MathListIndex index);
    public void clearHighlights();
    public void moveCaretToPoint(PointF point);
    public void startEditing();
    public void enableTap(bool enable);

    // Insert a list at a given point.
    public void insertMathList(Atoms.MathList list, PointF point);

    public RectangleF mathDisplaySize { get; }
  }
}
