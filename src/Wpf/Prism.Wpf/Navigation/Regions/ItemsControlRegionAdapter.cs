using Prism.Properties;

namespace Prism.Navigation.Regions
{
    /// <summary>
    /// Adapter that creates a new <see cref="AllActiveRegion"/> and binds all
    /// the views to the adapted <see cref="ItemsControl"/>.
    /// </summary>
    public class ItemsControlRegionAdapter : RegionAdapterBase<ItemsControl>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ItemsControlRegionAdapter"/>.
        /// </summary>
        /// <param name="regionBehaviorFactory">The factory used to create the region behaviors to attach to the created regions.</param>
        public ItemsControlRegionAdapter(IRegionBehaviorFactory regionBehaviorFactory)
            : base(regionBehaviorFactory)
        {
        }

        /// <summary>
        /// Adapts an <see cref="ItemsControl"/> to an <see cref="IRegion"/>.
        /// </summary>
        /// <param name="region">The new region being used.</param>
        /// <param name="regionTarget">The object to adapt.</param>
        protected override void Adapt(IRegion region, ItemsControl regionTarget)
        {
            if (region == null)
                throw new ArgumentNullException(nameof(region));

            if (regionTarget == null)
                throw new ArgumentNullException(nameof(regionTarget));

            // NOTE: In Avalonia, regionTarget.ItemsSource will not be null. Keep it rollin' baby!
#if !AVALONIA
            bool itemsSourceIsSet = regionTarget.ItemsSource != null;
            itemsSourceIsSet = itemsSourceIsSet || regionTarget.HasBinding(ItemsControl.ItemsSourceProperty);

            if (itemsSourceIsSet)
            {
                throw new InvalidOperationException(Resources.ItemsControlHasItemsSourceException);
            }

            // If control has child items, move them to the region and then bind control to region. Can't set ItemsSource if child items exist.
            if (regionTarget.Items.Count > 0)
            {
                foreach (object childItem in regionTarget.Items)
                {
                    region.Add(childItem);
                }
                // Control must be empty before setting ItemsSource
                regionTarget.Items.Clear();
            }

            regionTarget.ItemsSource = region.Views;
#else
            // If control has child items, move them to the region and then bind control to region. Can't set ItemsSource if child items exist.
            if (regionTarget.ItemCount > 0)
            {
                foreach (object childItem in regionTarget.Items)
                    region.Add(childItem);

                // Control must be empty before setting ItemsSource
                regionTarget.Items.Clear();
            }

            // Detect when an item has been added/removed to the ItemsControl's backing region. Copy
            // all items to a new collection and bind to the region's ItemsSource
            region.Views.CollectionChanged += (s, e) =>
            {
                var enumerator = region.Views.GetEnumerator();
                List<object> items = new();
                while (enumerator.MoveNext())
                {
                    items.Add(enumerator.Current);
                }

                regionTarget.ItemsSource = items;
            };
#endif
        }

        /// <summary>
        /// Creates a new instance of <see cref="AllActiveRegion"/>.
        /// </summary>
        /// <returns>A new instance of <see cref="AllActiveRegion"/>.</returns>
        protected override IRegion CreateRegion()
        {
            return new AllActiveRegion();
        }
    }
}
