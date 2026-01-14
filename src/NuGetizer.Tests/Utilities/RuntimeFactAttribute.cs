using System.Runtime.InteropServices;
using Xunit;

namespace NuGetizer.Tests;

public class RuntimeFactAttribute : FactAttribute
{
    /// <summary>
    /// Use <c>nameof(OSPlatform.Windows|Linux|OSX|FreeBSD)</c>
    /// </summary>
    public RuntimeFactAttribute(string osPlatform)
    {
        if (osPlatform != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Create(osPlatform)))
            Skip = $"Only running on {osPlatform}.";
    }

    public RuntimeFactAttribute(Architecture architecture)
    {
        if (RuntimeInformation.ProcessArchitecture != architecture)
            Skip = $"Requires {architecture} but was {RuntimeInformation.ProcessArchitecture}.";
    }

    /// <summary>
    /// Empty constructor for use in combination with RuntimeIdentifier property.
    /// </summary>
    public RuntimeFactAttribute() { }

    /// <summary>
    /// Sets the runtime identifier the test requires to run.
    /// </summary>
    public string? RuntimeIdentifier
    {
        get => RuntimeInformation.RuntimeIdentifier;
        set
        {
            if (value != null && RuntimeInformation.RuntimeIdentifier != value)
                Skip += $"Requires {value} but was {RuntimeInformation.RuntimeIdentifier}.";
        }
    }
}

public class RuntimeTheoryAttribute : TheoryAttribute
{
    /// <summary>
    /// Use <c>nameof(OSPlatform.Windows|Linux|OSX|FreeBSD)</c>
    /// </summary>
    public RuntimeTheoryAttribute(string osPlatform)
    {
        if (osPlatform != null && !RuntimeInformation.IsOSPlatform(OSPlatform.Create(osPlatform)))
            Skip = $"Only running on {osPlatform}.";
    }

    public RuntimeTheoryAttribute(Architecture architecture)
    {
        if (RuntimeInformation.ProcessArchitecture != architecture)
            Skip = $"Requires {architecture} but was {RuntimeInformation.ProcessArchitecture}.";
    }

    /// <summary>
    /// Empty constructor for use in combination with RuntimeIdentifier property.
    /// </summary>
    public RuntimeTheoryAttribute() { }

    /// <summary>
    /// Sets the runtime identifier the test requires to run.
    /// </summary>
    public string? RuntimeIdentifier
    {
        get => RuntimeInformation.RuntimeIdentifier;
        set
        {
            if (value != null && RuntimeInformation.RuntimeIdentifier != value)
                Skip += $"Requires {value} but was {RuntimeInformation.RuntimeIdentifier}.";
        }
    }
}
