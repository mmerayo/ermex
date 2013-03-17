using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using Common.Logging;
using ermeX.Gateway.CodeGen.Restful;
using NUnit.Framework;

namespace ermeX.Gateway.Test.ModelGeneratorTester
{
    [TestFixture]
    public class ModelGeneratorTester
    {
        private ModelGenerator _modelGenerator;

        [SetUp]
        public void Setup()
        {
            var properties = new NameValueCollection();
            properties["level"] = "ALL";
            Common.Logging.LogManager.Adapter = new Common.Logging.Simple.ConsoleOutLoggerFactoryAdapter(properties);

            var config = ConfigurationManagement.ConfigurationManager.GetConfiguration();
            _modelGenerator = new ModelGenerator(config);
        }

        [Test]
        public void Generate_Model_From_Wsdl()
        {
            var document = _modelGenerator.GenerateModel();

            Assert.IsNotNull(document);
            Assert.IsTrue(document.Services.Count == 1);
            Assert.IsTrue(document.Services[0].Name == "BookList");
            Assert.IsTrue(document.Services[0].Endpoints.Count == 1);

            Assert.IsTrue(document.Bindings.Count == 1);
            Assert.IsTrue(document.Bindings[0].Name == "BookListHTTPBinding");
            Assert.IsTrue(document.Bindings[0].Operations.Count == 1);

            Assert.IsTrue(document.Interfaces.Count == 1);
            Assert.IsTrue(document.Interfaces[0].Name == "BookListInterface");
            Assert.IsTrue(document.Interfaces[0].Operations.Count == 1);
        }
    }
}
