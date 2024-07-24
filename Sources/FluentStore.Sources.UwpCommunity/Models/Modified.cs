using System;

namespace FluentStore.Sources.UwpCommunity.Models;

public abstract class Modified
{
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
