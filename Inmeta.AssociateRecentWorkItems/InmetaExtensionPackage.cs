using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Inmeta.AssociateRecentWorkItems
{
 [PackageRegistration(UseManagedResourcesOnly = true)]
 [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    [Guid(GuidList.guidInmetaAssociateRecentWorkItemPkgString)]
    public sealed class InmetaExtensionPackage : Package
    {
        public InmetaExtensionPackage()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        protected override void Initialize()
        {
            Debug.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

        }
    }
}
