using Fluxinator.Model;

namespace Fluxinator.ApiClient;

public interface IDeploymentClient
{
    Task<IEnumerable<Deployment>> GetDeployments(string namespaceName);
    Task<IEnumerable<string>> GetNamespaces();
    Task Redeploy(string namespaceName, string name);
    void Initialize();
}
