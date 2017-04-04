using Sitecore.Data;
using Sitecore.Data.Events;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.WebEdit;
using Sitecore.Web.UI.Sheer;
using Sitecore.Links;
using Sitecore.Data.Items;
using System;
using Sitecore.Web;
using System.Linq;
using System.Collections.Generic;
using Sitecore.Sites;
using Sitecore.Configuration;

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
            Item item = IsLayoutExist(Database.GetDatabase("master").GetItem(this.parentID));
            SiteInfo si = GetSite(item);
            UrlOptions option = new UrlOptions() { AlwaysIncludeServerUrl = true, ShortenUrls = true, Site = new Sites.SiteContext(si) };
            string url = LinkManager.GetItemUrl(item, option);
            SheerResponse.Eval($"window.parent.location.href='{url}?sc_mode=edit'");
        }
        private SiteInfo GetSite(Item item)
        {
            var siteInfoList = Sitecore.Configuration.Factory.GetSiteInfoList();
            foreach (Sitecore.Web.SiteInfo siteInfo in siteInfoList)
            {
                string fullPath = (siteInfo.RootPath + siteInfo.StartItem).Replace("//", "/");
                if (item.Paths.FullPath.Contains(fullPath) && !String.IsNullOrEmpty(fullPath))
                {
                    return siteInfo;
                }
            }
            return SiteContextFactory.GetSiteInfo(Settings.Preview.DefaultSite);
        }
        private Item IsLayoutExist(Item item)
        {
            Item _item = item;
            LayoutItem _layout = item.Visualization.Layout;
            while (true)
            {
                if (_layout == null)
                {
                    _item = _item.Parent;
                    if (_item.ID == ID.Parse("00000000-0000-0000-0000-000000000000") || _item.ID == ID.Parse("11111111-1111-1111-1111-111111111111"))
                        return item;
                    else
                        _layout = _item.Visualization.Layout;
                }

                else
                    return _item;
            }
        }
    }
}