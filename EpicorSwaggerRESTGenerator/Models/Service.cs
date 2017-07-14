using Newtonsoft.Json;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EpicorRESTGenerator.Models
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2007/app")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2007/app", IsNullable = false)]
    public partial class service
    {
        private serviceWorkspace workspaceField;

        private string baseField;

        /// <remarks/>
        public serviceWorkspace workspace
        {
            get
            {
                return this.workspaceField;
            }
            set
            {
                this.workspaceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "http://www.w3.org/XML/1998/namespace")]
        public string @base
        {
            get
            {
                return this.baseField;
            }
            set
            {
                this.baseField = value;
            }
        }

        public static service getServices(string serviceURL)
        {
            using (WebClient client = getWebClient())
            {
                service services = new service();
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(service));
                string xml = client.DownloadString(serviceURL);
                using (StringReader sr = new StringReader(xml))
                {
                    services = (service)serializer.Deserialize(sr);
                }
                return services;
            }
        }

        public static async Task<bool> generateCode(service services, EpicorDetails details)
        {
            using (WebClient client = getWebClient())
            {
                foreach (var service in services.workspace.collection)
                {
                    var name = service.href.Replace(".", "").Replace("-", "");
                    try
                    {
                        string x = client.DownloadString(details.APIURL + service.href);

                        dynamic jsonObj = JsonConvert.DeserializeObject(x);
                        if (!details.APIURL.Contains("baq"))
                        {
                            foreach (var j in jsonObj["paths"])
                            {
                                j.First["post"]["operationId"] = j.Name.Replace(@"\", "").Replace("/", "");
                            }
                        }



                        string output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

                        var document = await SwaggerDocument.FromJsonAsync(output);
                        var settings = new SwaggerToCSharpClientGeneratorSettings() { ClassName = name, OperationNameGenerator = new SingleClientFromOperationIdOperationNameGenerator() };
                        var generator = new SwaggerToCSharpClientGenerator(document, settings);
                        if (details.useBaseClass) generator.Settings.ClientBaseClass = details.BaseClass;
                        generator.Settings.UseHttpClientCreationMethod = true;
                        generator.Settings.AdditionalNamespaceUsages = new[] { "Newtonsoft.Json", "Newtonsoft.Json.Linq" };
                        generator.Settings.GenerateSyncMethods = false;

                        var code = generator.GenerateFile();
                        code = code
                            //need to replace with my actual namespace
                            .Replace("MyNamespace", details.Namespace + "." + name)
                            //Had an error so added but I dont think this replacement is needed for all scenarios, maybe add flag in details later
                            .Replace("var client_ = await CreateHttpClientAsync(cancellationToken).ConfigureAwait(false);", "var client_ = CreateHttpClientAsync(cancellationToken);")
                            //no need
                            .Replace("#pragma warning disable // Disable all warnings", "")
                            //cant use so had to replace
                            .Replace("<Key>k", "Keyk")
                            //cant use so had to replace
                            .Replace("<Value>k", "Valuek")
                            //cant use so had to replace
                            .Replace("_tLÐ¡TotalCost", "_tLDTotalCost")
                            //cant use so had to replace
                            .Replace("TLÐ¡TotalCost", "TLDTotalCost")
                            //had to change to dictionary<string,jtoken>, additial properties may return a list, parse into jtoken
                            .Replace("private System.Collections.Generic.IDictionary<string, string> _additionalProperties = new System.Collections.Generic.Dictionary<string, string>();", "private System.Collections.Generic.IDictionary<string, JToken> _additionalProperties = new System.Collections.Generic.Dictionary<string, JToken>();")
                            .Replace("public System.Collections.Generic.IDictionary<string, string> AdditionalProperties", " public System.Collections.Generic.IDictionary<string, JToken> AdditionalProperties")
                            //I dont like the required attribute, changed to allow nulls
                            .Replace(", Required = Newtonsoft.Json.Required.Always)]", ", Required = Newtonsoft.Json.Required.AllowNull)]")
                            .Replace("[System.ComponentModel.DataAnnotations.Required]", "");

                        string codeFile = "";
                        var filename = name + ".cs";

                        var split = service.href.Split('.');
                        var codeDir = Path.GetDirectoryName(details.Project);

                        if (split[0].ToUpper() == "ICE")
                        {
                            codeFile = codeDir + name + ".cs";
                            addReference(details.Project, filename);
                        }
                        else
                        {
                            codeFile = codeDir + name + ".cs";
                            addReference(details.Project, filename);
                        }
                        File.WriteAllText(codeFile, code);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("{1} : <------> {0}", ex, name);
                        string directory = AppDomain.CurrentDomain.BaseDirectory + @"/Logs/";
                        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
                        File.AppendAllText(directory + DateTime.Now.ToString("MMDDYYYY_hhmmssfffff") + ".txt", name + Environment.NewLine + ex);
                    }
                }
            }
            return true;
        }

        private static WebClient getWebClient()
        {
            WebClient client = new WebClient();
            ServicePointManager.ServerCertificateValidationCallback += (senderC, cert, chain, sslPolicyErrors) => true;
            client.UseDefaultCredentials = true;
            return client;
        }
        private static void addReference(string projectFile, string filename)
        {
            using (var collection = new Microsoft.Build.Evaluation.ProjectCollection())
            {
                collection.LoadProject(projectFile);
                var project = collection.LoadedProjects.FirstOrDefault(o => o.FullPath == projectFile);
                var items = project.GetItems("Compile");
                if (!items.Any(o => o.EvaluatedInclude == filename || o.UnevaluatedInclude == filename))
                {
                    project.AddItem("Compile", filename);
                    project.Save();
                }

                collection.UnloadProject(project);
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2007/app")]
    public partial class serviceWorkspace
    {

        private title titleField;

        private serviceWorkspaceCollection[] collectionField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/Atom")]
        public title title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("collection")]
        public serviceWorkspaceCollection[] collection
        {
            get
            {
                return this.collectionField;
            }
            set
            {
                this.collectionField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2005/Atom")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.w3.org/2005/Atom", IsNullable = false)]
    public partial class title
    {

        private string typeField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string type
        {
            get
            {
                return this.typeField;
            }
            set
            {
                this.typeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.w3.org/2007/app")]
    public partial class serviceWorkspaceCollection
    {

        private title titleField;

        private string[] textField;

        private string hrefField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://www.w3.org/2005/Atom")]
        public title title
        {
            get
            {
                return this.titleField;
            }
            set
            {
                this.titleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string[] Text
        {
            get
            {
                return this.textField;
            }
            set
            {
                this.textField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string href
        {
            get
            {
                return this.hrefField;
            }
            set
            {
                this.hrefField = value;
            }
        }
    }

    public class EpicorDetails
    {
        public string Namespace { get; set; }
        public bool useBaseClass { get; set; }
        public string BaseClass { get; set; }
        public string APIURL { get; set; }
        public string Project { get; set; }
    }
}
