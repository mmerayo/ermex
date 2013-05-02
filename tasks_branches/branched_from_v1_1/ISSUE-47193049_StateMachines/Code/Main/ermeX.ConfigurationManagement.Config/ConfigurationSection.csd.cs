//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.296
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// The ermeXConfiguration Configuration Section.
    /// </summary>
    public partial class ermeXConfiguration : global::System.Configuration.ConfigurationSection
    {
        
        #region Singleton Instance
        /// <summary>
        /// The XML name of the ermeXConfiguration Configuration Section.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string ermeXConfigurationSectionName = "ermeXConfiguration";
        
        /// <summary>
        /// Gets the ermeXConfiguration instance.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public static global::ermeX.Configuration.ermeXConfiguration Instance
        {
            get
            {
                return ((global::ermeX.Configuration.ermeXConfiguration)(global::System.Configuration.ConfigurationManager.GetSection(global::ermeX.Configuration.ermeXConfiguration.ermeXConfigurationSectionName)));
            }
        }
        #endregion
        
        #region Xmlns Property
        /// <summary>
        /// The XML name of the <see cref="Xmlns"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string XmlnsPropertyName = "xmlns";
        
        /// <summary>
        /// Gets the XML namespace of this Configuration Section.
        /// </summary>
        /// <remarks>
        /// This property makes sure that if the configuration file contains the XML namespace,
        /// the parser doesn't throw an exception because it encounters the unknown "xmlns" attribute.
        /// </remarks>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.ermeXConfiguration.XmlnsPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public string Xmlns
        {
            get
            {
                return ((string)(base[global::ermeX.Configuration.ermeXConfiguration.XmlnsPropertyName]));
            }
        }
        #endregion
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region ComponentDefinition Property
        /// <summary>
        /// The XML name of the <see cref="ComponentDefinition"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string ComponentDefinitionPropertyName = "componentDefinition";
        
        /// <summary>
        /// Gets or sets the ComponentDefinition.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The ComponentDefinition.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.ermeXConfiguration.ComponentDefinitionPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual global::ermeX.Configuration.LocalComponent ComponentDefinition
        {
            get
            {
                return ((global::ermeX.Configuration.LocalComponent)(base[global::ermeX.Configuration.ermeXConfiguration.ComponentDefinitionPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.ermeXConfiguration.ComponentDefinitionPropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// the configuration of the local component
    /// </summary>
    public partial class LocalComponent : global::System.Configuration.ConfigurationElement
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region ComponentId Property
        /// <summary>
        /// The XML name of the <see cref="ComponentId"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string ComponentIdPropertyName = "componentId";
        
        /// <summary>
        /// Gets or sets the unique identifier of the ermeX component in the ermeX network
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The unique identifier of the ermeX component in the ermeX network")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.ComponentIdPropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false)]
        public virtual global::System.Guid ComponentId
        {
            get
            {
                return ((global::System.Guid)(base[global::ermeX.Configuration.LocalComponent.ComponentIdPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.ComponentIdPropertyName] = value;
            }
        }
        #endregion
        
        #region TcpPort Property
        /// <summary>
        /// The XML name of the <see cref="TcpPort"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string TcpPortPropertyName = "tcpPort";
        
        /// <summary>
        /// Gets or sets tCP port the components listens to
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("TCP port the components listens to")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.TcpPortPropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false, DefaultValue=8135)]
        public virtual int TcpPort
        {
            get
            {
                return ((int)(base[global::ermeX.Configuration.LocalComponent.TcpPortPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.TcpPortPropertyName] = value;
            }
        }
        #endregion
        
        #region MessagesExpirationDays Property
        /// <summary>
        /// The XML name of the <see cref="MessagesExpirationDays"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string MessagesExpirationDaysPropertyName = "messagesExpirationDays";
        
        /// <summary>
        /// Gets or sets the days after a message is expired. 
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("the days after a message is expired. ")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.MessagesExpirationDaysPropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false, DefaultValue=31)]
        public virtual int MessagesExpirationDays
        {
            get
            {
                return ((int)(base[global::ermeX.Configuration.LocalComponent.MessagesExpirationDaysPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.MessagesExpirationDaysPropertyName] = value;
            }
        }
        #endregion
        
        #region DiscoverSubscriptors Property
        /// <summary>
        /// The XML name of the <see cref="DiscoverSubscriptors"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DiscoverSubscriptorsPropertyName = "discoverSubscriptors";
        
        /// <summary>
        /// Gets or sets indicates if the message subscriptor types should be discovered when the component is started
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("indicates if the message subscriptor types should be discovered when the componen" +
            "t is started")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.DiscoverSubscriptorsPropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false, DefaultValue=true)]
        public virtual bool DiscoverSubscriptors
        {
            get
            {
                return ((bool)(base[global::ermeX.Configuration.LocalComponent.DiscoverSubscriptorsPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.DiscoverSubscriptorsPropertyName] = value;
            }
        }
        #endregion
        
        #region DiscoverServices Property
        /// <summary>
        /// The XML name of the <see cref="DiscoverServices"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DiscoverServicesPropertyName = "discoverServices";
        
        /// <summary>
        /// Gets or sets indicates if the services should be discovered when the component is started
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("indicates if the services should be discovered when the component is started")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.DiscoverServicesPropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false, DefaultValue=true)]
        public virtual bool DiscoverServices
        {
            get
            {
                return ((bool)(base[global::ermeX.Configuration.LocalComponent.DiscoverServicesPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.DiscoverServicesPropertyName] = value;
            }
        }
        #endregion
        
        #region FriendComponent Property
        /// <summary>
        /// The XML name of the <see cref="FriendComponent"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string FriendComponentPropertyName = "friendComponent";
        
        /// <summary>
        /// Gets or sets the friend component settings. Its used to join to the network. Dont configure if this component is the first added and the others will join to it
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("the friend component settings. Its used to join to the network. Dont configure if" +
            " this component is the first added and the others will join to it")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.FriendComponentPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual global::ermeX.Configuration.FriendComponentElement FriendComponent
        {
            get
            {
                return ((global::ermeX.Configuration.FriendComponentElement)(base[global::ermeX.Configuration.LocalComponent.FriendComponentPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.FriendComponentPropertyName] = value;
            }
        }
        #endregion
        
        #region Database Property
        /// <summary>
        /// The XML name of the <see cref="Database"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DatabasePropertyName = "database";
        
        /// <summary>
        /// Gets or sets the Database.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The Database.")]
        [global::System.Configuration.CallbackValidatorAttribute(Type=typeof(global::ermeX.Configuration.DatabaseCallbackValidatorClass), CallbackMethodName="ValidateCallback")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.LocalComponent.DatabasePropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual global::ermeX.Configuration.Database Database
        {
            get
            {
                return ((global::ermeX.Configuration.Database)(base[global::ermeX.Configuration.LocalComponent.DatabasePropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.LocalComponent.DatabasePropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// the component to request join network. Only one is needed
    /// </summary>
    public partial class FriendComponentElement : global::System.Configuration.ConfigurationElement
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region RemoteIp Property
        /// <summary>
        /// The XML name of the <see cref="RemoteIp"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string RemoteIpPropertyName = "remoteIp";
        
        /// <summary>
        /// Gets or sets the remote component IP
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("the remote component IP")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.FriendComponentElement.RemoteIpPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual string RemoteIp
        {
            get
            {
                return ((string)(base[global::ermeX.Configuration.FriendComponentElement.RemoteIpPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.FriendComponentElement.RemoteIpPropertyName] = value;
            }
        }
        #endregion
        
        #region RemoteTcpPort Property
        /// <summary>
        /// The XML name of the <see cref="RemoteTcpPort"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string RemoteTcpPortPropertyName = "remoteTcpPort";
        
        /// <summary>
        /// Gets or sets the remote component TcpPort
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("the remote component TcpPort")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.FriendComponentElement.RemoteTcpPortPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual int RemoteTcpPort
        {
            get
            {
                return ((int)(base[global::ermeX.Configuration.FriendComponentElement.RemoteTcpPortPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.FriendComponentElement.RemoteTcpPortPropertyName] = value;
            }
        }
        #endregion
        
        #region RemoteComponentId Property
        /// <summary>
        /// The XML name of the <see cref="RemoteComponentId"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string RemoteComponentIdPropertyName = "remoteComponentId";
        
        /// <summary>
        /// Gets or sets the remote component id in the ermeX Network
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The remote component id in the ermeX Network")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.FriendComponentElement.RemoteComponentIdPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual global::System.Guid RemoteComponentId
        {
            get
            {
                return ((global::System.Guid)(base[global::ermeX.Configuration.FriendComponentElement.RemoteComponentIdPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.FriendComponentElement.RemoteComponentIdPropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// The Database Configuration Element.
    /// </summary>
    public partial class Database : global::System.Configuration.ConfigurationElement
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region DbType Property
        /// <summary>
        /// The XML name of the <see cref="DbType"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DbTypePropertyName = "dbType";
        
        /// <summary>
        /// Gets or sets the db type. Values: InMemory, SQLite, SqlServer
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("the db type. Values: InMemory, SQLite, SqlServer")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.Database.DbTypePropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false, DefaultValue=DbType.InMemory)]
        public virtual global::ermeX.Configuration.DbType DbType
        {
            get
            {
                return ((global::ermeX.Configuration.DbType)(base[global::ermeX.Configuration.Database.DbTypePropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.Database.DbTypePropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// The PhisicalDatabase Configuration Element.
    /// </summary>
    public partial class PhisicalDatabase : global::ermeX.Configuration.Database
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region ConnectionString Property
        /// <summary>
        /// The XML name of the <see cref="ConnectionString"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string ConnectionStringPropertyName = "connectionString";
        
        /// <summary>
        /// Gets or sets the connectionstring of the database
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("the connectionstring of the database")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.PhisicalDatabase.ConnectionStringPropertyName, IsRequired=true, IsKey=false, IsDefaultCollection=false)]
        public virtual string ConnectionString
        {
            get
            {
                return ((string)(base[global::ermeX.Configuration.PhisicalDatabase.ConnectionStringPropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.PhisicalDatabase.ConnectionStringPropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// The SqlServerDatabase Configuration Element.
    /// </summary>
    public partial class SqlServerDatabase : global::ermeX.Configuration.PhisicalDatabase
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region DbType Property
        /// <summary>
        /// The XML name of the <see cref="DbType"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DbTypePropertyName = "dbType";
        
        /// <summary>
        /// Gets or sets the DbType.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The DbType.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.SqlServerDatabase.DbTypePropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false, DefaultValue=DbType.SqlServer)]
        public virtual global::ermeX.Configuration.DbType DbType
        {
            get
            {
                return ((global::ermeX.Configuration.DbType)(base[global::ermeX.Configuration.SqlServerDatabase.DbTypePropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.SqlServerDatabase.DbTypePropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// The SqliteDatabase Configuration Element.
    /// </summary>
    public partial class SqliteDatabase : global::ermeX.Configuration.PhisicalDatabase
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region DbType Property
        /// <summary>
        /// The XML name of the <see cref="DbType"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DbTypePropertyName = "dbType";
        
        /// <summary>
        /// Gets or sets the DbType.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The DbType.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.SqliteDatabase.DbTypePropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false, DefaultValue=DbType.SQLite)]
        public virtual global::ermeX.Configuration.DbType DbType
        {
            get
            {
                return ((global::ermeX.Configuration.DbType)(base[global::ermeX.Configuration.SqliteDatabase.DbTypePropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.SqliteDatabase.DbTypePropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// The InMemoryDatabase Configuration Element.
    /// </summary>
    public partial class InMemoryDatabase : global::ermeX.Configuration.Database
    {
        
        #region IsReadOnly override
        /// <summary>
        /// Gets a value indicating whether the element is read-only.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public override bool IsReadOnly()
        {
            return false;
        }
        #endregion
        
        #region DbType Property
        /// <summary>
        /// The XML name of the <see cref="DbType"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string DbTypePropertyName = "dbType";
        
        /// <summary>
        /// Gets or sets the DbType.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The DbType.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Configuration.InMemoryDatabase.DbTypePropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false, DefaultValue=DbType.InMemory)]
        public virtual global::ermeX.Configuration.DbType DbType
        {
            get
            {
                return ((global::ermeX.Configuration.DbType)(base[global::ermeX.Configuration.InMemoryDatabase.DbTypePropertyName]));
            }
            set
            {
                base[global::ermeX.Configuration.InMemoryDatabase.DbTypePropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// DbType.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
    public enum DbType
    {
        
        /// <summary>
        /// InMemory.
        /// </summary>
        InMemory,
        
        /// <summary>
        /// SQLite.
        /// </summary>
        SQLite,
        
        /// <summary>
        /// SqlServer.
        /// </summary>
        SqlServer,
    }
}
namespace ermeX.Configuration
{
    
    
    /// <summary>
    /// Class for the Database callback validator
    /// </summary>
    public partial class DatabaseCallbackValidatorClass
    {
        
        /// <summary>
        /// Validation callback for the Database callback validator
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <exception cref="global::System.ArgumentException">The value was not valid.</exception>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public static void ValidateCallback(object value)
        {
            // IMPORTANT NOTE: The code below does not build by default.
            // You have placed a callback validator on this property.
            // Copy the commented code below to a separate file and 
            // implement the method.
            // 
            // public partial class DatabaseCallbackValidatorClass
            // {
            //     
            //     public static void Validate(object value)
            //     {
            //         throw new global::System.NotImplementedException();
            //     }
            // }
            // 
            global::ermeX.Configuration.DatabaseCallbackValidatorClass.Validate(value);
        }
    }
}
