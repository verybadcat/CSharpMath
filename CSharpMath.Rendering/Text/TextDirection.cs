using System;
using System.Collections.Generic;

using Typography.TextBreak.SheenBidi;

namespace CSharpMath.Rendering.Text {
  /// <summary>Specifies the base direction used to resolve a bidi paragraph.</summary>
  public enum TextDirection {
    /// <summary>Derive the base direction from the first strong character, defaulting to LTR.</summary>
    Auto,
    /// <summary>Resolve the paragraph with a left-to-right base direction.</summary>
    LeftToRight,
    /// <summary>Resolve the paragraph with a right-to-left base direction.</summary>
    RightToLeft
  }

  /// <summary>A non-empty logical UTF-16 range and its UAX #9 embedding level.</summary>
  public sealed class TextRun {
    /// <summary>Creates a logical UTF-16 run with a resolved embedding level.</summary>
    /// <param name="logicalStart">The run's global UTF-16 offset.</param>
    /// <param name="length">The positive run length in UTF-16 code units.</param>
    /// <param name="embeddingLevel">The resolved UAX #9 embedding level.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="logicalStart"/> is negative or <paramref name="length"/> is not positive.
    /// </exception>
    public TextRun(int logicalStart, int length, byte embeddingLevel) {
      if (logicalStart < 0)
        throw new ArgumentOutOfRangeException(nameof(logicalStart));
      if (length <= 0)
        throw new ArgumentOutOfRangeException(nameof(length));

      LogicalStart = logicalStart;
      Length = length;
      EmbeddingLevel = embeddingLevel;
    }

    /// <summary>Gets this run's global UTF-16 offset in the resolved logical text.</summary>
    public int LogicalStart { get; }
    /// <summary>Gets this run's length in UTF-16 code units.</summary>
    public int Length { get; }
    /// <summary>Gets the resolved UAX #9 embedding level.</summary>
    public byte EmbeddingLevel { get; }
    /// <summary>Gets whether the embedding level is right-to-left.</summary>
    public bool IsRightToLeft => (EmbeddingLevel & 1) != 0;
    /// <summary>Gets <see cref="LogicalStart"/>.</summary>
    public int Start => LogicalStart;
  }

  /// <summary>
  /// Immutable logical paragraph ordering metadata; it does not shape or lay out visual lines.
  /// </summary>
  public sealed class BidiParagraph {
    readonly IReadOnlyList<TextRun> _runs;

    /// <summary>Resolves one logical paragraph into contiguous embedding-level runs.</summary>
    /// <param name="text">The paragraph text, without a terminating separator.</param>
    /// <param name="direction">The requested base-direction mode.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="direction"/> is not a defined <see cref="TextDirection"/> value.
    /// </exception>
    public BidiParagraph(string text, TextDirection direction = TextDirection.Auto) :
      this(ValidateText(text), 0, string.Empty, ValidateDirection(direction)) { }

    internal BidiParagraph(
      string text,
      int logicalStart,
      string separator,
      TextDirection direction) {
      Text = text;
      LogicalStart = logicalStart;
      Length = text.Length;
      Separator = separator;
      Direction = direction;

      var baseLevel = direction == TextDirection.RightToLeft ? (byte)1 : (byte)0;
      var runs = new List<TextRun>();
      if (text.Length != 0) {
        var paragraph = new Paragraph(text, ToBaseDirection(direction));
        baseLevel = paragraph.BaseLevel;
        AddRuns(runs, text, logicalStart, paragraph.Levels);
      }

      BaseLevel = baseLevel;
      _runs = runs.AsReadOnly();
    }

    static void AddRuns(
      ICollection<TextRun> runs,
      string text,
      int logicalStart,
      IReadOnlyList<byte> levels) {
      var start = 0;
      var level = levels[0];
      for (var index = 1; index < levels.Count; index++) {
        // SheenBidi classifies a surrogate pair as one scalar while exposing one level per UTF-16
        // code unit. Keep the pair atomic if its low-surrogate BN level differs transiently.
        if (char.IsHighSurrogate(text[index - 1]) && char.IsLowSurrogate(text[index]))
          continue;
        if (levels[index] == level)
          continue;

        runs.Add(new TextRun(logicalStart + start, index - start, level));
        start = index;
        level = levels[index];
      }
      runs.Add(new TextRun(logicalStart + start, levels.Count - start, level));
    }

    static string ValidateText(string text) {
      if (text == null)
        throw new ArgumentNullException(nameof(text));
      return text;
    }

    internal static TextDirection ValidateDirection(TextDirection direction) {
      if (direction < TextDirection.Auto || direction > TextDirection.RightToLeft)
        throw new ArgumentOutOfRangeException(nameof(direction));
      return direction;
    }

    static BaseDirection ToBaseDirection(TextDirection direction) => direction switch {
      TextDirection.LeftToRight => BaseDirection.LeftToRight,
      TextDirection.RightToLeft => BaseDirection.RightToLeft,
      _ => BaseDirection.AutoLeftToRight
    };

    /// <summary>Gets the paragraph text without its terminating separator.</summary>
    public string Text { get; }
    /// <summary>Gets the paragraph's global UTF-16 offset in the resolved logical text.</summary>
    public int LogicalStart { get; }
    /// <summary>Gets the paragraph text length in UTF-16 code units, excluding <see cref="Separator"/>.</summary>
    public int Length { get; }
    /// <summary>Gets the exact CR, LF, or CRLF separator following this paragraph, if any.</summary>
    public string Separator { get; }
    /// <summary>Gets the resolved paragraph embedding level.</summary>
    public byte BaseLevel { get; }
    /// <summary>Gets contiguous logical runs covering <see cref="Text"/>.</summary>
    public IReadOnlyList<TextRun> Runs => _runs;
    /// <summary>Gets the requested base-direction mode.</summary>
    public TextDirection Direction { get; }
  }

  /// <summary>Resolves logical-order paragraphs, retaining all original separators.</summary>
  public static class BidiResolver {
    /// <summary>
    /// Splits logical text at CR, LF, and CRLF and resolves every resulting paragraph.
    /// </summary>
    /// <param name="text">The complete logical text.</param>
    /// <param name="direction">The requested base-direction mode for every paragraph.</param>
    /// <returns>
    /// An immutable list that retains empty paragraphs and exact separators. Paragraph and run
    /// positions are global UTF-16 offsets into <paramref name="text"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="direction"/> is not a defined <see cref="TextDirection"/> value.
    /// </exception>
    public static IReadOnlyList<BidiParagraph> ResolveParagraphs(
      string text,
      TextDirection direction = TextDirection.Auto) {
      if (text == null)
        throw new ArgumentNullException(nameof(text));
      BidiParagraph.ValidateDirection(direction);

      var result = new List<BidiParagraph>();
      var start = 0;
      var index = 0;
      while (index < text.Length) {
        if (text[index] != '\r' && text[index] != '\n') {
          index++;
          continue;
        }

        var separatorStart = index++;
        if (text[separatorStart] == '\r' && index < text.Length && text[index] == '\n')
          index++;

        result.Add(new BidiParagraph(
          text.Substring(start, separatorStart - start),
          start,
          text.Substring(separatorStart, index - separatorStart),
          direction));
        start = index;
      }

      result.Add(new BidiParagraph(text.Substring(start), start, string.Empty, direction));
      return result.AsReadOnly();
    }
  }
}
