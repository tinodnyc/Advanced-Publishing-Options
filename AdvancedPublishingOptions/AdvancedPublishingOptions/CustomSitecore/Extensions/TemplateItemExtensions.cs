using Sitecore.Data;
using Sitecore.Data.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvancedPublishingOptions.CustomSitecore.Extensions
{
    public static class TemplateItemExtensions
    {
        private static readonly Dictionary<Tuple<ID, ID>, bool> Cache =
            new Dictionary<Tuple<ID, ID>, bool>();

        private static readonly object Locker = new object();

        /// <summary>
        /// Checks if <paramref name="source"/> inherits from <paramref name="template"/>
        /// or a <see cref="Template"/> that inherits from <paramref name="template"/>.
        /// </summary>
        /// <param name="source">A possible child template.</param>
        /// <param name="template">A possible parent template.</param>
        /// <returns>
        /// True if <paramref name="source"/> inherits from <paramref name="template"/>,
        /// or a <see cref="Template"/> that inherits from <paramref name="template"/>.
        /// </returns>
        public static bool InheritsFrom(this TemplateItem source, TemplateItem template)
        {
            if (source == null || template == null)
                return false;

            var key = Tuple.Create(source.ID, template.ID);
            if (Cache.ContainsKey(key))
                return Cache[key];

            //Use [double checked locking][1] to avoid [race conditions with Dictionaries][2]
            //[1]: http://en.wikipedia.org/wiki/Double-checked_locking
            //[2]: http://stackoverflow.com/a/10565639/497418
            bool inherits;
            lock (Locker)
            {
                if (Cache.ContainsKey(key))
                {
                    inherits = Cache[key];
                }
                else
                {
                    inherits = source.ID.Equals(template.ID) ||
                               source.BaseTemplates.Any(baseTemplate => baseTemplate.InheritsFrom(template));
                    Cache.Add(key, inherits);
                }
            }
            return inherits;
        }
    }
}