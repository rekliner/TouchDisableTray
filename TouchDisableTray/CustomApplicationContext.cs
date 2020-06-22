using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Collections.Generic;

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
        private bool showingYes = true;

        /// <summary>
        /// This class should be created and passed into Application.Run( ... )
        /// </summary>
        public CustomApplicationContext() 
        {
            InitializeContext();
            //hostManager = new HostManager(notifyIcon);
            //hostManager.BuildServerAssociations();
            //if (!hostManager.IsDecorated) { ShowSettingsForm(); }
        }

        private bool contextMenuInitialized = false;
        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;

            if (!contextMenuInitialized)
            {
                //notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                var stuffItem = new ToolStripMenuItem("&DoStuff");
                // add to the .Click event
                notifyIcon.ContextMenuStrip.Items.Add(stuffItem);
                var exitItem = new ToolStripMenuItem("&Exit");
                exitItem.Click += exitItem_Click;
                notifyIcon.ContextMenuStrip.Items.Add(exitItem);

                contextMenuInitialized = true;
            }
        }

        # region the child forms

        private SettingsForm settingsForm;
        //private System.Windows.Window introForm;

        private void ShowSettingsForm()
        {
            if (settingsForm == null)
            {
                settingsForm = new SettingsForm {/* HostManager = hostManager */};
                settingsForm.Closed += settingsForm_Closed; // avoid reshowing a disposed form
                settingsForm.Show();
            }
            else { settingsForm.Activate(); }
            //if (introForm == null)
            //{
            //    introForm = new WpfFormLibrary.IntroForm();
            //    introForm.Closed += mainForm_Closed; // avoid reshowing a disposed form
            //    ElementHost.EnableModelessKeyboardInterop(introForm);
            //    introForm.Show();
            //}
            //else { introForm.Activate(); }
        }

        private void ShowDetailsForm()
        {
            //if (detailsForm == null)
            //{
            //    detailsForm = new DetailsForm {HostManager = hostManager};
            //    detailsForm.Closed += detailsForm_Closed; // avoid reshowing a disposed form
            //    detailsForm.Show();
            //}
            //else { detailsForm.Activate(); }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e) { ShowSettingsForm();    }

        // From http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu
        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            switch(e.Button)
            {
                case MouseButtons.Right:
                    {
                        MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                        mi.Invoke(notifyIcon, null);
                        break;
                    }
                case MouseButtons.Left:
                    {
                        // Switch the icon
                        if(showingYes)
                        {
                            notifyIcon.Icon = NoIcon;
                        }
                        else
                        {
                            notifyIcon.Icon = YesIcon;
                        }
                        showingYes = !showingYes;
                        break;
                    }
            }
        }


        //// attach to context menu items
        //private void showHelpItem_Click(object sender, EventArgs e)     { ShowSettingsForm();    }
        //private void showDetailsItem_Click(object sender, EventArgs e)  { ShowDetailsForm();  }

        //// null out the forms so we know to create a new one.
        private void settingsForm_Closed(object sender, EventArgs e)     { settingsForm = null; }
        //private void mainForm_Closed(object sender, EventArgs e)        { introForm = null;   }

        # endregion the child forms

        # region generic code framework

        private System.ComponentModel.IContainer components;	// a list of components to dispose when the context is disposed
        private NotifyIcon notifyIcon;				            // the icon that sits in the system tray

        private void InitializeContext()
        {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components)
                             {
                                 ContextMenuStrip = new ContextMenuStrip(),
                                 Icon = YesIcon,
                                 Text = DefaultTooltip,
                                 Visible = true
                             };
            notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            notifyIcon.MouseUp += notifyIcon_MouseUp;
        }

        /// <summary>
        /// When the application context is disposed, dispose things like the notify icon.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && components != null) { components.Dispose(); }
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
            //// before we exit, let forms clean themselves up.
            //if (introForm != null) { introForm.Close(); }
            //if (detailsForm != null) { detailsForm.Close(); }

            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }

        # endregion generic code framework

    }
}
