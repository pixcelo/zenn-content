namespace WhisperNetSample
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.dataGridView1.Location = new System.Drawing.Point(12, 60);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowTemplate.Height = 21;
            this.dataGridView1.Size = new System.Drawing.Size(760, 335);
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
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 450);
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
    }
}
