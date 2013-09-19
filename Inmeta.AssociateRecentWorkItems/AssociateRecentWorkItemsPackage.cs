using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Osiris.AssociateRecentWorkItems
{
 [PackageRegistration(UseManagedResourcesOnly = true)]
 [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidOsirisAssociateRecentWorkItemPkgString)]
    public sealed class AssociateRecentWorkItemsPackage : Package
    {
    }
}
