using Sitecore.Data;
using Sitecore.Data.Items;

namespace AdvancedPublishingOptions.CustomSitecore.Extensions
{
    public static class ItemExtensions
    {
        /// <summary>
        /// Checks if <paramref name="source"/> uses a template
        /// that inherits from <paramref name="template"/>.
        /// </summary>
        /// <param name="source">The <see cref="Item"/> to test for inheritance.</param>
        /// <param name="template">The <see cref="TemplateItem"/> to check against.</param>
        /// <returns>
        /// true if <paramref name="source"/> uses a Template that is or inherits from <paramref name="template"/>.
        /// </returns>
        public static bool InheritsFrom(this Item source, TemplateItem template)
        {
            return source != null && source.Template.InheritsFrom(template);
        }

        /// <summary>
        /// Checks if the <paramref name="source"/> <see cref="Item"/> uses a
        /// template that inherits from the given template <see cref="ID"/>.
        /// </summary>
        /// <param name="source">The <see cref="Item"/> to check</param>
        /// <param name="templateID">The <see cref="ID"/> of the template</param>
        /// <returns>
        /// <c>true</c> if the item inherits from the template with the given ID
        /// </returns>
        public static bool InheritsFrom(this Item source, ID templateID)
        {
            return source.InheritsFrom(source.Database.Templates[templateID]);
        }
    }
}