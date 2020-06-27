namespace CSharpMath.CoreTests {
  using System;
  // This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
  // See license.txt or http://opensource.org/licenses/mit-license.php
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.Linq;
  using Xunit;
  public class TrieTests {

    // Based on https://github.com/gmamaladze/trienet/blob/f0961ebec078f65184d3bc85de8454919b335236/TrieNet.Test/PatriciaTrieTest.cs
    [Fact]
    public void TestNotExactMatched() {
      var trie = new Structures.PatriciaTrie<int>();
      trie.Add("aaabbb", 1);
      trie.Add("aaaccc", 2);

      var actual = trie["aab"];
      Assert.Empty(actual);
    }

    // Based on https://github.com/gmamaladze/trienet/blob/f0961ebec078f65184d3bc85de8454919b335236/TrieNet.Test/BaseTrieTest.cs

    static readonly string[] Words40 = new[] {
      "daubreelite",
      "daubingly",
      "daubingly",
      "phycochromaceous",
      "phycochromaceae",
      "phycite",
      "athymic",
      "athwarthawse",
      "athrotaxis",
      "unaccorded",
      "unaccordant",
      "unaccord",
      "kokoona",
      "koko",
      "koklas",
      "s",
      "flexibilty",
      "flexanimous",
      "collochemistry",
      "collochemistry",
      "collocationable",
      "capomo",
      "capoc",
      "capoc",
      "ungivingness",
      "ungiveable",
      "ungive",
      "prestandard",
      "prestandard",
      "prestabilism",
      "megalocornea",
      "megalocephalia",
      "megalocephalia",
      "afaced",
      "aettekees",
      "aetites",
      "comolecule",
      "comodato",
      "comodato",
      "cognoscibility"
    };
    static Structures.PatriciaTrie<int> CreateWords40Trie() {
      var trie = new Structures.PatriciaTrie<int>();
      for (int i = 0; i < Words40.Length; i++) {
        trie.Add(Words40[i], i);
      }
      return trie;
    }
    static Structures.PatriciaTrie<int> SharedTrie { get; } = CreateWords40Trie();


    [Theory]
    [InlineData("d", new[] { 0, 1, 2 })]
    [InlineData("da", new[] { 0, 1, 2 })]
    [InlineData("dau", new[] { 0, 1, 2 })]
    [InlineData("daub", new[] { 0, 1, 2 })]
    [InlineData("daubr", new[] { 0 })]
    [InlineData("daubre", new[] { 0 })]
    [InlineData("daubree", new[] { 0 })]
    [InlineData("daubreel", new[] { 0 })]
    [InlineData("daubreeli", new[] { 0 })]
    [InlineData("daubreelit", new[] { 0 })]
    [InlineData("daubreelite", new[] { 0 })]
    [InlineData("d", new[] { 0, 1, 2 })]
    [InlineData("da", new[] { 0, 1, 2 })]
    [InlineData("dau", new[] { 0, 1, 2 })]
    [InlineData("daub", new[] { 0, 1, 2 })]
    [InlineData("daubi", new[] { 1, 2 })]
    [InlineData("daubin", new[] { 1, 2 })]
    [InlineData("daubing", new[] { 1, 2 })]
    [InlineData("daubingl", new[] { 1, 2 })]
    [InlineData("daubingly", new[] { 1, 2 })]
    [InlineData("d", new[] { 0, 1, 2 })]
    [InlineData("da", new[] { 0, 1, 2 })]
    [InlineData("dau", new[] { 0, 1, 2 })]
    [InlineData("daub", new[] { 0, 1, 2 })]
    [InlineData("daubi", new[] { 1, 2 })]
    [InlineData("daubin", new[] { 1, 2 })]
    [InlineData("daubing", new[] { 1, 2 })]
    [InlineData("daubingl", new[] { 1, 2 })]
    [InlineData("daubingly", new[] { 1, 2 })]
    [InlineData("p", new[] { 3, 4, 5, 27, 28, 29 })]
    [InlineData("ph", new[] { 3, 4, 5 })]
    [InlineData("phy", new[] { 3, 4, 5 })]
    [InlineData("phyc", new[] { 3, 4, 5 })]
    [InlineData("phyco", new[] { 3, 4 })]
    [InlineData("phycoc", new[] { 3, 4 })]
    [InlineData("phycoch", new[] { 3, 4 })]
    [InlineData("phycochr", new[] { 3, 4 })]
    [InlineData("phycochro", new[] { 3, 4 })]
    [InlineData("phycochrom", new[] { 3, 4 })]
    [InlineData("phycochroma", new[] { 3, 4 })]
    [InlineData("phycochromac", new[] { 3, 4 })]
    [InlineData("phycochromace", new[] { 3, 4 })]
    [InlineData("phycochromaceo", new[] { 3 })]
    [InlineData("phycochromaceou", new[] { 3 })]
    [InlineData("phycochromaceous", new[] { 3 })]
    [InlineData("p", new[] { 3, 4, 5, 27, 28, 29 })]
    [InlineData("ph", new[] { 3, 4, 5 })]
    [InlineData("phy", new[] { 3, 4, 5 })]
    [InlineData("phyc", new[] { 3, 4, 5 })]
    [InlineData("phyco", new[] { 3, 4 })]
    [InlineData("phycoc", new[] { 3, 4 })]
    [InlineData("phycoch", new[] { 3, 4 })]
    [InlineData("phycochr", new[] { 3, 4 })]
    [InlineData("phycochro", new[] { 3, 4 })]
    [InlineData("phycochrom", new[] { 3, 4 })]
    [InlineData("phycochroma", new[] { 3, 4 })]
    [InlineData("phycochromac", new[] { 3, 4 })]
    [InlineData("phycochromace", new[] { 3, 4 })]
    [InlineData("phycochromacea", new[] { 4 })]
    [InlineData("phycochromaceae", new[] { 4 })]
    [InlineData("p", new[] { 3, 4, 5, 27, 28, 29 })]
    [InlineData("ph", new[] { 3, 4, 5 })]
    [InlineData("phy", new[] { 3, 4, 5 })]
    [InlineData("phyc", new[] { 3, 4, 5 })]
    [InlineData("phyci", new[] { 5 })]
    [InlineData("phycit", new[] { 5 })]
    [InlineData("phycite", new[] { 5 })]
    [InlineData("a", new[] { 6, 7, 8, 33, 34, 35 })]
    [InlineData("at", new[] { 6, 7, 8 })]
    [InlineData("ath", new[] { 6, 7, 8 })]
    [InlineData("athy", new[] { 6 })]
    [InlineData("athym", new[] { 6 })]
    [InlineData("athymi", new[] { 6 })]
    [InlineData("athymic", new[] { 6 })]
    [InlineData("a", new[] { 6, 7, 8, 33, 34, 35 })]
    [InlineData("at", new[] { 6, 7, 8 })]
    [InlineData("ath", new[] { 6, 7, 8 })]
    [InlineData("athw", new[] { 7 })]
    [InlineData("athwa", new[] { 7 })]
    [InlineData("athwar", new[] { 7 })]
    [InlineData("athwart", new[] { 7 })]
    [InlineData("athwarth", new[] { 7 })]
    [InlineData("athwartha", new[] { 7 })]
    [InlineData("athwarthaw", new[] { 7 })]
    [InlineData("athwarthaws", new[] { 7 })]
    [InlineData("athwarthawse", new[] { 7 })]
    [InlineData("a", new[] { 6, 7, 8, 33, 34, 35 })]
    [InlineData("at", new[] { 6, 7, 8 })]
    [InlineData("ath", new[] { 6, 7, 8 })]
    [InlineData("athr", new[] { 8 })]
    [InlineData("athro", new[] { 8 })]
    [InlineData("athrot", new[] { 8 })]
    [InlineData("athrota", new[] { 8 })]
    [InlineData("athrotax", new[] { 8 })]
    [InlineData("athrotaxi", new[] { 8 })]
    [InlineData("athrotaxis", new[] { 8 })]
    [InlineData("u", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("un", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("una", new[] { 9, 10, 11 })]
    [InlineData("unac", new[] { 9, 10, 11 })]
    [InlineData("unacc", new[] { 9, 10, 11 })]
    [InlineData("unacco", new[] { 9, 10, 11 })]
    [InlineData("unaccor", new[] { 9, 10, 11 })]
    [InlineData("unaccord", new[] { 9, 10, 11 })]
    [InlineData("unaccorde", new[] { 9 })]
    [InlineData("unaccorded", new[] { 9 })]
    [InlineData("u", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("un", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("una", new[] { 9, 10, 11 })]
    [InlineData("unac", new[] { 9, 10, 11 })]
    [InlineData("unacc", new[] { 9, 10, 11 })]
    [InlineData("unacco", new[] { 9, 10, 11 })]
    [InlineData("unaccor", new[] { 9, 10, 11 })]
    [InlineData("unaccord", new[] { 9, 10, 11 })]
    [InlineData("unaccorda", new[] { 10 })]
    [InlineData("unaccordan", new[] { 10 })]
    [InlineData("unaccordant", new[] { 10 })]
    [InlineData("u", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("un", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("una", new[] { 9, 10, 11 })]
    [InlineData("unac", new[] { 9, 10, 11 })]
    [InlineData("unacc", new[] { 9, 10, 11 })]
    [InlineData("unacco", new[] { 9, 10, 11 })]
    [InlineData("unaccor", new[] { 9, 10, 11 })]
    [InlineData("unaccord", new[] { 9, 10, 11 })]
    [InlineData("k", new[] { 12, 13, 14 })]
    [InlineData("ko", new[] { 12, 13, 14 })]
    [InlineData("kok", new[] { 12, 13, 14 })]
    [InlineData("koko", new[] { 12, 13 })]
    [InlineData("kokoo", new[] { 12 })]
    [InlineData("kokoon", new[] { 12 })]
    [InlineData("kokoona", new[] { 12 })]
    [InlineData("k", new[] { 12, 13, 14 })]
    [InlineData("ko", new[] { 12, 13, 14 })]
    [InlineData("kok", new[] { 12, 13, 14 })]
    [InlineData("koko", new[] { 12, 13 })]
    [InlineData("k", new[] { 12, 13, 14 })]
    [InlineData("ko", new[] { 12, 13, 14 })]
    [InlineData("kok", new[] { 12, 13, 14 })]
    [InlineData("kokl", new[] { 14 })]
    [InlineData("kokla", new[] { 14 })]
    [InlineData("koklas", new[] { 14 })]
    [InlineData("s", new[] { 15 })]
    [InlineData("f", new[] { 16, 17 })]
    [InlineData("fl", new[] { 16, 17 })]
    [InlineData("fle", new[] { 16, 17 })]
    [InlineData("flex", new[] { 16, 17 })]
    [InlineData("flexi", new[] { 16 })]
    [InlineData("flexib", new[] { 16 })]
    [InlineData("flexibi", new[] { 16 })]
    [InlineData("flexibil", new[] { 16 })]
    [InlineData("flexibilt", new[] { 16 })]
    [InlineData("flexibilty", new[] { 16 })]
    [InlineData("f", new[] { 16, 17 })]
    [InlineData("fl", new[] { 16, 17 })]
    [InlineData("fle", new[] { 16, 17 })]
    [InlineData("flex", new[] { 16, 17 })]
    [InlineData("flexa", new[] { 17 })]
    [InlineData("flexan", new[] { 17 })]
    [InlineData("flexani", new[] { 17 })]
    [InlineData("flexanim", new[] { 17 })]
    [InlineData("flexanimo", new[] { 17 })]
    [InlineData("flexanimou", new[] { 17 })]
    [InlineData("flexanimous", new[] { 17 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("col", new[] { 18, 19, 20 })]
    [InlineData("coll", new[] { 18, 19, 20 })]
    [InlineData("collo", new[] { 18, 19, 20 })]
    [InlineData("colloc", new[] { 18, 19, 20 })]
    [InlineData("colloch", new[] { 18, 19 })]
    [InlineData("colloche", new[] { 18, 19 })]
    [InlineData("collochem", new[] { 18, 19 })]
    [InlineData("collochemi", new[] { 18, 19 })]
    [InlineData("collochemis", new[] { 18, 19 })]
    [InlineData("collochemist", new[] { 18, 19 })]
    [InlineData("collochemistr", new[] { 18, 19 })]
    [InlineData("collochemistry", new[] { 18, 19 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("col", new[] { 18, 19, 20 })]
    [InlineData("coll", new[] { 18, 19, 20 })]
    [InlineData("collo", new[] { 18, 19, 20 })]
    [InlineData("colloc", new[] { 18, 19, 20 })]
    [InlineData("colloch", new[] { 18, 19 })]
    [InlineData("colloche", new[] { 18, 19 })]
    [InlineData("collochem", new[] { 18, 19 })]
    [InlineData("collochemi", new[] { 18, 19 })]
    [InlineData("collochemis", new[] { 18, 19 })]
    [InlineData("collochemist", new[] { 18, 19 })]
    [InlineData("collochemistr", new[] { 18, 19 })]
    [InlineData("collochemistry", new[] { 18, 19 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("col", new[] { 18, 19, 20 })]
    [InlineData("coll", new[] { 18, 19, 20 })]
    [InlineData("collo", new[] { 18, 19, 20 })]
    [InlineData("colloc", new[] { 18, 19, 20 })]
    [InlineData("colloca", new[] { 20 })]
    [InlineData("collocat", new[] { 20 })]
    [InlineData("collocati", new[] { 20 })]
    [InlineData("collocatio", new[] { 20 })]
    [InlineData("collocation", new[] { 20 })]
    [InlineData("collocationa", new[] { 20 })]
    [InlineData("collocationab", new[] { 20 })]
    [InlineData("collocationabl", new[] { 20 })]
    [InlineData("collocationable", new[] { 20 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("ca", new[] { 21, 22, 23 })]
    [InlineData("cap", new[] { 21, 22, 23 })]
    [InlineData("capo", new[] { 21, 22, 23 })]
    [InlineData("capom", new[] { 21 })]
    [InlineData("capomo", new[] { 21 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("ca", new[] { 21, 22, 23 })]
    [InlineData("cap", new[] { 21, 22, 23 })]
    [InlineData("capo", new[] { 21, 22, 23 })]
    [InlineData("capoc", new[] { 22, 23 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("ca", new[] { 21, 22, 23 })]
    [InlineData("cap", new[] { 21, 22, 23 })]
    [InlineData("capo", new[] { 21, 22, 23 })]
    [InlineData("capoc", new[] { 22, 23 })]
    [InlineData("u", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("un", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("ung", new[] { 24, 25, 26 })]
    [InlineData("ungi", new[] { 24, 25, 26 })]
    [InlineData("ungiv", new[] { 24, 25, 26 })]
    [InlineData("ungivi", new[] { 24 })]
    [InlineData("ungivin", new[] { 24 })]
    [InlineData("ungiving", new[] { 24 })]
    [InlineData("ungivingn", new[] { 24 })]
    [InlineData("ungivingne", new[] { 24 })]
    [InlineData("ungivingnes", new[] { 24 })]
    [InlineData("ungivingness", new[] { 24 })]
    [InlineData("u", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("un", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("ung", new[] { 24, 25, 26 })]
    [InlineData("ungi", new[] { 24, 25, 26 })]
    [InlineData("ungiv", new[] { 24, 25, 26 })]
    [InlineData("ungive", new[] { 25, 26 })]
    [InlineData("ungivea", new[] { 25 })]
    [InlineData("ungiveab", new[] { 25 })]
    [InlineData("ungiveabl", new[] { 25 })]
    [InlineData("ungiveable", new[] { 25 })]
    [InlineData("u", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("un", new[] { 9, 10, 11, 24, 25, 26 })]
    [InlineData("ung", new[] { 24, 25, 26 })]
    [InlineData("ungi", new[] { 24, 25, 26 })]
    [InlineData("ungiv", new[] { 24, 25, 26 })]
    [InlineData("ungive", new[] { 25, 26 })]
    [InlineData("p", new[] { 3, 4, 5, 27, 28, 29 })]
    [InlineData("pr", new[] { 27, 28, 29 })]
    [InlineData("pre", new[] { 27, 28, 29 })]
    [InlineData("pres", new[] { 27, 28, 29 })]
    [InlineData("prest", new[] { 27, 28, 29 })]
    [InlineData("presta", new[] { 27, 28, 29 })]
    [InlineData("prestan", new[] { 27, 28 })]
    [InlineData("prestand", new[] { 27, 28 })]
    [InlineData("prestanda", new[] { 27, 28 })]
    [InlineData("prestandar", new[] { 27, 28 })]
    [InlineData("prestandard", new[] { 27, 28 })]
    [InlineData("p", new[] { 3, 4, 5, 27, 28, 29 })]
    [InlineData("pr", new[] { 27, 28, 29 })]
    [InlineData("pre", new[] { 27, 28, 29 })]
    [InlineData("pres", new[] { 27, 28, 29 })]
    [InlineData("prest", new[] { 27, 28, 29 })]
    [InlineData("presta", new[] { 27, 28, 29 })]
    [InlineData("prestan", new[] { 27, 28 })]
    [InlineData("prestand", new[] { 27, 28 })]
    [InlineData("prestanda", new[] { 27, 28 })]
    [InlineData("prestandar", new[] { 27, 28 })]
    [InlineData("prestandard", new[] { 27, 28 })]
    [InlineData("p", new[] { 3, 4, 5, 27, 28, 29 })]
    [InlineData("pr", new[] { 27, 28, 29 })]
    [InlineData("pre", new[] { 27, 28, 29 })]
    [InlineData("pres", new[] { 27, 28, 29 })]
    [InlineData("prest", new[] { 27, 28, 29 })]
    [InlineData("presta", new[] { 27, 28, 29 })]
    [InlineData("prestab", new[] { 29 })]
    [InlineData("prestabi", new[] { 29 })]
    [InlineData("prestabil", new[] { 29 })]
    [InlineData("prestabili", new[] { 29 })]
    [InlineData("prestabilis", new[] { 29 })]
    [InlineData("prestabilism", new[] { 29 })]
    [InlineData("m", new[] { 30, 31, 32 })]
    [InlineData("me", new[] { 30, 31, 32 })]
    [InlineData("meg", new[] { 30, 31, 32 })]
    [InlineData("mega", new[] { 30, 31, 32 })]
    [InlineData("megal", new[] { 30, 31, 32 })]
    [InlineData("megalo", new[] { 30, 31, 32 })]
    [InlineData("megaloc", new[] { 30, 31, 32 })]
    [InlineData("megaloco", new[] { 30 })]
    [InlineData("megalocor", new[] { 30 })]
    [InlineData("megalocorn", new[] { 30 })]
    [InlineData("megalocorne", new[] { 30 })]
    [InlineData("megalocornea", new[] { 30 })]
    [InlineData("m", new[] { 30, 31, 32 })]
    [InlineData("me", new[] { 30, 31, 32 })]
    [InlineData("meg", new[] { 30, 31, 32 })]
    [InlineData("mega", new[] { 30, 31, 32 })]
    [InlineData("megal", new[] { 30, 31, 32 })]
    [InlineData("megalo", new[] { 30, 31, 32 })]
    [InlineData("megaloc", new[] { 30, 31, 32 })]
    [InlineData("megaloce", new[] { 31, 32 })]
    [InlineData("megalocep", new[] { 31, 32 })]
    [InlineData("megaloceph", new[] { 31, 32 })]
    [InlineData("megalocepha", new[] { 31, 32 })]
    [InlineData("megalocephal", new[] { 31, 32 })]
    [InlineData("megalocephali", new[] { 31, 32 })]
    [InlineData("megalocephalia", new[] { 31, 32 })]
    [InlineData("m", new[] { 30, 31, 32 })]
    [InlineData("me", new[] { 30, 31, 32 })]
    [InlineData("meg", new[] { 30, 31, 32 })]
    [InlineData("mega", new[] { 30, 31, 32 })]
    [InlineData("megal", new[] { 30, 31, 32 })]
    [InlineData("megalo", new[] { 30, 31, 32 })]
    [InlineData("megaloc", new[] { 30, 31, 32 })]
    [InlineData("megaloce", new[] { 31, 32 })]
    [InlineData("megalocep", new[] { 31, 32 })]
    [InlineData("megaloceph", new[] { 31, 32 })]
    [InlineData("megalocepha", new[] { 31, 32 })]
    [InlineData("megalocephal", new[] { 31, 32 })]
    [InlineData("megalocephali", new[] { 31, 32 })]
    [InlineData("megalocephalia", new[] { 31, 32 })]
    [InlineData("a", new[] { 6, 7, 8, 33, 34, 35 })]
    [InlineData("af", new[] { 33 })]
    [InlineData("afa", new[] { 33 })]
    [InlineData("afac", new[] { 33 })]
    [InlineData("aface", new[] { 33 })]
    [InlineData("afaced", new[] { 33 })]
    [InlineData("a", new[] { 6, 7, 8, 33, 34, 35 })]
    [InlineData("ae", new[] { 34, 35 })]
    [InlineData("aet", new[] { 34, 35 })]
    [InlineData("aett", new[] { 34 })]
    [InlineData("aette", new[] { 34 })]
    [InlineData("aettek", new[] { 34 })]
    [InlineData("aetteke", new[] { 34 })]
    [InlineData("aettekee", new[] { 34 })]
    [InlineData("aettekees", new[] { 34 })]
    [InlineData("a", new[] { 6, 7, 8, 33, 34, 35 })]
    [InlineData("ae", new[] { 34, 35 })]
    [InlineData("aet", new[] { 34, 35 })]
    [InlineData("aeti", new[] { 35 })]
    [InlineData("aetit", new[] { 35 })]
    [InlineData("aetite", new[] { 35 })]
    [InlineData("aetites", new[] { 35 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("com", new[] { 36, 37, 38 })]
    [InlineData("como", new[] { 36, 37, 38 })]
    [InlineData("comol", new[] { 36 })]
    [InlineData("comole", new[] { 36 })]
    [InlineData("comolec", new[] { 36 })]
    [InlineData("comolecu", new[] { 36 })]
    [InlineData("comolecul", new[] { 36 })]
    [InlineData("comolecule", new[] { 36 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("com", new[] { 36, 37, 38 })]
    [InlineData("como", new[] { 36, 37, 38 })]
    [InlineData("comod", new[] { 37, 38 })]
    [InlineData("comoda", new[] { 37, 38 })]
    [InlineData("comodat", new[] { 37, 38 })]
    [InlineData("comodato", new[] { 37, 38 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("com", new[] { 36, 37, 38 })]
    [InlineData("como", new[] { 36, 37, 38 })]
    [InlineData("comod", new[] { 37, 38 })]
    [InlineData("comoda", new[] { 37, 38 })]
    [InlineData("comodat", new[] { 37, 38 })]
    [InlineData("comodato", new[] { 37, 38 })]
    [InlineData("c", new[] { 18, 19, 20, 21, 22, 23, 36, 37, 38, 39 })]
    [InlineData("co", new[] { 18, 19, 20, 36, 37, 38, 39 })]
    [InlineData("cog", new[] { 39 })]
    [InlineData("cogn", new[] { 39 })]
    [InlineData("cogno", new[] { 39 })]
    [InlineData("cognos", new[] { 39 })]
    [InlineData("cognosc", new[] { 39 })]
    [InlineData("cognosci", new[] { 39 })]
    [InlineData("cognoscib", new[] { 39 })]
    [InlineData("cognoscibi", new[] { 39 })]
    [InlineData("cognoscibil", new[] { 39 })]
    [InlineData("cognoscibili", new[] { 39 })]
    [InlineData("cognoscibilit", new[] { 39 })]
    [InlineData("cognoscibility", new[] { 39 })]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "xUnit1025:InlineData should be unique within the Theory it belongs to",
      Justification = "These test cases are extracted from the original source (TrieNet). They should not be modified.")]
    public void Test(string query, int[] expected) {
      static void AssertSetEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
        Assert.Equal(expected.ToHashSet(), actual.ToHashSet());
      IEnumerable<int> Words40IndicesOf(string word) =>
        Words40.SelectMany((w, i) => w == word ? new[] { i } : Array.Empty<int>());

      void TestRetrieve() {
        IEnumerable<int> actual = SharedTrie[query];
        AssertSetEqual(expected, actual);
      }
      TestRetrieve();

      void TestRemove() {
        var trie = CreateWords40Trie();
        var success = trie.Remove(query);
        Assert.Equal(Words40.Contains(query), success);
        AssertSetEqual(expected.Except(Words40IndicesOf(query)), trie[query]);
      }
      TestRemove();

      void TestRetrieveFromRemoved(Structures.PatriciaTrie<int> removed, IEnumerable<int> removedIndices) {
        IEnumerable<int> actual = removed[query];
        AssertSetEqual(expected.Except(removedIndices), actual);
      }
      foreach (var word in Words40) {
        var trie = CreateWords40Trie();
        Assert.True(trie.Remove(word));
        TestRetrieveFromRemoved(trie, Words40IndicesOf(word));
      }
    }

    [Fact]
    public void TimeAdd() {
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      var trie = new Structures.PatriciaTrie<int>();
      foreach (var phrase in Words40) {
        trie.Add(phrase, phrase.GetHashCode());
      }

      stopwatch.Stop();
      Console.WriteLine(nameof(TimeAdd) + ": " + stopwatch.Elapsed);
      Assert.InRange(stopwatch.Elapsed, TimeSpan.Zero, TimeSpan.FromMilliseconds(0.2));
    }
    [Fact]
    public void TimeAddLongWords() {
      var stopwatch = new Stopwatch();
      stopwatch.Start();

      var trie = new Structures.PatriciaTrie<int>();
      foreach (var phrase in LongPhrases40) {
        trie.Add(phrase, phrase.GetHashCode());
      }

      stopwatch.Stop();
      Console.WriteLine(nameof(TimeAddLongWords) + ": " + stopwatch.Elapsed);
      Assert.InRange(stopwatch.Elapsed, TimeSpan.Zero, TimeSpan.FromMilliseconds(0.4));
    }

    public static string[] LongPhrases40 = new[] {
      "enterfeatnanocephalousssanapaiteadullamitesorchidologiessphenomandibularunremandedmeechersevererszamaforegamelaundsconelikesphalmaphylloptosiselvishvyproverbiologistdouanesdispensationalismapotypetearsheetsmesodisilicicnoncorruptnessheliotroperjinnywinkunrecurrent",
      "scourwayproimmunityunstonetutoriatepreauditorydeaconaldishwipinginstillatorpardaosdespondentnessbourreaunonponderositysubconicquiniblemowtnonrecitationticchenreburnishnonexecutionsforfaulterflaughteringmausolespermysallokinesisescribientesbauckiehomopauseoverinfluence",
      "noncontinuableprostascokemantelautomaticallydarnixsnowslipssubgoalcaddopostencephalondilutelyunrulablekilanalmacenistaangustateembryoctonychunneredlingismsoverdignifiedsparsonsitesubaqueansupercatholicallysprecounselmicrodistillationdisrestorecondensednesspredirectpinipicrin",
      "unpredaciousnesschiarooscurossalfinrectigradeliverberryimpallupanayanodiflorousreadjourningrillettesaxeassoilzieingfruitwomanendonuclearunadventurousnesserinlacworksbandaiteparchableryukyuannondeformedanomitevolvocaceousreawaretiburtineunviolinedcuselitesawman",
      "octavianporencephaliamirabilisesagrypnodefichtelitestylionindusiformacanthonhandspansunglospectrobolometerminisedananimadversivenesstrilliaceousoveytogetherhoodschoolgirlismnonmelodramaticallyssidiondawneredsubsphericpleuropedaloverlockingsamoritedendroclimatologiessyinstantiliquoracataposis",
      "overdelicatenesskischensizeablenessseptomaxillarychumpishbelanderfallageantipragmaticismagranulocyticmechanalnoneasternepimyocardialbegoredretreatingnessfourquinecavilingnessradiestheticpolymixiidmelanotekitecacurunweepinghechtsactinostlimburgitesnonsubmergibilitysawbwaadddaunattractablewitherweightbacteriopurpurin",
      "concatervateinogenesissgyrographembiotocidaeinguinodyniasfemorisrewishnonballotingoxidoreductionradiomuscularnoncurtailingsentogenousunrefundingrotavatorsuneathsflexibiltyunconsiderableszinkificationsnorseleruninstructiblesecchjuckholluschickslabellate",
      "banintraligamentousargononunresumptivefinancistcentrarchidfumisteryserragemonseignevrunfoolishnessmahajunleatherfishesunfattenunsoothingsethylthioethanesuperrespectablesconsolanparcimoniesadenousblinterpedagogerybinoosphradiumconformatorsanisylflambageacquaintant",
      "voicebandhuamuchilarticulabilitygiornatatelightlyingdealkylatepostbulbarosteotrophydiscommissioningcalombapoyntingvectionmacrosomiaimpolarilymetapoliticbiaswisebeedgedparafloccularitcheoglansolepieceladylintywhiteuncoherentlycoprophilismpiaclekarachivoglitespeculativismupbboreinterequinoctialbubbleless",
      "sesoenteritismormaorshipmislyunpromiscuouslybescribblinghypopetalypennyfeenonobscurityhayliftpietosoessentializationflappetapparailsanticoagulatorprenominicaldrymouthspolymetamericmonariobelonosphaeritedwaiblesnonconsumptivelypapaiabeforenesscorkmakermacrosplanchnicundiagrammaticallymaxostoma",
      "lasiocampidprunetincoventrybrigandishsuperacidityinterlucatenilometerveilednesslivishlyimprecatorilyrustfulmucroniformavelongeassociatorscleisteschylocystperithelialcapellaneprisiadkahypothetistgonotocontoariotomyssamidoazoattemperatorphthirusvellenagetriticalnessesthylose",
      "unexcoriatedunimpressionabilityradiotropicbaronizedunfalcatedunretractedbayheadcoracocostaldovefloweroverbravenotopteridquadrinomicalsubdolouslyexhalatesceleratesperiareumcondensedlyoverpuissantlyhumanitymongermanucodemontrossaprilsilicoferruginousnpfxcaumstonetrebletreehyenanchinextraregularlybecommasemipathologically",
      "sherryvalliesnonmonarchallysovereasinessmesocoelesubgenitalsouserworsementchondrectomydestinismavidyalysosomallyuncircumlocutoryboardysugescentpimolasciosophistarretezconscientisationleatmensanthroposociologistphyllobranchiatelonelihoodinteressortortilclintonchuradaunsatiabilityantitemperance",
      "palimpsetsubmontagnebicornutemucoflocculentsallactiteparagonimiasisdreckiersubtotemunnormalnesssupersensitisercorruptednessluskschangarinemendableanthropoclimatologycobaltocyanicssacalinenondeficientcarcasslessfrustulumboliviansprepsychotictidelessnessrhyotaxiticundermuslinscurtaxepanlogist",
      "precultureploughjoggersbodilizephysiologueerethizontidaehistoplasminlanchowsstatutumamphophilnondeprivationelectromotionspanpolismretrorenalpentadecagonmuscosenessarmoraciahippocastanaceousrecessorndebelelayshipmagnetolysisunhandselledfraudlessnessreevasionllerphytochemicalsbefan",
      "caddiingunludicroussdiscanderingfindyssheemraadnonglucosidalbuggesssscotographyinfoldmentunpredictivelymullerianphoenicochroitearcanenessestoxiinfectiousayacahuitecesserscadastrationleucocytolysesperistrumoustextiferousunbemoanedcolloxylindevauntpergelisolcounterinteresthala",
      "odoriphoreunfacetiousnessstauropegiadermatolysissbodypaintsalligatoridaeuntimorouslyjimsonpaintproofeylupwrapsunresignedlyprealludeunvisiblybulbotubermultititularunverbosenessgastrolatrouseelwrackstemplarlikenesssmysophiliaurushicqurangrasswidowhoodcyanuricheathenriess",
      "weeshnonmischievousnesscitronciruscungeboiorthopyramidsourjackunsortcompanionizingesthesiometricinshiningchrysopeeanacrogynaeundestroyableproletarizationnunciustephromyeliticmevingpresubstitutiondecipiumunmachineableapomecometrysacraryprecanonicaltuboovarianbemonstersreedeoophoromalaciaselectrographitewaltrot",
      "spacinessesfarmholdpyrocollodiontalterlamentationalattendresscakersleyinginterirrigationtransdermicaddlementsunshameablydihydrogensclairsentientdesonationunpiouslypteropogonscalfhoodduodramaswoolulosekenogeneticavailmentendotropictrymsfeebleheartedbounceablys",
      "dispendiouslypostfeministsunicursalityflaggellaparavaginitistroussheteroxenousawardmentequangularmycoplasmatacaearomanipugliaflavorsomenesshemiteriaungraphicboltelnonextensionautocombustiblerhizomatictruismaticsketchabilitiestrilinoleateindazolecanotierscombercaryopterisesflattyfluoratesinecureshipcolocasia",
      "cubiconetechnopsychologyprewarrantspostcommissureimpersonatrixsunoriginativewhafaboutfetoplacentaltridecenetransmigrationistslimnographmescalismsitalianoctachronousimproficiencypreeffectmyotrophyvaleraldehydesapskullsubjectileoligoprotheticdollfacebedotehydroscopistexpressorstowsenonswearermisregulating",
      "palpigerousarchimperialistgangesbitterlessfrankfortsaikuchisemivitalquinquiliteralodorometerbandboxicaloverquicklybedinmargarateacetlaunderopinionmassednessunfrettyruntgenizingcyanophycinshadelessnessplaymongerstyrolstrophanhinlipopexiainterclericalfordablenessmakeshiftinesssfootpaddery",
      "blatherydisunifysnonforeignesspurpartsulphocarbamideoutferrethardockcoevolvedcoevolvesnondemonstrativenessnonreparationpetrolizedunvitrescentunneareduncentralcleronomyroadstonebritishismdispassionedsundescriptivenesshydrogalvanicautoplasmotherapyhoordingredocketingunwreckedenfoldennonbankableprimevityunetymologically",
      "paraplastinovermotorglaikitnesseskingrowmesiolingualthackoorcrossbeakoutpraisedsenaitecryometryherschelbutsudanseparatoriesdiscoplacentalianpreinsulatechoristryprincipestrichophytiaundislodgeableextratabularpreemployercouadiaphanyeventognathousintercirculatingscribbliestcobblerlessantrinsubministrantrockish",
      "outfledattaccopremadnessempiriologicaluneradicateduntautologicallygantonoleocystmstantiliberalistvasemakingcocklighthydroxydehydrocorticosteroneanoscopetartagostackhousiaceousparanuclearterminizepurplinessorepearchfouetsinterdistinguishpanarchyjnanendriyaepirogeneticlateroversionidicantiphthisicalambulancingregidor",
      "pyroxylenechararasseparatedlyanachronismaticalpicroerythrinfaninmesostomidnondespoticallygilgameshrecompetitionteredinidaebuteonineisosterismtranshumanizegasboatnoctambulesuperplausiblycossyritelingtowpalaebiologistschronosemicglossoplegiahypoalkalineendoaortitislushiercreammakermonosemicestocadawoilienocardia",
      "pamplegiaepikeianonperceptiblesimmeringlyhydrocaulussuresbykathaguitermanitelamnectomyoutmalaproppedhemiramphinebeneplacitnonsilicatewelkeremovelesscorrelativismwitchbroomrisslebirkiestshemitepandariccroatiaphotoepinasticmacrosplanchnicornationinsensingsorroanoncumbrousparatitlesmahdism",
      "strouthiocameliancyanochroianonimpeachablesubtrochantericbudgypailettepaininglypindanonexemptionmonoplasmaticbiliprasinnoncelestialshithertolyleneenrobementastronsmyrnioteingeniosepihyalcytococciseignioralcondiddlementditremidundermanagerkidgierpootersbetrunkdeparliamentvidkids",
      "aponogetonaceousunconsultativesvirificshrinkergchileansoutshovingrhombiformtricompoundcapotenpachangaigniformdespairfulnesssprintlinebetafitelethargicalnesssericiculturescrassilingualnonadjacenciessketoketenevellincherheterizeelectrocataphoreticarchaeohippusalsweillfouetsundrossinessreduviidae",
      "unsmirkingnonamotionlaryngismalnonsynodicallyleysingdamaskinesmookspondilbishoplessbritishersnoldaraireslaccicprivilegerswangynonsingularitiesnoucheopisthographicalsubtransverselyweirdlessnessubermenschsrehypothecateincorporealizepractitioneryuninucleatedinvertibratesafenerrelayer",
      "calamaroidfarcemeatsubsulfatecolluncoredeemerbeslaveredunshammedprocellosedarksummonocondyliangollanspolarogramprepedunclebakeoutabnegativeoctachlorideundejectedopsyprionacelacinulasmudfatsammyblackbushrifledomautoserotherapypandeanredecisionconfricamentumsomaticovisceral",
      "extumescenceintwinementarenginternuncewoohoodiscodactylouseccoproticophorichindustanduskeroverpublicizingumbrettranshumanizehornslatebelozengednonannexationhousefurnishingsdinnerlysylvestralrefordsupercrimescrotectomyimperatesgriddlerspostallantoicnonsuspensiveresalutationmainpinbradyseisms",
      "nonactualnessegiptononcategoricalnesslecturessdeanthropomorphicsoverperchfibrinokinasebeentosophisticativegleicheniaceaeophiodontidaegroomishbescribblingsprecommunicationcataphrenicprecogitatingparochialitiesinfranchisethebsesquisquarezamindarieshospitaangelshipunderpriestprimegiltsanctcrotonbughuddroun",
      "pseudolunulanonlyricalnessscatchiebeerhallspostclaviculariguassuunloathlyunallusivelyovercontributionsirventunprofoundsemianunmammaliannonderogativecryptovolcanismantisudoralpleasablenesssquilliannucleohistonenondisparaginguncallusedcageylysephyrulasaumurelencticalcivilizadeintramorainicfrontstall",
      "epithalamitemplelikebiforinsalmanazarsblepharoncosissprediscountablecummockacheuleanunbanneredfleyednessdecohesionshirtlessnessamexpentadecahydratedunearthliestadetautophotoelectrictarantulatedtenderishtinynessantiasthmaticsunretrogradingporokeratosistaprootedskeraunophobiatarrietrollflowers",
      "jettinglydessignmentsnontravelerpalatiumglucocorticorduninthronedtertiiagaricalesswaptreunenonruminationthyreoiditispimanunderhangespadongheleemsbicyclismmethylidynehomoclinalrosalgercorrealgobacknontrademadreporaldioptographunderbrewmennoniteunconceitedlys",
      "intersolublesubtowergorgoniaprejudiciousnesslerizationbahanprelexicalcopertarentrayeusepseudobranchathoniteyakshadutchingcajanusginkgoalesabudefduflaparohysterectomyantidiphtheriainquietnessnonpuebloprereceiverglossemicintergonialnonplatitudinouslyphosphoreousdisauncountermandedabbycircuminsularbinotic",
      "alumnalscalyclinonmetamorphosishemisaprophyticrewarehousenonsupporterurbanakylcrysticpreburnunsuperlativeinsectiferoussoldatfirstersalagounprobatedcytoblastemousdowagerismlymphorrheasubarticlestntsidioglossiaspottledbackspeiringlovesomenessspongiosityantigonorrhealextracalicular",
      "corrigesalintataophyllogenoussprisaltrivantrenettespecificativelypaedotrophiesmillibarnmelolonthineplenartiesctenodontumbelliferoneregraduateunorganicallypelagraembootaunadmirablyfingerfishesrearraysinistruousresolicitationforeweighscomodatograviersseptenniadtwitchfireethicosocial",
      "forepolingsemifeudalismunhumannesschaungedadvolutionwinterboundunneedfulnessserenditecanangapetrolintocodynamometerdisquietednesslachrymaeformpostzygapophysisverminlikehydagedolosunannihilatorymurlackschamberwomansuperunityscnidoscoluswiwimoorillsuncalkgattinehargeisanemoricole"
    };
  }
}
