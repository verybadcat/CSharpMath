using System;
using System.Linq;
using System.Text;
namespace CSharpMath.Editor {
  using Constants;
  public class MathKeyboard<TButton, TLayout> where TButton : class, IButton where TLayout : IButtonLayout<TButton, TLayout> {
    public MathKeyboard(Func<TLayout> layoutCtor, Action<TButton, EventHandler> registerPressed, System.Drawing.RectangleF bounds, TButton fractionButton, TButton multiplyButton, TButton equalsButton, TButton divisionButton, TButton exponentButton, TButton subscriptButton, TButton shiftButton, TButton squareRootButton, TButton radicalButton, TButton logBaseButton, TButton absButton, TButton[] numbers, TButton[] variables, TButton[] operators, TButton[] relations, TButton[] letters, TButton[] greekLetters, TButton alphaRho, TButton deltaOmega, TButton sigmaPhi, TButton muNu, TButton lambdaBeta, TButton backspace, TButton dismiss, TButton enter) {
      layout = layoutCtor();
      layout.Bounds = bounds;
      if (fractionButton != null) {
        registerPressed(fractionButton, delegate { keyPressed(Symbols.FractionSlash); });
        layout.Add(fractionButton);
      }
      if (multiplyButton != null) {
        registerPressed(multiplyButton, delegate { keyPressed(Symbols.Multiplication); });
        layout.Add(multiplyButton);
      }
      if (equalsButton != null) {
        registerPressed(equalsButton, delegate { keyPressed('='); });
        layout.Add(equalsButton);
      }
      if (divisionButton != null) {
        registerPressed(divisionButton, delegate { keyPressed(Symbols.Division); });
        layout.Add(divisionButton);
      }
      if (exponentButton != null) {
        registerPressed(exponentButton, delegate { keyPressed('^'); });
        layout.Add(exponentButton);
      }
      if(subscriptButton != null){
        registerPressed(subscriptButton, delegate { keyPressed('_'); });
        layout.Add(subscriptButton);
      }
      if (shiftButton != null) {
        registerPressed(shiftButton, delegate { shiftPressed(); });
        layout.Add(shiftButton);
      }
      if (squareRootButton != null) {
        registerPressed(squareRootButton, delegate { keyPressed(Symbols.SquareRoot); });
        layout.Add(squareRootButton);
      }
      if (radicalButton != null) {
        registerPressed(radicalButton, delegate { keyPressed(Symbols.CubeRoot); });
        layout.Add(radicalButton);
      }
      if (logBaseButton != null) {
        registerPressed(logBaseButton, delegate { keyPressed("log_"); });
        layout.Add(logBaseButton);
      }
      if(absButton != null){
        registerPressed(absButton, delegate { keyPressed("|"); });
        layout.Add(absButton);
      }
      foreach (var button in (numbers ?? Array.Empty<TButton>()).Concat(variables ?? Array.Empty<TButton>()).Concat(operators ?? Array.Empty<TButton>()).Concat(relations ?? Array.Empty<TButton>()).Concat(letters ?? Array.Empty<TButton>()).Concat(greekLetters ?? Array.Empty<TButton>()) ?? Array.Empty<TButton>()) {
        registerPressed(button, delegate { keyPressed(button.Text); });
        layout.Add(button);
      }
      if (alphaRho != null) {
        registerPressed(alphaRho, delegate { keyPressed(alphaRho.Text); });
        layout.Add(alphaRho);
      }
      if (deltaOmega != null) {
        registerPressed(deltaOmega, delegate { keyPressed(deltaOmega.Text); });
        layout.Add(deltaOmega);
      }
      if (sigmaPhi != null) {
        registerPressed(sigmaPhi, delegate { keyPressed(sigmaPhi.Text); });
        layout.Add(sigmaPhi);
      }
      if (muNu != null) {
        registerPressed(muNu, delegate { keyPressed(muNu.Text); });
        layout.Add(muNu);
      }
      if (lambdaBeta != null) {
        registerPressed(lambdaBeta, delegate { keyPressed(lambdaBeta.Text); });
        layout.Add(lambdaBeta);
      }
      if (backspace != null) {
        registerPressed(backspace, delegate { backspacePressed(); });
        layout.Add(backspace);
      }
      if (dismiss != null) {
        registerPressed(dismiss, dismissPressed);
        layout.Add(dismiss);
      }
      if (enter != null) {
        registerPressed(enter, delegate { keyPressed('\n'); });
        layout.Add(enter);
      }

      this.fractionButton = fractionButton;
      this.multiplyButton = multiplyButton;
      this.equalsButton = equalsButton;
      this.divisionButton = divisionButton;
      this.exponentButton = exponentButton;
      this.subscriptButton = subscriptButton;
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
    public readonly TLayout layout;

    public Action<string> insertText;
    public Action delete;
    public TButton fractionButton; //(weak)
    public TButton multiplyButton; //(weak)
    public TButton equalsButton; //(weak)
    public TButton divisionButton; //(weak)
    public TButton exponentButton; //(weak)
    public TButton subscriptButton;
    public TButton shiftButton; //(weak)
    public TButton squareRootButton; //(weak)
    public TButton radicalButton; //(weak)
    public TButton logBaseButton;
    public TButton absButton;

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

    public event EventHandler textChanged = delegate { };
    public event EventHandler dismissPressed = delegate { };

    public void keyPressed(char key) =>
      insertText(key.ToString());

    public void keyPressed(string key) =>
      insertText(key);
    public void backspacePressed() =>
      delete();
    public void parensPressed() =>
      insertText("()");
    public void subscriptPressed() =>
    insertText("_");

    public void absValuePressed() =>
    insertText("||");
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
    public void logWithBasePressed() =>
    insertText("log_");

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
      get => fractionButton?.Enabled ?? false;
      set { if (fractionButton != null) fractionButton.Enabled = value; }
    }
    public bool EqualsEnabled {
      get => equalsButton?.Enabled ?? false;
      set { if (equalsButton != null) equalsButton.Enabled = value; }
    }
    public bool ExponentHighlighted {
      get => exponentButton?.Selected ?? false;
      set { if (exponentButton != null) exponentButton.Selected = value; }
    }
    public bool SquareRootHighlighted {
      get => squareRootButton?.Selected ?? false;
      set { if (squareRootButton != null) squareRootButton.Selected = value; }
    }
    public bool RadicalHighlighted {
      get => radicalButton?.Selected ?? false;
      set { if (radicalButton != null) radicalButton.Selected = value; }
    }
    public bool Shifted { get; set; }
    public bool Selected { get; set; }
  }
}