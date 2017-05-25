using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsbMonitor;

namespace LogViewer {
    public partial class MainForm : Form {
        private static readonly string ALL_USERS = "(Все пользователи)";

        private LogDatabase _db;
        private SortedSet<string> _windowsUsers = new SortedSet<string>();
        private SortedSet<string> _allWindowsUsers = new SortedSet<string>();
        private Dictionary<string, RegisteredUser> _registeredUsers = new Dictionary<string, RegisteredUser>();

        public MainForm() {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e) {
            _db = new LogDatabase();
            try {
                InitUsers();
            } catch (System.Data.Entity.Core.EntityException) {
                MessageBox.Show("Cannot access database! [Maybe, you should run me as admin? :) ]");
                Close();
                return;
            }
        }

        private void InitUsers() {
            _registeredUsers.Clear();
            foreach(var user in _db.RegisteredUsers) {
                _registeredUsers.Add(user.Username, user);
            }

            _allWindowsUsers.Clear();
            SelectQuery query = new SelectQuery("Win32_UserAccount");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            foreach (ManagementObject envVar in searcher.Get()) {
                _allWindowsUsers.Add(Environment.UserDomainName + "\\" + (string)envVar["Name"]);
            }

            _windowsUsers.Clear();
            _windowsUsers.UnionWith(_db.LogEntries.Select(entry => entry.Username));
            _windowsUsers.RemoveWhere(u => _registeredUsers.ContainsKey(u));

            cbUsers.Items.Clear();
            cbUsers.Items.Add(ALL_USERS);
            cbUsers.Items.AddRange(_registeredUsers.Values.ToArray());
            cbUsers.Items.AddRange(_windowsUsers.ToArray());
            cbUsers.SelectedIndex = 0;

            cbUsbUsers.Items.Clear();
            cbUsbUsers.Items.Add(ALL_USERS);
            cbUsbUsers.Items.AddRange(_registeredUsers.Values.ToArray());
            cbUsbUsers.SelectedIndex = 0;

            cbWindowsUser.Items.Clear();
            cbWindowsUser.Items.Add(ALL_USERS);
            _windowsUsers.UnionWith(_allWindowsUsers);
            cbWindowsUser.Items.AddRange(_windowsUsers.ToArray());
            //cbWindowsUser.Items.AddRange(_registeredUsers.Keys.ToArray());
            cbWindowsUser.SelectedIndex = 0;

            tbSerial.Clear();
            tbUsbSerial.Clear();
            tbPath.Clear();
            dtpFrom.Value = DateTime.Now;
            dtpTo.Value = DateTime.Now;
            tbCurrentSerial.Clear();
            tbFirstName.Clear();
            tbSecondName.Clear();
        }

        private void FilterLogEntries() {
            IEnumerable<LogEntry> logs;
            var username = cbUsers.SelectedItem as string;
            if (username == ALL_USERS) {
                logs = _db.LogEntries;
            } else {
                if (username == null) {
                    username = ((RegisteredUser)cbUsers.SelectedItem).Username;
                }
                logs = _db.LogEntries.Where(l => l.Username == username);
            }

            if (tbSerial.Text != "") {
                logs = logs.Where(l => l.SerialNumber != null && l.SerialNumber.Contains(tbSerial.Text));
            }

            if (tbPath.Text != "") {
                logs = logs.Where(l => {
                    var usbEntry = l as UsbStateEntry;
                    if (usbEntry != null) {
                        return usbEntry.DriveLetter.Contains(tbPath.Text);
                    }
                    var fsEntry = l as FSWatcherEntry;
                    return fsEntry.Path.Contains(tbPath.Text) ||
                        (fsEntry.OldPath != null && fsEntry.OldPath.Contains(tbPath.Text));
                });
            }

            logs = logs.Where(l => l.When.Date >= dtpFrom.Value.Date && l.When.Date <= dtpTo.Value.Date);

            lbEvents.Items.Clear();
            lbEvents.Items.AddRange(logs.Reverse().Select(l => LogEntryToListBoxItem(l)).ToArray());
        }

