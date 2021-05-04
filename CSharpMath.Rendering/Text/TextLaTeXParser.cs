using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;

namespace CSharpMath.Rendering.Text {
  using Atom;
  using CSharpMath.Structures;
  using System.Drawing;
  using static CSharpMath.Structures.Result;
  public static class TextLaTeXParser {
    /* //Paste this into the C# Interactive, fill <username> yourself
#r "C:/Users/<username>/source/repos/CSharpMath/Typography/Build/NetStandard/Typography.TextBreak/bin/Debug/netstandard1.3/Typography.TextBreak.dll"
using Typography.TextBreak;
(int, WordKind, char)[] BreakText(string text) {
var breaker = new CustomBreaker();
var breakList = new List<BreakAtInfo>();
breaker.BreakWords(text);
breaker.LoadBreakAtList(breakList);
//index is after the boundary -> last one will be out of range
return breakList.Select(i => (i.breakAt, i.wordKind, text.ElementAtOrDefault(i.breakAt))).ToArray();
}
BreakText(@"Here are some text $1 + 12 \frac23 \sqrt4$ $$Display$$ text")
     */
    /* //Version 2
#r "C:/Users/<username>/source/repos/CSharpMath/Typography/Build/NetStandard/Typography.TextBreak/bin/Debug/netstandard1.3/Typography.TextBreak.dll"
using Typography.TextBreak;
string BreakText(string text, string seperator = "|")
{
  var breaker = new CustomBreaker();
  var breakList = new List<BreakAtInfo>();
  breaker.BreakWords(text);
  breaker.LoadBreakAtList(breakList);
  //reverse to ensure earlier inserts do not affect later ones
  foreach (var @break in breakList.Select(i => i.breakAt).Reverse())
      text = text.Insert(@break, seperator);
  return text;
}
BreakText(@"Here are some text $1 + 12 \frac23 \sqrt4$ $$Display$$ text")
     */
    /// <summary>Handle additional languages</summary>
    public static List<BreakingEngine> AdditionalBreakingEngines { get; } = new();
    public static Result<TextAtom> TextAtomFromLaTeX(string latexSource) {
      if (string.IsNullOrEmpty(latexSource))
        return new TextAtom.List(Array.Empty<TextAtom>());
      int endAt = 0;
      bool? displayMath = null;
      var mathLaTeX = new StringBuilder();
      bool backslashEscape = false;
      bool afterCommand = false; // ignore spaces after command
      bool afterNewline = false;
      int dollarCount = 0;
      var globalAtoms = new TextAtomListBuilder();
      List<BreakAtInfo> breakList = new List<BreakAtInfo>(); // Roslyn bug that assumes breakList is nullable resulting in warnings so var is not used
      var breaker = new CustomBreaker(v => breakList.Add(new BreakAtInfo(v.LatestBreakAt, v.LatestWordKind))) {
        BreakNumberAfterText = true,
        ThrowIfCharOutOfRange = false
      };
      foreach (var engine in AdditionalBreakingEngines)
        breaker.AddBreakingEngine(engine);
      breaker.BreakWords(latexSource);

      Result CheckDollarCount(int startAt, ref int endAt, TextAtomListBuilder atoms) {
        switch (dollarCount) {
          case 0:
            break;
          case 1:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                return "Cannot close display math mode with $";
              case false:
                if (atoms.Math(mathLaTeX.ToString(), false, startAt, ref endAt).Error is string error)
                  return error;
                mathLaTeX.Clear();
                displayMath = null;
                break;
              case null:
                displayMath = false;
                break;
            }
            break;
          case 2:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                if (atoms.Math(mathLaTeX.ToString(), true, startAt - 1, ref endAt).Error is string error)
                  return error;
                mathLaTeX.Clear();
                displayMath = null;
                break;
              case false:
                return "Cannot close inline math mode with $$";
              case null:
                displayMath = true;
                break;
            }
            break;
          default:
            return "Invalid number of $: " + dollarCount;
        }
        return Ok();
      }
      Result<int> BuildBreakList(ReadOnlySpan<char> latex, TextAtomListBuilder atoms,
          int i, bool oneCharOnly, char stopChar) {
        void ParagraphBreak() {
          atoms.Break();
          atoms.Space(Space.ParagraphIndent);
        }
        for (; i < breakList.Count; i++) {
          void ObtainSection(ReadOnlySpan<char> latexInput, int index,
            out int start, out int end, out ReadOnlySpan<char> section, out WordKind kind) {
            (start, end) = (index == 0 ? 0 : breakList[index - 1].breakAt, breakList[index].breakAt);
            section = latexInput.Slice(start, end - start);
            kind = breakList[index].wordKind;
          }
          ObtainSection(latex, i, out var startAt, out endAt, out var textSection, out var wordKind);
          bool NextSection(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            bool success = ++i < breakList.Count;
            if (success) ObtainSection(latexInput, i, out startAt, out endAt, out section, out wordKind);
            return success;
          }
          Result<TextAtom> ReadArgumentAtom(ReadOnlySpan<char> latexInput) {
            backslashEscape = false;
            var argAtoms = new TextAtomListBuilder();
            return BuildBreakList(latexInput, argAtoms, ++i, true, '\0')
              .Bind(index => { i = index; return argAtoms.Build(); });
          }
          SpanResult<char> ReadArgumentString(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            afterCommand = false;
            if (!NextSection(latexInput, ref section) || section.IsNot('{')) return Err("Missing {");
            int endingIndex = -1;
            //startAt + 1 to not start at the { we started at
            bool isEscape = false;
            for (int j = startAt + 1, bracketDepth = 0; j < latexInput.Length; j++) {
              if (latexInput[j] == '\\')
                isEscape = true;
              else if (latexInput[j] == '{' && !isEscape) bracketDepth++;
              else if (latexInput[j] == '}' && !isEscape)
                if (bracketDepth > 0) bracketDepth--;
                else { endingIndex = j; break; }
              else isEscape = false;
            }
            if (endingIndex == -1) return Err("Missing }");
            var resultText = latexInput.Slice(endAt, endingIndex - endAt);
            while (startAt < endingIndex)
              _ = NextSection(latexInput, ref section); //this never fails because the above check
            return Ok(resultText);
          }
          Result<Color> ReadColor(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section)  =>
            ReadArgumentString(latexInput, ref section).Bind(color =>
              LaTeXSettings.ParseColor(color.ToString()) is Color value ?
                Ok(value) :
              Err("Invalid color: " + color.ToString())
            );
          //Nothing should be before dollar sign checking -- dollar sign checking uses continue;
          atoms.TextLength = startAt;
          if (textSection.Is('$')) {
            if (backslashEscape)
              if (displayMath != null) mathLaTeX.Append(@"\$");
              else atoms.Text("$");
            else {
              dollarCount++;
              continue;
            }
            backslashEscape = false;
          } else {
            { if (CheckDollarCount(startAt, ref endAt, atoms).Error is string error) return error; }
            switch (backslashEscape, displayMath) {
              case (false, { }):
                //Unescaped text section, inside display/inline math mode
                switch (textSection) {
                  case var _ when textSection.Is('$'):
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case var _ when textSection.Is('\\'):
                    backslashEscape = true;
                    continue;
                  default:
                    mathLaTeX.Append(textSection);
                    break;
                }
                afterCommand = false;
                break;
              case (false, null):
                //Unescaped text section, not inside display/inline math mode
                switch (textSection) {
                  case var _ when stopChar > 0 && textSection[0] == stopChar:
                    return Ok(i);
                  case var _ when textSection.Is('$'):
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case var _ when textSection.Is('\\'):
                    backslashEscape = true;
                    continue;
                  case var _ when textSection.Is('#'):
                    return "Unexpected command argument reference character # outside of new command definition (currently unsupported)";
                  case var _ when textSection.Is('^'):
                  case var _ when textSection.Is('_'):
                    return $"Unexpected script indicator {textSection[0]} outside of math mode";
                  case var _ when textSection.Is('&'):
                    return $"Unexpected alignment tab character & outside of table environments";
                  case var _ when textSection.Is('~'):
                    atoms.ControlSpace();
                    break;
                  case var _ when textSection.Is('%'):
                    var comment = new StringBuilder();
                    while (NextSection(latex, ref textSection) && wordKind != WordKind.NewLine)
                      comment.Append(textSection);
                    atoms.Comment(comment.ToString());
                    break;
                  case var _ when textSection.Is('{'):
                    if (BuildBreakList(latex, atoms, ++i, false, '}').Bind(index => i = index).Error is string error)
                      return error;
                    break;
                  case var _ when textSection.Is('}'):
                    return "Missing opening brace";
                  // !char.IsSurrogate(textSection[0]) is result of https://github.com/LayoutFarm/Typography/issues/206
                  case var _ when wordKind == WordKind.NewLine && !char.IsSurrogate(textSection[0]):
                    if (oneCharOnly) continue;
                    // Consume newlines after commands
                    // Double newline == paragraph break
                    if (afterNewline) {
                      ParagraphBreak();
                      afterNewline = false;
                      break;
                    } else {
                      atoms.ControlSpace();
                      afterNewline = true;
                      continue;
                    }
                  case var _ when wordKind == WordKind.Whitespace && !char.IsSurrogate(textSection[0]):
                    //Collpase spaces
                    if (afterCommand || oneCharOnly) continue;
                    else atoms.ControlSpace();
                    break;
                  default: //Just ordinary text
                    if (oneCharOnly) {
                      var firstCodepointLength = char.IsHighSurrogate(textSection[0]) ? 2 : 1;
                      if (startAt + firstCodepointLength < endAt) { //Only re-read if current break span is more than 1 long
                        i--;
                        breakList[i] = new BreakAtInfo(breakList[i].breakAt + firstCodepointLength, breakList[i].wordKind);
                      }
                      atoms.Text(textSection.Slice(0, firstCodepointLength).ToString());
                    } else atoms.Text(textSection.ToString());
                    break;
                }
                afterCommand = false;
                break;
              case (true, { }):
                //Escaped text section but in inline/display math mode
                switch (textSection) {
                  case var _ when textSection.Is('$'):
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case var _ when textSection.Is('('):
                    return displayMath switch
                    {
                      true => "Cannot open inline math mode in display math mode",
                      false => "Cannot open inline math mode in inline math mode",
                      null => throw new InvalidCodePathException("displayMath is null. This switch should not be hit."),
                    };
                  case var _ when textSection.Is(')'):
                    switch (displayMath) {
                      case true:
                        return "Cannot close inline math mode in display math mode";
                      case false:
                        if (atoms.Math(mathLaTeX.ToString(), false, startAt, ref endAt).Error is string mathError)
                          return mathError;
                        mathLaTeX.Clear();
                        displayMath = null;
                        break;
                      case null:
                        throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                    }
                    break;
                  case var _ when textSection.Is('['):
                    return displayMath switch
                    {
                      true => "Cannot open display math mode in display math mode",
                      false => "Cannot open display math mode in inline math mode",
                      null => throw new InvalidCodePathException("displayMath is null. This switch should not be hit."),
                    };
                  case var _ when textSection.Is(']'):
                    switch (displayMath) {
                      case true:
                        if (atoms.Math(mathLaTeX.ToString(), true, startAt, ref endAt).Error is string mathError)
                          return mathError;
                        mathLaTeX.Clear();
                        displayMath = null;
                        break;
                      case false:
                        return "Cannot close display math mode in inline math mode";
                      default:
                        throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                    }
                    break;
                  default:
                    mathLaTeX.Append('\\').Append(textSection);
                    break;
                }
                backslashEscape = false;
                break;
              case (true, null):
                //Escaped text section and not in inline/display math mode
                afterCommand = true;
                switch (textSection.ToString()) {
                  case var _ when wordKind == WordKind.Whitespace: //control space
                    atoms.ControlSpace();
                    break;
                  case "(":
                    displayMath = false;
                    break;
                  case ")":
                    return "Cannot close inline math mode outside of math mode";
                  case "[":
                    displayMath = true;
                    break;
                  case "]":
                    return "Cannot close display math mode outside of math mode";
                  case "\\":
                    atoms.Break();
                    break;
                  case ",":
                    atoms.Space(Space.ShortSpace);
                    break;
                  case ":":
                  case ">":
                    atoms.Space(Space.MediumSpace);
                    break;
                  case ";":
                    atoms.Space(Space.LongSpace);
                    break;
                  case "!":
                    atoms.Space(-Space.ShortSpace);
                    break;
                  case "enspace":
                    atoms.Space(Space.EmWidth / 2);
                    break;
                  case "quad":
                    atoms.Space(Space.EmWidth);
                    break;
                  case "qquad":
                    atoms.Space(Space.EmWidth * 2);
                    break;
                  case "hspace": {
                      if (ReadArgumentString(latex, ref textSection).Bind(space => {
                        int lastNum = -1;
                        for (int j = 0; j < space.Length; j++) {
                          if ('0' <= space[j] && space[j] <= '9' || space[j] == '.') lastNum = j;
                        }
                        if (lastNum == -1) return Err("Space cannot be empty");
                        return Space.Create(space.Slice(0, lastNum + 1).ToString(), space.Slice(lastNum + 1).ToString(), true);
                      }).Bind(space => atoms.Space(space)).Error is string error)
                        return Err(error);
                      break;
                    }
                  case "par":
                    ParagraphBreak();
                    break;
                  case "fontsize": {
                      var (fontSize, error) =
                        ReadArgumentString(latex, ref textSection).Bind(fontSize => {
                          Span<byte> charBytes = stackalloc byte[fontSize.Length];
                          for (int j = 0; j < fontSize.Length; j++) {
                            if (fontSize[j] > 127) return Err("Invalid font size");
                            charBytes[j] = (byte)fontSize[j];
                          }
                          return System.Buffers.Text.Utf8Parser.TryParse(charBytes, out float parsedResult, out var consumed, 'f')
                            && fontSize.Slice(consumed).IsWhiteSpace()
                            ? Ok(parsedResult)
                            : Err("Invalid font size");
                        });
                      if (error != null) return error;
                      var (resizedContent, error2) = ReadArgumentAtom(latex);
                      if (error2 != null) return error2;
                      atoms.Size(resizedContent, fontSize);
                      break;
                    }
                  case "color": {
                      var (color, error) = ReadColor(latex, ref textSection);
                      if (error != null) return error;
                      var (coloredContent, error2) = ReadArgumentAtom(latex);
                      if (error2 != null) return error2;
                      atoms.Color(coloredContent, color);
                      break;
                    }
                  //case "red", "yellow", ...
                  case var shortColor when
                    LaTeXSettings.PredefinedColors.FirstToSecond.TryGetValue(shortColor, out var color): {
                      int tmp_commandLength = shortColor.Length;
                      if (ReadArgumentAtom(latex).Bind(
                          coloredContent => atoms.Color(coloredContent, color)
                        ).Error is string error
                      ) return error;
                      break;
                    }
                  //case "textbf", "textit", ...
                  case var textStyle when !textStyle.StartsWith("math")
                    && LaTeXSettings.FontStyles.FirstToSecond.TryGetValue(
                        textStyle.StartsWith("text") ? textStyle.Replace("text", "math") : textStyle,
                        out var fontStyle): {
                      int tmp_commandLength = textStyle.Length;
                      if (ReadArgumentAtom(latex)
                        .Bind(builtContent => atoms.Style(builtContent, fontStyle))
                        .Error is string error)
                        return error;
                      break;
                    }
                  //case "^", "\"", ...
                  case var textAccent when
                    TextLaTeXSettings.PredefinedAccents.FirstToSecond.TryGetValue(textAccent, out var accent): {
                      if (ReadArgumentAtom(latex)
                        .Bind(builtContent => atoms.Accent(builtContent, accent))
                        .Error is string error)
                        return error;
                      break;
                    }
                  //case "textasciicircum", "textless", ...
                  case var textSymbol when TextLaTeXSettings.PredefinedTextSymbols.FirstToSecond.TryGetValue(textSymbol, out var replaceResult):
                    atoms.Text(replaceResult);
                    break;
                  case var command:
                    if (displayMath != null) mathLaTeX.Append(command); //don't eat the command when parsing math
                    else return $@"Invalid command \{command}";
                    break;
                }
                backslashEscape = false;
                break;
            }
          }
          afterNewline = false;
          if (oneCharOnly) return Ok(i);
        }
        if (backslashEscape) return @"Invalid command \";
        if (stopChar > 0) return stopChar == '}' ? "Expected }, unbalanced braces" : $@"Expected {stopChar}";
        return Ok(i);
      }
      var error = BuildBreakList(latexSource.AsSpan(), globalAtoms, 0, false, '\0').Error;
      if (error != null) return LaTeXParser.HelpfulErrorMessage(error, latexSource, endAt);
      error = CheckDollarCount(latexSource.Length, ref endAt, globalAtoms).Error;
      if (error != null) return LaTeXParser.HelpfulErrorMessage(error, latexSource, endAt);
      if (displayMath != null) return LaTeXParser.HelpfulErrorMessage("Math mode was not terminated", latexSource, endAt);
      return globalAtoms.Build();
    }
    public static StringBuilder TextAtomToLaTeX(TextAtom atom, StringBuilder? b = null) {
      b ??= new StringBuilder();
      switch (atom) {
        case TextAtom.Text t:
          foreach (var ch in t.Content) {
            var c = ch.ToStringInvariant();
            if (TextLaTeXSettings.PredefinedTextSymbols.SecondToFirst.TryGetValue(c, out var v))
              if ('a' <= v[0] && v[0] <= 'z' || 'A' <= v[0] && v[0] <= 'Z')
                b.Append('\\').Append(v).Append(' ');
              else b.Append('\\').Append(v);
            else b.Append(c);
          }
          return b;
        case TextAtom.Newline _:
          return b.Append(@"\\");
        case TextAtom.Math m:
          b.Append('\\')
            .Append(m.DisplayStyle ? '[' : '(');
          return LaTeXParser.MathListToLaTeX(m.Content, b)
            .Append('\\')
            .Append(m.DisplayStyle ? ']' : ')');
        case TextAtom.Space s:
          return s.Content.IsMu
          ? s.Content.Length switch
          {
            -3 => b.Append(@"\! "),
            3 => b.Append(@"\, "),
            4 => b.Append(@"\: "),
            5 => b.Append(@"\; "),
            9 => b.Append(@"\enspace "),
            18 => b.Append(@"\quad "),
            36 => b.Append(@"\qquad "),
            var l => b.Append(@"\hspace{").Append((l / 18).ToStringInvariant("0.0####")).Append("em").Append('}')
          }
          : b.Append(@"\hspace{").Append(s.Content.Length.ToStringInvariant("0.0####")).Append("pt").Append('}');
        case TextAtom.ControlSpace _:
          return b.Append(@"\ ");
        case TextAtom.Accent a:
          b.Append('\\').Append(TextLaTeXSettings.PredefinedAccents.SecondToFirst[a.AccentChar]).Append('{');
          return TextAtomToLaTeX(a.Content, b).Append('}');
        case TextAtom.Style t:
          b.Append('\\')
            .Append(LaTeXSettings.FontStyles.SecondToFirst[t.FontStyle] is var style && style.StartsWith("math")
                    ? style.Replace("math", "text") : style)
            .Append('{');
          return TextAtomToLaTeX(t.Content, b).Append('}');
        case TextAtom.Size z:
          b.Append(@"\fontsize{").Append(z.PointSize).Append("}{");
          return TextAtomToLaTeX(z.Content, b).Append('}');
        case TextAtom.Colored c:
          b.Append(@"\color{");
          LaTeXSettings.ColorToString(c.Colour, b).Append("}{");
          return TextAtomToLaTeX(c.Content, b).Append('}');
        case TextAtom.List l:
          foreach (var a in l.Content)
            TextAtomToLaTeX(a, b);
          return b;
        case null:
          throw new ArgumentNullException(nameof(atom),
            "TextAtoms should never be null. You must have sneaked one in.");
        case var a:
          throw new InvalidCodePathException(
            "There should not be an unknown type of TextAtom." +
            $"However, one with type {a.GetType()} was encountered.");
      }
    }
  }
}