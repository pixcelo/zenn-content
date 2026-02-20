using System.ComponentModel.DataAnnotations;

namespace BlazorDataBindingSample.Models;

/// <summary>
/// EditForm用のデータモデル
/// </summary>
public class PersonModel
{
    /// <summary>
    /// 名前
    /// </summary>
    [Required(ErrorMessage = "名前は必須です")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "名前は2文字以上50文字以内で入力してください")]
    public string Name { get; set; } = "";

    /// <summary>
    /// メールアドレス
    /// </summary>
    [Required(ErrorMessage = "メールアドレスは必須です")]
    [EmailAddress(ErrorMessage = "有効なメールアドレスを入力してください")]
    public string Email { get; set; } = "";

    /// <summary>
    /// 年齢
    /// </summary>
    [Required(ErrorMessage = "年齢は必須です")]
    [Range(0, 120, ErrorMessage = "年齢は0歳から120歳の範囲で入力してください")]
    public int? Age { get; set; }

    /// <summary>
    /// 電話番号
    /// </summary>
    [Phone(ErrorMessage = "有効な電話番号を入力してください")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 誕生日
    /// </summary>
    [Required(ErrorMessage = "誕生日は必須です")]
    public DateTime? BirthDate { get; set; }

    /// <summary>
    /// 性別
    /// </summary>
    [Required(ErrorMessage = "性別を選択してください")]
    public string Gender { get; set; } = "";

    /// <summary>
    /// 趣味（複数選択可）
    /// </summary>
    public List<string> Hobbies { get; set; } = new();

    /// <summary>
    /// 自己紹介
    /// </summary>
    [StringLength(500, ErrorMessage = "自己紹介は500文字以内で入力してください")]
    public string? Bio { get; set; }

    /// <summary>
    /// 利用規約への同意
    /// </summary>
    [Range(typeof(bool), "true", "true", ErrorMessage = "利用規約に同意してください")]
    public bool AgreeToTerms { get; set; }

    /// <summary>
    /// Webサイト
    /// </summary>
    [Url(ErrorMessage = "有効なURLを入力してください")]
    public string? Website { get; set; }

    /// <summary>
    /// 郵便番号
    /// </summary>
    [RegularExpression(@"^\d{3}-?\d{4}$", ErrorMessage = "郵便番号は000-0000の形式で入力してください")]
    public string? PostalCode { get; set; }
}
