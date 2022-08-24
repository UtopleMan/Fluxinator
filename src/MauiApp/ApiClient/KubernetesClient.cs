using Fluxinator.Model;
using Json.Patch;
using k8s;
using k8s.Models;
using System.Text.Json;

namespace Fluxinator.ApiClient;
public class KubernetesDeploymentClient : IDeploymentClient
{
    private Kubernetes client;
    private GenericClient genericClient;
    private const string Kind = "Kustomization";
    private const string Group = "kustomize.toolkit.fluxcd.io";
    private const string Version = "v1beta2";
    private const string PluralName = "kustomizations";
    private const string ReconciliationRequested = "reconcile.fluxcd.io/requestedAt";

    public void Initialize()
    {
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
        client = new Kubernetes(config);
        genericClient = new GenericClient(client, Group, Version, PluralName);
    }
    public async Task<IEnumerable<string>> GetNamespaces()
    {
        if (client == null) Initialize();
        var result = await client.CoreV1.ListNamespaceAsync(timeoutSeconds: 10);
        return result.Items.Select(x => x.Metadata.Name);
    }
    public async Task<IEnumerable<Deployment>> GetDeployments(string namespaceName)
    {
        if (client == null) Initialize();
        var result = await client.CoreV1.ListNamespaceAsync(timeoutSeconds: 10);
        var namespaceItem = result.Items.Single(x => x.Metadata.Name == namespaceName);
        var customResources = new CustomResourceList<KustomizationResource>();
        if (namespaceName != null)
            customResources = await genericClient.ListNamespacedAsync<CustomResourceList<KustomizationResource>>(namespaceName).ConfigureAwait(false);
        else
            customResources = await genericClient.ListAsync<CustomResourceList<KustomizationResource>>().ConfigureAwait(false);
        return customResources.Items.Select(x => new Deployment(x.Metadata.Namespace(), x.Metadata.Name,
            x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Message,
            x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Reason, 
            x.Status.Conditions.OrderByDescending(y => y.Timestamp).First().Timestamp, 
            x.Status.Conditions.Select(y => new Run(y.Message, y.Reason, y.Timestamp))));
    }
    public async Task Redeploy(string namespaceName, string name)
    {
        if (client == null) Initialize();
        KustomizationResource resource;
        if (namespaceName != null)
            resource = await genericClient.ReadNamespacedAsync<KustomizationResource>(namespaceName, name).ConfigureAwait(false);
        else
            resource = await genericClient.ReadAsync<KustomizationResource>(name).ConfigureAwait(false);

        var current = JsonSerializer.SerializeToDocument(resource);
        if (resource.Metadata.Annotations.ContainsKey(ReconciliationRequested))
            resource.Metadata.Annotations.Remove(ReconciliationRequested);
        resource.Metadata.Annotations.Add(ReconciliationRequested, DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffff0Z"));
        var next = JsonSerializer.SerializeToDocument(resource);
        var patch = current.CreatePatch(next);

        var crPatch = new V1Patch(patch, V1Patch.PatchType.JsonPatch);

        if (namespaceName != null)
            await client.CustomObjects.PatchNamespacedCustomObjectAsync(crPatch, Group, Version, namespaceName, PluralName, resource.Metadata.Name).ConfigureAwait(false);
        else
            await client.CustomObjects.PatchClusterCustomObjectAsync(crPatch, Group, Version, PluralName, resource.Metadata.Name).ConfigureAwait(false);
    }
}
