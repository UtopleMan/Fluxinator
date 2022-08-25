using Fluxinator.Model;

namespace Fluxinator.ApiClient;

public interface IDeploymentClient
{
    IEnumerable<string> GetContexts();
    Task<IEnumerable<Deployment>> GetDeployments(string context, string namespaceName);
    Task<IEnumerable<string>> GetNamespaces(string context);
    Task Redeploy(string context, string namespaceName, string name);
}
