using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// イベントバインディングのデモページ
/// </summary>
public partial class EventBinding : ComponentBase
{
    // マウスイベント
    private string lastMouseEvent = "なし";
    private int clickCount = 0;
    private string mouseOverColor = "white";

    // キーボードイベント
    private string lastKeyEvent = "なし";
    private string pressedKey = "";

    // フォームイベント
    private string changedValue = "";
    private string inputValue = "";
    private string focusState = "なし";
    private string focusBorderColor = "#ced4da";

    // イベント引数
    private double clickX = 0;
    private double clickY = 0;
    private string keyCode = "";
    private bool ctrlKey = false;
    private bool shiftKey = false;
    private bool altKey = false;

    // イベント伝播
    private string lastClickTarget = "";

    // preventDefault
    private int submitCount = 0;

    // マウスイベントハンドラ
    private void OnClick()
    {
        clickCount++;
        lastMouseEvent = "クリック";
    }

    private void OnDoubleClick()
    {
        lastMouseEvent = "ダブルクリック";
    }

    private void OnMouseOver()
    {
        lastMouseEvent = "マウスオーバー";
        mouseOverColor = "#e7f3ff";
    }

    private void OnMouseOut()
    {
        lastMouseEvent = "マウスアウト";
        mouseOverColor = "white";
    }

    // キーボードイベントハンドラ
    private void OnKeyDown()
    {
        lastKeyEvent = "キーダウン";
    }

    private void OnKeyUp()
    {
        lastKeyEvent = "キーアップ";
    }

    private void OnKeyPress(KeyboardEventArgs e)
    {
        lastKeyEvent = "キープレス";
        pressedKey = e.Key;
    }

    // フォームイベントハンドラ
    private void OnChange(ChangeEventArgs e)
    {
        changedValue = e.Value?.ToString() ?? "";
    }

    private void OnInput(ChangeEventArgs e)
    {
        inputValue = e.Value?.ToString() ?? "";
    }

    private void OnFocus()
    {
        focusState = "フォーカス中";
        focusBorderColor = "#0d6efd";
    }

    private void OnBlur()
    {
        focusState = "フォーカス外";
        focusBorderColor = "#ced4da";
    }

    // イベント引数を使用
    private void OnClickWithArgs(MouseEventArgs e)
    {
        clickX = e.ClientX;
        clickY = e.ClientY;
    }

    private void OnKeyDownWithArgs(KeyboardEventArgs e)
    {
        keyCode = e.Code;
        ctrlKey = e.CtrlKey;
        shiftKey = e.ShiftKey;
        altKey = e.AltKey;
    }

    // イベント伝播
    private void OnOuterClick()
    {
        lastClickTarget = "外側のdiv";
    }

    private void OnInnerClick()
    {
        lastClickTarget = "内側のdiv（伝播あり）";
    }

    private void OnInnerClickStopPropagation()
    {
        lastClickTarget = "内側のdiv（伝播なし）";
    }

    // preventDefault
    private void OnSubmit()
    {
        submitCount++;
    }
}
