using System.Configuration;
using System.Data;
using System.Windows;
using UKA_Demo.View;

namespace UKA_Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private void Application_Startup ( object sender, StartupEventArgs e )
        {
            var view = new SerialCommunicationView();
            view.Show();
        }
    }

}
