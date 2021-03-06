USE [AppointmentBooking]
GO
IF EXISTS(SELECT 1 FROM sys.objects WHERE name='FK_Room_Floor' AND OBJECT_NAME(parent_object_id)='Room')
BEGIN
	ALTER TABLE [dbo].[Room] DROP CONSTRAINT [FK_Room_Floor]
END
GO
IF EXISTS(SELECT 1 FROM sys.objects WHERE name='FK_Recurrence_BookedMeeting' AND OBJECT_NAME(parent_object_id)='Recurrence')
BEGIN
	ALTER TABLE [dbo].[Recurrence] DROP CONSTRAINT [FK_Recurrence_BookedMeeting]
END
GO

IF EXISTS(SELECT 1 FROM sys.objects WHERE name='FK_BookedMeeting_Room' AND OBJECT_NAME(parent_object_id)='Recurrence')
BEGIN
	ALTER TABLE [dbo].[Recurrence] DROP CONSTRAINT [FK_BookedMeeting_Room]
END
GO
/****** Object:  Table [dbo].[BookedMeeting]    Script Date: 29-05-2018 16:09:11 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE name='Recurrence' AND type_desc='USER_TABLE')
BEGIN
	DROP TABLE [dbo].[Recurrence]
END
GO

/****** Object:  Table [dbo].[BookedMeeting]    Script Date: 29-05-2018 16:09:11 ******/
IF EXISTS (SELECT * FROM sys.objects WHERE name='BookedMeeting' AND type_desc='USER_TABLE')
BEGIN
	DROP TABLE [dbo].[BookedMeeting]
END
GO

/****** Object:  Table [dbo].[Room]    Script Date: 29-05-2018 16:09:11 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name='Room' AND type_desc='USER_TABLE')
BEGIN
	DROP TABLE [dbo].[Room]
END
GO
/****** Object:  Table [dbo].[FloorList]    Script Date: 29-05-2018 16:09:11 ******/
IF EXISTS (SELECT 1 FROM sys.objects WHERE name='FloorList' AND type_desc='USER_TABLE')
BEGIN
	DROP TABLE [dbo].[FloorList]
END
GO

GO
/****** Object:  Table [dbo].[BookedMeeting]    Script Date: 29-05-2018 16:09:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookedMeeting](
	[ID] [int]  IDENTITY(1,1) NOT NULL,
	[UserID] [NVARCHAR](50) NULL,
	[StartDateTime] [datetime] NULL,
	[EndDateTime] [datetime] NULL,
	[Description] [nvarchar](200) NULL,
	[IsConfirmed] BIT CONSTRAINT DEF_BookedMeeting_IsConfirmed DEFAULT (0), 	
 CONSTRAINT [PK_BookedMeeting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY]

GO

/****** Object:  Table [dbo].[Recurrence]    Script Date: 29-05-2018 16:09:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Recurrence](
	[ID] [int]  IDENTITY(1,1) NOT NULL,
	[RoomID] [int] NULL,
	[BookedMeetingID] [int] NULL,
	[StartDateTime] [datetime] NULL,
	[EndDateTime] [datetime] NULL,
	[IsConfirmed] BIT CONSTRAINT DEF_Recurrence_IsConfirmed DEFAULT (0),	
	 	
 CONSTRAINT [PK_Recurrence] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

) ON [PRIMARY]

GO

ALTER TABLE [dbo].[Recurrence]  WITH CHECK ADD  CONSTRAINT [FK_Recurrence_BookedMeeting] FOREIGN KEY([BookedMeetingID])
REFERENCES [dbo].[BookedMeeting] ([ID])
GO




/****** Object:  Table [dbo].[FloorList]    Script Date: 29-05-2018 16:09:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FloorList](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[FloorName] [nvarchar](50) NULL,
 CONSTRAINT [PK_Floor] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
/****** Object:  Table [dbo].[Room]    Script Date: 29-05-2018 16:09:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Room](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[RoomName] [nvarchar](50) NULL,
	[FloorID] [int] NULL,
	[RoomEmail] [nvarchar](100) NULL,
	[Capacity] [INT],
 CONSTRAINT [PK_Room] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
ALTER TABLE [dbo].[Recurrence]  WITH CHECK ADD  CONSTRAINT [FK_Recurrence_Room] FOREIGN KEY([RoomID])
REFERENCES [dbo].[Room] ([ID])
GO
ALTER TABLE [dbo].[Room]  WITH CHECK ADD  CONSTRAINT [FK_Room_Floor] FOREIGN KEY([FloorID])
REFERENCES [dbo].[FloorList] ([ID])
GO
INSERT [dbo].[FloorList] ([FloorName]) VALUES (N'All')
GO
INSERT [dbo].[FloorList] ([FloorName]) VALUES (N'First Floor')
GO
INSERT [dbo].[FloorList] ([FloorName]) VALUES (N'Third Floor')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Big Bang Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaBigBangMeetingRoom@civica.co.uk',1)
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Black Hole Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaBlackHoleMeetingRoom@civica.co.uk',2)

INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Nebula Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaNebulaMeetingRoom@civica.co.uk',3)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Shooting Star Meeting room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaShootingStarMeetingRoom@civica.co.uk',4)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Super Nova Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaSuperNovaMeetingRoom@civica.co.uk',5)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Aqua Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaA@sfwltd.co.uk',6)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Brain Wave Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaB@sfwltd.co.uk',7)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Footprint Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaF@sfwltd.co.uk',8)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES (N'Baroda Season Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaS2@sfwltd.co.uk',9)
GO													   
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail],[Capacity]) VALUES ( N'Baroda Splash Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaS@sfwltd.co.uk',10)
 

