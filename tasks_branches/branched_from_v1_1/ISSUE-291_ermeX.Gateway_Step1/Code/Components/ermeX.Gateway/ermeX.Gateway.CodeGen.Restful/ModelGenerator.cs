using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ermeX.Gateway.Configuration;
using ermeX.Gateway.CodeGen.Restful.Models;
using System.Xml.Linq;
using System.Net;

namespace ermeX.Gateway.CodeGen.Restful
{
    public class ModelGenerator
    {
        private RestfulServiceDefinition ServiceDefinition;
        private XElement WsdlRoot;
        private Document Document;

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
                    service.Interface = Document.Interfaces.First(x => x.Name == serviceElement.Attribute("interface").Value);
                }
                else
                {
                    throw new Exception("The document has no interface.");
                }

                var endpointElements = serviceElement.Elements().Where(x => x.Name.LocalName == "endpoint" && x.Name.Namespace == "http://www.w3.org/ns/wsdl");

                foreach (var endpointElement in endpointElements)
                {
                    var endpoint = new Endpoint();

                    endpoint.Name = endpointElement.Attribute("name").Value;

                    endpoint.Address = endpointElement.Attribute("address") != null ? endpointElement.Attribute("address").Value : "";

                    if (Document.Bindings != null && Document.Bindings.Count == 0)
                    {
                        endpoint.Binding = Document.Bindings.First(x => x.Name == endpointElement.Attribute("binding").Value);
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
                    binding.Interface = Document.Interfaces.First(x => x.Name == bindingElement.Attribute("interface").Value);
                }

                var operationElements = bindingElement.Elements()
                                            .Where(x => x.Name.LocalName == "operation"
                                                    && x.Name.Namespace == "http://www.w3.org/ns/wsdl");

                if (operationElements.Count() > 0)
                {
                    foreach (var operationElement in operationElements)
                    {
                        var operation = new BindingOperation();
                        operation.RefOperation = binding.Interface.Operations.First(x => x.Name == operationElement.Attribute("ref").Value);
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

        private void GenerateSchema()
        {
            throw new NotImplementedException();
        }
    }
}
