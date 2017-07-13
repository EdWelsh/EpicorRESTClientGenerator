using EpicorRESTGenerator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EpicorRESTGenerator.WPFGUI
{
    /// <summary>
    /// Interaction logic for generator.xaml
    /// </summary>
    public partial class generator : Window
    {
        service services;
        public generator()
        {
            InitializeComponent();
        }

        private void serviceGetButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serviceURLTextBox.Text))
            {
                MessageBox.Show("Service URL is Required");
            }
            services =service.getServices(serviceURLTextBox.Text);
            serviceListBox.ItemsSource = services.workspace.collection.Select(o => o.href);
        }

        private async void generatButton_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            EpicorDetails details = new EpicorDetails();
            details.BaseClass = BaseClassTextBox.Text;
            details.EpicorAPIUrl = EpicorAPIURLTextBox.Text;
            details.EpicorERPCodeDir = ERPProjectDirTextBox.Text;
            details.EpicorERPProject = ERPProjectTextBox.Text;
            details.EpicorICECodeDir = ICEProjectDirTextBox.Text;
            details.EpicorICEProject = ICEProjectTextBox.Text;
            details.Namespace = NamespaceTextBox.Text;
            details.useBaseClass = (bool)UseBaseClassCheckBox.IsChecked;
            services.workspace.collection = services.workspace.collection.Where(o => serviceListBox.SelectedItems.Contains(o.href)).ToArray<serviceWorkspaceCollection>();
            var test = await service.generateCode(services, details);
            if (test)
                MessageBox.Show("Success");
            else
                MessageBox.Show("Somehing went wrong");

            this.IsEnabled = true;
        }
    }
}
