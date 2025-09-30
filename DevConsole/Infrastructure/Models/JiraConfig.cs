using DevConsole.Infrastructure.Services;
using System.Text;

namespace DevConsole.Infrastructure.Models;

public class JiraConfig
{
    private const string SomewhatSecret = "sdf¤WE44R3w&trwGF//AF2ASF";
    private const string LocalStorageFileName = "JiraConfig.json";

    public string BaseUrl { get; init; } = "https://headfirstgames.atlassian.net";

    public required string UserEmail { get; init; }

    public required string ApiToken { get; init; }

    public required string? BranchPrefix { get; init; }

    public static Result<JiraConfig> GetJiraConfig()
    {
        var result = LocalStorageProvider.Load(LocalStorageFileName);
        if (!result.Successful)
        {
            ColorConsole.WriteFailure("No Jira config found. Please run 'dev jira-setup' to set up Jira access.");
            return Result<JiraConfig>.Fail();
        }

        return CryptoUtility.Decrypt<JiraConfig>(result.Value, SomewhatSecret);
    }

    public static bool SaveJiraConfig(JiraConfig cfg)
    {
        var json = CryptoUtility.Encrypt(cfg, SomewhatSecret);
        return LocalStorageProvider.Save(Encoding.UTF8.GetBytes(json), LocalStorageFileName);
    }
}