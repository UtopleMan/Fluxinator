using k8s;
using k8s.Models;
using System.Text.Json.Serialization;

namespace Fluxinator.Model;

public enum DeploymentStates
{
    Success,
    Running,
    Waiting,
    Failed
}
public record Deployment(string Namespace, string Name, DeploymentStates DeploymentState, string LatestMessage, string LatestReason, DateTimeOffset LatestTime, IEnumerable<Run> Runs);
public record Run(string Message, string Reason, DateTimeOffset Time);
public record Event(string Id, string Text);
public class KustomizationResource : CustomResource<KustomizationResourceSpec, KustomizationResourceStatus>
{
}
public record KustomizationResourceSpec
{
    [JsonPropertyName("interval")]
    public string? Interval { get; set; }

    [JsonPropertyName("force")]
    public bool Force { get; set; }

    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [JsonPropertyName("prune")]
    public bool Prune { get; set; }

    [JsonPropertyName("targetNamespace")]
    public string? TargetNamespace { get; set; }

    [JsonPropertyName("timeout")]
    public string? Timeout { get; set; }

    [JsonPropertyName("sourceRef")]
    public SourceReference? SourceReference { get; set; }
}
public record SourceReference
{
    [JsonPropertyName("kind")]
    public string? Kind { get; set; }
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}

public class KustomizationResourceStatus : V1Status
{
    [JsonPropertyName("conditions")]
    public List<Condition>? Conditions { get; set; }

    [JsonPropertyName("inventory")]
    public Inventory? Inventory { get; set; }
}
public record Inventory
{
    [JsonPropertyName("entries")]
    public List<Entry>? Entries { get; set; }
}
public record Entry
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("v")]
    public string? Version { get; set; }
}
public record Condition
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("reason")]
    public string? Reason { get; set; }

    [JsonPropertyName("lastTransitionTime")]
    public DateTimeOffset Timestamp { get; set; }

    //[JsonPropertyName("status")]
    //public string Status { get; set; }

    //[JsonPropertyName("type")]
    //public string ConditionType { get; set; }
}

public class CustomResourceDefinition
{
    public string? Version { get; set; }

    public string? Group { get; set; }

    public string? PluralName { get; set; }

    public string? Kind { get; set; }

    public string? Namespace { get; set; }
}

public abstract class CustomResource : KubernetesObject
{
    [JsonPropertyName("metadata")]
    public V1ObjectMeta? Metadata { get; set; }
}

public abstract class CustomResource<TSpec, TStatus> : CustomResource
{
    [JsonPropertyName("spec")]
    public TSpec? Spec { get; set; }

    [JsonPropertyName("status")]
    public TStatus? Status { get; set; }
}

public class CustomResourceList<T> : KubernetesObject
where T : CustomResource
{
    public V1ListMeta? Metadata { get; set; }
    public List<T>? Items { get; set; }
}