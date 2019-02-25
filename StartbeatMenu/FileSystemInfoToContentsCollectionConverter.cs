using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace StartbeatMenu
{
    class FileSystemInfoToContentsCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DirectoryInfo)
            {
                Debug.WriteLine("DIRECTORYINFO");
                return System.Linq.Enumerable.ToList((value as DirectoryInfo).EnumerateFileSystemInfos());
            }
            else
            {
                Debug.WriteLine("SOMETHING ELSE");
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
