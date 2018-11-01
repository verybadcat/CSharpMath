using System;
using System.Linq;
using System.Text;
namespace CSharpMath.Editor {
  using Constants;
  public class MathKeyboard<TButton> where TButton : IButton {
    public MathKeyboard(Action<TButton, EventHandler> registerPressed, TButton fractionButton, TButton multiplyButton, TButton equalsButton, TButton divisionButton, TButton exponentButton, TButton lessEqualsButton, TButton greaterEqualsButton, TButton shiftButton, TButton squareRootButton, TButton radicalButton, StringBuilder text, Box<int> textPosition, TButton[] numbers, TButton[] variables, TButton[] operators, TButton[] relations, TButton[] letters, TButton[] greekLetters, TButton alphaRho, TButton deltaOmega, TButton sigmaPhi, TButton muNu, TButton lambdaBeta) {
      textView = text;
      registerPressed(fractionButton, delegate { fractionPressed(); });
      registerPressed(multiplyButton, delegate { keyPressed(Symbols.Multiplication); });
      registerPressed(equalsButton, delegate { keyPressed('='); });
      registerPressed(divisionButton, delegate { keyPressed(Symbols.Division); });
      registerPressed(exponentButton, delegate { exponentPressed(); });
      registerPressed(lessEqualsButton, delegate { keyPressed(Symbols.LessEqual); });
      registerPressed(greaterEqualsButton, delegate { keyPressed(Symbols.GreaterEqual); });
      registerPressed(shiftButton, delegate { shiftPressed(); });
      registerPressed(squareRootButton, delegate { squareRootPressed(); });
      registerPressed(radicalButton, delegate { rootWithPowerPressed(); });
      foreach (var button in numbers.Concat(variables).Concat(operators).Concat(relations).Concat(letters).Concat(greekLetters))
        registerPressed(button, delegate { keyPressed(button.Text); });
      registerPressed(alphaRho, delegate { keyPressed(alphaRho.Text); });
      registerPressed(deltaOmega, delegate { keyPressed(deltaOmega.Text); });
      registerPressed(sigmaPhi, delegate { keyPressed(sigmaPhi.Text); });
      registerPressed(muNu, delegate { keyPressed(muNu.Text); });
      registerPressed(lambdaBeta, delegate { keyPressed(lambdaBeta.Text); });

      this.fractionButton = fractionButton;
      this.multiplyButton = multiplyButton;
      this.equalsButton = equalsButton;
      this.divisionButton = divisionButton;
      this.exponentButton = exponentButton;
      this.lessEqualsButton = lessEqualsButton;
      this.greaterEqualsButton = greaterEqualsButton;
      this.shiftButton = shiftButton;
      this.squareRootButton = squareRootButton;
      this.radicalButton = radicalButton;
      this.numbers = numbers;
      this.variables = variables;
      this.operators = operators;
      this.relations = relations;
      this.letters = letters;
      this.greekLetters = greekLetters;
      _alphaRho = alphaRho;
      _deltaOmega = deltaOmega;
      _sigmaPhi = sigmaPhi;
      _muNu = muNu;
      _lambdaBeta = lambdaBeta;
    }

    public TButton fractionButton; //(weak)
    public TButton multiplyButton; //(weak)
    public TButton equalsButton; //(weak)
    public TButton divisionButton; //(weak)
    public TButton exponentButton; //(weak)
    public TButton lessEqualsButton; //(weak)
    public TButton greaterEqualsButton; //(weak)
    public TButton shiftButton; //(weak)
    public TButton squareRootButton; //(weak)
    public TButton radicalButton; //(weak)

    public readonly StringBuilder textView; //(weak)
    public Box<int> cursorPosition;
    public TButton[] numbers;
    public TButton[] variables;
    public TButton[] operators;
    public TButton[] relations;
    public TButton[] letters;
    public TButton[] greekLetters;

    public TButton _alphaRho; //(weak)
    public TButton _deltaOmega; //(weak)
    public TButton _sigmaPhi; //(weak)
    public TButton _muNu; //(weak)
    public TButton _lambdaBeta; //(weak)

    public void keyPressed(char key) =>
      textView.Insert(cursorPosition.Content++, key);
    public void keyPressed(string key) {
      textView.Insert(cursorPosition.Content, key);
      cursorPosition.Content += key.Length;
    }
    public void backspacePressed() =>
      textView.Remove(--cursorPosition.Content, 1);
    public void enterPressed() =>
      textView.Insert(cursorPosition.Content++, '\n');
    public event EventHandler dismissPressed;
    public void fractionPressed() =>
      textView.Insert(cursorPosition.Content++, Symbols.FractionSlash);
    public void exponentPressed() =>
      textView.Insert(cursorPosition.Content++, '^');
    public void parensPressed() {
      textView.Insert(cursorPosition.Content++, '(');
      textView.Insert(cursorPosition.Content++, ')');
    }
    public void subscriptPressed() =>
      textView.Insert(cursorPosition.Content++, '_');
    public void absValuePressed() {
      textView.Insert(cursorPosition.Content++, '|');
      textView.Insert(cursorPosition.Content++, '|');
    }
    public void shiftPressed() {
      Shifted ^= true;
      if (Shifted) {
        foreach (var button in letters)
          button.Text = button.Text.ToUpper();
        _alphaRho.Text = "ρ";
        _deltaOmega.Text = "ω";
        _sigmaPhi.Text = "Φ";
        _muNu.Text = "ν";
        _lambdaBeta.Text = "β";
      } else {
        foreach (var button in letters)
          button.Text = button.Text.ToLower();
        _alphaRho.Text = "α";
        _deltaOmega.Text = "Δ";
        _sigmaPhi.Text = "σ";
        _muNu.Text = "μ";
        _lambdaBeta.Text = "λ";
      }
    }
    public void squareRootPressed() =>
      textView.Insert(cursorPosition.Content++, Symbols.SquareRoot);
    public void rootWithPowerPressed() =>
      textView.Insert(cursorPosition.Content++, Symbols.CubeRoot);
    public void logWithBasePressed() {
      textView.Insert(cursorPosition.Content++, 'l');
      textView.Insert(cursorPosition.Content++, 'o');
      textView.Insert(cursorPosition.Content++, 'g');
      textView.Insert(cursorPosition.Content++, '_');
    }

    bool _numbersEnabled;
    public bool NumbersEnabled {
      get => _numbersEnabled;
      set {
        _numbersEnabled = value;
        foreach (var button in numbers) button.Enabled = value;
      }
    }

    bool _operatorsEnabled;
    public bool OperatorsEnabled {
      get => _operatorsEnabled;
      set {
        _operatorsEnabled = value;
        foreach (var button in operators) button.Enabled = value;
      }
    }

    bool _variablesEnabled;
    public bool VariablesEnabled {
      get => _variablesEnabled;
      set {
        _variablesEnabled = value;
        foreach (var button in variables) button.Enabled = value;
      }
    }

    public bool FractionEnabled {
      get => fractionButton.Enabled;
      set => fractionButton.Enabled = value;
    }
    public bool EqualsEnabled {
      get => equalsButton.Enabled;
      set => equalsButton.Enabled = value;
    }
    public bool ExponentHighlighted {
      get => exponentButton.Selected;
      set => exponentButton.Selected = value;
    }
    public bool SquareRootHighlighted {
      get => squareRootButton.Selected;
      set => squareRootButton.Selected = value;
    }
    public bool RadicalHighlighted {
      get => radicalButton.Selected;
      set => radicalButton.Selected = value;
    }
    public bool Shifted { get; set; }
    public bool Selected { get; set; }
  }
}