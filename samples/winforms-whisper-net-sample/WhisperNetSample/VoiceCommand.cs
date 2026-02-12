using System;

namespace WhisperNetSample
{
    /// <summary>
    /// 音声コマンドの定義
    /// </summary>
    public class VoiceCommand
    {
        /// <summary>
        /// コマンドを起動するトリガーフレーズの配列
        /// 例: new[] { "ポップアップを開いて", "ダイアログ表示", "ダイアログを開く" }
        /// </summary>
        public string[] TriggerPhrases { get; set; }

        /// <summary>
        /// コマンドが認識されたときに実行されるアクション
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// コマンドの説明（ヘルプ表示用）
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// コマンドのカテゴリ（分類用）
        /// 例: "ダイアログ操作", "アプリ操作", "システム"
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// コマンドが有効かどうか
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// 認識されたテキストがこのコマンドにマッチするかを判定
        /// </summary>
        /// <param name="recognizedText">音声認識結果のテキスト</param>
        /// <returns>マッチした場合true</returns>
        public bool Matches(string recognizedText)
        {
            if (string.IsNullOrWhiteSpace(recognizedText) || !IsEnabled)
                return false;

            // 認識テキストを正規化（空白除去、小文字化）
            var normalizedText = recognizedText.Trim().ToLower();

            foreach (var phrase in TriggerPhrases)
            {
                if (string.IsNullOrWhiteSpace(phrase))
                    continue;

                var normalizedPhrase = phrase.Trim().ToLower();

                // 完全一致または部分一致で判定
                if (normalizedText.Contains(normalizedPhrase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// コマンドを実行
        /// </summary>
        public void Execute()
        {
            if (IsEnabled && Action != null)
            {
                Action.Invoke();
            }
        }

        /// <summary>
        /// ヘルプ表示用の文字列表現
        /// </summary>
        public override string ToString()
        {
            if (TriggerPhrases == null || TriggerPhrases.Length == 0)
                return $"{Category}: {Description}";

            var triggers = string.Join("、", TriggerPhrases);
            return $"[{Category}] {triggers}\n  → {Description}";
        }
    }
}
