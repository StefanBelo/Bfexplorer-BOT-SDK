/////////////////////////////////////////////////////////////////////////////
//
// Copyright © 2015 - 2016, Stefan Belopotocan, http://bfexplorer.net
//
/////////////////////////////////////////////////////////////////////////////

using System;
using System.Globalization;
using System.Windows.Data;

// http://geekswithblogs.net/codingbloke/archive/2010/05/28/a-generic-boolean-value-converter.aspx

namespace BeloSoft.MyFootballStrategy.UI.Converters
{
    /// <summary>
    /// BoolToValueConverter
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoolToValueConverter<T> : IValueConverter
    {
        /// <summary>
        /// FalseValue
        /// </summary>
        public T FalseValue { get; set; }

        /// <summary>
        /// TrueValue
        /// </summary>
        public T TrueValue { get; set; }

        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return FalseValue;
            }
            else
            {
                try
                {
                    return System.Convert.ToBoolean(value) ? TrueValue : FalseValue;
                }
                catch
                {
                    return FalseValue;
                }
            }
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }
}