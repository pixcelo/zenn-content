using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// カウンターのデモページ
/// </summary>
public partial class Counter : ComponentBase
{
    private int currentCount = 0;

    private void IncrementCount()
    {
        currentCount++;
    }
}
