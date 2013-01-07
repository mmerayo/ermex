CREATE SCHEMA [ClientComponent] AUTHORIZATION [dbo]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].[OutgoingMessages](
	[OutgoingMessages_Id] [int] IDENTITY(1,1) NOT NULL,
	[OutgoingMessages_PublishedBy] [uniqueidentifier] NOT NULL,
	[OutgoingMessages_PublishedTo] [uniqueidentifier] NOT NULL,
	[OutgoingMessages_BusMessageId] [int] NOT NULL,
	[OutgoingMessages_TimePublishedUtc] [bigint] NOT NULL,
	[OutgoingMessages_Tries] [int] NOT NULL,
	[OutgoingMessages_Failed] [bit] NOT NULL,
	[OutgoingMessages_Version] [bigint] NOT NULL,
	[OutgoingMessages_ComponentOwner] [uniqueidentifier] NOT NULL,
	[OutgoingMessages_Delivering] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OutgoingMessages_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].[OutgoingMessageSuscriptions](
	[OutgoingMessageSuscriptions_Id] [int] IDENTITY(1,1) NOT NULL,
	[OutgoingMessageSuscriptions_BizMessageFullTypeName] [nvarchar](256) NOT NULL,
	[OutgoingMessageSuscriptions_DateLastUpdateUtc] [bigint] NOT NULL,
	[OutgoingMessageSuscriptions_Version] [bigint] NOT NULL,
	[OutgoingMessageSuscriptions_ComponentId] [uniqueidentifier] NOT NULL,
	[OutgoingMessageSuscriptions_ComponentOwner] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[OutgoingMessageSuscriptions_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_OutgoingMessageSuscriptions] ON [ClientComponent].[OutgoingMessageSuscriptions] 
(
	[OutgoingMessageSuscriptions_ComponentOwner] ASC,
	[OutgoingMessageSuscriptions_ComponentId] ASC,
	[OutgoingMessageSuscriptions_BizMessageFullTypeName] ASC
	
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].[ConnectivityDetails](
	[ConnectivityDetails_Id] [int] IDENTITY(1,1) NOT NULL,
	[ConnectivityDetails_Ip] [nvarchar](1024) NOT NULL,
	[ConnectivityDetails_Port] [int] NOT NULL,
	[ConnectivityDetails_IsLocal] [bit] NOT NULL,
	[ConnectivityDetails_ComponentOwner] [uniqueidentifier] NOT NULL,
	[ConnectivityDetails_Version] [bigint] NOT NULL,
	[ConnectivityDetails_ServerId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK__Connecti__8838777D1AF3F935] PRIMARY KEY CLUSTERED 
(
	[ConnectivityDetails_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].[Components](
	[Components_Id] [int] IDENTITY(1,1) NOT NULL,
	[Components_ComponentId] [uniqueidentifier] NOT NULL,
	[Components_ComponentOwner] [uniqueidentifier] NOT NULL,
	[Components_Version] [bigint] NOT NULL,
	[Components_Latency] [int] NOT NULL,
	[Components_IsRunning] [bit] NOT NULL,
	[Components_ExchangedDefinitions] [bit] NOT NULL,
	[Components_ComponentExchanges] [uniqueidentifier] NULL
PRIMARY KEY CLUSTERED 
(
	[Components_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UQ__Componen__66FAE64429F972FF] UNIQUE NONCLUSTERED 
(
	[Components_ComponentId] ASC,
	[Components_ComponentOwner] ASC
	
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].[IncomingMessageSuscriptions](
	[IncomingMessageSuscriptions_Id] [int] IDENTITY(1,1) NOT NULL,
	[IncomingMessageSuscriptions_BizMessageFullTypeName] [nvarchar](256) NOT NULL,
	[IncomingMessageSuscriptions_DateLastUpdateUtc] [bigint] NOT NULL,
	[IncomingMessageSuscriptions_ComponentOwner] [uniqueidentifier] NOT NULL,
	[IncomingMessageSuscriptions_Version] [bigint] NOT NULL,
	[IncomingMessageSuscriptions_SuscriptionHandlerId] [uniqueidentifier] NOT NULL,
	[IncomingMessageSuscriptions_HandlerType] [nvarchar](1024) NOT NULL
PRIMARY KEY CLUSTERED 
(
	[IncomingMessageSuscriptions_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_IncomingMessageSuscriptions] ON [ClientComponent].[IncomingMessageSuscriptions] 
(
	[IncomingMessageSuscriptions_BizMessageFullTypeName] ASC,
	[IncomingMessageSuscriptions_SuscriptionHandlerId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].[IncomingMessages](
	[IncomingMessages_Id] [int] IDENTITY(1,1) NOT NULL,
	[IncomingMessages_PublishedBy] [uniqueidentifier] NOT NULL,
	[IncomingMessages_PublishedTo] [uniqueidentifier] NOT NULL,
	[IncomingMessages_BusMessageId] [int] NOT NULL,
	[IncomingMessages_TimePublishedUtc] [bigint] NOT NULL,	
	[IncomingMessages_TimeReceivedUtc] [bigint] NOT NULL,
	[IncomingMessages_ComponentOwner] [uniqueidentifier] NOT NULL,
	[IncomingMessages_Version] [bigint] NOT NULL,
	[IncomingMessages_SuscriptionHandlerId] [uniqueidentifier] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[IncomingMessages_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [ClientComponent].ServicesDetails(
	[ServicesDetails_Id] [int] IDENTITY(1,1) NOT NULL,
	[ServicesDetails_ServiceImplementationMethodName] [nvarchar](1024) NOT NULL,
	[ServicesDetails_ServiceImplementationTypeName] [nvarchar](1024)NOT NULL,
	[ServicesDetails_ServiceInterfaceTypeName] [nvarchar](1024) NOT NULL,
	[ServicesDetails_OperationIdentifier] [uniqueidentifier] NOT NULL,
	[ServicesDetails_Publisher] [uniqueidentifier] NOT NULL,
	[ServicesDetails_Version] [bigint] NOT NULL,
	[ServicesDetails_ComponentOwner] [uniqueidentifier] NOT NULL,
	[ServicesDetails_IsSystemService] [bit] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ServicesDetails_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE UNIQUE NONCLUSTERED INDEX [idx_uniqueoperation] ON [ClientComponent].[ServicesDetails] 
(
	[ServicesDetails_OperationIdentifier] ASC,
	[ServicesDetails_Publisher] ASC,
	[ServicesDetails_ComponentOwner] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


/****** Object:  Table [ClientComponent].[BusMessages]    Script Date: 01/04/2013 14:21:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ClientComponent].[BusMessages](
	[BusMessages_Id] [int] IDENTITY(1,1) NOT NULL,

	[BusMessages_MessageId] [uniqueidentifier] NOT NULL,
	[BusMessages_CreatedTimeUtc] [bigint] NOT NULL,
	[BusMessages_Publisher] [uniqueidentifier] NOT NULL,
	[BusMessages_JsonMessage] [nvarchar](max) NOT NULL,
	[BusMessages_Version] [bigint] NOT NULL,
	[BusMessages_Status] [int] NOT NULL,
	[BusMessages_ComponentOwner] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_BusMessages] PRIMARY KEY CLUSTERED 
(
	[BusMessages_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO



/****** Object:  Table [ClientComponent].[ChunkedServiceRequestMessages]    Script Date: 01/04/2013 14:21:01 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO

CREATE TABLE [ClientComponent].[ChunkedServiceRequestMessages](
	[ChunkedServiceRequestMessages_Id] [int] IDENTITY(1,1) NOT NULL,
	[ChunkedServiceRequestMessages_Operation] [uniqueidentifier] NOT NULL,
	[ChunkedServiceRequestMessages_Data] [varbinary](max) NOT NULL,
	[ChunkedServiceRequestMessages_CorrelationId] [uniqueidentifier] NOT NULL,
	[ChunkedServiceRequestMessages_Order] [int] NOT NULL,
	[ChunkedServiceRequestMessages_Eof] [bit] NOT NULL,
	[ChunkedServiceRequestMessages_Version] [bigint] NOT NULL,
	[ChunkedServiceRequestMessages_ComponentOwner] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_ChunkedServiceRequestMessages] PRIMARY KEY CLUSTERED 
(
	[ChunkedServiceRequestMessages_Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

SET ANSI_PADDING OFF
GO


/****** Object:  Index [IX_ChunkedServiceRequestMessages]    Script Date: 01/04/2013 14:21:56 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_ChunkedServiceRequestMessages] ON [ClientComponent].[ChunkedServiceRequestMessages] 
(
	[ChunkedServiceRequestMessages_CorrelationId] ASC,
	[ChunkedServiceRequestMessages_Order] ASC,
	[ChunkedServiceRequestMessages_ComponentOwner] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO
