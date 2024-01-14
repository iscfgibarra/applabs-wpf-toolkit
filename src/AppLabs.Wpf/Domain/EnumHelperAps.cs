using System.ComponentModel;

namespace AppLabs.Wpf.Domain;


/// <summary>
/// <para>Clase de ayuda para desplegar valores de enumeraciones.</para>
/// <para>Debe usarse junto con el atributo  [Description("Ejemplo")] en la enumeracion </para> 
/// <para> que funcione correctamente.</para>
/// </summary>    
public static class EnumHelperAps
{
    /// <summary>
    /// Regresa la descripcion del valor enumerado.
    /// </summary>
    /// <param name="eValue">Valor de enumeración.</param>
    /// <returns></returns>
    public static string? Description(this Enum eValue)
    {
        var nAttributes = eValue.GetType().GetField(eValue.ToString())?.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (nAttributes is { Length: 0 })
            return eValue.ToString();

        if (nAttributes?[0] is DescriptionAttribute descriptionAttribute)
            return descriptionAttribute.Description;
        
        
        return null;
    }

    /// <summary>
    /// Recorre la enumeracion para traer sus valores y descripciones.
    /// </summary>
    /// <typeparam name="TEnum">Enumeracion</typeparam>
    /// <returns>Lista de <see cref="ValueDescriptionAps"/> ValueDescription que contiene Valor de Enumeracion y Descripcion.</returns>
    public static IEnumerable<ValueDescriptionAps> GetAllValuesAndDescriptions<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
    {
        if (!typeof(TEnum).IsEnum)
            throw new ArgumentException("TEnum debe ser un tipo de enumeración.");

        return from e in Enum.GetValues(typeof(TEnum)).Cast<Enum>()
            select new ValueDescriptionAps() { Value = e, Description = e.Description() };
    }
}