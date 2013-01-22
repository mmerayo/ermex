<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="d0ed9acb-0435-4532-afdd-b5115bc4d562" namespace="ermeX" xmlSchemaNamespace="ermeX" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
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
    <configurationSection name="ermeXSection" isReadOnly="true" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="ermeXSection">
      <elementProperties>
        <elementProperty name="ComponentDefinition" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="componentDefinition" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/ComponentElement" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="ComponentElement" isReadOnly="true">
      <attributeProperties>
        <attributeProperty name="ComponentId" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="componentId" isReadOnly="true" documentation="The unique identifier of the ermeX component in the ermeX network">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="TcpPort" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="tcpPort" isReadOnly="true" documentation="TCP port the components listens to">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="MessagesExpirationDays" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="messagesExpirationDays" isReadOnly="true" documentation="the days after a message is expired. " defaultValue="31">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/TimeSpan" />
          </type>
        </attributeProperty>
        <attributeProperty name="DiscoverSubscriptors" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="discoverSubscriptors" isReadOnly="true" documentation="indicates if the message subscriptor types should be discovered when the component is started" defaultValue="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="DiscoverServices" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="discoverServices" isReadOnly="true" documentation="indicates if the services should be discovered when the component is started" defaultValue="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Boolean" />
          </type>
        </attributeProperty>
      </attributeProperties>
      <elementProperties>
        <elementProperty name="FriendComponent" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="friendComponent" isReadOnly="false" documentation="the friend component settings. Its used to join to the network. Dont configure if this component is the first added and the others will join to it">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/FriendComponentElement" />
          </type>
        </elementProperty>
        <elementProperty name="Database" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="database" isReadOnly="true">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DatabaseElement" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="FriendComponentElement" documentation="the component to request join network. Only one is needed" isReadOnly="true">
      <attributeProperties>
        <attributeProperty name="RemoteIp" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="remoteIp" isReadOnly="false" documentation="the remote component IP">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="RemoteTcpPort" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="remoteTcpPort" isReadOnly="false" documentation="the remote component TcpPort">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="RemoteComponentId" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="remoteComponentId" isReadOnly="false" documentation="The remote component id in the ermeX Network">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="DatabaseElement" isReadOnly="true" />
    <configurationElement name="PhisicalDatabaseElement" isReadOnly="true">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DatabaseElement" />
      </baseClass>
      <attributeProperties>
        <attributeProperty name="ConnectionString" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="connectionString" isReadOnly="true" documentation="the connectionstring of the database">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="SqlServerDatabaseElement" isReadOnly="true">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/PhisicalDatabaseElement" />
      </baseClass>
    </configurationElement>
    <configurationElement name="SqliteDatabaseElement" isReadOnly="true">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/PhisicalDatabaseElement" />
      </baseClass>
    </configurationElement>
    <configurationElement name="InMemoryDatabaseElement" isReadOnly="true">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DatabaseElement" />
      </baseClass>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators />
  </propertyValidators>
</configurationSectionModel>