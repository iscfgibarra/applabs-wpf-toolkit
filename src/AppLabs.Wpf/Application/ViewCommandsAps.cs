using System.ComponentModel;
using System.Windows.Input;
using System.Xml;

namespace AppLabs.Wpf.Application;

public class ViewCommandsAps: INotifyPropertyChanged
{
    private ICommand? _addCommand;
    private ICommand? _editCommand;
    private ICommand? _deleteCommand;
    private ICommand? _okCommand;
    private ICommand? _cancelCommand;
    private ICommand? _nextCommand;
    private ICommand? _previousCommand;
    private ICommand? _firstCommand;
    private ICommand? _lastCommand;
    private ICommand? _hiddenResultCommand;
    private List<ICommand> _commands;
    
    public ViewCommandsAps()
    {
        _commands = new List<ICommand>();
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        var handler = PropertyChanged;

        if (handler != null)
        {
            handler(this, e);
        }
    }


    /// <summary>
    /// Comando agregar a una coleccion existente.
    /// </summary>
    public ICommand? AddCommand
    {
        get => _addCommand;
        set
        {
            if (value == null) return;
            
            _addCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("AddCommand"));

        }
    }


    /// <summary>
    /// Permite editar el registro actual de la coleccion subyacente.
    /// </summary>
    public ICommand? EditCommand
    {
        get => _editCommand;
        set
        {
            if (value == null) return;
            
            _editCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("EditCommand"));
        }
    }


    /// <summary>
    /// Permite borrar el registro de l
    /// </summary>
    public ICommand? DeleteCommand
    {
        get => _deleteCommand;
        set
        {
            if (value == null) return;
            _deleteCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("DeleteCommand"));
        }
    }


    public ICommand? OkCommand
    {
        get => _okCommand;
        set
        {
            if (value == null) return;
            _okCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("OkCommand"));
        }
    }


    public ICommand? CancelCommand
    {
        get => _cancelCommand;
        set
        {
            if (value == null) return;
            _cancelCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("CancelCommand"));
        }
    }


    public ICommand? NextCommand
    {
        get => _nextCommand;
        set
        {
            if (value == null) return;
            _nextCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("NextCommand"));
        }
    }


    public ICommand? PreviousCommand
    {
        get => _previousCommand;
        set
        {
            if (value == null) return;
            _previousCommand = value;
            _commands.Add(value);
            OnPropertyChanged(new PropertyChangedEventArgs("PreviousCommand"));
        }
    }


    public ICommand? FirstCommand
    {
        get => _firstCommand;
        set
        {
            _firstCommand = value;
            OnPropertyChanged(new PropertyChangedEventArgs("FirstCommand"));
        }
    }


    public ICommand LastCommand
    {
        get => _lastCommand;
        set
        {
            _lastCommand = value;
            OnPropertyChanged(new PropertyChangedEventArgs("LastCommand"));
        }
    }


    public ICommand HiddenResultCommand
    {
        get => _hiddenResultCommand;
        set
        {
            _hiddenResultCommand = value;
            OnPropertyChanged(new PropertyChangedEventArgs("HiddenResultCommand"));
        }
    }


    /// <summary>
    /// Evalua si los comandos add, edit, delete, first, previous, next y last pueden ejecutarse.
    /// </summary>
    public virtual void EvaluateCommands()
    {
        foreach (var aux in _commands.Select(command => _addCommand as CommandAps))
        {
            aux?.OnCanExecutedChanged();
        }
    }
}