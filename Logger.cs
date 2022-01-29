using System;

namespace FileSignature
{
	internal static class Logger
	{
		public static void Log(Exception e)
		{
			Console.WriteLine($"Message: {e.Message}");
			Console.WriteLine("Stack trace:");
			Console.WriteLine(e.StackTrace);
		}
	}
}
