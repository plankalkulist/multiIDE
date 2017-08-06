using System;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections;
using XSpace;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections.Generic;

namespace multiIDE
{
    public partial class frmConsole : Form
    {
        public virtual IODevices.ConsoleDevice ParentConsoleDevice { get; protected set; }
        public virtual RichTextBox Display { get; protected set; }
        //
        public bool IsBusyInputting { get; private set; } = false;
        public bool IsDisposing { get; protected set; } = false;
        public object IsBusyInputtingBy { get; private set; }
        public int PostsToOutputLeft { get; private set; } = 0;
        public Hashtable DataClients { get; set; } = new Hashtable();
        public List<Color> DataClientsColors { get; set; } = new List<Color>();

        /////
        protected string _PreText { get; set; }
        protected string _PreRtf;
        protected bool _IsInputing;
        //
        protected readonly SynchronizationContext syncContext;
        //
        [DllImport("user32.dll")] //scrolling to the end of textbox
        protected static extern int PostMessage(IntPtr wnd, uint Msg, IntPtr wParam, IntPtr lParam);
        protected const uint WM_VSCROLL = 0x0115;
        protected const uint SB_BOTTOM = 7;

        public frmConsole(IODevices.ConsoleDevice parentConsoleDevice)
        {
            InitializeComponent();
            //
            Load += frmConsole_Load;
            //
            ParentConsoleDevice = parentConsoleDevice;
            Display = rtbDisplay;
            //
            syncContext = SynchronizationContext.Current;
        }

        protected void frmConsole_Load(object sender, EventArgs e)
        {
            UpdateTitle();
            rtbDisplay.Clear();
        }

        protected void _UpdateTitle()
        {
            if (!this.IsDisposed)
            {
                if (ParentConsoleDevice != null)
                {
                    this.Text = ParentConsoleDevice.Title;
                }
                else
                {
                    this.Text = "--ERROR: SINGLE WINDOW--";
                }
            }
        }

        protected void _CheckDisplayParameters()
        {
            if (!this.IsDisposed)
            {
                if (Display.TextLength > ParentConsoleDevice.MaxDisplayTextLength)
                {
                    int l = (int)(Display.TextLength * 0.1);
                    Display.Select(l, Display.TextLength - l);
                    Display.Rtf = Display.SelectedRtf;
                }
            }
        }

        // // //

        protected bool _InputCancelled;
        protected bool _DisposeAfterInput;
        public int InputtingForLength { get; private set; }

        protected string _Input(object sender, int length)
        {
            string inData;

            if (!this.IsDisposed)
            {
                IsBusyInputting = true;
                IsBusyInputtingBy = sender;
                InputtingForLength = length;

                _PreText = rtbDisplay.Text;
                _PreRtf = rtbDisplay.Rtf;
                rtbDisplay.ReadOnly = false;
                rtbDisplay.TextChanged += rtbDisplay_TextChanged;
                rtbDisplay.KeyDown += rtbDisplay_KeyDown;
                rtbDisplay.SelectionStart = rtbDisplay.TextLength;
                rtbDisplay.SelectionLength = 0;
                PostMessage(rtbDisplay.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);

                _IsInputing = true;
                _InputCancelled = false;
                _DisposeAfterInput = false;
                IVirtualMachine senderMachine = (sender as IInputPort)?.ParentIDE.Machine ?? sender as IVirtualMachine;
                while (_IsInputing && !_InputCancelled
                                   && !(senderMachine?.Status == VirtualMachineRunningStatus.Breaking
                                   || senderMachine?.Status == VirtualMachineRunningStatus.StandBy
                                   || senderMachine?.Status == VirtualMachineRunningStatus.Paused)
                                   && !(senderMachine?.RunningTask.Status == TaskStatus.Canceled
                                    || senderMachine?.RunningTask.Status == TaskStatus.RanToCompletion
                                    || senderMachine?.RunningTask.Status == TaskStatus.Faulted))
                {
                    Application.DoEvents();
                    Thread.Yield();
                }
                _IsInputing = false;
                if (!_InputCancelled && !(senderMachine?.Status == VirtualMachineRunningStatus.Breaking))
                {
                    inData = rtbDisplay.Text.Substring(_PreText.Length);
                }
                else
                {
                    Display.Rtf = _PreRtf;
                    inData = null;
                }

                rtbDisplay.ReadOnly = true;
                rtbDisplay.TextChanged -= rtbDisplay_TextChanged;
                rtbDisplay.KeyDown -= rtbDisplay_KeyDown;
                if (ParentConsoleDevice.AutoScrollDisplayToEnd)
                    PostMessage(rtbDisplay.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);

                IsBusyInputting = false;
                IsBusyInputtingBy = null;
                InputtingForLength = -1;

                if (_DisposeAfterInput)
                {
                    ParentConsoleDevice.Dispose();
                }
            }
            else
            {
                inData = null;
            }

            _CheckDisplayParameters();
            return inData;
        }

