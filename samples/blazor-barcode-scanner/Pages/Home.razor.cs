namespace blazor_barcode_scanner.Pages
{
    public partial class Home
    {
        private bool showModal = false;
        private string lastResult = string.Empty;
        private string lastScannedTime = string.Empty;
        private List<string> scanHistory = new();

        private void OpenScanner()
        {
            showModal = true;
        }

        private void CloseScanner()
        {
            showModal = false;
        }

        private void HandleScanResult(string barcodeText)
        {
            lastResult = barcodeText;
            lastScannedTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
            scanHistory.Insert(0, $"[{lastScannedTime}] {lastResult}");

            // スキャン成功後、モーダルを閉じる
            showModal = false;
            StateHasChanged();
        }
    }
}
