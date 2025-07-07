using System.Collections;
using DotNetEnv;

namespace CleanArch.Web.Api.Common;

public static class EnvironmentInspector
{
    public static void LoadAndPrintAll()
    {
        Env.Load();
        PrintAll();
    }

    public static void PrintAll()
    {
        Console.WriteLine("---- ENVIRONMENT VARIABLES ----");
        foreach (
            DictionaryEntry env in Environment
                .GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .OrderBy(e => e.Key.ToString(), StringComparer.OrdinalIgnoreCase)
        )
        {
            Console.WriteLine($"{env.Key} = {env.Value}");
        }
        Console.WriteLine("--------------------------------");
    }
}
