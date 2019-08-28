namespace CSharpMath.Editor {
  using System;
  using System.Collections.Generic;
  using System.Drawing;
  using System.Linq;
  using Display;
  using Display.Text;
  using FrontEnd;
  using Color = Structures.Color;

  partial class DisplayEditingExtensions {
    public static MathListIndex IndexForPoint<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, PointF point) where TFont : IFont<TGlyph> {
      // The origin of for the subelements of a MathList is the current position, so translate the current point to our origin.
      var translatedPoint = new PointF(point.X - self.Position.X, point.Y - self.Position.Y);

      IDisplay<TFont, TGlyph> closest = null;
      var xbounds = new List<IDisplay<TFont, TGlyph>>();
      float minDistance = float.MaxValue;
      foreach (var display in self.Displays) {
        var bounds = display.DisplayBounds;
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
      IDisplay<TFont, TGlyph> displayWithPoint;
      switch (xbounds.Count) {
        case 0:
          if (translatedPoint.X <= -PixelDelta)
            // All the way to the left
            return self.Range.Location < 0 ? null : MathListIndex.Level0Index(self.Range.Location);
          else if (translatedPoint.X >= self.Width + PixelDelta) {
            // if closest is a script
            if (closest != null && closest is ListDisplay<TFont, TGlyph> ld
                && ld.LinePosition != Enumerations.LinePosition.Regular) {
              // then we try to find its parent
              var parent = self.Displays.FirstOrDefault(d => d.HasScript && d.Range.Contains(ld.IndexInParent));

              if (parent != null) {
                return MathListIndex.Level0Index(parent.Range.End);
              }

            }
            // All the way to the right
            return self.Range.End < 0 ? null : MathListIndex.Level0Index(self.Range.End);
          } else
            // It is within the ListDisplay but not within the X bounds of any sublist. Use the closest in that case.
            displayWithPoint = closest;
          break;
        case 1:
          displayWithPoint = xbounds[0];
          var rect = new RectangleF(displayWithPoint.Position, displayWithPoint.DisplayBounds.Size);
          if (translatedPoint.X >= self.Width - PixelDelta)
            //The point is close to the end. Only use the selected X bounds if the Y is within range.
            if (translatedPoint.Y <= rect.YMin() - PixelDelta)
              //The point is less than the Y including the delta. Move the cursor to the end rather than in this atom.
              return MathListIndex.Level0Index(self.Range.End);
          break;
        default:
          //Use the closest since there are more than 2 sublists which have this X position.
          displayWithPoint = closest;
          break;
      }
      if (displayWithPoint is null)
        return null;

      var index = displayWithPoint.IndexForPoint(context, translatedPoint);
      if (displayWithPoint is ListDisplay<TFont, TGlyph> closestLine) {
        if (closestLine.LinePosition is Enumerations.LinePosition.Regular)
          throw Arg($"{nameof(ListDisplay<TFont, TGlyph>)} {nameof(ListDisplay<TFont, TGlyph>.LinePosition)} {nameof(Enumerations.LinePosition.Regular)} " +
            $"inside an {nameof(ListDisplay<TFont, TGlyph>)} - shouldn't happen", nameof(self));
        // This is a subscript or a superscript, return the right type of subindex
        var indexType = closestLine.LinePosition is Enumerations.LinePosition.Subscript ? MathListSubIndexType.Subscript : MathListSubIndexType.Superscript;
        // The index of the atom this denotes.
        if (closestLine.IndexInParent is int.MinValue)
          throw Arg($"Index was not set for a {indexType} in the {nameof(ListDisplay<TFont, TGlyph>)}.", nameof(self));
        return MathListIndex.IndexAtLocation(closestLine.IndexInParent, index, indexType);
      } else if (displayWithPoint.HasScript)
        //The display list has a subscript or a superscript. If the index is at the end of the atom, then we need to put it before the sub/super script rather than after.
        if (index?.AtomIndex == displayWithPoint.Range.End)
          return MathListIndex.IndexAtLocation(index.AtomIndex - 1, MathListIndex.Level0Index(1), MathListSubIndexType.Nucleus);
      return index;
    }
    
    public static PointF? PointForIndex<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self, TypesettingContext<TFont, TGlyph> context, MathListIndex index) where TFont : IFont<TGlyph> {
      if (index is null) return null;

      PointF? position;
      if (index.AtomIndex == self.Range.End)
        // Special case the edge of the range
        position = new PointF(self.Width, 0);
      else if (self.Range.Contains(index.AtomIndex) && self.SubDisplayForIndex(index) is IDisplay<TFont, TGlyph> display)
        switch (index.SubIndexType) {
          case MathListSubIndexType.Nucleus:
            var nucleusPosition = index.AtomIndex + index.SubIndex.AtomIndex;
            position = display.PointForIndex(context, MathListIndex.Level0Index(nucleusPosition));
            break;
          case MathListSubIndexType.None:
            if (!display.HasScript) {
              position = display.PointForIndex(context, index);
            } else {
              var mainPosition = display.PointForIndex(context, index);
              var scripted = self.Displays.SingleOrDefault(d =>
                d is ListDisplay<TFont, TGlyph> ld &&
                    ld.IndexInParent == index.AtomIndex - 1
                  );
              if (scripted != null && mainPosition != null) {
                position = new PointF(mainPosition.Value.X + scripted.Width, 0);
              } else
                position = mainPosition;
            }
            break;
          default:
            // Recurse
            position = display.PointForIndex(context, index.SubIndex);
            break;
        }
      else
        // Outside the range
        return null;

      if (position is PointF found) {
        // Convert bounds from our coordinate system before returning
        found.X += self.Position.X;
        found.Y += self.Position.Y;
        return found;
      }
      else
        // We didn't find the position
        return null;
    }
    
    public static void HighlightCharacterAt<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self, MathListIndex index, Color color) where TFont : IFont<TGlyph> {
      if (index is null) return;
      if (self.Range.Contains(index.AtomIndex) && self.SubDisplayForIndex(index) is IDisplay<TFont, TGlyph> display)
        if (index.SubIndexType is MathListSubIndexType.Nucleus || index.SubIndexType is MathListSubIndexType.None)
          display.HighlightCharacterAt(index, color);
        else
          // Recurse
          display.HighlightCharacterAt(index.SubIndex, color);
    }

    public static void Highlight<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self, Color color) where TFont : IFont<TGlyph> {
      foreach (var display in self.Displays)
        display.Highlight(color);
    }

    public static IDisplay<TFont, TGlyph> SubDisplayForIndex<TFont, TGlyph>(this ListDisplay<TFont, TGlyph> self, MathListIndex index) where TFont : IFont<TGlyph> {
      // Inside the range
      if (index.SubIndexType is MathListSubIndexType.Superscript || index.SubIndexType is MathListSubIndexType.Subscript)
        foreach (var display in self.Displays)
          if (display is ListDisplay<TFont, TGlyph> list && index.AtomIndex == list.IndexInParent &&
            // This is the right character for the sub/superscript, check that it's type matches the index
            ((list.LinePosition is Enumerations.LinePosition.Subscript && index.SubIndexType is MathListSubIndexType.Subscript) ||
            (list.LinePosition is Enumerations.LinePosition.Superscript && index.SubIndexType is MathListSubIndexType.Superscript)))
            return list;
          else { }
      else
        foreach (var display in self.Displays)
          if (!(display is ListDisplay<TFont, TGlyph>) && display.Range.Contains(index.AtomIndex))
            //not a subscript/superscript and ... jackpot, the the index is in the range of this atom.
            switch (index.SubIndexType) {
              case MathListSubIndexType.None:
              case MathListSubIndexType.Nucleus:
                return display;

              case MathListSubIndexType.Degree:
              case MathListSubIndexType.Radicand:
                if (display is RadicalDisplay<TFont, TGlyph> radical)
                  return radical.SubListForIndexType(index.SubIndexType);
                //Log($"No radical found at index {index.AtomIndex}");
                break;
              case MathListSubIndexType.Numerator:
              case MathListSubIndexType.Denominator:
                if (display is FractionDisplay<TFont, TGlyph> fraction)
                  return fraction.SubListForIndexType(index.SubIndexType);
                //Log($"No fraction found at index {index.AtomIndex}");
                break;
              case MathListSubIndexType.Superscript:
              case MathListSubIndexType.Subscript:
              default:
                throw new InvalidCodePathException("Superscripts and subscripts should have been handled in a separate case above.");
            }
      return null;
    }
  }
}
