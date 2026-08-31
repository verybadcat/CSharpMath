using System.Linq;
using Typography.OpenFont;

namespace CSharpMath.Rendering.BackEnd {
  internal sealed class FontSnapshot {
    public FontSnapshot(TypefaceDescriptor[] localDescriptors, Typeface[] localTypefaces,
      Typeface[] globalTypefaces) {
      LocalDescriptors = localDescriptors;
      LocalTypefaces = localTypefaces;
      GlobalTypefaces = globalTypefaces;
      Descriptors = localDescriptors.Concat(globalTypefaces.Select(TypefaceDescriptor.Adapt)).ToArray();
      Typefaces = localTypefaces.Concat(globalTypefaces).ToArray();
    }
    public TypefaceDescriptor[] LocalDescriptors { get; }
    public TypefaceDescriptor[] Descriptors { get; }
    public Typeface[] LocalTypefaces { get; }
    public Typeface[] GlobalTypefaces { get; }
    public Typeface[] Typefaces { get; }
  }
}
