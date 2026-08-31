using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CSharpMath.Rendering.Text;
using Xunit;

namespace CSharpMath.Rendering.Tests {
  // The cases explicitly labelled "BidiCharacterTest" below are a transcribed/modified,
  // representative subset of Unicode 17.0 BidiCharacterTest-17.0.0.txt. They do not claim
  // full UAX #9 conformance. Versioned source URL and row references are retained below.
  // Normative algorithm: https://www.unicode.org/reports/tr9/tr9-51.html (revision 51).
  // Versioned vectors: https://www.unicode.org/Public/17.0.0/ucd/BidiCharacterTest.txt.
  // Redistributed under Unicode License v3; see Unicode-Data-LICENSE.txt in this project.
  public class TestBidiParagraphs {
    sealed class UnknownTextAtom : TextAtom {
      public override int? SingleChar(Atom.FontStyle style) => null;
      public override bool Equals(TextAtom atom) => ReferenceEquals(this, atom);
      public override int GetHashCode() => 0;
    }

    static void AssertRuns(
      BidiParagraph paragraph,
      byte expectedBaseLevel,
      params (int start, int length, byte level)[] expected) {
      Assert.Equal(expectedBaseLevel, paragraph.BaseLevel);
      Assert.Equal(expected.Length, paragraph.Runs.Count);
      Assert.Equal(expected, paragraph.Runs.Select(r => (r.LogicalStart, r.Length, r.EmbeddingLevel)));

      Assert.Equal(paragraph.Text.Length, paragraph.Length);
      var end = checked(paragraph.LogicalStart + paragraph.Length);
      var cursor = paragraph.LogicalStart;
      foreach (var run in paragraph.Runs) {
        Assert.NotNull(run);
        Assert.True(run.Length > 0);
        Assert.Equal(cursor, run.LogicalStart);
        Assert.Equal(run.LogicalStart, run.Start);
        Assert.Equal((run.EmbeddingLevel & 1) != 0, run.IsRightToLeft);

        var runEnd = checked(run.LogicalStart + run.Length);
        Assert.InRange(runEnd, paragraph.LogicalStart + 1, end);
        AssertNotInsideSurrogatePair(paragraph, run.LogicalStart);
        AssertNotInsideSurrogatePair(paragraph, runEnd);
        cursor = runEnd;
      }
      Assert.Equal(end, cursor);
    }

    static void AssertNotInsideSurrogatePair(BidiParagraph paragraph, int globalBoundary) {
      var paragraphEnd = paragraph.LogicalStart + paragraph.Length;
      if (globalBoundary <= paragraph.LogicalStart || globalBoundary >= paragraphEnd)
        return;

      var localBoundary = globalBoundary - paragraph.LogicalStart;
      Assert.False(
        char.IsHighSurrogate(paragraph.Text[localBoundary - 1]) &&
        char.IsLowSurrogate(paragraph.Text[localBoundary]));
    }

    // Focused #289 regression derived directly from UAX #9 revision 51, not a
    // BidiCharacterTest row. Starting classes are L L L WS R R AL WS EN EN EN AN AN.
    // W2/W3, N1/N2, and I2 resolve levels to 0 0 0 0 1 1 1 1 2 2 2 2 2.
    [Fact]
    public void MixedHebrewArabicAndDigitsRegressionHasThreeRuns() =>
      AssertRuns(
        new BidiParagraph("abc אבج 123٤٥"),
        0,
        (0, 4, 0),
        (4, 4, 1),
        (8, 5, 2));

    // Focused #289 regression derived directly from UAX #9 revision 51, not a
    // BidiCharacterTest row. N0 resolves the brackets to L; X5b gives the LRI level 0
    // and pushes level 2; W7/I1 leave its digits at level 2; X6a gives the PDI level 0.
    // The resulting levels are 0 0 0 1 1 1 0 0 2 2 2 0.
    [Fact]
    public void BracketsAndLriRegressionHasFiveRuns() =>
      AssertRuns(
        new BidiParagraph("A (אבג)\u2066123\u2069"),
        0,
        (0, 3, 0),
        (3, 3, 1),
        (6, 2, 0),
        (8, 3, 2),
        (11, 1, 0));

    // BidiCharacterTest-17.0.0.txt:46, direction 0.
    // 0061 0062 0063 0020 0028 0064 0065 0066 0020 0627 0628 062C 0029 0020 05D0 05D1 05D2
    [Fact]
    public void Unicode17HebrewArabicLatinLtrVector() =>
      AssertRuns(
        new BidiParagraph("abc (def ابج) אבג", TextDirection.LeftToRight),
        0,
        (0, 9, 0),
        (9, 3, 1),
        (12, 2, 0),
        (14, 3, 1));

