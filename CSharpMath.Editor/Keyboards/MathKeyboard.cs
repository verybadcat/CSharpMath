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
    public MathKeyboard(TypesettingContext<TFont, TGlyph> context) => Context = context;
    //private readonly List<MathListIndex> highlighted;

    protected TypesettingContext<TFont, TGlyph> Context { get; }

    public CaretHandle? Caret { get; protected set; }
    public IDisplay<TFont, TGlyph> Display { get; protected set; }
    public MathList MathList { get; } = new MathList();
    public string LaTeX => MathListBuilder.MathListToString(MathList);
    private MathListIndex _insertionIndex = MathListIndex.Level0Index(0);
    public MathListIndex InsertionIndex
      { get => _insertionIndex; set { _insertionIndex = value; InsertionPointChanged(); } }
    public TFont Font { get; set; }
    public LineStyle LineStyle { get; set; }
    public Color SelectColor { get; set; }
    public RectangleF Measure => Display.DisplayBounds;
    public bool HasText => MathList?.Atoms?.Count > 0;

    public void RecreateDisplayFromMathList() {
      var position = Display?.Position ?? default;
      Display = Context.CreateLine(MathList, Font, LineStyle);
      Display.Position = position;
    }

    /// <summary>Keyboard should now be hidden and input be discarded.</summary>
    public event EventHandler DismissPressed;
    /// <summary>Keyboard should now be hidden and input be saved.</summary>
    public event EventHandler ReturnPressed;
    /// <summary><see cref="Display"/> should be redrawn.</summary>
    public event EventHandler RedrawRequested;

    public PointF? ClosestPointToIndex(MathListIndex index) => Display?.PointForIndex(Context, index);
    public MathListIndex ClosestIndexToPoint(PointF point) => Display?.IndexForPoint(Context, point);

    public void KeyPress(params MathKeyboardInput[] inputs) {
      foreach (var input in inputs) KeyPress(input);
    }
    public void KeyPress(MathKeyboardInput input) {
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

      void HandleScriptButton(bool isSuperScript) {
        var subIndexType = isSuperScript ? MathListSubIndexType.Superscript : MathListSubIndexType.Subscript;
        IMathList GetScript(IMathAtom atom) => isSuperScript ? atom.Superscript : atom.Subscript;
        void SetScript(IMathAtom atom, IMathList value) { if (isSuperScript) atom.Superscript = value; else atom.Subscript = value; }
        if (_insertionIndex.AtBeginningOfLine) {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = MathAtoms.Placeholder;
          emptyAtom.Superscript = MathAtoms.PlaceholderList;
          MathList.InsertAndAdvance(ref _insertionIndex, emptyAtom, subIndexType);
        } else {
          var prevAtom = MathList.AtomAt(_insertionIndex.Previous);
#warning Simplify to tuple patterns when C# 8 is out
          switch ((GetScript(prevAtom) is null, _insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts)) {
            case var t when t == (true, true):
              SetScript(MathList.AtomAt(_insertionIndex.LevelDown()), MathAtoms.PlaceholderList);
              _insertionIndex = _insertionIndex.LevelDown().LevelUpWithSubIndex(subIndexType, MathListIndex.Level0Index(0));
              break;
            case var t when t == (true, false):
              SetScript(prevAtom, MathAtoms.PlaceholderList);
              _insertionIndex = _insertionIndex.Previous.LevelUpWithSubIndex(subIndexType, MathListIndex.Level0Index(0));
              break;
            case var t when t == (false, true):
              // If we are already inside the nucleus, then we come out and go up to the script
              _insertionIndex = _insertionIndex.LevelDown().LevelUpWithSubIndex(subIndexType, MathListIndex.Level0Index(GetScript(prevAtom).Atoms.Count));
              break;
            case var t when t == (false, false):
              _insertionIndex = _insertionIndex.Previous.LevelUpWithSubIndex(subIndexType, MathListIndex.Level0Index(GetScript(prevAtom).Atoms.Count));
              break;
          }
        }
      }

      void HandleRadical(bool degreeIsPlaceholder, bool degreeIs3) {
        Radical rad;
        if (degreeIsPlaceholder) {
          rad = MathAtoms.PlaceholderRadical;
          MathList.InsertAndAdvance(ref _insertionIndex, rad, MathListSubIndexType.Degree);
        } else {
          rad = MathAtoms.PlaceholderSquareRoot;
          if (degreeIs3) rad.Degree = MathLists.WithAtoms(MathAtoms.ForCharacter('3'));
          MathList.InsertAndAdvance(ref _insertionIndex, rad, MathListSubIndexType.Radicand);
        }
      }

      void HandleSlashButton() {
        // special / handling - makes the thing a fraction
        var numerator = new MathList();
        for (; !_insertionIndex.AtBeginningOfLine; _insertionIndex = _insertionIndex.Previous) {
          var a = MathList.AtomAt(_insertionIndex.Previous);
          if (a.AtomType != MathAtomType.Number && a.AtomType != MathAtomType.Variable)
            //We don't put this atom on the fraction
            break;
          else
            numerator.Insert(0, a);
        }
        MathList.RemoveAtoms(new MathListRange(_insertionIndex, numerator.Count));
        if (numerator.Count == 0) {
          // so we didn't really find any numbers before this, so make the numerator 1
          numerator.Add(MathAtoms.ForCharacter('1'));
          if (MathList.AtomAt(_insertionIndex.Previous)?.AtomType is MathAtomType.Fraction)
            //Add a times symbol
            MathList.InsertAndAdvance(ref _insertionIndex, MathAtoms.Times, MathListSubIndexType.None);
        }

        MathList.InsertAndAdvance(ref _insertionIndex, new Fraction {
          Numerator = numerator,
          Denominator = MathAtoms.PlaceholderList
        }, MathListSubIndexType.Denominator);
      }
      void InsertParens() {
        MathList.InsertAndAdvance(ref _insertionIndex, MathAtoms.ForCharacter('('), MathListSubIndexType.None);
        MathList.InsertAndAdvance(ref _insertionIndex, MathAtoms.ForCharacter(')'), MathListSubIndexType.None);
        // Don't go to the next insertion index, to start inserting before the close parens.
        _insertionIndex = _insertionIndex.Previous;
      }
      void InsertAbsValue() {
        MathList.InsertAndAdvance(ref _insertionIndex, MathAtoms.ForCharacter('|'), MathListSubIndexType.None);
        MathList.InsertAndAdvance(ref _insertionIndex, MathAtoms.ForCharacter('|'), MathListSubIndexType.None);
        // Don't go to the next insertion index, to start inserting before the close parens.
        _insertionIndex = _insertionIndex.Previous;
      }

      void MoveCursorLeft() {
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        var prev = _insertionIndex.Previous;
        switch (MathList.AtomAt(prev)) {
          case null: // At beginning of line
            var levelDown = _insertionIndex.LevelDown();
            switch (_insertionIndex.FinalSubIndexType) {
              case MathListSubIndexType.Subscript:
                var scriptAtom = MathList.AtomAt(levelDown);
                if (scriptAtom.Superscript != null)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Superscript, MathListIndex.Level0Index(scriptAtom.Superscript.Count));
                else
                  goto case MathListSubIndexType.Superscript;
                break;
              case MathListSubIndexType.Superscript:
                _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                break;
              case MathListSubIndexType.BetweenBaseAndScripts:
                if (MathList.AtomAt(levelDown) is IRadical rad)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Radicand, MathListIndex.Level0Index(rad.Radicand.Count));
                else if (MathList.AtomAt(levelDown) is IFraction frac)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Denominator, MathListIndex.Level0Index(frac.Denominator.Count));
                else goto case MathListSubIndexType.Radicand;
                break;
              case MathListSubIndexType.Radicand:
                if (MathList.AtomAt(levelDown) is IRadical radDeg && radDeg.Degree is IMathList deg)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Degree, MathListIndex.Level0Index(deg.Count));
                else
                  goto case MathListSubIndexType.Denominator;
                break;
              case MathListSubIndexType.Denominator:
                if (MathList.AtomAt(levelDown) is IFraction fracNum)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Numerator, MathListIndex.Level0Index(fracNum.Numerator.Count));
                else
                  goto default;
                break;
              case MathListSubIndexType.Degree:
              case MathListSubIndexType.Numerator:
              default:
                _insertionIndex = _insertionIndex.LevelDown() ?? _insertionIndex;
                break;
            }
            break;
          case var a when a.Subscript != null:
            _insertionIndex = prev.LevelUpWithSubIndex(MathListSubIndexType.Subscript, MathListIndex.Level0Index(a.Subscript.Count));
            break;
          case var a when a.Superscript != null:
            _insertionIndex = prev.LevelUpWithSubIndex(MathListSubIndexType.Superscript, MathListIndex.Level0Index(a.Superscript.Count));
            break;
          case IRadical rad:
            _insertionIndex = prev.LevelUpWithSubIndex(MathListSubIndexType.Radicand, MathListIndex.Level0Index(rad.Radicand.Count));
            break;
          case IFraction frac:
            _insertionIndex = prev.LevelUpWithSubIndex(MathListSubIndexType.Denominator, MathListIndex.Level0Index(frac.Denominator.Count));
            break;
          default:
            _insertionIndex = prev;
            break;
        }
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        if (MathList.AtomAt(_insertionIndex) is null &&
          MathList.AtomAt(_insertionIndex?.Previous)?.AtomType is MathAtomType.Placeholder)
          _insertionIndex = _insertionIndex.Previous; // Skip right side of placeholders when end of line
      }
      void MoveCursorRight() {
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
        switch (MathList.AtomAt(_insertionIndex)) {
          case null: //After Count
            var levelDown = _insertionIndex.LevelDown();
            switch (_insertionIndex.FinalSubIndexType) {
              case MathListSubIndexType.Degree:
                if (MathList.AtomAt(levelDown) is IRadical)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Radicand, MathListIndex.Level0Index(0));
                else
                  throw new SubIndexTypeMismatchException($"Radical not found at {levelDown}");
                break;
              case MathListSubIndexType.Numerator:
                if (MathList.AtomAt(levelDown) is IFraction)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Denominator, MathListIndex.Level0Index(0));
                else
                  throw new SubIndexTypeMismatchException($"Fraction not found at {levelDown}");
                break;
              case MathListSubIndexType.Radicand:
              case MathListSubIndexType.Denominator:
                var scriptAtom = MathList.AtomAt(levelDown);
                if (scriptAtom.Superscript != null || scriptAtom.Subscript != null)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                else
                  goto default;
                break;
              case MathListSubIndexType.BetweenBaseAndScripts:
                if (MathList.AtomAt(levelDown).Superscript != null)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Superscript, MathListIndex.Level0Index(0));
                else
                  goto case MathListSubIndexType.Superscript;
                break;
              case MathListSubIndexType.Superscript:
                if (MathList.AtomAt(levelDown).Subscript != null)
                  _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Subscript, MathListIndex.Level0Index(0));
                else
                  goto default;
                break;
              case MathListSubIndexType.Subscript:
              default:
                _insertionIndex = _insertionIndex.LevelDown()?.Next ?? _insertionIndex;
                break;
            }
            break;
          case var a when _insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts:
            _insertionIndex = _insertionIndex.LevelDown().LevelUpWithSubIndex(
              a.Superscript != null ? MathListSubIndexType.Superscript : MathListSubIndexType.Subscript,
              MathListIndex.Level0Index(0));
            break;
          case IFraction _:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListSubIndexType.Numerator, MathListIndex.Level0Index(0));
            break;
          case IRadical rad:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(
              rad.Degree is IMathList ? MathListSubIndexType.Degree : MathListSubIndexType.Radicand,
              MathListIndex.Level0Index(0));
            break;
          case var a when a.Superscript != null || a.Subscript != null:
            _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
            break;
          case var a when a.AtomType is MathAtomType.Placeholder && MathList.AtomAt(_insertionIndex.Next) is null:
            // Skip right side of placeholders when end of line
            goto case null;
          default:
            _insertionIndex = _insertionIndex.Next;
            break;
        }
        if (_insertionIndex is null)
          throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
      }

      void DeleteBackwards() {
#warning Refactor pls (include this logic in MathList.RemoveAt)
        // delete the last atom from the list
        var prevIndex = _insertionIndex.Previous;
        if (HasText && !(prevIndex is null)) {
          MathList.RemoveAt(prevIndex);
          if (prevIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts) {
            // it was in the nucleus and we removed it, get out of the nucleus and get in the nucleus of the previous one.
            var downIndex = prevIndex.LevelDown();
            prevIndex = downIndex.Previous is MathListIndex downPrev
              ? downPrev.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1))
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
              insertionAtom.Nucleus = Symbols.BlackSquare;
              MathList.InsertAndAdvance(ref _insertionIndex, insertionAtom, MathListSubIndexType.None);
              _insertionIndex = _insertionIndex.Previous;
            }
          }
        }
      }

      void InsertAtom(IMathAtom a) =>
        MathList.InsertAndAdvance(ref _insertionIndex, a,
          a.AtomType is MathAtomType.Fraction ?
          MathListSubIndexType.Numerator :
          MathListSubIndexType.None);
      void InsertCharacterKey(MathKeyboardInput i) => InsertAtom(AtomForKeyPress(i));
      void InsertSymbolName(string s) => InsertAtom(MathAtoms.ForLatexSymbolName(s));

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
          Caret = null;
          return;
        case MathKeyboardInput.Dismiss:
          DismissPressed?.Invoke(this, EventArgs.Empty);
          Caret = null;
          return;
        case MathKeyboardInput.BothRoundBrackets:
          InsertParens();
          break;
        case MathKeyboardInput.Slash:
          HandleSlashButton();
          break;
        case MathKeyboardInput.Power:
          HandleScriptButton(true);
          break;
        case MathKeyboardInput.Subscript:
          HandleScriptButton(false);
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
          HandleScriptButton(true);
          break;
        case MathKeyboardInput.Logarithm:
          InsertSymbolName("log");
          break;
        case MathKeyboardInput.NaturalLogarithm:
          InsertSymbolName("ln");
          break;
        case MathKeyboardInput.LogarithmWithBase:
          InsertSymbolName("log");
          HandleScriptButton(false);
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
          InsertSymbolName("arcosh");
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
        default:
          break;
      }
      InsertionPointChanged();
    }

    public void MoveCaretToPoint(PointF point) {
      point.Y *= -1; //inverted canvas, blah blah
      InsertionIndex = ClosestIndexToPoint(point) ?? MathListIndex.Level0Index(MathList.Atoms.Count);
      Caret = new CaretHandle(Font.PointSize);
    }

    /// <summary>Helper method to update caretView when insertion point/selection changes.</summary>
    private void InsertionPointChanged() {
      void VisualizePlaceholders(IMathList mathList) {
        foreach (var mathAtom in (IList<IMathAtom>)mathList?.Atoms ?? Array.Empty<IMathAtom>()) {
          if (mathAtom.AtomType is MathAtomType.Placeholder)
            mathAtom.Nucleus = Symbols.WhiteSquare;
          if (mathAtom.Superscript is IMathList super)
            VisualizePlaceholders(super);
          if (mathAtom.Subscript is IMathList sub)
            VisualizePlaceholders(sub);
          if (mathAtom is Radical rad) {
            VisualizePlaceholders(rad.Degree);
            VisualizePlaceholders(rad.Radicand);
          }
          if (mathAtom is Fraction frac) {
            VisualizePlaceholders(frac.Numerator);
            VisualizePlaceholders(frac.Denominator);
          }
        }
      }
      VisualizePlaceholders(MathList);
      if(MathList.AtomAt(_insertionIndex) is IMathAtom atom && atom.AtomType is MathAtomType.Placeholder)
        atom.Nucleus = Symbols.BlackSquare;
      /* Find the insert point rect and create a caretView to draw the caret at this position. */

      // Check that we were returned a valid position before displaying a caret there.
      Caret = new CaretHandle(Font.PointSize);
      RecreateDisplayFromMathList();
      RedrawRequested?.Invoke(this, EventArgs.Empty);
    }
    
    public void Clear() {
      MathList.Clear();
      InsertionIndex = null;
    }

    // Insert a list at a given point.
    public void InsertMathList(IMathList list, PointF point) {
      var detailedIndex = ClosestIndexToPoint(point);
      // insert at the given index - but don't consider sublevels at this point
      var index = MathListIndex.Level0Index(detailedIndex.AtomIndex);
      foreach (var atom in list.Atoms) {
        MathList.InsertAndAdvance(ref index, atom, MathListSubIndexType.None);
      }
      InsertionIndex = index; // move the index to the end of the new list.
    }

    public void HighlightCharacterAt(MathListIndex index, Structures.Color color) {
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
