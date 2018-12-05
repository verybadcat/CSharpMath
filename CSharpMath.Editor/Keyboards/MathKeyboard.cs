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
        registerPressed(fractionButton, delegate { KeyPressed(Symbols.FractionSlash); });
        layout.Add(fractionButton);
      }
      if (multiplyButton != null) {
        registerPressed(multiplyButton, delegate { KeyPressed(Symbols.Multiplication); });
        layout.Add(multiplyButton);
      }
      if (equalsButton != null) {
        registerPressed(equalsButton, delegate { KeyPressed("="); });
        layout.Add(equalsButton);
      }
      if (divisionButton != null) {
        registerPressed(divisionButton, delegate { KeyPressed(Symbols.Division); });
        layout.Add(divisionButton);
      }
      if (exponentButton != null) {
        registerPressed(exponentButton, delegate { KeyPressed("^"); });
        layout.Add(exponentButton);
      }
      if(subscriptButton != null){
        registerPressed(subscriptButton, delegate { KeyPressed("_"); });
        layout.Add(subscriptButton);
      }
      if (shiftButton != null) {
        registerPressed(shiftButton, delegate { ShiftPressed(); });
        layout.Add(shiftButton);
      }
      if (squareRootButton != null) {
        registerPressed(squareRootButton, delegate { KeyPressed(Symbols.SquareRoot); });
        layout.Add(squareRootButton);
      }
      if (radicalButton != null) {
        registerPressed(radicalButton, delegate { KeyPressed(Symbols.CubeRoot); });
        layout.Add(radicalButton);
      }
      if (logBaseButton != null) {
        registerPressed(logBaseButton, delegate { KeyPressed("log"); KeyPressed("_"); });
        layout.Add(logBaseButton);
      }
      if(absButton != null){
        registerPressed(absButton, delegate { KeyPressed("||"); });
        layout.Add(absButton);
      }
      foreach (var button in (numbers ?? Array.Empty<TButton>()).Concat(variables ?? Array.Empty<TButton>()).Concat(operators ?? Array.Empty<TButton>()).Concat(relations ?? Array.Empty<TButton>()).Concat(letters ?? Array.Empty<TButton>()).Concat(greekLetters ?? Array.Empty<TButton>()) ?? Array.Empty<TButton>()) {
        registerPressed(button, delegate { KeyPressed(button.Text); });
        layout.Add(button);
      }
      if (alphaRho != null) {
        registerPressed(alphaRho, delegate { KeyPressed(alphaRho.Text); });
        layout.Add(alphaRho);
      }
      if (deltaOmega != null) {
        registerPressed(deltaOmega, delegate { KeyPressed(deltaOmega.Text); });
        layout.Add(deltaOmega);
      }
      if (sigmaPhi != null) {
        registerPressed(sigmaPhi, delegate { KeyPressed(sigmaPhi.Text); });
        layout.Add(sigmaPhi);
      }
      if (muNu != null) {
        registerPressed(muNu, delegate { KeyPressed(muNu.Text); });
        layout.Add(muNu);
      }
      if (lambdaBeta != null) {
        registerPressed(lambdaBeta, delegate { KeyPressed(lambdaBeta.Text); });
        layout.Add(lambdaBeta);
      }
      if (backspace != null) {
        registerPressed(backspace, delegate { BackspacePressed(); });
        layout.Add(backspace);
      }
      if (dismiss != null) {
        registerPressed(dismiss, DismissPressed);
        layout.Add(dismiss);
      }
      if (enter != null) {
        registerPressed(enter, delegate { KeyPressed("\n"); });
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

    public event Action<string> InsertText;
    public event Action Delete;
    public event EventHandler DismissPressed = delegate { };
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
    
    public void KeyPressed(string key) =>
      InsertText?.Invoke(key);
    public void BackspacePressed() =>
      Delete?.Invoke();
    public void ParensPressed() =>
      KeyPressed("()");
    public void ShiftPressed() {
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