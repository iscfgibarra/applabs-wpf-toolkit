using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Data;
using AppLabs.Wpf.Application;
using AppLabs.Wpf.Domain;
using AppLabs.Wpf.Events;

namespace AppLabs.Wpf.Toolkit;

public class ViewAps : INotifyPropertyChanged, INavegableCollectionAps
{
    public static string CleanError { get; set; }


    [DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(
        IntPtr procHandle,
        Int32 min,
        Int32 max);

    /// <summary>
    /// Force clean memory on windows
    /// </summary>
    public static void ClearMemory()
    {
        try
        {
            Process mem = Process.GetCurrentProcess();
            SetProcessWorkingSetSize(mem.Handle, -1, -1);
        }
        catch (Exception ex)
        {
            CleanError = ex.Message;
        }
    }


    /// <summary>
    /// Trabajador auxiliar en el proceso de carga de los datos.
    /// </summary>
    protected BackgroundWorker? BwLoading;

    /// <summary>
    /// Trabajador auxiliar en el proceso de guardado de los datos.
    /// </summary>
    protected BackgroundWorker? BwSaving;


    protected ViewCommandsAps ViewCommandsAps { get; set; }


    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        var handler = PropertyChanged;

        if (handler != null)
        {
            handler(this, e);
        }
    }

    public ViewAps()
    {
        ViewCommandsAps = new ViewCommandsAps();
        ViewCommandsAps.PropertyChanged += (_, e) =>
        {
            OnPropertyChanged(new PropertyChangedEventArgs(e.PropertyName));
        };
    }


    protected ViewStatesAps viewState;

    public ViewStatesAps ViewState
    {
        get => viewState;
        set
        {
            viewState = value;
            switch (viewState)
            {
                case ViewStatesAps.Adding:
                    ControlsReadOnly = false;
                    AddEditDeleteVisibility = Visibility.Collapsed;
                    OkCancelVisibility = Visibility.Visible;
                    IdFieldsReadOnly = false;
                    ListEnabled = false;
                    DetailsVisibility = Visibility.Visible;
                    DialogCommandType = DialogCommandTypeAps.Saving;
                    DialogCommandMessage = "Al terminar puedes guardar el nuevo registro";
                    DialogResultVisibility = Visibility.Collapsed;
                    break;
                case ViewStatesAps.Modifying:
                    ControlsReadOnly = false;
                    AddEditDeleteVisibility = Visibility.Collapsed;
                    OkCancelVisibility = Visibility.Visible;
                    IdFieldsReadOnly = true;
                    ListEnabled = false;
                    DetailsVisibility = Visibility.Visible;
                    DialogCommandType = DialogCommandTypeAps.Saving;
                    DialogCommandMessage = "Al terminar de editar puedes almacenar tus cambios";
                    DialogResultVisibility = Visibility.Collapsed;
                    break;
                case ViewStatesAps.Waiting:
                    ControlsReadOnly = true;
                    AddEditDeleteVisibility = Visibility.Visible;
                    OkCancelVisibility = Visibility.Collapsed;
                    IdFieldsReadOnly = false;
                    ListEnabled = true;
                    DetailsVisibility = Visibility.Collapsed;
                    DialogCommandType = DialogCommandTypeAps.InformationMessage;
                    DialogResultVisibility = Visibility.Collapsed;
                    break;
                case ViewStatesAps.Deleting:
                    ControlsReadOnly = true;
                    AddEditDeleteVisibility = Visibility.Collapsed;
                    OkCancelVisibility = Visibility.Visible;
                    IdFieldsReadOnly = true;
                    ListEnabled = false;
                    DetailsVisibility = Visibility.Visible;
                    DialogCommandType = DialogCommandTypeAps.Deleting;
                    DialogCommandMessage = "¿Estas seguro que deseas eliminar este registro?";
                    DialogResultVisibility = Visibility.Collapsed;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("value",
                        "Al establecer el ViewState no se reconoce el valor como válido.");
            }

            OnPropertyChanged(new PropertyChangedEventArgs("ViewState"));
            OnPropertyChanged(new PropertyChangedEventArgs("ControlsReadOnly"));
            OnPropertyChanged(new PropertyChangedEventArgs("AddEditDeleteVisibility"));
            OnPropertyChanged(new PropertyChangedEventArgs("OkCancelVisibility"));
            OnPropertyChanged(new PropertyChangedEventArgs("IdFieldsReadOnly"));
            OnPropertyChanged(new PropertyChangedEventArgs("DetailsVisibility"));
            OnPropertyChanged(new PropertyChangedEventArgs("ListEnabled"));
            OnPropertyChanged(new PropertyChangedEventArgs("DialogCommandType"));
            OnPropertyChanged(new PropertyChangedEventArgs("DialogCommandMessage"));
            OnPropertyChanged(new PropertyChangedEventArgs("DialogResultVisibility"));
            ViewCommandsAps.EvaluateCommands();
        }
    }

