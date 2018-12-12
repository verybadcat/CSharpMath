#define UNUSED_KEYBOARD_FEATURES
//#undef UNUSED_KEYBOARD_FEATURES
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  using Atoms;
  using Constants;
  using Enumerations;
  using Interfaces;
  public class Keyboard {
    public MathListIndex InsertionIndex { get; set; }
    public MathList MathList { get; set; }
    public bool HasText => MathList?.Atoms?.Count > 0;
    public event EventHandler DismissPressed;
    public event EventHandler ReturnPressed;
    public event EventHandler RedrawRequested;

    private static MathAtom AtomForKeyPress(KeyboardInput input) {
      var c = (char)input;
      // Get the basic conversion from MathAtoms, and then special case unicode characters and latex special characters.
      switch (input) {
        //https://github.com/kostub/MathEditor/blob/61f67c6224000c224e252f6eeba483003f11d3d5/mathEditor/editor/MTEditableMathLabel.m#L414
        case KeyboardInput.Multiply:
        case KeyboardInput.Multiply_:
          return MathAtoms.Times;
        case KeyboardInput.SquareRoot:
          return MathAtoms.PlaceholderSquareRoot;
        case KeyboardInput.CubeRoot:
          var sqroot = MathAtoms.PlaceholderSquareRoot;
          sqroot.Degree = MathLists.WithAtoms(MathAtoms.ForCharacter('3'));
          return sqroot;
        case KeyboardInput.Infinity:
        case KeyboardInput.Degree:
        case KeyboardInput.Angle:
          return MathAtoms.Create(MathAtomType.Ordinary, c);
        case KeyboardInput.Divide:
        case KeyboardInput.Slash:
          return MathAtoms.Divide;
        case KeyboardInput.Fraction:
          return MathAtoms.PlaceholderFraction;
        case KeyboardInput.LeftCurlyBracket:
          return MathAtoms.Create(MathAtomType.Open, c);
        case KeyboardInput.RightCurlyBracket:
          return MathAtoms.Create(MathAtomType.Close, c);
        case KeyboardInput.GreaterOrEquals:
        case KeyboardInput.LessOrEquals:
          return MathAtoms.Create(MathAtomType.Relation, c);
        case var _ when c >= UnicodeFontChanger.UnicodeGreekLowerStart && c <= UnicodeFontChanger.UnicodeGreekLowerEnd:
          // All greek letters are rendered as variables.
          return MathAtoms.Create(MathAtomType.Variable, c);
        case var _ when c >= UnicodeFontChanger.UnicodeGreekUpperStart && c <= UnicodeFontChanger.UnicodeGreekUpperEnd:
          // Including capital greek letters
          return MathAtoms.Create(MathAtomType.Variable, c);
#if UNUSED_KEYBOARD_FEATURES
        case var _ when c < '\x21' || c > '\x7E' || c is '\'' || c is '~':
          // Not ascii
          return null;
#endif
        case var _ when MathAtoms.ForCharacter(c) is MathAtom atom:
          return atom;
        default:
          //Just an ordinary character
          return MathAtoms.Create(MathAtomType.Ordinary, c);
      }
    }

    public void KeyPress(KeyboardInput input) {
#if UNUSED_KEYBOARD_FEATURES
      /// <returns>True if updated</returns>
      bool UpdatePlaceholderIfPresent(IMathAtom emptyAtom) {
        var current = MathList.AtomAt(InsertionIndex);
        if (current?.AtomType is MathAtomType.Placeholder) {
          if (current.Superscript is IMathList super)
            emptyAtom.Superscript = super;
          if (current.Subscript is IMathList sub)
            emptyAtom.Subscript = sub;
          //Remove the placeholder and replace with emptyAtom.
          MathList.RemoveAt(InsertionIndex);
          MathList.Insert(InsertionIndex, emptyAtom);
          return true;
        }
        return false;
      }
      ///<summary>If the index is in a radical, subscript, or exponent, fetches the next index after the root atom.</summary>
      MathListIndex GetIndexAfterSpecialStructure(MathListIndex index, MathListSubIndexType type) {
        while (index.HasSubIndexOfType(type))
          index = index.LevelDown();
        //Point to just after this node.
        return index.Next;
      }
      MathListIndex GetOutOfRadical(MathListIndex index) {
        if (index.HasSubIndexOfType(MathListSubIndexType.Degree))
          index = GetIndexAfterSpecialStructure(index, MathListSubIndexType.Degree);
        if (index.HasSubIndexOfType(MathListSubIndexType.Radicand))
          index = GetIndexAfterSpecialStructure(index, MathListSubIndexType.Radicand);
        return index;
      }
#endif
      void HandleExponentButton() {
#if UNUSED_KEYBOARD_FEATURES
        if (InsertionIndex.HasSubIndexOfType(MathListSubIndexType.Superscript))
          // The index is currently inside an exponent. The exponent button gets it out of the exponent and move forward.
          InsertionIndex = GetIndexAfterSpecialStructure(InsertionIndex, MathListSubIndexType.Superscript);
        else {
          //Not in an exponent. Add one.
#endif
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
#if UNUSED_KEYBOARD_FEATURES
        }
#endif
      }

      void HandleRadical(bool placeholderDegree, bool degreeIs3) {
        var current = InsertionIndex;
#if UNUSED_KEYBOARD_FEATURES
        if ((current.HasSubIndexOfType(MathListSubIndexType.Degree) || current.HasSubIndexOfType(MathListSubIndexType.Radicand)) && MathList.Atoms[current.AtomIndex] is Radical rad)
          if (placeholderDegree)
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
        else
#endif
        if (placeholderDegree) {
          rad = MathAtoms.PlaceholderRadical;
          MathList.Insert(current, rad);
          InsertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
        } else {
          rad = MathAtoms.PlaceholderSquareRoot;
          if (degreeIs3) rad.Degree = MathLists.WithAtoms(MathAtoms.ForCharacter('3'));
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
          numerator.Add(MathAtoms.ForCharacter('1'));
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
        
        MathList.Insert(InsertionIndex, MathAtoms.ForCharacter('('));
        InsertionIndex = InsertionIndex.Next;
        MathList.Insert(InsertionIndex, MathAtoms.ForCharacter(')'));
        // Don't go to the next insertion index, to start inserting before the close parens.
      }
      void InsertAbsValue() {
        MathList.Insert(InsertionIndex, MathAtoms.ForCharacter('|'));
        InsertionIndex = InsertionIndex.Next;
        MathList.Insert(InsertionIndex, MathAtoms.ForCharacter('|'));
        // Don't go to the next insertion index, to start inserting before the second absolute value
      }
      void MoveCursorLeft() {
        if (InsertionIndex is null)
          throw new InvalidOperationException($"{nameof(InsertionIndex)} is null.");
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
          } else if (InsertionIndex.Previous is MathListIndex prev)
          switch (MathList.AtomAt(prev)) {
            case null:
            default:
              InsertionIndex = prev;
              break;
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
          }
        if (InsertionIndex is null)
          throw new InvalidOperationException($"{nameof(InsertionIndex)} is null.");
        RedrawRequested?.Invoke(this, EventArgs.Empty);
      }

      void MoveCursorRight() {
        if (InsertionIndex is null)
          throw new InvalidOperationException($"{nameof(InsertionIndex)} is null.");
        switch (MathList.AtomAt(InsertionIndex)) {
          case null when MathList.AtomAt(InsertionIndex) is null: //After Count
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
          case null:
          default:
            InsertionIndex = InsertionIndex.Next;
            break;
          case IFraction frac:
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
            break;
          case IRadical rad:
            if (rad.Degree is IMathList)
              InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
            else
              InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
            break;
          case var a when a.Superscript != null:
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
            break;
          case var a when a.Subscript != null:
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
            break;
        }
        if (InsertionIndex is null)
          throw new InvalidOperationException($"{nameof(InsertionIndex)} is null.");
        RedrawRequested?.Invoke(this, EventArgs.Empty);
      }

      void DeleteBackwards() {
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
            var insertionAtom = MathList.AtomAt(InsertionIndex);
            if (insertionAtom is null) {
              // add a placeholder if we deleted everything in the list
              insertionAtom = MathAtoms.Placeholder;
              // mark the placeholder as selected since that is the current insertion point.
              insertionAtom.Nucleus = Symbols.BlackSquare.ToString();
              MathList.Insert(InsertionIndex, insertionAtom);
            }
          }
        }
      }

      void InsertAtom(MathAtom a) {
        if (!UpdatePlaceholderIfPresent(a))
          // If a placeholder wasn't updated then insert the new element.
          MathList.Insert(InsertionIndex, a);
        if (a.AtomType is MathAtomType.Fraction)
          // go to the numerator
          InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
        else
          InsertionIndex = InsertionIndex.Next;
      }
      void InsertKey(KeyboardInput i) => InsertAtom(AtomForKeyPress(i));
      void RemovePlaceholderIfPresent() {
        var current = MathList.AtomAt(InsertionIndex);
        if (current is null) //Might be at end of MathList
          current = MathList.AtomAt(InsertionIndex.Previous);
        if (current?.AtomType is MathAtomType.Placeholder)
          // Remove this element - the inserted text replaces the placeholder
          MathList.RemoveAt(InsertionIndex);
      }

      var atom = AtomForKeyPress(input);
      if (InsertionIndex.SubIndexType is MathListSubIndexType.Denominator && atom.AtomType is MathAtomType.Relation)
        // pull the insertion index out
        InsertionIndex = InsertionIndex.LevelDown().Next;

      switch (input) {
#warning Unimplemented up/down buttons
        case KeyboardInput.Up:
          break;
        case KeyboardInput.Down:
          break;
        case KeyboardInput.Left:
          MoveCursorLeft();
          break;
        case KeyboardInput.Right:
          MoveCursorRight();
          break;
        case KeyboardInput.Backspace:
          DeleteBackwards();
          break;
        case KeyboardInput.Clear:
          MathList.Clear();
          break;
        case KeyboardInput.Return:
          ReturnPressed?.Invoke(this, EventArgs.Empty);
          break;
        case KeyboardInput.Dismiss:
          DismissPressed?.Invoke(this, EventArgs.Empty);
          break;
        case KeyboardInput.BothRoundBrackets:
          InsertParens();
          break;
        case KeyboardInput.LeftRoundBracket:
        case KeyboardInput.RightRoundBracket:
        case KeyboardInput.LeftSquareBracket:
        case KeyboardInput.RightSquareBracket:
        case KeyboardInput.LeftCurlyBracket:
        case KeyboardInput.RightCurlyBracket:
        case KeyboardInput.D0:
        case KeyboardInput.D1:
        case KeyboardInput.D2:
        case KeyboardInput.D3:
        case KeyboardInput.D4:
        case KeyboardInput.D5:
        case KeyboardInput.D6:
        case KeyboardInput.D7:
        case KeyboardInput.D8:
        case KeyboardInput.D9:
        case KeyboardInput.Decimal:
        case KeyboardInput.Plus:
        case KeyboardInput.Minus:
        case KeyboardInput.Minus_:
        case KeyboardInput.Multiply:
        case KeyboardInput.Multiply_:
        case KeyboardInput.Divide:
        case KeyboardInput.Fraction:
        case KeyboardInput.Ratio:
        case KeyboardInput.Ratio_:
        case KeyboardInput.Percentage:
        case KeyboardInput.Comma:
        case KeyboardInput.Factorial:
        case KeyboardInput.Infinity:
        case KeyboardInput.Angle:
        case KeyboardInput.Degree:
          InsertKey(input);
          break;
        case KeyboardInput.Slash:
          HandleSlashButton();
          break;
        case KeyboardInput.Power:
          HandleExponentButton();
          break;
        case KeyboardInput.Subscript:
          HandleSubscriptButton();
          break;
        case KeyboardInput.SquareRoot:
          HandleRadical(false, false);
          break;
        case KeyboardInput.CubeRoot:
          HandleRadical(false, true);
          break;
        case KeyboardInput.NthRoot:
          HandleRadical(true, false);
          break;
        case KeyboardInput.VerticalBar:
          break;
        case KeyboardInput.Absolute:
          break;
        case KeyboardInput.BaseEPower:
          break;
        case KeyboardInput.Logarithm:
          break;
        case KeyboardInput.NaturalLogarithm:
          break;
        case KeyboardInput.LogarithmWithBase:
          break;
        case KeyboardInput.Equals:
          break;
        case KeyboardInput.NotEquals:
          break;
        case KeyboardInput.LessThan:
          break;
        case KeyboardInput.LessOrEquals:
          break;
        case KeyboardInput.GreaterThan:
          break;
        case KeyboardInput.GreaterOrEquals:
          break;
        case KeyboardInput.A:
          break;
        case KeyboardInput.B:
          break;
        case KeyboardInput.C:
          break;
        case KeyboardInput.D:
          break;
        case KeyboardInput.E:
          break;
        case KeyboardInput.F:
          break;
        case KeyboardInput.G:
          break;
        case KeyboardInput.H:
          break;
        case KeyboardInput.I:
          break;
        case KeyboardInput.J:
          break;
        case KeyboardInput.K:
          break;
        case KeyboardInput.L:
          break;
        case KeyboardInput.M:
          break;
        case KeyboardInput.N:
          break;
        case KeyboardInput.O:
          break;
        case KeyboardInput.P:
          break;
        case KeyboardInput.Q:
          break;
        case KeyboardInput.R:
          break;
        case KeyboardInput.S:
          break;
        case KeyboardInput.T:
          break;
        case KeyboardInput.U:
          break;
        case KeyboardInput.V:
          break;
        case KeyboardInput.W:
          break;
        case KeyboardInput.X:
          break;
        case KeyboardInput.Y:
          break;
        case KeyboardInput.Z:
          break;
        case KeyboardInput.SmallA:
          break;
        case KeyboardInput.SmallB:
          break;
        case KeyboardInput.SmallC:
          break;
        case KeyboardInput.SmallD:
          break;
        case KeyboardInput.SmallE:
          break;
        case KeyboardInput.SmallF:
          break;
        case KeyboardInput.SmallG:
          break;
        case KeyboardInput.SmallH:
          break;
        case KeyboardInput.SmallI:
          break;
        case KeyboardInput.SmallJ:
          break;
        case KeyboardInput.SmallK:
          break;
        case KeyboardInput.SmallL:
          break;
        case KeyboardInput.SmallM:
          break;
        case KeyboardInput.SmallN:
          break;
        case KeyboardInput.SmallO:
          break;
        case KeyboardInput.SmallP:
          break;
        case KeyboardInput.SmallQ:
          break;
        case KeyboardInput.SmallR:
          break;
        case KeyboardInput.SmallS:
          break;
        case KeyboardInput.SmallT:
          break;
        case KeyboardInput.SmallU:
          break;
        case KeyboardInput.SmallV:
          break;
        case KeyboardInput.SmallW:
          break;
        case KeyboardInput.SmallX:
          break;
        case KeyboardInput.SmallY:
          break;
        case KeyboardInput.SmallZ:
          break;
        case KeyboardInput.Alpha:
          break;
        case KeyboardInput.Beta:
          break;
        case KeyboardInput.Gamma:
          break;
        case KeyboardInput.Delta:
          break;
        case KeyboardInput.Epsilon:
          break;
        case KeyboardInput.Zeta:
          break;
        case KeyboardInput.Eta:
          break;
        case KeyboardInput.Theta:
          break;
        case KeyboardInput.Iota:
          break;
        case KeyboardInput.Kappa:
          break;
        case KeyboardInput.Lambda:
          break;
        case KeyboardInput.Mu:
          break;
        case KeyboardInput.Nu:
          break;
        case KeyboardInput.Xi:
          break;
        case KeyboardInput.Omicron:
          break;
        case KeyboardInput.Pi:
          break;
        case KeyboardInput.Rho:
          break;
        case KeyboardInput.Sigma:
          break;
        case KeyboardInput.Tau:
          break;
        case KeyboardInput.Upsilon:
          break;
        case KeyboardInput.Phi:
          break;
        case KeyboardInput.Chi:
          break;
        case KeyboardInput.Omega:
          break;
        case KeyboardInput.SmallAlpha:
          break;
        case KeyboardInput.SmallBeta:
          break;
        case KeyboardInput.SmallGamma:
          break;
        case KeyboardInput.SmallDelta:
          break;
        case KeyboardInput.SmallEpsilon:
          break;
        case KeyboardInput.SmallZeta:
          break;
        case KeyboardInput.SmallEta:
          break;
        case KeyboardInput.SmallTheta:
          break;
        case KeyboardInput.SmallIota:
          break;
        case KeyboardInput.SmallKappa:
          break;
        case KeyboardInput.SmallLambda:
          break;
        case KeyboardInput.SmallMu:
          break;
        case KeyboardInput.SmallNu:
          break;
        case KeyboardInput.SmallXi:
          break;
        case KeyboardInput.SmallOmicron:
          break;
        case KeyboardInput.SmallPi:
          break;
        case KeyboardInput.SmallRho:
          break;
        case KeyboardInput.SmallSigma:
          break;
        case KeyboardInput.SmallSigma2:
          break;
        case KeyboardInput.SmallTau:
          break;
        case KeyboardInput.SmallUpsilon:
          break;
        case KeyboardInput.SmallPhi:
          break;
        case KeyboardInput.SmallChi:
          break;
        case KeyboardInput.SmallOmega:
          break;
        case KeyboardInput.Sine:
          break;
        case KeyboardInput.Cosine:
          break;
        case KeyboardInput.Tangent:
          break;
        case KeyboardInput.Cotangent:
          break;
        case KeyboardInput.Secant:
          break;
        case KeyboardInput.Cosecant:
          break;
        case KeyboardInput.ArcSine:
          break;
        case KeyboardInput.ArcCosine:
          break;
        case KeyboardInput.ArcTangent:
          break;
        case KeyboardInput.ArcCotangent:
          break;
        case KeyboardInput.ArcSecant:
          break;
        case KeyboardInput.ArcCosecant:
          break;
        case KeyboardInput.HyperbolicSine:
          break;
        case KeyboardInput.HyperbolicCosine:
          break;
        case KeyboardInput.HyperbolicTangent:
          break;
        case KeyboardInput.HyperbolicCotangent:
          break;
        case KeyboardInput.HyperbolicSecant:
          break;
        case KeyboardInput.HyperbolicCosecant:
          break;
        case KeyboardInput.AreaHyperbolicSine:
          break;
        case KeyboardInput.AreaHyperbolicCosine:
          break;
        case KeyboardInput.AreaHyperbolicTangent:
          break;
        case KeyboardInput.AreaHyperbolicCotangent:
          break;
        case KeyboardInput.AreaHyperbolicSecant:
          break;
        case KeyboardInput.AreaHyperbolicCosecant:
          break;
        default:
          break;
      }

      switch (str) {
        case "^":
          // Special ^ handling - adds an exponent
          HandleExponentButton();
          break;
        case Symbols.SquareRoot:
          HandleRadical(false);
          break;
        case Symbols.CubeRoot:
          HandleRadical(true);
          break;
        case "_":
          HandleSubscriptButton();
          break;
        case "/":
          HandleSlashButton();
          break;
        case "()":
          RemovePlaceholderIfPresent();
          InsertParens();
          break;
        case "||":
          RemovePlaceholderIfPresent();
          InsertAbsValue();
          break;
        case var _ when atom is IMathAtom a:
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
  }
}
