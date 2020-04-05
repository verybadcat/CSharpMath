using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;

namespace CSharpMath.Rendering.Text {
  using Atom;
  using CSharpMath.Structures;
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
    public const int StringArgumentLimit = 25;
    public static bool NoEnhancedColors { get; set; }
    public static Result<TextAtom> TextAtomFromLaTeX(string latexSource) {
      if (string.IsNullOrEmpty(latexSource))
        return new TextAtom.List(Array.Empty<TextAtom>());
      bool? displayMath = null;
      var mathLaTeX = new StringBuilder();
      bool backslashEscape = false;
      bool afterCommand = false; //ignore spaces after command
      bool afterNewline = false;
      int dollarCount = 0;
      var globalAtoms = new TextAtomListBuilder();
      var breakList = new List<BreakAtInfo>();
      var breaker = new CustomBreaker(v => breakList.Add(new BreakAtInfo(v.LatestBreakAt, v.LatestWordKind))) {
        BreakNumberAfterText = true,
        ThrowIfCharOutOfRange = false
      };
      breaker.BreakWords(latexSource);

      Result CheckDollarCount(int currentPosition, TextAtomListBuilder atoms) {
        switch (dollarCount) {
          case 0:
            break;
          case 1:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                return "Cannot close display math mode with $";
              case false:
                if (atoms.Math(mathLaTeX.ToString(), false).Error is string mathError)
                  return "[Math mode error] " + mathError;
                mathLaTeX.Clear();
                displayMath = null;
                break;
              case null:
                mathLaTeX.Clear();
                displayMath = false;
                break;
            }
            break;
          case 2:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                if (atoms.Math(mathLaTeX.ToString(), true).Error is string mathError)
                  return "[Math mode error] " + mathError;
                mathLaTeX.Clear();
                displayMath = null;
                break;
              case false:
                return "Cannot close inline math mode with $$";
              case null:
                mathLaTeX.Clear();
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
          ObtainSection(latex, i, out var startAt, out var endAt, out var textSection, out var wordKind);
          bool PreviousSection(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            bool success = i-- > 0;
            if (success) ObtainSection(latexInput, i, out startAt, out endAt, out section, out wordKind);
            return success;
          }
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
            if (!NextSection(latexInput, ref section)) return Err("Missing argument");
            if (section.IsNot('{')) return Err("Missing {");
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
          ReadOnlySpan<char> NextSectionUntilPunc(ReadOnlySpan<char> latexInput, ref ReadOnlySpan<char> section) {
            int start = endAt;
            ReadOnlySpan<char> specialChars = stackalloc[] { '#', '$', '%', '&', '\\', '^', '_', '{', '}', '~' };
            while (NextSection(latexInput, ref section))
              if (wordKind != WordKind.Punc || specialChars.IndexOf(section[0]) != -1) {
                //We have overlooked by one
                PreviousSection(latexInput, ref section);
                break;
              }
            return latexInput.Slice(start, endAt - start);
          }
          //Nothing should be before dollar sign checking -- dollar sign checking uses continue;
          atoms.TextLength = startAt;
          if (textSection.Is('$')) {
            if (backslashEscape)
              if (displayMath != null) mathLaTeX.Append(@"\$");
              else atoms.Text("$", NextSectionUntilPunc(latex, ref textSection));
            else {
              dollarCount++;
              continue;
            }
            backslashEscape = false;
          } else {
            { if (CheckDollarCount(startAt, atoms).Error is string error) return error; }
            if (!backslashEscape) {
              //Unescaped text section, inside display/inline math mode
              if (displayMath != null)
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
              //Unescaped text section, not inside display/inline math mode
              else switch (textSection) {
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
                  case var _ when wordKind == WordKind.NewLine:
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
                  case var _ when wordKind == WordKind.Whitespace:
                    //Collpase spaces
                    if (afterCommand) continue;
                    else atoms.ControlSpace();
                    break;
                  default: //Just ordinary text
                    if (oneCharOnly) {
                      if (startAt + 1 < endAt) { //Only re-read if current break span is more than 1 long
                        i--;
                        breakList[i] = new BreakAtInfo(breakList[i].breakAt + 1, breakList[i].wordKind);
                      }
                      //Need to allocate in the end :(
                      //Don't look ahead for punc; we are looking for one char only
                      atoms.Text(textSection[0].ToString(), default);
                    } else atoms.Text(textSection.ToString(), NextSectionUntilPunc(latex, ref textSection));
                    break;
                }
              afterCommand = false;
            }

            //Escaped text section but in inline/display math mode
            else if (displayMath != null) {
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
                      if (atoms.Math(mathLaTeX.ToString(), false).Error is string mathError)
                        return "[Math mode error] " + mathError;
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
                      if (atoms.Math(mathLaTeX.ToString(), true).Error is string mathError)
                        return "[Math mode error] " + mathError;
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
            } else {
              //Escaped text section and not in inline/display math mode
              afterCommand = true;
              switch (textSection.ToString()) {
                case var _ when wordKind == WordKind.Whitespace: //control space
                  atoms.ControlSpace();
                  break;
                case "(":
                  mathLaTeX = new StringBuilder();
                  displayMath = false;
                  break;
                case ")":
                  return "Cannot close inline math mode outside of math mode";
                case "[":
                  mathLaTeX = new StringBuilder();
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
                      if (space.Length > StringArgumentLimit)
                        return Err($"Length of space has over {StringArgumentLimit} characters. Please shorten it.");
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
                    if (ReadArgumentString(latex, ref textSection).Bind(fontSize => {
                      if (fontSize.Length > StringArgumentLimit)
                        return Err($"Length of font size has over {StringArgumentLimit} characters. Please shorten it.");
                      Span<byte> charBytes = stackalloc byte[fontSize.Length];
                      for (int j = 0; j < fontSize.Length; j++) {
                        if (fontSize[j] > 127) return Err("Invalid font size");
                        charBytes[j] = (byte)fontSize[j];
                      }
                      return System.Buffers.Text.Utf8Parser.TryParse(charBytes, out float parsedResult, out _, 'f') ?
                        Ok(parsedResult) :
                        Err("Invalid font size");
                    }).Bind(
                      ReadArgumentAtom(latex),
                      (fontSize, resizedContent) =>
                        atoms.Size(resizedContent, fontSize)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                case "color": {
                    if (ReadArgumentString(latex, ref textSection).Bind(color =>
                        color.Length > StringArgumentLimit ?
                          Err($"Length of color has over {StringArgumentLimit} characters. Please shorten it.") :
                        Color.Create(color, !NoEnhancedColors) is Color value ?
                          Ok(value) :
                        Err("Invalid color: " + color.ToString())
                      ).Bind(
                        ReadArgumentAtom(latex),
                        (color, coloredContent) =>
                          atoms.Color(coloredContent, color)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                //case "red", "yellow", ...
                case var shortColor when
                  !NoEnhancedColors && Color.PredefinedColors.TryGetByFirst(shortColor, out var color): {
                    int tmp_commandLength = shortColor.Length;
                    if (ReadArgumentAtom(latex).Bind(
                        coloredContent => atoms.Color(coloredContent, color)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                //case "textbf", "textit", ...
                case var textStyle when !textStyle.StartsWith("math")
                  && LaTeXSettings.FontStyles.TryGetValue(
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
                  TextLaTeXSettings.PredefinedAccents.TryGetByFirst(textAccent, out var accent): {
                    int tmp_commandLength = textAccent.Length;
                    if (ReadArgumentAtom(latex)
                      .Bind(builtContent => atoms.Accent(builtContent, accent))
                      .Error is string error)
                      return error;
                    break;
                  }
                //case "textasciicircum", "textless", ...
                case var textSymbol when TextLaTeXSettings.PredefinedTextSymbols.TryGetValue(textSymbol, out var replaceResult):
                  atoms.Text(replaceResult, NextSectionUntilPunc(latex, ref textSection));
                  break;
                case var command:
                  if (displayMath != null) mathLaTeX.Append(command); //don't eat the command when parsing math
                  else return $@"Invalid command \{command}";
                  break;
              }
              backslashEscape = false;
            }
          }
          afterNewline = false;
          if (oneCharOnly) return Ok(i);
        }
        if (backslashEscape) return @"Invalid command \";
        if (stopChar > 0) return stopChar == '}' ? "Expected }, unbalanced braces" : $@"Expected {stopChar}";
        return Ok(i);
      }
      {
        if (BuildBreakList(latexSource.AsSpan(), globalAtoms, 0, false, '\0').Error is string error)
          return error;
      }
      { if (CheckDollarCount(latexSource.Length, globalAtoms).Error is string error) return error; }
      if (displayMath != null) return "Math mode was not terminated";
      return globalAtoms.Build();
    }
    public static StringBuilder TextAtomToLaTeX(TextAtom atom, StringBuilder? b = null) {
      b ??= new StringBuilder();
      switch (atom) {
        case TextAtom.Text t:
          return b.Append(t.Content);
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
          b.Append('\\').Append(TextLaTeXSettings.PredefinedAccents[second: a.AccentChar]).Append('{');
          return TextAtomToLaTeX(a.Content, b).Append('}');
        case TextAtom.Style t:
          b.Append('\\')
            .Append(LaTeXSettings.FontStyles[t.FontStyle] is var style && style.StartsWith("math")
                    ? style.Replace("math", "text") : style)
            .Append('{');
          return TextAtomToLaTeX(t.Content, b).Append('}');
        case TextAtom.Size z:
          b.Append(@"\fontsize{").Append(z.PointSize).Append("}{");
          return TextAtomToLaTeX(z.Content, b).Append('}');
        case TextAtom.Color c:
          b.Append(@"\color{").Append(c.Colour).Append("}{");
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