    public bool ControlsReadOnly { get; set; }

    public Visibility OkCancelVisibility { get; set; }

    public Visibility AddEditDeleteVisibility { get; set; }

    public bool IdFieldsReadOnly { get; set; }

    public Visibility DetailsVisibility { get; set; }

    /// <summary>
    /// Permite definir que tipo de Mensaje que
    /// se mostrara para los procesos de agregado, 
    /// edicion y eliminacion.
    /// </summary>
    public DialogCommandTypeAps DialogCommandType { get; set; }

    /// <summary>
    /// Mensaje que se muestra el el cuadro de dialogo definido en 
    /// DialogCommandType
    /// </summary>
    public string DialogCommandMessage { get; set; }


    /// <summary>
    /// Mensaje 
    /// </summary>
    public string ProcessCompletedMessage { get; set; }

    public DialogCommandTypeAps ProcessCompletedCommandType { get; set; }

    public Visibility DialogResultVisibility { get; set; }


    protected void ResultMessage(string message, DialogCommandTypeAps dialogCommandType)
    {
        ProcessCompletedMessage = message;
        ProcessCompletedCommandType = dialogCommandType;
        DialogResultVisibility = Visibility.Visible;
        OnPropertyChanged(new PropertyChangedEventArgs("ProcessCompletedMessage"));
        OnPropertyChanged(new PropertyChangedEventArgs("ProcessCompletedCommandType"));
        OnPropertyChanged(new PropertyChangedEventArgs("DialogResultVisibility"));
    }


    protected void HiddenResult()
    {
        DialogResultVisibility = Visibility.Collapsed;
        OnPropertyChanged(new PropertyChangedEventArgs("DialogResultVisibility"));
    }


    public bool ListEnabled { get; set; }
    private bool _isBusy;

    public bool IsBusy
    {
        get { return _isBusy; }
        set
        {
            _isBusy = value;
            OnPropertyChanged(new PropertyChangedEventArgs("IsBusy"));
        }
    }

    private string _busyMessage;

    public string BusyMessage
    {
        get { return _busyMessage; }
        set
        {
            _busyMessage = value;
            OnPropertyChanged(new PropertyChangedEventArgs("BusyMessage"));
        }
    }

    /// <summary>
    /// Establece IsBusy a verdadero y BusyMessage con el mensaje enviado.
    /// </summary>
    /// <param name="busyMessage"></param>
    public void Busy(string busyMessage)
    {
        IsBusy = true;
        BusyMessage = busyMessage;
    }

    /// <summary>
    /// Establece IsBusy a falso y BusyMessage a vacio.
    /// </summary>
    public void Unoccupied()
    {
        IsBusy = false;
        BusyMessage = string.Empty;
    }


    /// <summary>
    /// Evento que se dispara cuando algun proceso ha sido completado, debe dispararse 
    /// usando OnProcessCompleted.
    /// </summary>
    public event ProcessCompletedEventHandlerAps ProcessCompleted;

    /// <summary>
    /// Dispara el evento ProcessCompleted (Proceso completo), que puede usarse de manera génerica
    /// para notificar a la interfaz que la vista completo alguna tarea o proceso.
    /// </summary>
    /// <param name="tipoDeProceso">Una cadena que describe el tipo de proceso que se ha completado.</param>
    /// <param name="mensajeAlCompletar">El mensaje que enviara (la interfaz) al dispararse este evento.</param>
    public void OnProcessCompleted(string processType, string succeededMessage)
    {
        if (ProcessCompleted != null)
            ProcessCompleted(this, new ProcessCompletedEventArgsAps(processType, succeededMessage));
    }
   


    /// <summary>
    /// Evento que permite controlar el borrado de registros
    /// </summary>
    public event DeletingEventHandler Deleting;

    /// <summary>
    /// Dispara el evento antes de ejecutar el borrado de un registro
    /// </summary>
    public void OnDeleting()
    {
        Deleting(this);
    }

