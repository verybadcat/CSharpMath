using Typography.TextBreak;

// BreakAtInfo from Typography, moved here since this is now in a test project in Typography

namespace CSharpMath.Rendering.Text {
  internal struct BreakAtInfo {
    public readonly int breakAt;
    public readonly WordKind wordKind;
    
    public BreakAtInfo(int breakAt, WordKind wordKind) {
      this.breakAt = breakAt;
      this.wordKind = wordKind;
    }
  }
}