    // BidiCharacterTest-17.0.0.txt:47, direction 1; same code points as line 46.
    [Fact]
    public void Unicode17HebrewArabicLatinRtlVector() =>
      AssertRuns(
        new BidiParagraph("abc (def ابج) אבג", TextDirection.RightToLeft),
        1,
        (0, 3, 2),
        (3, 2, 1),
        (5, 3, 2),
        (8, 9, 1));

    // UAX #9 P2/P3 selects LTR for the line-46 vector, so Auto has its direction-0 levels.
    [Fact]
    public void AutoUsesLtrVectorLevelsWhenFirstStrongCharacterIsL() =>
      AssertRuns(
        new BidiParagraph("abc (def ابج) אבג", TextDirection.Auto),
        0,
        (0, 9, 0),
        (9, 3, 1),
        (12, 2, 0),
        (14, 3, 1));

    // UAX #9 P2/P3 selects RTL for BidiCharacterTest lines 52-53, so Auto has line 53's levels.
    // 05D0 05D1 05D2 0020 0028 0627 0628 062C 0020 0064 0065 0066 0029 0020 0061 0062 0063
    [Fact]
    public void AutoUsesRtlVectorLevelsWhenFirstStrongCharacterIsR() =>
      AssertRuns(
        new BidiParagraph("אבג (ابج def) abc", TextDirection.Auto),
        1,
        (0, 9, 1),
        (9, 3, 2),
        (12, 2, 1),
        (14, 3, 2));

    // BidiCharacterTest-17.0.0.txt:143, direction 1.
    // 0061 0020 0031 0020 0032 002D 0033
    [Fact]
    public void Unicode17EuropeanDigitsRtlVector() =>
      AssertRuns(
        new BidiParagraph("a 1 2-3", TextDirection.RightToLeft),
        1,
        (0, 7, 2));

    // BidiCharacterTest-17.0.0.txt:144, direction 0.
    // 05D0 0020 0031 002D 0032
    [Fact]
    public void Unicode17HebrewEuropeanDigitsLtrVector() =>
      AssertRuns(
        new BidiParagraph("א 1-2", TextDirection.LeftToRight),
        0,
        (0, 2, 1),
        (2, 3, 2));

    // BidiCharacterTest-17.0.0.txt:197, direction 0.
    // 0061 0028 0661 0029
    [Fact]
    public void Unicode17ArabicIndicDigitAndBracketsLtrVector() =>
      AssertRuns(
        new BidiParagraph("a(١)", TextDirection.LeftToRight),
        0,
        (0, 2, 0),
        (2, 1, 2),
        (3, 1, 0));

    // BidiCharacterTest-17.0.0.txt:198, direction 1; same code points as line 197.
    [Fact]
    public void Unicode17ArabicIndicDigitAndBracketsRtlVector() =>
      AssertRuns(
        new BidiParagraph("a(١)", TextDirection.RightToLeft),
        1,
        (0, 1, 2),
        (1, 1, 1),
        (2, 1, 2),
        (3, 1, 1));

    // BidiCharacterTest-17.0.0.txt:181, direction 0.
    // 0061 0028 05D0 005B 05D1 005D 0021 0029 0062
    [Fact]
    public void Unicode17PairedBracketsAndNeutralPunctuationLtrVector() =>
      AssertRuns(
        new BidiParagraph("a(א[ב]!)b", TextDirection.LeftToRight),
        0,
        (0, 2, 0),
        (2, 4, 1),
        (6, 3, 0));

    // BidiCharacterTest-17.0.0.txt:182, direction 1; same code points as line 181.
    [Fact]
    public void Unicode17PairedBracketsAndNeutralPunctuationRtlVector() =>
      AssertRuns(
        new BidiParagraph("a(א[ב]!)b", TextDirection.RightToLeft),
        1,
        (0, 1, 2),
        (1, 7, 1),
        (8, 1, 2));

    // BidiCharacterTest-17.0.0.txt:282, direction 0.
    // 0061 0028 0062 005B 0063 2068 05D0 2069 0064 005D 0065 0029 0066
    [Fact]
    public void Unicode17FsiAndPdiLtrVector() =>
      AssertRuns(
        new BidiParagraph("a(b[c\u2068א\u2069d]e)f", TextDirection.LeftToRight),
        0,
        (0, 6, 0),
        (6, 1, 1),
        (7, 6, 0));

