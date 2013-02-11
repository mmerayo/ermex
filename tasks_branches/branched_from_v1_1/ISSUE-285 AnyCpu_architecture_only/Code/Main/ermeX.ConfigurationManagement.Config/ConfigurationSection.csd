<?xml version="1.0" encoding="utf-8"?>
<configurationSectionModel xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="d0ed9acb-0435-4532-afdd-b5115bc4d562" namespace="ermeX.Configuration" xmlSchemaNamespace="ermeX.Configuration" xmlns="http://schemas.microsoft.com/dsltools/ConfigurationSectionDesigner">
  <typeDefinitions>
    <externalType name="String" namespace="System" />
    <externalType name="Boolean" namespace="System" />
    <externalType name="Int32" namespace="System" />
    <externalType name="Int64" namespace="System" />
    <externalType name="Single" namespace="System" />
    <externalType name="Double" namespace="System" />
    <externalType name="DateTime" namespace="System" />
    <externalType name="TimeSpan" namespace="System" />
    <externalType name="Guid" namespace="System" />
    <enumeratedType name="DbType" namespace="ermeX.Configuration">
      <literals>
        <enumerationLiteral name="InMemory" />
        <enumerationLiteral name="SQLite" />
        <enumerationLiteral name="SqlServer" />
      </literals>
    </enumeratedType>
  </typeDefinitions>
  <configurationElements>
    <configurationSection name="ermeXConfiguration" codeGenOptions="Singleton, XmlnsProperty" xmlSectionName="ermeXConfiguration">
      <elementProperties>
        <elementProperty name="ComponentDefinition" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="componentDefinition" isReadOnly="false">
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/LocalComponent" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationSection>
    <configurationElement name="LocalComponent" documentation="the configuration of the local component">
      <attributeProperties>
        <attributeProperty name="ComponentId" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="componentId" isReadOnly="false" documentation="The unique identifier of the ermeX component in the ermeX network">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Guid" />
          </type>
        </attributeProperty>
        <attributeProperty name="TcpPort" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="tcpPort" isReadOnly="false" documentation="TCP port the components listens to" defaultValue="8135">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="MessagesExpirationDays" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="messagesExpirationDays" isReadOnly="false" documentation="the days after a message is expired. " defaultValue="31">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="DiscoverSubscriptors" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="discoverSubscriptors" isReadOnly="false" documentation="indicates if the message subscriptor types should be discovered when the component is started" defaultValue="true">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Boolean" />
          </type>
        </attributeProperty>
        <attributeProperty name="DiscoverServices" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="discoverServices" isReadOnly="false" documentation="indicates if the services should be discovered when the component is started" defaultValue="true">
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
        <elementProperty name="Database" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="database" isReadOnly="false">
          <validator>
            <callbackValidatorMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Database" />
          </validator>
          <type>
            <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Database" />
          </type>
        </elementProperty>
      </elementProperties>
    </configurationElement>
    <configurationElement name="FriendComponentElement" documentation="the component to request join network. Only one is needed">
      <attributeProperties>
        <attributeProperty name="RemoteIp" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="remoteIp" isReadOnly="false" documentation="the remote component IP">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
        <attributeProperty name="RemoteTcpPort" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="remoteTcpPort" isReadOnly="false" documentation="the remote component TcpPort">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Int32" />
          </type>
        </attributeProperty>
        <attributeProperty name="RemoteComponentId" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="remoteComponentId" isReadOnly="false" documentation="The remote component id in the ermeX Network">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Guid" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="Database">
      <attributeProperties>
        <attributeProperty name="DbType" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="dbType" isReadOnly="false" documentation="the db type. Values: InMemory, SQLite, SqlServer" defaultValue="DbType.InMemory">
          <type>
            <enumeratedTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DbType" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="PhisicalDatabase">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Database" />
      </baseClass>
      <attributeProperties>
        <attributeProperty name="ConnectionString" isRequired="true" isKey="false" isDefaultCollection="false" xmlName="connectionString" isReadOnly="false" documentation="the connectionstring of the database">
          <type>
            <externalTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/String" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="SqlServerDatabase">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/PhisicalDatabase" />
      </baseClass>
      <attributeProperties>
        <attributeProperty name="DbType" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dbType" isReadOnly="false" defaultValue="DbType.SqlServer">
          <type>
            <enumeratedTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DbType" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="SqliteDatabase">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/PhisicalDatabase" />
      </baseClass>
      <attributeProperties>
        <attributeProperty name="DbType" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dbType" isReadOnly="false" defaultValue="DbType.SQLite">
          <type>
            <enumeratedTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DbType" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
    <configurationElement name="InMemoryDatabase">
      <baseClass>
        <configurationElementMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/Database" />
      </baseClass>
      <attributeProperties>
        <attributeProperty name="DbType" isRequired="false" isKey="false" isDefaultCollection="false" xmlName="dbType" isReadOnly="false" defaultValue="DbType.InMemory">
          <type>
            <enumeratedTypeMoniker name="/d0ed9acb-0435-4532-afdd-b5115bc4d562/DbType" />
          </type>
        </attributeProperty>
      </attributeProperties>
    </configurationElement>
  </configurationElements>
  <propertyValidators>
    <validators>
      <callbackValidator name="Database" callback="Validate" />
    </validators>
  </propertyValidators>
</configurationSectionModel>