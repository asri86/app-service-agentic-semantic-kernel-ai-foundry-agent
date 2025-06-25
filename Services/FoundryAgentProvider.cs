using Azure.AI.Agents.Persistent;
using Microsoft.Extensions.Configuration;

namespace CRUDTasksWithAgent.Services
{
    // This provider exists so that the Foundry agent client could be injected directly in Program.cs,
    // but we want to expose IsConfigured so Blazor components can check if the required environment variables are set.
    // This enables showing a friendly message in the UI if the client is not configured.

    // To keep the scenario simple, just create a new thread for each injected provider (scoped to the 
    // browser session). You can manage the threads in the Azure AI Foundry portal, or add thread 
    // management features in your application code.

    public interface IFoundryAgentProvider
    {
        bool IsConfigured { get; } // Indicates if the provider is ready for use
        public PersistentAgent? Agent { get; } // An agent instance
        PersistentAgentsClient? Client { get; } // The agents client
        public string? ThreadId { get; } // The agent thread ID
    }

    public class FoundryAgentProvider : IFoundryAgentProvider
    {
        public bool IsConfigured { get; }
        public PersistentAgentsClient? Client { get; }
        public string? ThreadId { get; }
        public PersistentAgent? Agent { get; }

        public FoundryAgentProvider(IConfiguration config)
        {
            IsConfigured = false;

            // Create a new client instance
            var endpoint = config["AzureAIFoundryProjectEndpoint"];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                return; // Fail gracefully
            }
            Client = new PersistentAgentsClient(endpoint, new Azure.Identity.DefaultAzureCredential());

            // Create a new thread. 
            PersistentAgentThread agentThread = Client.Threads.CreateThread();
            ThreadId = agentThread.Id;

            // Create a new angent instance
            var agentId = config["AzureAIFoundryAgentId"];
            if (string.IsNullOrWhiteSpace(agentId))
            {
                return; // Fail gracefully
            }
            Agent = Client.Administration.GetAgent(agentId);

            IsConfigured = true;
        }
    }
}
