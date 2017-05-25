using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UsbEject.Library;

namespace UsbMonitor {
    public partial class UsbMonitor : ServiceBase {
        private ManagementEventWatcher _mewCreation;

        // Храним все созданные FileSystemWatcher'ы
        private Dictionary<string, FileSystemWatcher> _fswMap = new Dictionary<string, FileSystemWatcher>();

        // Кэш буква_диск->серийник для подлюченных usb
        private Dictionary<string, string> _letterToSerialMap = new Dictionary<string, string>();

        private Dictionary<string, DeviceInformation> _letterToDevInfoMap = new Dictionary<string, DeviceInformation>();
        private LogDatabase _db;
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private Thread _dbSaverThread;
        private IntPtr _deviceEventHandle;
        private Win32.ServiceControlHandlerEx _serviceCallback;
        private readonly string _dbPath = Path.Combine(Path.GetDirectoryName(
            System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8)), "UsbMonitor.db");

        public UsbMonitor() {
            InitializeComponent();
        }

        /// <summary>
        /// Вызывается при запуске сервиса
        /// </summary>
        protected override void OnStart(string[] args) {
            base.OnStart(args);

            // Регистрируем наш обработчик событий сервиса
            RegisterDeviceNotification();

            _db = new LogDatabase();
            // Если файл не существует на диске, то просим базу данных его создать и даём администраторам доступ к файлу
            if (!File.Exists(_dbPath)) {
                _db.Database.Initialize(false);
                GrantAccess(_dbPath);
            }

            // Отдельный поток для сохранения базы данных каждые 5 секунд
            _dbSaverThread = new Thread(DbSaver);

            // Подписываемся на события вставки/удаления usb
            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_VolumeChangeEvent WHERE EventType = 2 OR EventType = 3");
            _mewCreation = new ManagementEventWatcher(query);
            _mewCreation.EventArrived += new EventArrivedEventHandler(USBEventArrived_Creation);

            // Получаем список дисков, которые уже подключены при запуске сервиса
            var usbDisks = GetLogicalUsbDisks();
            if (usbDisks == null) {
                Console.WriteLine("Cannot get usb disks, stopped.");
                Stop();
                return;
            }

            // И проверяем допустимы ли они (если нет, то отключаем их)
            foreach(var item in usbDisks) {
                if (item["caption"] == null) {
                    continue;
                }
                CheckAttachedUsb((string)item["caption"], (string)item["VolumeSerialNumber"]);
            }

            // Запускаем подписку на события и сохранялку базы данных
            _mewCreation.Start();
            _dbSaverThread.Start();
        }

        /// <summary>
        /// Вызывается при остановке службы
        /// </summary>
        protected override void OnStop() {
            base.OnStop();
            try {
                _mewCreation.Stop();
            } catch (COMException) {
                // Already stopped, doing nothing
            }

            // Отключаем сохранялку и ждём, пока она завершиться
            _stopEvent.Set();
            if (_dbSaverThread.ThreadState == System.Threading.ThreadState.Running) {
                _dbSaverThread.Join();
            }

            // Удаляем все обработчики и открытые объекты
            Win32.UnregisterDeviceNotification(_deviceEventHandle);
            foreach(var item in _letterToDevInfoMap.Keys.ToList()) {
                UnregisterHandle(item);
            }
        }

        /// <summary>
        /// Т.к. C# не позволяет получить доступ к внутреннему ServiceControlHandler
        /// А нам нужен доступ, чтобы отловить безопасного удаления флешки, то пишем свой
        /// </summary>
        private int ServiceControlHandler(int control, int eventType, IntPtr eventData, IntPtr context) {
            if (control == Win32.SERVICE_CONTROL_STOP || control == Win32.SERVICE_CONTROL_SHUTDOWN) {
                Stop();
            } else if (control == Win32.SERVICE_CONTROL_DEVICEEVENT) {
                switch (eventType) {
                    // Запрос на безопасное удаление
                    case Win32.DBT_DEVICEQUERYREMOVE:
                        Win32.DEV_BROADCAST_HDR hdr;
                        hdr = (Win32.DEV_BROADCAST_HDR)
                            Marshal.PtrToStructure(eventData, typeof(Win32.DEV_BROADCAST_HDR));

                        // Дёргаем из структур windows необходимый нам путь к диску
                        if (hdr.dbcc_devicetype == Win32.DBT_DEVTYP_HANDLE) {
                            Win32.DEV_BROADCAST_HANDLE broadcastHandle;
                            broadcastHandle = (Win32.DEV_BROADCAST_HANDLE)
                                Marshal.PtrToStructure(eventData, typeof(Win32.DEV_BROADCAST_HANDLE));
                            string driveLetter = null;
                            foreach(var item in _letterToDevInfoMap) {
                                if (item.Value.DeviceNotifyHandle == broadcastHandle.dbch_hdevnotify) {
                                    driveLetter = item.Key;
                                    break;
                                }
                            }

                            // Удаляем все связанные с диском объекты
                            if (driveLetter != null) {
                                UnregisterHandle(driveLetter);
                            }
                        }
                        break;
                }
            }

            return 0;
        }

