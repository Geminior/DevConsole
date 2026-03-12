using DevConsole.Infrastructure.Commands;
using DevConsole.Infrastructure.Models;
using DevConsole.Infrastructure.Output;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DevConsole.Infrastructure;

public static class ProcessHelper
{
    private static readonly string[] DirectCommandWhitelist =
    {
        "git",
        "helm",
        "kubectl",
        "docker",
        "docker-compose",
        "wsl"
    };

    public static ProcessResult GetOutput(string command, bool outputCommand = false, string? workingDirectory = null,
                                          bool expectExitCodeToBeZero = true, string? input = null,
                                          Dictionary<string, string?>? environmentVariables = null)
    {
        if (outputCommand)
        {
            ColorConsole.WriteCommand(command);
        }

        ProcessStart:
        var process = new Process
        {
            StartInfo = BuildProcessStartInfo(command, workingDirectory, environmentVariables: environmentVariables)
        };

        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        if (input is not null)
        {
            process.StartInfo.RedirectStandardInput = true;
        }

        var error = string.Empty;
        process.ErrorDataReceived += (_, e) => error += e.Data;
        if (!process.Start())
        {
            throw new InvalidOperationException($"Failed to start process with command {command}");
        }

        if (input is not null)
        {
            process.StandardInput.WriteLine(input);
        }

        process.BeginErrorReadLine();

        if (!TryReadOutput(out var output))
        {
            // ColorConsole.WriteLine(command, ConsoleColor.DarkGray);
            ColorConsole.WriteLine($"Output read timed out, killing process and retrying...", ConsoleColor.Yellow);
            process.WaitForExit(1000);
            process.Kill();
            goto ProcessStart;
        }

        process.WaitForExit();

        if (expectExitCodeToBeZero)
        {
            ThrowOnErrorExitCode(command, process, output, error);
        }

        return new ProcessResult(command, output, error, process.ExitCode);

        bool TryReadOutput(out string result)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                result = process.StandardOutput.ReadToEndAsync(cancellationTokenSource.Token)
                                .Result.Trim();

                return true;
            }
            catch (TaskCanceledException)
            {
                result = string.Empty;
                return false;
            }
            catch (AggregateException age) when (age.InnerException is TaskCanceledException)
            {
                result = string.Empty;
                return false;
            }
        }
    }

    public static JsonProcessResult<T> GetJsonOutput<T>(string command,
                                                        bool outputCommand = false,
                                                        string? workingDirectory = null,
                                                        bool expectExitCodeToBeZero = true,
                                                        bool ignoreError = false,
                                                        bool seekValidJsonStart = false) where T : class
    {
        var untypedResult = GetOutput(command, outputCommand, workingDirectory, expectExitCodeToBeZero);
        var json = untypedResult.Output;
        if (seekValidJsonStart)
        {
            json = json[Math.Max(0, json.IndexOf('{'))..];
        }

        var output = default(T?);
        if (untypedResult.ExitCode == 0 || untypedResult.Error.Length == 0 || ignoreError)
        {
            output = JsonConvert.DeserializeObject<T>(json);
        }

        return new JsonProcessResult<T>(output, untypedResult.Error);
    }

    public static int Run(string command, string? workingDirectory = null, bool outputCommand = true,
                          bool expectExitCodeToBeZero = true,
                          Dictionary<string, string?>? environmentVariables = null, IProcessOutputHandler? outputHandler = null)
    {
        int DoRun()
        {
            if (outputCommand)
            {
                ColorConsole.WriteCommand(command);
            }

            var startInfo =
                BuildProcessStartInfo(command, workingDirectory, environmentVariables: environmentVariables);

            outputHandler?.Prepare(startInfo);

            var process = Process.Start(startInfo) ??
                          throw new InvalidOperationException($"Failed to start process with command {command}");

            outputHandler?.Handle(process);

            process.WaitForExit();

            if (expectExitCodeToBeZero)
            {
                ThrowOnErrorExitCode(command, process);
            }

            return process.ExitCode;
        }

        return DoRun();
    }

    private static void ThrowOnErrorExitCode(string command, Process process, string? unprintedOutput = null,
                                             string? unprintedError = null)
    {
        if (process.ExitCode <= 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(unprintedOutput))
        {
            Console.WriteLine(unprintedOutput);
        }

        if (!string.IsNullOrWhiteSpace(unprintedError))
        {
            Console.WriteLine(unprintedError);
        }

        throw new ExitCodeException(command, process.ExitCode);
    }

    private static ProcessStartInfo BuildProcessStartInfo(string command, string? workingDirectory = null,
                                                          bool newWindow = false, Dictionary<string, string?>? environmentVariables = null)
    {
        var processStartInfo = GetDirectProcessStartInfo(command, newWindow);

        if (processStartInfo is null)
        {
            var shell = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "powershell" : "pwsh";

            var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command));
            processStartInfo = new ProcessStartInfo(shell, $"-NoProfile -EncodedCommand {encoded}")
            {
                UseShellExecute = newWindow
            };
        }

        if (workingDirectory is not null)
        {
            processStartInfo.WorkingDirectory = workingDirectory;
        }

        if (environmentVariables is not null)
        {
            foreach (var (key, value) in environmentVariables)
            {
                processStartInfo.EnvironmentVariables[key] = value;
            }
        }

        return processStartInfo;
    }

    private static ProcessStartInfo? GetDirectProcessStartInfo(string command, bool newWindow)
    {
        if (command.Contains("|") || command.Contains("$") || command.Contains("&"))

            // We cannot do piping or variable expansion in direct commands.
        {
            return null;
        }

        var cmd = command.Split(" ").First();
        var args = string.Join(" ", command.Split(" ").Skip(1));

        if (DirectCommandWhitelist.Contains(cmd))
        {
            return new ProcessStartInfo(cmd, args)
            {
                UseShellExecute = newWindow
            };
        }

        return null;
    }

    public static Process? Start(string command, string? workingDirectory = null, bool newWindow = false,
                                 string? windowTitle = null)
    {
        if (newWindow && !string.IsNullOrWhiteSpace(windowTitle))
        {
            command = $"$host.UI.RawUI.WindowTitle = \"{windowTitle}\"; {command}";
        }

        ColorConsole.WriteCommand(command);

        var startInfo = BuildProcessStartInfo(command, workingDirectory, newWindow);

        return Process.Start(startInfo);
    }
}