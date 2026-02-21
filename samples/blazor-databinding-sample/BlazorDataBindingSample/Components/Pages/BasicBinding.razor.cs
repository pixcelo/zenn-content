using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// 基本的なバインディングのデモページ
/// </summary>
public partial class BasicBinding : ComponentBase
{
    private string currentValue = "初期値";
    private string currentTime = DateTime.Now.ToString("HH:mm:ss");
    private int count = 0;
    private bool isButtonDisabled = true;
    private bool isVisible = false;
    private string currentClass = "bg-primary text-white";
    private string[] cssClasses = { "bg-primary text-white", "bg-success text-white", "bg-danger text-white", "bg-warning text-dark" };
    private int classIndex = 0;

    private void UpdateValue()
    {
        currentValue = $"更新された値 ({DateTime.Now:HH:mm:ss})";
        currentTime = DateTime.Now.ToString("HH:mm:ss");
    }

    private void IncrementCount()
    {
        count++;
    }

    private void ToggleDisabled()
    {
        isButtonDisabled = !isButtonDisabled;
    }

    private void ToggleVisibility()
    {
        isVisible = !isVisible;
    }

    private void ChangeClass()
    {
        classIndex = (classIndex + 1) % cssClasses.Length;
        currentClass = cssClasses[classIndex];
    }
}
