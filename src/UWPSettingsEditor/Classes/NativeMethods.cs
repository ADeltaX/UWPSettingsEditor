using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace UWPSettingsEditor
{
    public static class NativeMethods
    {
        [DllImport("Shell32.dll", EntryPoint = "ExtractIconExW", CharSet = CharSet.Unicode, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern int ExtractIconEx(string sFile, int iIndex, out IntPtr piLargeVersion, out IntPtr piSmallVersion, int amountIcons);

        public static BitmapSource ExtractIcon(string file, int number, bool largeIcon)
        {
            ExtractIconEx(file, number, out IntPtr large, out IntPtr small, 1);
            try
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    largeIcon ? large : small,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                return null;
            }
        }
    }
}
