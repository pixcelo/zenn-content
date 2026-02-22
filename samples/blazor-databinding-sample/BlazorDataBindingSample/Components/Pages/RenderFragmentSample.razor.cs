using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

public partial class RenderFragmentSample : ComponentBase
{
    private int card1ClickCount = 0;
    private int card2ClickCount = 0;
    private string inputText = "";

    private void OnCard1ButtonClick()
    {
        card1ClickCount++;
    }

    private void OnCard2ButtonClick()
    {
        card2ClickCount++;
    }
}
