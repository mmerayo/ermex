using System;
using System.Reflection;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;
using UoW=ermeX.DAL.DataAccess.UnitOfWork.UnitOfWork;
namespace ermeX.Tests.DAL.Integration.UnitOfWork
{
    [TestFixture]
    public class Test_usage_of_UnitOfWork
    {
        [SetUp]
        public void SetupContext()
        {
            UoW.Configuration.AddAssembly(Assembly.GetExecutingAssembly());
            new SchemaExport(UoW.Configuration).Execute(false, true, false, false);
        }

        [Test]
        public void Can_add_a_new_instance_of_an_entity_to_the_database()
        {
            using (UoW.Start())
            {
                var person = new Person {Name = "John Doe", Birthdate = new DateTime(1915, 12, 15)};
                UoW.CurrentSession.Save(person);
                UoW.Current.TransactionalFlush();
            }
        }
    }

    public class Person
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual DateTime Birthdate { get; set; }
    }
}