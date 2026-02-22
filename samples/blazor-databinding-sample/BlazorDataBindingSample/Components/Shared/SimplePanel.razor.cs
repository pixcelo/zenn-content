using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Shared;

public partial class SimplePanel : ComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
