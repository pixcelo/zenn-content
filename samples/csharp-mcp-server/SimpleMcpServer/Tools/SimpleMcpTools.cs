using System.ComponentModel;
using ModelContextProtocol.Server;
using Microsoft.Extensions.Logging;

/// <summary>
/// MCP診断用の最小限ツールクラス
/// </summary>
internal class SimpleMcpTools
{
    private readonly ILogger<SimpleMcpTools> _logger;

    public SimpleMcpTools(ILogger<SimpleMcpTools> logger)
    {
        _logger = logger;
        _logger.LogInformation("🚀 SimpleMcpTools constructed successfully!");
        Console.Error.WriteLine("🚀 SimpleMcpTools constructed successfully!");
    }

    [McpServerTool]
    [Description("MCPサーバーの基本動作確認")]
    public async Task<string> Ping()
    {
        _logger.LogInformation("🏓 SimpleMcpTools.Ping called!");
        Console.Error.WriteLine("🏓 SimpleMcpTools.Ping called!");
        await Task.Delay(10);
        return "🏓 Pong from SimpleMcpTools!";
    }

    [McpServerTool]
    [Description("文字列を返すシンプルなテスト")]
    public async Task<string> Echo(
        [Description("エコーするメッセージ")] string message = "Hello World")
    {
        _logger.LogInformation("📢 SimpleMcpTools.Echo called with: {Message}", message);
        Console.Error.WriteLine($"📢 SimpleMcpTools.Echo called with: {message}");
        await Task.Delay(10);
        return $"Echo: {message}";
    }
}