using CommunityToolkit.Diagnostics;
using FluentStore.SDK;
using FluentStore.SDK.Images;
using FluentStore.SDK.Packages;
using Garfoot.Utilities.FluentUrn;
using System.Linq;
using Winstall.Models;

namespace FluentStore.Sources.WinGet
{
    public class WinstallPack : GenericPackageCollection<Pack>
    {
        public WinstallPack(PackageHandlerBase packageHandler, Pack pack = null, Creator creator = null) : base(packageHandler)
        {
            if (pack != null)
                Update(pack);
            if (creator != null)
                Update(creator);
        }

        public void Update(Pack pack)
        {
            Guard.IsNotNull(pack);
            Model = pack;

            // Set base properties
            Title = pack.Title;
            Description = pack.Description;
            Price = 0.0;
            DisplayPrice = "View";
            Urn = new(WinGetHandler.NAMESPACE_WINSTALL_PACK, new RawNamespaceSpecificString(pack.Id));

            Images.Clear();
            Images.Add(TextImage.CreateFromName(Title, ImageType.Logo));

            Items = new(pack.Apps.Select(a => new WinGetPackage(PackageHandler, a)));
        }

        public void Update(Creator creator)
        {
            Guard.IsNotNull(creator);

            // Set base properties
            PublisherId = creator.IdStr;
            DeveloperName = creator.Name;
        }
    }
}
