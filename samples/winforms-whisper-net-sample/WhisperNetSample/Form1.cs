using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Whisper.net;
using Whisper.net.Ggml;

namespace WhisperNetSample
{
    public partial class Form1 : Form
    {
        // モデル情報のマッピング
        private class ModelInfo
        {
            public GgmlType Type { get; set; }
            public string Name { get; set; }
            public string Size { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return $"{Name} ({Size}) - {Description}";
            }
        }

        // 選択中のモデルタイプ
        private GgmlType selectedModelType = GgmlType.Base;

        // Whisper処理用ファクトリー
        private WhisperFactory whisperFactory;

        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            // DataGridViewの初期設定
            InitializeDataGridView();

            // ComboBoxの初期化
            InitializeModelTypeComboBox();

            // モデルの自動ダウンロードと初期化
            await InitializeWhisperModelAsync(selectedModelType);
        }

        private void InitializeDataGridView()
        {
            // DataTableを作成
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("名前", typeof(string));
            dt.Columns.Add("説明", typeof(string));

            // サンプルデータを追加
            dt.Rows.Add(1, "サンプル1", "これはテストデータです");
            dt.Rows.Add(2, "サンプル2", "Whisper.netのサンプルプロジェクト");
            dt.Rows.Add(3, "サンプル3", ".NET Framework 4.8対応");

            // DataGridViewにバインド
            dataGridView1.DataSource = dt;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ボタンがクリックされました！", "テスト",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnOpenPopup_Click(object sender, EventArgs e)
        {
            // ポップアップフォームを作成して表示
            using (var popupForm = new PopupForm())
            {
                popupForm.ShowDialog(this);
            }
        }

        /// <summary>
        /// ComboBoxの初期化
        /// </summary>
        private void InitializeModelTypeComboBox()
        {
            var models = new[]
            {
                new ModelInfo { Type = GgmlType.Tiny, Name = "Tiny", Size = "75MB", Description = "最小・高速" },
                new ModelInfo { Type = GgmlType.Base, Name = "Base", Size = "142MB", Description = "バランス型（推奨）" },
                new ModelInfo { Type = GgmlType.Small, Name = "Small", Size = "466MB", Description = "高精度" },
                new ModelInfo { Type = GgmlType.Medium, Name = "Medium", Size = "1.5GB", Description = "より高精度" },
                new ModelInfo { Type = GgmlType.LargeV3, Name = "Large V3", Size = "2.9GB", Description = "最高精度" },
                new ModelInfo { Type = GgmlType.LargeV3Turbo, Name = "Large V3 Turbo", Size = "1.6GB", Description = "最新・高精度・高速" }
            };

            modelTypeComboBox.DisplayMember = "ToString";
            modelTypeComboBox.ValueMember = "Type";
            modelTypeComboBox.DataSource = models;

            // Baseモデルを選択（インデックス1）
            modelTypeComboBox.SelectedIndex = 1;
        }

        /// <summary>
        /// モデル選択変更イベント
        /// </summary>
        private async void modelTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modelTypeComboBox.SelectedItem is ModelInfo selectedModel)
            {
                selectedModelType = selectedModel.Type;
                await InitializeWhisperModelAsync(selectedModelType);
            }
        }

        /// <summary>
        /// モデルファイルパスを取得
        /// </summary>
        private string GetModelPath(GgmlType modelType)
        {
            return Path.Combine(
                Application.StartupPath,
                $"ggml-{modelType.ToString().ToLower()}.bin");
        }

        /// <summary>
        /// Whisperモデルの初期化
        /// </summary>
        private async Task InitializeWhisperModelAsync(GgmlType modelType)
        {
            try
            {
                var modelPath = GetModelPath(modelType);

                // モデルファイルが存在するかチェック
                if (!File.Exists(modelPath))
                {
                    // モデルをダウンロード
                    await DownloadModelAsync(modelType, modelPath);
                }

                // WhisperFactoryを初期化
                whisperFactory?.Dispose();
                whisperFactory = WhisperFactory.FromPath(modelPath);

                // ステータス表示を更新（成功）
                var fileInfo = new FileInfo(modelPath);
                var sizeMB = fileInfo.Length / (1024 * 1024);
                UpdateStatus($"Whisperモデル初期化完了 ({modelType}, {sizeMB}MB)", true);
            }
            catch (Exception ex)
            {
                // エラー時の処理
                UpdateStatus($"エラー: {ex.Message}", false);
                MessageBox.Show(
                    $"Whisperモデルの初期化に失敗しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// モデルファイルをダウンロード
        /// </summary>
        private async Task DownloadModelAsync(GgmlType modelType, string modelPath)
        {
            UpdateStatus($"モデル ({modelType}) をダウンロード中...", true);

            // Hugging Faceからモデルをダウンロード
            using (var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(modelType))
            {
                // ファイルに保存
                using (var fileWriter = File.OpenWrite(modelPath))
                {
                    await modelStream.CopyToAsync(fileWriter);
                }
            }

            UpdateStatus($"モデル ({modelType}) ダウンロード完了", true);
        }

        /// <summary>
        /// ステータス表示を更新（UIスレッドセーフ）
        /// </summary>
        private void UpdateStatus(string message, bool isSuccess)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateStatus(message, isSuccess)));
                return;
            }

            // ステータスラベルを更新
            statusLabel.Text = message;
            statusLabel.ForeColor = isSuccess ? System.Drawing.Color.Green : System.Drawing.Color.Red;
        }
    }
}
