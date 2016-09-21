using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Management.Instrumentation;
using System.Collections.Specialized;
using System.Threading;

namespace HDDLED
{
    public partial class InvisibleForm : Form
    { 
        #region Global Variables
        NotifyIcon hddNotifyIcon;
        Icon busyIcon;
        Icon idleIcon;
        Thread hddInfoWorkerThread;
        #endregion

        #region Main Form (entry point)
        public InvisibleForm()
        {
            InitializeComponent();
            
            // Load icons from files into objects
            busyIcon = new Icon("HDD_Busy.ico");
            idleIcon = new Icon("HDD_Idle.ico");

            //Create notify icons and assign idle icon and show it
            hddNotifyIcon = new NotifyIcon();
            hddNotifyIcon.Icon = idleIcon;
            hddNotifyIcon.Visible = true;

            // Create all context menu items and add them to notification tray icon 
            MenuItem progNameMenuItem = new MenuItem("Hard Drive LED v1");
            MenuItem quitMenuItem = new MenuItem("Quit");
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add(progNameMenuItem);
            contextMenu.MenuItems.Add(quitMenuItem);
            hddNotifyIcon.ContextMenu = contextMenu;

            // Wire up quit button to close application
            quitMenuItem.Click += quitMenuItem_Click;
            
            //Hide the form - notifcation form application
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            // Start worker thread that pulls HDD activity
            hddInfoWorkerThread = new Thread(new ThreadStart(HddActivityThread));
            hddInfoWorkerThread.Start();
        }

       
        #endregion

        #region Context Menu Event Handlers
        //Close the application on click of 'quit' button context menu
        void quitMenuItem_Click(object sender, EventArgs e)
        {
            hddInfoWorkerThread.Abort();
            hddNotifyIcon.Dispose();
            this.Close();
        }
        #endregion
        
        
        #region Hard Drive Activity Threads

        // This is the thread that pulls the HDD for activity and updates the notification icon
        public void HddActivityThread()
        {
            ManagementClass driveDataClass = new ManagementClass("Win32_PerfFormattedData_PerfDisk_PhysicalDisk");
            
            try
            {
                
                //Main loop
                while (true)
                {
                    // Connect to the drive performance instance
                    ManagementObjectCollection driveDataClassCollection = driveDataClass.GetInstances();
                    foreach ( ManagementObject obj in driveDataClassCollection)
                    {
                        // Only processs the _Total instance and ignore all the individual instances
                        if ( obj["Name"].ToString() == "_Total")
                        {
                            if ( Convert.ToUInt64(obj["DiskBytesPersec"]) > 0)
                            {
                                //Show busy icon
                                hddNotifyIcon.Icon = busyIcon;
                                
                            }
                            else
                            {
                                // Show idle icon
                                hddNotifyIcon.Icon = idleIcon;
                                
                            }
                        }

                    }

                    // Sleep for 10th of millisecond 
                    Thread.Sleep(100);
                }
            }
            catch ( ThreadAbortException tbe )
            {
                driveDataClass.Dispose();
            }
        }  
          #endregion
    }
}
