using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ermeX.Configuration;
using ermeX.ConfigurationManagement.Settings.Data.DbEngines;
using ermeX.Tests.Common.Networking;

namespace ermeX.Tests.ConfigurationManagement.Config
{
    [TestFixture]
    public class ConfigurationSectionTests
    {
        [Test]
        public void Validates_NormalSettings()
        {
            ermeXConfiguration target;
            var config = GetConfiguration(out target);
            Assert.IsNotNull(target, "Could not load the section from the test.config");

            Database db = null;
            var componentDefinition = new LocalComponent();

            componentDefinition.TcpPort = 6666;
            componentDefinition.ComponentId = Guid.NewGuid();
            componentDefinition.MessagesExpirationDays = 1;
        
            target.ComponentDefinition = componentDefinition;
            foreach (var value in Enum.GetValues(typeof (DbEngineType)))
            {
                switch ((DbEngineType) value)
                {
                    case DbEngineType.SqlServer2008:
                        db = new SqlServerDatabase() {ConnectionString = "thisistheconnstr"};
                        break;
                    case DbEngineType.Sqlite:
                        db = new SqliteDatabase() {ConnectionString = "thisistheconnstr"};
                        break;
                    case DbEngineType.SqliteInMemory:
                        db = new InMemoryDatabase();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                target.ComponentDefinition.Database = db;
                config.Save(ConfigurationSaveMode.Modified);
            }
        }

        

        [Test]
        public void ValidatesDatabase()
        {
            ermeXConfiguration target;
            var config = GetConfiguration(out target);
            Assert.IsNotNull(target, "Could not load the section from the test.config");

            Database db = null;
            target.ComponentDefinition = new LocalComponent()
            {
                ComponentId = Guid.NewGuid(),
                TcpPort = 6666,
                MessagesExpirationDays = 1,

            };
            foreach (var value in Enum.GetValues(typeof(DbEngineType)))
            {
                switch ((DbEngineType)value)
                {
                    case DbEngineType.SqlServer2008:
                        db = new SqlServerDatabase() ; //no connstr
                        break;
                    case DbEngineType.Sqlite:
                        db = new SqliteDatabase() ; //no connstr
                        break;
                    case DbEngineType.SqliteInMemory:
                       // db = new InMemoryDatabase();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                Assert.Throws<ConfigurationErrorsException>(()=>target.ComponentDefinition.Database = db);
                config.Save(ConfigurationSaveMode.Modified);
                
            }
        }

        [Test]
        public void ValidatesLocalComponent_HasDb()
        {
            ermeXConfiguration target;
            var config = GetConfiguration(out target);

            Database db = null;
            target.ComponentDefinition = new LocalComponent()
            {
                ComponentId = Guid.NewGuid(),
                TcpPort = 6666,
                MessagesExpirationDays = 1,
            };
            Assert.IsNotNull(target.ComponentDefinition.Database);

            Assert.DoesNotThrow(()=>config.Save(ConfigurationSaveMode.Modified));

            Assert.IsTrue(target.ComponentDefinition.Database.DbType==DbType.InMemory);
        }

       

        [Test]
        public void Can_Start_Lonely_Component()
        {
            ermeXConfiguration target;
            var config = GetConfiguration(out target);

            target.ComponentDefinition = new LocalComponent()
                {
                    ComponentId = Guid.NewGuid()
                };
            config.Save(ConfigurationSaveMode.Minimal);

            Assert.DoesNotThrow(()=>WorldGate.ConfigureAndStart());
        }

        [Ignore("TODO")]
        [Test]
        public void Can_Start_Joined_To_Friend_Component()
        {
            
        }

        private static System.Configuration.Configuration GetConfiguration(out ermeXConfiguration target)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //target = (ermeXConfiguration) config.GetSection("ermeXConfiguration");
            config.Sections.Remove("ermeXConfiguration");
            target = new ermeXConfiguration();
            config.Sections.Add("ermeXConfiguration", target);
            return config;
        }

    }
}
