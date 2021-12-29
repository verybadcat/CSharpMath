namespace CSharpMath.Editor {
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Drawing;
  using System.Linq;
  using System.Threading;
  using CSharpMath.Atom;
  using Display;
  using Display.Displays;
  using Display.FrontEnd;
  using Structures;

  partial class Extensions {
    public static MathListIndex? IndexForPoint<TFont, TGlyph>
      (this ListDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point)
      where TFont : IFont<TGlyph> {
      // The origin of for the subelements of a MathList is the current position,
      // so translate the current point to our origin.
      var translatedPoint = new PointF(point.X - self.Position.X, point.Y - self.Position.Y);

      IDisplay<TFont, TGlyph>? closest = null;
      var xbounds = new List<IDisplay<TFont, TGlyph>>();
      float minDistance = float.MaxValue;
      foreach (var display in self.Displays) {
        var bounds = display.DisplayBounds();
        var rect = new RectangleF(display.Position, bounds.Size);
        var maxBoundsX = rect.Right;
        if (rect.X - PixelDelta <= translatedPoint.X && translatedPoint.X <= maxBoundsX + PixelDelta)
          xbounds.Add(display);
        var distance = DistanceFromPointToRect(translatedPoint, rect);
        if (distance < minDistance) {
          closest = display;
          minDistance = distance;
        }
      }
      IDisplay<TFont, TGlyph>? displayWithPoint;
      switch (xbounds.Count) {
        case 0:
          if (translatedPoint.X <= -PixelDelta)
            // All the way to the left
            return self.Range.Location < 0
                   ? null
                   : MathListIndex.Level0Index(self.Range.Location);
          else if (translatedPoint.X >= self.Width + PixelDelta) {
            // if closest is a script
            if (closest is ListDisplay<TFont, TGlyph> ld && ld.LinePosition != LinePosition.Regular) {
              // then we try to find its parent
              var parent = self.Displays.FirstOrDefault(d => d.HasScript && d.Range.Contains(ld.IndexInParent));
              if (parent != null) {
                return MathListIndex.Level0Index(parent.Range.End);
              }
            }
            // All the way to the right
            return
              self.Range.End < 0
              ? null
              : self.Displays.Count == 1
                && self.Displays[0] is TextLineDisplay<TFont, TGlyph> { Atoms:var atoms }
                && atoms.Count == 1
                && atoms[0] is Atom.Atoms.Placeholder
              ? MathListIndex.Level0Index(self.Range.Location)
              : MathListIndex.Level0Index(self.Range.End);
          } else
            // It is within the ListDisplay but not within the X bounds of any sublist.
            // Use the closest in that case.
            displayWithPoint = closest;
          break;
        case 1:
          displayWithPoint = xbounds[0];
          break;
        default:
          // Use the closest since there are more than 2 sublists which have this X position.
          displayWithPoint = closest;
          break;
      }
      if (displayWithPoint is null)
        return null;

      var index = displayWithPoint.IndexForPoint(context, translatedPoint);
      if (displayWithPoint is ListDisplay<TFont, TGlyph> closestLine) {
        if (closestLine.LinePosition is LinePosition.Regular)
          throw new ArgumentException(nameof(ListDisplay<TFont, TGlyph>) + " with " +
            nameof(ListDisplay<TFont, TGlyph>.LinePosition) + " " + nameof(LinePosition.Regular) +
            $" inside a {nameof(ListDisplay<TFont, TGlyph>)} - shouldn't happen", nameof(self));
        // This is a subscript or a superscript, return the right type of subindex
        var indexType =
          closestLine.LinePosition is LinePosition.Subscript
          ? MathListSubIndexType.Subscript
          : MathListSubIndexType.Superscript;
        // The index of the atom this denotes.
        if (closestLine.IndexInParent is int.MinValue)
          throw new ArgumentException
            ($"Index was not set for a {indexType} in the {nameof(ListDisplay<TFont, TGlyph>)}.", nameof(self));
        return new MathListIndex(closestLine.IndexInParent, (indexType, index!)); // TODO: verify non-null
      } else if (displayWithPoint.HasScript)
        // The display list has a subscript or a superscript.
        // If the index is at the end of the atom,
        // then we need to put it before the sub/super script rather than after.
        if (index?.AtomIndex == displayWithPoint.Range.End)
          return new MathListIndex(
            index.AtomIndex - 1, (MathListSubIndexType.BetweenBaseAndScripts, MathListIndex.Level0Index(1)));
      return index;
    }

    public static PointF? PointForIndex<TFont, TGlyph>
      (this ListDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, MathListIndex index)
      where TFont : IFont<TGlyph> {

      PointF? position;
      if (index.AtomIndex == self.Range.End) {
        // Special case the edge of the range
        position = new PointF(self.Width, 0);
      }
      else if (self.Range.Contains(index.AtomIndex)
                 && self.SubDisplayForIndex(index) is IDisplay<TFont, TGlyph> display)
      {
        position = index.SubIndexInfo switch
        {
          (MathListSubIndexType.BetweenBaseAndScripts, MathListIndex subIndex) =>
            display.PointForIndex(context, MathListIndex.Level0Index(index.AtomIndex + subIndex.AtomIndex)),
          null => display.PointForIndex(context, index),
          (_, MathListIndex subIndex) => display.PointForIndex(context, subIndex)
        };
      }
        else
        // Outside the range
        return null;
      if (position is PointF found) {
        // Convert bounds from our coordinate system before returning
        found.X += self.Position.X;
        found.Y += self.Position.Y;
        return found;
      } else
        // We didn't find the position
        return null;
    }
    
    public static void HighlightCharacterAt<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self,
      MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (self.Range.Contains(index.AtomIndex) && self.SubDisplayForIndex(index) is IDisplay<TFont, TGlyph> display)
      {
        switch (index.SubIndexInfo) {
          case (MathListSubIndexType.BetweenBaseAndScripts, _):
          case null:
            display.HighlightCharacterAt(index, color); break;
          case (_, MathListIndex subIndex):
            // Recurse
            display.HighlightCharacterAt(subIndex, color); break;
          default:break;
        }
      }
    }

    public static void Highlight<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self, Color color)
      where TFont : IFont<TGlyph> {
      foreach (var display in self.Displays)
        display.Highlight(color);
    }

    public static IDisplay<TFont, TGlyph>? SubDisplayForIndex<TFont, TGlyph>(
      this ListDisplay<TFont, TGlyph> self, MathListIndex index) where TFont : IFont<TGlyph> {
      // Inside the range
      if (index.SubIndexInfo is (MathListSubIndexType.Superscript,_)
         || index.SubIndexInfo is (MathListSubIndexType.Subscript,_))
        foreach (var display in self.Displays)
          switch (display) {
            case ListDisplay<TFont, TGlyph> list
              when index.AtomIndex == list.IndexInParent
              // This is the right character for the sub/superscript, check that it's type matches the index
              && (list.LinePosition is LinePosition.Subscript
               && index.SubIndexInfo is (MathListSubIndexType.Subscript,_)
               || list.LinePosition is LinePosition.Superscript
               && index.SubIndexInfo is (MathListSubIndexType.Superscript,_)):
              return list;
            case LargeOpLimitsDisplay<TFont, TGlyph> largeOps
              when largeOps.Range.Contains(index.AtomIndex):
              return index.SubIndexInfo is (MathListSubIndexType.Subscript,_)
              ? largeOps.LowerLimit : largeOps.UpperLimit;
          }
      else
        foreach (var display in self.Displays)
          if (!(display is ListDisplay<TFont, TGlyph>) && display.Range.Contains(index.AtomIndex))
            //not a subscript/superscript and ... jackpot, the the index is in the range of this atom.
            switch (index.SubIndexInfo) {
              case null:
              case (MathListSubIndexType.BetweenBaseAndScripts, _):
                return display;
              case (MathListSubIndexType.Degree, _) when display is RadicalDisplay<TFont, TGlyph> radical:
                  return radical.Degree;
              case (MathListSubIndexType.Radicand, _) when display is RadicalDisplay<TFont, TGlyph> radical:
                return radical.Radicand;
              case (MathListSubIndexType.Numerator, _) when display is FractionDisplay<TFont, TGlyph> fraction:
                return fraction.Numerator;
              case (MathListSubIndexType.Denominator, _) when display is FractionDisplay<TFont, TGlyph> fraction:
                return fraction.Denominator;
              case (MathListSubIndexType.Inner, _) when display is InnerDisplay<TFont, TGlyph> inner:
                return inner.Inner;
              case (MathListSubIndexType.Superscript, _):
              case (MathListSubIndexType.Subscript, _):
                throw new InvalidCodePathException
                  ("Superscripts and subscripts should have been handled in a separate case above.");
              default:
                throw new SubIndexTypeMismatchException(index);
            }
      return null;
    }
  }
}