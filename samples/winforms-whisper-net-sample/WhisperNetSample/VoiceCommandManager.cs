using System;
using System.Collections.Generic;
using System.Linq;

namespace WhisperNetSample
{
    /// <summary>
    /// 音声コマンドの管理クラス
    /// コマンドの登録、認識、実行を担当
    /// </summary>
    public class VoiceCommandManager
    {
        private readonly List<VoiceCommand> _commands = new List<VoiceCommand>();

        /// <summary>
        /// 登録済みコマンドの一覧を取得
        /// </summary>
        public IReadOnlyList<VoiceCommand> Commands => _commands.AsReadOnly();

        /// <summary>
        /// コマンドが認識されたときに発火するイベント
        /// </summary>
        public event EventHandler<VoiceCommandRecognizedEventArgs> CommandRecognized;

        /// <summary>
        /// コマンドが見つからなかったときに発火するイベント
        /// </summary>
        public event EventHandler<string> CommandNotFound;

        /// <summary>
        /// 音声コマンドを登録
        /// </summary>
        /// <param name="command">登録するコマンド</param>
        public void RegisterCommand(VoiceCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.TriggerPhrases == null || command.TriggerPhrases.Length == 0)
                throw new ArgumentException("TriggerPhrasesが空です", nameof(command));

            if (command.Action == null)
                throw new ArgumentException("Actionが未設定です", nameof(command));

            _commands.Add(command);
        }

        /// <summary>
        /// 複数のコマンドを一括登録
        /// </summary>
        public void RegisterCommands(IEnumerable<VoiceCommand> commands)
        {
            foreach (var command in commands)
            {
                RegisterCommand(command);
            }
        }

        /// <summary>
        /// 認識されたテキストからコマンドを検索して実行
        /// </summary>
        /// <param name="recognizedText">音声認識結果のテキスト</param>
        /// <returns>コマンドが見つかって実行された場合true</returns>
        public bool ProcessRecognizedText(string recognizedText)
        {
            if (string.IsNullOrWhiteSpace(recognizedText))
                return false;

            // マッチするコマンドを検索（最初にマッチしたもの）
            var matchedCommand = _commands.FirstOrDefault(cmd => cmd.Matches(recognizedText));

            if (matchedCommand != null)
            {
                try
                {
                    // コマンドを実行
                    matchedCommand.Execute();

                    // イベント発火
                    CommandRecognized?.Invoke(this, new VoiceCommandRecognizedEventArgs
                    {
                        Command = matchedCommand,
                        RecognizedText = recognizedText
                    });

                    return true;
                }
                catch (Exception ex)
                {
                    // コマンド実行エラーはイベントとして通知
                    CommandRecognized?.Invoke(this, new VoiceCommandRecognizedEventArgs
                    {
                        Command = matchedCommand,
                        RecognizedText = recognizedText,
                        Error = ex
                    });

                    return false;
                }
            }
            else
            {
                // コマンドが見つからなかった
                CommandNotFound?.Invoke(this, recognizedText);
                return false;
            }
        }

        /// <summary>
        /// 指定したカテゴリのコマンド一覧を取得
        /// </summary>
        public IEnumerable<VoiceCommand> GetCommandsByCategory(string category)
        {
            return _commands.Where(cmd =>
                string.Equals(cmd.Category, category, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// すべてのカテゴリを取得
        /// </summary>
        public IEnumerable<string> GetCategories()
        {
            return _commands
                .Select(cmd => cmd.Category)
                .Where(cat => !string.IsNullOrEmpty(cat))
                .Distinct()
                .OrderBy(cat => cat);
        }

        /// <summary>
        /// ヘルプメッセージを生成（全コマンド一覧）
        /// </summary>
        public string GenerateHelpMessage()
        {
            if (_commands.Count == 0)
                return "登録されているコマンドはありません。";

            var categories = GetCategories().ToList();
            var helpLines = new List<string>();

            helpLines.Add("=== 利用可能な音声コマンド ===\n");

            foreach (var category in categories)
            {
                helpLines.Add($"【{category}】");
                var categoryCommands = GetCommandsByCategory(category);

                foreach (var cmd in categoryCommands)
                {
                    if (cmd.IsEnabled)
                    {
                        var triggers = string.Join("、", cmd.TriggerPhrases.Take(3));  // 最大3つまで表示
                        helpLines.Add($"  「{triggers}」");
                        helpLines.Add($"    → {cmd.Description}");
                    }
                }

                helpLines.Add("");  // カテゴリ間に空行
            }

            helpLines.Add("※ 🎤ボタンを押しながら、上記のコマンドを話してください。");

            return string.Join("\n", helpLines);
        }

        /// <summary>
        /// すべてのコマンドをクリア
        /// </summary>
        public void Clear()
        {
            _commands.Clear();
        }

        /// <summary>
        /// 特定のコマンドを削除
        /// </summary>
        public bool RemoveCommand(VoiceCommand command)
        {
            return _commands.Remove(command);
        }

        /// <summary>
        /// すべてのコマンドを有効化
        /// </summary>
        public void EnableAllCommands()
        {
            foreach (var cmd in _commands)
            {
                cmd.IsEnabled = true;
            }
        }

        /// <summary>
        /// すべてのコマンドを無効化
        /// </summary>
        public void DisableAllCommands()
        {
            foreach (var cmd in _commands)
            {
                cmd.IsEnabled = false;
            }
        }
    }

    /// <summary>
    /// コマンド認識イベントの引数
    /// </summary>
    public class VoiceCommandRecognizedEventArgs : EventArgs
    {
        /// <summary>
        /// 認識されたコマンド
        /// </summary>
        public VoiceCommand Command { get; set; }

        /// <summary>
        /// 音声認識結果のテキスト
        /// </summary>
        public string RecognizedText { get; set; }

        /// <summary>
        /// コマンド実行時のエラー（エラーがない場合はnull）
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// コマンド実行が成功したかどうか
        /// </summary>
        public bool IsSuccess => Error == null;
    }
}
