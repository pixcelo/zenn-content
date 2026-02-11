namespace WhisperNetSample
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 録音デバイスのクリーンアップ
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                }

                if (_waveFileWriter != null)
                {
                    _waveFileWriter.Dispose();
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnTest = new System.Windows.Forms.Button();
            this.btnOpenPopup = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.statusLabel = new System.Windows.Forms.Label();
            this.labelModelType = new System.Windows.Forms.Label();
            this.modelTypeComboBox = new System.Windows.Forms.ComboBox();
            this.btnSelectAudioFile = new System.Windows.Forms.Button();
            this.btnTranscribe = new System.Windows.Forms.Button();
            this.txtSelectedFile = new System.Windows.Forms.TextBox();
            this.labelSelectedFile = new System.Windows.Forms.Label();
            this.btnStartRecording = new System.Windows.Forms.Button();
            this.btnStopRecording = new System.Windows.Forms.Button();
            this.labelRecordingStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            //
            // btnTest
            //
            this.btnTest.Location = new System.Drawing.Point(12, 12);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(100, 30);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "テストボタン";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            //
            // btnOpenPopup
            //
            this.btnOpenPopup.Location = new System.Drawing.Point(130, 12);
            this.btnOpenPopup.Name = "btnOpenPopup";
            this.btnOpenPopup.Size = new System.Drawing.Size(120, 30);
            this.btnOpenPopup.TabIndex = 2;
            this.btnOpenPopup.Text = "ポップアップ表示";
            this.btnOpenPopup.UseVisualStyleBackColor = true;
            this.btnOpenPopup.Click += new System.EventHandler(this.btnOpenPopup_Click);
            //
            // dataGridView1
            //
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 110);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(760, 285);
            this.dataGridView1.TabIndex = 1;
            //
            // statusLabel
            //
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(12, 405);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(100, 12);
            this.statusLabel.TabIndex = 3;
            this.statusLabel.Text = "初期化中...";
            //
            // labelModelType
            //
            this.labelModelType.AutoSize = true;
            this.labelModelType.Location = new System.Drawing.Point(270, 18);
            this.labelModelType.Name = "labelModelType";
            this.labelModelType.Size = new System.Drawing.Size(40, 12);
            this.labelModelType.TabIndex = 4;
            this.labelModelType.Text = "モデル:";
            //
            // modelTypeComboBox
            //
            this.modelTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modelTypeComboBox.FormattingEnabled = true;
            this.modelTypeComboBox.Location = new System.Drawing.Point(316, 15);
            this.modelTypeComboBox.Name = "modelTypeComboBox";
            this.modelTypeComboBox.Size = new System.Drawing.Size(200, 20);
            this.modelTypeComboBox.TabIndex = 5;
            this.modelTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.modelTypeComboBox_SelectedIndexChanged);
            //
            // labelSelectedFile
            //
            this.labelSelectedFile.AutoSize = true;
            this.labelSelectedFile.Location = new System.Drawing.Point(12, 55);
            this.labelSelectedFile.Name = "labelSelectedFile";
            this.labelSelectedFile.Size = new System.Drawing.Size(88, 12);
            this.labelSelectedFile.TabIndex = 6;
            this.labelSelectedFile.Text = "音声ファイル:";
            //
            // txtSelectedFile
            //
            this.txtSelectedFile.Location = new System.Drawing.Point(106, 52);
            this.txtSelectedFile.Name = "txtSelectedFile";
            this.txtSelectedFile.ReadOnly = true;
            this.txtSelectedFile.Size = new System.Drawing.Size(410, 19);
            this.txtSelectedFile.TabIndex = 7;
            this.txtSelectedFile.Text = "（未選択）";
            //
            // btnSelectAudioFile
            //
            this.btnSelectAudioFile.Location = new System.Drawing.Point(530, 50);
            this.btnSelectAudioFile.Name = "btnSelectAudioFile";
            this.btnSelectAudioFile.Size = new System.Drawing.Size(100, 23);
            this.btnSelectAudioFile.TabIndex = 8;
            this.btnSelectAudioFile.Text = "ファイル選択";
            this.btnSelectAudioFile.UseVisualStyleBackColor = true;
            this.btnSelectAudioFile.Click += new System.EventHandler(this.btnSelectAudioFile_Click);
            //
            // btnTranscribe
            //
            this.btnTranscribe.Enabled = false;
            this.btnTranscribe.Location = new System.Drawing.Point(645, 50);
            this.btnTranscribe.Name = "btnTranscribe";
            this.btnTranscribe.Size = new System.Drawing.Size(120, 23);
            this.btnTranscribe.TabIndex = 9;
            this.btnTranscribe.Text = "文字起こし実行";
            this.btnTranscribe.UseVisualStyleBackColor = true;
            this.btnTranscribe.Click += new System.EventHandler(this.btnTranscribe_Click);
            //
            // btnStartRecording
            //
            this.btnStartRecording.Location = new System.Drawing.Point(12, 75);
            this.btnStartRecording.Name = "btnStartRecording";
            this.btnStartRecording.Size = new System.Drawing.Size(120, 23);
            this.btnStartRecording.TabIndex = 10;
            this.btnStartRecording.Text = "🎤 録音開始";
            this.btnStartRecording.UseVisualStyleBackColor = true;
            this.btnStartRecording.Click += new System.EventHandler(this.btnStartRecording_Click);
            //
            // btnStopRecording
            //
            this.btnStopRecording.Enabled = false;
            this.btnStopRecording.Location = new System.Drawing.Point(145, 75);
            this.btnStopRecording.Name = "btnStopRecording";
            this.btnStopRecording.Size = new System.Drawing.Size(100, 23);
            this.btnStopRecording.TabIndex = 11;
            this.btnStopRecording.Text = "⏹ 停止";
            this.btnStopRecording.UseVisualStyleBackColor = true;
            this.btnStopRecording.Click += new System.EventHandler(this.btnStopRecording_Click);
            //
            // labelRecordingStatus
            //
            this.labelRecordingStatus.AutoSize = true;
            this.labelRecordingStatus.Location = new System.Drawing.Point(260, 80);
            this.labelRecordingStatus.Name = "labelRecordingStatus";
            this.labelRecordingStatus.Size = new System.Drawing.Size(41, 12);
            this.labelRecordingStatus.TabIndex = 12;
            this.labelRecordingStatus.Text = "待機中";
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 450);
            this.Controls.Add(this.labelRecordingStatus);
            this.Controls.Add(this.btnStopRecording);
            this.Controls.Add(this.btnStartRecording);
            this.Controls.Add(this.btnTranscribe);
            this.Controls.Add(this.btnSelectAudioFile);
            this.Controls.Add(this.txtSelectedFile);
            this.Controls.Add(this.labelSelectedFile);
            this.Controls.Add(this.modelTypeComboBox);
            this.Controls.Add(this.labelModelType);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.btnOpenPopup);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.btnTest);
            this.Name = "Form1";
            this.Text = "Whisper.net サンプル (.NET Framework 4.8)";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnOpenPopup;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Label labelModelType;
        private System.Windows.Forms.ComboBox modelTypeComboBox;
        private System.Windows.Forms.Label labelSelectedFile;
        private System.Windows.Forms.TextBox txtSelectedFile;
        private System.Windows.Forms.Button btnSelectAudioFile;
        private System.Windows.Forms.Button btnTranscribe;
        private System.Windows.Forms.Button btnStartRecording;
        private System.Windows.Forms.Button btnStopRecording;
        private System.Windows.Forms.Label labelRecordingStatus;
    }
}
