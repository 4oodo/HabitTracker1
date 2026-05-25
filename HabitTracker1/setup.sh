#!/bin/bash
# Script for quick setup on Unix-like systems

echo "======================================"
echo "  Habit Tracker - Setup Script"
echo "======================================"
echo ""

# For Windows users, they need to use SQL Server Management Studio
# This is a helper script for Linux/Mac users

echo "⚠️  Внимание: Для Windows используйте SQL Server Management Studio!"
echo ""
echo "Инструкции для Windows:"
echo "========================"
echo "1. Откройте файл Database/CreateDatabase.sql"
echo "2. Запустите его в SQL Server Management Studio"
echo "3. Это создаст все необходимые таблицы"
echo ""
echo "Инструкции для Linux/Mac:"
echo "========================"
echo "1. Установите sqlcmd:"
echo "   sudo apt-get install mssql-tools"  
echo ""
echo "2. Выполните SQL скрипт:"
echo "   sqlcmd -S DESKTOP-N513RVN -d HabitTrackerDb1 -i Database/CreateDatabase.sql"
echo ""
echo "Или для LocalDB:"
echo "   sqlcmd -S (localdb)\mssqllocaldb -d HabitTrackerDb1 -i Database/CreateDatabase.sql"
echo ""

echo "✅ Если подключение успешно, база готова к работе!"
echo ""
echo "Запуск приложения в Visual Studio:"
echo "===================================="
echo "1. Откройте HabitTracker1.slnx"
echo "2. Нажмите F5"
echo "3. Создайте аккаунт"
echo "4. Начните добавлять привычки!"
echo ""
