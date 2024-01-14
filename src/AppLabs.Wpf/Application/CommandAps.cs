using System.Windows.Input;

namespace AppLabs.Wpf.Application;

/// <summary>
/// Una implementacion de ICommand
/// </summary>
public class CommandAps : ICommand
{
    private readonly Func<object, bool> _canExecute;
    private readonly Action<object> _executeAction;
    private bool _canExecuteCache;
     
    public CommandAps(Action<object> executeAction, Func<object, bool> canExecute)
    {
        this._executeAction = executeAction;
        this._canExecute = canExecute;            
    }
    
    public bool CanExecute(object parameter)
    {
        bool temp = _canExecute(parameter);

        if (_canExecuteCache == temp) return _canExecuteCache;
        
        _canExecuteCache = temp;
        
        if (CanExecuteChanged != null)
        {
            CanExecuteChanged(this, new EventArgs());
        }

        return _canExecuteCache;
    }
    
        
    public void Execute(object parameter)
    {
        _executeAction(parameter);
    }

    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Vuele a disparar el evento CanExecute asignado cuando es llamado.
    /// </summary>
    public void OnCanExecutedChanged()
    {
        if (CanExecuteChanged != null)
        {
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }


}