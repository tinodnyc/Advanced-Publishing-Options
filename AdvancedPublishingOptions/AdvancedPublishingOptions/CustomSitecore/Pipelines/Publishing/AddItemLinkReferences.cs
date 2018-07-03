using AdvancedPublishingOptions.CustomSitecore.CustomItems;
using AdvancedPublishingOptions.CustomSitecore.Extensions;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Comparers;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Publishing;
using Sitecore.Publishing.Pipelines.GetItemReferences;
using Sitecore.Publishing.Pipelines.Publish;
using Sitecore.Publishing.Pipelines.PublishItem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedPublishingOptions.CustomSitecore.Pipelines.Publishing
{
    public class AddItemLinkReferences : GetItemReferencesProcessor
    {
        public AddItemLinkReferences()
        {
        }

        /// <summary>
        /// Gets the list of item references.
        /// </summary>
        /// <param name="context">The publish item context.</param>
        /// <returns>
        /// The list of item references.
        /// </returns>
        protected override List<Item> GetItemReferences(PublishItemContext context)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(context, "context");

            var deepRelatedTemplates = GetAdvancedPublishingSettings(context);
            List<Item> items = new List<Item>();
            if (context.PublishOptions.Mode != PublishMode.SingleItem)
            {
                return items;
            }
            switch (context.Action)
            {
                case PublishAction.PublishSharedFields:
                    {
                        var sourceItem = context.PublishHelper.GetSourceItem(context.ItemId);
                        if (sourceItem == null)
                        {
                            return items;
                        }
                        //added custom parameter deepRelatedTemplates
                        items.AddRange(this.GetReferences(sourceItem, true, deepRelatedTemplates: deepRelatedTemplates));
                        break;
                    }
                case PublishAction.PublishVersion:
                    {
                        Item versionToPublish = context.VersionToPublish;
                        if (versionToPublish == null)
                        {
                            return items;
                        }
                        //added custom parameter deepRelatedTemplates
                        items.AddRange(this.GetReferences(versionToPublish, false, deepRelatedTemplates: deepRelatedTemplates));
                        break;
                    }
                default:
                    {
                        return items;
                    }
            }
            return items;
        }

        protected virtual Dictionary<ID, bool> GetAdvancedPublishingSettings(PublishItemContext context)
        {
            var deepRelatedTemplates = new Dictionary<ID, bool>();

            var db = context.VersionToPublish?.Database;

            if (db != null)
            {
                var langaugeCode = Sitecore.Configuration.Settings.GetSetting("AdvancedPublishingOptions.GlobalSettings.LanguageIsoCode");

                Language lang;

                if (Language.TryParse(langaugeCode, out lang))
                {
                    using (new LanguageSwitcher(lang))
                    {
                        var publishingDataFolderItemId = Sitecore.Configuration.Settings.GetSetting("AdvancedPublishingOptions.GlobalSettings.ItemId");
                        var publishingDataFolder = db.GetItem(publishingDataFolderItemId);

                        if (publishingDataFolder != null)
                        {
                            if (publishingDataFolder != null)
                            {
                                var deepRelatedSettingsItems = publishingDataFolder.Children.Where(it => it.InheritsFrom(DeepRelatedItemDataItem.TemplateId))
                                    .Select(it => new DeepRelatedItemDataItem(it));

                                deepRelatedTemplates = (from i in deepRelatedSettingsItems
                                                            // Ignore invalid template selections
                                                        where i.TemplateField.TargetID != (ID)null
                                                        where i.TemplateField.TargetID.Guid != Guid.Empty
                                                        group i by i.TemplateField.TargetID into iGroup
                                                        select new
                                                        {
                                                            ItemID = iGroup.Key,
                                                            // If the same template exists more than once, any Checked will apply
                                                            Checked = iGroup.Any(ig => ig.SubitemsField.Checked)
                                                        }).ToDictionary(i => i.ItemID, i => i.Checked);
                            }
                        }
                        else
                        {
                            Sitecore.Diagnostics.Log.Error("AdvancedPublishingOptions: Global settings item could not be found", this);
                        }
                    }
                }
                else
                {
                    Sitecore.Diagnostics.Log.Error("AdvancedPublishingOptions: AdvancedPublishingOptions.GlobalSettings.LanguageIsoCode could not be parsed", this);
                }
            }

            return deepRelatedTemplates;
        }

        /// <summary>
        /// Gets the related references.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="sharedOnly">Determines whether to process shared fields only or not.</param>
        /// <returns>
        /// The related references.
        /// </returns>
        protected virtual IEnumerable<Item> GetReferences(Item item, bool sharedOnly, bool subitems = false, bool recurse = true, Dictionary<ID, bool> deepRelatedTemplates = null)
        {
            Sitecore.Diagnostics.Assert.ArgumentNotNull(item, "item");
            var items = new List<Item>();
            var validLinks = item.Links.GetValidLinks();
            validLinks = (
                from link in validLinks
                where item.Database.Name.Equals(link.TargetDatabaseName, StringComparison.OrdinalIgnoreCase)
                select link).ToArray();
            if (sharedOnly)
            {
                validLinks = (validLinks).Where((link) =>
                {
                    Item sourceItem = link.GetSourceItem();
                    if (sourceItem == null)
                    {
                        return false;
                    }
                    if (ID.IsNullOrEmpty(link.SourceFieldID))
                    {
                        return true;
                    }
                    return sourceItem.Fields[link.SourceFieldID].Shared;
                }).ToArray();
            }
            var list = (
                from link in validLinks
                select link.GetTargetItem() into relatedItem
                where relatedItem != null
                select relatedItem).ToList();
            foreach (var item1 in list)
            {
                #region CustomCode

                AddItemsToPublishingQueue(item1, ref items, sharedOnly, subitems, recurse, deepRelatedTemplates);

                #endregion CustomCode
            }
            return items.Distinct<Item>(new ItemIdComparer());
        }

        protected virtual void AddItemsToPublishingQueue(Item item1, ref List<Item> items, bool sharedOnly, bool subitems, bool recurse, Dictionary<ID, bool> deepRelatedTemplates)
        {
            items.AddRange(PublishQueue.GetParents(item1));
            items.Add(item1);
            if (subitems)
            {
                items.AddRange(item1.Children);
            }

            if (deepRelatedTemplates?.ContainsKey(item1.TemplateID) == true && recurse)
            {
                var relatedItems = GetReferences(item1, sharedOnly, deepRelatedTemplates[item1.TemplateID], recurse: false).ToArray();
                items.AddRange(relatedItems);
            }

            var relatedImages = item1.Links.GetValidLinks().Where(link => link.TargetPath.StartsWith(Constants.MediaLibraryPath))
                .Select(link => link.GetTargetItem()).ToArray();

            if (relatedImages.Any())
            {
                var imageParents = relatedImages.SelectMany(PublishQueue.GetParents);
                items.AddRange(imageParents);
                items.AddRange(relatedImages);
            }
        }
    }
}