using EpicorSwaggerRESTGenerator.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

namespace EpicorSwaggerRESTGenerator.WPFGUI
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
            if((bool)useCredentialsCheckBox.IsChecked)
            {
                if(string.IsNullOrEmpty(usernameTextBox.Text))
                {
                    MessageBox.Show("Username is required");
                    return;
                }
                if(string.IsNullOrEmpty(passwordTextBox.Text))
                {
                    MessageBox.Show("Password is required");
                    return;
                }  
            }

            isvalidURL(serviceURLTextBox.Text + "/api/v1/");

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
            if (!isValid(ERPAPIURLServiceTextBox) || !isvalidURL(ERPAPIURLServiceTextBox.Text)) { MessageBox.Show("Please provide a services URL for the ERP Services", ""); return; }
            if (!isValid(ERPProjectTextBox) || !fileExists(ERPProjectTextBox)) { MessageBox.Show("Please provide the ERP project directory", ""); return; }
            if (!isValid(ERPAPIURLTextBox)) { MessageBox.Show("Please provide the ERP API URL", ""); return; }
            if (ERPServiceListBox.SelectedItems.Count == 0) { MessageBox.Show("Please select the service you wish to generate a client for!", ""); return; }
            IsEnabled = false;
            services.workspace.collection = services.workspace.collection.Where(o => ERPServiceListBox.SelectedItems.Contains(o.href)).ToArray();
            var r = generate(ERPAPIURLTextBox.Text, ERPProjectTextBox.Text).Result;
            IsEnabled = true;
        }
        private void GeneratICEButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid(ICEAPIURLServiceTextBox) || !isvalidURL(ICEAPIURLServiceTextBox.Text)) { MessageBox.Show("Please provide a services URL for the ICE Services", ""); return; }
            if (!isValid(ICEProjectTextBox) || !fileExists(ICEProjectTextBox)) { MessageBox.Show("Please provide the ICE project directory", ""); return; }
            if (!isValid(ICEAPIURLTextBox)) { MessageBox.Show("Please provide the ICE API URL", ""); return; }
            if (ICEServiceListBox.SelectedItems.Count == 0) { MessageBox.Show("Please select the service you wish to generate a client for!", ""); return; }
            IsEnabled = false;
            services.workspace.collection = services.workspace.collection.Where(o => ICEServiceListBox.SelectedItems.Contains(o.href)).ToArray();
            var r = generate(ICEAPIURLTextBox.Text, ICEProjectTextBox.Text).Result;
            IsEnabled = true;
        }
        private void GeneratBAQButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isValid(BAQAPIURLServiceTextBox) || !isvalidURL(BAQAPIURLServiceTextBox.Text)) { MessageBox.Show("Please provide a services URL for the BAQ Services", ""); return; }
            if (!isValid(BAQProjectTextBox) || !fileExists(BAQProjectTextBox)) { MessageBox.Show("Please provide the BAQ project directory", ""); return; }
            if (!isValid(BAQAPIURLTextBox)) { MessageBox.Show("Please provide the BAQ API URL", ""); return; }
            if (BAQServiceListBox.SelectedItems.Count == 0) { MessageBox.Show("Please select the service you wish to generate a client for!", ""); return; }
            IsEnabled = false;
            services.workspace.collection = services.workspace.collection.Where(o => BAQServiceListBox.SelectedItems.Contains(o.href)).ToArray();
            var r = generate(BAQAPIURLTextBox.Text, BAQProjectTextBox.Text).Result;
            IsEnabled = true;
        }


        private void PopulateService(TextBox textBox, ListBox listBox, string type)
        {
            if (string.IsNullOrEmpty(textBox.Text))
            {
                MessageBox.Show("Services URL is Required");
            }
            if (!isvalidURL(textBox.Text)) return;

           EpicorDetails details = new EpicorDetails();
            if((bool)useCredentialsCheckBox.IsChecked)
            {
                details.Username = usernameTextBox.Text;
                details.Password = passwordTextBox.Text;
            }

            services = service.getServices(textBox.Text, details);
            services.workspace.collection = services.workspace.collection.Where(o => o.href.ToUpper().StartsWith(type)).ToArray<serviceWorkspaceCollection>();
            listBox.ItemsSource = services.workspace.collection.Select(o => o.href);
        }
        private bool isValid(TextBox textBox)
        {
            return !String.IsNullOrEmpty(textBox.Text);
        }
        private bool isvalidURL(string url)
        {
            EpicorDetails details = new EpicorDetails();
            if ((bool)useCredentialsCheckBox.IsChecked)
            {
                details.Username = usernameTextBox.Text;
                details.Password = passwordTextBox.Text;
            }
            try
            {
                var valid = service.getServices(url, details);
                if (valid.workspace != null && valid.workspace.collection != null && valid.workspace.collection.Count() == 0)
                {
                    MessageBox.Show("Service is invalid");
                    return false;
                }
            }
            catch (WebException ex)
            {
                if(ex.Response != null)
                {
                    using (WebResponse response = ex.Response)
                    {
                        HttpWebResponse httpResponse = (HttpWebResponse)response;
                        MessageBox.Show(string.Format("Error code: {0}", httpResponse.StatusDescription));
                        string text = "";
                        using (Stream data = response.GetResponseStream())
                        using (var reader = new StreamReader(data))
                        {
                            // text is the response body
                            text = reader.ReadToEnd();
                        }
                        MessageBox.Show(text);
                        return false;
                    }
                }
                else
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }
        private bool fileExists(TextBox textBox)
        {
            return File.Exists(textBox.Text);
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

        private void useCredentialsCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            usernameTextBox.IsEnabled = true;
            passwordTextBox.IsEnabled = true;
        }
        private void useCredentialsCheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            usernameTextBox.IsEnabled = false;
            passwordTextBox.IsEnabled = false;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Document = new FlowDocument();

            OpenFileDialog openFile1 = new OpenFileDialog();
            openFile1.DefaultExt = "*.cs";
            openFile1.Filter = "CS Files|*.cs";

            var hasValue = openFile1.ShowDialog().HasValue;
            if (hasValue)
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(System.IO.File.ReadAllText(openFile1.FileName));
                FlowDocument document = new FlowDocument(paragraph);
                richTextBox.Document = document;
            }
        }
        private void button_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = "*.cs";
            dialog.Filter = "CS Files|*.cs";

            if (dialog.ShowDialog() == true)
            {
                TextRange t = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                FileStream file = new FileStream(dialog.FileName, FileMode.Create);
                t.Save(file, System.Windows.DataFormats.Text);
                file.Close();
                //File.WriteAllText(dialog.FileName, richTextBox.Document.);
            }
        }
    }
}
