﻿namespace DriveSync;

public class Command : ICommand
{
    private readonly Action<object> executeMethod;
    private readonly Func<object, bool> canExecuteMethod;

    /// <summary>
    /// A Constructor with two parameters.
    /// </summary>
    /// <remarks>
    /// Also has an overload with one parameter.
    /// </remarks>
    /// <param name="ExecutableMethod"></param>
    /// <param name="CanExecuteMethod"></param>
    public Command(Action<object> ExecutableMethod, Func<object, bool> CanExecuteMethod)
    {
        executeMethod = ExecutableMethod;
        canExecuteMethod = CanExecuteMethod;
    }

    /// <summary>
    /// A Constructor with one parameter.
    /// </summary>
    /// <remarks>
    /// Also has an overload with two parameters.
    /// </remarks>
    /// <param name="ExecutableMethod"></param>
    public Command(Action<object> ExecutableMethod)
    {
        executeMethod = ExecutableMethod;
    }

    public bool CanExecute(object parameter)
    {
        return canExecuteMethod == null || canExecuteMethod(parameter);
    }

    public void Execute(object parameter)
    {
        executeMethod(parameter);
    }

    public event EventHandler CanExecuteChanged {
        add {
            CommandManager.RequerySuggested += value;
        }
        remove {
            CommandManager.RequerySuggested -= value;
        }
    }
}
