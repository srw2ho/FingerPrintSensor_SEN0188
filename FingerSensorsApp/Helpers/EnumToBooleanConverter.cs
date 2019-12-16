using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace FingerSensorsApp.Helpers
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public Type EnumType { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                if (!Enum.IsDefined(EnumType, value))
                {
                    throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum".GetLocalized());
                }

                var enumValue = Enum.Parse(EnumType, enumString);

                return enumValue.Equals(value);
            }

            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName".GetLocalized());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string enumString)
            {
                return Enum.Parse(EnumType, enumString);
            }

            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName".GetLocalized());
        }
    }

    public sealed class IsEnableConverter : Windows.UI.Xaml.Data.IValueConverter

    {

        public object Convert(object value, Type targetType, object parameter, string language)

        {



            var boolvalue = (bool)value;

            string param = parameter as string;

            if ((param != null) && (param == "Negation"))

            {

                return !boolvalue;

            }

            else return boolvalue;

        }



        public object ConvertBack(object value, Type targetType, object parameter, string language)

        {

            var boolvalue = (bool)value;

            string param = parameter as string;

            if ((param != null) && (param == "Negation"))

            {

                return !boolvalue;

            }

            else return boolvalue;

        }

    }
    class BoolToVisibilityConverter : IValueConverter
    {


        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Boolean && (bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is Visibility && (Visibility)value == Visibility.Visible)
            {
                return true;
            }
            return false;
        }
    }
}
