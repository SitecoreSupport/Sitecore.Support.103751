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
using Sitecore.StringExtensions;

namespace Sitecore.Support.Shell.Applications.WebEdit
{
    public class WebEditRibbonForm : Sitecore.Shell.Applications.WebEdit.WebEditRibbonForm
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
            Item item = Database.GetDatabase("master").GetItem(this.parentID);
            SiteInfo siteInfo = GetSite(item);
            item = IsLayoutExist(item) ?? Database.GetDatabase("master").GetItem((siteInfo.RootPath + siteInfo.StartItem).Replace("//", "/"));
            UrlOptions option = new UrlOptions()
            {
                AlwaysIncludeServerUrl = true,
                Site = new Sites.SiteContext(siteInfo),
                LanguageEmbedding = LinkManager.LanguageEmbedding,
                LowercaseUrls = LinkManager.LowercaseUrls,
                ShortenUrls = LinkManager.ShortenUrls,
                UseDisplayName = LinkManager.UseDisplayName,
                LanguageLocation = LinkManager.LanguageLocation,
                AddAspxExtension = LinkManager.AddAspxExtension,
                SiteResolving = true,
                EncodeNames = LinkManager.Provider.EncodeNames,
            };
            string url = LinkManager.GetItemUrl(item, option);
            SheerResponse.Eval($"window.parent.location.href='{url}?sc_mode=edit&sc_site={siteInfo.Name}'");
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
                        return null;
                    else
                        _layout = _item.Visualization.Layout;
                }
                else
                    return _item;
            }
        }
        private SiteInfo GetSite(Item item)
        {
            SiteInfo site = null;
            foreach (SiteInfo info in SiteContextFactory.Sites)
            {
                if (((!info.RootPath.IsNullOrEmpty() && !info.StartItem.IsNullOrEmpty()) && (!info.Domain.Equals("sitecore") && !info.Domain.Equals(""))) && !info.VirtualFolder.Contains("/sitecore modules/web"))
                {
                    Item item2 = null;

                    if (item.Paths.FullPath.Contains(info.RootPath))
                    {
                        item2 = Database.GetDatabase("master").GetItem(info.RootPath + info.StartItem);
                        if (item2 == null) continue;
                    }
                    else
                    {
                        continue;
                    }
                    Assert.IsNotNull(item2, "Sitecore.Support.103751 : Current StartItem can't be null");
                    if (item2.ParentID.Equals(item.ParentID))
                        return site = info;
                    if (item.Paths.Path.Contains(item2.Parent.Paths.Path))
                        return site = info;
                }
            }
            return SiteContextFactory.GetSiteInfo(Settings.Preview.DefaultSite);
        }
    }
}