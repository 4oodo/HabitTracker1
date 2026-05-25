-- Скрипт создания структуры базы данных HabitTrackerDb1
-- Сервер: DESKTOP-N513RVN
-- Дата создания: 2024

-- Создание таблицы Users (Пользователи)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Users]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Users]
	(
		[UserId] INT PRIMARY KEY IDENTITY(1,1),
		[Username] NVARCHAR(50) NOT NULL UNIQUE,
		[PasswordHash] NVARCHAR(MAX) NOT NULL,
		[Email] NVARCHAR(100),
		[CreatedDate] DATETIME NOT NULL,
		[LastLoginDate] DATETIME
	)
END

-- Создание таблицы Categories (Категории)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Categories]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Categories]
	(
		[CategoryId] INT PRIMARY KEY IDENTITY(1,1),
		[Name] NVARCHAR(100) NOT NULL,
		[UserId] INT NOT NULL,
		[CreatedDate] DATETIME NOT NULL,
		CONSTRAINT FK_Categories_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE
	)
END

-- Создание таблицы Habits (Привычки)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[Habits]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[Habits]
	(
		[HabitId] INT PRIMARY KEY IDENTITY(1,1),
		[Name] NVARCHAR(200) NOT NULL,
		[Description] NVARCHAR(500),
		[UserId] INT NOT NULL,
		[CategoryId] INT,
		[Frequency] INT NOT NULL DEFAULT 0, -- 0=Daily, 1=Weekdays, 2=SpecificDays, 3=EveryNDays, 4=Weekly
		[DaysOfWeek] NVARCHAR(100), -- Comma-separated or JSON
		[FrequencyDays] INT DEFAULT 1,
		[Unit] NVARCHAR(50), -- Единица измерения (раз, минуты, км, страницы)
		[TargetValue] DECIMAL(10,2),
		[ReminderTime] TIME,
		[IsActive] BIT DEFAULT 1,
		[CreatedDate] DATETIME NOT NULL,
		[DeletedDate] DATETIME,
		CONSTRAINT FK_Habits_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE CASCADE,
		CONSTRAINT FK_Habits_Categories FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories]([CategoryId])
	)
END

-- Создание таблицы HabitLogs (Логи выполнения)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE id = object_id(N'[dbo].[HabitLogs]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN
	CREATE TABLE [dbo].[HabitLogs]
	(
		[LogId] INT PRIMARY KEY IDENTITY(1,1),
		[HabitId] INT NOT NULL,
		[UserId] INT NOT NULL,
		[LogDate] DATETIME NOT NULL,
		[IsCompleted] BIT NOT NULL,
		[Value] DECIMAL(10,2), -- Количество (минут, км, страниц и т.д.)
		[Note] NVARCHAR(500),
		[CreatedDate] DATETIME NOT NULL,
		CONSTRAINT FK_HabitLogs_Habits FOREIGN KEY ([HabitId]) REFERENCES [dbo].[Habits]([HabitId]) ON DELETE CASCADE,
		CONSTRAINT FK_HabitLogs_Users FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users]([UserId]) ON DELETE NO ACTION
	)
END

-- Создание индексов для оптимизации
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Users_Username')
	CREATE INDEX [IX_Users_Username] ON [dbo].[Users]([Username])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Categories_UserId')
	CREATE INDEX [IX_Categories_UserId] ON [dbo].[Categories]([UserId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Habits_UserId')
	CREATE INDEX [IX_Habits_UserId] ON [dbo].[Habits]([UserId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_Habits_CategoryId')
	CREATE INDEX [IX_Habits_CategoryId] ON [dbo].[Habits]([CategoryId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_HabitLogs_HabitId')
	CREATE INDEX [IX_HabitLogs_HabitId] ON [dbo].[HabitLogs]([HabitId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_HabitLogs_UserId')
	CREATE INDEX [IX_HabitLogs_UserId] ON [dbo].[HabitLogs]([UserId])

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = N'IX_HabitLogs_LogDate')
	CREATE INDEX [IX_HabitLogs_LogDate] ON [dbo].[HabitLogs]([LogDate])

-- Вставка категорий по умолчанию (для первого пользователя)
-- Это можно сделать позже или вручную через приложение

PRINT 'Структура базы данных HabitTrackerDb1 создана успешно!'
