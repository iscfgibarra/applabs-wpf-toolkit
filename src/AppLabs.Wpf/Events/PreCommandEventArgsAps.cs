namespace AppLabs.Wpf.Events;

public class PreCommandEventArgsAps : EventArgs
{
    private readonly string _commandType;
    private readonly string _preCommandMessage;

    public string CommandType => _commandType;

    public string PreCommandMessage => _preCommandMessage;


    public PreCommandEventArgsAps(string commandType, string preCommandMessage)
    {
        _commandType = commandType;
        _preCommandMessage = preCommandMessage;
    }
}