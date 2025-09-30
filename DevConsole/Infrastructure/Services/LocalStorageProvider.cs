using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DevConsole.Infrastructure.Services;

public static class LocalStorageProvider
{
    private static readonly string StorageFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DevConsole");

    public static bool Save(byte[] data, string fileName)
    {
        Directory.CreateDirectory(StorageFolder);
        var path = Path.Combine(StorageFolder, fileName);

        try
        {
            File.WriteAllBytes(path, data);
            return true;
        }
        catch (Exception e)
        {
            ColorConsole.WriteFailure(e.Message);
        }

        return false;
    }

    public static Result<byte[], StorageState> Load(string fileName)
    {
        var path = Path.Combine(StorageFolder, fileName);

        try
        {
            if (!File.Exists(path))
            {
                return StorageState.FileNotFound;
            }

            return File.ReadAllBytes(path);
        }
        catch (Exception e)
        {
            ColorConsole.WriteFailure(e.Message);
            return StorageState.Error;
        }
    }
}