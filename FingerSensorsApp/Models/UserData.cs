using System;

using Windows.UI.Xaml.Media.Imaging;

namespace FingerSensorsApp.Models
{
    public class UserData
    {
        public string Name { get; set; }

        public string UserPrincipalName { get; set; }

        public BitmapImage Photo { get; set; }

        public string GivenName { get; set; }
        public string Surname { get; set; }


    }
}
