using System;

namespace WhisperNetSample
{
    /// <summary>
    /// 文字起こし結果のメタデータ
    /// </summary>
    public class TranscriptionMetadata
    {
        /// <summary>
        /// 作成日時
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 使用したWhisperモデル名
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 言語コード（例: "ja", "en"）
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 元の音声ファイルパス
        /// </summary>
        public string AudioFilePath { get; set; }

        /// <summary>
        /// 音声の総再生時間
        /// </summary>
        public TimeSpan TotalDuration { get; set; }

        /// <summary>
        /// セグメント数
        /// </summary>
        public int SegmentCount { get; set; }

        public TranscriptionMetadata()
        {
            CreatedAt = DateTime.Now;
            Language = "ja";
        }
    }
}
