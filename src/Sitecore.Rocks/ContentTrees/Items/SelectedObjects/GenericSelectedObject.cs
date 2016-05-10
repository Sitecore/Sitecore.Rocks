// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using Sitecore.Rocks.Annotations;
using Sitecore.Rocks.Diagnostics;

namespace Sitecore.Rocks.ContentTrees.Items.SelectedObjects
{
    /* --------------------------------------------------------------------------*/
    /* **** READ THIS http://msdn.microsoft.com/en-us/magazine/cc163816.aspx *** */
    /* --------------------------------------------------------------------------*/

    public class GenericSelectedObject : BaseSelectedObject
    {
        public GenericSelectedObject([NotNull] object genericObject)
        {
            Assert.ArgumentNotNull(genericObject, nameof(genericObject));

            GenericObject = genericObject;
        }

        [NotNull]
        protected object GenericObject { get; private set; }
    }
}
