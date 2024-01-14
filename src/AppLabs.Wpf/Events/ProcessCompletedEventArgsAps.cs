namespace AppLabs.Wpf.Events;

public class ProcessCompletedEventArgsAps : EventArgs
{
    private readonly string _processType;
    private readonly string _succeededMessage;

    public string ProcessType => _processType;

    public string SuccededMessage => _succeededMessage;

    public ProcessCompletedEventArgsAps(string processType, string succeededMessage)
    {
        _processType = processType;
        _succeededMessage = succeededMessage;
    }
   
}