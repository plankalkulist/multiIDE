using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Linq;
using System.ComponentModel;
using System.Drawing;
using multiIDE.IODevices;
using XSpace;
using multiIDE.Machines;

namespace multiIDE
{
    /// <summary>
    /// Application's main form.
    /// </summary>
    public sealed partial class mdiMultiIDE : Form, IMainForm
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "mdiMultiIDE";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Application's main form";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        #endregion

        #region Environment properties
        [Browsable(false)]
        public IWorkplace ParentWorkplace
        {
            get { return _ParentWorkplace; }
            set
            {
                _ParentWorkplace = value;
                _ParentWorkplace.GotUpdated += ParentWorkplace_GotUpdated;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        public string CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        public int Id { get; set; } = CommonConstants.UndefinedComponentId;
        //
        [Category("Appearance"), DefaultValue("")]
        public string Title
        {
            get
            {
                return (_CustomTitle.IsNotNullOrEmpty()) ? _CustomTitle
                        : $"{Application.ProductName} {Application.ProductVersion}{Program.VersionSuffix}";
            }
            set
            {
                _CustomTitle = value;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        object IComponent.Tag { get; set; }
        //
        private IWorkplace _ParentWorkplace;
        private string _CustomTitle = "";
        #endregion

        #region Essential properties
        [Category("Environment"), ReadOnly(true)]
        public IIDE SelectedIDE { get; private set; } = null;
        //
        [Category("Environment"), ReadOnly(true)]
        public List<string> RecentFiles
        {
            get
            {
                return (List<string>)this.Invoke(new Func<List<string>>(()
                        => mnuRecentProgramFiles_.DropDownItems.Cast<ToolStripItem>()
                        .Where(i => i.Tag?.ToString() == "IsFileName")
                        .Select(i => i.Text).ToList()));
            }
        }
        #endregion

        public override string ToString() => "multiIDE Main Form";

        /////
        private bool _SelectedMachineIsPausable;
        private bool _SelectedMachineIsSteppable;
        private bool _SelectedMachineIsStepOverable;
        private bool _SelectedMachineIsCloneable;
        private bool _SelectedMachineIsStateShottable;

        public mdiMultiIDE()
        {
            InitializeComponent();

            foreach (var stripItem in this.menuStrip.Items)
            {
                var menuItem = stripItem as ToolStripMenuItem;
                if (menuItem?.Tag?.ToString() == "NotImplemented")
                {
                    menuItem.BackColor = Color.DarkGray;
                    menuItem.ForeColor = Color.DarkRed;
                    menuItem.ToolTipText = "(is not implemented yet)";
                }
                else
                    foreach (var subItem in menuItem.DropDownItems)
                    {
                        var submenuItem = subItem as ToolStripMenuItem;
                        if (submenuItem?.Tag?.ToString() == "NotImplemented")
                        {
                            submenuItem.BackColor = Color.DarkGray;
                            submenuItem.ForeColor = Color.DarkRed;
                            submenuItem.ToolTipText = "(is not implemented yet)";
                        }
                    }
            }
            //
            mnuIOPorts_.DropDownOpening += (object sendie, EventArgs eie) =>
            {
                BuildIOportsMenu();
            };
        }

        #region Environment subs
        public void SelectIDE(IIDE ide)
        {
            if (this.InvokeRequired)
            {
                var a = new Action<IIDE>(_SelectIDE);
                this.Invoke(a, ide);
            }
            else
                _SelectIDE(ide);
        }

        public void ParentWorkplace_GotUpdated(object sender, EventArgs ne)
        {
            if (this.InvokeRequired)
            {
                var a = new EventHandler(_ParentWorkplace_GotUpdated);
                this.Invoke(a, sender, ne);
            }
            else
            {
                _ParentWorkplace_GotUpdated(sender, ne);
            }
        }

        public void SelectedIDE_GotUpdated(object sender, EventArgs ne)
        {
            if (this.InvokeRequired)
            {
                var a = new EventHandler(_SelectedIDE_GotUpdated);
                this.Invoke(a, sender, ne);
            }
            else
            {
                _SelectedIDE_GotUpdated(sender, ne);
            }
        }

        public void AssociatedMachine_StatusChanged(object sender, VirtualMachineStatusChangedEventArgs e)
        {
            if (this.InvokeRequired)
            {
                var a = new Action<object, VirtualMachineStatusChangedEventArgs>(_AssociatedMachine_StatusChanged);
                this.Invoke(a, sender, e);
            }
            else
            {
                _AssociatedMachine_StatusChanged(sender, e);
            }
        }

        public void AddRecentFile(string fileName)
        {
            if (this.InvokeRequired)
            {
                var a = new Action<string>(_AddRecentFile);
                this.Invoke(a, fileName);
            }
            else
                _AddRecentFile(fileName);
        }

        public new void Activate()
        {
            if (this.InvokeRequired)
            {
                var a = new Action(base.Activate);
                this.Invoke(a);
            }
            else
            {
                base.Activate();
            }
        }

        public void UpdateTitle()
        {
            if (this.InvokeRequired)
            {
                var a = new Action(_UpdateTitle);
                this.Invoke(a);
            }
            else
            {
                _UpdateTitle();
            }
        }

        // // //

        private void _SelectIDE(IIDE ide)
        {
            if (ide != null)
            {
                mnuNewProgramFile.Enabled = true;
                mnuOpenProgramFile.Enabled = true;
                mntOpenProgramFile.Enabled = mnuOpenProgramFile.Enabled;
                mnuSaveProgramFile.Enabled = !ide.ProgramFile.IsReadOnly;
                mntSaveProgramFile.Enabled = mnuSaveProgramFile.Enabled;
                mnuSaveProgramFileAs.Enabled = true;
                mnuSaveIDE.Enabled = !ide.ConfigFile.IsReadOnly;
                mnuSaveIDEas.Enabled = true;
                mnuCloseIDE.Enabled = true;
                mnuCloseThisIDE.Enabled = true;
                mnuRecentProgramFiles_.Enabled = true;

                mnuEdit_.Visible = true;

                mnuIDE_.Visible = true;
                if (ParentWorkplace.IDEs.Count > 1)
                {
                    mnuMinimizeAllExceptThis.Enabled = true;
                    mnuCloseAllExceptThis.Enabled = true;
                }
                else
                {
                    mnuMinimizeAllExceptThis.Enabled = false;
                    mnuCloseAllExceptThis.Enabled = false;
                }

                var codeEditorForm = ide.CodeEditor as Form;
                if (codeEditorForm != null)
                {
                    codeEditorForm.Show();
                    if (codeEditorForm.WindowState == FormWindowState.Minimized)
                        codeEditorForm.WindowState = FormWindowState.Normal;
                    codeEditorForm.Activate();
                }

                if (SelectedIDE != null)
                {
                    SelectedIDE.GotUpdated -= this.SelectedIDE_GotUpdated;
                    SelectedIDE.Machine.StatusChanged -= this.AssociatedMachine_StatusChanged;
                }
                ide.GotUpdated += this.SelectedIDE_GotUpdated;
                ide.Machine.StatusChanged += this.AssociatedMachine_StatusChanged;

                SelectedIDE = ide;

                if (ide.Machine != null)
                {
                    _SelectedMachineIsPausable = SelectedIDE.Machine is IPausable;
                    _SelectedMachineIsSteppable = SelectedIDE.Machine is ISteppable;
                    _SelectedMachineIsStepOverable = SelectedIDE.Machine is IStepOverable;
                    _SelectedMachineIsCloneable = SelectedIDE.Machine is ICloneable;
                    _SelectedMachineIsStateShottable = SelectedIDE.Machine is IStateShottable;

                    mnuRun_.Visible = true;
                    mchAutoResetOnStart.Checked = ide.AutoResetOnStart;
                    mnuPause.Visible = _SelectedMachineIsPausable;
                    mnuStep.Visible = _SelectedMachineIsSteppable;
                    mnuStepOver.Visible = _SelectedMachineIsStepOverable;

                    mnuMachine_.Visible = true;
                    mnuIOPorts_.DropDownItems.Clear();
                    mnuIOPorts_.DropDownItems.Add("...");
                    mnuIOPorts_.DropDownItems[0].Enabled = false;
                    mnuCloneMachine.Enabled = _SelectedMachineIsCloneable;
                    mnuCopyState.Visible = _SelectedMachineIsStateShottable;
                    mnuPasteState.Visible = _SelectedMachineIsStateShottable;
                    sprSaveLoadMachineState.Visible = _SelectedMachineIsStateShottable;
                    mnuSaveState.Visible = _SelectedMachineIsStateShottable;
                    mnuSaveStateAs.Visible = _SelectedMachineIsStateShottable;
                    mnuLoadState.Visible = _SelectedMachineIsStateShottable;
                }
                else
                {
                    mnuRun_.Visible = false;
                    mnuMachine_.Visible = false;
                    throw new NullReferenceException();
                }
            }
            else
            {
                mnuNewProgramFile.Enabled = false;
                mnuOpenProgramFile.Enabled = false;
                mntOpenProgramFile.Enabled = false;
                mnuSaveProgramFile.Enabled = false;
                mntSaveProgramFile.Enabled = mnuSaveProgramFile.Enabled;
                mnuSaveProgramFileAs.Enabled = false;
                mnuSaveIDE.Enabled = false;
                mnuSaveIDEas.Enabled = false;
                mnuCloseIDE.Enabled = false;
                mnuCloseThisIDE.Enabled = false;
                mnuRecentProgramFiles_.Enabled = false;

                mnuEdit_.Visible = false;

                mnuRun_.Visible = false;
                mnuMachine_.Visible = false;

                mnuIDE_.Visible = false;

                if (SelectedIDE != null)
                {
                    SelectedIDE.GotUpdated -= this.SelectedIDE_GotUpdated;
                    SelectedIDE.Machine.StatusChanged -= AssociatedMachine_StatusChanged;
                }

                SelectedIDE = null;
            }

            if (ParentWorkplace.IsInitialized)
            {
                if (SelectedIDE != null)
                {
                    lblStatus.Text = "Selected IDE : " + SelectedIDE.Title;
                    _SelectedIDE_GotUpdated(this, EventArgs.Empty);
                    _AssociatedMachine_StatusChanged(this, new VirtualMachineStatusChangedEventArgs(SelectedIDE.Machine.Status
                            , SelectedIDE.Machine.NextSymbol));
                }
                else
                {
                    lblStatus.Text = "No IDE selected";
                    _UpdateTitle();
                }
            }
            else
            {
                lblStatus.Text = "No Workplace initialized";
                UpdateTitle();
            }
        }

        private void _ParentWorkplace_GotUpdated(object sender, EventArgs ne)
        {
            if (ParentWorkplace.IsInitialized)
            {
                mnuNewIDE.Enabled = true;
                mntNewIDE.Enabled = mnuNewIDE.Enabled;
                mnuOpenIDE.Enabled = true;
                mnuSaveWorkplace.Enabled = true;
                mnuSaveWorkplaceAs.Enabled = true;
                mnuSaveAll.Enabled = true;
                mnuCloseWorkplace.Enabled = true;

                mnuWorkplace_.Visible = true;
            }
            else
            {
                mnuNewIDE.Enabled = false;
                mntNewIDE.Enabled = mnuNewIDE.Enabled;
                mnuOpenIDE.Enabled = false;
                mnuSaveWorkplace.Enabled = false;
                mnuSaveWorkplaceAs.Enabled = false;
                mnuSaveAll.Enabled = false;
                mnuCloseWorkplace.Enabled = false;

                mnuWorkplace_.Visible = false;

                _SelectIDE(null);
            }
        }

        private void _SelectedIDE_GotUpdated(object sender, EventArgs ne)
        {
            _UpdateTitle();
        }

        private void _AssociatedMachine_StatusChanged(object sender, VirtualMachineStatusChangedEventArgs e)
        {
            switch (e.NewStatus)
            {
                case VirtualMachineRunningStatus.StandBy:
                    mnuStart.Text = "Start";
                    mnuStart.Enabled = true;
                    if (_SelectedMachineIsPausable) mnuPause.Enabled = false;
                    if (_SelectedMachineIsSteppable) mnuStep.Enabled = true;
                    if (_SelectedMachineIsStepOverable) mnuStepOver.Enabled = true;
                    mnuBreak.Enabled = false;
                    mnuTerminate.Enabled = false;
                    if (_SelectedMachineIsCloneable) mnuCloneMachine.Enabled = true;
                    if (_SelectedMachineIsStateShottable)
                    {
                        mnuCopyState.Enabled = true;
                        mnuPasteState.Enabled = true;
                        mnuSaveState.Enabled = !SelectedIDE.MachineStateFile.IsReadOnly;
                        mnuSaveStateAs.Enabled = true;
                        mnuLoadState.Enabled = true;
                    }
                    break;
                case VirtualMachineRunningStatus.Runtime:
                    mnuStart.Text = "Continue";
                    mnuStart.Enabled = false;
                    if (_SelectedMachineIsPausable) mnuPause.Enabled = true;
                    if (_SelectedMachineIsSteppable) mnuStep.Enabled = false;
                    if (_SelectedMachineIsStepOverable) mnuStepOver.Enabled = false;
                    mnuBreak.Enabled = true;
                    mnuTerminate.Enabled = true;
                    if (_SelectedMachineIsCloneable) mnuCloneMachine.Enabled = false;
                    if (_SelectedMachineIsStateShottable)
                    {
                        mnuCopyState.Enabled = false;
                        mnuPasteState.Enabled = false;
                        mnuSaveState.Enabled = false;
                        mnuSaveStateAs.Enabled = false;
                        mnuLoadState.Enabled = false;
                    }
                    break;
                case VirtualMachineRunningStatus.Paused:
                    mnuStart.Text = "Continue";
                    mnuStart.Enabled = true;
                    if (_SelectedMachineIsPausable) mnuPause.Enabled = false;
                    if (_SelectedMachineIsSteppable) mnuStep.Enabled = true;
                    if (_SelectedMachineIsStepOverable) mnuStepOver.Enabled = true;
                    mnuBreak.Enabled = true;
                    mnuTerminate.Enabled = true;
                    if (_SelectedMachineIsCloneable) mnuCloneMachine.Enabled = true;
                    if (_SelectedMachineIsStateShottable)
                    {
                        mnuCopyState.Enabled = true;
                        mnuPasteState.Enabled = true;
                        mnuSaveState.Enabled = !SelectedIDE.MachineStateFile.IsReadOnly;
                        mnuSaveStateAs.Enabled = true;
                        mnuLoadState.Enabled = true;
                    }
                    break;
                case VirtualMachineRunningStatus.Breaking:
                    mnuStart.Enabled = false;
                    if (_SelectedMachineIsPausable) mnuPause.Enabled = false;
                    if (_SelectedMachineIsSteppable) mnuStep.Enabled = false;
                    if (_SelectedMachineIsStepOverable) mnuStepOver.Enabled = false;
                    mnuBreak.Enabled = false;
                    mnuTerminate.Enabled = true;
                    if (_SelectedMachineIsCloneable) mnuCloneMachine.Enabled = false;
                    if (_SelectedMachineIsStateShottable)
                    {
                        mnuCopyState.Enabled = false;
                        mnuPasteState.Enabled = false;
                        mnuSaveState.Enabled = false;
                        mnuSaveStateAs.Enabled = false;
                        mnuLoadState.Enabled = false;
                    }
                    break;
                case VirtualMachineRunningStatus.Pausing:
                case VirtualMachineRunningStatus.Stepping:
                case VirtualMachineRunningStatus.SteppingOver:
                    mnuTerminate.Enabled = true;
                    break;
                default:
                    mnuTerminate.Enabled = true;
                    break;
            }

            _UpdateTitle();
        }

        private void _AddRecentFile(string fileName)
        {
            if (mnuRecentProgramFiles_.DropDownItems[0].Text == "None")
            {
                mnuRecentProgramFiles_.DropDownItems.Clear();
                mnuRecentProgramFiles_.DropDownItems.Add(new ToolStripSeparator());
                mnuRecentProgramFiles_.DropDownItems.Add(new ToolStripMenuItem("Clear", null,
                    (object sender, EventArgs e) =>
                    {
                        mnuRecentProgramFiles_.DropDownItems.Clear();
                        mnuRecentProgramFiles_.DropDownItems.Add("None").Enabled = false;
                    }
                    , Keys.Control & Keys.Q));
            }

            mnuRecentProgramFiles_.DropDownItems.Insert(0, new ToolStripMenuItem
                    (fileName, null, mnuRecentFilesItem_Click)
            { Tag = "IsFileName" });
        }

        private void _UpdateTitle()
        {
            if (ParentWorkplace.IsInitialized)
            {
                Title = "";
                this.Text =
                        $"{ParentWorkplace.Title}"
                        + $" ({ParentWorkplace.IDEs.Count(i => i.Machine.Status != VirtualMachineRunningStatus.StandBy)}"
                        + $"/{ParentWorkplace.IDEs.Count}) - {this.Title}";
            }
            else
            {
                Title = "";
                this.Text = Title;
            }
        }
        #endregion

        #region Main Menu subs
        private void BuildIOportsMenu(int specifiedIOportMenuItemIndex = -3)
        {
            if (SelectedIDE.Machine != null)
            {
                mnuMachine_.Visible = true;

                IMultiIOable multiIOableMachine = SelectedIDE.Machine as IMultiIOable;
                int portMenuItemIndex = -3;
                int startPortIndex, lastPortIndex;
                if (specifiedIOportMenuItemIndex == -3)
                {
                    mnuIOPorts_.DropDownItems.Clear();
                    portMenuItemIndex = 0;
                    if (multiIOableMachine != null)
                    {
                        startPortIndex = 0;
                        lastPortIndex = 255;
                    }
                    else
                    {
                        startPortIndex = -1; // ActionIOport index
                        lastPortIndex = -1;
                    }
                }
                else if (0 <= specifiedIOportMenuItemIndex && specifiedIOportMenuItemIndex <= 255)
                {
                    portMenuItemIndex = specifiedIOportMenuItemIndex;
                    startPortIndex = (int)(((object[])(mnuIOPorts_.DropDownItems[specifiedIOportMenuItemIndex].Tag))[4]);
                    lastPortIndex = startPortIndex;
                    mnuIOPorts_.DropDownItems.RemoveAt(portMenuItemIndex);
                }
                else
                {
                    throw new ArgumentException("Invalid index");
                }

                if (portMenuItemIndex == 0)
                {
                    mnuIOPorts_.DropDownItems.Add("-"); // placing before IO ports to avoid menu running to upper-left corner of the screen
                    portMenuItemIndex++;
                }

                IInputPort currentInputPort;
                IOutputPort currentOutputPort;
                for (int portIndex = startPortIndex; portIndex <= lastPortIndex; portIndex++)
                {
                    if (portIndex == -1)
                    {
                        currentInputPort = SelectedIDE?.Machine?.ActionInputPort ?? null;
                        currentOutputPort = SelectedIDE?.Machine?.ActionOutputPort ?? null;
                    }
                    else
                    {
                        currentInputPort = multiIOableMachine?.InputPorts[portIndex] ?? null;
                        currentOutputPort = multiIOableMachine?.OutputPorts[portIndex] ?? null;
                    }
                    //
                    if (currentInputPort != null || currentOutputPort != null
                        || portIndex == (multiIOableMachine?.ActionInputPortIndex ?? CommonConstants.UndefinedComponentId)
                        || portIndex == (multiIOableMachine?.ActionOutputPortIndex ?? CommonConstants.UndefinedComponentId))
                    {
                        ToolStripMenuItem mnuIOport = new ToolStripMenuItem("(loading...)");
                        if (multiIOableMachine != null)
                        {
                            mnuIOport.Text = $"{"#" + portIndex.ToString(),3}: "
                                             + ((portIndex == multiIOableMachine.ActionInputPortIndex) ? "*" : " ")
                                             + $" {currentInputPort?.Title ?? "--no input port--",-30}"
                                             + " / "
                                             + ((portIndex == multiIOableMachine.ActionOutputPortIndex) ? "*" : " ")
                                             + $" {currentOutputPort?.Title ?? "--no output port--",-30}";
                            mnuIOport.ToolTipText = (((currentInputPort?.Id ?? CommonConstants.UndefinedComponentId) == multiIOableMachine.ActionInputPortIndex)
                                                        ? "This Input Port is ActionInputPort of the machine at the moment. " : "")
                                                    + (((currentOutputPort?.Id ?? CommonConstants.UndefinedComponentId) == multiIOableMachine.ActionOutputPortIndex)
                                                        ? "This Output Port is ActionOutputPort of the machine at the moment." : "");
                        }
                        else
                        {
                            mnuIOport.Text = $"   {currentInputPort?.Title ?? "--no input port--",-30}"
                                             + " / "
                                             + $"  {currentOutputPort?.Title ?? "--no output port--",-30}";
                        }
                        //
                        // Building Input Device submenu
                        //
                        ToolStripMenuItem mnuInputDevice = new ToolStripMenuItem("(InputDevice)")
                        { Tag = new object[] { mnuIOport, portMenuItemIndex, currentInputPort } };

                        if (currentInputPort != null)
                        {
                            mnuInputDevice.DropDownItems.Add(new ToolStripMenuItem("(loading...)")
                            { Enabled = false });
                            EventHandler inputDeviceSubmenuOpening = (object _mnuInputDeviceSender, EventArgs _inputDeviceSubmenuOpeningArgs) =>
                            {
                                ToolStripMenuItem _inputDeviceMenuItem = _mnuInputDeviceSender as ToolStripMenuItem;
                                IInputPort _currentInputPort = (IInputPort)(((object[])(_inputDeviceMenuItem.Tag))[2]);

                                _inputDeviceMenuItem.DropDownItems.Clear();

                                if (_currentInputPort.InputDevice != null)
                                {
                                    if (!_currentInputPort.InputDevice.IsInitialized
                                        || _currentInputPort.InputDevice as IReinitializeable != null)
                                    {
                                        _inputDeviceMenuItem.DropDownItems.Add(
                                            new ToolStripMenuItem((!_currentInputPort.InputDevice.IsInitialized ? "I" : "Rei")
                                                                  + "nitialize By multiIDE Form", null,
                                                    (object __sender, EventArgs __e) =>
                                                    {
                                                        ToolStripMenuItem __parentInputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                        int __portMenuItemIndex = (int)(((object[])__parentInputDeviceMenuItem.Tag)[1]);
                                                        IInputPort __inputPort = (IInputPort)(((object[])(__parentInputDeviceMenuItem.Tag))[2]);
                                                        //hiding old menu
                                                        __parentInputDeviceMenuItem.DropDownItems.Clear();
                                                        (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = false;
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = false;
                                                        //
                                                        ParentWorkplace.InitializeIOdeviceInIndividualThread(__inputPort.InputDevice);
                                                        // showing back menu
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = true;
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = true;
                                                        //
                                                        BuildIOportsMenu(__portMenuItemIndex);
                                                    })
                                            { Tag = new object[] { _inputDeviceMenuItem } });
                                        (_inputDeviceMenuItem.DropDownItems[_inputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                            .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    }

                                    _inputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem("Settings...", null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentInputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                IInputPort __inputPort = (IInputPort)(((object[])(__parentInputDeviceMenuItem.Tag))[2]);
                                                ParentWorkplace.InitializeNewSettingWindow(__inputPort.InputDevice);
                                            })
                                    { Tag = new object[] { _inputDeviceMenuItem } });
                                    (_inputDeviceMenuItem.DropDownItems[_inputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });

                                    Type _inputDeviceType = _currentInputPort.InputDevice.GetType();
                                    MethodInfo[] _inputDeviceMethods = _inputDeviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                                        .Where(m => m.ReturnType == typeof(void) && m.GetParameters().ToList().Count == 0 && m.GetCustomAttribute<MenuBrowsableAttribute>() != null
                                                    && (m.GetCustomAttribute<MenuBrowsableAttribute>()).Browsable).ToArray();
                                    if (_inputDeviceMethods.Length > 0)
                                        _inputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());
                                    foreach (MethodInfo _inputDeviceMethodInfo in _inputDeviceMethods)
                                    {
                                        _inputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem(_inputDeviceMethodInfo.Name, null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentInputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                int __portMenuItemIndex = (int)(((object[])__parentInputDeviceMenuItem.Tag)[1]);
                                                IInputPort __inputPort = (IInputPort)(((object[])(__parentInputDeviceMenuItem.Tag))[2]);
                                                MethodInfo __inputDeviceMethodInfo = (MethodInfo)((object[])((__sender as ToolStripMenuItem).Tag))[1];
                                                //hiding old menu
                                                __parentInputDeviceMenuItem.DropDownItems.Clear();
                                                (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = false;
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = false;
                                                //
                                                bool __locked = false;
                                                if (__inputDeviceMethodInfo.GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? false) IOmonitor.IdleForLock(__inputPort.InputDevice, out __locked);
                                                __inputDeviceMethodInfo.Invoke(__inputPort.InputDevice, null);
                                                if (__locked) IOmonitor.Unlock(__inputPort.InputDevice);
                                                // showing back menu
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = true;
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = true;
                                                //
                                                BuildIOportsMenu(__portMenuItemIndex);
                                            })
                                        {
                                            Tag = new object[] { _inputDeviceMenuItem, _inputDeviceMethodInfo },
                                            Enabled = !(_inputDeviceMethodInfo.GetCustomAttribute<AvailableWhenInitializedOnlyAttribute>()?.Checking ?? false)
                                                      || _currentInputPort.InputDevice.IsInitialized,
                                            ToolTipText = _inputDeviceMethodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                                        });
                                        (_inputDeviceMenuItem.DropDownItems[_inputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                            .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    }
                                    _inputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                    _inputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem("Disconnect", null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentInputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                int __portMenuItemIndex = (int)(((object[])__parentInputDeviceMenuItem.Tag)[1]);
                                                IInputPort __inputPort = (IInputPort)(((object[])(__parentInputDeviceMenuItem.Tag))[2]);
                                                //hiding old menu
                                                __parentInputDeviceMenuItem.DropDownItems.Clear();
                                                (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = false;
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = false;
                                                //
                                                __inputPort.ParentIDE.MachineSetInputDevice(__inputPort, null);
                                                // showing back menu
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = true;
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = true;
                                                //
                                                BuildIOportsMenu(__portMenuItemIndex);
                                            })
                                    { Tag = new object[] { _inputDeviceMenuItem } });
                                    (_inputDeviceMenuItem.DropDownItems[_inputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    _inputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());
                                }

                                _inputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem("Connect To:")
                                { Enabled = false });
                                foreach (InputDeviceTypeInfo _inputDeviceTypeInfo in ParentWorkplace.InputDeviceBuilder.VisibleTypes)
                                {
                                    _inputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem($"New {_inputDeviceTypeInfo.DefaultName}", null,
                                        (object __sender, EventArgs __e) =>
                                        {
                                            ToolStripMenuItem __parentInputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentInputDeviceMenuItem.Tag)[1]);
                                            IInputPort __inputPort = (IInputPort)(((object[])(__parentInputDeviceMenuItem.Tag))[2]);
                                            InputDeviceTypeInfo __inputDeviceTypeInfo = (InputDeviceTypeInfo)((object[])((__sender as ToolStripMenuItem).Tag))[1];
                                            //hiding menu
                                            __parentInputDeviceMenuItem.DropDownItems.Clear();
                                            (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = false;
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = false;
                                            //
                                            IInputDevice __newInputDevice;
                                            if (ParentWorkplace.NewInputDevice(__inputDeviceTypeInfo, out __newInputDevice))
                                                __inputPort.ParentIDE.MachineSetInputDevice(__inputPort, __newInputDevice);
                                            // showing back menu
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = true;
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = true;
                                            //
                                            BuildIOportsMenu(__portMenuItemIndex);
                                        })
                                    {
                                        Tag = new object[] { _inputDeviceMenuItem, _inputDeviceTypeInfo },
                                        ToolTipText = $"{_inputDeviceTypeInfo.TypeName}   Version: {_inputDeviceTypeInfo.Version + (_inputDeviceTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {_inputDeviceTypeInfo.Author}" : "")}\n   from {_inputDeviceTypeInfo.SourceFileName.ShrinkFileName(65)}\n{_inputDeviceTypeInfo.Description}"
                                    });
                                    (_inputDeviceMenuItem.DropDownItems[_inputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                }

                                if ((ParentWorkplace.InputDevices.Count > 1 && _currentInputPort.InputDevice != null)
                                    || (ParentWorkplace.InputDevices.Count > 0 && _currentInputPort.InputDevice == null))
                                {
                                    _inputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());
                                    foreach (IInputDevice _inputDevice in ParentWorkplace.InputDevices)
                                    {
                                        if (_inputDevice != _currentInputPort.InputDevice)
                                        {
                                            _inputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem(_inputDevice.Title, null,
                                                    (object __sender, EventArgs __e) =>
                                                    {
                                                        ToolStripMenuItem __parentInputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                        int __portMenuItemIndex = (int)(((object[])__parentInputDeviceMenuItem.Tag)[1]);
                                                        IInputPort __inputPort = (IInputPort)(((object[])(__parentInputDeviceMenuItem.Tag))[2]);
                                                        IInputDevice __inputDevice = (IInputDevice)((object[])((__sender as ToolStripMenuItem).Tag))[1];
                                                        //hiding old menu
                                                        __parentInputDeviceMenuItem.DropDownItems.Clear();
                                                        (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = false;
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = false;
                                                        //
                                                        __inputPort.ParentIDE.MachineSetInputDevice(__inputPort, __inputDevice);
                                                        // showing back menu
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = true;
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = true;
                                                        //
                                                        BuildIOportsMenu(__portMenuItemIndex);
                                                    })
                                            { Tag = new object[] { _inputDeviceMenuItem, _inputDevice } });
                                            (_inputDeviceMenuItem.DropDownItems[_inputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                                .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                        }
                                    }
                                }
                            };
                            mnuInputDevice.DropDownOpening += inputDeviceSubmenuOpening;
                        }
                        //
                        // Building Output Device submenu
                        //
                        ToolStripMenuItem mnuOutputDevice = new ToolStripMenuItem("(OutputDevice)")
                        { Tag = new object[] { mnuIOport, portMenuItemIndex, currentOutputPort } };

                        if (currentOutputPort != null)
                        {
                            mnuOutputDevice.DropDownItems.Add(new ToolStripMenuItem("(loading...)")
                            { Enabled = false });
                            EventHandler outputDeviceSubmenuOpening = (object _mnuOutputDeviceSender, EventArgs _outputDeviceSubmenuOpeningArgs) =>
                            {
                                ToolStripMenuItem _outputDeviceMenuItem = _mnuOutputDeviceSender as ToolStripMenuItem;
                                IOutputPort _currentOutputPort = (IOutputPort)(((object[])(_outputDeviceMenuItem.Tag))[2]);

                                _outputDeviceMenuItem.DropDownItems.Clear();

                                if (_currentOutputPort.OutputDevice != null)
                                {
                                    if (!_currentOutputPort.OutputDevice.IsInitialized
                                        || _currentOutputPort.OutputDevice as IReinitializeable != null)
                                    {
                                        _outputDeviceMenuItem.DropDownItems.Add(
                                            new ToolStripMenuItem((!_currentOutputPort.OutputDevice.IsInitialized ? "I" : "Rei")
                                                                  + "nitialize By multiIDE Form", null,
                                                    (object __sender, EventArgs __e) =>
                                                    {
                                                        ToolStripMenuItem __parentOutputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                        int __portMenuItemIndex = (int)(((object[])__parentOutputDeviceMenuItem.Tag)[1]);
                                                        IOutputPort __outputPort = (IOutputPort)(((object[])(__parentOutputDeviceMenuItem.Tag))[2]);
                                                        //hiding old menu
                                                        __parentOutputDeviceMenuItem.DropDownItems.Clear();
                                                        (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = false;
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = false;
                                                        //
                                                        ParentWorkplace.InitializeIOdeviceInIndividualThread(__outputPort.OutputDevice);
                                                        // showing back menu
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = true;
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = true;
                                                        //
                                                        BuildIOportsMenu(__portMenuItemIndex);
                                                    })
                                            { Tag = new object[] { _outputDeviceMenuItem } });
                                        (_outputDeviceMenuItem.DropDownItems[_outputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                            .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    }

                                    _outputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem("Settings...", null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentOutputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                IOutputPort __outputPort = (IOutputPort)(((object[])(__parentOutputDeviceMenuItem.Tag))[2]);
                                                ParentWorkplace.InitializeNewSettingWindow(__outputPort.OutputDevice);
                                            })
                                    { Tag = new object[] { _outputDeviceMenuItem } });
                                    (_outputDeviceMenuItem.DropDownItems[_outputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });

                                    Type _outputDeviceType = _currentOutputPort.OutputDevice.GetType();
                                    MethodInfo[] _outputDeviceMethods = _outputDeviceType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                                        .Where(m => m.ReturnType == typeof(void) && m.GetParameters().ToList().Count == 0 && m.GetCustomAttribute<MenuBrowsableAttribute>() != null
                                                    && (m.GetCustomAttribute<MenuBrowsableAttribute>()).Browsable).ToArray();
                                    if (_outputDeviceMethods.Length > 0)
                                        _outputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());
                                    foreach (MethodInfo _outputDeviceMethodInfo in _outputDeviceMethods)
                                    {
                                        _outputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem(_outputDeviceMethodInfo.Name, null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentOutputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                int __portMenuItemIndex = (int)(((object[])__parentOutputDeviceMenuItem.Tag)[1]);
                                                IOutputPort __outputPort = (IOutputPort)(((object[])(__parentOutputDeviceMenuItem.Tag))[2]);
                                                MethodInfo __outputDeviceMethodInfo = (MethodInfo)((object[])((__sender as ToolStripMenuItem).Tag))[1];
                                                //hiding old menu
                                                __parentOutputDeviceMenuItem.DropDownItems.Clear();
                                                (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = false;
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = false;
                                                //
                                                bool __locked = false;
                                                if (__outputDeviceMethodInfo.GetCustomAttribute<LockingRequiredAttribute>()?.IsLockingRequired ?? false) IOmonitor.IdleForLock(__outputPort.OutputDevice, out __locked);
                                                __outputDeviceMethodInfo.Invoke(__outputPort.OutputDevice, null);
                                                if (__locked) IOmonitor.Unlock(__outputPort.OutputDevice);
                                                // showing back menu
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = true;
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = true;
                                                //
                                                BuildIOportsMenu(__portMenuItemIndex);
                                            })
                                        {
                                            Tag = new object[] { _outputDeviceMenuItem, _outputDeviceMethodInfo },
                                            Enabled = !(_outputDeviceMethodInfo.GetCustomAttribute<AvailableWhenInitializedOnlyAttribute>()?.Checking ?? false)
                                                      || _currentOutputPort.OutputDevice.IsInitialized,
                                            ToolTipText = _outputDeviceMethodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                                        });
                                        (_outputDeviceMenuItem.DropDownItems[_outputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                            .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    }
                                    _outputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                    _outputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem("Disconnect", null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentOutputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                int __portMenuItemIndex = (int)(((object[])__parentOutputDeviceMenuItem.Tag)[1]);
                                                IOutputPort __outputPort = (IOutputPort)(((object[])(__parentOutputDeviceMenuItem.Tag))[2]);
                                                //hiding old menu
                                                __parentOutputDeviceMenuItem.DropDownItems.Clear();
                                                (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = false;
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = false;
                                                //
                                                __outputPort.ParentIDE.MachineSetOutputDevice(__outputPort, null);
                                                // showing back menu
                                                foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                    __mnuDropDownItem0.Visible = true;
                                                foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                    __mnuDropDownItem1.Visible = true;
                                                //
                                                BuildIOportsMenu(__portMenuItemIndex);
                                            })
                                    { Tag = new object[] { _outputDeviceMenuItem } });
                                    (_outputDeviceMenuItem.DropDownItems[_outputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    _outputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());
                                }

                                _outputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem("Connect To:")
                                { Enabled = false });
                                foreach (OutputDeviceTypeInfo _outputDeviceTypeInfo in ParentWorkplace.OutputDeviceBuilder.VisibleTypes)
                                {
                                    _outputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem($"New {_outputDeviceTypeInfo.DefaultName}", null,
                                        (object __sender, EventArgs __e) =>
                                        {
                                            ToolStripMenuItem __parentOutputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentOutputDeviceMenuItem.Tag)[1]);
                                            IOutputPort __outputPort = (IOutputPort)(((object[])(__parentOutputDeviceMenuItem.Tag))[2]);
                                            OutputDeviceTypeInfo __outputDeviceTypeInfo = (OutputDeviceTypeInfo)((object[])((__sender as ToolStripMenuItem).Tag))[1];
                                            //hiding menu
                                            __parentOutputDeviceMenuItem.DropDownItems.Clear();
                                            (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = false;
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = false;
                                            //
                                            IOutputDevice __newOutputDevice;
                                            if (ParentWorkplace.NewOutputDevice(__outputDeviceTypeInfo, out __newOutputDevice))
                                                __outputPort.ParentIDE.MachineSetOutputDevice(__outputPort, __newOutputDevice);
                                            // showing back menu
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = true;
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = true;
                                            //
                                            BuildIOportsMenu(__portMenuItemIndex);
                                        })
                                    {
                                        Tag = new object[] { _outputDeviceMenuItem, _outputDeviceTypeInfo },
                                        ToolTipText = $"{_outputDeviceTypeInfo.TypeName}   Version: {_outputDeviceTypeInfo.Version + (_outputDeviceTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {_outputDeviceTypeInfo.Author}" : "")}\n   from {_outputDeviceTypeInfo.SourceFileName.ShrinkFileName(65)}\n{_outputDeviceTypeInfo.Description}"
                                    });
                                    (_outputDeviceMenuItem.DropDownItems[_outputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                }

                                if ((ParentWorkplace.OutputDevices.Count > 1 && _currentOutputPort.OutputDevice != null)
                                    || (ParentWorkplace.OutputDevices.Count > 0 && _currentOutputPort.OutputDevice == null))
                                {
                                    _outputDeviceMenuItem.DropDownItems.Add(new ToolStripSeparator());
                                    foreach (IOutputDevice _outputDevice in ParentWorkplace.OutputDevices)
                                    {
                                        if (_outputDevice != _currentOutputPort.OutputDevice)
                                        {
                                            _outputDeviceMenuItem.DropDownItems.Add(new ToolStripMenuItem(_outputDevice.Title, null,
                                                    (object __sender, EventArgs __e) =>
                                                    {
                                                        ToolStripMenuItem __parentOutputDeviceMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                        int __portMenuItemIndex = (int)(((object[])__parentOutputDeviceMenuItem.Tag)[1]);
                                                        IOutputPort __outputPort = (IOutputPort)(((object[])(__parentOutputDeviceMenuItem.Tag))[2]);
                                                        IOutputDevice __outputDevice = (IOutputDevice)((object[])((__sender as ToolStripMenuItem).Tag))[1];
                                                        //hiding old menu
                                                        __parentOutputDeviceMenuItem.DropDownItems.Clear();
                                                        (mnuIOPorts_.DropDownItems[__portMenuItemIndex] as ToolStripMenuItem).DropDownItems.Clear();
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = false;
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = false;
                                                        //
                                                        __outputPort.ParentIDE.MachineSetOutputDevice(__outputPort, __outputDevice);
                                                        // showing back menu
                                                        foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                            __mnuDropDownItem0.Visible = true;
                                                        foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                            __mnuDropDownItem1.Visible = true;
                                                        //
                                                        BuildIOportsMenu(__portMenuItemIndex);
                                                    })
                                            { Tag = new object[] { _outputDeviceMenuItem, _outputDevice } });
                                            (_outputDeviceMenuItem.DropDownItems[_outputDeviceMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                                .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                        }
                                    }
                                }
                            };
                            mnuOutputDevice.DropDownOpening += outputDeviceSubmenuOpening;
                        }
                        //
                        // Building IO port submenu
                        //
                        mnuIOport.Tag = new object[] { portMenuItemIndex, currentInputPort, currentOutputPort, SelectedIDE, portIndex, mnuInputDevice, mnuOutputDevice };

                        mnuIOport.DropDownItems.Add(new ToolStripMenuItem("(loading...)")
                        { Enabled = false });
                        EventHandler ioPortSubmenuOpening = (object _mnuIOportSender, EventArgs _ioPortSubmenuOpeningArgs) =>
                        {
                            ToolStripMenuItem _ioPortMenuItem = _mnuIOportSender as ToolStripMenuItem;
                            IInputPort _currentInputPort = (IInputPort)(((object[])(_ioPortMenuItem.Tag))[1]);
                            IOutputPort _currentOutputPort = (IOutputPort)(((object[])(_ioPortMenuItem.Tag))[2]);
                            IIDE _ide = (IIDE)(((object[])(_ioPortMenuItem.Tag))[3]);
                            int _portIndex = (int)(((object[])(_ioPortMenuItem.Tag))[4]);
                            ToolStripMenuItem _inputDeviceMenuItem = (ToolStripMenuItem)(((object[])(_ioPortMenuItem.Tag))[5]);
                            ToolStripMenuItem _outputDeviceMenuItem = (ToolStripMenuItem)(((object[])(_ioPortMenuItem.Tag))[6]);

                            _ioPortMenuItem.DropDownItems.Clear();

                            _inputDeviceMenuItem.Text = "In    : " + (_currentInputPort?.InputDevice?.Title ?? _currentInputPort?.Title ?? "--no input port--");
                            mnuIOport.DropDownItems.Add(_inputDeviceMenuItem);
                            //
                            if (_currentInputPort != null)
                            {
                                mnuIOport.DropDownItems.Add(new ToolStripSeparator());

                                ToolStripMenuItem _mnuInputConverting = new ToolStripMenuItem
                                    ($"Input Converting     : {_currentInputPort.InputConverting.ToString()}", null, null, "InputConverting");
                                Type _inputConvertingsType = _currentInputPort.InputConverting.GetType();
                                if (_inputConvertingsType.IsEnum)
                                {
                                    foreach (var _inputConverting in Enum.GetValues(_inputConvertingsType))
                                    {
                                        _mnuInputConverting.DropDownItems.Add(new ToolStripMenuItem(_inputConverting.ToString(), null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                                IInputPort __inputPort = (IInputPort)(((object[])(__parentIOportMenuItem.Tag))[1]);
                                                int __inputConverting = (int)(((object[])((__sender as ToolStripMenuItem).Tag))[1]);
                                                __inputPort.InputConverting = __inputConverting;
                                                //hiding old menu
                                                ToolStripMenuItem __parentInputConvertingMenuItem = __parentIOportMenuItem.DropDownItems["InputConverting"] as ToolStripMenuItem;
                                                foreach (ToolStripItem inputConvertingDropDownItem in __parentInputConvertingMenuItem.DropDownItems)
                                                    inputConvertingDropDownItem.Visible = false;
                                                foreach (ToolStripItem inputDeviceDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                    inputDeviceDropDownItem.Visible = false;
                                                //
                                                BuildIOportsMenu(__portMenuItemIndex);
                                            })
                                        {
                                            Tag = new object[] { _ioPortMenuItem, _inputConverting },
                                            Checked = (int)_currentInputPort.InputConverting == (int)_inputConverting,
                                            ToolTipText = ((_inputConvertingsType.GetField(_inputConverting.ToString()).GetCustomAttribute<DescriptionAttribute>()))?.Description
                                        });
                                        (_mnuInputConverting.DropDownItems[_mnuInputConverting.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                            .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    }
                                }
                                else
                                {
                                    ToolStripTextBox _mnuInputConvertingInt = new ToolStripTextBox()
                                    {
                                        ToolTipText = "Enter the Input Converting integer value and press Enter."
                                    };
                                    _mnuInputConvertingInt.TextBox.Text = _currentInputPort.InputConverting.ToString();
                                    _mnuInputConvertingInt.KeyUp += new KeyEventHandler(
                                        (object __sender, KeyEventArgs __e) =>
                                        {
                                            if (__e.KeyCode != Keys.Enter)
                                                return;
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IInputPort __inputPort = (IInputPort)(((object[])(__parentIOportMenuItem.Tag))[1]);
                                            int __i;
                                            if (int.TryParse(_mnuInputConvertingInt.TextBox.Text, out __i))
                                                __inputPort.InputConverting = __i;
                                            //hiding old menu
                                            ToolStripMenuItem __parentInputConvertingMenuItem = __parentIOportMenuItem.DropDownItems["InputConverting"] as ToolStripMenuItem;
                                            foreach (ToolStripItem __inputConvertingDropDownItem in __parentInputConvertingMenuItem.DropDownItems)
                                                __inputConvertingDropDownItem.Visible = false;
                                            foreach (ToolStripItem __inputDeviceDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                __inputDeviceDropDownItem.Visible = false;
                                            //
                                            BuildIOportsMenu(__portMenuItemIndex);
                                        });
                                    _mnuInputConvertingInt.Tag = new object[] { _ioPortMenuItem };
                                    _mnuInputConvertingInt.ToolTipText = ((_inputConvertingsType.GetCustomAttribute<DescriptionAttribute>()))?.Description;
                                    _mnuInputConverting.DropDownItems.Add(_mnuInputConvertingInt);
                                }
                                _ioPortMenuItem.DropDownItems.Add(_mnuInputConverting);

                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem("Settings...", null,
                                    (object __sender, EventArgs __e) =>
                                    {
                                        ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                        IInputPort __inputPort = (IInputPort)(((object[])(__parentIOportMenuItem.Tag))[1]);
                                        IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                        __ide.InitializeNewSettingWindow(__inputPort);
                                    })
                                {
                                    Tag = new object[] { _ioPortMenuItem }
                                });
                                (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                    .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                _ioPortMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                Type _inputPortType = _currentInputPort.GetType();
                                MethodInfo[] _inputPortMethods = _inputPortType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                                    .Where(m => m.ReturnType == typeof(void) && m.GetParameters().ToList().Count == 0 && m.GetCustomAttribute(typeof(MenuBrowsableAttribute)) != null
                                                && (m.GetCustomAttribute<MenuBrowsableAttribute>()).Browsable).ToArray();
                                foreach (MethodInfo _inputPortMethodInfo in _inputPortMethods)
                                {
                                    _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem(_inputPortMethodInfo.Name, null,
                                        (object sender, EventArgs e) =>
                                        {
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IInputPort __inputPort = (IInputPort)(((object[])(__parentIOportMenuItem.Tag))[1]);
                                            MethodInfo __inputPortMethodInfo = (MethodInfo)(((object[])((sender as ToolStripMenuItem).Tag))[1]);
                                            //hiding old menu
                                            __parentIOportMenuItem.DropDownItems.Clear();
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = false;
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = false;
                                            //
                                            __inputPortMethodInfo.Invoke(__inputPort, null);
                                            // showing back menu
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = true;
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = true;
                                            //
                                            BuildIOportsMenu(__portMenuItemIndex);
                                        })
                                    {
                                        Tag = new object[] { _ioPortMenuItem, _inputPortMethodInfo }
                                    });
                                    (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                }
                                if (_inputPortMethods.Length > 0)
                                    _ioPortMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                if (_ide.Machine is IMultiIOable)
                                {
                                    ToolStripMenuItem _mnuInputPortMove = new ToolStripMenuItem("Move To");
                                    ToolStripTextBox _mnuInputPortMoveToIndex = new ToolStripTextBox()
                                    {
                                        ToolTipText = "Enter the Input Port slot index to insert this Input Port to\n   or -1 for ActionInputPortIndex\n   or -2 or leave empty for inserting in next free Input Port slot\nand press Enter."
                                    };
                                    _mnuInputPortMoveToIndex.TextBox.Text = _portIndex.ToString();
                                    _mnuInputPortMoveToIndex.KeyUp += new KeyEventHandler(
                                        (object __sender, KeyEventArgs __e) =>
                                        {
                                            if (__e.KeyCode != Keys.Enter)
                                                return;
                                            ToolStripTextBox __mnuInputPortMoveToIndex = (ToolStripTextBox)__sender;
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])(__mnuInputPortMoveToIndex.Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                            int __portFromIndex = (int)(((object[])(__mnuInputPortMoveToIndex.Tag))[1]);
                                            int __i, __portToIndex;
                                            if (int.TryParse(__mnuInputPortMoveToIndex.TextBox.Text, out __i))
                                                __portToIndex = __ide.MachineInsertInputPort(__i, (__ide.Machine as IMultiIOable).InputPorts[__portFromIndex]);
                                            else
                                                __portToIndex = __ide.MachineInsertInputPort(-2, (__ide.Machine as IMultiIOable).InputPorts[__portFromIndex]);
                                            __ide.MachineInsertInputPort(__portFromIndex, null);
                                            //hiding old menu
                                            __mnuInputPortMoveToIndex.Visible = false;
                                            foreach (ToolStripItem __ioPortDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                __ioPortDropDownItem.Visible = false;
                                            //
                                            BuildIOportsMenu();
                                        });
                                    _mnuInputPortMoveToIndex.Tag = new object[] { _ioPortMenuItem, _portIndex };
                                    _mnuInputPortMove.DropDownItems.Add(_mnuInputPortMoveToIndex);
                                    _ioPortMenuItem.DropDownItems.Add(_mnuInputPortMove);
                                }

                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem("Delete", null,
                                        (object __sender, EventArgs __e) =>
                                        {
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                            int __portIndex = (int)(((object[])(__parentIOportMenuItem.Tag))[4]);
                                            __ide.MachineInsertInputPort(__portIndex, null);
                                            //hiding old menu
                                            foreach (ToolStripItem __ioPortDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                __ioPortDropDownItem.Visible = false;
                                            //
                                            BuildIOportsMenu();
                                        })
                                { Tag = new object[] { _ioPortMenuItem } });
                                (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                    .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                _ioPortMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem("Replace With:")
                                { Enabled = false });
                            }
                            foreach (InputPortTypeInfo _inputPortTypeInfo in _ide.ParentWorkplace.InputPortBuilder.VisibleTypes)
                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem($"New {_inputPortTypeInfo.DefaultName}", null,
                                    (object __sender, EventArgs __e) =>
                                    {
                                        ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                        int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                        IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                        int __portIndex = (int)(((object[])(__parentIOportMenuItem.Tag))[4]);
                                        InputPortTypeInfo __inputPortTypeInfo = ((InputPortTypeInfo)(((object[])((__sender as ToolStripMenuItem).Tag))[1]));
                                        __ide.MachineInsertInputPort(__portIndex, __ide.ParentWorkplace.InputPortBuilder.GetNew(__inputPortTypeInfo));
                                        //hiding old menu
                                        foreach (ToolStripItem __ioPortDropDownItem in __parentIOportMenuItem.DropDownItems)
                                            __ioPortDropDownItem.Visible = false;
                                        //
                                        BuildIOportsMenu(__portMenuItemIndex);
                                    })
                                {
                                    Tag = new object[] { _ioPortMenuItem, _inputPortTypeInfo },
                                    ToolTipText = $"{_inputPortTypeInfo.TypeName}   Version: {_inputPortTypeInfo.Version + (_inputPortTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {_inputPortTypeInfo.Author}" : "")}\n   from {_inputPortTypeInfo.SourceFileName.ShrinkFileName(65)}\n{_inputPortTypeInfo.Description}"
                                });
                            (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });

                            mnuIOport.DropDownItems.Add(new ToolStripSeparator());
                            mnuIOport.DropDownItems.Add(new ToolStripSeparator());
                            mnuIOport.DropDownItems.Add(new ToolStripSeparator());

                            _outputDeviceMenuItem.Text = "Out : " + (_currentOutputPort?.OutputDevice?.Title ?? _currentOutputPort?.Title ?? "--no output port--");
                            mnuIOport.DropDownItems.Add(_outputDeviceMenuItem);
                            //
                            if (_currentOutputPort != null)
                            {
                                mnuIOport.DropDownItems.Add(new ToolStripSeparator());

                                ToolStripMenuItem _mnuOutputConverting = new ToolStripMenuItem
                                    ($"Output Converting     : {_currentOutputPort.OutputConverting.ToString()}", null, null, "OutputConverting");
                                Type _outputConvertingsType = _currentOutputPort.OutputConverting.GetType();
                                if (_outputConvertingsType.IsEnum)
                                {
                                    foreach (var _outputConverting in Enum.GetValues(_outputConvertingsType))
                                    {
                                        _mnuOutputConverting.DropDownItems.Add(new ToolStripMenuItem(_outputConverting.ToString(), null,
                                            (object __sender, EventArgs __e) =>
                                            {
                                                ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                                int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                                IOutputPort __outputPort = (IOutputPort)(((object[])(__parentIOportMenuItem.Tag))[2]);
                                                int __outputConverting = (int)(((object[])((__sender as ToolStripMenuItem).Tag))[1]);
                                                __outputPort.OutputConverting = __outputConverting;
                                                //hiding old menu
                                                ToolStripMenuItem __parentOutputConvertingMenuItem = __parentIOportMenuItem.DropDownItems["OutputConverting"] as ToolStripMenuItem;
                                                foreach (ToolStripItem outputConvertingDropDownItem in __parentOutputConvertingMenuItem.DropDownItems)
                                                    outputConvertingDropDownItem.Visible = false;
                                                foreach (ToolStripItem outputDeviceDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                    outputDeviceDropDownItem.Visible = false;
                                                //
                                                BuildIOportsMenu(__portMenuItemIndex);
                                            })
                                        {
                                            Tag = new object[] { _ioPortMenuItem, _outputConverting },
                                            Checked = (int)_currentOutputPort.OutputConverting == (int)_outputConverting,
                                            ToolTipText = ((_outputConvertingsType.GetField(_outputConverting.ToString()).GetCustomAttribute<DescriptionAttribute>()))?.Description
                                        });
                                        (_mnuOutputConverting.DropDownItems[_mnuOutputConverting.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                            .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                    }
                                }
                                else
                                {
                                    ToolStripTextBox _mnuOutputConvertingInt = new ToolStripTextBox()
                                    {
                                        ToolTipText = "Enter the Output Converting integer value and press Enter."
                                    };
                                    _mnuOutputConvertingInt.TextBox.Text = _currentOutputPort.OutputConverting.ToString();
                                    _mnuOutputConvertingInt.KeyUp += new KeyEventHandler(
                                        (object __sender, KeyEventArgs __e) =>
                                        {
                                            if (__e.KeyCode != Keys.Enter)
                                                return;
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IOutputPort __outputPort = (IOutputPort)(((object[])(__parentIOportMenuItem.Tag))[2]);
                                            int __i;
                                            if (int.TryParse(_mnuOutputConvertingInt.TextBox.Text, out __i))
                                                __outputPort.OutputConverting = __i;
                                            //hiding old menu
                                            ToolStripMenuItem __parentOutputConvertingMenuItem = __parentIOportMenuItem.DropDownItems["OutputConverting"] as ToolStripMenuItem;
                                            foreach (ToolStripItem __outputConvertingDropDownItem in __parentOutputConvertingMenuItem.DropDownItems)
                                                __outputConvertingDropDownItem.Visible = false;
                                            foreach (ToolStripItem __outputDeviceDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                __outputDeviceDropDownItem.Visible = false;
                                            //
                                            BuildIOportsMenu(__portMenuItemIndex);
                                        });
                                    _mnuOutputConvertingInt.Tag = new object[] { _ioPortMenuItem };
                                    _mnuOutputConvertingInt.ToolTipText = ((_outputConvertingsType.GetCustomAttribute<DescriptionAttribute>()))?.Description;
                                    _mnuOutputConverting.DropDownItems.Add(_mnuOutputConvertingInt);
                                }
                                _ioPortMenuItem.DropDownItems.Add(_mnuOutputConverting);

                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem("Settings...", null,
                                    (object __sender, EventArgs __e) =>
                                    {
                                        ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                        IOutputPort __outputPort = (IOutputPort)(((object[])(__parentIOportMenuItem.Tag))[2]);
                                        IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                        __ide.InitializeNewSettingWindow(__outputPort);
                                    })
                                {
                                    Tag = new object[] { _ioPortMenuItem }
                                });
                                (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                    .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                _ioPortMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                Type _outputPortType = _currentOutputPort.GetType();
                                MethodInfo[] _outputPortMethods = _outputPortType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                                    .Where(m => m.ReturnType == typeof(void) && m.GetParameters().ToList().Count == 0 && m.GetCustomAttribute(typeof(MenuBrowsableAttribute)) != null
                                                && (m.GetCustomAttribute<MenuBrowsableAttribute>()).Browsable).ToArray();
                                foreach (MethodInfo _outputPortMethodInfo in _outputPortMethods)
                                {
                                    _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem(_outputPortMethodInfo.Name, null,
                                        (object sender, EventArgs e) =>
                                        {
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IOutputPort __outputPort = (IOutputPort)(((object[])(__parentIOportMenuItem.Tag))[2]);
                                            MethodInfo __outputPortMethodInfo = (MethodInfo)(((object[])((sender as ToolStripMenuItem).Tag))[1]);
                                            //hiding old menu
                                            __parentIOportMenuItem.DropDownItems.Clear();
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = false;
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = false;
                                            //
                                            __outputPortMethodInfo.Invoke(__outputPort, null);
                                            // showing back menu
                                            foreach (ToolStripItem __mnuDropDownItem0 in mnuMachine_.DropDownItems)
                                                __mnuDropDownItem0.Visible = true;
                                            foreach (ToolStripItem __mnuDropDownItem1 in mnuIOPorts_.DropDownItems)
                                                __mnuDropDownItem1.Visible = true;
                                            //
                                            BuildIOportsMenu(__portMenuItemIndex);
                                        })
                                    {
                                        Tag = new object[] { _ioPortMenuItem, _outputPortMethodInfo }
                                    });
                                    (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                        .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                }
                                if (_outputPortMethods.Length > 0)
                                    _ioPortMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                if (_ide.Machine is IMultiIOable)
                                {
                                    ToolStripMenuItem _mnuOutputPortMove = new ToolStripMenuItem("Move To");
                                    ToolStripTextBox _mnuOutputPortMoveToIndex = new ToolStripTextBox()
                                    {
                                        ToolTipText = "Enter the Output Port slot index to insert this Output Port to\n   or -1 for ActionOutputPortIndex\n   or -2 or leave empty for inserting in next free Output Port slot\nand press Enter."
                                    };
                                    _mnuOutputPortMoveToIndex.TextBox.Text = _portIndex.ToString();
                                    _mnuOutputPortMoveToIndex.KeyUp += new KeyEventHandler(
                                        (object __sender, KeyEventArgs __e) =>
                                        {
                                            if (__e.KeyCode != Keys.Enter)
                                                return;
                                            ToolStripTextBox __mnuOutputPortMoveToIndex = (ToolStripTextBox)__sender;
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])(__mnuOutputPortMoveToIndex.Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                            int __portFromIndex = (int)(((object[])(__mnuOutputPortMoveToIndex.Tag))[1]);
                                            int __i, __portToIndex;
                                            if (int.TryParse(__mnuOutputPortMoveToIndex.TextBox.Text, out __i))
                                                __portToIndex = __ide.MachineInsertOutputPort(__i, (__ide.Machine as IMultiIOable).OutputPorts[__portFromIndex]);
                                            else
                                                __portToIndex = __ide.MachineInsertOutputPort(-2, (__ide.Machine as IMultiIOable).OutputPorts[__portFromIndex]);
                                            __ide.MachineInsertOutputPort(__portFromIndex, null);
                                            //hiding old menu
                                            __mnuOutputPortMoveToIndex.Visible = false;
                                            foreach (ToolStripItem __ioPortDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                __ioPortDropDownItem.Visible = false;
                                            //
                                            BuildIOportsMenu();
                                        });
                                    _mnuOutputPortMoveToIndex.Tag = new object[] { _ioPortMenuItem, _portIndex };
                                    _mnuOutputPortMove.DropDownItems.Add(_mnuOutputPortMoveToIndex);
                                    _ioPortMenuItem.DropDownItems.Add(_mnuOutputPortMove);
                                }

                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem("Delete", null,
                                        (object __sender, EventArgs __e) =>
                                        {
                                            ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                            int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                            IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                            int __portIndex = (int)(((object[])(__parentIOportMenuItem.Tag))[4]);
                                            __ide.MachineInsertOutputPort(__portIndex, null);
                                            //hiding old menu
                                            foreach (ToolStripItem __ioPortDropDownItem in __parentIOportMenuItem.DropDownItems)
                                                __ioPortDropDownItem.Visible = false;
                                            //
                                            BuildIOportsMenu();
                                        })
                                { Tag = new object[] { _ioPortMenuItem } });
                                (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                    .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                                _ioPortMenuItem.DropDownItems.Add(new ToolStripSeparator());

                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem("Replace With:")
                                { Enabled = false });
                            }
                            foreach (OutputPortTypeInfo _outputPortTypeInfo in _ide.ParentWorkplace.OutputPortBuilder.VisibleTypes)
                                _ioPortMenuItem.DropDownItems.Add(new ToolStripMenuItem($"New {_outputPortTypeInfo.DefaultName}", null,
                                    (object __sender, EventArgs __e) =>
                                    {
                                        ToolStripMenuItem __parentIOportMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                        int __portMenuItemIndex = (int)(((object[])__parentIOportMenuItem.Tag)[0]);
                                        IIDE __ide = (IIDE)(((object[])(__parentIOportMenuItem.Tag))[3]);
                                        int __portIndex = (int)(((object[])(__parentIOportMenuItem.Tag))[4]);
                                        OutputPortTypeInfo __outputPortTypeInfo = ((OutputPortTypeInfo)(((object[])((__sender as ToolStripMenuItem).Tag))[1]));
                                        __ide.MachineInsertOutputPort(__portIndex, __ide.ParentWorkplace.OutputPortBuilder.GetNew(__outputPortTypeInfo));
                                        //hiding old menu
                                        foreach (ToolStripItem __ioPortDropDownItem in __parentIOportMenuItem.DropDownItems)
                                            __ioPortDropDownItem.Visible = false;
                                        //
                                        BuildIOportsMenu(__portMenuItemIndex);
                                    })
                                {
                                    Tag = new object[] { _ioPortMenuItem, _outputPortTypeInfo },
                                    ToolTipText = $"{_outputPortTypeInfo.TypeName}   Version: {_outputPortTypeInfo.Version + (_outputPortTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {_outputPortTypeInfo.Author}" : "")}\n   from {_outputPortTypeInfo.SourceFileName.ShrinkFileName(65)}\n{_outputPortTypeInfo.Description}"
                                });
                            (_ioPortMenuItem.DropDownItems[_ioPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });

                        };
                        mnuIOport.DropDownOpening += ioPortSubmenuOpening;

                        mnuIOPorts_.DropDownItems.Insert(portMenuItemIndex, mnuIOport);
                        portMenuItemIndex += 1;
                    }
                }
                //
                // Adding Input & Output Ports submenus
                //
                if (multiIOableMachine != null || mnuIOPorts_.DropDownItems.Count == 1)
                {
                    if (specifiedIOportMenuItemIndex == -3)
                    {
                        ToolStripMenuItem mnuAddInputPort = new ToolStripMenuItem("Add New Input Port", null, null, "AddInputPort")
                        { Tag = new object[] { SelectedIDE } };

                        mnuAddInputPort.DropDownItems.Add(new ToolStripMenuItem("(loading...)")
                        { Enabled = false });
                        EventHandler addInputPortSubmenuOpening = (object _mnuAddInputPortSender, EventArgs _addInputPortSubmenuOpeningArgs) =>
                        {
                            ToolStripMenuItem _addInputPortMenuItem = _mnuAddInputPortSender as ToolStripMenuItem;
                            IIDE _ide = (IIDE)(((object[])(_addInputPortMenuItem.Tag))[0]);
                            _addInputPortMenuItem.DropDownItems.Clear();

                            ToolStripTextBox _mnuNewInputPortIndex = new ToolStripTextBox()
                            { ToolTipText = "Enter the Input Port slot index to insert new Input Port to\n   or -1 for ActionInputPortIndex\n   or -2 or leave empty for inserting in next free Input Port slot\nand click on New <type>." };
                            _addInputPortMenuItem.DropDownItems.Add(_mnuNewInputPortIndex);

                            foreach (InputPortTypeInfo _inputPortTypeInfo in _ide.ParentWorkplace.InputPortBuilder.VisibleTypes)
                                _addInputPortMenuItem.DropDownItems.Add(new ToolStripMenuItem($"New {_inputPortTypeInfo.DefaultName}", null,
                                    (object __sender, EventArgs __e) =>
                                    {
                                        ToolStripMenuItem __parentAddInputPortMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                        ToolStripTextBox __mnuNewInputPortIndex = (ToolStripTextBox)(((object[])((__sender as ToolStripMenuItem).Tag))[1]);
                                        IIDE __ide = (IIDE)(((object[])((__sender as ToolStripMenuItem).Tag))[2]);
                                        InputPortTypeInfo __inputPortTypeInfo = ((InputPortTypeInfo)(((object[])((__sender as ToolStripMenuItem).Tag))[3]));
                                        int __i, __portIndex;
                                        if (int.TryParse(__mnuNewInputPortIndex.TextBox.Text, out __i))
                                            __portIndex = __ide.MachineInsertInputPort(__i, __ide.ParentWorkplace.InputPortBuilder.GetNew(__inputPortTypeInfo));
                                        else
                                            __portIndex = __ide.MachineInsertInputPort(-2, __ide.ParentWorkplace.InputPortBuilder.GetNew(__inputPortTypeInfo));
                                        //hiding old menu
                                        foreach (ToolStripItem __addInputPortDropDownItem in _addInputPortMenuItem.DropDownItems)
                                            __addInputPortDropDownItem.Visible = false;
                                        //
                                        BuildIOportsMenu();
                                    })
                                {
                                    Tag = new object[] { _addInputPortMenuItem, _mnuNewInputPortIndex, _ide, _inputPortTypeInfo },
                                    ToolTipText = $"{_inputPortTypeInfo.TypeName}   Version: {_inputPortTypeInfo.Version + (_inputPortTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {_inputPortTypeInfo.Author}" : "")}\n   from {_inputPortTypeInfo.SourceFileName.ShrinkFileName(65)}\n{_inputPortTypeInfo.Description}"
                                });
                            (_addInputPortMenuItem.DropDownItems[_addInputPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                        };
                        mnuAddInputPort.DropDownOpening += addInputPortSubmenuOpening;
                        //
                        ToolStripMenuItem mnuAddOutputPort = new ToolStripMenuItem("Add New Output Port", null, null, "AddOutputPort")
                        { Tag = new object[] { SelectedIDE } };

                        mnuAddOutputPort.DropDownItems.Add(new ToolStripMenuItem("(loading...)")
                        { Enabled = false });
                        EventHandler addOutputPortSubmenuOpening = (object _mnuAddOutputPortSender, EventArgs _addOutputPortSubmenuOpeningArgs) =>
                        {
                            ToolStripMenuItem _addOutputPortMenuItem = _mnuAddOutputPortSender as ToolStripMenuItem;
                            IIDE _ide = (IIDE)(((object[])(_addOutputPortMenuItem.Tag))[0]);
                            _addOutputPortMenuItem.DropDownItems.Clear();

                            ToolStripTextBox _mnuNewOutputPortIndex = new ToolStripTextBox()
                            { ToolTipText = "Enter the Output Port slot index to insert new Output Port to\n   or -1 for ActionOutputPortIndex\n   or -2 or leave empty for inserting in next free Output Port slot\nand click on New <type>." };
                            _addOutputPortMenuItem.DropDownItems.Add(_mnuNewOutputPortIndex);

                            foreach (OutputPortTypeInfo _outputPortTypeInfo in _ide.ParentWorkplace.OutputPortBuilder.VisibleTypes)
                                _addOutputPortMenuItem.DropDownItems.Add(new ToolStripMenuItem($"New {_outputPortTypeInfo.DefaultName}", null,
                                    (object __sender, EventArgs __e) =>
                                    {
                                        ToolStripMenuItem __parentAddOutputPortMenuItem = (ToolStripMenuItem)(((object[])((__sender as ToolStripMenuItem).Tag))[0]);
                                        ToolStripTextBox __mnuNewOutputPortIndex = (ToolStripTextBox)(((object[])((__sender as ToolStripMenuItem).Tag))[1]);
                                        IIDE __ide = (IIDE)(((object[])((__sender as ToolStripMenuItem).Tag))[2]);
                                        OutputPortTypeInfo __outputPortTypeInfo = ((OutputPortTypeInfo)(((object[])((__sender as ToolStripMenuItem).Tag))[3]));
                                        int __i, __portIndex;
                                        if (int.TryParse(__mnuNewOutputPortIndex.TextBox.Text, out __i))
                                            __portIndex = __ide.MachineInsertOutputPort(__i, __ide.ParentWorkplace.OutputPortBuilder.GetNew(__outputPortTypeInfo));
                                        else
                                            __portIndex = __ide.MachineInsertOutputPort(-2, __ide.ParentWorkplace.OutputPortBuilder.GetNew(__outputPortTypeInfo));
                                        //hiding old menu
                                        foreach (ToolStripItem __addOutputPortDropDownItem in _addOutputPortMenuItem.DropDownItems)
                                            __addOutputPortDropDownItem.Visible = false;
                                        //
                                        BuildIOportsMenu();
                                    })
                                {
                                    Tag = new object[] { _addOutputPortMenuItem, _mnuNewOutputPortIndex, _ide, _outputPortTypeInfo },
                                    ToolTipText = $"{_outputPortTypeInfo.TypeName}   Version: {_outputPortTypeInfo.Version + (_outputPortTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {_outputPortTypeInfo.Author}" : "")}\n   from {_outputPortTypeInfo.SourceFileName.ShrinkFileName(65)}\n{_outputPortTypeInfo.Description}"
                                });
                            (_addOutputPortMenuItem.DropDownItems[_addOutputPortMenuItem.DropDownItems.Count - 1] as ToolStripMenuItem)?
                                .DropDownItems.Add(new ToolStripMenuItem("(click)") { Enabled = false, ToolTipText = "(this menu item was added to abort IO ports menu hiding after click)" });
                        };
                        mnuAddOutputPort.DropDownOpening += addOutputPortSubmenuOpening;
                        //
                        if (!(mnuIOPorts_.DropDownItems[mnuIOPorts_.DropDownItems.Count - 1] is ToolStripSeparator))
                            mnuIOPorts_.DropDownItems.Add(new ToolStripSeparator());
                        mnuIOPorts_.DropDownItems.Add(mnuAddInputPort);
                        mnuIOPorts_.DropDownItems.Add(mnuAddOutputPort);
                    }
                    else
                    {
                        foreach (ToolStripItem mnuDropDownItem2 in
                            (mnuIOPorts_.DropDownItems["AddInputPort"] as ToolStripMenuItem).DropDownItems)
                            mnuDropDownItem2.Visible = true;
                        foreach (ToolStripItem mnuDropDownItem2 in
                            (mnuIOPorts_.DropDownItems["AddOutputPort"] as ToolStripMenuItem).DropDownItems)
                            mnuDropDownItem2.Visible = true;
                    }
                }
            }
            else
            {
                throw new Exception();
            }
        }

        private void mnuNewProgramFile_Click(object sender, EventArgs e)
        {
            SelectedIDE?.NewProgramFileByUser();
        }

        private void mnuNewIDE_Click(object sender, EventArgs e)
        {
            IIDE newIde;
            ParentWorkplace.NewIDEbyUser(out newIde);
            SelectIDE(newIde);
        }

        private void mnuOpenProgramFile_Click(object sender, EventArgs e)
        {
            SelectedIDE?.OpenProgramFileByUser();
        }

        private void mnuSaveProgramFile_Click(object sender, EventArgs e)
        {
            SelectedIDE?.SaveProgramFileByUser();
        }

        private void mnuSaveProgramFileAs_Click(object sender, EventArgs e)
        {
            SelectedIDE?.SaveProgramFileAsByUser();
        }

        private void mnuSaveIDE_Click(object sender, EventArgs e)
        {
            ParentWorkplace?.SaveIDEbyUser(SelectedIDE);
        }

        private void mnuSaveIDEas_Click(object sender, EventArgs e)
        {
            ParentWorkplace?.SaveIDEasByUser(SelectedIDE);
        }

        private async void mnuCloseIDE_Click(object sender, EventArgs e)
        {
            await ParentWorkplace.CloseIDEbyUser(SelectedIDE);
        }

        private async void mnuExit_Click(object sender, EventArgs e)
        {
            await ParentWorkplace.ExitByUser();
        }

        private async void mnuStart_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineStartAsync();
        }

