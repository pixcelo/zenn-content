using Microsoft.AspNetCore.Components;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// フォームバインディングのデモページ
/// </summary>
public partial class FormBinding : ComponentBase
{
    // テキスト入力
    private string textValue = "";
    private string passwordValue = "";
    private string emailValue = "";
    private string textareaValue = "";

    // 数値入力
    private int intValue = 0;
    private decimal decimalValue = 0m;
    private int rangeValue = 50;

    // 日付と時刻
    private DateTime dateValue = DateTime.Today;
    private TimeOnly? timeValue = TimeOnly.FromDateTime(DateTime.Now);
    private DateTime datetimeValue = DateTime.Now;

    // 選択系
    private bool checkbox1 = false;
    private bool checkbox2 = false;
    private bool checkbox3 = false;
    private string radioValue = "option1";
    private string selectValue = "";
    private string multiSelectValue = "";

    // その他
    private string colorValue = "#0d6efd";
    private string urlValue = "";
    private string telValue = "";
}
