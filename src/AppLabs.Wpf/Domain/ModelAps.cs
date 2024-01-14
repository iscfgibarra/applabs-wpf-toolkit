using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace AppLabs.Wpf.Domain;

public abstract class ModelAps<TBindingModel> : INotifyPropertyChanged, INotifyDataErrorInfo
    where TBindingModel : ModelAps<TBindingModel>
{
    private readonly List<PropertyValidationAps<TBindingModel>> _validations =
        new List<PropertyValidationAps<TBindingModel>>();

    private Dictionary<string, List<string>> _errorMessages = new Dictionary<string, List<string>>();

    protected ModelAps()
    {
        PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "HasErrors") return;
            if (e.PropertyName != null)
                ValidateProperty(e.PropertyName);
        };
    }

    #region INotifyDataErrorInfo

    public IEnumerable GetErrors(string? propertyName)
    {
        if (propertyName != null && _errorMessages.TryGetValue(propertyName, out var errors))
            return errors;
        
        return Array.Empty<string>();
    }

    [Display(AutoGenerateField = false)]
    public bool HasErrors => _errorMessages.Count > 0;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged = delegate { };

    public void OnErrorsChanged(string propertyName)
    {
        if (ErrorsChanged != null)
            ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged = delegate { };

    protected void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    protected void OnPropertyChanged(Expression<Func<object>> expression)
    {
        OnPropertyChanged(GetPropertyName(expression));
    }

    protected void OnCurrentPropertyChanged()
    {
        string? methodName = string.Empty;

        StackTrace stackTrace = new StackTrace(); // get call stack
        StackFrame[] stackFrames = stackTrace.GetFrames(); // get method calls (frames)

        if (stackFrames.Length > 1)
        {
            methodName = stackFrames[1].GetMethod()?.Name;
        }


        if (methodName == null || (!methodName.Contains("set_") && !methodName.Contains("SET_"))) return;
        
        if (!methodName.StartsWith("set_", StringComparison.OrdinalIgnoreCase))
            throw new NotSupportedException(methodName +
                                            " OnCurrentPropertyChanged puede solo ser invocado una vez el setter de la propiedad.");

        string propertyName = methodName.Substring(4);
        OnPropertyChanged(propertyName);
    }

    protected PropertyValidationAps<TBindingModel> AddValidationFor(Expression<Func<object>> expression)
    {
        return AddValidationFor(GetPropertyName(expression));
    }

    protected PropertyValidationAps<TBindingModel> AddValidationFor(string propertyName)
    {
        var validation = new PropertyValidationAps<TBindingModel>(propertyName);
        _validations.Add(validation);

        return validation;
    }

    protected void AddAllAttributeValidators()
    {
        PropertyInfo[] propertyInfos = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo propertyInfo in propertyInfos)
        {
            Attribute[] custom = Attribute.GetCustomAttributes(propertyInfo, typeof(ValidationAttribute), true);
            foreach (var attribute in custom)
            {
                var property = propertyInfo;
                var validationAttribute = attribute as ValidationAttribute;

                if (validationAttribute == null)
                    throw new NotSupportedException(
                        "validationAttribute variable should be inherited from ValidationAttribute type");

                string? name = property.Name;

                var displayAttribute =
                    Attribute.GetCustomAttributes(propertyInfo, typeof(DisplayAttribute)).FirstOrDefault() as
                        DisplayAttribute;
                if (displayAttribute != null)
                {
                    name = displayAttribute.GetName();
                }

                if (name != null)
                {
                    var message = validationAttribute.FormatErrorMessage(name);

                    AddValidationFor(propertyInfo.Name)
                        .When(_ =>
                        {
                            var value = property.GetGetMethod()?.Invoke(this, new object[] { });
                            var result = validationAttribute.GetValidationResult(value,
                                new ValidationContext(this, null, null) { MemberName = property.Name });
                            return result != ValidationResult.Success;
                        })
                        .Show(message);
                }
            }
        }
    }

    public void ValidateAll()
    {
        var propertyNamesWithValidationErrors = _errorMessages.Keys;

        _errorMessages = new Dictionary<string, List<string>>();

        _validations.ForEach(PerformValidation);

        var propertyNamesThatMightHaveChangedValidation =
            _errorMessages.Keys.Union(propertyNamesWithValidationErrors).ToList();

        propertyNamesThatMightHaveChangedValidation.ForEach(OnErrorsChanged);

        OnPropertyChanged(() => HasErrors);
    }

    public void ValidateProperty(Expression<Func<object>> expression)
    {
        ValidateProperty(GetPropertyName(expression));
    }

    private void ValidateProperty(string propertyName)
    {
        _errorMessages.Remove(propertyName);

        _validations.Where(v => v.PropertyName == propertyName).ToList().ForEach(PerformValidation);
        OnErrorsChanged(propertyName);
        OnPropertyChanged(() => HasErrors);
    }

    private void PerformValidation(PropertyValidationAps<TBindingModel> validationAps)
    {
        if (validationAps.IsInvalid((TBindingModel)this))
        {
            AddErrorMessageForProperty(validationAps.PropertyName, validationAps.GetErrorMessage());
        }
    }

    public void AddErrorMessageForProperty(string propertyName, string errorMessage)
    {
        if (_errorMessages.ContainsKey(propertyName))
        {
            _errorMessages[propertyName].Add(errorMessage);
            OnErrorsChanged(propertyName);
        }
        else
        {
            _errorMessages.Add(propertyName, new List<string> { errorMessage });
            OnErrorsChanged(propertyName);
        }
    }


    public void RemoveErrorMessageForProperty(string propertyName, string errorMessage)
    {
        if (_errorMessages.ContainsKey(propertyName))
        {
            _errorMessages[propertyName].Remove(errorMessage);
            OnErrorsChanged(propertyName);
        }
    }


    public void RemoveErrorMessagesForProperty(string propertyName)
    {
        if (_errorMessages.ContainsKey(propertyName))
        {
            _errorMessages.Remove(propertyName);
        }
    }

    public void ClearAllErrorValidation()
    {
        _errorMessages.Clear();
    }

    private static string GetPropertyName(Expression<Func<object>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        MemberExpression? memberExpression;

        var unaryExpression = expression.Body as UnaryExpression;
        if (unaryExpression is { NodeType: ExpressionType.Convert })
            memberExpression = unaryExpression.Operand as MemberExpression;
        else
            memberExpression = expression.Body as MemberExpression;

        if (memberExpression == null)
            throw new ArgumentException("The expression is not a member access expression", "expression");

        var property = memberExpression.Member as PropertyInfo;
        if (property == null)
            throw new ArgumentException("The member access expression does not access a property", "expression");

        var getMethod = property.GetGetMethod(true);
        if (getMethod != null && getMethod.IsStatic)
            throw new ArgumentException("The referenced property is a static property", "expression");

        return memberExpression.Member.Name;
    }
}