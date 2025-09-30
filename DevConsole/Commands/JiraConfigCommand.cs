using DevConsole.Infrastructure;
using DevConsole.Infrastructure.Commands;
using DevConsole.Infrastructure.Models;
using DevConsole.Infrastructure.Services;
using DevConsole.Infrastructure.Services.AzureDevOps;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.Linq;
using System.Text.RegularExpressions;

namespace DevConsole.Commands;

public sealed class JiraConfigCommand : DevConsoleCommand
{
    private readonly Prompts _promptService;

    public JiraConfigCommand(Prompts promptService)
        : base("jira-setup", "Sets up Jira credentials.")
    {
        _promptService = promptService;

        Handler = CommandHandler.Create(DoCommand);
    }

    private void DoCommand()
    {
        var email = _promptService.Input<string>("Enter your email address used for Jira login");
        var token = _promptService.Input<string>("Enter your Jira API token (you can create one at https://id.atlassian.com/manage/api-tokens)");
        var cfg = new JiraConfig
        {
            UserEmail = email,
            ApiToken = token
        };

        var testResult = GetOutput(
            $"acli --server {cfg.BaseUrl} " +
            $"--user {cfg.UserEmail} " +
            $"--token {cfg.ApiToken} " +
            $"--action getUser " +
            $"--userId {cfg.UserEmail}", expectExitCodeToBeZero: false);

        if (testResult.ExitCode != 0)
        {
            ColorConsole.WriteFailure($"Jira Setup failed, ensure you have entered the correct credentials ({testResult.Error})");
            return;
        }

        if (JiraConfig.SaveJiraConfig(cfg))
        {
            ColorConsole.WriteLine("Jira configuration saved successfully.", ConsoleColor.Green);
        }
    }
}