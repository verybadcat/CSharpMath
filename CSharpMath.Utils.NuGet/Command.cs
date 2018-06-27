using System;
using System.Windows.Input;

namespace CSharpMath.Utils.NuGet {
  //https://www.codeproject.com/Articles/274982/Commands-in-MVVM

  /// <summary>
  /// The Command class - an ICommand that can fire a function.
  /// </summary>
  public class Command : ICommand {
    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="canExecute">if set to <c>true</c> [can execute].</param>
    public Command(Action action, bool canExecute = true) {
      //  Set the action.
      this.action = action;
      this.canExecute = canExecute;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    /// <param name="parameterizedAction">The parameterized action.</param>
    /// <param name="canExecute">if set to <c>true</c> [can execute].</param>
    public Command(Action<object> parameterizedAction, bool canExecute = true) {
      //  Set the action.
      this.parameterizedAction = parameterizedAction;
      this.canExecute = canExecute;
    }

    /// <summary>
    /// The action (or parameterized action) that will be called when the command is invoked.
    /// </summary>
    protected Action action = null;
    protected Action<object> parameterizedAction = null;

    /// <summary>
    /// Bool indicating whether the command can execute.
    /// </summary>
    private bool canExecute = false;

    /// <summary>
    /// Gets or sets a value indicating whether this instance can execute.
    /// </summary>
    /// <value>
    ///     <c>true</c> if this instance can execute; otherwise, <c>false</c>.
    /// </value>
    public bool CanExecute {
      get { return canExecute; }
      set {
        if (canExecute != value) {
          canExecute = value;
          CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
      }
    }

    /// <summary>
    /// Defines the method that determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command.
    ///  If the command does not require data to be passed,
    ///  this object can be set to null.</param>
    /// <returns>
    /// true if this command can be executed; otherwise, false.
    /// </returns>
    bool ICommand.CanExecute(object parameter) {
      return canExecute;
    }

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command.
    ///  If the command does not require data to be passed,
    ///  this object can be set to null.</param>
    void ICommand.Execute(object parameter) {
      this.DoExecute(parameter);

    }

    /// <summary>
    /// Occurs when can execute is changed.
    /// </summary>
    public event EventHandler CanExecuteChanged;

    /// <summary>
    /// Occurs when the command is about to execute.
    /// </summary>
    public event EventHandler<CancelCommandEventArgs> Executing;

    /// <summary>
    /// Occurs when the command executed.
    /// </summary>
    public event EventHandler<CommandEventArgs> Executed;

    protected void InvokeAction(object param) {
      Action theAction = action;
      Action<object> theParameterizedAction = parameterizedAction;
      if (theAction != null)
        theAction();
      else theParameterizedAction?.Invoke(param);
    }

    protected void InvokeExecuted(CommandEventArgs args) {
      Executed?.Invoke(this, args);
    }

    protected void InvokeExecuting(CancelCommandEventArgs args) {
      Executing?.Invoke(this, args);
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="param">The param.</param>
    public virtual void DoExecute(object param) {
      //  Invoke the executing command, allowing the command to be cancelled.
      CancelCommandEventArgs args =
         new CancelCommandEventArgs() { Parameter = param, Cancel = false };
      InvokeExecuting(args);

      //  If the event has been cancelled, bail now.
      if (args.Cancel)
        return;

      //  Call the action or the parameterized action, whichever has been set.
      InvokeAction(param);

      //  Call the executed function.
      InvokeExecuted(new CommandEventArgs() { Parameter = param });
    }
  }

  public class CommandEventArgs : EventArgs {
    /// <summary>
    /// The parameter passed onto the function.
    /// </summary>
    public object Parameter { get; set; }
  }
  public class CancelCommandEventArgs : CommandEventArgs {
    /// <summary>
    /// The function will not execute if this has been set.
    /// </summary>
    public bool Cancel { get; set; }
  }
}