using System.IO;

internal static class ModuleInitializer
{
	public static string BaseDirectory { get; private set; }

	public static void Initialize()
	{
		BaseDirectory = Directory.GetCurrentDirectory();
	}
}