     private CollectionView? _mainCollectionView;
    private CollectionView? _childCollectionView;
    
  
    
    /// <summary>
    /// Permite ver la vista de la coleccion maestra en la vista, para filtrar y manipular.
    /// </summary>      
    public CollectionView? MainCollectionView
    {
        get => _mainCollectionView;
        set
        {
            _mainCollectionView = value;
            OnPropertyChanged(new PropertyChangedEventArgs("MainCollectionView"));
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
        }
    }


    /// <summary>
    /// Mueve  al siguiente objeto disponible en la coleccion maestra.
    /// </summary>
    public virtual void MoveToNext()
    {
        if (_mainCollectionView != null)
        {
            _mainCollectionView.MoveCurrentToNext();
            ViewCommandsAps.EvaluateCommands();
            OnPropertyChanged(new PropertyChangedEventArgs("MainCollectionView"));
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
        }
    }

    /// <summary>
    /// Mueve al objeto anterior disponible en la colección maestra.
    /// </summary>
    public virtual void MoveToPrevious()
    {
        if (_mainCollectionView != null)
        {
            _mainCollectionView.MoveCurrentToPrevious();
            ViewCommandsAps.EvaluateCommands();
            OnPropertyChanged(new PropertyChangedEventArgs("MainCollectionView"));
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
        }
    }

    /// <summary>
    /// Mueve al primer objeto de la coleccion maestra.
    /// </summary>
    public virtual void MoveToFirst()
    {
        if (_mainCollectionView != null)
        {
            _mainCollectionView.MoveCurrentToFirst();
            ViewCommandsAps.EvaluateCommands();
            OnPropertyChanged(new PropertyChangedEventArgs("MainCollectionView"));
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
        }
    }

    /// <summary>
    /// Mueve al ultimo objeto de la colección maestra.
    /// </summary>
    public virtual void MoveToLast()
    {
        if (_mainCollectionView != null)
        {
            _mainCollectionView.MoveCurrentToLast();
            ViewCommandsAps.EvaluateCommands();
            OnPropertyChanged(new PropertyChangedEventArgs("MainCollectionView"));
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
        }
    }

    /// <summary>
    /// Mueve el cursor al objeto especificado ppor la posicion en la coleccion maestra.
    /// </summary>
    /// <param name="index"></param>
    public virtual void MoveToPosition(int index)
    {
        if (_mainCollectionView != null)
        {
            _mainCollectionView.MoveCurrentToPosition(index);
            ViewCommandsAps.EvaluateCommands();
            OnPropertyChanged(new PropertyChangedEventArgs("MainCollectionView"));
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
        }
    }

    public int RowNumber
    {
        set
        {
            OnPropertyChanged(new PropertyChangedEventArgs("RowNumber"));
            ViewCommandsAps.EvaluateCommands();
        }
        get
        {
            if (_mainCollectionView != null) return _mainCollectionView.CurrentPosition + 1;
            return 0;
        }
    }


  

    /// <summary>
    /// Permite ver la vista de la coleccion secundaria en la vista, para filtrar y manipular.
    /// </summary>      
    public CollectionView? ChildCollectionView
    {
        get => _childCollectionView;
        set
        {
            _childCollectionView = value;
            OnPropertyChanged(new PropertyChangedEventArgs("ChildCollectionView"));
        }
    }

    /// <summary>
    /// Mueve  al siguiente objeto disponible en la coleccion secundaria.
    /// </summary>
    public virtual void ChildsMoveToNext()
    {
        if (_childCollectionView != null)
            _childCollectionView.MoveCurrentToNext();
    }

    /// <summary>
    /// Mueve al objeto anterior disponible en la colección secundaria.
    /// </summary>
    public virtual void ChildsMoveToPrevious()
    {
        if (_childCollectionView != null)
            _childCollectionView.MoveCurrentToPrevious();
    }

    /// <summary>
    /// Mueve al primer objeto de la coleccion secuendaria.
    /// </summary>
    public virtual void ChildsMoveToFirst()
    {
        if (_childCollectionView != null)
            _childCollectionView.MoveCurrentToFirst();
    }

    /// <summary>
    /// Mueve al ultimo objeto de la colección maestra.
    /// </summary>
    public virtual void ChildsMoveToLast()
    {
        if (_childCollectionView != null)
            _childCollectionView.MoveCurrentToLast();
    }
}

