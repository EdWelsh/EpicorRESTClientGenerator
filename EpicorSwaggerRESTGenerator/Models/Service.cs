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

namespace EpicorSwaggerRESTGenerator.Models
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
        public static service getServices(string serviceURL, EpicorDetails details)
        {
            using (WebClient client = Client.getWebClient(string.IsNullOrEmpty(details.Username) ? "" : details.Username, string.IsNullOrEmpty(details.Password) ? "" : details.Password))
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
            using (WebClient client = Client.getWebClient(string.IsNullOrEmpty(details.Username) ? "" : details.Username, string.IsNullOrEmpty(details.Password) ? "" : details.Password))
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
                                var post = j.First["post"];
                                if (post != null)
                                {
                                    var postOpID = j.First["post"]["operationId"];
                                    if (postOpID != null)
                                    {
                                        j.First["post"]["operationId"] = j.Name.Replace(@"\", "").Replace("/", "");
                                    }
                                }
                            }
                        }

                        string output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);

                        var document = await SwaggerDocument.FromJsonAsync(output);
                        var settings = new SwaggerToCSharpClientGeneratorSettings() { ClassName = name, OperationNameGenerator = new SingleClientFromOperationIdOperationNameGenerator() };
                        var generator = new SwaggerToCSharpClientGenerator(document, settings);
                        if (details.useBaseClass) generator.Settings.ClientBaseClass = details.BaseClass;
                        generator.Settings.UseHttpClientCreationMethod = true;
                        generator.Settings.AdditionalNamespaceUsages = new[] { "Newtonsoft.Json", "Newtonsoft.Json.Linq" };
                        generator.Settings.GenerateSyncMethods = true;

                        var code = generator.GenerateFile();
                        code = code
                            //need to replace with my actual namespace
                            .Replace("MyNamespace", details.Namespace + "." + service.href.Replace("-", ""))
                            //Had an error so added but I dont think this replacement is needed for all scenarios, maybe add flag in details later
                            .Replace("var client_ = await CreateHttpClientAsync(cancellationToken).ConfigureAwait(false);", "var client_ = CreateHttpClientAsync(cancellationToken);")
                            //.Replace("var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);", "var response_ = await client_.SendAsync(request_, System.Net.Http.HttpCompletionOption.ResponseHeadersRead, cancellationToken);")
                            //.Replace("var responseData_ = await response_.Content.ReadAsStringAsync().ConfigureAwait(false);", "var responseData_ = await response_.Content.ReadAsStringAsync();")
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
                            .Replace("[System.ComponentModel.DataAnnotations.Required]", "")
                            .Replace(@"public string BaseUrl", "public new string BaseUrl")
                        //.Replace("get { return _baseUrl; }", "get;")
                        //.Replace("set { _baseUrl = value; }", "set;");
                        ;
                        //addReference(details.Project, service.href + ".cs");
                        File.WriteAllText(Path.GetDirectoryName(details.Project) + "\\" + service.href + ".cs", code);
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
}
