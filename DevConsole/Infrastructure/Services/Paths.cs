using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DevConsole.Infrastructure.Services;

public class Paths : IPaths
{
    private static readonly Lazy<string> ToolsRootDirectory = new(() =>
    {
        var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                                          throw new InvalidOperationException());

        string metaRootDirectory = null!;
        while (directory is not null)
        {
            if (directory.EnumerateFiles("DevConsole.sln").Any())
            {
                metaRootDirectory = directory.Parent?.FullName ?? throw new InvalidOperationException("Unable to find tools root dir");
                break;
            }

            directory = directory.Parent;
        }

        return metaRootDirectory;
    });

    public string DevConsoleFolder => Path.Combine(ToolsRootDirectory.Value, "DevConsole");
}