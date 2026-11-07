namespace SomaUI;

public class TraceModel {
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TraceStatus Status { get; set; } = TraceStatus.Finished;
    public TraceAction Action { get; set; } = TraceAction.None;

    public bool Expanded { get; set; }
    public bool DefaultOpen { get; set; }
    public bool Executing { get; set; }
    public string? ErrorMessage { get; set; }

    public Func<TraceModel, Task>? AsyncAction { get; set; }
    public List<TraceModel> Children { get; set; } = [];

    public TraceStatus ComputeStatus() {
        if (Children == null || !Children.Any())
            return Status;
        if (Children.Any(c => c.ComputeStatus() == TraceStatus.Error))
            return TraceStatus.Error;
        if (Children.Any(c => c.ComputeStatus() == TraceStatus.Running || c.Executing))
            return TraceStatus.Running;
        if (Children.Any(c => c.ComputeStatus() == TraceStatus.Pending))
            return TraceStatus.Pending;
        return TraceStatus.Finished;
    }
}

//public sealed record TraceAsyncAction : TraceModel {

//}

//public sealed record TraceGroup : TraceModel {
//    public List<TraceModel> Children { get; init; } = [];
//}
