using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;
using HardwareHelperLib;

/* ***** BEGIN LICENSE BLOCK *****
 * Version: MIT License
 *
 * Copyright (c) 2010 Michael Sorens http://www.simple-talk.com/author/michael-sorens/
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 *
 * ***** END LICENSE BLOCK *****
 */

namespace TouchDisableTray
{

    /// <summary>
    /// Framework for running application as a tray app.
    /// </summary>
    /// <remarks>
    /// Tray app code adapted from "Creating Applications with NotifyIcon in Windows Forms", Jessica Fosler,
    /// http://windowsclient.net/articles/notifyiconapplications.aspx
    /// </remarks>
    public class CustomApplicationContext : ApplicationContext
    {
        // Icon graphic from http://prothemedesign.com/circular-icons/
        private static readonly string DefaultTooltip = "Enable or Disable touch screen";
        //private readonly HostManager hostManager;

        private Icon _yesIcon = null;
        private Icon YesIcon
        {
            get
            {
                if (_yesIcon == null)
                {
                    _yesIcon = new Icon("c_yes.ico");
                }
                return _yesIcon;
            }
        }
        private Icon _noIcon = null;
        private Icon NoIcon
        {
            get
            {
                if (_noIcon == null)
                {
                    _noIcon = new Icon("c_no.ico");
                }
                return _noIcon;
            }
        }
        /// <summary>
        /// Indicates whether we're <B>currently</B> showing yes or no
        /// </summary>
        private bool showingYes = true;

        HH_Lib _hwLib = null;
        private HH_Lib hwLib
        {
            get
            {
                if(_hwLib == null)
                {
                    _hwLib = new HH_Lib();
                }
                return _hwLib;
            }
        }

        DEVICE_INFO touchScreenDevice;

        /// <summary>
        /// This class should be created and passed into Application.Run( ... )
        /// </summary>
        public CustomApplicationContext() 
        {
            InitializeContext();
        }

        private bool contextMenuInitialized = false;
        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (!contextMenuInitialized)
            {
                //notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                var settingsItem = new ToolStripMenuItem("&Settings");
                settingsItem.Click += settingsItem_Click;
                // add to the .Click event
                notifyIcon.ContextMenuStrip.Items.Add(settingsItem);
                var exitItem = new ToolStripMenuItem("&Exit");
                exitItem.Click += exitItem_Click;
                notifyIcon.ContextMenuStrip.Items.Add(exitItem);

                contextMenuInitialized = true;
            }
        }

        # region the child forms

        private SettingsForm settingsForm;

        private void ShowSettingsForm()
        {
            if (settingsForm == null)
            {
                settingsForm = new SettingsForm {/* HostManager = hostManager */};
                settingsForm.Closed += settingsForm_Closed; // avoid reshowing a disposed form
                settingsForm.Show();
            }
            else { settingsForm.Activate(); }
        }

        // From http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu
        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Right:
                    {
                        MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu",
                            BindingFlags.Instance | BindingFlags.NonPublic);
                        mi.Invoke(notifyIcon, null);
                        break;
                    }
                case MouseButtons.Left:
                    try
                    {
                        // Switch the icon and hardware state. Exception will skip the rest if
                        // there's a failure
                        hwLib.SetDeviceState(touchScreenDevice, !showingYes);
                        showingYes = !showingYes;
                        notifyIcon.Icon = showingYes ? YesIcon : NoIcon;
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Exception while trying to change hardware state.  Message was: "
                            + ex.ToString(), "Error");
                    }
                    break;
            }
        }

        //// null out the forms so we know to create a new one.
        private void settingsForm_Closed(object sender, EventArgs e)     { settingsForm = null; }

        # endregion the child forms

        # region generic code framework

        private NotifyIcon notifyIcon;            // the icon that sits in the system tray

        private void InitializeContext()
        {
            var devices = hwLib.GetAll("HID-compliant touch screen");
            if (devices.Count != 1)
            {
                MessageBox.Show("More than one device or no touch screen device found!  Error!");
                throw new Exception("Multiple devices found");
            }
            touchScreenDevice = devices[0];
            showingYes = touchScreenDevice.status == DeviceStatus.Enabled;

            notifyIcon = new NotifyIcon()
                             {
                                 ContextMenuStrip = new ContextMenuStrip(),
                                 Icon = showingYes ? YesIcon : NoIcon,
                                 Text = DefaultTooltip,
                                 Visible = true
                             };
            notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            notifyIcon.MouseUp += notifyIcon_MouseUp;
        }

        private void settingsItem_Click(object sender, EventArgs e)
        {
            ShowSettingsForm();
        }

        /// <summary>
        /// When the exit menu item is clicked, make a call to terminate the ApplicationContext.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitItem_Click(object sender, EventArgs e) 
        {
            ExitThread();
        }

        /// <summary>
        /// If we are presently showing a form, clean it up.
        /// </summary>
        protected override void ExitThreadCore()
        {
            // before we exit, let forms clean themselves up.
            if (settingsForm != null) { settingsForm.Close(); }

            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }

        # endregion generic code framework

    }
}
