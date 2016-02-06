namespace Sentinel.Support.Wpf
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class ThemeInfo
    {
        private const int MaxPath = 260;

        // Cache the name of the current theme.
        private static string currentTheme = string.Empty;

        /// <summary>
        /// Gets the filename of the current theme.
        /// </summary>
        /// <remarks>
        /// Will return an empty string if themes are not supported by
        /// the operating system or disabled by the user.
        /// </remarks>
        public static string CurrentThemeFileName
        {
            get
            {
                if (string.IsNullOrEmpty(currentTheme))
                {
                    if (IsSupportedByOS)
                    {
                        if (!IsEnabledByUser)
                        {
                            currentTheme = string.Empty;
                        }

                        var name = new StringBuilder(MaxPath, MaxPath);
                        SafeNativeMethods.GetCurrentThemeName(name, MaxPath, IntPtr.Zero, 0, IntPtr.Zero, 0);

                        currentTheme = Path.GetFileNameWithoutExtension(name.ToString());
                    }
                }

                return currentTheme;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the user has enabled visual styles in the operating system.
        /// </summary>
        public static bool IsEnabledByUser => IsSupportedByOS && SafeNativeMethods.IsThemeActive();

        /// <summary>
        /// Gets a value indicating whether the operating system supports visual styles.
        /// </summary>
        public static bool IsSupportedByOS => Environment.OSVersion.Platform == PlatformID.Win32NT
                                              && Environment.OSVersion.Version >= new Version(5, 1);

        internal abstract class SafeNativeMethods
        {
            /// <summary>
            /// Retrieves the name of the current visual style.
            /// </summary>
            /// <param name="pszThemeFileName">Pointer to a string that receives the theme path and file name.</param>
            /// <param name="maxNameChars">Value that contains the maximum number of characters allowed in the theme file name.</param>
            /// <param name="sizeColourBuffer">Pointer to a string that receives the color scheme name. This parameter may be set to NULL.</param>
            /// <param name="cchMaxColorChars">Value of type int that contains the maximum number of characters allowed in the color scheme name.</param>
            /// <param name="buffer">Pointer to a string that receives the size name. This parameter may be set to NULL.</param>
            /// <param name="maxBufferSize">Value of type int that contains the maximum number of characters allowed in the size name.</param>
            /// <returns>Returns S_OK if successful, otherwise an error code.</returns>
            [DllImport("UxTheme.dll", CharSet = CharSet.Unicode, EntryPoint = "GetCurrentThemeName", SetLastError = true)]
            internal static extern int GetCurrentThemeName(
                [MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszThemeFileName,
                int maxNameChars,
                IntPtr sizeColourBuffer,
                int cchMaxColorChars,
                IntPtr buffer,
                int maxBufferSize);

            /// <summary>
            /// Tests if a visual style for the current application is active.
            /// </summary>
            /// <returns>Boolean value indicating whether the use of themes is active.</returns>
            [DllImport("UxTheme.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            internal static extern bool IsThemeActive();
        }
    }
}