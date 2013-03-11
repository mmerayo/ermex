using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ermeX.Gateway.CodeGen.Restful;
using System.Diagnostics;
using System.Net;
using System.Xml.Linq;

namespace ermeX.Gateway.Test.ModelGeneratorTester
{
    [TestFixture]
    public class ModelGeneratorTester
    {
        ModelGenerator ModelGenerator;

        [SetUp]
        public void Setup()
        {
            var config = ConfigurationManagement.ConfigurationManager.GetConfiguration();
            ModelGenerator = new ModelGenerator(config);
        }

        [Test]
        public void Generate_Model_From_Wsdl()
        {
            var document = ModelGenerator.GenerateModel();

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
