using System.Collections.Generic;
using System.Linq;
using System;

namespace CSharpMath {
  public class Requirements : HashSet<string>
  {
    public Requirements() : base() { }
    public bool Satisfies(Requirements otherRequirements) {
      return otherRequirements.IsSubsetOf(this);
    }
    public bool IsSatisfiedBy(Requirements otherRequirements) {
      return otherRequirements.Satisfies(this);
    }
    public Requirements(IEnumerable<string> strings): base(strings.Select(s => s.Trim())) {}
    public static Requirements FromCommaSeparatedString(string s) {
      Requirements r;
      if (string.IsNullOrWhiteSpace(s)) { // not strictly necessary, but may prevent creation of an array object.
        r = new Requirements();
      } else {
        string[] components = s.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
        return new Requirements(components);
        // string.Split(null) assumes that whitespace is the splitting character, as does
        // string.Split(new char[0]).
      }
      return r;
    }
		/// <summary>Any inherited Union method is going to return a HashSet . . .
		/// This union is null-safe.  Any null inputs are simply ignored.</summary>
		public static Requirements UnionRequirements(Requirements met1, Requirements met2) {
			Requirements r = new Requirements();
			if (met1 != null) {
				foreach (string s in met1) {
					r.Add(s);
				}
			}
			if (met2 != null) {
				foreach (string s in met2) {
					r.Add(s);
				}
			}
			return r;
		}
      

    public void DeepCopyIvarsFrom(Requirements cloneMe) {
      this.Clear();
      foreach(string s in cloneMe) {
        this.Add(s);
      }
    }

    public Requirements Clone() {
      Requirements r = new Requirements ();
      r.DeepCopyIvarsFrom(this);
      return r;
    }
      
  }
}
