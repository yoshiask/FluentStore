namespace FluentStore.Sources.UwpCommunity.Models;

public class Collaborator
{
    public int Id { get; set; }
    public bool IsOwner { get; set; }
    public string Role { get; set; }
    public string Name { get; set; }
    public string DiscordId { get; set; }
}