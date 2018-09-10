using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Rendering {
  public static class TextAtoms {
    public static AliasDictionary<string, string> PredefinedTextSymbols { get; } = new AliasDictionary<string, string> {
      /*Ten special characters and their commands:
       & \&
       % \%
       $ \$
       # \#
       _ \_
       { \{
       } \}
       ~ \textasciitilde
       ^ \textasciicircum
       \ \textbackslash
      */
      //https://en.wikibooks.org/wiki/LaTeX/Special_Characters
      { "€", "euro" },
      { "°", "textdegree" },
      { "℃", "celsius" },

      { "ł", "l" }, //barred l (l with stroke)
      { "Ł", "L" }, //barred L (L with stroke)
      { "ø", "o" }, //slashed o (o with stroke)
      { "Ø", "O" }, //slashed O (O with stroke)
      { "ı", "i" }, //dotless i
      { "ȷ", "j" }, //dotless j

      //http://tug.ctan.org/info/symbols/comprehensive/symbols-a4.pdf page 14 (2 Body-text symbols)
      { "%", "%" },
      { "&", "&" },
      { "#", "#" },
      { "^", "textasciicircum" },
      { "<", "textless" },
      { "˜", "textasciitilde" }, //˜ is the accent
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
      { "`", "\u0300" }, //grave
      { "'", "\u0301" }, //acute
      { "^", "\u0302" }, //circumflex
      { "\"", "\u0308" }, //umlaut, trema or dieresis
      { "H", "\u030B" }, //long Hungarian umlaut (double acute)
      { "c", "\u00B8" }, //cedilla //\u0327 is not in Latin Modern Math, so forced to use \u00B8, but still no problems
      { "k", "\u02DB" }, //ogonek //\u0328 is not in Latin Modern Math, so forced to use \u02DB, but still no problems
      { "=", "\u0304" }, //macron accent (a bar over the letter)
      { "b", "\u0331" }, //macron accent below (a bar under the letter)
      { "~", "\u0303" }, //tilde
      { ".", "\u0307" }, //dot over the letter
      { "d", "\u0323" }, //dot under the letter
      { "r", "\u030A" }, //ring over the letter
      { "u", "\u0306" }, //breve over the letter
      { "v", "\u030C" }, //caron/háček ("v") over the letter
      { "t", "\u23DC" }, //"tie" (inverted u) over the two letters
                                        /*
 \`{a} \'{a} \^{a} \"{a} \H{a} \c{a} \k{a} \={a} \b{a} \~{a} \.{a} \d{a} \r{a} \u{a} \v{a} \t{a}
                                        { "overbar", "\u0305" }, //not in iosMath
                                        { "ovhook", "\u0309" }, //not in iosMath
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
