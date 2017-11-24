using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CSharpMath
{
  public class ArrayAttribute: Attribute<List<GenericAttribute>>, IList<GenericAttribute>
  {
    public override TResult Accept<TResult>(IResultingAttributeVisitor<TResult> visitor) {
      TResult r = visitor.Visit(this);
      return r;
    }
    public override bool PayloadsAreEqual(List<GenericAttribute> payload1, List<GenericAttribute> payload2) {
      int payload1Count = payload1.Count();
      int payload2Count = payload2.Count();
      bool r = false;
      if (payload1Count == payload2Count) {
        r = true;
        IEnumerator<GenericAttribute> enumerator = payload1.GetEnumerator();
        IEnumerator<GenericAttribute> otherEnumerator = payload2.GetEnumerator();
        while (enumerator.MoveNext()) {
          otherEnumerator.MoveNext();
          GenericAttribute child1 = enumerator.Current;
          GenericAttribute child2 = otherEnumerator.Current;
          if (!child1.EqualsAttribute(child2)) {
            r = false;
            break;
          }
        }
      }
      return r;
    }

    public ArrayAttribute(IEnumerable<GenericAttribute> children): base(children.ToList()) {
    }
    public List<object> ToNonAttributeList(bool recursively, bool unwrapSingleQuoteStrings) {
      List<object> r = new List<object>();
      foreach(GenericAttribute entry in this.Payload) {
        object addMe;
        if (recursively) {
          addMe = entry.ToNonAttributeObjectRecursively(unwrapSingleQuoteStrings);
        } else {
          addMe = entry.PayloadObject;
        }
        r.Add(addMe);
      }
      return r;
    }
    public override object ToNonAttributeObjectRecursively(bool unwrapSingleQuoteSerializingStrings) {
      return this.ToNonAttributeList(true, unwrapSingleQuoteSerializingStrings);
    }
    public override IEnumerable<GenericAttribute> GetChildren() {
      return this.Payload;
    }
    public ArrayAttribute() : this(new List<GenericAttribute>()){}

    public override GenericAttribute CloneAttribute() {
      List<GenericAttribute> childClones = new List<GenericAttribute>();
      List<GenericAttribute> payload = this.Payload;
      foreach (GenericAttribute child in payload) {
        GenericAttribute childClone = child.CloneAttribute();
        childClones.Add(childClone);
      }
      ArrayAttribute r = new ArrayAttribute(childClones);
      return r;
    }

    public override GenericAttribute CloneAttributeButReplaceString(string replaceMe, string replacement) {
      List<GenericAttribute> childClones = new List<GenericAttribute>();
      List<GenericAttribute> payload = this.Payload;
      foreach (GenericAttribute child in payload) {
        GenericAttribute childClone = child.CloneAttributeButReplaceString(replaceMe, replacement);
        childClones.Add(childClone);
      }
      ArrayAttribute r = new ArrayAttribute(childClones);
      return r;
    }

    internal override Requirements RequirementsForEnclosingDictionary {
      get {
        Requirements r = new Requirements();
        List<GenericAttribute> kids = this.Payload;
        foreach (GenericAttribute child in kids) {
          string s = child.PayloadString();
          if (s != null) {
            r.Add(s);
          }
        }
        return r;
      }
    }


    public override GenericAttribute TrimWithMetRequirements(Requirements met, bool unwrap) {
      List<GenericAttribute> kids = this.Payload;
      List<GenericAttribute> rKids = new List<GenericAttribute>();
      foreach (GenericAttribute child in kids) {
        bool satisfied = child.IsSatisfiedBy(met);
        if (satisfied) {
          GenericAttribute trimmedChild = child.TrimWithMetRequirements(met, unwrap);
          rKids.Add(trimmedChild);
        }
      }
      GenericAttribute r = new ArrayAttribute(rKids);
      return r;
    }

    public IEnumerator<GenericAttribute> GetEnumerator() {
      return this.Payload.GetEnumerator();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
      return this.Payload.GetEnumerator();
    }

    public int IndexOf(GenericAttribute item) {
      return this.Payload.IndexOf(item);
    }

    public void Insert(int index, GenericAttribute item) {
      this.Payload.Insert(index, item);
    }

    public void RemoveAt(int index) {
      this.Payload.RemoveAt(index);
    }

    public GenericAttribute this[int index] {
      get {
        return this.Payload[index];
      }
      set {
        this.Payload[index] = value;
      }
    }

    public void Add(GenericAttribute item) {
      this.Payload.Add(item);
    }

    public void Clear() {
      this.Payload.Clear();
    }

    public bool Contains(GenericAttribute item) {
      return this.Payload.Contains(item);
    }

    public void CopyTo(GenericAttribute[] array, int arrayIndex) {
      this.Payload.CopyTo(array, arrayIndex);
    }

    public bool Remove(GenericAttribute item) {
      bool r = this.Payload.Remove(item);
      return r;
    }

    public int Count {
      get {
        int r = this.Payload.Count;
        return r;
      }
    }

    public bool IsReadOnly {
      get {
        return false;
      }
    }
  }
  public static class ArrayAttributeAdditions {
    public static void AddWrapped(this ArrayAttribute attribute, object obj) {
      GenericAttribute wrappedObj = GenericAttributes.Wrap(obj);
      attribute.Add(wrappedObj);
    }
  }
}

