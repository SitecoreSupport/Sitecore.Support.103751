using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.WebEdit;
using Sitecore.Web.UI.Sheer;
using System;

namespace Sitecore.Support.Shell.Applications.WebEdit
{
    public class WebEditRibbonForm: Sitecore.Shell.Applications.WebEdit.WebEditRibbonForm
    {
        private ID parentID;

        protected override void DeletedNotification(object sender, ItemDeletedEventArgs args)
        {
            base.DeletedNotification(sender, args);
            this.parentID = args.ParentID;
        }

        protected new void Redirect(ClientPipelineArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.IsNotNullOrEmpty(args.Parameters["url"], "url");
            ItemPath paths = Database.GetDatabase("master").GetItem(this.parentID).Paths;
            SheerResponse.Eval($"window.parent.location.href='{paths.Path}?sc_mode=edit'");
        }
    }
}