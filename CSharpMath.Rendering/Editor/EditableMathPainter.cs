using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
#warning Proper private/public scope
namespace CSharpMath.Rendering {
  using Atoms;
  using Constants;
  using Editor;
  using Enumerations;
  using Interfaces;
  public abstract class EditableMathPainter<TCanvas, TColor, TButton, TTextView> : MathPainter<TCanvas, TColor> where TButton : IButton where TTextView : class, ITextView {
    protected EditableMathPainter(float fontSize = DefaultFontSize * 3 / 2) : base(fontSize) { }

    readonly CaretView<Fonts, Glyph> caretView;
    readonly List<MathListIndex> highlighted;
    readonly MathKeyboardView<TButton, TTextView> keyboard;
    TTextView textView;
    MathListIndex insertionIndex;
    protected override void SetRedisplay() {
      base.SetRedisplay();
      insertionIndex = MathListIndex.Level0Index(MathList.Atoms.Count);
      InsertionPointChanged();
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

    public bool HasText => MathList.Atoms.Count > 0;
    public Structures.Color SelectColor { get; set; }

    bool isEditing;
    public void StartEditing() {
      if (isEditing) return;
      isEditing = true;
      if (insertionIndex is null)
        insertionIndex = MathListIndex.Level0Index(MathList.Atoms.Count);
      keyboard.StartedEditing(textView);
      InsertionPointChanged();
      BeginEditing?.Invoke(this, EventArgs.Empty);
    }
    public void FinishEditing() {
      if (!isEditing) return;
      isEditing = false;
      keyboard.FinishedEditing(textView);
      InsertionPointChanged();
      EndEditing?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler BeginEditing;
    public event EventHandler EndEditing;
    public event EventHandler TextModified;

    public void Tap(PointF point) {
      if (!isEditing) {
        insertionIndex = null;
        caretView.showHandle = false;
        StartEditing();
      } else {
        // If already editing move the cursor and show handle
        insertionIndex = ClosestIndexToPoint(point) ??
          MathListIndex.Level0Index(MathList.Atoms.Count);
        caretView.showHandle = false;
        InsertionPointChanged();
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
      InsertionPointChanged();
    }

    public void MoveCaretToPoint(PointF point){
      insertionIndex = ClosestIndexToPoint(point);
      caretView.showHandle = false;
      InsertionPointChanged();
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
      var current = MathList.AtomAt(insertionIndex);
      if (current.AtomType is MathAtomType.Placeholder)
        // Remove this element - the inserted text replaces the placeholder
        MathList.RemoveAt(insertionIndex);
    }
    
    /// <returns>True if updated</returns>
    public bool UpdatePlaceholderIfPresent(IMathAtom atom) {
      var current = MathList.AtomAt(insertionIndex);
      if(current.AtomType is MathAtomType.Placeholder) {
        if (current.Superscript is IMathList super)
          atom.Superscript = super;
        if (current.Subscript is IMathList sub)
          atom.Subscript = sub;
        //Remove the placeholder and replace with atom.
        MathList.RemoveAt(insertionIndex);
        MathList.Insert(insertionIndex, atom);
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

    public void HandleExponentButton() {
      if (insertionIndex.HasSubIndexOfType(MathListSubIndexType.Superscript))
        // The index is currently inside an exponent. The exponent button gets it out of the exponent and move forward.
        insertionIndex = GetIndexAfterSpecialStructure(insertionIndex, MathListSubIndexType.Superscript);
      else {
        //Not in an exponent. Add one.
        if (!insertionIndex.AtBeginningOfLine) {
          var atom = MathList.AtomAt(insertionIndex.Previous);
          if (atom.Superscript is null) {
            atom.Superscript = MathAtoms.PlaceholderList;
            insertionIndex = insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
          } else if (insertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus)
            // If we are already inside the nucleus, then we come out and go up to the superscript
            insertionIndex = insertionIndex.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(atom.Superscript.Atoms.Count), MathListSubIndexType.Superscript);
          else
            insertionIndex = insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(atom.Superscript.Atoms.Count), MathListSubIndexType.Superscript);
        } else {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = MathAtoms.Placeholder;
          emptyAtom.Superscript = MathAtoms.PlaceholderList;
          if (!UpdatePlaceholderIfPresent(emptyAtom))
            // If the placeholder hasn't been updated then insert it.
            MathList.Insert(insertionIndex, emptyAtom);
          insertionIndex = insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
        }
      }
    }

    public void HandleSubscriptButton() {
      if (insertionIndex.HasSubIndexOfType(MathListSubIndexType.Subscript))
        // The index is currently inside an subscript. The subscript button gets it out of the subscript and move forward.
        insertionIndex = GetIndexAfterSpecialStructure(insertionIndex, MathListSubIndexType.Subscript);
      else {
        //Not in a subscript. Add one.
        if (!insertionIndex.AtBeginningOfLine) {
          var atom = MathList.AtomAt(insertionIndex.Previous);
          if (atom.Subscript is null) {
            atom.Subscript = MathAtoms.PlaceholderList;
            insertionIndex = insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
          } else if (insertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus)
            // If we are already inside the nucleus, then we come out and go down to the subscript
            insertionIndex = insertionIndex.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(atom.Subscript.Atoms.Count), MathListSubIndexType.Subscript);
          else
            insertionIndex = insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(atom.Subscript.Atoms.Count), MathListSubIndexType.Subscript);
        } else {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = MathAtoms.Placeholder;
          emptyAtom.Subscript = MathAtoms.PlaceholderList;
          if (!UpdatePlaceholderIfPresent(emptyAtom))
            // If the placeholder hasn't been updated then insert it.
            MathList.Insert(insertionIndex, emptyAtom);
          insertionIndex = insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
        }
      }
    }

    public void HandleSlashButton() {
      // special / handling - makes the thing a fraction
      var numerator = new MathList();
      var current = insertionIndex;
      for(; !current.AtBeginningOfLine; current = current.Previous) {
        var atom = MathList.AtomAt(current.Previous);
        if (atom.AtomType != Enumerations.MathAtomType.Number && atom.AtomType != Enumerations.MathAtomType.Variable)
          //We don't put this atom on the fraction
          break;
        else
          //Add the number to the beginning of the list
          numerator.Insert(0, atom);
      }
      if(current.AtomIndex == insertionIndex.AtomIndex) {
        // so we didn't really find any numbers before this, so make the numerator 1
        numerator.Add(AtomForCharacter('1'));
        if (!current.AtBeginningOfLine) {
          var prevAtom = MathList.AtomAt(current.Previous);
          if(prevAtom.AtomType is MathAtomType.Fraction) {
            //Add a times symbol
            MathList.Insert(current, MathAtoms.Times);
            current = current.Next;
          }
        }
      } else
        // delete stuff in the Mathlist from current to insertionIndex
        MathList.RemoveAtoms(new MathListRange(current, insertionIndex.AtomIndex - current.AtomIndex));

      //Create the fraction
      var frac = new Fraction { Numerator = numerator, Denominator = MathAtoms.PlaceholderList };

      //Insert it
      MathList.Insert(current, frac);
      //Update the insertion index to go the denominator
      insertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Denominator);
    }

