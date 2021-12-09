using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
        fixture.Register(() => new JsonSerializerOptions());
        fixture.Customize(new AutoNSubstituteCustomization());
        fixture.Customizations.Insert(-1, new TargetRelay());
        fixture.Customizations.Add(new TypeOmitter<IDictionary<string, JsonElement>>());
        fixture.Customizations.Add(new TypeOmitter<JsonConverter>());
        customize(fixture);
        return fixture;
    }
}
