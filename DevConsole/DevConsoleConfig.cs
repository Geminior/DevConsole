using DevConsole.Infrastructure.Commands;

namespace DevConsole;

public class DevConsoleConfig
{
    public string AzureDevOpsOrganizationName => "HeadFirstGames";

    public string AzureDevOpsProjectName => "Balrog";

    public string AzureDevOpsOrganizationUri => $"https://dev.azure.com/{AzureDevOpsOrganizationName}";

    public string AzureTenant => throw new UserActionException("Please set AzureTenant in DevConsole/DevConsoleConfig.cs");
    public string AzureSubscription => throw new UserActionException("Please set AzureSubscription in DevConsole/DevConsoleConfig.cs");
}