    public MathListIndex GetOutOfRadical(MathListIndex index) {
      if (index.HasSubIndexOfType(MathListSubIndexType.Degree))
        index = GetIndexAfterSpecialStructure(index, MathListSubIndexType.Degree);
      if (index.HasSubIndexOfType(MathListSubIndexType.Radicand))
        index = GetIndexAfterSpecialStructure(index, MathListSubIndexType.Radicand);
      return index;
    }

    public void HandleRadical(bool degreeButtonPressed) {
      var current = insertionIndex;

      if((current.HasSubIndexOfType(MathListSubIndexType.Degree) || current.HasSubIndexOfType(MathListSubIndexType.Radicand)) && MathList.Atoms[current.AtomIndex] is Radical rad)
        if (degreeButtonPressed)
          if (rad.Degree is null) {
            rad.Degree = MathAtoms.PlaceholderList;
            insertionIndex = current.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
          } else if (current.HasSubIndexOfType(MathListSubIndexType.Radicand))
            // The radical the cursor is at has a degree. If the cursor is in the radicand, move the cursor to the degree
            insertionIndex = current.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
          else
            // If the cursor is at the degree, get out of the radical
            insertionIndex = GetOutOfRadical(current);
        else if (current.HasSubIndexOfType(MathListSubIndexType.Degree))
          // If the radical the cursor at has a degree, and the cursor is at the degree, move the cursor to the radicand.
          insertionIndex = current.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
        else
          // If the cursor is at the radicand, get out of the radical.
          insertionIndex = GetOutOfRadical(current);
      else if (degreeButtonPressed) {
        rad = MathAtoms.PlaceholderRadical;
        MathList.Insert(current, rad);
        insertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
      } else {
        rad = MathAtoms.PlaceholderSquareRoot;
        MathList.Insert(current, rad);
        insertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
      }
    }
    public void InsertParans(){
      MathList.Insert(insertionIndex, AtomForCharacter('('));
      insertionIndex = insertionIndex.Next;
      MathList.Insert(insertionIndex, AtomForCharacter(')'));
      // Don't go to the next insertion index, to start inserting before the close parens.
    }
    public void InsertAbsValue() {
      MathList.Insert(insertionIndex, AtomForCharacter('|'));
      insertionIndex = insertionIndex.Next;
      MathList.Insert(insertionIndex, AtomForCharacter('|'));
      // Don't go to the next insertion index, to start inserting before the second absolute value
    }

