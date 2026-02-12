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
        // 録音モード
        private enum RecordingMode
        {
            Microphone,  // マイクのみ
            PCAudio,     // PCオーディオのみ
            Mix          // ミックス（両方）
        }

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
        private object _mixingLock = new object();  // 同期用ロック

        // 現在の録音モード
        private RecordingMode _currentRecordingMode;

        // 最後に文字起こししたメタデータ
        private TranscriptionMetadata _lastTranscriptionMetadata;

        // 音声コマンド管理
        private VoiceCommandManager _voiceCommandManager;

        // 音声コマンド用の録音データ
        private WaveInEvent _voiceCommandWaveIn;
        private WaveFileWriter _voiceCommandFileWriter;
        private string _voiceCommandFilePath;

        // 開いているポップアップフォームの参照
        private PopupForm _activePopupForm;

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

            // 音声コマンドの初期化
            InitializeVoiceCommands();

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
            // 既存のポップアップがあれば閉じる
            if (_activePopupForm != null && !_activePopupForm.IsDisposed)
            {
                _activePopupForm.Close();
            }

            // ポップアップフォームを作成して表示
            _activePopupForm = new PopupForm();
            _activePopupForm.FormClosed += (s, args) => { _activePopupForm = null; };
            _activePopupForm.Show(this);
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
        /// Whisperモデルの初期化
        /// </summary>
        private async Task InitializeWhisperModelAsync(GgmlType modelType)
        {
            try
            {
                var modelPath = Path.Combine(
                    Application.StartupPath,
                    $"ggml-{modelType.ToString().ToLower()}.bin");

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
                    .WithLanguage("ja")  // 日本語を明示的に指定
                    .Build())
                {

                    // DataTableを準備
                    var dt = new DataTable();
                    dt.Columns.Add("開始時刻", typeof(string));
                    dt.Columns.Add("終了時刻", typeof(string));
                    dt.Columns.Add("テキスト", typeof(string));

                    // 音声ファイルを処理
                    TimeSpan totalDuration = TimeSpan.Zero;
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

                                // 総時間を更新
                                if (result.End > totalDuration)
                                {
                                    totalDuration = result.End;
                                }
                            }
                        }
                        finally
                        {
                            await enumerator.DisposeAsync();
                        }
                    }

                    // DataGridViewに表示
                    dataGridView1.DataSource = dt;

                    // メタデータを保存
                    _lastTranscriptionMetadata = new TranscriptionMetadata
                    {
                        CreatedAt = DateTime.Now,
                        ModelName = _selectedModelType.ToString(),
                        Language = "ja",
                        AudioFilePath = _selectedAudioFilePath,
                        TotalDuration = totalDuration,
                        SegmentCount = dt.Rows.Count
                    };

                    // エクスポートボタンを有効化
                    btnExport.Enabled = true;

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
                    _currentRecordingMode = RecordingMode.Microphone;
                    StartMicRecording();
                }
                else if (radioPCOnly.Checked)
                {
                    _currentRecordingMode = RecordingMode.PCAudio;
                    StartPCAudioRecording();
                }
                else if (radioMix.Checked)
                {
                    _currentRecordingMode = RecordingMode.Mix;
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

            // ファイルライターを初期化（48kHz Stereoのまま録音）
            // 録音完了後に16kHz Monoにリサンプリングする
            _waveFileWriter = new WaveFileWriter(_recordingFilePath, _wasapiCapture.WaveFormat);

            // データ受信イベントハンドラ
            _wasapiCapture.DataAvailable += (s, args) =>
            {
                // そのまま書き込み（リサンプリングしない）
                _waveFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
            };

            // 録音開始
            _wasapiCapture.StartRecording();
            UpdateStatus("PC音声録音中... (音が鳴っていない場合は録音されません)", false);
        }

        /// <summary>
        /// ミックスされた音声データをファイルに書き込む
        /// </summary>
        private void ProcessMixedAudio()
        {
            lock (_mixingLock)
            {
                // 両方のバッファから読み取れるサイズを確認
                int availableMic = _micBuffer.BufferedBytes;
                int availablePC = _pcAudioBuffer.BufferedBytes;

                if (availableMic == 0 || availablePC == 0)
                {
                    return;  // どちらかのバッファが空なら何もしない
                }

                // 小さい方に合わせる（両方から同じバイト数を読み取る）
                int bytesToRead = Math.Min(availableMic, availablePC);

                // 偶数バイトに調整（16bitサンプル = 2bytes）
                bytesToRead = (bytesToRead / 2) * 2;

                if (bytesToRead == 0)
                {
                    return;
                }

                byte[] micBytes = new byte[bytesToRead];
                byte[] pcBytes = new byte[bytesToRead];

                int micRead = _micBuffer.Read(micBytes, 0, bytesToRead);
                int pcRead = _pcAudioBuffer.Read(pcBytes, 0, bytesToRead);

                // 実際に読み取れたバイト数に合わせる
                int actualBytes = Math.Min(micRead, pcRead);
                if (actualBytes == 0)
                {
                    return;
                }

                // 16bitサンプルとしてミックス
                byte[] mixed = new byte[actualBytes];
                for (int i = 0; i < actualBytes; i += 2)
                {
                    short sample1 = BitConverter.ToInt16(micBytes, i);
                    short sample2 = BitConverter.ToInt16(pcBytes, i);
                    int mixedSample = sample1 + sample2;

                    // クリッピング防止
                    if (mixedSample > short.MaxValue)
                    {
                        mixedSample = short.MaxValue;
                    }
                    if (mixedSample < short.MinValue)
                    {
                        mixedSample = short.MinValue;
                    }

                    byte[] mixedBytes = BitConverter.GetBytes((short)mixedSample);
                    mixed[i] = mixedBytes[0];
                    mixed[i + 1] = mixedBytes[1];
                }

                // ファイルに書き込み
                _waveFileWriter.Write(mixed, 0, mixed.Length);
            }
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
                ProcessMixedAudio();  // ミックス処理を実行
            };

            // PCオーディオデータ受信イベント
            _wasapiCapture.DataAvailable += (s, args) =>
            {
                // ソースバッファに追加
                pcAudioSourceBuffer.AddSamples(args.Buffer, 0, args.BytesRecorded);

                // バッファに十分なデータがあればリサンプリングして16kHz Monoバッファに追加
                while (pcAudioSourceBuffer.BufferedBytes > 0)
                {
                    var buffer = new byte[4096];  // 固定サイズバッファ
                    int bytesRead = _resampler.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        _pcAudioBuffer.AddSamples(buffer, 0, bytesRead);
                    }
                    else
                    {
                        break;  // これ以上読み取れない
                    }
                }

                ProcessMixedAudio();  // ミックス処理を実行
            };

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

                // ミックス録音用バッファのクリーンアップ
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

                // PCオーディオ録音の場合はリサンプリング
                if (_currentRecordingMode == RecordingMode.PCAudio || _currentRecordingMode == RecordingMode.Mix)
                {
                    var tempPath = _recordingFilePath;
                    _recordingFilePath = ResampleToWhisperFormat(tempPath);

                    // 元ファイル削除
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch
                    {
                        // ファイル削除失敗は無視
                    }
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
        /// 録音後のWAVファイルをWhisper推奨フォーマット(16kHz Mono)にリサンプリング
        /// </summary>
        /// <param name="sourceFile">元のWAVファイルパス</param>
        /// <returns>リサンプリング後のWAVファイルパス</returns>
        private string ResampleToWhisperFormat(string sourceFile)
        {
            var outputPath = Path.Combine(Path.GetTempPath(), $"resampled_{Guid.NewGuid()}.wav");
            var targetFormat = new WaveFormat(16000, 16, 1);  // 16kHz, 16bit, Mono

            using (var reader = new WaveFileReader(sourceFile))
            using (var resampler = new MediaFoundationResampler(reader, targetFormat))
            {
                WaveFileWriter.CreateWaveFile(outputPath, resampler);
            }

            return outputPath;
        }

        /// <summary>
        /// エクスポートボタンクリック
        /// </summary>
        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null || _lastTranscriptionMetadata == null)
            {
                MessageBox.Show("エクスポートする文字起こし結果がありません", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // 保存形式選択ダイアログ
                using (var form = new Form())
                {
                    form.Text = "エクスポート形式を選択";
                    form.Size = new System.Drawing.Size(450, 280);
                    form.StartPosition = FormStartPosition.CenterParent;
                    form.FormBorderStyle = FormBorderStyle.FixedDialog;
                    form.MaximizeBox = false;
                    form.MinimizeBox = false;

                    var checkBoxJson = new CheckBox { Text = "JSON (.json) - 構造化データ", Location = new System.Drawing.Point(20, 20), Width = 400, Checked = true };
                    var checkBoxText = new CheckBox { Text = "テキスト (.txt) - 読みやすい形式", Location = new System.Drawing.Point(20, 50), Width = 400 };
                    var checkBoxCsv = new CheckBox { Text = "CSV (.csv) - Excel・データ分析向け", Location = new System.Drawing.Point(20, 80), Width = 400 };
                    var checkBoxSrt = new CheckBox { Text = "SRT (.srt) - 動画字幕形式", Location = new System.Drawing.Point(20, 110), Width = 400 };
                    var checkBoxAudio = new CheckBox { Text = "音声ファイルも一緒に保存", Location = new System.Drawing.Point(20, 150), Width = 400, Checked = true };

                    var btnOk = new Button { Text = "保存", Location = new System.Drawing.Point(150, 190), Width = 100, DialogResult = DialogResult.OK };
                    var btnCancel = new Button { Text = "キャンセル", Location = new System.Drawing.Point(260, 190), Width = 100, DialogResult = DialogResult.Cancel };

                    form.Controls.AddRange(new Control[] { checkBoxJson, checkBoxText, checkBoxCsv, checkBoxSrt, checkBoxAudio, btnOk, btnCancel });
                    form.AcceptButton = btnOk;
                    form.CancelButton = btnCancel;

                    if (form.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    // 少なくとも1つの形式が選択されているか確認
                    if (!checkBoxJson.Checked && !checkBoxText.Checked && !checkBoxCsv.Checked && !checkBoxSrt.Checked)
                    {
                        MessageBox.Show("少なくとも1つの形式を選択してください", "エラー",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 保存先フォルダ選択
                    using (var folderDialog = new FolderBrowserDialog())
                    {
                        folderDialog.Description = "保存先フォルダを選択してください";
                        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        var defaultPath = Path.Combine(documentsPath, "WhisperNetSample", "Transcriptions");
                        folderDialog.SelectedPath = defaultPath;

                        if (folderDialog.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }

                        // フォルダを作成
                        if (!Directory.Exists(folderDialog.SelectedPath))
                        {
                            Directory.CreateDirectory(folderDialog.SelectedPath);
                        }

                        // ファイル名のベース（タイムスタンプ）
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        var baseName = $"{timestamp}_transcript";
                        var dt = dataGridView1.DataSource as DataTable;

                        var savedFiles = new System.Collections.Generic.List<string>();

                        // 各形式で保存
                        if (checkBoxJson.Checked)
                        {
                            var filePath = Path.Combine(folderDialog.SelectedPath, $"{baseName}.json");
                            TranscriptionExporter.SaveAsJson(dt, filePath, _lastTranscriptionMetadata);
                            savedFiles.Add(filePath);
                        }

                        if (checkBoxText.Checked)
                        {
                            var filePath = Path.Combine(folderDialog.SelectedPath, $"{baseName}.txt");
                            TranscriptionExporter.SaveAsText(dt, filePath, _lastTranscriptionMetadata);
                            savedFiles.Add(filePath);
                        }

                        if (checkBoxCsv.Checked)
                        {
                            var filePath = Path.Combine(folderDialog.SelectedPath, $"{baseName}.csv");
                            TranscriptionExporter.SaveAsCsv(dt, filePath);
                            savedFiles.Add(filePath);
                        }

                        if (checkBoxSrt.Checked)
                        {
                            var filePath = Path.Combine(folderDialog.SelectedPath, $"{baseName}.srt");
                            TranscriptionExporter.SaveAsSrt(dt, filePath);
                            savedFiles.Add(filePath);
                        }

                        // 音声ファイルもコピー
                        if (checkBoxAudio.Checked && !string.IsNullOrEmpty(_lastTranscriptionMetadata.AudioFilePath))
                        {
                            try
                            {
                                var audioFileName = $"{timestamp}_recording{Path.GetExtension(_lastTranscriptionMetadata.AudioFilePath)}";
                                var audioDestPath = Path.Combine(folderDialog.SelectedPath, audioFileName);
                                File.Copy(_lastTranscriptionMetadata.AudioFilePath, audioDestPath, true);
                                savedFiles.Add(audioDestPath);
                            }
                            catch (Exception ex)
                            {
                                // 音声ファイルコピー失敗は警告のみ
                                MessageBox.Show($"音声ファイルのコピーに失敗しました: {ex.Message}", "警告",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }

                        // 完了メッセージ
                        var message = $"エクスポート完了!\n\n保存先:\n{folderDialog.SelectedPath}\n\n保存ファイル数: {savedFiles.Count}";
                        MessageBox.Show(message, "エクスポート完了",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // フォルダを開く
                        System.Diagnostics.Process.Start("explorer.exe", folderDialog.SelectedPath);

                        UpdateStatus($"エクスポート完了 ({savedFiles.Count}ファイル)", true);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エクスポートに失敗しました。\n\n{ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus($"エクスポートエラー: {ex.Message}", false);
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

        #region 音声コマンド機能

        /// <summary>
        /// 音声コマンドの初期化
        /// </summary>
        private void InitializeVoiceCommands()
        {
            _voiceCommandManager = new VoiceCommandManager();

            // イベントハンドラ登録
            _voiceCommandManager.CommandRecognized += OnCommandRecognized;
            _voiceCommandManager.CommandNotFound += OnCommandNotFound;

            // コマンドを登録
            RegisterVoiceCommands();
        }

        /// <summary>
        /// 音声コマンドを登録
        /// </summary>
        private void RegisterVoiceCommands()
        {
            // ダイアログ操作コマンド
            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "ポップアップを開いて", "ダイアログ表示", "ダイアログを開く", "ポップアップ表示" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => btnOpenPopup.PerformClick()));
                    }
                    else
                    {
                        btnOpenPopup.PerformClick();
                    }
                },
                Description = "ポップアップダイアログを表示します",
                Category = "ダイアログ操作"
            });

            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "ポップアップを閉じて", "ダイアログを閉じる", "閉じて" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            if (_activePopupForm != null && !_activePopupForm.IsDisposed)
                            {
                                _activePopupForm.Close();
                                _activePopupForm = null;
                            }
                            else
                            {
                                UpdateVoiceCommandStatus("閉じるポップアップがありません", false);
                            }
                        }));
                    }
                    else
                    {
                        if (_activePopupForm != null && !_activePopupForm.IsDisposed)
                        {
                            _activePopupForm.Close();
                            _activePopupForm = null;
                        }
                        else
                        {
                            UpdateVoiceCommandStatus("閉じるポップアップがありません", false);
                        }
                    }
                },
                Description = "開いているポップアップを閉じます",
                Category = "ダイアログ操作"
            });

            // アプリ内操作コマンド
            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "録音開始", "録音スタート", "レコーディング開始" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => btnStartRecording.PerformClick()));
                    }
                    else
                    {
                        btnStartRecording.PerformClick();
                    }
                },
                Description = "音声の録音を開始します",
                Category = "アプリ操作"
            });

            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "録音停止", "ストップ", "録音終了" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => btnStopRecording.PerformClick()));
                    }
                    else
                    {
                        btnStopRecording.PerformClick();
                    }
                },
                Description = "録音を停止します",
                Category = "アプリ操作"
            });

            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "文字起こし実行", "文字起こし", "トランスクリプション実行" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => btnTranscribe.PerformClick()));
                    }
                    else
                    {
                        btnTranscribe.PerformClick();
                    }
                },
                Description = "音声ファイルの文字起こしを実行します",
                Category = "アプリ操作"
            });

            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "結果をエクスポート", "エクスポート", "保存" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() => btnExport.PerformClick()));
                    }
                    else
                    {
                        btnExport.PerformClick();
                    }
                },
                Description = "文字起こし結果をエクスポートします",
                Category = "アプリ操作"
            });

            // ヘルプコマンド
            _voiceCommandManager.RegisterCommand(new VoiceCommand
            {
                TriggerPhrases = new[] { "ヘルプ", "コマンド一覧", "使い方" },
                Action = () =>
                {
                    if (InvokeRequired)
                    {
                        Invoke(new Action(() =>
                        {
                            var helpMessage = _voiceCommandManager.GenerateHelpMessage();
                            MessageBox.Show(helpMessage, "音声コマンド一覧",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }));
                    }
                    else
                    {
                        var helpMessage = _voiceCommandManager.GenerateHelpMessage();
                        MessageBox.Show(helpMessage, "音声コマンド一覧",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                },
                Description = "利用可能な音声コマンドの一覧を表示します",
                Category = "ヘルプ"
            });
        }

        /// <summary>
        /// ヘルプボタンクリック
        /// </summary>
        private void btnVoiceCommandHelp_Click(object sender, EventArgs e)
        {
            var helpMessage = _voiceCommandManager.GenerateHelpMessage();
            MessageBox.Show(helpMessage, "音声コマンド一覧",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// コマンド認識成功時のイベントハンドラ
        /// </summary>
        private void OnCommandRecognized(object sender, VoiceCommandRecognizedEventArgs e)
        {
            if (e.IsSuccess)
            {
                UpdateVoiceCommandStatus($"✓ {e.Command.Description}", true);
            }
            else
            {
                UpdateVoiceCommandStatus($"✗ コマンド実行エラー: {e.Error?.Message}", false);
            }
        }

        /// <summary>
        /// コマンドが見つからなかった時のイベントハンドラ
        /// </summary>
        private void OnCommandNotFound(object sender, string recognizedText)
        {
            UpdateVoiceCommandStatus($"❓ コマンドが見つかりませんでした: 「{recognizedText}」", false);
        }

        /// <summary>
        /// 音声コマンドステータス更新
        /// </summary>
        private void UpdateVoiceCommandStatus(string message, bool isSuccess, bool isRecording = false, System.Drawing.Color? customColor = null)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateVoiceCommandStatus(message, isSuccess, isRecording, customColor)));
                return;
            }

            labelVoiceCommandStatus.Text = message;

            // 色の設定
            if (customColor.HasValue)
            {
                labelVoiceCommandStatus.ForeColor = customColor.Value;
            }
            else if (isRecording)
            {
                labelVoiceCommandStatus.ForeColor = System.Drawing.Color.Red;
            }
            else if (isSuccess)
            {
                labelVoiceCommandStatus.ForeColor = System.Drawing.Color.Green;
            }
            else
            {
                labelVoiceCommandStatus.ForeColor = System.Drawing.Color.Red;
            }
        }

        /// <summary>
        /// 音声コマンドボタン押下（録音開始）
        /// </summary>
        private void btnVoiceCommand_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            try
            {
                // マイクデバイスの存在確認
                if (WaveInEvent.DeviceCount == 0)
                {
                    UpdateVoiceCommandStatus("マイクが見つかりません", false);
                    return;
                }

                // 録音ファイルパスを生成
                _voiceCommandFilePath = Path.Combine(Path.GetTempPath(), $"voice_command_{Guid.NewGuid()}.wav");

                // WaveInEventを初期化（16kHz Mono、Whisper推奨設定）
                _voiceCommandWaveIn = new WaveInEvent();
                _voiceCommandWaveIn.WaveFormat = new WaveFormat(16000, 16, 1);

                // ファイルライターを初期化
                _voiceCommandFileWriter = new WaveFileWriter(_voiceCommandFilePath, _voiceCommandWaveIn.WaveFormat);

                // データ受信イベントハンドラ
                _voiceCommandWaveIn.DataAvailable += (s, args) =>
                {
                    _voiceCommandFileWriter.Write(args.Buffer, 0, args.BytesRecorded);
                };

                // ボタンの外観を録音中に変更
                btnVoiceCommand.BackColor = System.Drawing.Color.Red;
                btnVoiceCommand.ForeColor = System.Drawing.Color.White;

                // 録音開始
                _voiceCommandWaveIn.StartRecording();
                UpdateVoiceCommandStatus("🔴 録音中... 話してください", false, true);
            }
            catch (Exception ex)
            {
                // エラー時は外観を元に戻す
                btnVoiceCommand.BackColor = System.Drawing.SystemColors.Control;
                btnVoiceCommand.ForeColor = System.Drawing.SystemColors.ControlText;
                UpdateVoiceCommandStatus($"✗ 録音開始エラー: {ex.Message}", false);
            }
        }

        /// <summary>
        /// 音声コマンドボタン離す（録音停止→認識実行）
        /// </summary>
        private async void btnVoiceCommand_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            try
            {
                // ボタンの外観を元に戻す
                btnVoiceCommand.BackColor = System.Drawing.SystemColors.Control;
                btnVoiceCommand.ForeColor = System.Drawing.SystemColors.ControlText;
                btnVoiceCommand.UseVisualStyleBackColor = true;

                // 録音停止
                if (_voiceCommandWaveIn != null)
                {
                    _voiceCommandWaveIn.StopRecording();
                    _voiceCommandWaveIn.Dispose();
                    _voiceCommandWaveIn = null;
                }

                if (_voiceCommandFileWriter != null)
                {
                    _voiceCommandFileWriter.Dispose();
                    _voiceCommandFileWriter = null;
                }

                // ボタンを一時的に無効化（認識中の誤操作防止）
                btnVoiceCommand.Enabled = false;
                UpdateVoiceCommandStatus("⏳ 認識中...", false, false, System.Drawing.Color.Blue);

                // 音声認識実行
                await ProcessVoiceCommandAsync(_voiceCommandFilePath);
            }
            catch (Exception ex)
            {
                UpdateVoiceCommandStatus($"✗ 認識エラー: {ex.Message}", false);
            }
            finally
            {
                // ボタンを再度有効化
                btnVoiceCommand.Enabled = true;

                // 一時ファイル削除
                try
                {
                    if (File.Exists(_voiceCommandFilePath))
                    {
                        File.Delete(_voiceCommandFilePath);
                    }
                }
                catch
                {
                    // ファイル削除失敗は無視
                }
            }
        }

        /// <summary>
        /// 音声コマンドの認識処理
        /// </summary>
        private async Task ProcessVoiceCommandAsync(string audioFilePath)
        {
            if (_whisperFactory == null)
            {
                UpdateVoiceCommandStatus("✗ Whisperモデルが初期化されていません", false);
                return;
            }

            try
            {
                // WhisperProcessorを作成
                using (var processor = _whisperFactory.CreateBuilder()
                    .WithLanguage("ja")
                    .Build())
                {
                    var recognizedText = "";

                    // 音声ファイルを処理
                    using (var fileStream = File.OpenRead(audioFilePath))
                    {
                        var enumerator = processor.ProcessAsync(fileStream).GetAsyncEnumerator();
                        try
                        {
                            while (await enumerator.MoveNextAsync())
                            {
                                var result = enumerator.Current;
                                recognizedText += result.Text + " ";
                            }
                        }
                        finally
                        {
                            await enumerator.DisposeAsync();
                        }
                    }

                    // 認識結果を処理
                    if (!string.IsNullOrWhiteSpace(recognizedText))
                    {
                        var trimmedText = recognizedText.Trim();
                        UpdateVoiceCommandStatus($"💬 認識: 「{trimmedText}」", false, false, System.Drawing.Color.DarkBlue);

                        // コマンドとして処理
                        var commandExecuted = _voiceCommandManager.ProcessRecognizedText(trimmedText);

                        if (!commandExecuted)
                        {
                            // コマンドが見つからなかった場合のメッセージは
                            // OnCommandNotFoundイベントハンドラで処理される
                        }
                    }
                    else
                    {
                        UpdateVoiceCommandStatus("✗ 音声が認識できませんでした", false);
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateVoiceCommandStatus($"✗ 認識エラー: {ex.Message}", false);
            }
        }

        #endregion
    }
}
