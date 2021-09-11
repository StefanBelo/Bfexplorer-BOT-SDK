/////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2015, Stefan Belopotocan, http://bfexplorer.net
//
/////////////////////////////////////////////////////////////////////////////

using System.Windows;
using System.Windows.Data;

namespace BeloSoft.MyFootballStrategy.UI.Converters
{
    /// <summary>
    /// BoolToVisibilityConverter
    /// </summary>
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility>
    {
        /// <summary>
        /// BoolToVisibilityConverter
        /// </summary>
        public BoolToVisibilityConverter()
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }
    }
}