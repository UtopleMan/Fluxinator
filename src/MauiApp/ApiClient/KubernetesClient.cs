using Fluxinator.Model;
using Json.Patch;
using k8s;
using k8s.Models;
using System.Text.Json;

namespace Fluxinator.ApiClient;
public class KubernetesDeploymentClient : IDeploymentClient
{
    private Dictionary<string, GenericClient> genericClients = new();
    private Dictionary<string, Kubernetes> clients = new();
    private const string Kind = "Kustomization";
    private const string Group = "kustomize.toolkit.fluxcd.io";
    private const string Version = "v1beta2";
    private const string PluralName = "kustomizations";
    private const string ReconciliationRequested = "reconcile.fluxcd.io/requestedAt";

    private void Initialize()
    {
        var config = KubernetesClientConfiguration.LoadKubeConfig();
        foreach (var context in config.Contexts)
        {
            var kubernetesConfig = KubernetesClientConfiguration.BuildConfigFromConfigObject(config, context.Name);
            var client = new Kubernetes(kubernetesConfig);
            var genericClient = new GenericClient(client, Group, Version, PluralName);
            clients.Add(context.Name, client);
            genericClients.Add(context.Name, genericClient);
        }
    }
    public async Task<IEnumerable<string>> GetNamespaces(string context)
    {
        if (!clients.Any()) Initialize();
        var result = await clients[context].CoreV1.ListNamespaceAsync(timeoutSeconds: 10);
        return result.Items.Select(x => x.Metadata.Name);
    }
    public async Task<IEnumerable<Deployment>> GetDeployments(string context, string namespaceName)
    {
        if (!clients.Any()) Initialize();
        var result = await clients[context].CoreV1.ListNamespaceAsync(timeoutSeconds: 10);
        var namespaceItem = result.Items.Single(x => x.Metadata.Name == namespaceName);
        var customResources = new CustomResourceList<KustomizationResource>();
        if (namespaceName != null)
            customResources = await genericClients[context].ListNamespacedAsync<CustomResourceList<KustomizationResource>>(namespaceName).ConfigureAwait(false);
        else
            customResources = await genericClients[context].ListAsync<CustomResourceList<KustomizationResource>>().ConfigureAwait(false);
        return customResources.Items.Select(x => new Deployment(x.Metadata.Namespace(), x.Metadata.Name, 
            GetState(x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Reason, x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Message),
            x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Message,
            x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Reason, 
            x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Timestamp, 
            x.Status.Conditions.Select(y => new Run(y.Message, y.Reason, y.Timestamp)))).OrderBy(x => x.Name);
    }
    private DeploymentStates GetState(string reason, string message)
    {
        if (reason == "ReconciliationSucceeded")
            return DeploymentStates.Success;
        else if (reason == "Progressing")
            return DeploymentStates.Running;
        else if (message.Contains("' is not ready"))
            return DeploymentStates.Waiting;
        else
            return DeploymentStates.Failed;
    }
    public async Task Redeploy(string context, string namespaceName, string name)
    {
        if (!clients.Any()) Initialize();
        KustomizationResource resource;
        if (namespaceName != null)
            resource = await genericClients[context].ReadNamespacedAsync<KustomizationResource>(namespaceName, name).ConfigureAwait(false);
        else
            resource = await genericClients[context].ReadAsync<KustomizationResource>(name).ConfigureAwait(false);

        var current = JsonSerializer.SerializeToDocument(resource);
        if (resource.Metadata.Annotations == null)
            resource.Metadata.Annotations = new Dictionary<string, string>();

        if (resource.Metadata.Annotations.ContainsKey(ReconciliationRequested))
            resource.Metadata.Annotations.Remove(ReconciliationRequested);
        resource.Metadata.Annotations.Add(ReconciliationRequested, DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffff0Z"));
        var next = JsonSerializer.SerializeToDocument(resource);
        var patch = current.CreatePatch(next);

        var crPatch = new V1Patch(patch, V1Patch.PatchType.JsonPatch);

        if (namespaceName != null)
            await clients[context].CustomObjects.PatchNamespacedCustomObjectAsync(crPatch, Group, Version, namespaceName, PluralName, resource.Metadata.Name).ConfigureAwait(false);
        else
            await clients[context].CustomObjects.PatchClusterCustomObjectAsync(crPatch, Group, Version, PluralName, resource.Metadata.Name).ConfigureAwait(false);
    }

    public IEnumerable<string> GetContexts()
    {
        if (!clients.Any()) Initialize();
        return clients.Keys.ToList();
    }
}
