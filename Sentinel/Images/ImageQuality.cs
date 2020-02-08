namespace Sentinel.Images
{
    public enum ImageQuality
    {
        /// <summary>
        /// Images of a small (typically icon) size.
        /// </summary>
        Small,

        /// <summary>
        /// Images suitable for high-res icons or at a stretch, large icon representation
        /// </summary>
        Medium,

        /// <summary>
        /// High definition/detail images
        /// </summary>
        Large,

        /// <summary>
        /// Let the system select the best available image
        /// </summary>
        BestAvailable,
    }
}