        /// <summary>
        /// Удаляет все открытые объекты, которые могут занимать диск (например, FileSystemWatcher)
        /// </summary>
        private void UnregisterHandle(string driveLetter) {
            DeviceInformation devInfo = null;
            if (!_letterToDevInfoMap.TryGetValue(driveLetter, out devInfo)) {
                return;
            }

            FileSystemWatcher fsw;
            _fswMap.TryGetValue(driveLetter, out fsw);
            if (fsw != null) {
                fsw.EnableRaisingEvents = false;
            }

            Win32.CloseHandle(devInfo.DeviceDirectoryHandle);
            Win32.UnregisterDeviceNotification(devInfo.DeviceNotifyHandle);
            Marshal.FreeHGlobal(devInfo.Buffer);
            _letterToDevInfoMap.Remove(driveLetter);
        }

        /// <summary>
        /// Чтобы поймать запрос на безопасное удаление, нам нужно ещё получить дескриптор диска и держать его открытым
        /// (явно указываем, что этот диск мы заняли и хотим получить уведомление, когда его захотят извлечь)
        /// </summary>
        private void RegisterForHandle(string driveLetter) {
            var devInfo = new DeviceInformation();
            Win32.DEV_BROADCAST_HANDLE deviceHandle = new Win32.DEV_BROADCAST_HANDLE();
            int size = Marshal.SizeOf(deviceHandle);
            deviceHandle.dbch_size = size;
            deviceHandle.dbch_devicetype = Win32.DBT_DEVTYP_HANDLE;
            devInfo.DeviceDirectoryHandle = CreateFileHandle(driveLetter);
            deviceHandle.dbch_handle = devInfo.DeviceDirectoryHandle;
            devInfo.Buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(deviceHandle, devInfo.Buffer, true);
            devInfo.DeviceNotifyHandle = Win32.RegisterDeviceNotification(
                ServiceHandle, devInfo.Buffer, Win32.DEVICE_NOTIFY_SERVICE_HANDLE);
            if (devInfo.DeviceNotifyHandle == IntPtr.Zero) {
                // TODO handle error
            } else {
                _letterToDevInfoMap.Add(driveLetter, devInfo);
            }
        }

        /// <summary>
        /// Функция, которая забирает у библиотеки сервиса C# управление обработчиком сервиса и передаёт его нашей функции
        /// </summary>
        private void RegisterDeviceNotification() {
            _serviceCallback = new Win32.ServiceControlHandlerEx(ServiceControlHandler);
            Win32.RegisterServiceCtrlHandlerEx(ServiceName, _serviceCallback, IntPtr.Zero);

            Win32.DEV_BROADCAST_DEVICEINTERFACE deviceInterface = new Win32.DEV_BROADCAST_DEVICEINTERFACE();
            int size = Marshal.SizeOf(deviceInterface);
            deviceInterface.dbcc_size = size;
            deviceInterface.dbcc_devicetype = Win32.DBT_DEVTYP_DEVICEINTERFACE;
            IntPtr buffer = default(IntPtr);
            buffer = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(deviceInterface, buffer, true);
            _deviceEventHandle = Win32.RegisterDeviceNotification(ServiceHandle, buffer, Win32.DEVICE_NOTIFY_SERVICE_HANDLE | Win32.DEVICE_NOTIFY_ALL_INTERFACE_CLASSES);
            if (_deviceEventHandle == IntPtr.Zero) {
                // TODO handle error
            }
        }

        /// <summary>
        /// Функция, которая открывает тот самый дескриптор для диска
        /// </summary>
        public static IntPtr CreateFileHandle(string driveLetter) {
            // open the existing file for reading
            IntPtr handle = Win32.CreateFile(
                  driveLetter,
                  Win32.GENERIC_READ,
                  Win32.FILE_SHARE_READ | Win32.FILE_SHARE_WRITE,
                  0,
                  Win32.OPEN_EXISTING,
                  Win32.FILE_FLAG_BACKUP_SEMANTICS | Win32.FILE_ATTRIBUTE_NORMAL,
                  0);

            if (handle == Win32.INVALID_HANDLE_VALUE) {
                return IntPtr.Zero;
            } else {
                return handle;
            }
        }

