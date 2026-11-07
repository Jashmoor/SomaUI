using System.Diagnostics.CodeAnalysis;

namespace SomaUI;

public class BaseWithDependentChild : BaseComponent {

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(BaseWithDependentChild))]
    public BaseWithDependentChild() {
        UniqueID = Identifier.SequentialContext().GenerateId();
    }

}
