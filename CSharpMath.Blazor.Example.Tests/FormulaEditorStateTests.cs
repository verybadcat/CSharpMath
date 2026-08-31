using CSharpMath.Blazor.Example;
using Xunit;

public class FormulaEditorStateTests {
  [Fact]
  public void Default_is_multiline_and_reset_restores_it() {
    var state = new FormulaEditorState();
    Assert.Contains("\\\\", state.Latex);
    var initialRevision = state.Revision;
    state.SetLatex("x");
    state.Reset();
    Assert.Equal(FormulaEditorState.DefaultLatex, state.Latex);
    Assert.Equal(initialRevision + 2, state.Revision);
  }

  [Fact]
  public void SetLatex_preserves_invalid_input_for_renderer_error_display() {
    var state = new FormulaEditorState();
    state.SetLatex(@"\\notacommand{");
    Assert.Equal(@"\\notacommand{", state.Latex);
    Assert.Equal(1, state.Revision);
  }
}
