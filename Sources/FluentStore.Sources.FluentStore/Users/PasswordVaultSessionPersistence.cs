using System.IO;
using System.Linq;
using System.Windows.Documents;
using FluentStore.Services;
using Newtonsoft.Json;
using Supabase.Gotrue;
using Supabase.Gotrue.Interfaces;

namespace FluentStore.Sources.FluentStore.Users;

internal class PasswordVaultSessionPersistence(IPasswordVaultService vault, ICommonPathManager pathManager, FluentStoreAccountHandler accountHandler) : IGotrueSessionPersistence<Session>
{
    private const string USER_JSON_NAME = "user.json";

    private readonly string _resource = accountHandler.GetAuthProtocolUrl();
    private readonly JsonSerializer _serializer = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        Formatting = Formatting.Indented,
    };

    public void DestroySession()
    {
        try
        {
            foreach (var credential in vault.FindAllByResource(_resource))
                vault.Remove(credential);

            File.Delete(GetUserJsonFileName());
        }
        catch (IOException) { }
    }

    public Session LoadSession()
    {
        var credentials = vault.FindAllByResource(_resource)
            .ToDictionary(c => c.UserName);

        if (credentials.Count <= 0
            || !credentials.TryGetValue(nameof(Session.AccessToken), out var accessTokenCred)
            || !credentials.TryGetValue(nameof(Session.RefreshToken), out var refreshTokenCred))
            return null;

        Session session = new()
        {
            AccessToken = accessTokenCred.Password,
            RefreshToken = refreshTokenCred.Password,
        };

        var userFilePath = GetUserJsonFileName();

        if (File.Exists(userFilePath))
        {
            using var userFile = new StreamReader(userFilePath);
            using var reader = new JsonTextReader(userFile);

            var user = _serializer.Deserialize<User>(reader);

            session.User = user;
        }

        return session;
    }

    public void SaveSession(Session session)
    {
        vault.Add(nameof(Session.AccessToken), session.AccessToken, _resource);
        vault.Add(nameof(Session.RefreshToken), session.RefreshToken, _resource);

        if (session.User is not null)
        {
            using var userFile = new StreamWriter(GetUserJsonFileName());
            using var writer = new JsonTextWriter(userFile);
            
            _serializer.Serialize(writer, session.User);
        }
    }

    private string GetPersistenceDirectory() => pathManager.GetTempDirectory().CreateSubdirectory(accountHandler.Id).FullName;

    private string GetUserJsonFileName() => Path.Join(GetPersistenceDirectory(), USER_JSON_NAME);
}
