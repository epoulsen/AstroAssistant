﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace AstroAssistant.Converters
{
    public class StringEmptyToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            String sValue = value != null ? value.ToString() : null;
            bool isVisible = String.IsNullOrWhiteSpace((value != null ? value.ToString() : null));
            Visibility invisible = Visibility.Collapsed;
            if (parameter != null)
            {
                var parts = parameter.ToString().ToLower().Split(new Char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Any(p => p.Trim() == "not"))
                    isVisible = !isVisible;
                if (parts.Any(p => p.Trim() == "hidden"))
                    invisible = Visibility.Hidden;
            }
            return isVisible ? Visibility.Visible : invisible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
