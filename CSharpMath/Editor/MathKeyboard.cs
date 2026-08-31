namespace CSharpMath.Editor {
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Timers;
  using Atom;
  using Display;
  using Display.FrontEnd;
  using Atoms = Atom.Atoms;

  /// <summary>Controls which vertically-related branch horizontal navigation enters.</summary>
  public enum MathKeyboardHorizontalNavigationMode : byte {
    /// <summary>Visit all branches in the historical structural order.</summary>
    Exhaustive,
    /// <summary>Prefer the upper branch and leave a compound from either branch.</summary>
    VisualUpper,
    /// <summary>Prefer the lower branch and leave a compound from either branch.</summary>
    VisualLower
  }

  public class MathKeyboard<TFont, TGlyph> : IDisposable where TFont : IFont<TGlyph> {
    protected Timer blinkTimer;
    public const double DefaultBlinkMilliseconds = 800;
    public MathKeyboard(TypesettingContext<TFont, TGlyph> context, TFont font, double blinkMilliseconds = DefaultBlinkMilliseconds) {
      Context = context;
      Font = font;
      blinkTimer = new Timer(blinkMilliseconds);
      blinkTimer.Elapsed += (sender, e) => {
        if (!(MathList.AtomAt(_insertionIndex) is Atoms.Placeholder) || LaTeXSettings.PlaceholderBlinks)
          InsertionPositionHighlighted = !InsertionPositionHighlighted;
      };
      blinkTimer.Start();
    }
    public bool ShouldDrawCaret => InsertionPositionHighlighted && !(MathList.AtomAt(_insertionIndex) is Atoms.Placeholder);
    /// <summary>Gets or sets the horizontal navigation policy.</summary>
    public MathKeyboardHorizontalNavigationMode HorizontalNavigationMode { get; set; }
    public void StartBlinking() => blinkTimer.Start();
    public void StopBlinking() => blinkTimer.Stop();
    protected TypesettingContext<TFont, TGlyph> Context { get; }
    static void ResetPlaceholders(MathList mathList) {
      foreach (var mathAtom in mathList.Atoms) {
        ResetPlaceholders(mathAtom.Superscript);
        ResetPlaceholders(mathAtom.Subscript);
        switch (mathAtom) {
          case Atoms.Placeholder placeholder:
            placeholder.Color = LaTeXSettings.PlaceholderRestingColor;
            placeholder.Nucleus = LaTeXSettings.PlaceholderRestingNucleus;
            break;
          case IMathListContainer container:
            foreach (var list in container.InnerLists)
              ResetPlaceholders(list);
            break;
        }
      }
    }
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
    public MathList MathList { get; } = [];
    public string LaTeX => LaTeXParser.MathListToLaTeX(MathList).ToString();
    private MathListIndex _insertionIndex = new(0);
    private bool _insertionIndexCameFromVerticalNavigation;
    public MathListIndex InsertionIndex {
      get => _insertionIndex;
      set {
        _insertionIndexCameFromVerticalNavigation = false;
        _insertionIndex = value;
        ResetPlaceholders(MathList);
        InsertionPositionHighlighted = true;
      }
    }
    public TFont Font { get; set; }
    public LineStyle LineStyle { get; set; }
    public Color SelectColor { get; set; }
    public virtual RectangleF Measure => Display?.DisplayBounds() ?? RectangleF.Empty;
    public bool HasText => MathList?.Atoms?.Count > 0;
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

    // Hit-test in the list selected by the destination index. A root-level
    // hit-test can select a sibling branch when the destination is nested,
    // losing part of the index path.
    MathListIndex? IndexForVerticalPoint(
      Display.Displays.ListDisplay<TFont, TGlyph> display,
      MathListIndex target,
      PointF point) {
      if (target.SubIndexInfo is not { } subIndexInfo
        || subIndexInfo.SubIndexType is MathListSubIndexType.BetweenBaseAndScripts)
        return display.IndexForPoint(Context, point);

      var (type, subIndex) = subIndexInfo;
      var child = display.SubDisplayForIndex(target);
      if (child is null)
        return null;
      var translatedPoint = new PointF(point.X - display.Position.X, point.Y - display.Position.Y);
      var childIndex = child is Display.Displays.ListDisplay<TFont, TGlyph> childList
        ? IndexForVerticalPoint(childList, subIndex, translatedPoint)
        : child.IndexForPoint(Context, translatedPoint);
      return childIndex?.Wrap(target.AtomIndex, type);
    }

    MathListIndex? VerticalIndexAtPoint(MathListIndex target, PointF sourcePoint, bool constrainToTargetList = true) {
      if (Display is null)
        return null;
      if (Display.Width == 0)
        return CenteredVerticalIndex(_insertionIndex, target);
      var targetPoint = ClosestPointToIndex(target);
      return targetPoint is PointF point
        ? constrainToTargetList
          ? IndexForVerticalPoint(Display, target, new(sourcePoint.X, point.Y))
          : ClosestIndexToPoint(new(sourcePoint.X, point.Y))
        : null;
    }

    MathListIndex CenteredVerticalIndex(MathListIndex source, MathListIndex target) {
      MathList? ListAtIndex(MathListIndex index) {
        var list = MathList;
        for (var path = index; path.SubIndexInfo is { } info; path = info.SubIndex) {
          if (path.AtomIndex < 0 || path.AtomIndex >= list.Count)
            return null;
          var atom = list[path.AtomIndex];
          list = info.SubIndexType switch {
            MathListSubIndexType.Superscript => atom.Superscript,
            MathListSubIndexType.Subscript => atom.Subscript,
            MathListSubIndexType.Numerator when atom is Atoms.Fraction fraction => fraction.Numerator,
            MathListSubIndexType.Denominator when atom is Atoms.Fraction fraction => fraction.Denominator,
            MathListSubIndexType.Radicand when atom is Atoms.Radical radical => radical.Radicand,
            MathListSubIndexType.Degree when atom is Atoms.Radical radical => radical.Degree,
            MathListSubIndexType.Inner when atom is Atoms.Inner inner => inner.InnerList,
            _ => null!,
          };
          if (list is null)
            return null;
        }
        return list;
      }
      static MathListIndex WithFinalIndex(MathListIndex index, int finalIndex) =>
        index.SubIndexInfo is not { } info
        ? new(finalIndex)
        : new(index.AtomIndex, (info.SubIndexType, WithFinalIndex(info.SubIndex, finalIndex)));

      var sourceList = ListAtIndex(source);
      var targetList = ListAtIndex(target);
      if (sourceList is null || targetList is null)
        return target;
      if (targetList.Count == 1 && targetList[0] is Atoms.Placeholder)
        return WithFinalIndex(target, 0);
      var sourcePosition =
        sourceList.Count == 1 && sourceList[0] is Atoms.Placeholder && source.FinalIndex == 0
        ? 0.5f
        : source.FinalIndex;
      var targetPosition = (int)Math.Round(
        sourcePosition + (targetList.Count - sourceList.Count) / 2f,
        MidpointRounding.AwayFromZero);
      return WithFinalIndex(target, Math.Max(0, Math.Min(targetPosition, targetList.Count)));
    }
    public void KeyPress(params MathKeyboardInput[] inputs) {
      foreach (var input in inputs) KeyPress(input);
    }
    public void KeyPress(MathKeyboardInput input) {
      void HandleScriptButton(bool isSuperScript) {
        var subIndexType = isSuperScript ? MathListSubIndexType.Superscript : MathListSubIndexType.Subscript;
        MathList GetScript(MathAtom atom) => isSuperScript ? atom.Superscript : atom.Subscript;
        void SetScript(MathAtom atom, MathList value) => GetScript(atom).Append(value);
        void CreateEmptyAtom() {
          // Create an empty atom and move the insertion index up.
          var emptyAtom = LaTeXSettings.Placeholder;
          SetScript(emptyAtom, LaTeXSettings.PlaceholderList);
          _insertionIndex = MathList.InsertAndAdvance(_insertionIndex, emptyAtom, subIndexType);
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
        if (_insertionIndex.Previous is not MathListIndex previous) {
          CreateEmptyAtom();
        } else {
          var isBetweenBaseAndScripts =
            _insertionIndex.FinalSubIndexType is MathListSubIndexType.BetweenBaseAndScripts;
          var prevIndexCorrected =
            isBetweenBaseAndScripts
            ? _insertionIndex.LevelDown()
              ?? throw new InvalidCodePathException("BetweenBaseAndScripts index has null LevelDown")
            : previous;
          var prevAtom = MathList.AtomAt(prevIndexCorrected) ?? throw new InvalidCodePathException("prevAtom is null");
          if (!isBetweenBaseAndScripts && IsFullPlaceholderRequired(prevAtom)) {
            CreateEmptyAtom();
          } else {
            var script = GetScript(prevAtom);
            if (script.IsEmpty()) {
              SetScript(prevAtom, LaTeXSettings.PlaceholderList);
            }
            _insertionIndex = prevIndexCorrected.LevelUpWithSubIndex(subIndexType, 0);
          }
        }
      }

      void HandleSlashButton() {
        // special / handling - makes the thing a fraction
        var numerator = new Stack<MathAtom>();
        var parenDepth = 0;
        if (_insertionIndex.FinalSubIndexType == MathListSubIndexType.BetweenBaseAndScripts)
          _insertionIndex = _insertionIndex.LevelDown()?.Next
              ?? throw new InvalidCodePathException("_insertionIndex.LevelDown() returned null");
        for (; _insertionIndex.Previous != null; _insertionIndex = _insertionIndex.Previous) {
          switch (MathList.AtomAt(_insertionIndex.Previous), parenDepth) {
            case (null, _): throw new InvalidCodePathException("Invalid _insertionIndex");
            // Stop looking behind upon encountering these atoms unparenthesized
            case (Atoms.Open _, _) when --parenDepth < 0: goto stop;
            case (Atoms.Close a, _): parenDepth++; numerator.Push(a); break;
            case (Atoms.UnaryOperator _, 0): goto stop;
            case (Atoms.BinaryOperator _, 0): goto stop;
            case (Atoms.Relation _, 0): goto stop;
            case (Atoms.Fraction _, 0): goto stop;
            case (Atoms.Open _, _) when parenDepth < 0: goto stop;
            // We don't put this atom on the fraction
            case (var a, _): numerator.Push(a); break;
          }
        }
      stop: MathList.RemoveAtoms(new MathListRange(_insertionIndex, numerator.Count));
        if (numerator.Count == 0)
          // so we didn't really find any numbers before this, so make the numerator 1
          numerator.Push(new Atoms.Number("1"));
        if (MathList.AtomAt(_insertionIndex.Previous) is Atoms.Fraction)
          // Add a times symbol
          _insertionIndex = MathList.InsertAndAdvance(_insertionIndex, LaTeXSettings.Times, null);
        _insertionIndex = MathList.InsertAndAdvance(_insertionIndex, new Atoms.Fraction(
          [.. numerator],
          LaTeXSettings.PlaceholderList
        ), MathListSubIndexType.Denominator);
      }
      void InsertInner(string left, string right) =>
        _insertionIndex = MathList.InsertAndAdvance(_insertionIndex,
          new Atoms.Inner(new Boundary(left), LaTeXSettings.PlaceholderList, new Boundary(right)),
          MathListSubIndexType.Inner);

      void MoveCursorHorizontal(bool right) {
        // Horizontal navigation is a walk over the ordered branches of the
        // adjacent compound atom.  Keeping this order in one place makes
        // entry and exit exact inverses for every navigation policy.
        static bool HasContent(MathList list) => list.IsNonEmpty();
        static int Branches(MathAtom atom, MathKeyboardHorizontalNavigationMode mode,
          Span<MathListSubIndexType> branches) {
          var count = 0;
          if (atom is Atoms.Inner inner)
            branches[count++] = MathListSubIndexType.Inner;
          else if (atom is Atoms.Fraction fraction) {
            if (mode != MathKeyboardHorizontalNavigationMode.VisualLower)
              branches[count++] = MathListSubIndexType.Numerator;
            if (mode != MathKeyboardHorizontalNavigationMode.VisualUpper)
              branches[count++] = MathListSubIndexType.Denominator;
          } else if (atom is Atoms.Radical radical) {
            if (HasContent(radical.Degree))
              branches[count++] = MathListSubIndexType.Degree;
            branches[count++] = MathListSubIndexType.Radicand;
          }

          var hasSub = HasContent(atom.Subscript);
          var hasSuper = HasContent(atom.Superscript);
          if (hasSub || hasSuper) {
            // A placeholder's base-to-script seam is an implementation
            // detail; it must not become an extra horizontal stop.
            if (atom is not Atoms.Placeholder)
              branches[count++] = MathListSubIndexType.BetweenBaseAndScripts;
            if (mode == MathKeyboardHorizontalNavigationMode.Exhaustive) {
              if (hasSub)
                branches[count++] = MathListSubIndexType.Subscript;
              if (hasSuper)
                branches[count++] = MathListSubIndexType.Superscript;
            } else {
              var preferredSuper = mode == MathKeyboardHorizontalNavigationMode.VisualUpper;
              if ((preferredSuper && hasSuper) || (!preferredSuper && !hasSub))
                branches[count++] = MathListSubIndexType.Superscript;
              else
                branches[count++] = MathListSubIndexType.Subscript;
            }
          }
          return count;
        }

        static MathList BranchList(MathAtom atom, MathListSubIndexType branch) => branch switch {
          MathListSubIndexType.Superscript => atom.Superscript,
          MathListSubIndexType.Subscript => atom.Subscript,
          MathListSubIndexType.Numerator when atom is Atoms.Fraction fraction => fraction.Numerator,
          MathListSubIndexType.Denominator when atom is Atoms.Fraction fraction => fraction.Denominator,
          MathListSubIndexType.Radicand when atom is Atoms.Radical radical => radical.Radicand,
          MathListSubIndexType.Degree when atom is Atoms.Radical radical => radical.Degree,
          MathListSubIndexType.Inner when atom is Atoms.Inner inner => inner.InnerList,
          _ => throw new InvalidCodePathException("Invalid horizontal navigation branch")
        };

        static MathListIndex Enter(MathListIndex owner, MathAtom atom,
          MathListSubIndexType branch, bool atEnd) {
          var index = branch == MathListSubIndexType.BetweenBaseAndScripts
            ? 1 : atEnd ? BranchList(atom, branch).Count : 0;
          return owner.LevelUpWithSubIndex(branch, index);
        }

        MathListIndex? Step(bool moveRight) {
          var adjacent = moveRight ? MathList.AtomAt(_insertionIndex) : MathList.AtomAt(_insertionIndex.Previous);
          var terminalPlaceholder = moveRight
            && adjacent is Atoms.Placeholder {
              Superscript: { Count: 0 }, Subscript: { Count: 0 }
            }
            && MathList.AtomAt(_insertionIndex.Next) is null;
          if (adjacent is not null && !terminalPlaceholder) {
            Span<MathListSubIndexType> branches = stackalloc MathListSubIndexType[5];
            var branchCount = Branches(adjacent, HorizontalNavigationMode, branches);
            if (branchCount == 0)
              return moveRight ? _insertionIndex.Next : _insertionIndex.Previous;
            var owner = moveRight ? _insertionIndex : _insertionIndex.Previous!;
            return Enter(owner, adjacent, moveRight ? branches[0] : branches[branchCount - 1], !moveRight);
          }

          var ownerIndex = _insertionIndex.LevelDown();
          if (ownerIndex is null)
            return _insertionIndex; // root boundary
          var ownerAtom = MathList.AtomAt(ownerIndex);
          if (ownerAtom is null)
            return _insertionIndex;
          Span<MathListSubIndexType> branchesAtOwner = stackalloc MathListSubIndexType[5];
          var branchCountAtOwner = Branches(ownerAtom, HorizontalNavigationMode, branchesAtOwner);
          var currentType = _insertionIndex.FinalSubIndexType;
          var currentBranch = -1;
          for (var i = 0; i < branchCountAtOwner; i++)
            if (branchesAtOwner[i] == currentType) {
              currentBranch = i;
              break;
            }
          var nextBranch = currentBranch + (moveRight ? 1 : -1);
          if (currentBranch >= 0 && nextBranch >= 0 && nextBranch < branchCountAtOwner)
            return Enter(ownerIndex, ownerAtom, branchesAtOwner[nextBranch], !moveRight);
          return moveRight ? ownerIndex.Next : ownerIndex;
        }

        for (var attempts = 0; attempts < 4; attempts++) {
          var next = Step(right);
          if (next is null || next == _insertionIndex)
            break;
          _insertionIndex = next;
          // Do not expose the seam after a terminal bare placeholder.
          if (right && _insertionIndex.FinalSubIndexType == MathListSubIndexType.BetweenBaseAndScripts
            && MathList.AtomAt(_insertionIndex.LevelDown()) is Atoms.Placeholder)
            continue;
          if (!right && MathList.AtomAt(_insertionIndex) is null
            && _insertionIndex.Previous is { } previous
            && MathList.AtomAt(previous) is Atoms.Placeholder {
              Superscript: { Count: 0 }, Subscript: { Count: 0 }
            }) {
            _insertionIndex = previous;
          }
          break;
        }
      }
      // Tables use an explicit row/column path; this keeps vertical movement
      // independent of the display tree (which may contain empty cells).
      static MathList? ChildList(MathAtom atom, MathListSubIndexType type) => type switch {
        MathListSubIndexType.Superscript => atom.Superscript,
        MathListSubIndexType.Subscript => atom.Subscript,
        MathListSubIndexType.Numerator when atom is Atoms.Fraction f => f.Numerator,
        MathListSubIndexType.Denominator when atom is Atoms.Fraction f => f.Denominator,
        MathListSubIndexType.Radicand when atom is Atoms.Radical r => r.Radicand,
        MathListSubIndexType.Degree when atom is Atoms.Radical r => r.Degree,
        MathListSubIndexType.Inner when atom is Atoms.Inner i => i.InnerList,
        _ => null,
      };

      MathListIndex FindTable(MathList list, MathListIndex index, bool down, out bool handled) {
        handled = false;
        if (index.AtomIndex < 0 || index.AtomIndex >= list.Count || index.SubIndexInfo is not { } info)
          return index;
        var atom = list[index.AtomIndex];
        if (info.SubIndexType == MathListSubIndexType.TableRow
          && atom is Atoms.Table table
          && info.SubIndex.SubIndexInfo is (MathListSubIndexType.TableColumn, var column)
          && column.SubIndexInfo is (MathListSubIndexType.TableCell, var cell)) {
          if (info.SubIndex.AtomIndex < 0 || info.SubIndex.AtomIndex >= table.Cells.Count
            || column.AtomIndex < 0 || column.AtomIndex >= table.Cells[info.SubIndex.AtomIndex].Count) {
            handled = true;
            return index;
          }
          // Prefer a nested table in the current cell, retaining this complete prefix.
          var cellResult = FindTable(table.Cells[info.SubIndex.AtomIndex][column.AtomIndex], cell, down, out handled);
          if (handled)
            return new(index.AtomIndex, (info.SubIndexType,
              new(info.SubIndex.AtomIndex, (MathListSubIndexType.TableColumn,
                new(column.AtomIndex, (MathListSubIndexType.TableCell, cellResult))))));
          var row = info.SubIndex.AtomIndex;
          var sourceColumn = column.AtomIndex;
          var targetRow = row + (down ? 1 : -1);
          while (targetRow >= 0 && targetRow < table.NRows && table.Cells[targetRow].Count == 0)
            targetRow += down ? 1 : -1;
          if (targetRow < 0 || targetRow >= table.NRows) { handled = true; return index; }
          var targetColumn = Math.Min(sourceColumn, table.Cells[targetRow].Count - 1);
          var targetCell = table.Cells[targetRow][targetColumn];
          var targetCaret = Math.Max(0, Math.Min(cell.AtomIndex, targetCell.Count));
          var candidate = new MathListIndex(index.AtomIndex).TableCell(targetRow, targetColumn, new MathListIndex(targetCaret));
          // Older display backends do not yet expose table row paths to PointForIndex;
          // retain the deterministic caret fallback when that seam cannot resolve one.
          try {
            var sourcePoint = ClosestPointToIndex(index);
            if (sourcePoint is PointF point && ClosestPointToIndex(candidate) is PointF)
              candidate = VerticalIndexAtPoint(candidate, point) ?? candidate;
          } catch (ArgumentOutOfRangeException) {
            // The model path remains valid even when the display path is not indexed.
          }
          handled = true;
          return candidate;
        }
        var child = ChildList(atom, info.SubIndexType);
        if (child is null) return index;
        var nested = FindTable(child, info.SubIndex, down, out handled);
        return handled ? new(index.AtomIndex, (info.SubIndexType, nested)) : index;
      }
      bool MoveCursorInTable(bool down) {
        var result = FindTable(MathList, _insertionIndex, down, out var handled);
        if (handled) _insertionIndex = result;
        return handled;
      }
      void MoveCursorUp() {
        if (MoveCursorInTable(false)) return;
        if (Display is null) RecreateDisplayFromMathList();
        if (MathList.AtomAt(_insertionIndex) is Atoms.Placeholder { Superscript: { Count: var superCount } } && superCount > 0) {
          _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListSubIndexType.Superscript, 0);
          return;
        }
        if (_insertionIndex.Previous is { } prev && MathList.AtomAt(prev) is { Superscript: var super } && super.Count > 0) {
          _insertionIndex = prev.LevelUpWithSubIndex(MathListSubIndexType.Superscript, super.Count);
          if (_insertionIndex.Previous is { } prev2 && MathList.AtomAt(prev2) is Atoms.Placeholder p && p.Superscript.IsEmpty() && p.Subscript.IsEmpty())
            _insertionIndex = prev2;
          return;
        }
        for (MathListIndex? verticalIndex = _insertionIndex; verticalIndex != null; verticalIndex = verticalIndex.LevelDown()) {
          switch (verticalIndex.FinalSubIndexType) {
            case MathListSubIndexType.Denominator:
              var numerator =
                verticalIndex.LevelDown()?.LevelUpWithSubIndex(MathListSubIndexType.Numerator, 0)
                ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
              var sourcePoint =
                ClosestPointToIndex(_insertionIndex)
                ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
              _insertionIndex = VerticalIndexAtPoint(numerator, sourcePoint) ?? numerator;
              return;
            case MathListSubIndexType.Subscript:
              var levelDown =
                verticalIndex.LevelDown()
                ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
              sourcePoint =
                ClosestPointToIndex(_insertionIndex)
                ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
              if (MathList.AtomAt(levelDown) is { Superscript: { Count: 0 } } atom) {
                var left =
                  atom is Atoms.Placeholder
                  ? levelDown
                  : levelDown.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, 1);
                var leftX =
                  ClosestPointToIndex(left)?.X
                  ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
                var right = levelDown.Next;
                var rightX =
                  ClosestPointToIndex(right)?.X
                  ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
                _insertionIndex = Display?.Width == 0
                  ? verticalIndex.FinalIndex switch {
                    0 => left,
                    var index when index >= atom.Subscript.Count => right,
                    _ => sourcePoint.X - leftX <= rightX - sourcePoint.X ? left : right,
                  }
                  : sourcePoint.X - leftX <= rightX - sourcePoint.X ? left : right;
              } else {
                var superscript =
                  levelDown?.LevelUpWithSubIndex(MathListSubIndexType.Superscript, 0)
                  ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
                _insertionIndex = VerticalIndexAtPoint(superscript, sourcePoint, false) ?? superscript;
              }
              return;
            case MathListSubIndexType.BetweenBaseAndScripts:
              levelDown =
                verticalIndex.LevelDown()
                ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
              if (MathList.AtomAt(levelDown)?.Superscript.IsNonEmpty()
                ?? throw new InvalidCodePathException(nameof(levelDown) + " is invalid for " + nameof(MathList))) {
                _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Superscript, 0);
                return;
              }
              break;
          }
        }
      }
      void MoveCursorDown() {
        if (MoveCursorInTable(true)) return;
        if (Display is null) RecreateDisplayFromMathList();
        if (MathList.AtomAt(_insertionIndex) is Atoms.Placeholder { Subscript: { Count: var subCount } } && subCount > 0) {
          _insertionIndex = _insertionIndex.LevelUpWithSubIndex(MathListSubIndexType.Subscript, 0);
          return;
        }
        if (_insertionIndex.Previous is { } prev && MathList.AtomAt(prev) is { Subscript: var sub } && sub.Count > 0) {
          _insertionIndex = prev.LevelUpWithSubIndex(MathListSubIndexType.Subscript, sub.Count);
          if (_insertionIndex.Previous is { } prev2 && MathList.AtomAt(prev2) is Atoms.Placeholder p && p.Superscript.IsEmpty() && p.Subscript.IsEmpty())
            _insertionIndex = prev2;
          return;
        }
        for (MathListIndex? verticalIndex = _insertionIndex; verticalIndex != null; verticalIndex = verticalIndex.LevelDown()) {
          switch (verticalIndex.FinalSubIndexType) {
            case MathListSubIndexType.Numerator:
              var denominator =
                verticalIndex.LevelDown()?.LevelUpWithSubIndex(MathListSubIndexType.Denominator, 0)
                ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
              var sourcePoint =
                ClosestPointToIndex(_insertionIndex)
                ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
              _insertionIndex = VerticalIndexAtPoint(denominator, sourcePoint) ?? denominator;
              return;
            case MathListSubIndexType.Superscript:
              var levelDown =
                verticalIndex.LevelDown()
                ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
              sourcePoint =
                ClosestPointToIndex(_insertionIndex)
                ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
              if (MathList.AtomAt(levelDown) is { Subscript: { Count: 0 } } atom) {
                var left =
                  atom is Atoms.Placeholder
                  ? levelDown
                  : levelDown.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, 1);
                var leftX =
                  ClosestPointToIndex(left)?.X
                  ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
                var right = levelDown.Next;
                var rightX =
                  ClosestPointToIndex(right)?.X
                  ?? throw new InvalidCodePathException("Null closest point despite valid " + nameof(MathListIndex));
                _insertionIndex = Display?.Width == 0
                  ? verticalIndex.FinalIndex switch {
                    0 => left,
                    var index when index >= atom.Superscript.Count => right,
                    _ => sourcePoint.X - leftX <= rightX - sourcePoint.X ? left : right,
                  }
                  : sourcePoint.X - leftX <= rightX - sourcePoint.X ? left : right;
              } else {
                var subscript =
                  levelDown?.LevelUpWithSubIndex(MathListSubIndexType.Subscript, 0)
                  ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
                _insertionIndex = VerticalIndexAtPoint(subscript, sourcePoint, false) ?? subscript;
              }
              return;
            case MathListSubIndexType.BetweenBaseAndScripts:
              levelDown =
                verticalIndex.LevelDown()
                ?? throw new InvalidCodePathException("Null levelDown despite non-None " + nameof(verticalIndex.FinalSubIndexType));
              if (MathList.AtomAt(levelDown)?.Subscript.IsNonEmpty()
                ?? throw new InvalidCodePathException(nameof(levelDown) + " is invalid for " + nameof(MathList))) {
                _insertionIndex = levelDown.LevelUpWithSubIndex(MathListSubIndexType.Subscript, 0);
                return;
              }
              break;
          }
        }
      }
      void DeleteBackwards() {
        // delete the last atom from the list
        if (HasText && _insertionIndex.Previous is MathListIndex previous)
          _insertionIndex = MathList.RemoveAt(previous);
      }

      static bool IsPlaceholderList(MathList ml) => ml.Count == 1 && ml[0] is Atoms.Placeholder;
      void InsertAtom(MathAtom a) {
        static bool ContainsNestedScript(MathList list) {
          foreach (var atom in list.Atoms)
            if (atom.Superscript.IsNonEmpty() || atom.Subscript.IsNonEmpty())
              return true;
          return false;
        }
        var advanceType = a switch {
          Atoms.Fraction _ => MathListSubIndexType.Numerator,
          Atoms.Radical { Degree: { } d } when IsPlaceholderList(d) => MathListSubIndexType.Degree,
          Atoms.Radical _ => MathListSubIndexType.Radicand,
          _ => (MathListSubIndexType?)null
        };
        var insertionIndexBefore = _insertionIndex;
        var preserveVerticalScriptPosition =
          advanceType is null
          && MathList.AtomAt(insertionIndexBefore) is Atoms.Placeholder placeholder
          && (placeholder.Superscript.IsNonEmpty() || placeholder.Subscript.IsNonEmpty())
          && (_insertionIndexCameFromVerticalNavigation
            || ContainsNestedScript(placeholder.Superscript)
            || ContainsNestedScript(placeholder.Subscript));
        _insertionIndex = MathList.InsertAndAdvance(_insertionIndex, a, advanceType);
        if (preserveVerticalScriptPosition)
          _insertionIndex = insertionIndexBefore.LevelUpWithSubIndex(MathListSubIndexType.BetweenBaseAndScripts, 1);
      }
      void InsertSymbolName(string name, bool subscript = false, bool superscript = false) {
        var atom =
          LaTeXSettings.AtomForCommand(name) ??
            throw new InvalidCodePathException("Looks like someone mistyped a symbol name...");
        InsertAtom(atom);
        switch (subscript, superscript) {
          case (true, true):
            HandleScriptButton(true);
            _insertionIndex = _insertionIndex.LevelDown()?.Next
              ?? throw new InvalidCodePathException(
                "_insertionIndex.Previous returned null despite script button handling");
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

      switch (input) {
        case MathKeyboardInput.Up:
          var previousIndex = _insertionIndex;
          MoveCursorUp();
          _insertionIndexCameFromVerticalNavigation = previousIndex != _insertionIndex;
          break;
        case MathKeyboardInput.Down:
          previousIndex = _insertionIndex;
          MoveCursorDown();
          _insertionIndexCameFromVerticalNavigation = previousIndex != _insertionIndex;
          break;
        case MathKeyboardInput.Left:
          MoveCursorHorizontal(false);
          break;
        case MathKeyboardInput.Right:
          MoveCursorHorizontal(true);
          break;
        case MathKeyboardInput.Backspace:
          DeleteBackwards();
          break;
        case MathKeyboardInput.Clear:
          MathList.Clear();
          InsertionIndex = new(0);
          break;
        case MathKeyboardInput.Return:
          ReturnPressed?.Invoke(this, EventArgs.Empty);
          InsertionPositionHighlighted = false;
          StopBlinking();
          return;
        case MathKeyboardInput.Dismiss:
          DismissPressed?.Invoke(this, EventArgs.Empty);
          InsertionPositionHighlighted = false;
          StopBlinking();
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
          InsertAtom(LaTeXSettings.AtomForCommand(new string((char)input, 1))
            ?? throw new InvalidCodePathException($"{nameof(LaTeXSettings.AtomForCommand)} returned null for {input}"));
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
          InsertAtom(new Atoms.Variable(((char)input).ToStringInvariant()));
          break;
        default:
          break;
      }
      if (input is not MathKeyboardInput.Up and not MathKeyboardInput.Down)
        _insertionIndexCameFromVerticalNavigation = false;
      ResetPlaceholders(MathList);
      InsertionPositionHighlighted = true;
    }

    public void MoveCaretToPoint(PointF point) {
      point.Y *= -1; //inverted canvas, blah blah
      InsertionIndex = ClosestIndexToPoint(point) ?? new(MathList.Atoms.Count);
    }

    public void Clear() {
      MathList.Clear();
      InsertionIndex = new(0);
    }

    // Insert a list at a given point.
    public void InsertMathList(MathList list, PointF point) {
      var detailedIndex = ClosestIndexToPoint(point) ?? new(0);
      // insert at the given index - but don't consider sublevels at this point
      var index = new MathListIndex(detailedIndex.AtomIndex);
      foreach (var atom in list.Atoms)
        index = MathList.InsertAndAdvance(index, atom, null);
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
    public void Dispose() {
      ((IDisposable)blinkTimer).Dispose();
    }
  }
}
