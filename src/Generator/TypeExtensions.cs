using System;

internal static class TypeExtensions
{
    public static TReturnType? GetPublicProperty<TReturnType>(this object @this, string propertyName)
    {
        var property = @this.GetType().GetProperty(propertyName);
        var propertyGetter = property.GetGetMethod();
        return (TReturnType?)propertyGetter.Invoke(@this, Array.Empty<object>());
    }
}
