using System;
namespace CSharpMath
{
  /// <summary>A different kind of "attribute" altogether.
  /// But we put it here because of the name.</summary>
  [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate)]
  public sealed class PreserveAttribute : Attribute
  {
    public bool AllMembers;
    public bool Conditional;
  }
}
