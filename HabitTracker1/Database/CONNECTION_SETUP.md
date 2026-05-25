# Инструкция по настройке подключения к БД

## Текущие параметры подключения
- **Сервер**: DESKTOP-N513RVN
- **База данных**: HabitTrackerDb1
- **Аутентификация**: Windows Integrated Security

## Расположение строки подключения
Строка подключения находится в файле: `Data/DatabaseConnection.cs`

```csharp
private readonly string _connectionString = "Server=DESKTOP-N513RVN;Database=HabitTrackerDb1;Integrated Security=true;";
```

## Если нужно изменить параметры подключения

### 1. Если сервер находится на другой машине
Замените `DESKTOP-N513RVN` на имя вашего сервера:
```
Server=YOUR_SERVER_NAME;Database=HabitTrackerDb1;Integrated Security=true;
```

### 2. Если используется аутентификация через логин/пароль
Замените на:
```
Server=YOUR_SERVER_NAME;Database=HabitTrackerDb1;User Id=sa;Password=YOUR_PASSWORD;
```

### 3. Если используется LocalDB
```
Server=(localdb)\mssqllocaldb;Database=HabitTrackerDb1;Integrated Security=true;
```

### 4. Если используется Azure SQL
```
Server=YOUR_SERVER.database.windows.net;Database=HabitTrackerDb1;User Id=sa;Password=YOUR_PASSWORD;
```

## Проверка подключения

Используйте этот код для проверки подключения:
```csharp
var dbConnection = new DatabaseConnection();
bool isConnected = dbConnection.TestConnection();
if (isConnected)
	MessageBox.Show("Подключение успешно!");
else
	MessageBox.Show("Ошибка подключения!");
```

## Часто встречаемые ошибки

### "Named Pipes Provider, error: 40 - Could not open a connection"
**Причина**: Сервер недоступен или неправильное имя сервера
**Решение**: 
1. Проверьте имя сервера в SQL Server Management Studio
2. Убедитесь, что SQL Server Service запущен
3. Проверьте наличие пинга до сервера (если это удаленный сервер)

### "Cannot open database"
**Причина**: База данных не существует
**Решение**: 
1. Создайте базу данных HabitTrackerDb1
2. Выполните скрипт CreateDatabase.sql

### "Login failed for user"
**Причина**: Ошибка аутентификации
**Решение**: 
1. Проверьте корректность User Id и Password
2. Убедитесь, что пользователь имеет права на базу данных

### "Timeout expired"
**Причина**: Слишком долгое выполнение запроса
**Решение**: 
1. Увеличьте Connection Timeout в строке подключения
2. Проверьте производительность сервера БД
3. Проверьте индексы в БД

## Дополнительные параметры подключения

Вы можете добавить следующие параметры:
```
Server=DESKTOP-N513RVN;Database=HabitTrackerDb1;Integrated Security=true;
Connection Timeout=30;
Encrypt=false;
TrustServerCertificate=true;
```

- **Connection Timeout**: Время ожидания подключения (секунды)
- **Encrypt**: Шифрование соединения
- **TrustServerCertificate**: Доверие сертификату сервера

## Резервная копия БД

Для создания резервной копии:
```sql
BACKUP DATABASE [HabitTrackerDb1]
TO DISK = 'C:\Backups\HabitTrackerDb1.bak'
WITH INIT, COMPRESSION;
```

## Восстановление из резервной копии
```sql
RESTORE DATABASE [HabitTrackerDb1]
FROM DISK = 'C:\Backups\HabitTrackerDb1.bak'
WITH REPLACE;
```

---
**Дата создания**: Декабрь 2024
