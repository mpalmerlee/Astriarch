using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Controls.Theming;

namespace SystemMaster
{
    public static class ThemeManager
    {
        private static Uri themeResourceUri = new Uri(@"SystemMaster;component/Themes/ExpressionDark.xaml", UriKind.RelativeOrAbsolute);

        public static void ApplyStyle(UserControl uc)
        {

            /*
            ResourceDictionary rd = new ResourceDictionary();
            //Load resourse dictonary
            rd.Source = resourceUri;
            //Clear previous styles if any...
            App.Current.Resources.MergedDictionaries.Clear();
            //Add the loaded resource dictionary to the application merged dictionaries
            App.Current.Resources.MergedDictionaries.Add(rd);
             * */
            ImplicitStyleManager.SetResourceDictionaryUri(uc, themeResourceUri);
            ImplicitStyleManager.SetApplyMode(uc, ImplicitStylesApplyMode.OneTime);
            ImplicitStyleManager.Apply(uc); 
        }
    }
}
