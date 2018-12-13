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
          tab.InsertText += InsertText;
          tab.Delete += DeleteBackwards;
        }
    }

    
    public Lazy<string> LaTeX => Source.LaTeX;
    public Color SelectColor { get; set; }
    public override IDisplay<Fonts, Glyph> Display => _display;
    public MathList MathList => Source.MathList;
    public event Action RedrawRequested;

    private readonly CaretView<Fonts, Glyph> caretView;
    //private readonly List<MathListIndex> highlighted;
    private readonly Keyboard<Fonts, Glyph> keyboard;

    protected override void SetRedisplay() { }

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
      var point = caretView.Handle.InitialPoint.Plus(cursorPosition);
      path.BeginRead(1);
      path.Foreground = caretView.Handle.ActualColor;
      path.MoveTo(point.X, point.Y);
      point = caretView.Handle.NextPoint1.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      point = caretView.Handle.NextPoint2.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      point = caretView.Handle.NextPoint3.Plus(cursorPosition);
      path.LineTo(point.X, point.Y);
      point = caretView.Handle.FinalPoint.Plus(cursorPosition);
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




    
    public static bool IsNumeric(char c) => c is '.' || (c >= '0' && c <= '9');
    
  }
}