using SQLite.CodeFirst;
using System;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;

namespace UsbMonitor {
    public enum USB_STATE {
        INSERTED,
        REMOVED,
        REJECTED
    }

    public enum FILE_STATE {
        CREATED,
        RENAMED,
        CHANGED,
        REMOVED,
    }

    /// <summary>
    /// Базовый класс логов (остальные будут от него наследоваться)
    /// </summary>
    public class LogEntry {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime When { get; set; }
        public string SerialNumber { get; set; }
    }

    /// <summary>
    /// Класс-описание для логгирования вставки/удаления usb
    /// </summary>
    public class UsbStateEntry : LogEntry {
        public USB_STATE State { get; set; }
        public string DriveLetter { get; set; }
    }

    /// <summary>
    /// Класс-описание для логгирования операций над файлами
    /// </summary>
    public class FSWatcherEntry : LogEntry {
        public FILE_STATE State { get; set; }
        public string OldPath { get; set; }
        public string Path { get; set; }
    }

    /// <summary>
    /// Класс-описание зарегистрированных пользователей
    /// </summary>
    public class RegisteredUser {
        public int Id { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }

        public override string ToString() {
            return string.Format("{0} {1}", FirstName, SecondName);
        }
    }

    /// <summary>
    /// Класс-описание допустимых usb
    /// </summary>
    public class RegisteredUsbEntry {
        public int Id { get; set; }
        public RegisteredUser User { get; set; }
        public string UsbSerial { get; set; }
    }

    /// <summary>
    /// Основной класс, который представляет будущую базу данных
    /// </summary>
    public class LogDatabase : DbContext {
        public LogDatabase() : base(new SQLiteConnection() { ConnectionString =
            new SQLiteConnectionStringBuilder() {
                // Магическая строчка, которая получает текущую директорию, в которой был запущен наш процесс
                // и добавляет имя базы в конце
                DataSource = Path.Combine(Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8)), "UsbMonitor.db"),
        ForeignKeys = true }.ConnectionString}, true) { }

        // Entity Framework не позволяет создавать базы данных с нуля (если файла на диске нет)
        // Поможем ему с этим с помощью библиотеки SQLite.CodeFirst
        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<LogDatabase>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        // Будущие таблицы в базе данных
        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<RegisteredUsbEntry> RegisteredUsbs { get; set; }
        public DbSet<RegisteredUser> RegisteredUsers { get; set; }
    }
}
