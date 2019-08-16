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
  public class MathKeyboard<TFont, TGlyph> where TFont : IFont<TGlyph> {
    public MathKeyboard(TypesettingContext<TFont, TGlyph> context) => _context = context;

    private TypesettingContext<TFont, TGlyph> _context;
    private MathListIndex _insertionIndex = MathListIndex.Level0Index(0);
    protected CaretHandle? _caret;
    protected IDisplay<TFont, TGlyph> _display;
    //private readonly List<MathListIndex> highlighted;

    public CaretHandle? Caret => _caret;
    public MathList MathList { get; } = new MathList();
    public string LaTeX => MathListBuilder.MathListToString(MathList);
    public MathListIndex InsertionIndex
      { get => _insertionIndex; set { _insertionIndex = value; InsertionPointChanged(); } }
    public TFont Font { get; set; }
    public LineStyle LineStyle { get; set; }
    public Color SelectColor { get; set; }
    public IDisplay<TFont, TGlyph> Display => _display;
    public RectangleF Measure => Display.DisplayBounds;
    public bool HasText => MathList?.Atoms?.Count > 0;

    public void UpdateDisplay() {
      var position = _display?.Position ?? default;
      _display = _context.CreateLine(MathList, Font, LineStyle);
      _display.Position = position;
    }

    /// <summary>Keyboard should now be hidden and input be discarded.</summary>
    public event EventHandler DismissPressed;
    /// <summary>Keyboard should now be hidden and input be saved.</summary>
    public event EventHandler ReturnPressed;
    /// <summary><see cref="Display"/> should be redrawn.</summary>
    public event EventHandler RedrawRequested;

    private bool IndexAtEmptyPlaceholder(out IMathAtom placeholder) {
      placeholder = MathList.AtomAt(_insertionIndex) ??
        MathList.AtomAt(_insertionIndex?.Previous); //Might be at end of MathList
      return placeholder != null && placeholder.AtomType is MathAtomType.Placeholder &&
             placeholder.Superscript is null && placeholder.Subscript is null;
    }

    public void KeyPress(MathKeyboardInput input) {
      /// <returns>True if updated</returns>
      bool UpdatePlaceholderIfPresent(IMathAtom emptyAtom) {
        var current = MathList.AtomAt(_insertionIndex);
        if (current?.AtomType is MathAtomType.Placeholder) {
          if (current.Superscript is IMathList super)
            emptyAtom.Superscript = super;
          if (current.Subscript is IMathList sub)
            emptyAtom.Subscript = sub;
          //Remove the placeholder and replace with emptyAtom.
          MathList.RemoveAt(_insertionIndex);
          MathList.Insert(_insertionIndex, emptyAtom);
          return true;
        }
        return false;
      }
      MathAtom AtomForKeyPress(MathKeyboardInput i) {
        var c = (char)i;
        // Get the basic conversion from MathAtoms, and then special case unicode characters and latex special characters.
        switch (i) {
          //https://github.com/kostub/MathEditor/blob/61f67c6224000c224e252f6eeba483003f11d3d5/mathEditor/editor/MTEditableMathLabel.m#L414
          case MathKeyboardInput.Multiply:
          case MathKeyboardInput.Multiply_:
            return MathAtoms.Times;
          case MathKeyboardInput.SquareRoot:
            return MathAtoms.PlaceholderSquareRoot;
          case MathKeyboardInput.CubeRoot:
            var sqroot = MathAtoms.PlaceholderSquareRoot;
            sqroot.Degree = MathLists.WithAtoms(MathAtoms.ForCharacter('3'));
            return sqroot;
          case MathKeyboardInput.Infinity:
          case MathKeyboardInput.Degree:
          case MathKeyboardInput.Angle:
            return MathAtoms.Create(MathAtomType.Ordinary, c);
          case MathKeyboardInput.Divide:
          case MathKeyboardInput.Slash:
            return MathAtoms.Divide;
          case MathKeyboardInput.Fraction:
            return MathAtoms.PlaceholderFraction;
          case MathKeyboardInput.LeftCurlyBracket:
            return MathAtoms.Create(MathAtomType.Open, c);
          case MathKeyboardInput.RightCurlyBracket:
            return MathAtoms.Create(MathAtomType.Close, c);
          case MathKeyboardInput.GreaterOrEquals:
          case MathKeyboardInput.LessOrEquals:
            return MathAtoms.Create(MathAtomType.Relation, c);
          case var _ when c >= UnicodeFontChanger.UnicodeGreekLowerStart && c <= UnicodeFontChanger.UnicodeGreekLowerEnd:
            // All greek letters are rendered as variables.
            return MathAtoms.Create(MathAtomType.Variable, c);
          case var _ when c >= UnicodeFontChanger.UnicodeGreekUpperStart && c <= UnicodeFontChanger.UnicodeGreekUpperEnd:
            // Including capital greek letters
            return MathAtoms.Create(MathAtomType.Variable, c);
          case var _ when MathAtoms.ForCharacter(c) is MathAtom a:
            return a;
          default:
            //Just an ordinary character
            return MathAtoms.Create(MathAtomType.Ordinary, c);
        }
      }

      void HandleExponentButton() {
        if (!_insertionIndex.AtBeginningOfLine) {
          var a = MathList.AtomAt(_insertionIndex.Previous);
          if (a.Superscript is null) {
            a.Superscript = MathAtoms.PlaceholderList;
            _insertionIndex = _insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
          } else if (_insertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus)
            // If we are already inside the nucleus, then we come out and go up to the superscript
            _insertionIndex = _insertionIndex.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(a.Superscript.Atoms.Count), MathListSubIndexType.Superscript);
          else
            _insertionIndex = _insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Superscript.Atoms.Count), MathListSubIndexType.Superscript);
        } else {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = MathAtoms.Placeholder;
          emptyAtom.Superscript = MathAtoms.PlaceholderList;
          if (!UpdatePlaceholderIfPresent(emptyAtom))
            // If the placeholder hasn't been updated then insert it.
            MathList.Insert(_insertionIndex, emptyAtom);
          _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
        }
      }

      void HandleRadical(bool placeholderDegree, bool degreeIs3) {
        var current = _insertionIndex;
        Radical rad;
        if (placeholderDegree) {
          rad = MathAtoms.PlaceholderRadical;
          MathList.Insert(current, rad);
          _insertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
        } else {
          rad = MathAtoms.PlaceholderSquareRoot;
          if (degreeIs3) rad.Degree = MathLists.WithAtoms(MathAtoms.ForCharacter('3'));
          MathList.Insert(current, rad);
          _insertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
        }
      }

      void HandleSubscriptButton() {
        if (!_insertionIndex.AtBeginningOfLine) {
          var a = MathList.AtomAt(_insertionIndex.Previous);
          if (a.Subscript is null) {
            a.Subscript = MathAtoms.PlaceholderList;
            _insertionIndex = _insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
          } else if (_insertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus)
            // If we are already inside the nucleus, then we come out and go down to the subscript
            _insertionIndex = _insertionIndex.LevelDown().LevelUpWithSubIndex(MathListIndex.Level0Index(a.Subscript.Atoms.Count), MathListSubIndexType.Subscript);
          else
            _insertionIndex = _insertionIndex.Previous.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Subscript.Atoms.Count), MathListSubIndexType.Subscript);
        } else {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = MathAtoms.Placeholder;
          emptyAtom.Subscript = MathAtoms.PlaceholderList;
          if (!UpdatePlaceholderIfPresent(emptyAtom))
            // If the placeholder hasn't been updated then insert it.
            MathList.Insert(_insertionIndex, emptyAtom);
          _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
        }
      }

      void HandleSlashButton() {
        // special / handling - makes the thing a fraction
        var numerator = new MathList();
        var current = _insertionIndex;
        for (; !current.AtBeginningOfLine; current = current.Previous) {
          var a = MathList.AtomAt(current.Previous);
          if (a.AtomType != MathAtomType.Number && a.AtomType != MathAtomType.Variable)
            //We don't put this atom on the fraction
            break;
          else
            //Add the number to the beginning of the list
            numerator.Insert(0, a);
        }
        if (numerator.Count == 0) {
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
          // delete stuff in the Mathlist
          MathList.RemoveAtoms(new MathListRange(current, numerator.Count));

        MathList.Insert(current, new Fraction {
          Numerator = numerator,
          Denominator = MathAtoms.PlaceholderList
        });
        //Update the insertion index to go the denominator
        _insertionIndex = current.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Denominator);
      }

      void InsertParens() {
        RemovePlaceholderIfPresent();
        MathList.Insert(_insertionIndex, MathAtoms.ForCharacter('('));
        _insertionIndex = _insertionIndex.Next;
        MathList.Insert(_insertionIndex, MathAtoms.ForCharacter(')'));
        // Don't go to the next insertion index, to start inserting before the close parens.
      }
      void InsertAbsValue() {
        RemovePlaceholderIfPresent();
        MathList.Insert(_insertionIndex, MathAtoms.ForCharacter('|'));
        _insertionIndex = _insertionIndex.Next;
        MathList.Insert(_insertionIndex, MathAtoms.ForCharacter('|'));
        // Don't go to the next insertion index, to start inserting before the second absolute value
      }

      void MoveCursorLeft() {
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        if (_insertionIndex.AtBeginningOfLine)
          switch (_insertionIndex.FinalSubIndexType) {
            case MathListSubIndexType.Degree:
            case MathListSubIndexType.Numerator:
            case MathListSubIndexType.Nucleus:
            case MathListSubIndexType.Superscript:
            case MathListSubIndexType.Subscript:
            default:
              _insertionIndex = _insertionIndex.LevelDown() ?? _insertionIndex;
              break;
            case MathListSubIndexType.Radicand:
              var radicalIndex = _insertionIndex.LevelDown();
              if (MathList.AtomAt(radicalIndex) is IRadical rad)
                if (rad.Degree is IMathList deg)
                  _insertionIndex = radicalIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(deg.Count), MathListSubIndexType.Degree);
                else
                  goto default;
              else
                throw new InvalidCodePathException($"Radical not found at {radicalIndex}");
              break;
            case MathListSubIndexType.Denominator:
              var fracIndex = _insertionIndex.LevelDown();
              if (MathList.AtomAt(fracIndex) is IFraction frac)
                _insertionIndex = fracIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(frac.Numerator.Count), MathListSubIndexType.Numerator);
              else
                throw new InvalidCodePathException($"Fraction not found at {fracIndex}");
              break;
          } else if (_insertionIndex.Previous is MathListIndex prev)
          switch (MathList.AtomAt(prev)) {
            case null:
            default:
              _insertionIndex = prev;
              break;
            case var a when a.Subscript != null:
              _insertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Subscript.Count), MathListSubIndexType.Subscript);
              break;
            case var a when a.Superscript != null:
              _insertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(a.Superscript.Count), MathListSubIndexType.Superscript);
              break;
            case IRadical rad:
              _insertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(rad.Radicand.Count), MathListSubIndexType.Radicand);
              break;
            case IFraction frac:
              _insertionIndex = prev.LevelUpWithSubIndex(MathListIndex.Level0Index(frac.Denominator.Count), MathListSubIndexType.Denominator);
              break;
          }
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
      }
      void MoveCursorRight() {
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        switch (MathList.AtomAt(_insertionIndex)) {
          case null when MathList.AtomAt(_insertionIndex) is null: //After Count
            switch (_insertionIndex.FinalSubIndexType) {
              case MathListSubIndexType.Radicand:
              case MathListSubIndexType.Denominator:
              case MathListSubIndexType.Nucleus:
              case MathListSubIndexType.Superscript:
              case MathListSubIndexType.Subscript:
              default:
                _insertionIndex = _insertionIndex.LevelDown()?.Next ?? _insertionIndex;
                break;
              case MathListSubIndexType.Degree:
                var radicalIndex = _insertionIndex.LevelDown();
                if (MathList.AtomAt(radicalIndex) is IRadical)
                  _insertionIndex = radicalIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
                else
                  throw new InvalidCodePathException($"Radical not found at {radicalIndex}");
                break;
              case MathListSubIndexType.Numerator:
                var fracIndex = _insertionIndex.LevelDown();
                if (MathList.AtomAt(fracIndex) is IFraction)
                  _insertionIndex = fracIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Denominator);
                else
                  throw new InvalidCodePathException($"Fraction not found at {fracIndex}");
                break;
            }
            break;
          case null:
          default:
            _insertionIndex = _insertionIndex.Next;
            break;
          case IFraction frac:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
            break;
          case IRadical rad:
            if (rad.Degree is IMathList)
              _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Degree);
            else
              _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Radicand);
            break;
          case var a when a.Superscript != null:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Superscript);
            break;
          case var a when a.Subscript != null:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Subscript);
            break;
        }
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
      }

      void DeleteBackwards() {
        // delete the last atom from the list
        var prevIndex = _insertionIndex.Previous;
        if (HasText && !(prevIndex is null)) {
          MathList.RemoveAt(prevIndex);
          if (prevIndex.FinalSubIndexType is MathListSubIndexType.Nucleus) {
            // it was in the nucleus and we removed it, get out of the nucleus and get in the nucleus of the previous one.
            var downIndex = prevIndex.LevelDown();
            prevIndex = downIndex.Previous is MathListIndex downPrev
              ? downPrev.LevelUpWithSubIndex(MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus)
              : downIndex;
          }
          _insertionIndex = prevIndex;
          if (_insertionIndex.AtBeginningOfLine && _insertionIndex.SubIndexType != MathListSubIndexType.None) {
            // We have deleted to the beginning of the line and it is not the outermost line
            var insertionAtom = MathList.AtomAt(_insertionIndex);
            if (insertionAtom is null) {
              // add a placeholder if we deleted everything in the list
              insertionAtom = MathAtoms.Placeholder;
              // mark the placeholder as selected since that is the current insertion point.
              insertionAtom.Nucleus = Symbols.BlackSquare.ToString();
              MathList.Insert(_insertionIndex, insertionAtom);
            }
          }
        }
      }

      void InsertAtom(IMathAtom a) {
        if (!UpdatePlaceholderIfPresent(a))
          // If a placeholder wasn't updated then insert the new element.
          MathList.Insert(_insertionIndex, a);
        if (a.AtomType is MathAtomType.Fraction)
          // go to the numerator
          _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListIndex.Level0Index(0), MathListSubIndexType.Numerator);
        else
          _insertionIndex = _insertionIndex.Next;
      }
      void InsertCharacterKey(MathKeyboardInput i) => InsertAtom(AtomForKeyPress(i));
      void InsertSymbolName(string s) => InsertAtom(MathAtoms.ForLatexSymbolName(s));

      void RemovePlaceholderIfPresent() {
        if (IndexAtEmptyPlaceholder(out var placeholder))
          // Remove this element - the inserted text replaces the placeholder
          MathList.Remove(placeholder);
      }

      var atom = AtomForKeyPress(input);
      if (_insertionIndex.SubIndexType is MathListSubIndexType.Denominator && atom.AtomType is MathAtomType.Relation)
        // pull the insertion index out
        _insertionIndex = _insertionIndex.LevelDown().Next;

      switch (input) {
#warning Unimplemented up/down buttons
        case MathKeyboardInput.Up:
          break;
        case MathKeyboardInput.Down:
          break;
        case MathKeyboardInput.Left:
          MoveCursorLeft();
          break;
        case MathKeyboardInput.Right:
          MoveCursorRight();
          break;
        case MathKeyboardInput.Backspace:
          DeleteBackwards();
          break;
        case MathKeyboardInput.Clear:
          MathList.Clear();
          InsertionIndex = MathListIndex.Level0Index(0);
          break;
        case MathKeyboardInput.Return:
          ReturnPressed?.Invoke(this, EventArgs.Empty);
          _caret = null;
          return;
        case MathKeyboardInput.Dismiss:
          DismissPressed?.Invoke(this, EventArgs.Empty);
          _caret = null;
          return;
        case MathKeyboardInput.BothRoundBrackets:
          InsertParens();
          break;
        case MathKeyboardInput.LeftRoundBracket:
        case MathKeyboardInput.RightRoundBracket:
        case MathKeyboardInput.LeftSquareBracket:
        case MathKeyboardInput.RightSquareBracket:
        case MathKeyboardInput.LeftCurlyBracket:
        case MathKeyboardInput.RightCurlyBracket:
        case MathKeyboardInput.D0:
        case MathKeyboardInput.D1:
        case MathKeyboardInput.D2:
        case MathKeyboardInput.D3:
        case MathKeyboardInput.D4:
        case MathKeyboardInput.D5:
        case MathKeyboardInput.D6:
        case MathKeyboardInput.D7:
        case MathKeyboardInput.D8:
        case MathKeyboardInput.D9:
        case MathKeyboardInput.Decimal:
        case MathKeyboardInput.Plus:
        case MathKeyboardInput.Minus:
        case MathKeyboardInput.Minus_:
        case MathKeyboardInput.Multiply:
        case MathKeyboardInput.Multiply_:
        case MathKeyboardInput.Divide:
        case MathKeyboardInput.Fraction:
        case MathKeyboardInput.Ratio:
        case MathKeyboardInput.Ratio_:
        case MathKeyboardInput.Percentage:
        case MathKeyboardInput.Comma:
        case MathKeyboardInput.Factorial:
        case MathKeyboardInput.Infinity:
        case MathKeyboardInput.Angle:
        case MathKeyboardInput.Degree:
          InsertCharacterKey(input);
          break;
        case MathKeyboardInput.Slash:
          HandleSlashButton();
          break;
        case MathKeyboardInput.Power:
          HandleExponentButton();
          break;
        case MathKeyboardInput.Subscript:
          HandleSubscriptButton();
          break;
        case MathKeyboardInput.SquareRoot:
          HandleRadical(false, false);
          break;
        case MathKeyboardInput.CubeRoot:
          HandleRadical(false, true);
          break;
        case MathKeyboardInput.NthRoot:
          HandleRadical(true, false);
          break;
        case MathKeyboardInput.VerticalBar:
          InsertCharacterKey(input);
          break;
        case MathKeyboardInput.Absolute:
          InsertAbsValue();
          break;
        case MathKeyboardInput.BaseEPower:
          InsertCharacterKey(MathKeyboardInput.SmallE);
          HandleExponentButton();
          break;
        case MathKeyboardInput.Logarithm:
          InsertSymbolName("log");
          break;
        case MathKeyboardInput.NaturalLogarithm:
          InsertSymbolName("ln");
          break;
        case MathKeyboardInput.LogarithmWithBase:
          InsertSymbolName("log");
          HandleSubscriptButton();
          break;
        case MathKeyboardInput.Equals:
        case MathKeyboardInput.NotEquals:
        case MathKeyboardInput.LessThan:
        case MathKeyboardInput.LessOrEquals:
        case MathKeyboardInput.GreaterThan:
        case MathKeyboardInput.GreaterOrEquals:
        case MathKeyboardInput.A:
        case MathKeyboardInput.B:
        case MathKeyboardInput.C:
        case MathKeyboardInput.D:
        case MathKeyboardInput.E:
        case MathKeyboardInput.F:
        case MathKeyboardInput.G:
        case MathKeyboardInput.H:
        case MathKeyboardInput.I:
        case MathKeyboardInput.J:
        case MathKeyboardInput.K:
        case MathKeyboardInput.L:
        case MathKeyboardInput.M:
        case MathKeyboardInput.N:
        case MathKeyboardInput.O:
        case MathKeyboardInput.P:
        case MathKeyboardInput.Q:
        case MathKeyboardInput.R:
        case MathKeyboardInput.S:
        case MathKeyboardInput.T:
        case MathKeyboardInput.U:
        case MathKeyboardInput.V:
        case MathKeyboardInput.W:
        case MathKeyboardInput.X:
        case MathKeyboardInput.Y:
        case MathKeyboardInput.Z:
        case MathKeyboardInput.SmallA:
        case MathKeyboardInput.SmallB:
        case MathKeyboardInput.SmallC:
        case MathKeyboardInput.SmallD:
        case MathKeyboardInput.SmallE:
        case MathKeyboardInput.SmallF:
        case MathKeyboardInput.SmallG:
        case MathKeyboardInput.SmallH:
        case MathKeyboardInput.SmallI:
        case MathKeyboardInput.SmallJ:
        case MathKeyboardInput.SmallK:
        case MathKeyboardInput.SmallL:
        case MathKeyboardInput.SmallM:
        case MathKeyboardInput.SmallN:
        case MathKeyboardInput.SmallO:
        case MathKeyboardInput.SmallP:
        case MathKeyboardInput.SmallQ:
        case MathKeyboardInput.SmallR:
        case MathKeyboardInput.SmallS:
        case MathKeyboardInput.SmallT:
        case MathKeyboardInput.SmallU:
        case MathKeyboardInput.SmallV:
        case MathKeyboardInput.SmallW:
        case MathKeyboardInput.SmallX:
        case MathKeyboardInput.SmallY:
        case MathKeyboardInput.SmallZ:
        case MathKeyboardInput.Alpha:
        case MathKeyboardInput.Beta:
        case MathKeyboardInput.Gamma:
        case MathKeyboardInput.Delta:
        case MathKeyboardInput.Epsilon:
        case MathKeyboardInput.Zeta:
        case MathKeyboardInput.Eta:
        case MathKeyboardInput.Theta:
        case MathKeyboardInput.Iota:
        case MathKeyboardInput.Kappa:
        case MathKeyboardInput.Lambda:
        case MathKeyboardInput.Mu:
        case MathKeyboardInput.Nu:
        case MathKeyboardInput.Xi:
        case MathKeyboardInput.Omicron:
        case MathKeyboardInput.Pi:
        case MathKeyboardInput.Rho:
        case MathKeyboardInput.Sigma:
        case MathKeyboardInput.Tau:
        case MathKeyboardInput.Upsilon:
        case MathKeyboardInput.Phi:
        case MathKeyboardInput.Chi:
        case MathKeyboardInput.Omega:
        case MathKeyboardInput.SmallAlpha:
        case MathKeyboardInput.SmallBeta:
        case MathKeyboardInput.SmallGamma:
        case MathKeyboardInput.SmallDelta:
        case MathKeyboardInput.SmallEpsilon:
        case MathKeyboardInput.SmallZeta:
        case MathKeyboardInput.SmallEta:
        case MathKeyboardInput.SmallTheta:
        case MathKeyboardInput.SmallIota:
        case MathKeyboardInput.SmallKappa:
        case MathKeyboardInput.SmallLambda:
        case MathKeyboardInput.SmallMu:
        case MathKeyboardInput.SmallNu:
        case MathKeyboardInput.SmallXi:
        case MathKeyboardInput.SmallOmicron:
        case MathKeyboardInput.SmallPi:
        case MathKeyboardInput.SmallRho:
        case MathKeyboardInput.SmallSigma:
        case MathKeyboardInput.SmallSigma2:
        case MathKeyboardInput.SmallTau:
        case MathKeyboardInput.SmallUpsilon:
        case MathKeyboardInput.SmallPhi:
        case MathKeyboardInput.SmallChi:
        case MathKeyboardInput.SmallOmega:
          InsertCharacterKey(input);
          break;
        case MathKeyboardInput.Sine:
          InsertSymbolName("sin");
          break;
        case MathKeyboardInput.Cosine:
          InsertSymbolName("cos");
          break;
        case MathKeyboardInput.Tangent:
          InsertSymbolName("tan");
          break;
        case MathKeyboardInput.Cotangent:
          InsertSymbolName("cot");
          break;
        case MathKeyboardInput.Secant:
          InsertSymbolName("sec");
          break;
        case MathKeyboardInput.Cosecant:
          InsertSymbolName("csc");
          break;
        case MathKeyboardInput.ArcSine:
          InsertSymbolName("arcsin");
          break;
        case MathKeyboardInput.ArcCosine:
          InsertSymbolName("arccos");
          break;
        case MathKeyboardInput.ArcTangent:
          InsertSymbolName("arctan");
          break;
        case MathKeyboardInput.ArcCotangent:
          InsertSymbolName("arccot");
          break;
        case MathKeyboardInput.ArcSecant:
          InsertSymbolName("arcsec");
          break;
        case MathKeyboardInput.ArcCosecant:
          InsertSymbolName("arccsc");
          break;
        case MathKeyboardInput.HyperbolicSine:
          InsertSymbolName("sinh");
          break;
        case MathKeyboardInput.HyperbolicCosine:
          InsertSymbolName("cosh");
          break;
        case MathKeyboardInput.HyperbolicTangent:
          InsertSymbolName("tanh");
          break;
        case MathKeyboardInput.HyperbolicCotangent:
          InsertSymbolName("coth");
          break;
        case MathKeyboardInput.HyperbolicSecant:
          InsertSymbolName("sech");
          break;
        case MathKeyboardInput.HyperbolicCosecant:
          InsertSymbolName("csch");
          break;
        case MathKeyboardInput.AreaHyperbolicSine:
          InsertSymbolName("arsinh");
          break;
        case MathKeyboardInput.AreaHyperbolicCosine:
          InsertSymbolName("arconh");
          break;
        case MathKeyboardInput.AreaHyperbolicTangent:
          InsertSymbolName("artanh");
          break;
        case MathKeyboardInput.AreaHyperbolicCotangent:
          InsertSymbolName("arcoth");
          break;
        case MathKeyboardInput.AreaHyperbolicSecant:
          InsertSymbolName("arsech");
          break;
        case MathKeyboardInput.AreaHyperbolicCosecant:
          InsertSymbolName("arcsch");
          break;
        default:
          break;
      }
      InsertionPointChanged();
      return;
    }

    /// <summary>Helper method to update caretView when insertion point/selection changes.</summary>
    private void InsertionPointChanged() {
      void ClearPlaceholders(IMathList mathList) {
        foreach (var mathAtom in (IList<IMathAtom>)mathList?.Atoms ?? Array.Empty<IMathAtom>()) {
          if (mathAtom.AtomType is MathAtomType.Placeholder)
            mathAtom.Nucleus = Symbols.WhiteSquare;
          if (mathAtom.Superscript is IMathList super)
            ClearPlaceholders(super);
          if (mathAtom.Subscript is IMathList sub)
            ClearPlaceholders(sub);
          if (mathAtom is Radical rad) {
            ClearPlaceholders(rad.Degree);
            ClearPlaceholders(rad.Radicand);
          }
          if (mathAtom is Fraction frac) {
            ClearPlaceholders(frac.Numerator);
            ClearPlaceholders(frac.Denominator);
          }
        }
      }
      ClearPlaceholders(MathList);
      if (IndexAtEmptyPlaceholder(out var atom)) {
        atom.Nucleus = Symbols.BlackSquare;
        if (_insertionIndex.FinalSubIndexType is MathListSubIndexType.Nucleus) {
          // If the insertion index is inside a placeholder, move it out.
          _insertionIndex = _insertionIndex.LevelDown();
        }
        // TODO - disable caret
      }
      /* Find the insert point rect and create a caretView to draw the caret at this position. */

      // Check that we were returned a valid position before displaying a caret there.
      if (CaretRectForIndex(_insertionIndex) is PointF point)
        _caret = new CaretHandle(Font.PointSize, point);
      RedrawRequested?.Invoke(this, EventArgs.Empty);
    }

    public PointF? CaretRectForIndex(MathListIndex index) {
      UpdateDisplay();
      return _display?.PointForIndex(_context, index);
    }

    public MathListIndex ClosestIndexToPoint(PointF point) {
      UpdateDisplay();
      return _display?.IndexForPoint(_context, point);
    }

    public void Clear() {
      MathList.Clear();
      InsertionPointChanged();
    }

    public void MoveCaretToPoint(PointF point) {
      _insertionIndex = ClosestIndexToPoint(point);
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
      _insertionIndex = index; // move the index to the end of the new list.
      InsertionPointChanged();
    }

    public void SelectCharacterAtIndex(MathListIndex index, Structures.Color color) {
      UpdateDisplay();
      // setup highlights before drawing the MTLine
      _display?.HighlightCharacterAt(index, color);
    }

    public void ClearHighlights() => UpdateDisplay();

    public void Tap(PointF point) {
      point.Y *= -1; //inverted canvas, blah blah
      InsertionIndex = ClosestIndexToPoint(point) ??
        MathListIndex.Level0Index(MathList.Atoms.Count);
      if (CaretRectForIndex(InsertionIndex) is PointF p)
        _caret = new CaretHandle(Font.PointSize, p);
      InsertionPointChanged();
    }
  }
}
