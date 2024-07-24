using System.Collections.Generic;

namespace FluentStore.Sources.UwpCommunity.Models;

public class Tag : Modified
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public List<Project> Projects { get; set; }
}