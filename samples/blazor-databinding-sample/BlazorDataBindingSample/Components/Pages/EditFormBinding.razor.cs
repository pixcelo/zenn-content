using Microsoft.AspNetCore.Components;
using BlazorDataBindingSample.Models;

namespace BlazorDataBindingSample.Components.Pages;

/// <summary>
/// EditFormバインディングのデモページ
/// </summary>
public partial class EditFormBinding : ComponentBase
{
    private PersonModel person = new();
    private PersonModel? submittedPerson = null;
    private string errorMessage = "";
    private string submitTime = "";

    private void HandleValidSubmit()
    {
        // フォームが有効な場合の処理
        submittedPerson = new PersonModel
        {
            Name = person.Name,
            Email = person.Email,
            Age = person.Age,
            PhoneNumber = person.PhoneNumber,
            BirthDate = person.BirthDate,
            Gender = person.Gender,
            Hobbies = new List<string>(person.Hobbies),
            Website = person.Website,
            PostalCode = person.PostalCode,
            Bio = person.Bio,
            AgreeToTerms = person.AgreeToTerms
        };

        submitTime = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        errorMessage = "";
    }

    private void HandleInvalidSubmit()
    {
        // フォームが無効な場合の処理
        errorMessage = "入力内容にエラーがあります。必須項目を確認してください。";
        submittedPerson = null;
    }

    private void UpdateHobby(string hobby, ChangeEventArgs e)
    {
        bool isChecked = (bool)(e.Value ?? false);

        if (isChecked)
        {
            if (!person.Hobbies.Contains(hobby))
            {
                person.Hobbies.Add(hobby);
            }
        }
        else
        {
            person.Hobbies.Remove(hobby);
        }
    }

    private void ResetForm()
    {
        person = new PersonModel();
        submittedPerson = null;
        errorMessage = "";
    }
}
