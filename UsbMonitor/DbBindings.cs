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

    public class LogEntry {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime When { get; set; }
        public string SerialNumber { get; set; }
    }

    public class UsbStateEntry : LogEntry {
        public USB_STATE State { get; set; }
        public string DriveLetter { get; set; }
    }

    public class FSWatcherEntry : LogEntry {
        public FILE_STATE State { get; set; }
        public string OldPath { get; set; }
        public string Path { get; set; }
    }

    public class RegisteredUsbEntry {
        public int Id { get; set; }
        public string Username { get; set; }
        public string UsbSerial { get; set; }
    }

    public class LogDatabase : DbContext {
        public LogDatabase() : base(new SQLiteConnection() { ConnectionString =
            new SQLiteConnectionStringBuilder() {
                //DataSource = @"D:\disk\Projects\UsbMonitor\UsbMonitor.db",
                DataSource = Path.Combine(Path.GetDirectoryName(
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8)), "UsbMonitor.db"),
        ForeignKeys = true }.ConnectionString}, true) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            var sqliteConnectionInitializer = new SqliteCreateDatabaseIfNotExists<LogDatabase>(modelBuilder);
            Database.SetInitializer(sqliteConnectionInitializer);
        }

        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<RegisteredUsbEntry> RegisteredUsbs { get; set; }
    }
}
