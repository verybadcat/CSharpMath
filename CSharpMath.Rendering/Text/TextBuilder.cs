using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;

namespace CSharpMath.Rendering {
  using Enumerations;
  using Structures;
  using static Structures.Result;
  public static class TextBuilder {
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
    public static bool NoEnhancedColors { get; set; }
    public static Result<TextAtom> Build(string latex) {
      if (string.IsNullOrEmpty(latex)) return new TextAtom.List(Array.Empty<TextAtom>(), 0);
      bool? displayMath = null;
      StringBuilder mathLaTeX = null;
      bool backslashEscape = false;
      bool afterCommand = false; //ignore spaces after command
      bool afterNewline = false;
      int dollarCount = 0;
      var globalAtoms = new TextAtomListBuilder();
      var breaker = new CustomBreaker { BreakNumberAfterText = true, ThrowIfCharOutOfRange = false };
      var breakList = new List<BreakAtInfo>();
      breaker.BreakWords(latex);
      breaker.CopyBreakResults(breakList);
      Result CheckDollarCount(TextAtomListBuilder atoms) {
        switch (dollarCount) {
          case 0:
            break;
          case 1:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                return "Cannot close display math mode with $";
              case false:
                if (atoms.Add(mathLaTeX.ToString(), false).Error is string mathError)
                  return "[Math mode error] " + mathError;
                mathLaTeX = null;
                displayMath = null;
                break;
              case null:
                mathLaTeX = new StringBuilder();
                displayMath = false;
                break;
            }
            break;
          case 2:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                if (atoms.Add(mathLaTeX.ToString(), true).Error is string mathError)
                  return "[Math mode error] " + mathError;
                mathLaTeX = null;
                displayMath = null;
                break;
              case false:
                return "Cannot close inline math mode with $$";
              case null:
                mathLaTeX = new StringBuilder();
                displayMath = true;
                break;
            }
            break;
          default:
            return "Invalid number of $: " + dollarCount;
        }
        return Ok();
      }
      Result<int> BuildBreakList(TextAtomListBuilder atoms, int i, bool oneCharOnly, char stopChar) {
        void ParagraphBreak() {
          atoms.Break(3);
#warning Should the newline and space occupy the same range?
          atoms.TextLength -= 3;
          atoms.Add(Space.ParagraphIndent, 3);
        }
        for (; i < breakList.Count; i++) {
          (int startAt, int endAt, string textSection, WordKind wordKind) ObtainRange(int index) {
            var (start, end) = (index == 0 ? 0 : breakList[index - 1].breakAt, breakList[index].breakAt);
            return (start, end, latex.Substring(start, end - start), breakList[index].wordKind);
          }
          var (startAt, endAt, textSection, wordKind) = ObtainRange(i);
          bool SetNextRange() {
            bool success = ++i < breakList.Count;
            if (success) (startAt, endAt, textSection, wordKind) = ObtainRange(i);
            return success;
          }
          Result<TextAtom> ReadArgumentAtom() {
            backslashEscape = false;
            var argAtoms = new TextAtomListBuilder();
            if(BuildBreakList(argAtoms, ++i, true, '\0').Bind(index => i = index).Error is string error) return error;
            return argAtoms.Build();
          }
          Result<string> ReadArgumentString() {
            afterCommand = false;
            if (!SetNextRange()) return Err("Missing argument");
            if (textSection != "{") return Err("Missing {");
            int endingIndex = -1;
            //startAt + 1 to not start at the { we started at
            bool isEscape = false;
            for (int j = startAt + 1, bracketDepth = 0; j < latex.Length; j++) {
              if (latex[j] == '\\')
                isEscape = true;
              else if (latex[j] == '{' && !isEscape) bracketDepth++;
              else if (latex[j] == '}' && !isEscape)
                if (bracketDepth > 0) bracketDepth--;
                else { endingIndex = j; break; } else isEscape = false;
            }
            if (endingIndex == -1) return Err("Missing }");
            var resultText = latex.Substring(endAt, endingIndex - endAt);
            while (startAt < endingIndex)
              _ = SetNextRange(); //this never fails because the above check
            return Ok(resultText);
          }

          //Nothing should be before dollar sign checking -- dollar sign checking uses continue;
          atoms.TextLength = startAt;
          if (textSection == "$") {
            if (backslashEscape)
              if (displayMath != null) mathLaTeX.Append(@"\$");
              else atoms.Add("$");
            else {
              dollarCount++;
              continue;
            }
            backslashEscape = false;
          } else {
            { if (CheckDollarCount(atoms).Error is string error) return error; }
            if (stopChar > 0 && textSection[0] == stopChar) return Ok(i);
            if (!backslashEscape) {
            //Unescaped text section, inside display/inline math mode
              if(displayMath != null)
                switch (textSection) {
                  case "$":
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case "\\":
                    backslashEscape = true;
                    continue;
                  default:
                    mathLaTeX.Append(textSection);
                    break;
                }
              //Unescaped text section, not inside display/inline math mode
              else switch (textSection) {
                  case "$":
                    throw new InvalidCodePathException("The $ case should have been accounted for.");
                  case "\\":
                    backslashEscape = true;
                    continue;
                  case "#":
                    return "Unexpected command argument reference character # outside of new command definition (currently unsupported)";
                  case "^":
                  case "_":
                    return $"Unexpected script indicator {textSection} outside of math mode";
                  case "&":
                    return $"Unexpected alignment tab character & outside of table environments";
                  case "~":
                    atoms.Add();
                    break;
                  case "%":
                    var comment = new StringBuilder();
                    while (SetNextRange() && wordKind != WordKind.NewLine) comment.Append(textSection);
                    atoms.Comment(comment.ToString());
                    break;
                  case "{":
                    if(BuildBreakList(atoms, ++i, false, '}').Bind(index => i = index).Error is string error) return error;
                    break;
                  case "}":
                    return "Unexpected }, unbalanced braces";
                  case var _ when wordKind == WordKind.NewLine:
                    //Consume newlines after commands
                    //Double newline == paragraph break
                    if (afterNewline) {
                      ParagraphBreak();
                      afterNewline = false;
                      break;
                    } else {
                      atoms.Add();
                      afterNewline = true;
                      continue;
                    }
                  case var _ when wordKind == WordKind.Whitespace:
                    //Collpase spaces
                    if (afterCommand) continue;
                    else atoms.Add();
                    break;
                  case var punc when wordKind == WordKind.Punc && atoms.Last is TextAtom.Text t:
                    //Append punctuation to text
                    t.Append(textSection);
                    break;
                  default: //Just ordinary text
                    if (oneCharOnly) {
                      if (startAt + 1 < endAt) { //Only re-read if current break span is more than 1 long
                        i--;
                        breakList[i] = new BreakAtInfo(breakList[i].breakAt + 1, breakList[i].wordKind);
                      }
                      atoms.Add(textSection[0].ToString());
                    } else atoms.Add(textSection);
                    break;
                }
              afterCommand = false;
            }

            //Escaped text section but in inline/display math mode
            else if (displayMath != null) {
              switch (textSection) {
                case "$":
                  throw new InvalidCodePathException("The $ case should have been accounted for.");
                case "(":
                  switch (displayMath) {
                    case true:
                      return "Cannot open inline math mode in display math mode";
                    case false:
                      return "Cannot open inline math mode in inline math mode";
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                case ")":
                  switch (displayMath) {
                    case true:
                      return "Cannot close inline math mode in display math mode";
                    case false:
                      if (atoms.Add(mathLaTeX.ToString(), false).Error is string mathError)
                        return "[Math mode error] " + mathError;
                      mathLaTeX = null;
                      displayMath = null;
                      break;
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                  break;
                case "[":
                  switch (displayMath) {
                    case true:
                      return "Cannot open display math mode in display math mode";
                    case false:
                      return "Cannot open display math mode in inline math mode";
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                case "]":
                  switch (displayMath) {
                    case true:
                      if (atoms.Add(mathLaTeX.ToString(), true).Error is string mathError)
                        return "[Math mode error] " + mathError;
                      mathLaTeX = null;
                      displayMath = null;
                      break;
                    case false:
                      return "Cannot close display math mode in inline math mode";
                    default:
                      throw new InvalidCodePathException("displayMath is null. This switch should not be hit.");
                  }
                  break;
                default:
                  mathLaTeX.Append($@"\{textSection}");
                  break;
              }
              backslashEscape = false;
            } else {
              //Escaped text section and not in inline/display math mode
              afterCommand = true;
              switch (textSection) {
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
                case @"\":
                  atoms.Break(1);
                  break;
                case ",":
                  atoms.Add(Space.ShortSpace, 1);
                  break;
                case var _ when wordKind == WordKind.Whitespace: //control space
                  atoms.Add();
                  break;
                case "par":
                  ParagraphBreak();
                  break;
                case "fontsize": {
                    if (ReadArgumentString().Bind(fontSize =>
                        float.TryParse(fontSize, System.Globalization.NumberStyles.AllowDecimalPoint |
                                                 System.Globalization.NumberStyles.AllowLeadingWhite |
                                                 System.Globalization.NumberStyles.AllowTrailingWhite,
                                                 System.Globalization.CultureInfo.InvariantCulture,
                                                 out var parsedResult) ?
                        Ok(parsedResult) :
                        Err("Invalid font size")
                      ).Bind(
                        ReadArgumentAtom(),
                        (fontSize, resizedContent) =>
                          atoms.Add(resizedContent, fontSize, "fontsize".Length)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                case "color": {
                    if (ReadArgumentString().Bind(color =>
                        Color.Create(color, !NoEnhancedColors) is Color value ?
                        Ok(value) :
                        Err("Invalid color")
                      ).Bind(
                        ReadArgumentAtom(),
                        (color, coloredContent) =>
                          atoms.Add(coloredContent, color, "color".Length)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                //case "red", "yellow", ...
                case var shortColor when !NoEnhancedColors && Color.PredefinedColors.Contains(shortColor): {
                    if (Ok(Color.Create(shortColor, !NoEnhancedColors) ??
                          throw new InvalidCodePathException(
                            "This case's condition should have checked the validity of shortColor.")
                      ).Bind(
                        ReadArgumentAtom(),
                        (color, coloredContent) =>
                          atoms.Add(coloredContent, color, shortColor.Length)
                      ).Error is string error
                    ) return error;
                    break;
                  }
                //case "textbf", "textit", ...
                case var textStyle when !textStyle.Contains("math") && FontStyleExtensions.FontStyles.TryGetByFirst(textStyle.Replace("text", "math"), out var fontStyle): {
                    if (ReadArgumentAtom()
                      .Bind(builtContent => atoms.Add(builtContent, fontStyle, textStyle.Length))
                      .Error is string error)
                      return error;
                    break;
                  }
                //case "^", "\"", ...
                case var textAccent when TextAtoms.PredefinedAccents.TryGetByFirst(textAccent, out var accent): {
                    if (ReadArgumentAtom()
                      .Bind(builtContent => atoms.Add(builtContent, accent, textAccent.Length))
                      .Error is string error)
                      return error;
                    break;
                  }
                //case "textasciicircum", "textless", ...
                case var textSymbol when TextAtoms.PredefinedTextSymbols.TryGetValue(textSymbol, out var replaceResult):
                  atoms.Add(replaceResult);
                  break;
                case var command:
                  if (displayMath != null) mathLaTeX.Append(command); //don't eat the command when parsing math
                  else return @"Unknown command \" + command;
                  break;
              }
              backslashEscape = false;
            }
          }
          afterNewline = false;
          if (oneCharOnly) return Ok(i);
        }
        if (backslashEscape) return @"Unknown command \";
        if (stopChar > 0) return stopChar == '}' ? "Expected }, unbalanced braces" : $@"Expected {stopChar}";
        return Ok(i);
      }
      { if (BuildBreakList(globalAtoms, 0, false, '\0').Error is string error) return error; }
      { if (CheckDollarCount(globalAtoms).Error is string error) return error; }
      if (displayMath != null) return "Math mode was not terminated";
      return globalAtoms.Build();
    }
    public static StringBuilder Unbuild(TextAtom atom, StringBuilder b) {
      switch (atom) {
        case TextAtom.Text t:
          return b.Append(t.Content);
        case TextAtom.Newline n:
          return b.Append(@"\\");
        case TextAtom.Math m:
          return b.Append('\\').Append(m.DisplayStyle ? '[' : '(').Append(Atoms.MathListBuilder.MathListToString(m.Content)).Append('\\').Append(m.DisplayStyle ? ']' : ')');
        case TextAtom.Space s:
          return b.Append(@"\hspace").AppendInBraces(s.Content.Length.ToStringInvariant(), NullHandling.EmptyContent);
        case TextAtom.ControlSpace c:
          return b.Append(@"\ ");
        case TextAtom.Style t:
          return b.Append('\\').Append(t.FontStyle.FontName()).AppendInBraces(Unbuild(t.Content, new StringBuilder()).ToString(), NullHandling.None);
        case TextAtom.Size z:
          return b.Append(@"\fontsize").AppendInBraces(z.PointSize.ToStringInvariant(), NullHandling.EmptyContent)
                                       .AppendInBraces(Unbuild(z.Content, new StringBuilder()).ToString(), NullHandling.None);
        case TextAtom.List l:
          foreach (var a in l.Content) {
            b.Append(Unbuild(a, b));
          }
          return b;
        case null:
          throw new ArgumentNullException(nameof(atom), "TextAtoms should never be null. You must have sneaked one in.");
        case var a:
          throw new InvalidCodePathException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
      }
    }
  }
}