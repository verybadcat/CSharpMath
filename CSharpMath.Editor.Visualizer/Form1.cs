using CSharpMath.Editor;
namespace CSharpMath.Editor.Visualizer {
  public partial class Form1 : Form {
    private List<MathKeyboardInput> inputList = new();
    private LatexMathKeyboard latexMathKeyboard = new();
    public Form1() {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e) {
        foreach(MathKeyboardInput input in (MathKeyboardInput[]) Enum.GetValues(typeof(MathKeyboardInput)))
        {
          CommandFire.Items.Add(input);
        }
      }

    private void AddCommand_Click(object sender, EventArgs e) {
      if(CommandFire.SelectedIndex == -1) return;
      CommandList.Items.Add(CommandFire.SelectedItem.ToString());
      inputList.Add((MathKeyboardInput)CommandFire.SelectedItem);
    }

    private void Delete_Click(object sender, EventArgs e) {
      if(CommandList.Items.Count == 0) return;
      int lang = CommandList.Items.Count - 1;
      CommandList.Items.RemoveAt(lang);
      inputList.RemoveAt(lang);
    }

    private void Fire_Click(object sender, EventArgs e) {
      latexMathKeyboard.Clear();
      latexMathKeyboard.KeyPress(inputList.ToArray());
      LatexLable.Text = latexMathKeyboard.LaTeX;
    }

    private void DebugButton_Click(object sender, EventArgs e) {
      latexMathKeyboard.Clear();
      latexMathKeyboard.KeyPress(inputList.ToArray());
      LatexLable.Text = latexMathKeyboard.LaTeX;
    }


    private void CommandFire_KeyDown(object sender, KeyEventArgs e) {
      if (e.KeyCode == Keys.Enter) {
        AddCommand.PerformClick();
      }
      if (e.KeyCode == Keys.Back) {
        Delete.PerformClick();
      }
    }
  }
  }