    // BidiCharacterTest-17.0.0.txt:283, direction 1; same code points as line 282.
    [Fact]
    public void Unicode17FsiAndPdiRtlVector() =>
      AssertRuns(
        new BidiParagraph("a(b[c\u2068א\u2069d]e)f", TextDirection.RightToLeft),
        1,
        (0, 6, 2),
        (6, 1, 3),
        (7, 6, 2));

    // BidiCharacterTest-17.0.0.txt:293, direction 0.
    // 0061 0028 0062 2067 05D0 0066 2069 05D4 0029 05D5
    [Fact]
    public void Unicode17RliAndPdiLtrVector() =>
      AssertRuns(
        new BidiParagraph("a(b\u2067אf\u2069ד)ו", TextDirection.LeftToRight),
        0,
        (0, 4, 0),
        (4, 1, 1),
        (5, 1, 2),
        (6, 1, 0),
        (7, 1, 1),
        (8, 1, 0),
        (9, 1, 1));

    // BidiCharacterTest-17.0.0.txt:294, direction 1; same code points as line 293.
    [Fact]
    public void Unicode17RliAndPdiRtlVector() =>
      AssertRuns(
        new BidiParagraph("a(b\u2067אf\u2069ד)ו", TextDirection.RightToLeft),
        1,
        (0, 1, 2),
        (1, 1, 1),
        (2, 1, 2),
        (3, 1, 1),
        (4, 1, 3),
        (5, 1, 4),
        (6, 4, 1));

    // BidiCharacterTest-17.0.0.txt:304, direction 0.
    // 05D0 0028 05D1 2066 0061 05D5 2069 0065 0029 0066
    [Fact]
    public void Unicode17LriAndPdiLtrVector() =>
      AssertRuns(
        new BidiParagraph("א(ב\u2066aו\u2069e)f", TextDirection.LeftToRight),
        0,
        (0, 1, 1),
        (1, 1, 0),
        (2, 1, 1),
        (3, 1, 0),
        (4, 1, 2),
        (5, 1, 3),
        (6, 4, 0));

    // BidiCharacterTest-17.0.0.txt:305, direction 1; same code points as line 304.
    [Fact]
    public void Unicode17LriAndPdiRtlVector() =>
      AssertRuns(
        new BidiParagraph("א(ב\u2066aו\u2069e)f", TextDirection.RightToLeft),
        1,
        (0, 4, 1),
        (4, 1, 2),
        (5, 1, 3),
        (6, 1, 1),
        (7, 1, 2),
        (8, 1, 1),
        (9, 1, 2));

    [Fact]
    public void SplitsTerminatorsAndRetainsEmptySegments() {
      var paragraphs = BidiResolver.ResolveParagraphs("\r\none\n\r\n");
      Assert.Equal(new[] { "", "one", "", "" }, paragraphs.Select(p => p.Text));
      Assert.Equal(new[] { "\r\n", "\n", "\r\n", "" }, paragraphs.Select(p => p.Separator));
      Assert.Equal(new[] { 0, 2, 6, 8 }, paragraphs.Select(p => p.LogicalStart));
      Assert.All(paragraphs, paragraph => Assert.Equal(TextDirection.Auto, paragraph.Direction));
    }

    [Fact]
    public void AllowsEmptyInputWithRequestedBaseDirection() {
      var paragraphs = BidiResolver.ResolveParagraphs("", TextDirection.RightToLeft);

      var paragraph = Assert.Single(paragraphs);
      Assert.Equal("", paragraph.Text);
      Assert.Equal("", paragraph.Separator);
      Assert.Equal(0, paragraph.LogicalStart);
      Assert.Equal(TextDirection.RightToLeft, paragraph.Direction);
      AssertRuns(paragraph, 1);
    }

    [Fact]
    public void RejectsInvalidArgumentsWithoutChangingPainterDirection() {
      Assert.Throws<ArgumentNullException>(() => new BidiParagraph(null!));
      Assert.Throws<ArgumentNullException>(() => BidiResolver.ResolveParagraphs(null!));
      Assert.Throws<ArgumentOutOfRangeException>(
        () => new BidiParagraph("x", (TextDirection)99));
      Assert.Throws<ArgumentOutOfRangeException>(
        () => BidiResolver.ResolveParagraphs("", (TextDirection)99));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TextRun(-1, 1, 0));
      Assert.Throws<ArgumentOutOfRangeException>(() => new TextRun(0, 0, 0));

