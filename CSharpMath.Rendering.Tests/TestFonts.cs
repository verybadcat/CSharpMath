using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Typography.OpenFont;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  using BackEnd;
  using SkiaSharp;

  public class TestFonts {
    sealed class ProbePainter : CSharpMath.SkiaSharp.MathPainter {
      public Fonts CurrentFonts => Fonts;
    }
    sealed class SingleUseEnumerable : IEnumerable<Typeface> {
      readonly IReadOnlyList<Typeface> _items;
      bool _used;
      public int EnumerationCount { get; private set; }
      public SingleUseEnumerable(IEnumerable<Typeface> items) => _items = items.ToArray();
      public IEnumerator<Typeface> GetEnumerator() {
        if (_used) throw new InvalidOperationException("The enumerable was enumerated more than once.");
        _used = true;
        EnumerationCount++;
        return _items.GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    [Fact]
    public void ConstructorSnapshotsCallerAndPreservesPublicEnumerableSemantics() {
      var source = new SingleUseEnumerable(new[] { Fonts.GlobalTypefaces.First() });
      var fonts = new Fonts(source, 12);
      var resized = new Fonts(fonts, 24);
      var resizedAgain = new Fonts(resized, 36);

      Assert.Equal(1, source.EnumerationCount);
      Assert.Equal(4, fonts.Typefaces.Count());
      Assert.Equal(7, resized.Typefaces.Count());
      Assert.Equal(10, resizedAgain.Typefaces.Count());
      Assert.Equal(3, Fonts.GlobalTypefaces.Count());
      Assert.Equal(fonts.Typefaces.Concat(Fonts.GlobalTypefaces), resized.Typefaces);
      Assert.Equal(resized.Typefaces.Concat(Fonts.GlobalTypefaces), resizedAgain.Typefaces);

      // The public constructor intentionally consumes exactly the enumerable
      // supplied to it; passing an existing Fonts therefore preserves its
      // public enumerable semantics (including the global faces).
      var publicCopy = new Fonts(fonts, 48);
      Assert.Equal(7, publicCopy.Typefaces.Count());
    }

    [Fact]
    public void ConstructorAndPainterExposeDefensiveSnapshots() {
      var first = Fonts.GlobalTypefaces.First();
      var second = Fonts.GlobalTypefaces.Skip(1).First();
      var supplied = new[] { first };
      var fonts = new Fonts(supplied, 12);
      supplied[0] = second;
      Assert.Same(first, fonts.Typefaces.First());
      Assert.Throws<NotSupportedException>(() => ((IList<Typeface>)fonts.Typefaces).Add(second));

      var painter = new ProbePainter { LocalTypefaces = supplied };
      supplied[0] = first;
      Assert.Same(second, painter.LocalTypefaces.Single());
      Assert.Throws<NotSupportedException>(() => ((IList<Typeface>)painter.LocalTypefaces).Add(first));
    }

    [Fact]
    public void PainterRetainsLocalSnapshotAcrossRepeatedSizeChanges() {
      var source = new SingleUseEnumerable(new[] { Fonts.GlobalTypefaces.First() });
      var painter = new ProbePainter { LocalTypefaces = source };

      painter.FontSize = 13;
      painter.FontSize = 14;
      painter.FontSize = 15;

      Assert.Equal(1, source.EnumerationCount);
      Assert.Single(painter.LocalTypefaces);
      Assert.Equal(4, painter.CurrentFonts.Typefaces.Count());
      Assert.Equal(painter.LocalTypefaces.Concat(Fonts.GlobalTypefaces), painter.CurrentFonts.Typefaces);
    }

    [Fact]
    public void SamePainterCanSwitchLocalSnapshotsWithoutAccumulatingThem() {
      var first = Fonts.GlobalTypefaces.First();
      var second = Fonts.GlobalTypefaces.Skip(1).First();
      var painter = new ProbePainter { LocalTypefaces = new[] { first } };
      painter.FontSize = 21;
      painter.LocalTypefaces = new[] { second };
      painter.FontSize = 22;
      painter.LocalTypefaces = new[] { first, second };
      painter.FontSize = 23;

      Assert.Equal(new[] { first, second }, painter.LocalTypefaces);
      Assert.Equal(new[] { first, second }.Concat(Fonts.GlobalTypefaces), painter.CurrentFonts.Typefaces);
    }

    [Fact]
    public void ComicNeueLocalTypefaceRemainsUsableAcrossResizeAndSwitchCycles() {
      var path = Path.Combine(TestRenderingFixture.ThisDirectory.FullName, "ComicNeue_Bold.otf");
      using var stream = File.OpenRead(path);
      var comic = new OpenFontReader().Read(stream);
      Assert.NotNull(comic);
      var painter = new ProbePainter { LaTeX = "Comic Neue sample", LocalTypefaces = new[] { comic! } };
      painter.FontSize = 18;
      painter.LocalTypefaces = new[] { Fonts.GlobalTypefaces.First(), comic! };
      painter.FontSize = 24;
      painter.LocalTypefaces = new[] { comic! };
      using var rendered = painter.DrawAsStream();
      Assert.NotNull(rendered);
      Assert.True(rendered!.Length > 0);
      Assert.Same(comic, painter.LocalTypefaces.Single());
      Assert.Same(comic, painter.CurrentFonts.Typefaces.First());
    }
  }
}
