using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Editor {
  using System;
  using System.Collections.Generic;
  using Atom;
  using Structures;
  using Atoms = Atom.Atoms;

  public class LatexMathKeyboard {
    private enum Diraction : byte {
      Up,
      Down,
      Left,
      Right
    }
    public MathList MathList { get; private set; } = new MathList();
    public string LaTeX => LaTeXParser.MathListToLaTeX(MathList).ToString();

    public string _LatexWithCert() {
      KeyPress(MathKeyboardInput.Caret);
      string LatexHandle = LaTeX;
      KeyPress(MathKeyboardInput.Backspace);
      return LatexHandle;
    }

    internal MathListIndex _insertionIndex = MathListIndex.Level0Index(0);
    public MathListIndex InsertionIndex {
      get => _insertionIndex;
      set {
        _insertionIndex = value;
      }
    }
    public bool HasText => MathList.Atoms.Count > 0;

    public void KeyPress(params MathKeyboardInput[] inputs) {
      foreach (var input in inputs) KeyPress(input);
    }
    public virtual void KeyPress(MathKeyboardInput input) {
      ConvertClick(input);

      void HandleScriptButton(bool isSuperScript) {
        // check what kind of script
        var subIndexType = isSuperScript ? MathListSubIndexType.Superscript : MathListSubIndexType.Subscript;
        // if the script is empty add empty atom and move to the right list
        if (!IndexHasPrevious()) {
          CreateEmptyAtom();
          return;
        }

        // back to previous
        var isBetweenBaseAndScripts =
         finalSubType() is MathListSubIndexType.BetweenBaseAndScripts;
        var prevIndexCorrected = isBetweenBaseAndScripts ? _insertionIndex.LevelDown()
            ?? throw new InvalidCodePathException("BetweenBaseAndScripts indexInCurrectList has null LevelDown")
          : _insertionIndex.Previous;
        // check if the atom is non number who required a place holder
        var prevAtom = getAtom(prevIndexCorrected);
        if (prevAtom is null)
          throw new InvalidCodePathException("prevAtom is null");
        if (!isBetweenBaseAndScripts && IsFullPlaceholderRequired(prevAtom)) {
          CreateEmptyAtom();
          return;
        }
        // add placeholder if the script is empty
        var script = GetScript(prevAtom);
        if (script.IsEmpty()) {
          SetScript(prevAtom, LaTeXSettings.PlaceholderList);
        }
        // update the insertion to the right place
        _insertionIndex = prevIndexCorrected!.LevelUpWithSubIndex
          (subIndexType, MathListIndex.Level0Index(1));
        // ensure that the index in the right place
        if (InsertionIndex.FinalIndex == 2) {
          _insertionIndex = _insertionIndex.Previous ?? _insertionIndex;
        }


        bool IndexHasPrevious() { return (_insertionIndex.Previous is MathListIndex); }
        MathList GetScript(MathAtom atom) => isSuperScript ? atom.Superscript : atom.Subscript;
        void SetScript(MathAtom atom, MathList value) => GetScript(atom).Append(value);
        void CreateEmptyAtom() {
          // Create an empty atom and move the insertion indexInCurrectList up.
          var emptyAtom = LaTeXSettings.Placeholder;
          SetScript(emptyAtom, LaTeXSettings.PlaceholderList);
          MathList.InsertAndAdvance(ref _insertionIndex, emptyAtom, subIndexType);
        }
        static bool IsFullPlaceholderRequired(MathAtom mathAtom) =>
          mathAtom switch {
            Atoms.BinaryOperator _ => true,
            Atoms.UnaryOperator _ => true,
            Atoms.Relation _ => true,
            Atoms.Open _ => true,
            Atoms.Punctuation _ => true,
            _ => false
          };
      }
      void HandleSlashButton() {
        // special / handling - makes the thing atom fraction
        var numerator = new Stack<MathAtom>();
        var parenDepth = 0;
        if (finalSubType() == MathListSubIndexType.BetweenBaseAndScripts)
          _insertionIndex = _insertionIndex.LevelDown()?.Next
              ?? throw new InvalidCodePathException("_insertionIndex.LevelDown() returned null");
        for (; _insertionIndex.Previous is not null; _insertionIndex = _insertionIndex.Previous) {
          switch (getAtom(_insertionIndex.Previous), parenDepth) {
            case (null, _): throw new InvalidCodePathException("Invalid _insertionIndex");
            // Stop looking behind upon encountering these atoms unparenthesized
            case (Atoms.Open _, _) when --parenDepth < 0: goto stop;
            case (Atoms.Close a, _): parenDepth++; numerator.Push(a); break;
            case (Atoms.UnaryOperator _, 0): goto stop;
            case (Atoms.BinaryOperator _, 0): goto stop;
            case (Atoms.Relation _, 0): goto stop;
            case (Atoms.Fraction _, 0): goto stop;
            case (Atoms.Placeholder _, 0): goto stop;
            case (Atoms.Open _, _) when parenDepth < 0: goto stop;
            // We don't put this atom on the fraction
            case (var atom, _): numerator.Push(atom); break;
          }
        }
      stop: MathList.RemoveAtoms(new MathListRange(_insertionIndex, numerator.Count));
        if (numerator.Count == 0)
          // so we didn't really find any numbers before this, so make the numerator 1
          numerator.Push(new Atoms.Number("1"));
        if (getAtom(_insertionIndex.Previous) is Atoms.Fraction)
          // Add atom times symbol
          MathList.InsertAndAdvance(ref _insertionIndex, LaTeXSettings.Times, MathListSubIndexType.None);
        MathList.InsertAndAdvance(ref _insertionIndex, new Atoms.Fraction(
          new MathList(numerator),
          LaTeXSettings.PlaceholderList
        ), MathListSubIndexType.Denominator);
      }
      void InsertInner(string left, string right) =>
        MathList.InsertAndAdvance(ref _insertionIndex,
          new Atoms.Inner(new Boundary(left), LaTeXSettings.PlaceholderList, new Boundary(right)),
          MathListSubIndexType.Inner);
      void MoveCursor(Diraction diraction) {
        switch (diraction) {
          case Diraction.Down:
            // move down
            break;
          case Diraction.Up:
            MoveCursorUp();
            break;
          case Diraction.Left:
            MoveCursorLeft();
            break;
          case Diraction.Right:
            MoveCursorRight();
            break;
        }
        void MoveCursorUp() {
          var searchType = InsertionIndex?.GetParentIndexByType(MathListSubIndexType.Denominator,
          MathListSubIndexType.Subscript);

          if (searchType is null) return;

          var ParentAtom = MathList?.AtomAt(searchType) ?? MathList?.AtomAt(searchType?.Previous);

          double indexInCurrectList = 0, currectListCounter = 0, upperListCounter = 0;

          MathList requestedList;
          var lookForType = MathListSubIndexType.None;
          if (ParentAtom is Atoms.Fraction frac) {
            lookForType = MathListSubIndexType.Numerator;
            requestedList = frac.Numerator;
            currectListCounter = frac.Denominator.CountObjects - 1;
            upperListCounter = frac.Numerator.CountObjects - 1;
          } else if (ParentAtom is MathAtom atom) {
            lookForType = MathListSubIndexType.Superscript;
            requestedList = atom.Superscript;
            currectListCounter = atom.Subscript.CountObjects - 1;
            upperListCounter = atom.Superscript.CountObjects - 1;
          } else {
            throw new NullReferenceException("atomisnull");
          }

          Func<MathListIndex, bool> condition = (InsertionIndex) => {
            if (InsertionIndex.FinalSubIndexType == lookForType)
              if (MathList?.AtomListAt(InsertionIndex)?.EqualsList(requestedList) ?? false)
                return true;
            return false;
          };

          var nextMathList = SerachFor(condition, out int Index);

          MoveExecution();
          void MoveExecution() {

            double MaxMovement = indexInCurrectList + upperListCounter + 1;
            double minMovement = indexInCurrectList + 1;

            double ListsDifference = (upperListCounter - currectListCounter) / 2;

            double MoveLeftListReletive = currectListCounter - indexInCurrectList;

            MoveLeftListReletive = MoveLeftListReletive + ListsDifference;

            double MoveLeftCounter = 0;

            MoveLeftCounter += indexInCurrectList + MoveLeftListReletive;

            MoveLeftCounter = Math.Min(Math.Max(MoveLeftCounter, minMovement), MaxMovement);

            while (MoveLeftCounter-- >= 1) {
              MoveCursorLeft();
            }

          }

        }
        void MoveCursorLeft() {
          var prev = _insertionIndex.Previous;
          var previousAtom = getAtom(prev);
#pragma warning disable CS8604 // Possible null reference argument.
          switch (previousAtom) {
            case var _ when prev is null:
            case null: // At beginning of line
              // move by level down findel type
              var levelDown = _insertionIndex.LevelDown();
              var FinalType = finalSubType();
              MoveByFinalType(levelDown, FinalType);
              break;
            case Atoms.Placeholder p:
              _insertionIndex = prev;
              if ((p.Superscript.IsEmpty() && p.Subscript.IsEmpty()) || _insertionIndex.SubIndexType == MathListSubIndexType.BetweenBaseAndScripts) {
                MoveCursorLeft();
                return;
              }
              break;
            case { Superscript: var s } when s.IsNonEmpty():
              // היה צריך להחזיר null
              _insertionIndex = prev;
              IndexLevelUp(MathListSubIndexType.Superscript, MathListIndex.Level0Index(s.Count));
              break;
            case { Subscript: var s } when s.IsNonEmpty():
              _insertionIndex = prev.LevelUpWithSubIndex
                (MathListSubIndexType.Subscript, MathListIndex.Level0Index(s.Count));
              break;
            case Atoms.Inner { InnerList: var l }:
              _insertionIndex = prev;
              IndexLevelUp(MathListSubIndexType.Inner, MathListIndex.Level0Index(l.Count));
              break;
            case Atoms.Radical { Radicand: var r }:
              _insertionIndex = prev;
              IndexLevelUp(MathListSubIndexType.Radicand, MathListIndex.Level0Index(r.Count));
              break;
            case Atoms.Fraction { Denominator: var d }:
              _insertionIndex = prev;
              IndexLevelUp(MathListSubIndexType.Denominator, MathListIndex.Level0Index(d.Count));
              break;
            default:
              _insertionIndex = prev;
              break;
          }
#pragma warning restore CS8604 // Possible null reference argument.
          CheckNullIndex();
          var NextIndex = _insertionIndex?.Next;
          if (NextIndex is not null) {
            if (getAtom(NextIndex) is Atoms.Placeholder p && p.Superscript.IsEmpty() && p.Subscript.IsEmpty())
              _insertionIndex = NextIndex; // Skip right side of placeholders when end of line
          }

          void MoveByFinalType(MathListIndex? levelDown, MathListSubIndexType FinalType) {
            switch (FinalType) {
              case MathListSubIndexType.None:
                goto default;
              case var _ when levelDown is null:
                throw new InvalidCodePathException("Null levelDown despite non-None FinalSubIndexType");
              case MathListSubIndexType.Superscript:
                var scriptAtom = getAtom(levelDown);
                if (scriptAtom is null)
                  throw new InvalidCodePathException("Invalid levelDown");
                if (scriptAtom.Subscript.IsNonEmpty()) {
                  IndexLevelDownUp(MathListSubIndexType.Subscript, MathListIndex.Level0Index(scriptAtom.Subscript.Count));
                } else
                  goto case MathListSubIndexType.Subscript;
                break;
              case MathListSubIndexType.Subscript:
                IndexLevelDownUp(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                break;
              case MathListSubIndexType.BetweenBaseAndScripts:
                if (getAtom(levelDown) is Atoms.Radical rad && rad.Radicand.IsNonEmpty()) {
                  IndexLevelDownUp(MathListSubIndexType.Radicand, MathListIndex.Level0Index(rad.Radicand.Count));
                } else if (getAtom(levelDown) is Atoms.Fraction frac && frac.Denominator.IsNonEmpty()) {
                  IndexLevelDownUp(MathListSubIndexType.Denominator, MathListIndex.Level0Index(frac.Denominator.Count));
                } else if (getAtom(levelDown) is Atoms.Inner inner && inner.InnerList.IsNonEmpty()) {
                  IndexLevelDownUp(MathListSubIndexType.Inner, MathListIndex.Level0Index(inner.InnerList.Count));
                }
                  /// test
                  else goto case MathListSubIndexType.Radicand;
                break;
              case MathListSubIndexType.Radicand:
                if (getAtom(levelDown) is Atoms.Radical radDeg && radDeg.Degree.IsNonEmpty()) {
                  IndexLevelDownUp(MathListSubIndexType.Degree, MathListIndex.Level0Index(radDeg.Degree.Count));
                } else
                  goto case MathListSubIndexType.Denominator;
                break;
              case MathListSubIndexType.Denominator:
                if (getAtom(levelDown) is Atoms.Fraction fracNum && fracNum.Numerator.IsNonEmpty()) {
                  IndexLevelDownUp(MathListSubIndexType.Numerator, MathListIndex.Level0Index(fracNum.Numerator.Count));
                } else
                  goto default;
                break;
              case MathListSubIndexType.Degree:
              case MathListSubIndexType.Numerator:
              case MathListSubIndexType.Inner:
              default:
                _insertionIndex = levelDown ?? _insertionIndex;
                break;
            }
          }

          void CheckNullIndex() {
            if (_insertionIndex is null)
              throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
          }
        }
        void MoveCursorRight() {

          InsertionNullCheck();
          MoveByAtom();
          InsertionNullCheck();

          MoveRightIfPlaceHolder();

          void MoveByAtom() {
            switch (getAtom(_insertionIndex)) {
              case null: //After Count
                // move by level down findel type
                var levelDown = _insertionIndex.LevelDown();
                var levelDownAtom = getAtom(levelDown);
                searchOnLevelDown();
                break;
              case var a when finalSubType() is MathListSubIndexType.BetweenBaseAndScripts:
                levelDown = _insertionIndex.LevelDown();
                if (levelDown is null) {
                  throw new InvalidCodePathException
                    ("finalSubType() is BetweenBaseAndScripts but levelDown is null");
                }

                var typeOfAtom = a.Subscript.IsNonEmpty() ? MathListSubIndexType.Subscript : MathListSubIndexType.Superscript;

                _insertionIndex = levelDown;
                IndexLevelUp(typeOfAtom, MathListIndex.Level0Index(0));
                break;
              case Atoms.Inner _:
                IndexLevelUp(MathListSubIndexType.Inner, MathListIndex.Level0Index(0));
                break;
              case Atoms.Fraction _:
                IndexLevelUp(MathListSubIndexType.Numerator, MathListIndex.Level0Index(0));
                break;
              case Atoms.Radical rad:
                var Type = rad.Degree.IsNonEmpty() ? MathListSubIndexType.Degree : MathListSubIndexType.Radicand;
                IndexLevelUp(Type, MathListIndex.Level0Index(0));
                break;
              case var a when a.Superscript.IsNonEmpty() || a.Subscript.IsNonEmpty():
                IndexLevelUp(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                break;
              case Atoms.Placeholder:
              //  // Skip right side of placeholders when end of line
              //  goto case null;
              default:
                _insertionIndex = _insertionIndex.Next;
                break;
                void searchOnLevelDown() {
                  var FinalType = finalSubType();
                  switch (FinalType) {
                    case MathListSubIndexType.None:
                      goto default;
                    case var _ when levelDown is null:
                      throw new InvalidCodePathException("Null levelDown despite non-None FinalSubIndexType");
                    case var _ when levelDownAtom is null:
                      throw new InvalidCodePathException("Invalid levelDown");
                    case MathListSubIndexType.Degree:
                      if (levelDownAtom is Atoms.Radical)
                        IndexLevelDownUp(MathListSubIndexType.Radicand, MathListIndex.Level0Index(0));
                      else
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Radical), levelDown);
                      break;
                    case MathListSubIndexType.Numerator:
                      if (levelDownAtom is Atoms.Fraction)
                        IndexLevelDownUp(MathListSubIndexType.Denominator, MathListIndex.Level0Index(0));
                      else
                        throw new SubIndexTypeMismatchException(typeof(Atoms.Fraction), levelDown);
                      break;
                    case MathListSubIndexType.Radicand:
                    case MathListSubIndexType.Denominator:
                    case MathListSubIndexType.Inner:
                      if (HasSuperOrSub(levelDownAtom))
                        IndexLevelDownUp(MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1));
                      else
                        goto default;
                      break;
                    case MathListSubIndexType.BetweenBaseAndScripts:
                      if (levelDownAtom.Subscript.IsNonEmpty())
                        IndexLevelDownUp(MathListSubIndexType.Subscript, MathListIndex.Level0Index(0));
                      else
                        goto case MathListSubIndexType.Subscript;
                      break;
                    case MathListSubIndexType.Subscript:
                      if (levelDownAtom.Superscript.IsNonEmpty())
                        IndexLevelDownUp(MathListSubIndexType.Superscript, MathListIndex.Level0Index(0));
                      else
                        goto default;
                      break;
                    case MathListSubIndexType.Superscript:
                    default:
                      _insertionIndex = levelDown?.Next ?? _insertionIndex;
                      break;
                  }
                }
                static bool HasSuperOrSub(MathAtom levelDownAtom) {
                  return levelDownAtom.Superscript.IsNonEmpty() || levelDownAtom.Subscript.IsNonEmpty();
                }
            }
          }
          void InsertionNullCheck() {
            if (_insertionIndex is null)
              throw new InvalidOperationException($"{nameof(_insertionIndex)} is null.");
          }
          void MoveRightIfPlaceHolder() {
            if (getAtom(_insertionIndex) is Atoms.Placeholder)
              MoveCursorRight();
          }
        }
        void IndexLevelUp(MathListSubIndexType Type, MathListIndex subindex) {
          _insertionIndex = _insertionIndex.LevelUpWithSubIndex(Type, subindex);
        }
        void IndexLevelDownUp(MathListSubIndexType Type, MathListIndex subindex) {
          _insertionIndex = _insertionIndex.LevelDown() ?? _insertionIndex;
          IndexLevelUp(Type, subindex);
        }
      }
      void DeleteBackwards() {
        // delete the last atom from the list
        if (HasText) {
          if (_insertionIndex.Previous is MathListIndex previous) {
            _insertionIndex = previous;
            MathList.RemoveAt(ref _insertionIndex);
          }
        }
      }
      void InsertAtom(MathAtom atom) {
        // insert atom by his script/radical
        MathList.InsertAndAdvance(ref _insertionIndex, atom,
       atom switch {
         Atoms.Fraction _ => MathListSubIndexType.Numerator,
         Atoms.Radical { Degree: { } d } when IsPlaceholderList(d) => MathListSubIndexType.Degree,
         Atoms.Radical _ => MathListSubIndexType.Radicand,
         _ => MathListSubIndexType.None
       });
        static bool IsPlaceholderList(MathList ml) => ml.Count == 1 && ml[0] is Atoms.Placeholder;
      }
      void InsertSymbolName(string name, bool subscript = false, bool superscript = false) {
        // get atomcommand
        var atom =
          LaTeXSettings.AtomForCommand(name) ??
            throw new InvalidCodePathException("Looks like someone mistyped atom symbol name...");
        // insert atom symbole
        InsertAtom(atom);
        // insert scripts (first the super and then the sub) by the needs
        switch (subscript, superscript) {
          case (true, true):
            HandleScriptButton(true);
            _insertionIndex = _insertionIndex.LevelDown()?.Next
              ?? throw new InvalidCodePathException(
                "_insertionIndex.previousIndex returned null despite script button handling");
            HandleScriptButton(false);
            break;
          case (false, true):
            HandleScriptButton(true);
            break;
          case (true, false):
            HandleScriptButton(false);
            break;
          case (false, false):
            break;
        }
      }
      void ConvertClick(MathKeyboardInput input) {
        switch (input) {
          // TODO: Implement up/down buttons
          case MathKeyboardInput.Up:
            // searchcheck();
            MoveCursor(Diraction.Up);
            break;
          case MathKeyboardInput.Down:
            break;
          case MathKeyboardInput.Left:
            MoveCursor(Diraction.Left);
            break;
          case MathKeyboardInput.Right:
            MoveCursor(Diraction.Right);
            break;
          case MathKeyboardInput.Backspace:
            DeleteBackwards();
            break;
          case MathKeyboardInput.Clear:
            MathList.Clear();
            InsertionIndex = MathListIndex.Level0Index(0);
            break;
          case MathKeyboardInput.Return:
            return;
          case MathKeyboardInput.Dismiss:
            return;
          case MathKeyboardInput.Slash:
            HandleSlashButton();
            break;
          case MathKeyboardInput.Power:
            HandleScriptButton(true);
            break;
          case MathKeyboardInput.Subscript:
            HandleScriptButton(false);
            break;
          case MathKeyboardInput.Fraction:
            InsertAtom(new Atoms.Fraction(LaTeXSettings.PlaceholderList, LaTeXSettings.PlaceholderList));
            break;
          case MathKeyboardInput.SquareRoot:
            InsertAtom(new Atoms.Radical(new MathList(), LaTeXSettings.PlaceholderList));
            break;
          case MathKeyboardInput.CubeRoot:
            InsertAtom(new Atoms.Radical(new MathList(new Atoms.Number("3")), LaTeXSettings.PlaceholderList));
            break;
          case MathKeyboardInput.NthRoot:
            InsertAtom(new Atoms.Radical(LaTeXSettings.PlaceholderList, LaTeXSettings.PlaceholderList));
            break;
          case MathKeyboardInput.BothRoundBrackets:
            InsertInner("(", ")");
            break;
          case MathKeyboardInput.BothSquareBrackets:
            InsertInner("[", "]");
            break;
          case MathKeyboardInput.BothCurlyBrackets:
            InsertInner("{", "}");
            break;
          case MathKeyboardInput.Absolute:
            InsertInner("|", "|");
            break;
          case MathKeyboardInput.BaseEPower:
            InsertAtom(LaTeXSettings.AtomForCommand("e")
              ?? throw new InvalidCodePathException($"{nameof(LaTeXSettings.AtomForCommand)} returned null for e"));
            HandleScriptButton(true);
            break;
          case MathKeyboardInput.Caret:
            InsertSymbolName(@"\Caret");
            break;
          case MathKeyboardInput.Logarithm:
            InsertSymbolName(@"\log");
            break;
          case MathKeyboardInput.NaturalLogarithm:
            InsertSymbolName(@"\ln");
            break;
          case MathKeyboardInput.LogarithmWithBase:
            InsertSymbolName(@"\log", subscript: true);
            break;
          case MathKeyboardInput.Sine:
            InsertSymbolName(@"\sin");
            break;
          case MathKeyboardInput.Cosine:
            InsertSymbolName(@"\cos");
            break;
          case MathKeyboardInput.Tangent:
            InsertSymbolName(@"\tan");
            break;
          case MathKeyboardInput.Cotangent:
            InsertSymbolName(@"\cot");
            break;
          case MathKeyboardInput.Secant:
            InsertSymbolName(@"\sec");
            break;
          case MathKeyboardInput.Cosecant:
            InsertSymbolName(@"\csc");
            break;
          case MathKeyboardInput.ArcSine:
            InsertSymbolName(@"\arcsin");
            break;
          case MathKeyboardInput.ArcCosine:
            InsertSymbolName(@"\arccos");
            break;
          case MathKeyboardInput.ArcTangent:
            InsertSymbolName(@"\arctan");
            break;
          case MathKeyboardInput.ArcCotangent:
            InsertSymbolName(@"\arccot");
            break;
          case MathKeyboardInput.ArcSecant:
            InsertSymbolName(@"\arcsec");
            break;
          case MathKeyboardInput.ArcCosecant:
            InsertSymbolName(@"\arccsc");
            break;
          case MathKeyboardInput.HyperbolicSine:
            InsertSymbolName(@"\sinh");
            break;
          case MathKeyboardInput.HyperbolicCosine:
            InsertSymbolName(@"\cosh");
            break;
          case MathKeyboardInput.HyperbolicTangent:
            InsertSymbolName(@"\tanh");
            break;
          case MathKeyboardInput.HyperbolicCotangent:
            InsertSymbolName(@"\coth");
            break;
          case MathKeyboardInput.HyperbolicSecant:
            InsertSymbolName(@"\sech");
            break;
          case MathKeyboardInput.HyperbolicCosecant:
            InsertSymbolName(@"\csch");
            break;
          case MathKeyboardInput.AreaHyperbolicSine:
            InsertSymbolName(@"\arsinh");
            break;
          case MathKeyboardInput.AreaHyperbolicCosine:
            InsertSymbolName(@"\arcosh");
            break;
          case MathKeyboardInput.AreaHyperbolicTangent:
            InsertSymbolName(@"\artanh");
            break;
          case MathKeyboardInput.AreaHyperbolicCotangent:
            InsertSymbolName(@"\arcoth");
            break;
          case MathKeyboardInput.AreaHyperbolicSecant:
            InsertSymbolName(@"\arsech");
            break;
          case MathKeyboardInput.AreaHyperbolicCosecant:
            InsertSymbolName(@"\arcsch");
            break;
          case MathKeyboardInput.LimitWithBase:
            InsertSymbolName(@"\lim", subscript: true);
            break;
          case MathKeyboardInput.Integral:
            InsertSymbolName(@"\int");
            break;
          case MathKeyboardInput.IntegralLowerLimit:
            InsertSymbolName(@"\int", subscript: true);
            break;
          case MathKeyboardInput.IntegralUpperLimit:
            InsertSymbolName(@"\int", superscript: true);
            break;
          case MathKeyboardInput.IntegralBothLimits:
            InsertSymbolName(@"\int", subscript: true, superscript: true);
            break;
          case MathKeyboardInput.Summation:
            InsertSymbolName(@"\sum");
            break;
          case MathKeyboardInput.SummationLowerLimit:
            InsertSymbolName(@"\sum", subscript: true);
            break;
          case MathKeyboardInput.SummationUpperLimit:
            InsertSymbolName(@"\sum", superscript: true);
            break;
          case MathKeyboardInput.SummationBothLimits:
            InsertSymbolName(@"\sum", subscript: true, superscript: true);
            break;
          case MathKeyboardInput.Product:
            InsertSymbolName(@"\prod");
            break;
          case MathKeyboardInput.ProductLowerLimit:
            InsertSymbolName(@"\prod", subscript: true);
            break;
          case MathKeyboardInput.ProductUpperLimit:
            InsertSymbolName(@"\prod", superscript: true);
            break;
          case MathKeyboardInput.ProductBothLimits:
            InsertSymbolName(@"\prod", subscript: true, superscript: true);
            break;
          case MathKeyboardInput.DoubleIntegral:
            InsertSymbolName(@"\iint");
            break;
          case MathKeyboardInput.TripleIntegral:
            InsertSymbolName(@"\iiint");
            break;
          case MathKeyboardInput.QuadrupleIntegral:
            InsertSymbolName(@"\iiiint");
            break;
          case MathKeyboardInput.ContourIntegral:
            InsertSymbolName(@"\oint");
            break;
          case MathKeyboardInput.DoubleContourIntegral:
            InsertSymbolName(@"\oiint");
            break;
          case MathKeyboardInput.TripleContourIntegral:
            InsertSymbolName(@"\oiiint");
            break;
          case MathKeyboardInput.ClockwiseIntegral:
            InsertSymbolName(@"\intclockwise");
            break;
          case MathKeyboardInput.ClockwiseContourIntegral:
            InsertSymbolName(@"\varointclockwise");
            break;
          case MathKeyboardInput.CounterClockwiseContourIntegral:
            InsertSymbolName(@"\ointctrclockwise");
            break;
          case MathKeyboardInput.LeftArrow:
            InsertSymbolName(@"\leftarrow");
            break;
          case MathKeyboardInput.UpArrow:
            InsertSymbolName(@"\uparrow");
            break;
          case MathKeyboardInput.RightArrow:
            InsertSymbolName(@"\rightarrow");
            break;
          case MathKeyboardInput.DownArrow:
            InsertSymbolName(@"\downarrow");
            break;
          case MathKeyboardInput.PartialDifferential:
            InsertSymbolName(@"\partial");
            break;
          case MathKeyboardInput.NotEquals:
            InsertSymbolName(@"\neq");
            break;
          case MathKeyboardInput.LessOrEquals:
            InsertSymbolName(@"\leq");
            break;
          case MathKeyboardInput.GreaterOrEquals:
            InsertSymbolName(@"\geq");
            break;
          case MathKeyboardInput.Multiply:
            InsertSymbolName(@"\times");
            break;
          case MathKeyboardInput.Divide:
            InsertSymbolName(@"\div");
            break;
          case MathKeyboardInput.Infinity:
            InsertSymbolName(@"\infty");
            break;
          case MathKeyboardInput.Degree:
            InsertSymbolName(@"\degree");
            break;
          case MathKeyboardInput.Angle:
            InsertSymbolName(@"\angle");
            break;
          case MathKeyboardInput.LeftCurlyBracket:
            InsertSymbolName(@"\{");
            break;
          case MathKeyboardInput.RightCurlyBracket:
            InsertSymbolName(@"\}");
            break;
          case MathKeyboardInput.Percentage:
            InsertSymbolName(@"\%");
            break;
          case MathKeyboardInput.Space:
            InsertSymbolName(@"\ ");
            break;
          case MathKeyboardInput.Prime:
            InsertAtom(new Atoms.Prime(1));
            break;
          case MathKeyboardInput.LeftRoundBracket:
          case MathKeyboardInput.RightRoundBracket:
          case MathKeyboardInput.LeftSquareBracket:
          case MathKeyboardInput.RightSquareBracket:
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
          case MathKeyboardInput.Ratio:
          case MathKeyboardInput.Comma:
          case MathKeyboardInput.Semicolon:
          case MathKeyboardInput.Factorial:
          case MathKeyboardInput.VerticalBar:
          case MathKeyboardInput.Equals:
          case MathKeyboardInput.LessThan:
          case MathKeyboardInput.GreaterThan:
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
            var Atom = LaTeXSettings.AtomForCommand(new string((char)input, 1));
            InsertAtom(Atom ?? throw new InvalidCodePathException($"{nameof(LaTeXSettings.AtomForCommand)} returned null for {input}"));
            break;
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
          case MathKeyboardInput.Psi:
          case MathKeyboardInput.Omega:
          case MathKeyboardInput.SmallAlpha:
          case MathKeyboardInput.SmallBeta:
          case MathKeyboardInput.SmallGamma:
          case MathKeyboardInput.SmallDelta:
          case MathKeyboardInput.SmallEpsilon:
          case MathKeyboardInput.SmallEpsilon2:
          case MathKeyboardInput.SmallZeta:
          case MathKeyboardInput.SmallEta:
          case MathKeyboardInput.SmallTheta:
          case MathKeyboardInput.SmallIota:
          case MathKeyboardInput.SmallKappa:
          case MathKeyboardInput.SmallKappa2:
          case MathKeyboardInput.SmallLambda:
          case MathKeyboardInput.SmallMu:
          case MathKeyboardInput.SmallNu:
          case MathKeyboardInput.SmallXi:
          case MathKeyboardInput.SmallOmicron:
          case MathKeyboardInput.SmallPi:
          case MathKeyboardInput.SmallPi2:
          case MathKeyboardInput.SmallRho:
          case MathKeyboardInput.SmallRho2:
          case MathKeyboardInput.SmallSigma:
          case MathKeyboardInput.SmallSigma2:
          case MathKeyboardInput.SmallTau:
          case MathKeyboardInput.SmallUpsilon:
          case MathKeyboardInput.SmallPhi:
          case MathKeyboardInput.SmallPhi2:
          case MathKeyboardInput.SmallChi:
          case MathKeyboardInput.SmallPsi:
          case MathKeyboardInput.SmallOmega:
            // All Greek letters are rendered as variables.
            var variableAtom = new Atoms.Variable(((char)input).ToStringInvariant());
            InsertAtom(variableAtom);
            break;
          default:
            break;
        }
      }
      MathAtom? getAtom(MathListIndex? mathListIndex) => MathList?.AtomAt(mathListIndex);
      MathListSubIndexType finalSubType() => _insertionIndex.FinalSubIndexType;
    }

    public void Clear() {
      MathList.Clear();
      InsertionIndex = MathListIndex.Level0Index(0);
    }
    /// <summary>
    /// move left/right untill the condition is equal to true.
    /// </summary>
    /// <param name="Condition"></param>
    /// <param name="CountMovement"></param>
    /// <param name="ToLeft"></param>
    /// <param name="BackToFirstPos"></param>
    /// <returns></returns>
    public MathListIndex? SerachFor(Func<MathListIndex, bool> Condition,
      out int CountMovement, bool ToLeft = true, bool BackToFirstPos = true) {

      var CopyIndex = InsertionIndex;

      var isFirst = () => (InsertionIndex?.Previous == null && InsertionIndex?.LevelDown() == null);
      var isLast = () => (InsertionIndex?.LevelDown() == null && MathList.AtomAt(InsertionIndex) == null);

      Action Movement = ToLeft ? () => KeyPress(MathKeyboardInput.Left) : () => KeyPress(MathKeyboardInput.Right);
      Func<bool> OnTheEdge = ToLeft ? isFirst : isLast;

      Movement();
      CountMovement = 1;

      while (!OnTheEdge()) {
        if (Condition(InsertionIndex!)) {
          break;
        }
        Movement();
        CountMovement++;
      }

      var resultIndex = InsertionIndex;

      if (BackToFirstPos)
        InsertionIndex = CopyIndex;

      return resultIndex;



    }
    public void ChangeMathlistByAtom(MathList mathlist, MathAtom atom, bool setIndexAfterAtom = false) {
      InsertionIndex = MathListIndex.Level0Index(0);
      MathList = mathlist;
      ChangeInsertionIndexByAtom(atom, setIndexAfterAtom);
    }
    public void ChangeInsertionIndexByAtom(MathAtom atom, bool setIndexAfterAtom = false) {

      var refernceEqual = (MathListIndex i) => {
        var currectAtom = MathList?.AtomAt(i);
        if (object.ReferenceEquals(currectAtom, (atom))) {
          return true;
        }
        return false;
      };

      if (MathList.Count == 0) return;

      if (!refernceEqual(InsertionIndex)) {
        SerachFor(refernceEqual, out _, false, false);
      }

      if (setIndexAfterAtom)
        KeyPress(MathKeyboardInput.Right);

    }
    public void ChangeMathList(MathList mathlist) {
      MathList = mathlist;
      InsertionIndex = MathListIndex.Level0Index(mathlist.Count);
    }
  }
}