      var painter = new SkiaSharp.TextPainter();
      Assert.Equal(TextDirection.LeftToRight, painter.TextDirection);
      Assert.Throws<ArgumentOutOfRangeException>(
        () => painter.TextDirection = (TextDirection)99);
      Assert.Equal(TextDirection.LeftToRight, painter.TextDirection);
    }

    [Fact]
    public void SupplementaryCharactersHaveContiguousGlobalUtf16Runs() {
      const string text = "A😀אב\r\nאב😀A";
      var paragraphs = BidiResolver.ResolveParagraphs(text);

      Assert.Equal(2, paragraphs.Count);
      Assert.Equal("\r\n", paragraphs[0].Separator);
      AssertRuns(paragraphs[0], 0, (0, 3, 0), (3, 2, 1));
      AssertRuns(paragraphs[1], 1, (7, 4, 1), (11, 1, 2));
    }

    [Fact]
    public void TextPainterProjectsEverySupportedAtomWithGlobalOffsets() {
      var wrappedText = new TextAtom.Style(
        new TextAtom.Size(
          new TextAtom.Colored(
            new TextAtom.Accent(new TextAtom.Text("A😀"), "^"),
            Color.Red),
          18),
        Atom.FontStyle.Roman);
      var painter = new SkiaSharp.TextPainter {
        Content = new TextAtom.List(new TextAtom[] {
          wrappedText,
          new TextAtom.Space(Atom.Length.Point),
          new TextAtom.ControlSpace(),
          new TextAtom.Comment("omitted"),
          new UnknownTextAtom(),
          new TextAtom.Newline(),
          new TextAtom.Math(new Atom.MathList(), false),
          new TextAtom.Comment("also omitted"),
          new TextAtom.Text("אב")
        }),
        TextDirection = TextDirection.Auto
      };

      var paragraphs = painter.BidiParagraphs;
      Assert.Equal(2, paragraphs.Count);
      Assert.Equal("A😀  ", paragraphs[0].Text);
      Assert.Equal("\r\n", paragraphs[0].Separator);
      Assert.Equal("\uFFFCאב", paragraphs[1].Text);
      Assert.Equal("", paragraphs[1].Separator);
      AssertRuns(paragraphs[0], 0, (0, 5, 0));
      AssertRuns(paragraphs[1], 1, (7, 3, 1));
    }

    [Fact]
    public void TextPainterMetadataIsImmutableAndRecomputed() {
      var painter = new SkiaSharp.TextPainter {
        Content = new TextAtom.Text("abc"),
        TextDirection = TextDirection.Auto
      };
      var first = painter.BidiParagraphs;
      var firstParagraph = Assert.Single(first);

      Assert.Throws<NotSupportedException>(
        () => ((IList<BidiParagraph>)first).Add(new BidiParagraph("x")));
      Assert.Throws<NotSupportedException>(
        () => ((IList<TextRun>)firstParagraph.Runs).Add(new TextRun(0, 1, 0)));

      painter.Content = new TextAtom.Text("אב");
      painter.TextDirection = TextDirection.RightToLeft;
      var second = painter.BidiParagraphs;
      var secondParagraph = Assert.Single(second);
      var third = painter.BidiParagraphs;

      Assert.NotSame(first, second);
      Assert.NotSame(second, third);
      Assert.Equal("abc", firstParagraph.Text);
      Assert.Equal((byte)0, firstParagraph.BaseLevel);
      Assert.Equal("אב", secondParagraph.Text);
      Assert.Equal((byte)1, secondParagraph.BaseLevel);
    }

    [Fact]
    public void PainterBaseDirectionDefaultsToLtrAndIsNotAnAlignmentInput() {
      var painter = new SkiaSharp.TextPainter { Content = new TextAtom.Text("אב") };

      Assert.Equal(TextDirection.LeftToRight, painter.TextDirection);
      Assert.Equal((byte)0, Assert.Single(painter.BidiParagraphs).BaseLevel);
      var metadataProperty = typeof(SkiaSharp.TextPainter)
        .GetProperty(nameof(SkiaSharp.TextPainter.BidiParagraphs));
      Assert.NotNull(metadataProperty);
      Assert.Empty(metadataProperty.GetIndexParameters());

      painter.TextDirection = TextDirection.Auto;
      Assert.Equal((byte)1, Assert.Single(painter.BidiParagraphs).BaseLevel);
    }
  }
}
