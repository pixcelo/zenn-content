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

        // 選択中の音声ファイルパス
        private string selectedAudioFilePath;

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
        /// 音声ファイル選択ボタンクリック
        /// </summary>
        private void btnSelectAudioFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "WAV Files (*.wav)|*.wav|All Files (*.*)|*.*";
                openFileDialog.Title = "音声ファイルを選択";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedAudioFilePath = openFileDialog.FileName;
                    txtSelectedFile.Text = selectedAudioFilePath;
                    btnTranscribe.Enabled = true;
                    UpdateStatus("音声ファイルを選択しました", true);
                }
            }
        }

        /// <summary>
        /// 文字起こし実行ボタンクリック
        /// </summary>
        private async void btnTranscribe_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedAudioFilePath))
            {
                MessageBox.Show("音声ファイルを選択してください", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (whisperFactory == null)
            {
                MessageBox.Show("Whisperモデルが初期化されていません", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                UpdateStatus("文字起こし中...", true);
                btnTranscribe.Enabled = false;
                btnSelectAudioFile.Enabled = false;

                // WhisperProcessorを作成
                using (var processor = whisperFactory.CreateBuilder()
                    .WithLanguage("auto")  // 自動言語検出（日本語対応）
                    .Build())
                {

                    // DataTableを準備
                    var dt = new DataTable();
                    dt.Columns.Add("開始時刻", typeof(string));
                    dt.Columns.Add("終了時刻", typeof(string));
                    dt.Columns.Add("テキスト", typeof(string));

                    // 音声ファイルを処理
                    using (var fileStream = File.OpenRead(selectedAudioFilePath))
                    {
                        var enumerator = processor.ProcessAsync(fileStream).GetAsyncEnumerator();
                        try
                        {
                            while (await enumerator.MoveNextAsync())
                            {
                                var result = enumerator.Current;
                                // 時刻をmm:ss.ff形式に変換
                                var startTime = result.Start.ToString(@"mm\:ss\.ff");
                                var endTime = result.End.ToString(@"mm\:ss\.ff");
                                dt.Rows.Add(startTime, endTime, result.Text);
                            }
                        }
                        finally
                        {
                            await enumerator.DisposeAsync();
                        }
                    }

                    // DataGridViewに表示
                    dataGridView1.DataSource = dt;

                    UpdateStatus($"文字起こし完了 ({dt.Rows.Count}件)", true);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"エラー: {ex.Message}", false);
                MessageBox.Show(
                    $"文字起こしに失敗しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnTranscribe.Enabled = true;
                btnSelectAudioFile.Enabled = true;
            }
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
