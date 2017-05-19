using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UsbMonitor;

namespace LogViewer {
    public partial class MainForm : Form {
        private static readonly string ALL_USERS = "(Все пользователи)";

        private LogDatabase _db;
        private SortedSet<string> _users = new SortedSet<string>();

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
            _users.Clear();
            _users.UnionWith(_db.LogEntries.Select(entry => entry.Username));
            _users.UnionWith(_db.RegisteredUsbs.Select(entry => entry.Username));

            cbUsers.Items.Clear();
            cbUsers.Items.AddRange(_users.ToArray());
            cbUsers.Items.Add(ALL_USERS);

            cbUsbUsers.Items.Clear();
            cbUsbUsers.Items.AddRange(_users.ToArray());
            cbUsbUsers.Items.Add(ALL_USERS);

            tbSerial.Clear();
            tbUsbSerial.Clear();
            tbPath.Clear();
            dtpFrom.Value = DateTime.Now;
            dtpTo.Value = DateTime.Now;

            cbUsers.SelectedIndex = 0;
            cbUsbUsers.SelectedIndex = 0;
        }

        private void FilterLogEntries() {
            IEnumerable<LogEntry> logs;
            if ((string)cbUsers.SelectedItem == ALL_USERS) {
                logs = _db.LogEntries;
            } else {
                logs = _db.LogEntries.Where(l => l.Username == (string)cbUsers.SelectedItem);
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
            if ((string)cbUsbUsers.SelectedItem == ALL_USERS) {
                usb = _db.RegisteredUsbs;
            } else {
                usb = _db.RegisteredUsbs.Where(l => l.Username == (string)cbUsbUsers.SelectedItem);
            }

            if (tbUsbSerial.Text != "") {
                usb = usb.Where(l => l.UsbSerial.Contains(tbUsbSerial.Text));
            }

            lbUsb.Items.Clear();
            lbUsb.Items.AddRange(usb.Reverse().Select(l => RegisteredUsbEntryToBoxItem(l))
                .ToArray());
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

        private static ListBoxItem LogEntryToListBoxItem(LogEntry logEntry) {
            var usbEntry = logEntry as UsbStateEntry;
            StringBuilder sb = new StringBuilder();

            var when = logEntry.When;
            sb.AppendFormat("{0} {1} [{2}] <{3}>:\n  ", when.ToShortDateString(),
                when.ToLongTimeString(), logEntry.Username, logEntry.SerialNumber);

            if (usbEntry != null) {
                string action = UsbStateToString(usbEntry.State);
                sb.AppendFormat("{0} media {1} [{2}]", action, usbEntry.DriveLetter,
                    usbEntry.SerialNumber == null ? "NO_SERIAL" : usbEntry.SerialNumber);
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
                Text = sb.ToString()
            };
        }

        private static ListBoxItem RegisteredUsbEntryToBoxItem(RegisteredUsbEntry entry) {
            return new ListBoxItem {
                Id = entry.Id,
                Text = string.Format("{0} allowed [{1}]", entry.Username, entry.UsbSerial)
            };
        }

        private void cbUsers_SelectedIndexChanged(object sender, EventArgs e) {
            FilterLogEntries();
        }

        private void lbEvents_DrawItem(object sender, DrawItemEventArgs e) {
            e.DrawBackground();
            e.Graphics.DrawString(lbEvents.Items[e.Index].ToString(), e.Font, new SolidBrush(e.ForeColor), e.Bounds);
        }

        private void cbUsbUsers_SelectedIndexChanged(object sender, EventArgs e) {
            FilterRegisteredUsb();
        }

        private void bAddUsb_Click(object sender, EventArgs e) {
            if (cbUsbUsers.Text == "" || tbUsbSerial.Text == "") {
                //
                return;
            }

            if (_db.RegisteredUsbs.Any(i =>
                i.UsbSerial == tbUsbSerial.Text && i.Username == cbUsbUsers.Text)) {
                return;
            }

            _db.RegisteredUsbs.Add(new RegisteredUsbEntry {
                Username = cbUsbUsers.Text,
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
    }

    internal class ListBoxItem {
        public int Id;
        public string Text;

        public override string ToString() {
            return Text;
        }
    }
}
