using System;
using System.Data;
using System.Windows.Forms;

namespace WhisperNetSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // DataGridViewの初期設定
            InitializeDataGridView();
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
    }
}
