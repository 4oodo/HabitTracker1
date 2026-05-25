@echo off
REM ============================================
REM Habit Tracker - Windows Setup Script
REM ============================================

color 0A
cls

echo.
echo ======================================
echo    Habit Tracker - Setup Windows
echo ======================================
echo.

echo [INFO] Проверка наличия SQL Server...
echo.

REM Check if sqlcmd is available
where sqlcmd >nul 2>nul
if %ERRORLEVEL% NEQ 0 (
	echo [ERROR] sqlcmd не найден!
	echo.
	echo Пожалуйста, используйте SQL Server Management Studio:
	echo 1. Откройте файл: Database\CreateDatabase.sql
	echo 2. Подключитесь к серверу: DESKTOP-N513RVN
	echo 3. Выполните скрипт (Ctrl+E)
	echo.
	pause
	exit /b 1
)

echo [OK] sqlcmd найден!
echo.

REM Get server name
set /p SERVER="Введите имя сервера (или нажмите Enter для DESKTOP-N513RVN): "
if "%SERVER%"=="" (
	set SERVER=DESKTOP-N513RVN
)

echo [INFO] Подключение к серверу: %SERVER%
echo [INFO] База данных: HabitTrackerDb1
echo.

REM Execute SQL script
echo [INFO] Выполнение скрипта создания БД...
sqlcmd -S %SERVER% -d HabitTrackerDb1 -i Database\CreateDatabase.sql

if %ERRORLEVEL% EQU 0 (
	echo.
	echo [SUCCESS] База данных успешно создана!
	echo.
	echo Следующие шаги:
	echo 1. Откройте HabitTracker1.slnx в Visual Studio
	echo 2. Нажмите F5 для запуска приложения
	echo 3. Создайте новый аккаунт
	echo 4. Начните добавлять привычки!
	echo.
	pause
) else (
	echo.
	echo [ERROR] Ошибка при создании БД!
	echo.
	echo Проверьте:
	echo - SQL Server запущен на сервере %SERVER%
	echo - База данных HabitTrackerDb1 существует
	echo - У вас есть права администратора
	echo.
	pause
	exit /b 1
)

endlocal
