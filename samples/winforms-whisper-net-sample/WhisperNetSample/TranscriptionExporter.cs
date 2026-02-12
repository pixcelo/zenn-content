using System;
using System.Data;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Linq;

namespace WhisperNetSample
{
    /// <summary>
    /// 文字起こし結果のエクスポート機能
    /// </summary>
    public class TranscriptionExporter
    {
        /// <summary>
        /// テキスト形式で保存
        /// </summary>
        public static void SaveAsText(DataTable transcriptionData, string filePath, TranscriptionMetadata metadata = null)
        {
            var sb = new StringBuilder();

            // メタデータがあれば追記
            if (metadata != null)
            {
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine($"文字起こし日時: {metadata.CreatedAt:yyyy/MM/dd HH:mm:ss}");
                sb.AppendLine($"モデル: {metadata.ModelName}");
                sb.AppendLine($"言語: {metadata.Language}");
                sb.AppendLine($"総時間: {metadata.TotalDuration:mm\\:ss\\.ff}");
                sb.AppendLine($"セグメント数: {metadata.SegmentCount}");
                sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
                sb.AppendLine();
            }

            // データ行を追加
            foreach (DataRow row in transcriptionData.Rows)
            {
                var startTime = row["開始時刻"].ToString();
                var endTime = row["終了時刻"].ToString();
                var text = row["テキスト"].ToString();

                sb.AppendLine($"{startTime} - {endTime}");
                sb.AppendLine(text);
                sb.AppendLine();
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// CSV形式で保存
        /// </summary>
        public static void SaveAsCsv(DataTable transcriptionData, string filePath)
        {
            var sb = new StringBuilder();

            // ヘッダー行
            sb.AppendLine("開始時刻,終了時刻,テキスト,長さ(秒)");

            // データ行
            foreach (DataRow row in transcriptionData.Rows)
            {
                var startTime = row["開始時刻"].ToString();
                var endTime = row["終了時刻"].ToString();
                var text = row["テキスト"].ToString();

                // 長さを計算（秒単位）
                var duration = CalculateDurationInSeconds(startTime, endTime);

                // CSVエスケープ（ダブルクォートとカンマ対応）
                var escapedText = EscapeCsv(text);

                sb.AppendLine($"{startTime},{endTime},{escapedText},{duration:F2}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// JSON形式で保存
        /// </summary>
        public static void SaveAsJson(DataTable transcriptionData, string filePath, TranscriptionMetadata metadata)
        {
            var segments = transcriptionData.AsEnumerable().Select(row =>
            {
                var startTime = row["開始時刻"].ToString();
                var endTime = row["終了時刻"].ToString();

                return new
                {
                    start = startTime,
                    end = endTime,
                    duration = CalculateDurationInSeconds(startTime, endTime),
                    text = row["テキスト"].ToString()
                };
            }).ToList();

            var jsonData = new
            {
                metadata = new
                {
                    created_at = metadata.CreatedAt.ToString("yyyy-MM-ddTHH:mm:sszzz"),
                    model = metadata.ModelName,
                    language = metadata.Language,
                    audio_file = Path.GetFileName(metadata.AudioFilePath),
                    total_duration = metadata.TotalDuration.ToString(@"mm\:ss\.ff"),
                    segment_count = metadata.SegmentCount
                },
                segments
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(jsonData, options);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        /// <summary>
        /// SRT字幕形式で保存
        /// </summary>
        public static void SaveAsSrt(DataTable transcriptionData, string filePath)
        {
            var sb = new StringBuilder();
            int index = 1;

            foreach (DataRow row in transcriptionData.Rows)
            {
                var startTime = row["開始時刻"].ToString();
                var endTime = row["終了時刻"].ToString();
                var text = row["テキスト"].ToString();

                // SRT形式のタイムスタンプに変換（mm:ss.ff → 00:00:00,000）
                var srtStart = ConvertToSrtTime(startTime);
                var srtEnd = ConvertToSrtTime(endTime);

                sb.AppendLine(index.ToString());
                sb.AppendLine($"{srtStart} --> {srtEnd}");
                sb.AppendLine(text);
                sb.AppendLine();

                index++;
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }

        /// <summary>
        /// 音声ファイルをコピー
        /// </summary>
        public static void CopyAudioFile(string sourceFilePath, string destinationFolder)
        {
            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("音声ファイルが見つかりません", sourceFilePath);
            }

            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            var fileName = Path.GetFileName(sourceFilePath);
            var destinationPath = Path.Combine(destinationFolder, fileName);

            File.Copy(sourceFilePath, destinationPath, overwrite: true);
        }

        /// <summary>
        /// CSVフィールドのエスケープ
        /// </summary>
        private static string EscapeCsv(string field)
        {
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                return "\"" + field.Replace("\"", "\"\"") + "\"";
            }
            return field;
        }

        /// <summary>
        /// mm:ss.ff形式から秒数を計算
        /// </summary>
        private static double CalculateDurationInSeconds(string startTime, string endTime)
        {
            var start = ParseTime(startTime);
            var end = ParseTime(endTime);
            return (end - start).TotalSeconds;
        }

        /// <summary>
        /// mm:ss.ff形式をTimeSpanにパース
        /// </summary>
        private static TimeSpan ParseTime(string timeStr)
        {
            if (TimeSpan.TryParseExact(timeStr, @"mm\:ss\.ff", null, out var result))
            {
                return result;
            }
            return TimeSpan.Zero;
        }

        /// <summary>
        /// mm:ss.ff → SRT形式（00:00:00,000）に変換
        /// </summary>
        private static string ConvertToSrtTime(string timeStr)
        {
            var timeSpan = ParseTime(timeStr);
            var hours = (int)timeSpan.TotalHours;
            var minutes = timeSpan.Minutes;
            var seconds = timeSpan.Seconds;
            var milliseconds = timeSpan.Milliseconds;

            return $"{hours:D2}:{minutes:D2}:{seconds:D2},{milliseconds:D3}";
        }
    }
}
