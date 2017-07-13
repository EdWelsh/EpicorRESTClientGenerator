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
            services = service.getServices(serviceURLTextBox.Text);
            serviceListBox.ItemsSource = services.workspace.collection.Select(o => o.href);
        }

        private async void generatButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid(EpicorAPIURLTextBox)) { MessageBox.Show("Please provide a URL for the Epicor API", "");  return; }
            if (isValid(ERPProjectDirTextBox)) { MessageBox.Show("Please provide the ERP project directory", "");  return; }
            if (isValid(ICEProjectDirTextBox)) { MessageBox.Show("Please provide the ICE project directory", "");  return; }
            if (isValid(ERPProjectTextBox)) { MessageBox.Show("Please provide the ERP project", "");  return; }
            if (isValid(ICEProjectTextBox)) { MessageBox.Show("Please provide the ICE project", "");  return; }

            if (serviceListBox.SelectedItems.Count == 0)
            {
                MessageBoxResult result = MessageBox.Show("No services were selected, would you like to generate code for ALL services?", "", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.No:
                        MessageBox.Show("Please select the service you wish to generate a client for!", "");
                        return;
                }
            }

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

        private bool isValid(TextBox textBox)
        {
            return String.IsNullOrEmpty(textBox.Text);
        }
    }
}