        private async void mnuRestart_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineRestartAsync();
        }

        private async void mnuPause_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachinePauseAsync();
        }

        private async void mnuStep_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineStepAsync();
        }

        private async void mnuStepOver_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineStepOverAsync();
        }

        private async void mnuBreak_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineBreakAsync();
        }

        private async void mnuTerminate_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineTerminateAsync();
        }

        private async void mnuReset_Click(object sender, EventArgs e)
        {
            await SelectedIDE?.MachineResetAsync();
        }

        private void mchAutoResetOnStart_CheckedChanged(object sender, EventArgs e)
        {
            SelectedIDE.AutoResetOnStart = mchAutoResetOnStart.Checked;
        }

        private void mdiMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                mnuExit_Click(null, EventArgs.Empty);
            }
        }

        private void mnuRecentFilesItem_Click(object sender, EventArgs e)
        {
            if (SelectedIDE == null)
                throw new NullReferenceException();

            try
            {
                string filename = (sender as ToolStripMenuItem).Text;
                SelectedIDE.OpenProgramFile(filename);

                //moving filename in top of recent files menu list
                mnuRecentProgramFiles_.DropDownItems.Remove(sender as ToolStripMenuItem);
                mnuRecentProgramFiles_.DropDownItems.Insert(0, new ToolStripMenuItem(filename, null, mnuRecentFilesItem_Click));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
            }
        }

        private void mnuMachineSettings_Click(object sender, EventArgs e)
        {
            SelectedIDE.InitializeNewSettingWindow(SelectedIDE.Machine);
        }

        private void mnuComponentManager_Click(object sender, EventArgs e)
        {
            ParentWorkplace.InitializeNewComponentManager();
        }

        private async void mnuNewWorkplace_Click(object sender, EventArgs e)
        {
            await ParentWorkplace.NewWorkplaceByUser();
        }

        private void mnuOpenIDE_Click(object sender, EventArgs e)
        {
            IIDE newIde;
            ParentWorkplace.OpenIDEbyUser(out newIde);
            SelectIDE(newIde);
        }

        private async void mnuOpenWorkplace_Click(object sender, EventArgs e)
        {
            await ParentWorkplace.OpenWorkplaceByUser();
        }

        private async void mnuCloseWorkplace_Click(object sender, EventArgs e)
        {
            await ParentWorkplace.CloseWorkplaceByUser();
        }

        private void mnuSaveWorkplace_Click(object sender, EventArgs e)
        {
            ParentWorkplace.SaveWorkplaceByUser();
        }

        private void mnuSaveWorkplaceAs_Click(object sender, EventArgs e)
        {
            ParentWorkplace.SaveWorkplaceAsByUser();
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show
                    ($"{Application.ProductName} version {Application.ProductVersion}{Program.VersionSuffix}\r\n\r\n    by Evgeniy Chaev, 2017.",
                    "About multiIDE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void mntNewIDE_Click(object sender, EventArgs e)
        {
            mnuNewIDE_Click(sender, e);
        }

        private void mntOpenProgramFile_Click(object sender, EventArgs e)
        {
            mnuOpenProgramFile_Click(sender, e);
        }

        private void mntSaveProgramFile_Click(object sender, EventArgs e)
        {
            mnuSaveProgramFile_Click(sender, e);
        }
        #endregion

        private void mnuSelectedIDEsettings_Click(object sender, EventArgs e)
        {
            ParentWorkplace.InitializeNewSettingWindow(SelectedIDE);
        }

        private void mnuWorkplaceSettings_Click(object sender, EventArgs e)
        {
            ParentWorkplace.InitializeNewSettingWindow(ParentWorkplace);
        }

        private void mnuMachineActions_DropDownOpening(object sender, EventArgs e)
        {
            mnuMachineActions_.DropDownItems.Clear();

            Type machineType = SelectedIDE.Machine.GetType();
            MethodInfo[] machineMethods = machineType
                    .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod)
                    .Where(m => m.ReturnType == typeof(void) && m.GetParameters().ToList().Count == 0 &&
                            m.GetCustomAttribute<MenuBrowsableAttribute>() != null
                            && (m.GetCustomAttribute<MenuBrowsableAttribute>()).Browsable).ToArray();

            if (machineMethods != null && machineMethods.Length > 0)
                foreach (MethodInfo machineMethodInfo in machineMethods)
                {
                    mnuMachineActions_.DropDownItems.Add(new ToolStripMenuItem(machineMethodInfo.Name, null,
                            (object __sender, EventArgs __e) =>
                            {
                                MethodInfo __methodInfo =
                                    (MethodInfo)(((object[])((__sender as ToolStripMenuItem).Tag))[1]);

                                bool __locked = false;
                                if (__methodInfo.GetCustomAttribute<LockingRequiredAttribute>()
                                        ?.IsLockingRequired ??
                                    false) IOmonitor.IdleForLock(SelectedIDE.Machine, out __locked);
                                __methodInfo.Invoke(SelectedIDE.Machine, null);
                                if (__locked) IOmonitor.Unlock(SelectedIDE.Machine);
                            })
                    {
                        Tag = new object[] { "MethodListMenuItem", machineMethodInfo },
                        Enabled = !(machineMethodInfo.GetCustomAttribute<AvailableWhenInitializedOnlyAttribute>()?.Checking ?? false)
                                  || SelectedIDE.Machine.Status == VirtualMachineRunningStatus.Runtime,
                        ToolTipText = machineMethodInfo.GetCustomAttribute<DescriptionAttribute>()?.Description
                    });
                }
            else
                mnuMachineActions_.DropDownItems.Add(new ToolStripMenuItem("None") { Enabled = false });
        }

        private void mnuIDEextras__DropDownOpening(object sender, EventArgs e)
        {
            mnuIDEextras_.DropDownItems.Clear();

            if (SelectedIDE.ExtraComponents.Count > 0)
            {
                foreach (IExtraIdeComponent extraComponent in SelectedIDE.ExtraComponents)
                {
                    mnuIDEextras_.DropDownItems.Add(new ToolStripMenuItem(extraComponent.Title, null,
                            (object _sender, EventArgs _e) =>
                            {
                                IExtraIdeComponent _parentExtraIdeComponent = (IExtraIdeComponent)(((object[])((_sender as ToolStripMenuItem).Tag))[0]);
                                if (!_parentExtraIdeComponent.IsInitialized)
                                    _parentExtraIdeComponent.Initialize(this);
                                (_parentExtraIdeComponent as Form)?.Activate();
                            })
                    { Tag = new object[] { extraComponent } });
                }
            }
            else
                mnuIDEextras_.DropDownItems.Add(new ToolStripMenuItem("None") { Enabled = false });

            if (ParentWorkplace.IdeExtraBuilder.VisibleTypes.Count > 0)
            {
                mnuIDEextras_.DropDownItems.Add("-");
                foreach (ComponentTypeInfo extraComponentTypeInfo in ParentWorkplace.IdeExtraBuilder.VisibleTypes)
                {
                    if (Type.GetType(extraComponentTypeInfo.AssemblyQualifiedTypeName).GetCustomAttribute<MenuBrowsableAttribute>()?.Browsable ?? true)
                        mnuIDEextras_.DropDownItems.Add(new ToolStripMenuItem("New " + extraComponentTypeInfo.TypeName, null,
                                (object _sender, EventArgs _e) =>
                                {
                                    ComponentTypeInfo _extraComponentTypeInfo = (ComponentTypeInfo)(((object[])((_sender as ToolStripMenuItem).Tag))[0]);
                                    SelectedIDE.NewExtraIdeComponent(_extraComponentTypeInfo);
                                })
                        {
                            Tag = new object[] { extraComponentTypeInfo },
                            ToolTipText = $"{extraComponentTypeInfo.TypeName}   Version: {extraComponentTypeInfo.Version + (extraComponentTypeInfo.Author.IsNotNullOrEmpty() ? $"   by {extraComponentTypeInfo.Author}" : "")}\n   from {extraComponentTypeInfo.SourceFileName.ShrinkFileName(65)}\n{extraComponentTypeInfo.Description}"
                        }
                            );
                }
            }
        }
    }
}
