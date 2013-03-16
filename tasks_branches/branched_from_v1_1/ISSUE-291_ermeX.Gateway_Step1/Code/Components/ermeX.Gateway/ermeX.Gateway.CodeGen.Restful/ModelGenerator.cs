using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Gateway.Configuration;
using ermeX.Gateway.ConfigurationManagement.Settings;
using ermeX.Gateway.CodeGen.Restful.Models;
using System.Xml.Linq;
using System.Net;
using Common.Logging;

namespace ermeX.Gateway.CodeGen.Restful
{
    /// <remark>
    /// The generator was onlt tested with WSDL at http://www.ibm.com/developerworks/webservices/library/ws-restwsdl/
    /// Some assumption was made:
    /// - Interface, binding, operation references are only searched within one document
    /// </remark>
    public class ModelGenerator
    {
        private RestfulServiceDefinition ServiceDefinition;
        private XElement WsdlRoot;
        private Document Document;
        private Dictionary<string, XNamespace> Namespaces;
        private readonly ILog Logger = LogManager.GetLogger(StaticSettings.LoggerName);

        public ModelGenerator(RestfulServiceDefinition serviceDefinition)
        {
            ServiceDefinition = serviceDefinition;
            LoadDocument();
        }

        private void LoadDocument()
        {
            var webClient = new WebClient();
            var wsdlContent = webClient.DownloadString(ServiceDefinition.host);

            WsdlRoot = XDocument.Parse(wsdlContent).Root;

            Namespaces = WsdlRoot.Attributes()
                            .Where(x => x.IsNamespaceDeclaration)
                            .GroupBy(x => x.Name.Namespace == XNamespace.None ? String.Empty : x.Name.LocalName, x => XNamespace.Get(x.Value))
                            .ToDictionary(g => g.Key, g => g.First());
        }

        private List<XElement> LoadImportedSchema()
        {
            var typeElement = WsdlRoot.Elements()
                                .Where(x => x.Name.LocalName == "types"
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");

            var importElements = typeElement.Elements()
                                .Where(x => x.Name.LocalName == "import"
                                        && x.Name.Namespace == "http://www.w3.org/2001/XMLSchema");

            if (importElements.Count() > 0)
            {
                foreach (var element in importElements)
                {

                }
            }
            return null;
        }

        public Document GenerateModel()
        {
            Document = new Document();
            
            var interfaceElement = WsdlRoot.Elements()
                                    .Where(x => x.Name.LocalName == "interface" 
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");
            GenerateInterface(interfaceElement);
            
            var bindingElement = WsdlRoot.Elements()
                                    .Where(x => x.Name.LocalName == "binding" 
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");
            GenerateBinding(bindingElement);

            var serviceElement = WsdlRoot.Elements()
                                    .Where(x => x.Name.LocalName == "service"
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");
            GenerateServices(serviceElement);

            return Document;
        }

        private void GenerateServices(IEnumerable<XElement> serviceElements)
        {
            Document.Services = new List<Service>();

            foreach (var serviceElement in serviceElements)
            {
                var service = new Service();

                service.Name = serviceElement.Attribute("name").Value;

                if (Document.Interfaces != null && Document.Interfaces.Count != 0)
                {
                    var interfaceName = serviceElement.Attribute("interface").Value.Split(':')[1];
                    service.Interface = Document.Interfaces.First(x => x.Name == interfaceName);
                }
                else
                {
                    throw new Exception("The document has no interface.");
                }

                service.Endpoints = new List<Endpoint>();

                var endpointElements = serviceElement.Elements().Where(x => x.Name.LocalName == "endpoint" && x.Name.Namespace == "http://www.w3.org/ns/wsdl");

                foreach (var endpointElement in endpointElements)
                {
                    var endpoint = new Endpoint();

                    endpoint.Name = endpointElement.Attribute("name").Value;

                    endpoint.Address = endpointElement.Attribute("address") != null ? endpointElement.Attribute("address").Value : "";

                    if (Document.Bindings != null && Document.Bindings.Count != 0)
                    {
                        var bindingName = endpointElement.Attribute("binding").Value.Split(':')[1];
                        endpoint.Binding = Document.Bindings.First(x => x.Name == bindingName);
                    }
                    else
                    {
                        throw new Exception("The document has no binding");
                    }

                    service.Endpoints.Add(endpoint);
                }

                Document.Services.Add(service);
            }
        }

        private void GenerateBinding(IEnumerable<XElement> bindingElements)
        {
            Document.Bindings = new List<Binding>();

            foreach (var bindingElement in bindingElements)
            {
                var binding = new Binding();

                binding.Name = bindingElement.Attribute("name").Value;

                if (Document.Interfaces != null && Document.Interfaces.Count != 0)
                {
                    var interfaceName = bindingElement.Attribute("interface").Value.Split(':')[1];
                    binding.Interface = Document.Interfaces.First(x => x.Name == interfaceName);
                }

                binding.Operations = new List<BindingOperation>();

                var operationElements = bindingElement.Elements()
                                            .Where(x => x.Name.LocalName == "operation"
                                                    && x.Name.Namespace == "http://www.w3.org/ns/wsdl");

                if (operationElements.Count() > 0)
                {
                    foreach (var operationElement in operationElements)
                    {
                        var operation = new BindingOperation();

                        var refOperationName = operationElement.Attribute("ref").Value.Split(':')[1];
                        operation.RefOperation = binding.Interface.Operations.First(x => x.Name == refOperationName);

                        operation.HttpMethod = operationElement.Attributes()
                                                        .First(attr => attr.Name.LocalName == "method"
                                                                && attr.Name.NamespaceName == "http://www.w3.org/ns/wsdl/http")
                                                        .Value;
                        binding.Operations.Add(operation);
                    }
                }
                Document.Bindings.Add(binding);
            }
        }

        private void GenerateInterface(IEnumerable<XElement> interfaceElements)
        {
            Document.Interfaces = new List<Interface>();

            foreach (var interfaceElement in interfaceElements)
            {
                // Avoid naming conflit with reserve key words
                var _interface = new Interface();
                _interface.Name = interfaceElement.Attribute("name").Value;
                _interface.NamespaceUri = interfaceElement.Name.NamespaceName;
                Logger.Trace(x => x("{0} - {1}", _interface.Name, _interface.NamespaceUri));

                var operationElements = interfaceElement.Elements()
                                                .Where(x => x.Name.LocalName == "operation"
                                                    && x.Name.Namespace == "http://www.w3.org/ns/wsdl");

                if (operationElements.Count() > 0)
                {
                    _interface.Operations = operationElements.Select(x => new InterfaceOperation
                                                {
                                                    Name = x.Attribute("name").Value,
                                                    InputType = null,
                                                    OutputType = null
                                                }).ToList();
                }

                Document.Interfaces.Add(_interface);
            }
        }

        private void GenerateSchema(IEnumerable<XElement> typeElements)
        {

        }

    }
}
