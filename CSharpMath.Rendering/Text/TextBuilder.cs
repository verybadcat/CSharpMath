using System;
using System.Collections.Generic;
using System.Text;
using Typography.TextBreak;

namespace CSharpMath.Rendering {
  public static class TextBuilder {
    public static (TextAtom atom, string error) Build(string text) {
      string error = null;
      var breaker = new CustomBreaker();
      var breakList = new List<BreakAtInfo> { new BreakAtInfo(0, WordKind.Unknown) };
      breaker.BreakWords(text, false);
      breaker.LoadBreakAtList(breakList);
      /* //Paste this into the C# Interactive, fill <username> yourself
#r "C:/Users/<username>/source/repos/CSharpMath/Typography/Build/NetStandard/Typography.TextBreak/bin/Debug/netstandard1.3/Typography.TextBreak.dll"
using Typography.TextBreak;
const string text = "Here are some text $1 + 12 \\frac23 \\sqrt4$ $$Display$$ text";
var breaker = new CustomBreaker();
var breakList = new List<BreakAtInfo>();
breaker.BreakWords(text);
breaker.LoadBreakAtList(breakList);
//index is after the boundary -> last one will be out of range
breakList.Select(i => (i.breakAt, i.wordKind, text.ElementAtOrDefault(i.breakAt))).ToArray()
       */
      bool? displayMath = null;
      StringBuilder mathLaTeX = null;
      bool backslashEscape = false;
      int dollarCount = 0;
      var atoms = new TextAtomListBuilder();
      for (int i = 0; i < breakList.Count; i++) {
        var startAt = breakList[i].breakAt; 
        var endAt = i == breakList.Count - 1 ? text.Length : breakList[i + 1].breakAt;
        var endingChar = i == breakList.Count - 1 ? '\0' : text[endAt - 1];
        if(endingChar == '$') {
          if (backslashEscape) atoms.Add("$");
          else {
            dollarCount++;
            continue;
          }
        }
        switch (dollarCount) {
          case 0:
            break;
          case 1:
            dollarCount = 0;
            switch (displayMath) {
              case true:
                return (null, "Cannot close $ with $$");
              case false:
                var mathError = atoms.Add(mathLaTeX.ToString(), false);
                mathLaTeX = null;
                displayMath = null;
                if (mathError != null) return (null, mathError);
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
                atoms.Add(mathLaTeX.ToString(), true);
                mathLaTeX = null;
                displayMath = null;
                break;
              case false:
                return (null, "Cannot close $$ with $");
              case null:
                mathLaTeX = new StringBuilder();
                displayMath = true;
                break;
            }
            break;
          default:
            return (null, "Invalid number of $: " + dollarCount);
        }
        switch (endingChar) {
          case '$':
            break; //Already dealt with above
          case '\0':
            continue; //Don't do anything with the null char
          case '\\':
            if (displayMath != null) mathLaTeX.Append('\\'); //don't eat the backslash when parsing math
            else if (backslashEscape) {
              atoms.Add("\\");
            } else {
              backslashEscape = true;
              continue;
            }
            break;
          default:
            var textSection = text.Substring(startAt, endAt - startAt);
            if (displayMath == null) atoms.Add(textSection);
            else mathLaTeX.Append(textSection);
            break;
          }
        backslashEscape = false;
      }
      if (displayMath != null) return (null, "Math mode was not terminated");
      return (atoms.Build(), error);
    }
    public static StringBuilder Unbuild(TextAtom atom, StringBuilder b) {
      switch (atom) {
        case TextAtom.Text t:
          return b.Append(t.Content);
        case TextAtom.Math m:
          return b.Append(Atoms.MathListBuilder.MathListToString(m.Content));
        case TextAtom.List l:
          foreach (var a in l.Content) {
            b.Append(Unbuild(a, b));
          }
          return b;
        case var a:
          throw new TypeAccessException($"There should not be an unknown type of TextAtom. However, one with type {a.GetType()} was encountered.");
      }
    }
  }
}