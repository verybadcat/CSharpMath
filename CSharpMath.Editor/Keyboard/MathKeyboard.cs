namespace CSharpMath.Editor {
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Timers;
  using Atom;
  using Display;
  using Display.FrontEnd;
  using Structures;
  using Atoms = Atom.Atoms;

  public enum MathKeyboardCaretState : byte {
    Hidden,
    TemporarilyHidden,
    ShownThroughPlaceholder,
    Shown
  }
  public class MathKeyboard<TFont, TGlyph> : LatexMathKeyboard where TFont : IFont<TGlyph> {
    protected Timer blinkTimer;
    public const double DefaultBlinkMilliseconds = 800;
    public MathKeyboard(TypesettingContext<TFont, TGlyph> context, TFont font, double blinkMilliseconds = DefaultBlinkMilliseconds) {
      Context = context;
      Font = font;
      blinkTimer = new Timer(blinkMilliseconds);
      blinkTimer.Elapsed += (sender, e) => {
        switch (CaretState) {
          case MathKeyboardCaretState.Shown:
          case MathKeyboardCaretState.ShownThroughPlaceholder:
            CaretState = MathKeyboardCaretState.TemporarilyHidden;
            break;
          case MathKeyboardCaretState.TemporarilyHidden:
            CaretState = MathKeyboardCaretState.Shown;
            break;
        }
      };
      blinkTimer.Start();
    }
    public bool ShouldDrawCaret => InsertionPositionHighlighted && !(MathList.AtomAt(_insertionIndex) is Atoms.Placeholder);
    bool _insertionPositionHighlighted;
    public bool InsertionPositionHighlighted {
      get => _insertionPositionHighlighted;
      set {
        blinkTimer.Stop();
        blinkTimer.Start();
        _insertionPositionHighlighted = value;
        if (MathList.AtomAt(_insertionIndex) is Atoms.Placeholder placeholder) {
          (placeholder.Nucleus, placeholder.Color) =
            _insertionPositionHighlighted
            ? (LaTeXSettings.PlaceholderActiveNucleus, LaTeXSettings.PlaceholderActiveColor)
            : (LaTeXSettings.PlaceholderRestingNucleus, LaTeXSettings.PlaceholderRestingColor);
        }
        RecreateDisplayFromMathList();
        RedrawRequested?.Invoke(this, EventArgs.Empty);
      }
    }
    public Display.Displays.ListDisplay<TFont, TGlyph>? Display { get; protected set; }
    public void StartBlinking() => blinkTimer.Start();
    public void StopBlinking() => blinkTimer.Stop();
    //private readonly List<MathListIndex> highlighted;
    protected TypesettingContext<TFont, TGlyph> Context { get; }
    static void ResetPlaceholders(MathList mathList) {
      foreach (var mathAtom in mathList.Atoms) {
        ResetPlaceholders(mathAtom.Superscript);
        ResetPlaceholders(mathAtom.Subscript);
        switch (mathAtom) {
          case Atoms.Placeholder _:
            mathAtom.Nucleus = "\u25A1";
            break;
          case IMathListContainer container:
            foreach (var list in container.InnerLists)
              ResetPlaceholders(list);
            break;
        }
      }
    }
    MathKeyboardCaretState _caretState;
    public MathKeyboardCaretState CaretState {
      get => _caretState;
      set {
        blinkTimer.Stop();
        blinkTimer.Start();
        if (value != MathKeyboardCaretState.Hidden &&
           MathList.AtomAt(_insertionIndex) is Atoms.Placeholder placeholder)
          (placeholder.Nucleus, _caretState) =
            value == MathKeyboardCaretState.TemporarilyHidden
            ? ("\u25A1", MathKeyboardCaretState.TemporarilyHidden)
            : ("\u25A0", MathKeyboardCaretState.ShownThroughPlaceholder);
        else _caretState = value;
        RecreateDisplayFromMathList();
        RedrawRequested?.Invoke(this, EventArgs.Empty);
      }
    }
    public TFont Font { get; set; }
    public LineStyle LineStyle { get; set; }
    public virtual RectangleF Measure => Display?.DisplayBounds() ?? RectangleF.Empty;
    public void RecreateDisplayFromMathList() {
      var position = Display?.Position ?? default;
      Display = Typesetter.CreateLine(MathList, Font, Context, LineStyle);
      Display.Position = position;
    }
    /// <summary>Keyboard should now be hidden and input be discarded.</summary>
    public event EventHandler? DismissPressed;
    /// <summary>Keyboard should now be hidden and input be saved.</summary>
    public event EventHandler? ReturnPressed;
    /// <summary><see cref="Display"/> should be redrawn.</summary>
    public event EventHandler? RedrawRequested;
    public PointF? ClosestPointToIndex(MathListIndex index) =>
      Display?.PointForIndex(Context, index);
    public MathListIndex? ClosestIndexToPoint(PointF point) =>
      Display?.IndexForPoint(Context, point);
    public override void KeyPress(MathKeyboardInput input) {
      base.KeyPress(input);
      ResetPlaceholders(MathList);
      CaretState = MathKeyboardCaretState.Shown;
    }

    public void MoveCaretToPoint(PointF point) {
      point.Y *= -1; //inverted canvas, blah blah
      InsertionIndex = ClosestIndexToPoint(point) ?? MathListIndex.Level0Index(MathList.Atoms.Count);
    }

    // Insert a list at a given point.
    public void InsertMathList(MathList list, PointF point) {
      var detailedIndex = ClosestIndexToPoint(point) ?? MathListIndex.Level0Index(0);
      // insert at the given index - but don't consider sublevels at this point
      var index = MathListIndex.Level0Index(detailedIndex.AtomIndex);
      foreach (var atom in list.Atoms) {
        MathList.InsertAndAdvance(ref index, atom, MathListSubIndexType.None);
      }
      InsertionIndex = index; // move the index to the end of the new list.
    }

    public void HighlightCharacterAt(MathListIndex index, Color color) {
      // setup highlights before drawing the MTLine
      Display?.HighlightCharacterAt(index, color);
      RedrawRequested?.Invoke(this, EventArgs.Empty);
    }

    public void ClearHighlights() {
      RecreateDisplayFromMathList();
      RedrawRequested?.Invoke(this, EventArgs.Empty);
    }
  }
}