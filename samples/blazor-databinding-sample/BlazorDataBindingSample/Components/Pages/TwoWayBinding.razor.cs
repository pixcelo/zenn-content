using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// 双方向バインディングのデモページ
/// </summary>
public partial class TwoWayBinding : ComponentBase
{
    // 基本的な@bind
    private string name = "";
    private int age = 0;
    private decimal price = 0m;

    // @bind:event
    private string onChangeText = "";
    private string onInputText = "";

    // @bind:after
    private string callbackText = "";
    private string upperCaseText = "";
    private int changeCount = 0;

    // チェックボックスとラジオボタン
    private bool isAgreed = false;
    private string selectedColor = "red";

    // 日付と時刻
    private DateTime selectedDate = DateTime.Today;
    private TimeOnly? selectedTime = TimeOnly.FromDateTime(DateTime.Now);

    private void OnTextChanged()
    {
        upperCaseText = callbackText.ToUpper();
        changeCount++;
    }
}
