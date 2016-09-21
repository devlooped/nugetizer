using System.IO;

/// <summary>
/// This class is consumed by the Fody.ModuleInit and invokes the static Initialize 
/// ONCE for the entire run, ensuring that within the AppDomain, the current/base 
/// directory isn't changed by any code running directly or indirectly by the tests.
/// (i.e. running MSBuild tests/builds sometimes changes the base directory)
/// </summary>
internal static class ModuleInitializer
{
	public static string BaseDirectory { get; private set; }

	public static void Initialize()
	{
		BaseDirectory = Directory.GetCurrentDirectory();
	}
}