        /// <summary>
        /// Отвечает за присваивание правильных, безопасных прав для файла нашей базы данных
        /// </summary>
        private void GrantAccess(string path) {
            var dInfo = new DirectoryInfo(path);
            var dSecurity = dInfo.GetAccessControl();
            var admins = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            var localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            dSecurity.SetAccessRuleProtection(true, false);
            // dSecurity.SetOwner(localSystem);
            dSecurity.AddAccessRule(new FileSystemAccessRule(admins, FileSystemRights.Read | FileSystemRights.Write, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            dSecurity.AddAccessRule(new FileSystemAccessRule(localSystem, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            dInfo.SetAccessControl(dSecurity);
        }

        /// <summary>
        /// Крутимся в бесконечном цикле (пока нас не прервут) и сохраняем базу данных каждые 5 секунд, если есть изменения
        /// </summary>
        private void DbSaver() {
            while (true) {
                lock (_db.LogEntries) {
                    if (_db.ChangeTracker.HasChanges()) {
                        _db.SaveChanges();
                        Console.WriteLine("Saved db");
                    }
                }
                if (_stopEvent.WaitOne(5000)) {
                    break;
                }
            }
        }

        /// <summary>
        /// Заполняет базовую часть информации лога
        /// </summary>
        private static void FillLogEntry(LogEntry entry, string Serial) {
            entry.When = DateTime.Now;
            entry.Username = GetCurrentActiveUser();
            entry.SerialNumber = Serial;
        }

        /// <summary>
        /// Добавляет лог в базу данных
        /// Чтобы не было конфликтов с сохранялкой, сперва блокируем доступ сохранялке к базе данных
        /// </summary>
        private void AddEntryToDatabase(LogEntry entry) {
            lock(_db.LogEntries) {
                _db.LogEntries.Add(entry);
            }
        }

        /// <summary>
        /// Получает текущего пользователя компьютера (того, кто залогинен сейчас)
        /// </summary>
        public static string GetCurrentActiveUser() {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            var res = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            return res == null ? "NO_LOGGED_IN_USER" : res;
        }

        /// <summary>
        /// Добавляет объект FileSystemWatcher и запускает слежку за файлами по указанному пути
        /// </summary>
        private void AddNewFSWatcher(string path) {
            FileSystemWatcher fsw;
            try {
                fsw = new FileSystemWatcher(path);
            } catch (ArgumentException) {
                // Path is not available (e.g. usb stick was quickly unplugged)
                return;
            }

            fsw.Created += Fsw_Changed;
            fsw.Deleted += Fsw_Changed;
            fsw.Changed += Fsw_Changed;
            fsw.Renamed += Fsw_Renamed;
            fsw.IncludeSubdirectories = true;
            fsw.EnableRaisingEvents = true;
            _fswMap.Add(path, fsw);
        }

        /// <summary>
        /// Получает серийный номер usb диска
        /// </summary>
        protected string GetVolumeSerialNumberByName(string name) {
            var usbDisks = GetLogicalUsbDisks();
            if (usbDisks == null) {
                return null;
            }

            foreach(var disk in usbDisks) {
                if ((string)disk["caption"] == name) {
                    return (string)disk["VolumeSerialNumber"];
                }
            }
            return null;
        }

        /// <summary>
        /// Возвращает все доступные usb диски
        /// </summary>
        protected ManagementObject[] GetLogicalUsbDisks() {
            var res = new List<ManagementObject>();
            try {
                ManagementObjectCollection drives = new ManagementObjectSearcher(
                    "SELECT Caption, DeviceID FROM Win32_DiskDrive WHERE InterfaceType='USB'").Get();

                foreach (ManagementObject drive in drives) {
                    var partitionSearcher = new ManagementObjectSearcher(string.Format(
                        "ASSOCIATORS OF {{Win32_DiskDrive.DeviceID='{0}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition",
                        drive["DeviceID"])).Get();
                    foreach (ManagementObject partition in partitionSearcher) {
                        var diskSearcher = new ManagementObjectSearcher(string.Format(
                            "ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{0}'}} WHERE AssocClass = Win32_LogicalDiskToPartition",
                            partition["DeviceID"])).Get();
                        foreach (ManagementObject disk in diskSearcher) {
                            res.Add(disk);
                        }
                    }
                }
                return res.ToArray();
            } catch (Exception e) {
                //Console.WriteLine("error: " + e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Проверяет есть ли доступ у текущего пользователя к флешки с указанным серийным номером
        /// </summary>
        private bool IsUsbAllowed(string serial) {
            string user = GetCurrentActiveUser();
            if (_db.RegisteredUsbs.Any(i => i.User.Username == user && i.UsbSerial == serial)) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// WinApi возвращает внутренний путь к флешке, эта функция приводит его к нормальному виду (например к "C:\")
        /// </summary>
        private string PathToSerial(string path) {
            var driveLetter = Path.GetPathRoot(path);
            if (driveLetter.Last() == Path.DirectorySeparatorChar) {
                driveLetter = driveLetter.Remove(driveLetter.Length - 1);
            }
            string serial = null;
            _letterToSerialMap.TryGetValue(driveLetter, out serial);
            return serial;
        }

        /// <summary>
        /// Отключаем заданный usb (пытаемся 9 раз с перерывом в 333 мс)
        /// </summary>
        private void DetachUsb(string driveLetter) {
            var volumes = new VolumeDeviceClass();
            foreach(Volume vol in volumes.Devices) {
                if(vol.LogicalDrive == driveLetter) {
                    for (int i = 0; i < 9; ++i) {
                        try {
                            vol.Eject(false);
                            return;
                        } catch (Win32Exception) {
                            Thread.Sleep(333);
                            continue;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Проверяем есть ли у пользователя доступ к флешке и есть есть, то устанавливаем слежку за файлами
        /// Если нет, то отключаем флешку
        /// </summary>
        private void CheckAttachedUsb(string driveLetter, string serial) {
            _letterToSerialMap.Add(driveLetter, serial);
            if (IsUsbAllowed(serial)) {
                RegisterForHandle(driveLetter);
                AddNewFSWatcher(driveLetter);
            } else {
                Console.WriteLine("REJECTED for {0}", serial);
                var entry = new UsbStateEntry {
                    DriveLetter = driveLetter,
                    State = USB_STATE.REJECTED,
                    SerialNumber = serial
                };
                FillLogEntry(entry, serial);
                AddEntryToDatabase(entry);
                DetachUsb(driveLetter);
            }
        }

        /// <summary>
        /// Обработчик события вставки/удаления флешки
        /// </summary>
        private void USBEventArrived_Creation(object sender, EventArrivedEventArgs e) {
            string driveLetter = (string)e.NewEvent.Properties["DriveName"].Value;
            ushort eventType = (ushort)e.NewEvent.Properties["EventType"].Value;
            var entry = new UsbStateEntry { DriveLetter = driveLetter };
            FillLogEntry(entry, null);

            // Вставили
            if (eventType == 2) {
                Console.WriteLine("Added: " + driveLetter);
                entry.State = USB_STATE.INSERTED;
                // Пытаемся получить серийник 9 раз по 333 мс
                for (int i = 0; i < 9; ++i) {
                    entry.SerialNumber = GetVolumeSerialNumberByName(driveLetter);
                    if (entry.SerialNumber != null) {
                        break;
                    }
                    // Retry 9 times
                    Thread.Sleep(333);
                }
                CheckAttachedUsb(driveLetter, entry.SerialNumber);
                AddEntryToDatabase(entry);
            // Удалили
            } else if (eventType == 3) {
                Console.WriteLine("Removed: " + driveLetter);
                entry.State = USB_STATE.REMOVED;
                UnregisterHandle(driveLetter);
                FileSystemWatcher fsw;
                _fswMap.TryGetValue(driveLetter, out fsw);
                if (fsw != null) {
                    fsw.EnableRaisingEvents = false;
                    _fswMap.Remove(driveLetter);
                }

                string oldSerial;
                if (_letterToSerialMap.TryGetValue(driveLetter, out oldSerial)) {
                    _letterToSerialMap.Remove(driveLetter);
                    entry.SerialNumber = oldSerial;
                }
                AddEntryToDatabase(entry);
            }
        }

        /// <summary>
        /// Обработчик изменения файлов на диске (Создание/Изменение/Удаление)
        /// </summary>
        private void Fsw_Changed(object sender, FileSystemEventArgs e) {
            var entry = new FSWatcherEntry { Path = e.FullPath };

            FillLogEntry(entry, PathToSerial(e.FullPath));
            switch (e.ChangeType) {
                case WatcherChangeTypes.Created:
                    Console.WriteLine("Created: {0}", e.FullPath);
                    entry.State = FILE_STATE.CREATED;
                    break;
                case WatcherChangeTypes.Changed:
                    Console.WriteLine("Changed: {0}", e.FullPath);
                    entry.State = FILE_STATE.CHANGED;
                    break;
                case WatcherChangeTypes.Deleted:
                    Console.WriteLine("Deleted: {0}", e.FullPath);
                    entry.State = FILE_STATE.REMOVED;
                    break;
            }
            AddEntryToDatabase(entry);
        }

        /// <summary>
        /// Обработчик переименования файла на диске
        /// </summary>
        private void Fsw_Renamed(object sender, RenamedEventArgs e) {
            var entry = new FSWatcherEntry();
            FillLogEntry(entry, PathToSerial(e.FullPath));
            Console.WriteLine("Renamed {0} to {1}", e.OldFullPath, e.FullPath);
            entry.State = FILE_STATE.RENAMED;
            entry.OldPath = e.OldFullPath;
            entry.Path = e.FullPath;
            AddEntryToDatabase(entry);
        }

        // Дальше идут страшные структуры и функции для работы с WinApi
        private class DeviceInformation {
            public IntPtr DeviceDirectoryHandle;
            public IntPtr DeviceNotifyHandle;
            public IntPtr Buffer;
        }

        public class Win32 {
            public const int DEVICE_NOTIFY_SERVICE_HANDLE = 1;
            public const int DEVICE_NOTIFY_ALL_INTERFACE_CLASSES = 4;

            public const int SERVICE_CONTROL_STOP = 1;
            public const int SERVICE_CONTROL_DEVICEEVENT = 11;
            public const int SERVICE_CONTROL_SHUTDOWN = 5;

            public const uint GENERIC_READ = 0x80000000;
            public const uint OPEN_EXISTING = 3;
            public const uint FILE_SHARE_READ = 1;
            public const uint FILE_SHARE_WRITE = 2;
            public const uint FILE_SHARE_DELETE = 4;
            public const uint FILE_ATTRIBUTE_NORMAL = 128;
            public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;
            public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

            public const int DBT_DEVTYP_DEVICEINTERFACE = 5;
            public const int DBT_DEVTYP_HANDLE = 6;

            public const int DBT_DEVICEARRIVAL = 0x8000;
            public const int DBT_DEVICEQUERYREMOVE = 0x8001;
            public const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

            public const int WM_DEVICECHANGE = 0x219;

            public delegate int ServiceControlHandlerEx(int control, int eventType, IntPtr eventData, IntPtr context);

            [DllImport("advapi32.dll", SetLastError = true)]
            public static extern IntPtr RegisterServiceCtrlHandlerEx(string lpServiceName, ServiceControlHandlerEx cbex, IntPtr context);

            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetVolumePathNamesForVolumeNameW(
                    [MarshalAs(UnmanagedType.LPWStr)]
                    string lpszVolumeName,
                    [MarshalAs(UnmanagedType.LPWStr)]
                    string lpszVolumePathNames,
                    uint cchBuferLength,
                    ref UInt32 lpcchReturnLength);

            [DllImport("kernel32.dll")]
            public static extern bool GetVolumeNameForVolumeMountPoint(string
               lpszVolumeMountPoint, [Out] StringBuilder lpszVolumeName,
               uint cchBufferLength);

            [DllImport("user32.dll", SetLastError = true)]
            public static extern IntPtr RegisterDeviceNotification(IntPtr IntPtr, IntPtr NotificationFilter, Int32 Flags);

            [DllImport("user32.dll", CharSet = CharSet.Auto)]
            public static extern uint UnregisterDeviceNotification(IntPtr hHandle);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr CreateFile(
                  string FileName,                    // file name
                  uint DesiredAccess,                 // access mode
                  uint ShareMode,                     // share mode
                  uint SecurityAttributes,            // Security Attributes
                  uint CreationDisposition,           // how to create
                  uint FlagsAndAttributes,            // file attributes
                  int hTemplateFile                   // handle to template file
                  );

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hObject);

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct DEV_BROADCAST_DEVICEINTERFACE {
                public int dbcc_size;
                public int dbcc_devicetype;
                public int dbcc_reserved;
                [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
                public byte[] dbcc_classguid;
                [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
                public char[] dbcc_name;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DEV_BROADCAST_HDR {
                public int dbcc_size;
                public int dbcc_devicetype;
                public int dbcc_reserved;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct DEV_BROADCAST_HANDLE {
                public int dbch_size;
                public int dbch_devicetype;
                public int dbch_reserved;
                public IntPtr dbch_handle;
                public IntPtr dbch_hdevnotify;
                public Guid dbch_eventguid;
                public long dbch_nameoffset;
                public byte dbch_data;
                public byte dbch_data1;
            }
        }
    }
}
