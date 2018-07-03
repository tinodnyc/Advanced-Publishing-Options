using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;

namespace AdvancedPublishingOptions.CustomSitecore.CustomItems
{
    public class DeepRelatedItemDataItem : CustomItem
    {
        public DeepRelatedItemDataItem(Item innerItem) : base(innerItem)
        {
        }

        public static readonly ID TemplateId = new ID("a42166eb-0ea1-4870-b255-8d3414052e8c");

        public ReferenceField TemplateField
        {
            get
            {
                return InnerItem.Fields[new ID("fabd1fb7-1408-47b9-b0bc-b5f7aa979b97")];
            }
        }

        public CheckboxField SubitemsField
        {
            get
            {
                return InnerItem.Fields[new ID("f2fffb18-a90e-429d-913a-81c12896d1d0")];
            }
        }
    }
}