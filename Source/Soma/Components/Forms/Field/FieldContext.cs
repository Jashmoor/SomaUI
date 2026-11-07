namespace SomaUI;

internal class FieldContext {
    public FieldContext() {
        using var @base = Identifier.SequentialContext();
        Id = @base.GenerateId();
        LabelId = @base.GenerateId();
        DescriptionId = @base.GenerateId();
        ErrorId = @base.GenerateId();
    }
    public string Id { get; }
    public Action? StateChange;
    public string LabelId { get; }
    public string DescriptionId { get; }
    public string ErrorId { get; }
}
