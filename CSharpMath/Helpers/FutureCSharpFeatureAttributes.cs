using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpMath {
  // See the attribute guidelines at 
  //  http://go.microsoft.com/fwlink/?LinkId=85236
  /// <summary>Marker for nullable reference types. For C# 8.</summary>
  /// <remarks>Usage is not ReturnValue since Method is easier to apply.</remarks>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
  public sealed class NullableReferenceAttribute : Attribute { }
  /// <summary>Marker for non nullable reference types. For C# 8.</summary>
  /// <remarks>Usage is not ReturnValue since Method is easier to apply.</remarks>
  [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter | AttributeTargets.GenericParameter | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
  public sealed class NonNullableReferenceAttribute : Attribute { }
  /// <summary>
  /// Usage of this attribute is usually commented out.
  /// Marker for section of code requiring static interface members, that are currently #ifdef'd out.
  /// </summary>
  [AttributeUsage(AttributeTargets.All)]
  public sealed class StaticInterfaceMembersAttribute : Attribute { }
}
