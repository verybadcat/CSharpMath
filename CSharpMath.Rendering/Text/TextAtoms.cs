using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public static class TextAtoms {
    public static AliasDictionary<string, string> PredefinedTextSymbols { get; } = new AliasDictionary<string, string> {
      //https://en.wikibooks.org/wiki/LaTeX/Special_Characters
      { "€", "euro" },
      { "°", "textdegree" },
      { "℃", "celsius" },

      //http://tug.ctan.org/info/symbols/comprehensive/symbols-a4.pdf page 14 (2 Body-text symbols)
      { "%", "%" },
      { "&", "&" },
      { "#", "#" },
      { "^", "textasciicircum" },
      { "<", "textless" },
      { "˜", "textasciitilde" },
      { "ª", "textordfeminine" },
      { "∗", "textasteriskcentered" },
      { "º", "textordmasculine" },
      { "∖", "textbackslash" },
      { "¶", "P", "textparagraph" },
      { "|", "textbar" },
      { "·", "textperiodcentered" },
      { "‖", "textbardbl" },
      { "‱", "textpertenthousand" },
      { "○", "textbigcircle" },
      { "‰", "textperthousand" },
      { "{", "{", "textbraceleft" },
      { "¿", "textquestiondown" },
      { "}", "}", "textbraceright" },
      { "“", "textquotedblleft" },
      { "•", "textbullet" },
      { "”", "textquotedblright" },
      { "©", "copyright", "textcopyright" },
      { "‘", "textquoteleft" },
      { "†", "dag", "textdagger" },
      { "’", "textquoteright" },
      { "‡", "ddag", "textdaggerdbl" },
      { "®", "textregistered" },
      { "$", "$", "textdollar" },
      { "§", "S", "textsection" },
      { "…", "textellipsis" },
      { "£", "pounds", "textsterling" },
      { "—", "textemdash" },
      { "™", "texttrademark" },
      { "–", "textendash" },
      { "_", "_", "textunderscore" },
      { "¡", "textexclamdown" },
      { "␣", "textvisiblespace" },
      { ">", "textgreater" },
    };
    public static BiDictionary<string, string> PredefinedAccents { get; } = new BiDictionary<string, string> {
      //textsuperscript, textsubscript
      //textcircled
      { "`" , "\u0300" }, //grave
                     { "'" , "\u0301" }, //acute
                     { "^" , "\u0302" },  //circumflex
                     { "\"" , "\u0308" }, //umlaut, trema or dieresis
                     { "~" , "\u0303" }, //tilde
                     { "." , "\u0307" }, //dot
                     { "t", "\u20E9" }, //tie
                                        /*{ "bar" , "\u0304" },
                                        { "overbar", "\u0305" }, //not in iosMath
                                        { "breve" , "\u0306" },
                                        { "ovhook", "\u0309" }, //not in iosMath
                                        { "ocirc", "\u030A" }, //not in iosMath
                                        { "check" , "\u030C" },
                                        { "leftharpoonaccent", "\u20D0" }, //not in iosMath
                                        { "rightharpoonaccent", "\u20D1" }, //not in iosMath
                                        { "vertoverlay", "\u20D2" }, //not in iosMath
                                        { "vec" , "\u20D7" },
                                        { "dddot", "\u20DB" }, //not in iosMath
                                        { "ddddot", "\u20DC" }, //not in iosMath
                                        { "asteraccent", "\u20F0" }, //not in iosMath
                                        { "threeunderdot", "\u20E8" } //not in iosMath*/
    };

  }
}
