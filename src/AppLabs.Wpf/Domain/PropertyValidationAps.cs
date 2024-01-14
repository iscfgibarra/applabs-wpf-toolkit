namespace AppLabs.Wpf.Domain;

public class PropertyValidationAps<TBindingModel>
    where TBindingModel : ModelAps<TBindingModel>
{
    private Func<TBindingModel, bool>? _validationCriteria;
    private string _errorMessage;

    public PropertyValidationAps(string propertyName)
    {
        PropertyName = propertyName;
        _validationCriteria = null;
        _errorMessage = string.Empty;
    }

    public PropertyValidationAps<TBindingModel> When(Func<TBindingModel, bool> validationCriteria)
    {
        if (_validationCriteria != null)
            throw new InvalidOperationException("Solo se puede establecer el criterio de validación una vez.");

        _validationCriteria = validationCriteria;
        return this;
    }

    public PropertyValidationAps<TBindingModel> Show(string errorMessage)
    {
        if (_errorMessage != null)
            throw new InvalidOperationException("Sólo se puede establecer el mensage una vez.");

        _errorMessage = errorMessage;
        return this;
    }

    public bool IsInvalid(TBindingModel presentationModel)
    {
        if (_validationCriteria == null)
            throw new InvalidOperationException(
                "No se establecieron criterios de validacion. (Usa el metodo 'When(..)'.)");

        return _validationCriteria(presentationModel);
    }

    public string GetErrorMessage()
    {
        if (_errorMessage == null)
            throw new InvalidOperationException(
                "No se estableceieron errores a mostrar para la validación. (Usa el metodo 'Show(..)'.)");

        return _errorMessage;
    }

    public string PropertyName { get; }
}