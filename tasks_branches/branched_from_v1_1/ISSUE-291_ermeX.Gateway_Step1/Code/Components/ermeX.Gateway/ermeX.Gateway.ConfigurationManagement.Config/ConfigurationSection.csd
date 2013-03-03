<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="76dd11c0-04dd-44a1-97cd-9d9ff2a52d9b" namespace="ermeX.Gateway.Configuration" xmlSchemaNamespace="ermeX.Gateway.Configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="ermeXGatewayConfiguration" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="ermeXGatewayConfiguration">
      <elementProperties>
        <elementProperty name="RestfulServiceDefinition" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="restfulServiceDefinition" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/76dd11c0-04dd-44a1-97cd-9d9ff2a52d9b/RestfulServiceDefinition" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="RestfulServiceDefinition">
      <attributeProperties>
        <attributeProperty name="host" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="host" isReadOnly="false">
          <type>
            <externalTypeMoniker name="/76dd11c0-04dd-44a1-97cd-9d9ff2a52d9b/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>