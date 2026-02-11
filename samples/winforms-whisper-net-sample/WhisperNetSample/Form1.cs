using System;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;
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
        private GgmlType _selectedModelType = GgmlType.Base;

        // Whisper処理用ファクトリー
        private WhisperFactory _whisperFactory;

        // 選択中の音声ファイルパス
        private string _selectedAudioFilePath;

        // マイク録音用
        private WaveInEvent _waveIn;
        private WasapiLoopbackCapture _wasapiCapture;  // PC音声録音用
        private MediaFoundationResampler _resampler;  // リサンプラー（PCオーディオ用）
        private WaveFileWriter _waveFileWriter;
        private string _recordingFilePath;

        // ミックス録音用
        private BufferedWaveProvider _micBuffer;  // マイク用バッファ
        private BufferedWaveProvider _pcAudioBuffer;  // PCオーディオ用バッファ
        private MixingSampleProvider _mixer;  // ミキサー
        private System.Threading.Timer _mixingTimer;  // ミックス処理用タイマー

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
            await InitializeWhisperModelAsync(_selectedModelType);
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
                _selectedModelType = selectedModel.Type;
                await InitializeWhisperModelAsync(_selectedModelType);
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
                _whisperFactory?.Dispose();
                _whisperFactory = WhisperFactory.FromPath(modelPath);

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
        /// MP3ファイルをWAV形式に変換
        /// </summary>
        private string ConvertMp3ToWav(string mp3FilePath)
        {
            try
            {
                // 一時WAVファイルのパスを生成
                var tempWavPath = Path.Combine(Path.GetTempPath(), $"whisper_{Guid.NewGuid()}.wav");

                using (var reader = new Mp3FileReader(mp3FilePath))
                {
                    // Whisperは16kHz Monoを推奨
                    WaveFormat targetFormat = new WaveFormat(16000, 16, 1);
                    using (var resampler = new MediaFoundationResampler(reader, targetFormat))
                    {
                        WaveFileWriter.CreateWaveFile(tempWavPath, resampler);
                    }
                }

                return tempWavPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"MP3からWAVへの変換に失敗しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return null;
            }
        }

        /// <summary>
        /// 音声ファイル選択ボタンクリック
        /// </summary>
        private void btnSelectAudioFile_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "音声ファイル (*.wav;*.mp3)|*.wav;*.mp3|WAV Files (*.wav)|*.wav|MP3 Files (*.mp3)|*.mp3|All Files (*.*)|*.*";
                openFileDialog.Title = "音声ファイルを選択";
                openFileDialog.Multiselect = false;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    var selectedFile = openFileDialog.FileName;
                    var extension = Path.GetExtension(selectedFile).ToLower();

                    // MP3の場合はWAVに変換
                    if (extension == ".mp3")
                    {
                        UpdateStatus("MP3をWAVに変換中...", false);
                        var convertedWav = ConvertMp3ToWav(selectedFile);
                        if (convertedWav != null)
                        {
                            _selectedAudioFilePath = convertedWav;
                            txtSelectedFile.Text = $"{selectedFile} (変換済み)";
                            btnTranscribe.Enabled = true;
                            UpdateStatus("MP3ファイルを選択しました（WAVに変換済み）", true);
                        }
                        else
                        {
                            UpdateStatus("MP3変換に失敗しました", false);
                        }
                    }
                    else
                    {
                        _selectedAudioFilePath = selectedFile;
                        txtSelectedFile.Text = selectedFile;
                        btnTranscribe.Enabled = true;
                        UpdateStatus("音声ファイルを選択しました", true);
                    }
                }
            }
        }

        /// <summary>
        /// 文字起こし実行ボタンクリック
        /// </summary>
        private async void btnTranscribe_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_selectedAudioFilePath))
            {
                MessageBox.Show("音声ファイルを選択してください", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_whisperFactory == null)
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
                using (var processor = _whisperFactory.CreateBuilder()
                    .WithLanguage("auto")  // 自動言語検出（日本語対応）
                    .Build())
                {

                    // DataTableを準備
                    var dt = new DataTable();
                    dt.Columns.Add("開始時刻", typeof(string));
                    dt.Columns.Add("終了時刻", typeof(string));
                    dt.Columns.Add("テキスト", typeof(string));

                    // 音声ファイルを処理
                    using (var fileStream = File.OpenRead(_selectedAudioFilePath))
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
        /// 録音開始ボタンクリック
        /// </summary>
        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            try
            {
                // 録音ファイルパスを生成
                _recordingFilePath = Path.Combine(Path.GetTempPath(), $"recording_{Guid.NewGuid()}.wav");

                // 録音モードに応じて処理を分岐
                if (radioMicOnly.Checked)
                {
                    StartMicRecording();
                }
                else if (radioPCOnly.Checked)
                {
                    StartPCAudioRecording();
                }
                else if (radioMix.Checked)
                {
                    StartMixRecording();
                }

                // UI状態更新
                btnStartRecording.Enabled = false;
                btnStopRecording.Enabled = true;
                btnSelectAudioFile.Enabled = false;
                btnTranscribe.Enabled = false;
                labelRecordingStatus.Text = "録音中...";
                labelRecordingStatus.ForeColor = System.Drawing.Color.Red;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"録音開始に失敗しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// マイクのみ録音開始
        /// </summary>
        private void StartMicRecording()
        {
            // マイクデバイスの存在確認
            if (WaveInEvent.DeviceCount == 0)
            {
                MessageBox.Show("マイクが見つかりません", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // WaveInEventを初期化（16kHz Mono、Whisper推奨設定）
            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);

            // ファイルライターを初期化
            _waveFileWriter = new WaveFileWriter(_recordingFilePath, _waveIn.WaveFormat);

            // データ受信イベントハンドラ
            _waveIn.DataAvailable += (s, args) =>
            {
                _waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
            };

            // 録音開始
            _waveIn.StartRecording();
            UpdateStatus("マイク録音中...", false);
        }

        /// <summary>
        /// PCの音のみ録音開始
        /// </summary>
        private void StartPCAudioRecording()
        {
            // WasapiLoopbackCaptureを初期化
            _wasapiCapture = new WasapiLoopbackCapture();

            // システムオーディオは48kHz Stereoが多いため、16kHz Monoにリサンプリング
            var targetFormat = new WaveFormat(16000, 16, 1);

            // ファイルライターを初期化
            _waveFileWriter = new WaveFileWriter(_recordingFilePath, targetFormat);

            // BufferedWaveProviderを使用してデータを受け渡す
            var bufferedProvider = new BufferedWaveProvider(_wasapiCapture.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            // リサンプラーを作成
            _resampler = new MediaFoundationResampler(bufferedProvider, targetFormat);
            _resampler.ResamplerQuality = 60; // 高品質

            // データ受信イベントハンドラ
            _wasapiCapture.DataAvailable += (s, args) =>
            {
                // バッファにデータを追加
                bufferedProvider.AddSamples(args.Buffer, 0, args.BytesRecorded);

                // リサンプリングして書き込み
                var resampledBuffer = new byte[args.BytesRecorded];
                int bytesRead = _resampler.Read(resampledBuffer, 0, resampledBuffer.Length);

                if (bytesRead > 0)
                {
                    _waveFileWriter.Write(resampledBuffer, 0, bytesRead);
                }
            };

            // 録音開始
            _wasapiCapture.StartRecording();
            UpdateStatus("PC音声録音中... (音が鳴っていない場合は録音されません)", false);
        }

        /// <summary>
        /// ミックス録音開始（マイク + PCオーディオ）
        /// </summary>
        private void StartMixRecording()
        {
            // マイクデバイスの存在確認
            if (WaveInEvent.DeviceCount == 0)
            {
                MessageBox.Show("マイクが見つかりません", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 共通フォーマット: 16kHz Mono
            var targetFormat = new WaveFormat(16000, 16, 1);

            // ファイルライターを初期化
            _waveFileWriter = new WaveFileWriter(_recordingFilePath, targetFormat);

            // マイク録音の初期化
            _waveIn = new WaveInEvent();
            _waveIn.WaveFormat = targetFormat;  // 直接16kHz Monoで録音

            // マイク用バッファ
            _micBuffer = new BufferedWaveProvider(targetFormat)
            {
                DiscardOnBufferOverflow = true
            };

            // PCオーディオ録音の初期化
            _wasapiCapture = new WasapiLoopbackCapture();

            // PCオーディオ用バッファ（リサンプリング前の48kHz Stereo）
            var pcAudioSourceBuffer = new BufferedWaveProvider(_wasapiCapture.WaveFormat)
            {
                DiscardOnBufferOverflow = true
            };

            // PCオーディオ用バッファ（リサンプリング後の16kHz Mono）
            _pcAudioBuffer = new BufferedWaveProvider(targetFormat)
            {
                DiscardOnBufferOverflow = true
            };

            // リサンプラーを作成（48kHz Stereo → 16kHz Mono）
            _resampler = new MediaFoundationResampler(pcAudioSourceBuffer, targetFormat);
            _resampler.ResamplerQuality = 60;

            // マイクデータ受信イベント
            _waveIn.DataAvailable += (s, args) =>
            {
                _micBuffer.AddSamples(args.Buffer, 0, args.BytesRecorded);
            };

            // PCオーディオデータ受信イベント
            _wasapiCapture.DataAvailable += (s, args) =>
            {
                // ソースバッファに追加
                pcAudioSourceBuffer.AddSamples(args.Buffer, 0, args.BytesRecorded);

                // リサンプリングして16kHz Monoバッファに追加
                var resampledBuffer = new byte[args.BytesRecorded];
                int bytesRead = _resampler.Read(resampledBuffer, 0, resampledBuffer.Length);
                if (bytesRead > 0)
                {
                    _pcAudioBuffer.AddSamples(resampledBuffer, 0, bytesRead);
                }
            };

            // ミキサーを作成
            var micProvider = _micBuffer.ToSampleProvider();
            var pcAudioProvider = _pcAudioBuffer.ToSampleProvider();

            _mixer = new MixingSampleProvider(new[] { micProvider, pcAudioProvider });

            // タイマーでミックスしたデータをファイルに書き込む（100msごと）
            _mixingTimer = new System.Threading.Timer(_ =>
            {
                try
                {
                    // ミキサーから読み取り
                    var sampleBuffer = new float[targetFormat.SampleRate / 10];  // 100ms分
                    int samplesRead = _mixer.Read(sampleBuffer, 0, sampleBuffer.Length);

                    if (samplesRead > 0)
                    {
                        // float[] → byte[] 変換
                        var byteBuffer = new byte[samplesRead * 2];  // 16bit = 2bytes
                        for (int i = 0; i < samplesRead; i++)
                        {
                            var sample16 = (short)(sampleBuffer[i] * 32767f);
                            byteBuffer[i * 2] = (byte)(sample16 & 0xFF);
                            byteBuffer[i * 2 + 1] = (byte)(sample16 >> 8);
                        }

                        _waveFileWriter.Write(byteBuffer, 0, byteBuffer.Length);
                    }
                }
                catch
                {
                    // タイマー処理中のエラーは無視
                }
            }, null, 100, 100);  // 100ms後に開始、100msごとに実行

            // 録音開始
            _waveIn.StartRecording();
            _wasapiCapture.StartRecording();
            UpdateStatus("ミックス録音中（マイク + PC音声）...", false);
        }

        /// <summary>
        /// 録音停止ボタンクリック
        /// </summary>
        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            try
            {
                // マイク録音停止
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                    _waveIn = null;
                }

                // PC音声録音停止
                if (_wasapiCapture != null)
                {
                    _wasapiCapture.StopRecording();
                    _wasapiCapture.Dispose();
                    _wasapiCapture = null;
                }

                // ミックス録音用タイマー停止
                if (_mixingTimer != null)
                {
                    _mixingTimer.Dispose();
                    _mixingTimer = null;
                }

                // ミキサー・バッファのクリーンアップ
                _mixer = null;
                _micBuffer = null;
                _pcAudioBuffer = null;

                // リサンプラーをクリーンアップ
                if (_resampler != null)
                {
                    _resampler.Dispose();
                    _resampler = null;
                }

                // ファイルライターをクローズ
                if (_waveFileWriter != null)
                {
                    _waveFileWriter.Dispose();
                    _waveFileWriter = null;
                }

                // 録音ファイルを選択状態にする
                _selectedAudioFilePath = _recordingFilePath;
                txtSelectedFile.Text = $"録音ファイル: {Path.GetFileName(_recordingFilePath)}";

                // UI状態更新
                btnStartRecording.Enabled = true;
                btnStopRecording.Enabled = false;
                btnSelectAudioFile.Enabled = true;
                btnTranscribe.Enabled = true;
                labelRecordingStatus.Text = "待機中";
                labelRecordingStatus.ForeColor = System.Drawing.Color.Black;
                UpdateStatus("録音完了。文字起こしボタンを押してください。", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"録音停止に失敗しました。\n\n{ex.Message}",
                    "エラー",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
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
