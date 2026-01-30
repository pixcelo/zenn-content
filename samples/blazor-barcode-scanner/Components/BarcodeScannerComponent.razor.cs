using BlazorBarcodeScanner.ZXing.JS;
using Microsoft.AspNetCore.Components;

namespace blazor_barcode_scanner.Components
{
    public partial class BarcodeScannerComponent
    {
        [Parameter]
        public bool IsOpen { get; set; }

        [Parameter]
        public EventCallback<string> OnScanCompleted { get; set; }

        [Parameter]
        public EventCallback OnCancelled { get; set; }

        private async Task HandleBarcodeDetected(BarcodeReceivedEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.BarcodeText))
            {
                // スキャン成功時、結果を親コンポーネントに通知して自動クローズ
                await OnScanCompleted.InvokeAsync(args.BarcodeText);
            }
        }

        private async Task HandleCancel()
        {
            // キャンセル時、親コンポーネントに通知
            await OnCancelled.InvokeAsync();
        }

        private void HandleError(ErrorReceivedEventArgs args)
        {
            // エラーはログのみ（必要に応じて親に通知することも可能）
            Console.WriteLine($"Barcode scanner error: {args?.Message}");
        }
    }
}
