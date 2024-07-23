using Google.Apis.Firestore.v1.Data;

namespace FluentStoreAPI.Models;

public class Profile
{
    public Profile() { }

    internal Profile(Document d)
    {
        DisplayName = d.Fields[nameof(DisplayName)].StringValue;
    }

    public string DisplayName { get; set; }

    public static implicit operator Document(Profile profile)
    {
        return new()
        {
            Fields =
            {
                [nameof(DisplayName)] = new() { StringValue = profile.DisplayName },
            }
        };
    }
}
