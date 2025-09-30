using Newtonsoft.Json;

namespace DevConsole.Infrastructure.Models;

public class JiraIssue([JsonProperty("summary")] string summary)
{
    public string Summary { get; } = summary;
}