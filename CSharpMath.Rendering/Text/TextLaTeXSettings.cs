namespace CSharpMath.Rendering.Text {
  using CSharpMath.Structures;
  public static class TextLaTeXSettings {
    public static AliasBiDictionary<string, string> PredefinedTextSymbols { get; } =
      new AliasBiDictionary<string, string> {
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
        { "euro" , "€" },
        { "textdegree" , "°" },
        { "celsius" , "℃" },

        { "l" , "ł" }, //barred l (l with stroke)
        { "L" , "Ł" }, //barred L (L with stroke)
        { "o" , "ø" }, //slashed o (o with stroke)
        { "O" , "Ø" }, //slashed O (O with stroke)
        { "i" , "ı" }, //dotless i
        { "j" , "ȷ" }, //dotless j
        { "ae" , "æ" },
        { "AE" , "Æ" },
        { "oe" , "œ" },
        { "OE" , "Œ" },
        { "ss" , "ß" },
        { "aa" , "å" },
        { "AA" , "Å" },

        //https://rpi.edu/dept/arc/training/latex/LaTeX_symbols.pdf page 2 table 3
        { "SS" , "SS" },
        { "DH", "Ð" },
        { "dh", "ð" },
        { "DJ", "Đ" },
        { "dj", "đ" },
        { "NG", "Ŋ" },
        { "ng", "ŋ" },
        { "TH", "Þ" },
        { "th", "þ" },
      
        // Greek characters
        { "alpha", "\u03B1" },
        { "beta", "\u03B2" },
        { "gamma", "\u03B3" },
        { "delta", "\u03B4" },
        { "varepsilon", "\u03B5" },
        { "zeta", "\u03B6" },
        { "eta", "\u03B7" },
        { "theta", "\u03B8" },
        { "iota", "\u03B9" },
        { "kappa", "\u03BA" },
        { "lambda", "\u03BB" },
        { "mu", "\u03BC" },
        { "nu", "\u03BD" },
        { "xi", "\u03BE" },
        { "omicron", "\u03BF" },
        { "pi", "\u03C0" },
        { "rho", "\u03C1" },
        { "varsigma", "\u03C2" },
        { "sigma", "\u03C3" },
        { "tau", "\u03C4" },
        { "upsilon", "\u03C5" },
        { "varphi", "\u03C6" },
        { "chi", "\u03C7" },
        { "psi", "\u03C8" },
        { "omega", "\u03C9" },
        { "vartheta", "\u03D1" },
        { "phi", "\u03D5" },
        { "varpi", "\u03D6" },
        { "varkappa", "\u03F0" },
        { "varrho", "\u03F1" },
        { "epsilon", "\u03F5" },
        // Capital greek characters
        { "Gamma", "\u0393" },
        { "Delta", "\u0394" },
        { "Theta", "\u0398" },
        { "Lambda", "\u039B" },
        { "Xi", "\u039E" },
        { "Pi", "\u03A0" },
        { "Sigma", "\u03A3" },
        { "Upsilon", "\u03A5" },
        { "Phi", "\u03A6" },
        { "Psi", "\u03A8" },
        { "Omega", "\u03A9" },
        //http://tug.ctan.org/info/symbols/comprehensive/symbols-a4.pdf page 14 (2 Body-text symbols)
        { "%", "%" },
        { "&", "&" },
        { "#", "#" },
        { "textasciicircum", "^" },
        { "textless", "<" },
        { "textasciitilde", "˜" }, //˜ is the accent
        { "textordfeminine", "ª" },
        { "textasteriskcentered", "∗" },
        { "textordmasculine", "º" },
        { "backslash", "textbackslash", "\\" }, //originally was ∖
        { "P", "textparagraph", "¶" },
        { "textbar", "|" },
        { "textperiodcentered", "·" },
        { "textbardbl", "‖" },
        { "textpertenthousand", "‱" },
        { "textbigcircle", "○" },
        { "textperthousand", "‰" },
        { "{", "textbraceleft", "{" },
        { "textquestiondown", "¿" },
        { "}", "textbraceright", "}" },
        { "textquotedblleft", "“" },
        { "textbullet", "•" },
        { "textquotedblright", "”" },
        { "copyright", "textcopyright", "©" },
        { "textquoteleft", "‘" },
        { "dag", "textdagger", "†" },
        { "textquoteright", "’" },
        { "ddag", "textdaggerdbl", "‡" },
        { "textregistered", "®" },
        { "$", "textdollar", "$" },
        { "S", "textsection", "§" },
        { "textellipsis", "…" },
        { "pounds", "textsterling", "£" },
        { "textemdash", "—" },
        { "texttrademark", "™" },
        { "textendash", "–" },
        { "_", "textunderscore", "_" },
        { "textexclamdown", "¡" },
        { "textvisiblespace", "␣" },
        { "textgreater", ">" },
      };
    public static AliasBiDictionary<string, string> PredefinedAccents { get; } =
      new AliasBiDictionary<string, string> {
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
