using Sitecore.Data;
using Sitecore.Data.Items;

namespace AdvancedPublishingOptions.CustomSitecore.CustomItems
{
    public class PublishingDataFolderItem : CustomItem
    {
        protected PublishingDataFolderItem(Item innerItem) : base(innerItem)
        {
        }

        public static readonly ID TemplateId = new ID("0608a5b9-61c7-417a-97fd-6c36a8e50045");
    }
}