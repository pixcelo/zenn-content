using Radzen;

namespace BlazorRadzenApp.Components.CustomComponents
{
    public partial class RadzenDialogDemo
    {
        private string DialogResult { get; set; } = string.Empty;

        private async Task OpenDialog()
        {
            var result = await DialogService.OpenAsync<SimpleDialogContent>(
                "情報",
                new Dictionary<string, object>(),
                new DialogOptions() { Width = "400px", Height = "200px" }
            );

            DialogResult = result != null ? "ダイアログが閉じられました" : "キャンセルされました";
        }

        private async Task OpenConfirmDialog()
        {
            var confirmed = await DialogService.Confirm(
                "この操作を実行してもよろしいですか?",
                "確認",
                new ConfirmOptions { OkButtonText = "はい", CancelButtonText = "いいえ" }
            );

            DialogResult = confirmed == true ? "確認されました" : "キャンセルされました";
        }
    }
}
