// 検証用: 各重大度レベルでの自動修正をテスト
// このファイルで検証してください

namespace EditorConfigTest
{
    // 検証1: usingがnamespace内にある → 自動修正でnamespace外に移動されるか？
    using System;
    using System.Collections.Generic;

    class TestAutoFix
    {
        static void TestMethod()
        {
            // 検証2: ターゲット型new演算子
            // 保存時に new() に自動変換されるか？
            List<string> names = new List<string>();
            Dictionary<int, string> map = new Dictionary<int, string>();

            // 検証3: 中括弧の省略
            // 保存時に中括弧が自動追加されるか？
            if (names.Count > 0)
                Console.WriteLine("Items: " + names.Count);

            // 検証4: mapも使用する
            map.Add(1, "test");
        }

        static void Main(string[] args)
        {
            TestMethod();
        }
    }
}
