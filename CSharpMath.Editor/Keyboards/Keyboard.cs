#define UNUSED_KEYBOARD_FEATURES
//#undef UNUSED_KEYBOARD_FEATURES
namespace CSharpMath.Editor {
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using Atoms;
  using Constants;
  using Display;
  using Enumerations;
  using FrontEnd;
  using Interfaces;
  public class Keyboard<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public Keyboard(TypesettingContext<TFont, TGlyph> context, LineStyle style = LineStyle.Display)
      => (_context, LineStyle) = (context, style);

    protected CaretHandle? _caret;
    public CaretHandle? Caret => _caret;
    private TypesettingContext<TFont, TGlyph> _context;
    public MathList MathList { get; } = new MathList();
    public MathListIndex InsertionIndex { get; set; }
    public TFont Font { get; set; }
    public LineStyle LineStyle { get; set; }
    public IDisplay<TFont, TGlyph> Display => _display;
    protected IDisplay<TFont, TGlyph> _display;
    protected void UpdateDisplay() {
      var position = _display?.Position ?? default;
      _display = _context.CreateLine(MathList, Font, LineStyle);
      _display.Position = position;
    }

    public bool HasText => MathList?.Atoms?.Count > 0;
    public event EventHandler DismissPressed;
    public event EventHandler ReturnPressed;
    public event EventHandler RedrawRequested;

