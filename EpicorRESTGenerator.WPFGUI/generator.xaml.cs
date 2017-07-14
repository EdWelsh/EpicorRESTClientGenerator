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
        private void CheckService_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(serviceURLTextBox.Text))
            {
                MessageBox.Show("Epicor API URL is Required");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Do you want to generate a client for oData? Selecting No will default to custom methods", "", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    ERPAPIURLTextBox.Text = serviceURLTextBox.Text + "/api/swagger/v1/odata/";
                    ICEAPIURLTextBox.Text = serviceURLTextBox.Text + "/api/swagger/v1/odata/";
                    break;
                case MessageBoxResult.No:
                    ERPAPIURLTextBox.Text = serviceURLTextBox.Text + "/api/swagger/v1/methods/";
                    ICEAPIURLTextBox.Text = serviceURLTextBox.Text + "/api/swagger/v1/methods/";
                    break;
            }
            BAQAPIURLTextBox.Text = serviceURLTextBox.Text + "/api/swagger/v1/baq/";

            ERPAPIURLServiceTextBox.Text = serviceURLTextBox.Text + "/api/v1/";
            ICEAPIURLServiceTextBox.Text = serviceURLTextBox.Text + "/api/v1/";
            BAQAPIURLServiceTextBox.Text = serviceURLTextBox.Text + "/api/v1/BaqSvc/";



            tabControl.IsEnabled = true;
        }


        private void GetBAQServicesButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateService(BAQAPIURLServiceTextBox, BAQServiceListBox, "");
        }
        private void GetICEServicesButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateService(ICEAPIURLServiceTextBox, ICEServiceListBox, "ICE");
        }
        private void GetERPServicesButton_Click(object sender, RoutedEventArgs e)
        {
            PopulateService(ERPAPIURLServiceTextBox, ERPServiceListBox, "ERP");
        }


        private void GeneratERPButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid(ERPAPIURLServiceTextBox)) { MessageBox.Show("Please provide a services URL for the ERP Services", ""); return; }
            if (isValid(ERPProjectTextBox)) { MessageBox.Show("Please provide the ERP project directory", ""); return; }
            if (isValid(ERPAPIURLTextBox)) { MessageBox.Show("Please provide the ERP API URL", ""); return; }
            if (ERPServiceListBox.SelectedItems.Count == 0) { MessageBox.Show("Please select the service you wish to generate a client for!", ""); return; }
            IsEnabled = false;
            var r = generate(ERPAPIURLTextBox.Text, ERPProjectTextBox.Text).Result;
            IsEnabled = true;
        }
        private void GeneratICEButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid(ICEAPIURLServiceTextBox)) { MessageBox.Show("Please provide a services URL for the ICE Services", ""); return; }
            if (isValid(ICEProjectTextBox)) { MessageBox.Show("Please provide the ICE project directory", ""); return; }
            if (isValid(ICEAPIURLTextBox)) { MessageBox.Show("Please provide the ICE API URL", ""); return; }
            if (ICEServiceListBox.SelectedItems.Count == 0) { MessageBox.Show("Please select the service you wish to generate a client for!", ""); return; }

            IsEnabled = false;
            var r = generate(ICEAPIURLTextBox.Text, ICEProjectTextBox.Text).Result;
            IsEnabled = true;
        }
        private void GeneratBAQButton_Click(object sender, RoutedEventArgs e)
        {
            if (isValid(BAQAPIURLServiceTextBox)) { MessageBox.Show("Please provide a services URL for the BAQ Services", ""); return; }
            if (isValid(BAQProjectTextBox)) { MessageBox.Show("Please provide the BAQ project directory", ""); return; }
            if (isValid(BAQAPIURLTextBox)) { MessageBox.Show("Please provide the BAQ API URL", ""); return; }
            if (BAQServiceListBox.SelectedItems.Count == 0) { MessageBox.Show("Please select the service you wish to generate a client for!", ""); return; }
            IsEnabled = false;
            var r = generate(BAQAPIURLTextBox.Text, BAQProjectTextBox.Text).Result;
            IsEnabled = true;
        }


        private void PopulateService(TextBox textBox, ListBox listBox, string type)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                MessageBox.Show("Services URL is Required");
            }
            services = service.getServices(textBox.Text);
            services.workspace.collection = services.workspace.collection.Where(o => o.href.ToUpper().StartsWith(type)).ToArray<serviceWorkspaceCollection>();
            listBox.ItemsSource = services.workspace.collection.Select(o => o.href);
        }
        private bool isValid(TextBox textBox)
        {
            return String.IsNullOrEmpty(textBox.Text);
        }
        private async Task<bool> generate(string url, string proj)
        {
            EpicorDetails details = new EpicorDetails();
            details.BaseClass = BaseClassTextBox.Text;
            details.APIURL = url;
            details.Project = proj;
            details.Namespace = NamespaceTextBox.Text;
            details.useBaseClass = (bool)UseBaseClassCheckBox.IsChecked;

            var test = await service.generateCode(services, details);
            if (test)
                MessageBox.Show("Success");
            else
                MessageBox.Show("Somehing went wrong");
            return true;
        }
        private void OnTabItemChanged(object sender, SelectionChangedEventArgs e)
        {
            services = null;
            ERPServiceListBox.ItemsSource = null;
            ICEServiceListBox.ItemsSource = null;
            BAQServiceListBox.ItemsSource = null;
        }

        private void ServiceListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }
    }
}
