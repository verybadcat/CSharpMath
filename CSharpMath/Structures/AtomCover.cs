using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath.Structures {
    public struct AtomCover : IEquatable<AtomCover> {
    public string coverType { get; set; }
    public string? previousText { get; set; }
    public string? nextText { get; set; }

    public AtomCover(string coverType ,string? previousText = null, string? nextText = null) {
      this.coverType = coverType;
      this.previousText = previousText;
      this.nextText = nextText;
    }

    private string combination => coverType + previousText + nextText;
    public override bool Equals(object obj) {
      return obj is AtomCover other ?
      coverType == other.coverType ?
      previousText == other.previousText ?
      nextText == other.nextText ?
      true : false: false : false: false; 
    }

    public override int GetHashCode() {
      return combination.GetHashCode();
    }
    public bool Equals(AtomCover other) {
      return
      coverType == other.coverType ?
      previousText == other.previousText ?
      nextText == other.nextText ?
      true : false: false : false; 
    }
    public static bool IsNonNullOrEmpty(List<AtomCover>? cover) =>
     cover is not null ? cover.IsNonEmpty() ? true : false : false; 
    public static bool operator ==(AtomCover left, AtomCover right) {
      return left.Equals(right);
    }

    public static bool operator !=(AtomCover left, AtomCover right) {
      return !(left == right);
    }

  }
}
