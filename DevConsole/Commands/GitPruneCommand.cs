using DevConsole.Infrastructure;
using DevConsole.Infrastructure.Commands;
using DevConsole.Infrastructure.Models;
using DevConsole.Infrastructure.Services;
using DevConsole.Infrastructure.Services.AzureDevOps;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevConsole.Commands;

public sealed class GitPruneCommand : DevConsoleCommand
{
    private readonly Prompts _promptService;

    public GitPruneCommand(Prompts promptService)
        : base("git-prune", "Removes all local feature branches for which no remote exists.")
    {
        _promptService = promptService;
        Handler = CommandHandler.Create(DoCommand);

        AddOption(new Option<bool>(["-ar", "--allow-root"], "Allow deletion of the root branches (not limited to /feature/*)"));

        AddAlias("gp");
    }

    private void DoCommand(bool allowRoot)
    {
        //Get remote branches
        RunSupressed("git fetch --prune");

        //Get branches
        var currentBranch = GetOutput("git branch --show-current").Output.Trim();
        var localBranches = GetOutput("git branch -l").Output.Split('\n').Select(b => b.Trim()).ToHashSet();
        var remoteBranches = GetOutput("git branch -l -r").Output.Split('\n').Select(b => b.Replace("origin/", string.Empty).Trim()).ToArray();

        var branchesToDelete = localBranches
                               .Where(b => !remoteBranches.Contains(b)
                                           && (allowRoot || b.StartsWith(CreateBranchCommand.FeaturePrefix, StringComparison.OrdinalIgnoreCase))
                                           && !b.Contains(currentBranch, StringComparison.OrdinalIgnoreCase))
                               .ToArray();

        if (branchesToDelete.Length == 0)
        {
            ColorConsole.WriteLine("No branches to delete", ConsoleColor.Yellow);
            return;
        }

        ColorConsole.Write("The following branches will be deleted:\n", ConsoleColor.Blue);
        ColorConsole.WriteLine(string.Join('\n', branchesToDelete), ConsoleColor.Green);
        if (!_promptService.Confirm("Do you want to continue?", false))
        {
            ColorConsole.WriteLine("Clean-up aborted", ConsoleColor.Yellow);
            return;
        }

        Run("git branch -D " + string.Join(' ', branchesToDelete));
        ColorConsole.WriteLine("Clean-up complete", ConsoleColor.Green);
    }
}