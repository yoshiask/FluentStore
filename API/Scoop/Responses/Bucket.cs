using System;

namespace Scoop.Responses;

public class Bucket
{
    public string Name { get; set; }
    
    public string Source { get; set; }

    public DateTime Updated { get; set; }

    public int Manifests { get; set; }
}
