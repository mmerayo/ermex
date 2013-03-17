//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18033
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ermeX.Gateway.Configuration
{
    
    
    /// <summary>
    /// The ermeXGatewayConfiguration Configuration Section.
    /// </summary>
    public partial class ermeXGatewayConfiguration : global::System.Configuration.ConfigurationSection
    {
        
        #region Singleton Instance
        /// <summary>
        /// The XML name of the ermeXGatewayConfiguration Configuration Section.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string ermeXGatewayConfigurationSectionName = "ermeXGatewayConfiguration";
        
        /// <summary>
        /// Gets the ermeXGatewayConfiguration instance.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        public static global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration Instance
        {
            get
            {
                return ((global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration)(global::System.Configuration.ConfigurationManager.GetSection(global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration.ermeXGatewayConfigurationSectionName)));
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
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration.XmlnsPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public string Xmlns
        {
            get
            {
                return ((string)(base[global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration.XmlnsPropertyName]));
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
        
        #region RestfulServiceDefinition Property
        /// <summary>
        /// The XML name of the <see cref="RestfulServiceDefinition"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string RestfulServiceDefinitionPropertyName = "restfulServiceDefinition";
        
        /// <summary>
        /// Gets or sets the RestfulServiceDefinition.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The RestfulServiceDefinition.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration.RestfulServiceDefinitionPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual global::ermeX.Gateway.Configuration.RestfulServiceDefinition RestfulServiceDefinition
        {
            get
            {
                return ((global::ermeX.Gateway.Configuration.RestfulServiceDefinition)(base[global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration.RestfulServiceDefinitionPropertyName]));
            }
            set
            {
                base[global::ermeX.Gateway.Configuration.ermeXGatewayConfiguration.RestfulServiceDefinitionPropertyName] = value;
            }
        }
        #endregion
    }
}
namespace ermeX.Gateway.Configuration
{
    
    
    /// <summary>
    /// The RestfulServiceDefinition Configuration Element.
    /// </summary>
    public partial class RestfulServiceDefinition : global::System.Configuration.ConfigurationElement
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
        
        #region host Property
        /// <summary>
        /// The XML name of the <see cref="host"/> property.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        internal const string hostPropertyName = "host";
        
        /// <summary>
        /// Gets or sets the host.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("ConfigurationSectionDesigner.CsdFileGenerator", "2.0.0.5")]
        [global::System.ComponentModel.DescriptionAttribute("The host.")]
        [global::System.Configuration.ConfigurationPropertyAttribute(global::ermeX.Gateway.Configuration.RestfulServiceDefinition.hostPropertyName, IsRequired=false, IsKey=false, IsDefaultCollection=false)]
        public virtual string host
        {
            get
            {
                return ((string)(base[global::ermeX.Gateway.Configuration.RestfulServiceDefinition.hostPropertyName]));
            }
            set
            {
                base[global::ermeX.Gateway.Configuration.RestfulServiceDefinition.hostPropertyName] = value;
            }
        }
        #endregion
    }
}