    private bool IndexAtEmptyPlaceholder(out IMathAtom placeholder) {
      placeholder = MathList.AtomAt(InsertionIndex) ??
        MathList.AtomAt(InsertionIndex?.Previous); //Might be at end of MathList
      return placeholder != null && placeholder.AtomType is MathAtomType.Placeholder &&
             placeholder.Superscript is null && placeholder.Subscript is null;
    }
    
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
      /// <summary>Returns the position of the cursor. If null, the cursor should be hidden.</summary>
      PointF? Inner() {
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
          RemovePlaceholderIfPresent();
          MathList.Insert(InsertionIndex, MathAtoms.ForCharacter('('));
          InsertionIndex = InsertionIndex.Next;
          MathList.Insert(InsertionIndex, MathAtoms.ForCharacter(')'));
          // Don't go to the next insertion index, to start inserting before the close parens.
        }
        void InsertAbsValue() {
          RemovePlaceholderIfPresent();
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

        void InsertAtom(IMathAtom a) {
          if (!UpdatePlaceholderIfPresent(a))
            // If a placeholder wasn't updated then insert the new element.
            MathList.Insert(InsertionIndex, a);
          if (a.AtomType is MathAtomType.Fraction)
            // go to the numerator
            InsertionIndex = InsertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
          else
            InsertionIndex = InsertionIndex.Next;
        }
        void InsertCharacterKey(KeyboardInput i) => InsertAtom(AtomForKeyPress(i));
        void InsertSymbolName(string s) => InsertAtom(MathAtoms.ForLatexSymbolName(s));

        void RemovePlaceholderIfPresent() {
          if (IndexAtEmptyPlaceholder(out var placeholder))
            // Remove this element - the inserted text replaces the placeholder
            MathList.Remove(placeholder);
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
            return null;
          case KeyboardInput.Dismiss:
            DismissPressed?.Invoke(this, EventArgs.Empty);
            return null;
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
            InsertCharacterKey(input);
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
            InsertCharacterKey(input);
            break;
          case KeyboardInput.Absolute:
            InsertAbsValue();
            break;
          case KeyboardInput.BaseEPower:
            InsertCharacterKey(KeyboardInput.SmallE);
            HandleExponentButton();
            break;
          case KeyboardInput.Logarithm:
            InsertSymbolName("log");
            break;
          case KeyboardInput.NaturalLogarithm:
            InsertSymbolName("ln");
            break;
          case KeyboardInput.LogarithmWithBase:
            InsertSymbolName("log");
            HandleSubscriptButton();
            break;
          case KeyboardInput.Equals:
          case KeyboardInput.NotEquals:
          case KeyboardInput.LessThan:
          case KeyboardInput.LessOrEquals:
          case KeyboardInput.GreaterThan:
          case KeyboardInput.GreaterOrEquals:
          case KeyboardInput.A:
          case KeyboardInput.B:
          case KeyboardInput.C:
          case KeyboardInput.D:
          case KeyboardInput.E:
          case KeyboardInput.F:
          case KeyboardInput.G:
          case KeyboardInput.H:
          case KeyboardInput.I:
          case KeyboardInput.J:
          case KeyboardInput.K:
          case KeyboardInput.L:
          case KeyboardInput.M:
          case KeyboardInput.N:
          case KeyboardInput.O:
          case KeyboardInput.P:
          case KeyboardInput.Q:
          case KeyboardInput.R:
          case KeyboardInput.S:
          case KeyboardInput.T:
          case KeyboardInput.U:
          case KeyboardInput.V:
          case KeyboardInput.W:
          case KeyboardInput.X:
          case KeyboardInput.Y:
          case KeyboardInput.Z:
          case KeyboardInput.SmallA:
          case KeyboardInput.SmallB:
          case KeyboardInput.SmallC:
          case KeyboardInput.SmallD:
          case KeyboardInput.SmallE:
          case KeyboardInput.SmallF:
          case KeyboardInput.SmallG:
          case KeyboardInput.SmallH:
          case KeyboardInput.SmallI:
          case KeyboardInput.SmallJ:
          case KeyboardInput.SmallK:
          case KeyboardInput.SmallL:
          case KeyboardInput.SmallM:
          case KeyboardInput.SmallN:
          case KeyboardInput.SmallO:
          case KeyboardInput.SmallP:
          case KeyboardInput.SmallQ:
          case KeyboardInput.SmallR:
          case KeyboardInput.SmallS:
          case KeyboardInput.SmallT:
          case KeyboardInput.SmallU:
          case KeyboardInput.SmallV:
          case KeyboardInput.SmallW:
          case KeyboardInput.SmallX:
          case KeyboardInput.SmallY:
          case KeyboardInput.SmallZ:
          case KeyboardInput.Alpha:
          case KeyboardInput.Beta:
          case KeyboardInput.Gamma:
          case KeyboardInput.Delta:
          case KeyboardInput.Epsilon:
          case KeyboardInput.Zeta:
          case KeyboardInput.Eta:
          case KeyboardInput.Theta:
          case KeyboardInput.Iota:
          case KeyboardInput.Kappa:
          case KeyboardInput.Lambda:
          case KeyboardInput.Mu:
          case KeyboardInput.Nu:
          case KeyboardInput.Xi:
          case KeyboardInput.Omicron:
          case KeyboardInput.Pi:
          case KeyboardInput.Rho:
          case KeyboardInput.Sigma:
          case KeyboardInput.Tau:
          case KeyboardInput.Upsilon:
          case KeyboardInput.Phi:
          case KeyboardInput.Chi:
          case KeyboardInput.Omega:
          case KeyboardInput.SmallAlpha:
          case KeyboardInput.SmallBeta:
          case KeyboardInput.SmallGamma:
          case KeyboardInput.SmallDelta:
          case KeyboardInput.SmallEpsilon:
          case KeyboardInput.SmallZeta:
          case KeyboardInput.SmallEta:
          case KeyboardInput.SmallTheta:
          case KeyboardInput.SmallIota:
          case KeyboardInput.SmallKappa:
          case KeyboardInput.SmallLambda:
          case KeyboardInput.SmallMu:
          case KeyboardInput.SmallNu:
          case KeyboardInput.SmallXi:
          case KeyboardInput.SmallOmicron:
          case KeyboardInput.SmallPi:
          case KeyboardInput.SmallRho:
          case KeyboardInput.SmallSigma:
          case KeyboardInput.SmallSigma2:
          case KeyboardInput.SmallTau:
          case KeyboardInput.SmallUpsilon:
          case KeyboardInput.SmallPhi:
          case KeyboardInput.SmallChi:
          case KeyboardInput.SmallOmega:
            InsertCharacterKey(input);
            break;
          case KeyboardInput.Sine:
            InsertSymbolName("sin");
            break;
          case KeyboardInput.Cosine:
            InsertSymbolName("cos");
            break;
          case KeyboardInput.Tangent:
            InsertSymbolName("tan");
            break;
          case KeyboardInput.Cotangent:
            InsertSymbolName("cot");
            break;
          case KeyboardInput.Secant:
            InsertSymbolName("sec");
            break;
          case KeyboardInput.Cosecant:
            InsertSymbolName("csc");
            break;
          case KeyboardInput.ArcSine:
            InsertSymbolName("arcsin");
            break;
          case KeyboardInput.ArcCosine:
            InsertSymbolName("arccos");
            break;
          case KeyboardInput.ArcTangent:
            InsertSymbolName("arctan");
            break;
          case KeyboardInput.ArcCotangent:
            InsertSymbolName("arccot");
            break;
          case KeyboardInput.ArcSecant:
            InsertSymbolName("arcsec");
            break;
          case KeyboardInput.ArcCosecant:
            InsertSymbolName("arccsc");
            break;
          case KeyboardInput.HyperbolicSine:
            InsertSymbolName("sinh");
            break;
          case KeyboardInput.HyperbolicCosine:
            InsertSymbolName("cosh");
            break;
          case KeyboardInput.HyperbolicTangent:
            InsertSymbolName("tanh");
            break;
          case KeyboardInput.HyperbolicCotangent:
            InsertSymbolName("coth");
            break;
          case KeyboardInput.HyperbolicSecant:
            InsertSymbolName("sech");
            break;
          case KeyboardInput.HyperbolicCosecant:
            InsertSymbolName("csch");
            break;
          case KeyboardInput.AreaHyperbolicSine:
            InsertSymbolName("arsinh");
            break;
          case KeyboardInput.AreaHyperbolicCosine:
            InsertSymbolName("arconh");
            break;
          case KeyboardInput.AreaHyperbolicTangent:
            InsertSymbolName("artanh");
            break;
          case KeyboardInput.AreaHyperbolicCotangent:
            InsertSymbolName("arcoth");
            break;
          case KeyboardInput.AreaHyperbolicSecant:
            InsertSymbolName("arsech");
            break;
          case KeyboardInput.AreaHyperbolicCosecant:
            InsertSymbolName("arcsch");
            break;
          default:
            break;
        }
#if UNUSED_KEYBOARD_FEATURES
        // If trig function, insert parens after
        switch (input) {
          case KeyboardInput.Sine:
          case KeyboardInput.Cosine:
          case KeyboardInput.Tangent:
          case KeyboardInput.Cotangent:
          case KeyboardInput.Secant:
          case KeyboardInput.Cosecant:
            InsertParens();
            break;
        }
#endif
        return InsertionPointChanged();
      }
      _caret = Inner() is PointF point ? new CaretHandle(Font.PointSize, point) : new CaretHandle?();
    }

    private static void ClearPlaceholders(IMathList mathList) {
      foreach (var atom in (IList<IMathAtom>)mathList?.Atoms ?? Array.Empty<IMathAtom>()) {
        if (atom.AtomType is MathAtomType.Placeholder)
          atom.Nucleus = Symbols.WhiteSquare;
        if (atom.Superscript is IMathList super)
          ClearPlaceholders(super);
        if (atom.Subscript is IMathList sub)
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

    /// <summary>Helper method to update caretView when insertion point/selection changes.</summary>
    private PointF? InsertionPointChanged() {
      PointF? Inner() {
#if UNUSED_KEYBOARD_FEATURES
        // If not in editing mode, we don't show the caret.
        bool isEditing = new int().GetHashCode() == new int().GetHashCode();
        if (!isEditing) {
#warning INCOMPLETE: REVISIT
          /*
          [_caretView removeFromSuperview];
          self.cancelImage.hidden = YES;
          */
          return null;
        }
#endif
        ClearPlaceholders(MathList);
        if (IndexAtEmptyPlaceholder(out var atom)) {
          atom.Nucleus = Symbols.BlackSquare;
          if (InsertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus) {
            // If the insertion index is inside a placeholder, move it out.
            InsertionIndex = InsertionIndex.LevelDown();
          }
          // TODO - disable caret
        }
#if UNUSED_KEYBOARD_FEATURES
      else {
          var previousIndex = InsertionIndex.Previous;
          atom = MathList.AtomAt(previousIndex);
          if (atom != null && atom.AtomType is MathAtomType.Placeholder &&
             atom.Superscript is null && atom.Subscript is null) {
            InsertionIndex = previousIndex;
            atom.Nucleus = Symbols.BlackSquare;
            // TODO - disable caret
          }
        }
#endif
        //SetKeyboardMode();

        /* Find the insert point rect and create a caretView to draw the caret at this position. */

        // Check that we were returned a valid position before displaying a caret there.
        return CaretRectForIndex(InsertionIndex);



#warning INCOMPLETE: REVISIT

        /*
      //if caretPosition isnt null then.....
       
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
      var @return = Inner();
      RedrawRequested?.Invoke(this, EventArgs.Empty);
      return @return;
    }

    public PointF? CaretRectForIndex(MathListIndex index) {
      UpdateDisplay();
      // no mathlist so we can't figure it out.
      if (_display is null) return PointF.Empty;
      return _display.PointForIndex(_context, index);
    }

    public MathListIndex ClosestIndexToPoint(PointF point) {
      UpdateDisplay();
      // no mathlist, so can't figure it out.
      if (MathList is null) return null;
      return _display.IndexForPoint(_context, point);
    }

    public void Clear() {
      MathList.Clear();
      InsertionPointChanged();
    }

    public void MoveCaretToPoint(PointF point) {
      InsertionIndex = ClosestIndexToPoint(point);
      _caret = null;
      InsertionPointChanged();
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

    public void SelectCharacterAtIndex(MathListIndex index, Structures.Color color) {
      UpdateDisplay();
      if (_display is null)
        // no mathlist so we can't figure it out.
        return;
      // setup highlights before drawing the MTLine
      _display.HighlightCharacterAt(index, color);
    }

    public void ClearHighlights() => UpdateDisplay();

#if false && UNUSED_KEYBOARD_FEATURES
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
#endif
  }
}
