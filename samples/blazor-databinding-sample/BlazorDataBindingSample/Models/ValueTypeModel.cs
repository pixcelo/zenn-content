namespace BlazorDataBindingSample.Models;

/// <summary>
/// 値型のstruct（実験用）
/// </summary>
public struct ValueTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Value { get; set; }

    public ValueTypeModel(int id, string name, decimal value)
    {
        Id = id;
        Name = name;
        Value = value;
    }
}

/// <summary>
/// 参照型のclass（実験用）
/// </summary>
public class ReferenceTypeModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Value { get; set; }

    public ReferenceTypeModel()
    {
    }

    public ReferenceTypeModel(int id, string name, decimal value)
    {
        Id = id;
        Name = name;
        Value = value;
    }

    /// <summary>
    /// オブジェクトのハッシュコード（参照の同一性確認用）
    /// </summary>
    public string GetIdentity() => $"Hash: {GetHashCode():X8}";
}

/// <summary>
/// ネストした参照型（値型プロパティを持つ）
/// </summary>
public class NestedModel
{
    public int Counter { get; set; }
    public ReferenceTypeModel? Item { get; set; }

    public NestedModel()
    {
        Counter = 0;
    }
}
