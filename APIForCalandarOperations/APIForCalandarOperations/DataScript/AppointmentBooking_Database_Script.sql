USE [AppointmentBooking]
GO
ALTER TABLE [dbo].[Room] DROP CONSTRAINT [FK_Room_Floor]
GO
ALTER TABLE [dbo].[BookedMeeting] DROP CONSTRAINT [FK_BookedMeeting_Room]
GO
/****** Object:  Table [dbo].[Room]    Script Date: 29-05-2018 16:09:11 ******/
DROP TABLE [dbo].[Room]
GO
/****** Object:  Table [dbo].[FloorList]    Script Date: 29-05-2018 16:09:11 ******/
DROP TABLE [dbo].[FloorList]
GO
/****** Object:  Table [dbo].[BookedMeeting]    Script Date: 29-05-2018 16:09:11 ******/
DROP TABLE [dbo].[BookedMeeting]
GO
/****** Object:  Table [dbo].[BookedMeeting]    Script Date: 29-05-2018 16:09:11 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookedMeeting](
	[ID] [int]  IDENTITY(1,1) NOT NULL,
	[RoomID] [int] NULL,
	[UserID] [int] NULL,
	[StartDateTime] [datetime] NULL,
	[EndDateTime] [datetime] NULL,
	[Description] [nvarchar](200) NULL,
 CONSTRAINT [PK_BookedMeeting] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

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
 CONSTRAINT [PK_Room] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
INSERT [dbo].[FloorList] ([FloorName]) VALUES (N'All')
GO
INSERT [dbo].[FloorList] ([FloorName]) VALUES (N'First Floor')
GO
INSERT [dbo].[FloorList] ([FloorName]) VALUES (N'Third Floor')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Big Bang Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaBigBangMeetingRoom@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Black Hole Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaBlackHoleMeetingRoom@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Nebula Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaNebulaMeetingRoom@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Shooting Star Meeting room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaShootingStarMeetingRoom@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Super Nova Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='First Floor'), N'BarodaSuperNovaMeetingRoom@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Aqua Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaA@sfwltd.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Brain Wave Meeting Room',  (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaB@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Footprint Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaF@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES (N'Baroda Season Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaS2@civica.co.uk')
GO
INSERT [dbo].[Room] ([RoomName], [FloorID], [RoomEmail]) VALUES ( N'Baroda Splash Meeting Room', (SELECT ID FROM FloorList WHERE FloorName='Third Floor'), N'BarodaS@civica.co.uk')
GO

ALTER TABLE [dbo].[BookedMeeting]  WITH CHECK ADD  CONSTRAINT [FK_BookedMeeting_Room] FOREIGN KEY([RoomID])
REFERENCES [dbo].[Room] ([ID])
GO
ALTER TABLE [dbo].[BookedMeeting] CHECK CONSTRAINT [FK_BookedMeeting_Room]
GO
ALTER TABLE [dbo].[Room]  WITH CHECK ADD  CONSTRAINT [FK_Room_Floor] FOREIGN KEY([FloorID])
REFERENCES [dbo].[FloorList] ([ID])
GO
ALTER TABLE [dbo].[Room] CHECK CONSTRAINT [FK_Room_Floor]
GO
