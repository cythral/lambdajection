using System.Reflection;

using AutoFixture.Kernel;

internal class TypeOmitter<TType> : ISpecimenBuilder
{
    public object Create(object request, ISpecimenContext context)
    {
        return request switch
        {
            PropertyInfo propInfo => propInfo.PropertyType == typeof(TType) ? new OmitSpecimen() : new NoSpecimen(),
            TypeInfo typeInfo => typeInfo.AsType() == typeof(TType) ? new OmitSpecimen() : new NoSpecimen(),
            _ => new NoSpecimen(),
        };
    }
}
