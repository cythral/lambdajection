using System;
using System.Reflection;

using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.NUnit3;

internal class AutoAttribute : AutoDataAttribute
{
    public AutoAttribute(Type? customizer = null)
        : base(() => Create(fixture => customizer?.GetMethod("Customize", BindingFlags.Static | BindingFlags.Public)!.Invoke(null, new object[] { fixture })))
    {
    }

    public static IFixture Create(Action<Fixture> customize)
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoNSubstituteCustomization());
        fixture.Customizations.Insert(-1, new TargetRelay());
        customize(fixture);
        return fixture;
    }
}
