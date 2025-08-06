using System;

namespace SemanticCode.Services;

public class AgentNotificationService
{
    private static AgentNotificationService? _instance;
    public static AgentNotificationService Instance => _instance ??= new AgentNotificationService();

    public event EventHandler? AgentInstalled;

    public void NotifyAgentInstalled()
    {
        AgentInstalled?.Invoke(this, EventArgs.Empty);
    }
}