        protected void _Output(object sender, string outData)
        {
            if (!this.IsDisposed)
            {
                rtbDisplay.AppendText(outData);
                if (ParentConsoleDevice.AutoScrollDisplayToEnd)
                    PostMessage(rtbDisplay.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            }
            _CheckDisplayParameters();
        }

        protected string _GetDisplayRTFtext() => Display.Rtf;

        // // //

        #region Thread-safe wrapping
        public virtual void Input(object sender, out string inData, int length)
        {
            object sendie = (sender as IIdeComponent)?.ParentIDE.Machine ?? sender;

            if (this.InvokeRequired)
            {
                if (!DataClients.ContainsKey(sendie))
                {
                    DataClients.Add(sendie, DataClients.Count);
                    if (DataClientsColors.Count > DataClients.Count - 1)
                        DataClientsColors.RemoveAt(DataClients.Count - 1);
                    DataClientsColors.Insert(DataClients.Count - 1, XGraphics.HSV(55 * (int)DataClients.Count - 1, 80, 100));
                }

                object objInData = "";
                if (!DataClients.ContainsKey(sendie))
                    DataClients.Add(sendie, DataClients.Count);

                this.Invoke(new Action<System.Drawing.Color>((c) => { rtbDisplay.SelectionColor = c; })
                            , DataClientsColors[(int)DataClients[sendie]]);

                var d = new Func<object, int, string>(_Input);
                objInData = this.Invoke(d, sender, length);
                inData = (string)objInData;

                if (!this.IsDisposed)
                    this.Invoke(new Action<System.Drawing.Color>((c) =>
                    { rtbDisplay.SelectionColor = c; }), Color.White);
            }
            else
            {
                rtbDisplay.SelectionColor = System.Drawing.Color.White;
                inData = _Input(sender, length);
            }
        }

        public virtual void Output(object sender, string outData)
        {
            object sendie = (sender as IIdeComponent)?.ParentIDE.Machine ?? sender;

            if (this.InvokeRequired)
            {
                if (!DataClients.ContainsKey(sendie))
                {
                    DataClients.Add(sendie, DataClients.Count);
                    if (DataClientsColors.Count > DataClients.Count - 1)
                        DataClientsColors.RemoveAt(DataClients.Count - 1);
                    DataClientsColors.Insert(DataClients.Count - 1, XGraphics.HSV(55 * (int)DataClients.Count - 1, 80, 100));
                }

                syncContext.Post(new SendOrPostCallback((o) =>
                {
                    if (!this.IsDisposed)
                    {
                        if (!_IsInputing)
                        {
                            if (ParentConsoleDevice.ColorDataClients)
                                rtbDisplay.SelectionColor = DataClientsColors[(int)DataClients[sendie]];
                            _Output(sender, outData);
                            if (ParentConsoleDevice.ColorDataClients)
                                rtbDisplay.SelectionColor = System.Drawing.Color.White;
                        }
                        else
                        {
                            if (ParentConsoleDevice.ColorDataClients)
                                rtbDisplay.SelectionColor = DataClientsColors[(int)DataClients[sendie]];
                            rtbDisplay.AppendText(outData);
                            if (ParentConsoleDevice.ColorDataClients)
                                rtbDisplay.SelectionColor = System.Drawing.Color.White;
                            _PreText = rtbDisplay.Text;
                            _PreRtf = rtbDisplay.Rtf;
                        }
                    }
                }), null);

                // giving console initializer thread time to update UI
                Thread.Sleep(10);
            }
            else
            {
                rtbDisplay.SelectionColor = System.Drawing.Color.White;
                _Output(sender, outData);
            }
        }

        public new void Dispose()
        {
            IsDisposing = true;

            if (this.InvokeRequired)
            {
                var d = new Action(base.Dispose);
                this.Invoke(d);
            }
            else
            {
                base.Dispose();
            }

            IsDisposing = false;
        }

        public async Task DisposeAsync()
        {
            IsDisposing = true;

            if (this.InvokeRequired)
            {
                await Task.Run(() =>
                {
                    var d = new Action(base.Dispose);
                    this.Invoke(d);
                });
            }
            else
            {
                base.Dispose();
            }

            IsDisposing = false;
        }

        public virtual void UpdateTitle()
        {
            if (this.InvokeRequired)
            {
                var d = new Action(_UpdateTitle);
                this.Invoke(d);
            }
            else
            {
                _UpdateTitle();
            }

        }

        [LockingRequired]
        public void CheckDisplayParameters()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(_CheckDisplayParameters));
            }
            else
            {
                _CheckDisplayParameters();
            }
        }

        public new void Show()
        {
            if (!this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    syncContext.Post(new SendOrPostCallback((o) =>
                    {
                        base.Show();
                    }), null);
                }
                else
                {
                    base.Show();
                }

            }
        }

        public new void Activate()
        {
            if (!this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    syncContext.Post(new SendOrPostCallback((o) =>
                    {
                        base.Activate();
                    }), null);
                }
                else
                {
                    base.Activate();
                }
            }
        }

        public virtual string GetDisplayRTFtext()
        {
            if (!this.IsDisposed)
            {
                if (this.InvokeRequired)
                {
                    var d = new Func<string>(_GetDisplayRTFtext);
                    return (string)this.Invoke(d);
                }
                else
                {
                    return _GetDisplayRTFtext();
                }
            }
            else
            {
                return null;
            }
        }
        #endregion

        // // //

        private void rtbDisplay_TextChanged(object sender, EventArgs e)
        {
            if (_IsInputing && rtbDisplay
                .Text.Left(_PreText.Length < rtbDisplay.TextLength ? _PreText.Length : rtbDisplay.TextLength)
                    != _PreText)
            {
                if (rtbDisplay.TextLength <= _PreText.Length)
                {
                    rtbDisplay.Rtf = _PreRtf;
                    rtbDisplay.SelectionStart = rtbDisplay.TextLength;
                    PostMessage(rtbDisplay.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
                }
                else
                {
                    rtbDisplay.Rtf = _PreRtf + rtbDisplay.Rtf.Substring(_PreRtf.Length);
                    rtbDisplay.SelectionStart = rtbDisplay.TextLength;
                    PostMessage(rtbDisplay.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
                }
            }
        }

        private void rtbDisplay_KeyDown(object sender, KeyEventArgs e)
        {
            if (_IsInputing && rtbDisplay.SelectionStart < _PreText.Length)
            {
                rtbDisplay.SelectionStart = _PreText.Length;
                PostMessage(rtbDisplay.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            }

            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                _IsInputing = false;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                _InputCancelled = true;
                _IsInputing = false;
            }
        }

        protected void frmConsole_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                if (_IsInputing)
                {
                    if (MessageBox.Show($"Closing this window will dispose {ParentConsoleDevice.Title}"
                                + $" and cancel input for {IsBusyInputtingBy}. Continue?", "Warning about IO device dispose"
                                , MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1)
                            != DialogResult.OK)
                        return;
                    _InputCancelled = true;
                    _DisposeAfterInput = true;
                    _IsInputing = false;
                }
                else
                {
                    ParentConsoleDevice.Dispose();
                }
            }
        }

        protected void cmnDisplay_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cmnDisplay.Items.Clear();

            if (IsBusyInputting)
            {
                cmnDisplay.Items.Add(new ToolStripMenuItem
                        ($"Input To {(IsBusyInputtingBy as IIdeComponent)?.ParentIDE.Machine ?? IsBusyInputtingBy}", null, (_s, _e) =>
                                {
                                    ParentConsoleDevice.ParentWorkplace.MainFormActivate();
                                    ParentConsoleDevice.ParentWorkplace.MainFormSelectIDE((IsBusyInputtingBy as IIdeComponent)?.ParentIDE);
                                })
                {
                    ToolTipText = $"The Console Device is busy inputting {InputtingForLength} bytes {(IsBusyInputtingBy is IInputPort ? $"for {(IsBusyInputtingBy as IInputPort).InputConverting}" : "")} by {IsBusyInputtingBy}"
                });
                cmnDisplay.Items.Add("-");
            }

            cmnDisplay.Items.Add("Scroll To Begin", null, (_s, _e) =>
                    {
                        this.BeginInvoke(new Action(() =>
                                {
                                    rtbDisplay.SelectionStart = 0;
                                    rtbDisplay.ScrollToCaret();
                                }));
                    });
            cmnDisplay.Items.Add("Scroll To The End", null, (_s, _e) =>
                    {
                        this.BeginInvoke(new Action(() =>
                                {
                                    rtbDisplay.SelectionStart = rtbDisplay.TextLength;
                                    rtbDisplay.ScrollToCaret();
                                }));
                    });
            cmnDisplay.Items.Add(new ToolStripMenuItem
                    ("Auto Scroll To The End", null, (_s, _e) =>
                            {
                                bool isAutoScroll = (_s as ToolStripMenuItem).Checked;
                                (_s as ToolStripMenuItem).Checked = !isAutoScroll;
                                ParentConsoleDevice.AutoScrollDisplayToEnd = !isAutoScroll;
                            })
            { Checked = ParentConsoleDevice.AutoScrollDisplayToEnd });
            cmnDisplay.Items.Add("Clear Display", null, (_s, _e) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    rtbDisplay.Clear();
                }));
            });

            cmnDisplay.Items.Add("-");
            cmnDisplay.Items.Add(new ToolStripMenuItem
                    ("Color Data Clients", null, (_s, _e) =>
                    {
                        bool colored = (_s as ToolStripMenuItem).Checked;
                        (_s as ToolStripMenuItem).Checked = !colored;
                        ParentConsoleDevice.ColorDataClients = !colored;
                    })
            { Checked = ParentConsoleDevice.ColorDataClients });

            cmnDisplay.Items.Add("-");
            cmnDisplay.Items.Add(new ToolStripMenuItem("Initialized By:") { Enabled = false });
            cmnDisplay.Items.Add(new ToolStripMenuItem
                    (ParentConsoleDevice.InitializedBy.ToString(), null, (_s, _e) =>
                    {
                        ParentConsoleDevice.ParentWorkplace.MainFormActivate();
                        ParentConsoleDevice.ParentWorkplace.MainFormSelectIDE((ParentConsoleDevice.InitializedBy as IIdeComponent)?.ParentIDE);
                    })
            {
                BackColor = System.Drawing.Color.FromArgb(255, 10, 10, 10),
                ForeColor = System.Drawing.Color.White
            });

            if ((DataClients?.Count ?? -1) > 0)
            {
                cmnDisplay.Items.Add("-");
                cmnDisplay.Items.Add(new ToolStripMenuItem("Data Clients:") { Enabled = false });
                foreach (object key in DataClients.Keys)
                    cmnDisplay.Items.Add(new ToolStripMenuItem
                            (key.ToString(), null, (_s, _e) =>
                                    {
                                        ParentConsoleDevice.ParentWorkplace.MainFormActivate();
                                        ParentConsoleDevice.ParentWorkplace.MainFormSelectIDE(((_s as ToolStripMenuItem)?.Tag as IIdeComponent)?.ParentIDE);
                                    })
                    {
                        Tag = key,
                        BackColor = System.Drawing.Color.FromArgb(255, 10, 10, 10),
                        ForeColor = DataClientsColors[(int)DataClients[key]]
                    });
                cmnDisplay.Items.Add("Clear Data Clients List", null, (_s, _e) =>
                {
                    this.BeginInvoke(new Action(() =>
                            {
                                ParentConsoleDevice.ClearDataClientsList();
                            }));
                });
            }
        }
    }
}
