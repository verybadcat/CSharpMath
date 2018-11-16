using System;
using System.Collections.ObjectModel;

namespace CSharpMath.Editor {
  using Color = Structures.Color;
  public partial class MathKeyboardView<TButton, TLayout> where TButton : class, IButton where TLayout : IButtonLayout<TButton, TLayout> {
    public MathKeyboardView(Func<TLayout> layoutCtor, Action<TButton, EventHandler> registerPressed, System.Drawing.RectangleF bounds,
        TButton leftBtn,
        MathKeyboard<TButton, TLayout> numbers, TButton numbersBtn,
        MathKeyboard<TButton, TLayout> operations, TButton operationsBtn,
        MathKeyboard<TButton, TLayout> functions, TButton functionsBtn,
        MathKeyboard<TButton, TLayout> letters, TButton lettersBtn,
        TButton rightBtn) {
      //MathList = new Atoms.MathList();
      Tabs = new ReadOnlyCollection<MathKeyboard<TButton, TLayout>>(new[] { numbers, operations, functions, letters });
      foreach (var tab in Tabs) tab.layout.Visible = false;
      _currentTab = numbers;
      _currentTab.Selected = true;
      _currentTab.layout.Visible = true;

      registerPressed(leftBtn, delegate { leftPressed?.Invoke(this, EventArgs.Empty); });
      registerPressed(rightBtn, delegate { rightPressed?.Invoke(this, EventArgs.Empty); });

      registerPressed(numbersBtn, delegate { CurrentTab = NumbersTab; });
      registerPressed(operationsBtn, delegate { CurrentTab = OperationsTab; });
      registerPressed(functionsBtn, delegate { CurrentTab = FunctionsTab; });
      registerPressed(lettersBtn, delegate { CurrentTab = LettersTab; });
      layout = layoutCtor();
      layout.Add(leftBtn);
      layout.Add(numbersBtn);
      layout.Add(operationsBtn);
      layout.Add(functionsBtn);
      layout.Add(lettersBtn);
      layout.Add(rightBtn);
      foreach (var tab in Tabs)
        layout.Add(tab.layout);
      layout.Bounds = bounds;
    }

    public readonly TLayout layout;
    public event EventHandler leftPressed;
    public event EventHandler rightPressed;
    //public static readonly MathKeyboardView<TButton, TTextView> Instance = new MathKeyboardView<TButton, TTextView>();

    public ReadOnlyCollection<MathKeyboard<TButton, TLayout>> Tabs { get; }
    public MathKeyboard<TButton, TLayout> NumbersTab => Tabs[0];
    public MathKeyboard<TButton, TLayout> OperationsTab => Tabs[1];
    public MathKeyboard<TButton, TLayout> FunctionsTab => Tabs[2];
    public MathKeyboard<TButton, TLayout> LettersTab => Tabs[3];
    private MathKeyboard<TButton, TLayout> _currentTab;
    public MathKeyboard<TButton, TLayout> CurrentTab {
      get => _currentTab;
      set {
        _currentTab.Selected = false;
        _currentTab.layout.Visible = false;
        _currentTab = value;
        value.layout.Visible = true;
        value.Selected = true;
      }
    }


    //public void StartedEditing(System.Text.StringBuilder label) {
    //  foreach (var tab in Tabs)
    //    tab.textView = label;
    //}

    //public void FinishedEditing(System.Text.StringBuilder label) {
    //  foreach (var tab in Tabs)
    //    tab.textView = null;
    //}

    bool _equalsAllowed;
    public bool EqualsAllowed {
      get => _equalsAllowed;
      set {
        _equalsAllowed = value;
        foreach (var tab in Tabs) {
          tab.EqualsEnabled = value;
        }
      }
    }

    bool _numbersAllowed;
    public bool NumbersAllowed {
      get => _numbersAllowed;
      set {
        _numbersAllowed = value;
        foreach (var tab in Tabs) {
          tab.NumbersEnabled = value;
        }
      }
    }
    bool _operatorsAllowed;
    public bool OperatorsAllowed {
      get => _operatorsAllowed;
      set {
        _operatorsAllowed = value;
        foreach (var tab in Tabs) {
          tab.OperatorsEnabled = value;
        }
      }
    }
    bool _variablesAllowed;
    public bool VariablesAllowed {
      get => _variablesAllowed;
      set {
        _variablesAllowed = value;
        foreach (var tab in Tabs) {
          tab.VariablesEnabled = value;
        }
      }
    }
    bool _exponentHighlighted;
    public bool ExponentHighlighted {
      get => _exponentHighlighted;
      set {
        _exponentHighlighted = value;
        foreach (var tab in Tabs)
          tab.ExponentHighlighted = value;
      }
    }

    bool _squareRootHighlighted;
    public bool SquareRootHighlighted {
      get => _squareRootHighlighted;
      set {
        _squareRootHighlighted = value;
        foreach (var tab in Tabs)
          tab.SquareRootHighlighted = value;
      }
    }

    bool _radicalHighlighted;
    public bool RadicalHighlighted {
      get => _radicalHighlighted;
      set {
        _radicalHighlighted = value;
        foreach (var tab in Tabs)
          tab.RadicalHighlighted = value;
      }
    }
  }
}