    public void InsertText(string str) {
      if (str is null || str is "" || str is "\n")
        return;
      /*
          if ([str isEqualToString:@"\n"]) {
        if ([self.delegate respondsToSelector:@selector(returnPressed:)]) {
            [self.delegate returnPressed:self];
        }
        return;
    }*/
      var ch = str[0];
      var atom = str.Length > 1 ? MathAtoms.ForLatexSymbolName(str) : AtomForCharacter(ch);
      if (insertionIndex.SubIndexType is MathListSubIndexType.Denominator && atom.AtomType is MathAtomType.Relation)
        // pull the insertion index out
        insertionIndex = insertionIndex.LevelDown().Next;
      switch(ch){
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
          InsertParans();
          break;
        case var _ when str is "||":
          RemovePlaceholderIfPresent();
          InsertAbsValue();
          break;
        case var _ when atom is IMathAtom a:
          if (!UpdatePlaceholderIfPresent(a))
            // If a placeholder wasn't updated then insert the new element.
            MathList.Insert(insertionIndex, a);
          if (atom.AtomType is MathAtomType.Fraction)
            // go to the numerator
            insertionIndex = insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
          else
            insertionIndex = insertionIndex.Next;
          break;
      }
      InsertionPointChanged();

      // If trig function, insert parens after
      switch (str){
        case "sin":
        case "cos":
        case "tan":
        case "sec":
        case "csc":
        case "cot":
          InsertParans();
          break;
      }
      TextModified?.Invoke(this, EventArgs.Empty);
    }
    public void DeleteBackwards() {
      // delete the last atom from the list
      var prevIndex = insertionIndex.Previous;
      if (HasText && !(prevIndex is null)){
        MathList.RemoveAt(prevIndex);
        if(prevIndex.FinalSubIndexType is MathListSubIndexType.Nucleus){
          // it was in the nucleus and we removed it, get out of the nucleus and get in the nucleus of the previous one.
          var downIndex = prevIndex.LevelDown();
          prevIndex = downIndex.Previous is MathListIndex downPrev
            ? downPrev.LevelUpWithSubIndex(MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus)
            : downIndex;
        }
        insertionIndex = prevIndex;
        if(insertionIndex.AtBeginningOfLine && insertionIndex.SubIndexType != MathListSubIndexType.None){
          // We have deleted to the beginning of the line and it is not the outermost line
          var atom = MathList.AtomAt(insertionIndex);
          if(atom is null){
            // add a placeholder if we deleted everything in the list
            atom = MathAtoms.Placeholder;
            // mark the placeholder as selected since that is the current insertion point.
            atom.Nucleus = Symbols.BlackSquare.ToString();
            MathList.Insert(insertionIndex, atom);
          }
        }

        InsertionPointChanged();
        TextModified?.Invoke(this, EventArgs.Empty);
      }
    }

    public void SelectCharacterAtIndex(MathListIndex index){
      UpdateDisplay();
      if (_display is null)
        // no mathlist so we can't figure it out.
        return;
      // setup highlights before drawing the MTLine
      _display.HighlightCharacterAt(index, SelectColor);
    }
    public void ClearHighlights() { 
      _displayChanged = true; 
      UpdateDisplay();
    }
  }
}
