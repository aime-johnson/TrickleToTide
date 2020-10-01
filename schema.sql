/****** Object:  Table [ttt].[Position]    Script Date: 01/10/2020 12:27:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ttt].[Position](
	[Id] [uniqueidentifier] NOT NULL,
	[Nickname] [varchar](max) NULL,
	[Timestamp] [datetime] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Altitude] [float] NOT NULL,
	[Heading] [float] NOT NULL,
	[Speed] [float] NOT NULL,
	[Accuracy] [float] NOT NULL,
	[Category] [varchar](50) NULL,
 CONSTRAINT [PK_Position] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER AUTHORIZATION ON [ttt].[Position] TO  SCHEMA OWNER 
GO


/****** Object:  Table [ttt].[PositionHistory]    Script Date: 01/10/2020 12:27:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [ttt].[PositionHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PositionId] [uniqueidentifier] NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Latitude] [float] NOT NULL,
	[Longitude] [float] NOT NULL,
	[Altitude] [float] NOT NULL,
	[Heading] [float] NOT NULL,
	[Speed] [float] NOT NULL,
	[Accuracy] [float] NOT NULL,
 CONSTRAINT [PK_PositionHistory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER AUTHORIZATION ON [ttt].[PositionHistory] TO  SCHEMA OWNER 
GO

ALTER TABLE [ttt].[PositionHistory]  WITH CHECK ADD  CONSTRAINT [FK_PositionHistory_Position] FOREIGN KEY([PositionId])
REFERENCES [ttt].[Position] ([Id])
ON DELETE CASCADE
GO

ALTER TABLE [ttt].[PositionHistory] CHECK CONSTRAINT [FK_PositionHistory_Position]
GO