        private void FilterRegisteredUsb() {
            IEnumerable<RegisteredUsbEntry> usb;
            if (cbUsbUsers.SelectedItem as string == ALL_USERS) {
                usb = _db.RegisteredUsbs;
            } else {
                usb = _db.RegisteredUsbs.Where(l => l.User.Id == ((RegisteredUser)cbUsbUsers.SelectedItem).Id);
            }

            if (tbUsbSerial.Text != "") {
                usb = usb.Where(l => l.UsbSerial.Contains(tbUsbSerial.Text));
            }

            lbUsb.Items.Clear();
            lbUsb.Items.AddRange(usb.Reverse().Select(l => RegisteredUsbEntryToBoxItem(l))
                .ToArray());
        }

        private void FilterRegisteredUsers() {
            IEnumerable<RegisteredUser> users;
            if ((string)cbWindowsUser.SelectedItem == ALL_USERS) {
                users = _db.RegisteredUsers;
            } else {
                users = _db.RegisteredUsers.Where(l => l.Username == ((string)cbWindowsUser.SelectedItem));
            }

            if (tbFirstName.Text != "") {
                users = users.Where(l => l.FirstName.Contains(tbFirstName.Text));
            }

            if (tbSecondName.Text != "") {
                users = users.Where(l => l.SecondName.Contains(tbSecondName.Text));
            }

            lbRegisteredUsers.Items.Clear();
            lbRegisteredUsers.Items.AddRange(users.Reverse().Select(v => new ListBoxItem {
                Id = v.Id,
                Text = string.Format("{0} {1} - [{2}]", v.FirstName, v.SecondName, v.Username)
            }).ToArray());
        }

        private static string UsbStateToString(USB_STATE state) {
            switch (state) {
                case USB_STATE.INSERTED:
                    return "Inserted";
                case USB_STATE.REJECTED:
                    return "Denied";
                case USB_STATE.REMOVED:
                    return "Unplugged";
                default:
                    return null;
            }
        }

        private static string FileStateToString(FILE_STATE state) {
            switch (state) {
                case FILE_STATE.CHANGED:
                    return "Changed";
                case FILE_STATE.CREATED:
                    return "Created";
                case FILE_STATE.REMOVED:
                    return "Removed";
                case FILE_STATE.RENAMED:
                    return "Renamed";
                default:
                    return null;
            }
        }

        private ListBoxItem LogEntryToListBoxItem(LogEntry logEntry) {
            var usbEntry = logEntry as UsbStateEntry;
            StringBuilder sb = new StringBuilder();

            var when = logEntry.When;
            var username = logEntry.Username;
            RegisteredUser user;
            _registeredUsers.TryGetValue(username, out user);
            if (user != null) {
                username = user.ToString();
            }
            
            string serial = logEntry.SerialNumber == null ? "NO_SERIAL" : logEntry.SerialNumber;
            sb.AppendFormat("{0} {1} [{2}] <{3}>:\n  ", when.ToShortDateString(),
                when.ToLongTimeString(), username, serial);

            if (usbEntry != null) {
                string action = UsbStateToString(usbEntry.State);
                sb.AppendFormat("{0} media {1}", action, usbEntry.DriveLetter);
            } else {
                var fsEntry = logEntry as FSWatcherEntry;
                string action = FileStateToString(fsEntry.State);
                sb.AppendFormat("{0} file at \"{1}\"", action, fsEntry.Path);
                if (fsEntry.State == FILE_STATE.RENAMED) {
                    sb.AppendFormat(" from \"{0}\"", fsEntry.OldPath);
                }
            }

            return new ListBoxItem {
                Id = logEntry.Id,
                Text = sb.ToString(),
                Serial = serial
            };
        }

        private static ListBoxItem RegisteredUsbEntryToBoxItem(RegisteredUsbEntry entry) {
            return new ListBoxItem {
                Id = entry.Id,
                Text = string.Format("{0} allowed [{1}]", entry.User, entry.UsbSerial)
            };
        }

        private void cbUsers_SelectedIndexChanged(object sender, EventArgs e) {
            FilterLogEntries();
        }

