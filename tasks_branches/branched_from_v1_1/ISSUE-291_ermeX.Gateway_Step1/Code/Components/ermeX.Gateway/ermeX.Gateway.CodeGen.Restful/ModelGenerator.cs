using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using Common.Logging;
using ermeX.Gateway.CodeGen.Restful.Models;
using ermeX.Gateway.Configuration;
using ermeX.Gateway.ConfigurationManagement.Settings;

namespace ermeX.Gateway.CodeGen.Restful
{
    /// <remark>
    /// The generator was onlt tested with WSDL at http://www.ibm.com/developerworks/webservices/library/ws-restwsdl/
    /// Some assumption was made:
    /// - Interface, binding, operation references are only searched within one document
    /// </remark>
    public class ModelGenerator
    {
        private RestfulServiceDefinition _serviceDefinition;
        private XElement _wsdlRoot;
        private Document _document;
        private Dictionary<string, XNamespace> _namespaces;
        private readonly ILog _logger = LogManager.GetLogger(StaticSettings.LoggerName);

        public ModelGenerator(RestfulServiceDefinition serviceDefinition)
        {
            _serviceDefinition = serviceDefinition;
            LoadDocument();
        }

        private void LoadDocument()
        {
            var webClient = new WebClient();
            var wsdlContent = webClient.DownloadString(_serviceDefinition.host);

            _wsdlRoot = XDocument.Parse(wsdlContent).Root;

            _namespaces = _wsdlRoot.Attributes()
                            .Where(x => x.IsNamespaceDeclaration)
                            .GroupBy(x => x.Name.Namespace == XNamespace.None ? String.Empty : x.Name.LocalName, x => XNamespace.Get(x.Value))
                            .ToDictionary(g => g.Key, g => g.First());
        }

        private List<XElement> LoadImportedSchema()
        {
            var typeElement = _wsdlRoot.Elements()
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
            _document = new Document();

            var interfaceElement = _wsdlRoot.Elements()
                                    .Where(x => x.Name.LocalName == "interface"
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");
            GenerateInterface(interfaceElement);

            var bindingElement = _wsdlRoot.Elements()
                                    .Where(x => x.Name.LocalName == "binding"
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");
            GenerateBinding(bindingElement);

            var serviceElement = _wsdlRoot.Elements()
                                    .Where(x => x.Name.LocalName == "service"
                                        && x.Name.Namespace == "http://www.w3.org/ns/wsdl");
            GenerateServices(serviceElement);

            return _document;
        }

        private void GenerateServices(IEnumerable<XElement> serviceElements)
        {
            _document.Services = new List<Service>();

            foreach (var serviceElement in serviceElements)
            {
                var service = new Service();

                service.Name = serviceElement.Attribute("name").Value;

                if (_document.Interfaces != null && _document.Interfaces.Count != 0)
                {
                    var interfaceName = serviceElement.Attribute("interface").Value.Split(':')[1];
                    service.Interface = _document.Interfaces.First(x => x.Name == interfaceName);
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

                    if (_document.Bindings != null && _document.Bindings.Count != 0)
                    {
                        var bindingName = endpointElement.Attribute("binding").Value.Split(':')[1];
                        endpoint.Binding = _document.Bindings.First(x => x.Name == bindingName);
                    }
                    else
                    {
                        throw new Exception("The document has no binding");
                    }

                    service.Endpoints.Add(endpoint);
                }

                _document.Services.Add(service);
            }
        }

        private void GenerateBinding(IEnumerable<XElement> bindingElements)
        {
            _document.Bindings = new List<Binding>();

            foreach (var bindingElement in bindingElements)
            {
                var binding = new Binding();

                binding.Name = bindingElement.Attribute("name").Value;

                if (_document.Interfaces != null && _document.Interfaces.Count != 0)
                {
                    var interfaceName = bindingElement.Attribute("interface").Value.Split(':')[1];
                    binding.Interface = _document.Interfaces.First(x => x.Name == interfaceName);
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
                _document.Bindings.Add(binding);
            }
        }

        private void GenerateInterface(IEnumerable<XElement> interfaceElements)
        {
            _document.Interfaces = new List<Interface>();

            foreach (var interfaceElement in interfaceElements)
            {
                // Avoid naming conflit with reserve key words
                var _interface = new Interface();
                _interface.Name = interfaceElement.Attribute("name").Value;
                _interface.NamespaceUri = interfaceElement.Name.NamespaceName;
                _logger.Trace(x => x("{0} - {1}", _interface.Name, _interface.NamespaceUri));

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

                _document.Interfaces.Add(_interface);
            }
        }

        private void GenerateSchema(IEnumerable<XElement> typeElements)
        {
        }
    }
}