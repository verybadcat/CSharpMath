using System;
using System.Collections.ObjectModel;

namespace CSharpMath.Editor {

  public partial class MathKeyboardView<TButton> where TButton : class, IButton {
    public MathKeyboardView(MathKeyboard<TButton> numbers, MathKeyboard<TButton> operations, MathKeyboard<TButton> functions, MathKeyboard<TButton> letters) {
      _currentTab = NumbersTab;
      _currentTab.Selected = true;
      var text = new System.Text.StringBuilder();
      var textPosition = new Box<int>();
      Tabs = new ReadOnlyCollection<MathKeyboard<TButton>>(new[] { numbers, operations, functions, letters });
    }
    //public static readonly MathKeyboardView<TButton, TTextView> Instance = new MathKeyboardView<TButton, TTextView>();

    public ReadOnlyCollection<MathKeyboard<TButton>> Tabs { get; }
    public MathKeyboard<TButton> NumbersTab => Tabs[0];
    public MathKeyboard<TButton> OperationsTab => Tabs[1];
    public MathKeyboard<TButton> FunctionsTab => Tabs[2];
    public MathKeyboard<TButton> LettersTab => Tabs[3];
    private MathKeyboard<TButton> _currentTab;
    public MathKeyboard<TButton> CurrentTab {
      get => _currentTab;
      set {
        _currentTab.Selected = false;
        _currentTab = value;
        value.Selected = true;
      }
    }
    public Atoms.MathList MathList { get; set; }


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