        private void lbEvents_DrawItem(object sender, DrawItemEventArgs e) {
            e.DrawBackground();
            if (e.Index >= 0) {
                e.Graphics.DrawString(lbEvents.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
            }
        }

        private void cbUsbUsers_SelectedIndexChanged(object sender, EventArgs e) {
            FilterRegisteredUsb();
        }

        private void bAddUsb_Click(object sender, EventArgs e) {
            if (cbUsbUsers.Text == "" || tbUsbSerial.Text == "" || cbUsbUsers.Text == ALL_USERS) {
                return;
            }

            if (_db.RegisteredUsbs.Any(i =>
                i.UsbSerial == tbUsbSerial.Text && i.User.Id == ((RegisteredUser)cbUsbUsers.SelectedItem).Id)) {
                return;
            }

            _db.RegisteredUsbs.Add(new RegisteredUsbEntry {
                User = (RegisteredUser)(cbUsbUsers.SelectedItem),
                UsbSerial = tbUsbSerial.Text
            });
            _db.SaveChanges();
            InitUsers();
        }

        private void bDeleteUsb_Click(object sender, EventArgs e) {
            var item = lbUsb.SelectedItem as ListBoxItem;
            if (item != null) {
                _db.RegisteredUsbs.Remove(
                    _db.RegisteredUsbs.Where(i => i.Id == item.Id).First());
                _db.SaveChanges();
                InitUsers();
            }
        }

        private void tbUsbSerial_TextChanged(object sender, EventArgs e) {
            FilterRegisteredUsb();
        }

        private void bReset_Click(object sender, EventArgs e) {
            InitUsers();
        }

        private void tbSerial_TextChanged(object sender, EventArgs e) {
            FilterLogEntries();
        }

        private void dtpFrom_ValueChanged(object sender, EventArgs e) {
            FilterLogEntries();
        }

        private void dtpTo_ValueChanged(object sender, EventArgs e) {
            FilterLogEntries();
        }

        private void tbPath_TextChanged(object sender, EventArgs e) {
            FilterLogEntries();
        }

        private void lbEvents_SelectedIndexChanged(object sender, EventArgs e) {
            if (lbEvents.SelectedItem != null) {
                tbCurrentSerial.Text = ((ListBoxItem)lbEvents.SelectedItem).Serial;
            }
        }

        private void bAddRegisteredUser_Click(object sender, EventArgs e) {
            if (tbSecondName.Text == "" || tbFirstName.Text == "" || cbWindowsUser.Text == ALL_USERS) {
                return;
            }

            if (_db.RegisteredUsers.Any(i =>
                i.FirstName == tbFirstName.Text && i.SecondName == tbSecondName.Text
                || i.Username == cbWindowsUser.Text)) {
                return;
            }

            _db.RegisteredUsers.Add(new RegisteredUser {
                FirstName = tbFirstName.Text,
                SecondName = tbSecondName.Text,
                Username = cbWindowsUser.Text
            });
            _db.SaveChanges();
            InitUsers();
        }

        private void bDeleteRegisteredUser_Click(object sender, EventArgs e) {
            var item = lbRegisteredUsers.SelectedItem as ListBoxItem;
            if (item != null) {
                _db.RegisteredUsbs.RemoveRange(_db.RegisteredUsbs.Where(u => u.User.Id == item.Id));
                _db.RegisteredUsers.Remove(
                    _db.RegisteredUsers.Where(i => i.Id == item.Id).First());
                _db.SaveChanges();
                InitUsers();
            }
        }

        private void tbFirstName_TextChanged(object sender, EventArgs e) {
            FilterRegisteredUsers();
        }

        private void tbSecondName_TextChanged(object sender, EventArgs e) {
            FilterRegisteredUsers();
        }

        private void cbWindowsUser_SelectedIndexChanged(object sender, EventArgs e) {
            FilterRegisteredUsers();
        }
    }

    internal class ListBoxItem {
        public int Id;
        public string Text;
        public string Serial;

        public override string ToString() {
            return Text;
        }
    }
}
