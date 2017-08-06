using multiIDE.Dialogs;
using multiIDE.IODevices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using multiIDE.ComponentBuilding;
using multiIDE.Machines;
using XSpace;
using multiIDE.Extras;
using multiIDE.Commons;
using System.Reflection;

namespace multiIDE
{
    /// <summary>
    /// Main controller component.
    /// </summary>
    public sealed class Workplace : IWorkplace
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "Workplace";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Main controller component.";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        #endregion

        #region Environment properties
        [Category("Environment"), ReadOnly(false)]
        public string CustomName { get; set; } = "";
        //
        [Category("Environment"), ReadOnly(true)]
        public int Id { get; set; } = CommonConstants.UndefinedComponentId;
        //
        [Category("Appearance"), ReadOnly(true)]
        public string Title
        {
            get
            {
                return (_CustomTitle.IsNotNullOrEmpty()) ? _CustomTitle
                        : (CustomName.IsNotNullOrEmpty() ? CustomName
                        : this.ToString());
            }
            set
            {
                _CustomTitle = value;
            }
        }
        //
        [Category("Environment"), ReadOnly(true)]
        public object Tag { get; set; }
        //
        private string _CustomTitle = "";
        #endregion

        #region Essential properties
        [Browsable(false)]
        //
        [Category("Essential"), ReadOnly(true)]
        public IEnumerable<ComponentTypesAssemblySourceInfo> ComponentTypesAssemblySources
            => VirtualMachineBuilder.RegisteredTypes.Cast<ComponentTypeInfo>()
                .Concat(CodeEditorBuilder.RegisteredTypes)
                .Concat(InputPortBuilder.RegisteredTypes)
                .Concat(OutputPortBuilder.RegisteredTypes)
                .Concat(IdeExtraBuilder.RegisteredTypes)
                .Concat(InputDeviceBuilder.RegisteredTypes)
                .Concat(OutputDeviceBuilder.RegisteredTypes)
                .Concat(WorkplaceExtraBuilder.RegisteredTypes)
                .GetSourceInfos();
        //
        [Category("Essential"), ReadOnly(true)]
        public IMainForm MainForm { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public bool IsInitialized { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public int WorkplaceIdCounter { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public int IdeIdCounter { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public int MachinesIdCounter { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public List<IIDE> IDEs { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public List<IInputDevice> InputDevices { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public List<IOutputDevice> OutputDevices { get; private set; }
        //
        [Category("Essential"), ReadOnly(true)]
        public List<IWorkplaceComponent> ExtraComponents { get; private set; }
                = new List<IWorkplaceComponent>();
        //
        [Category("Essential"), ReadOnly(true)]
        public List<IComponent> CommonComponents { get; private set; }
                = new List<IComponent>();
        //
        [Browsable(false)]
        public SomeFile ConfigFile
        {
            get { return _ConfigFile; }
            set { _ConfigFile = value; }
        }
        //
        private SomeFile _ConfigFile;
        #endregion

        #region Component Builders
        [Browsable(false)]
        public IComponentBuilder<IVirtualMachine, VirtualMachineTypeInfo> VirtualMachineBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<ICodeEditor, CodeEditorTypeInfo> CodeEditorBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<IInputPort, InputPortTypeInfo> InputPortBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<IOutputPort, OutputPortTypeInfo> OutputPortBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<IExtraIdeComponent, ComponentTypeInfo> IdeExtraBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<IInputDevice, InputDeviceTypeInfo> InputDeviceBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<IOutputDevice, OutputDeviceTypeInfo> OutputDeviceBuilder { get; private set; }
        //
        [Browsable(false)]
        public IComponentBuilder<IExtraWorkplaceComponent, ComponentTypeInfo> WorkplaceExtraBuilder { get; private set; }
                /*= new ComponentBuilder<IExtraWorkplaceComponent, ComponentTypeInfo>("multiIDE.Extras"
                , ComponentBuildersService.DefaultWorkplaceExtraBuilder.RegisteredTypes);*/
        #endregion

        #region Events
        public event EventHandler<ShowMessageEventArgs> ShowingMessage;
        public event EventHandler<AskUserEventArgs> AskingUser;
        public event EventHandler<SaveFileByUserEventArgs> SavingFileByUser;
        public event EventHandler<OpenFileByUserEventArgs> OpeningFileByUser;
        public event EventHandler GotUpdated;
        //
        public void OnShowingMessage(string message, string caption = "", MessageBoxIcon icon = MessageBoxIcon.None)
        {
            ShowingMessage?.Invoke(this, new ShowMessageEventArgs(message, caption, icon));
        }
        //
        public DialogResult OnAskingUser(string message, string caption, MessageBoxButtons buttons
                , MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
        {
            var askingUserEventArgsObject = new AskUserEventArgs(message, caption, buttons, icon, defaultButton);
            AskingUser.Invoke(this, askingUserEventArgsObject);
            return askingUserEventArgsObject.Answer;
        }
        //
        private void OnGotUpdated()
        {
            GotUpdated?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        public override string ToString() => (CustomName.IsNotNullOrEmpty()) ? CustomName
                : $"{DefaultName}{Id}";

        /////

        private Workplace()
        {
            this.ShowingMessage += MainSymbolService.ShowMessage;
            this.AskingUser += MainSymbolService.AskUser;
            this.OpeningFileByUser += MainSymbolService.OpenFileByUser;
            this.SavingFileByUser += MainSymbolService.SaveFileByUser;
            //
            _ConfigFile = new SomeFile()
            {
                FileName = "",
                ShortFileName = "",
                IsChangesSaved = true
            };
            //
            IsInitialized = false;
        }

        public Workplace(IMainForm mainForm) : this()
        {
            MainForm = mainForm;
            MainForm.ParentWorkplace = this;
            OnGotUpdated();
            MainFormSelectIDE(null);
            MainFormActivate();
        }

        public Workplace(IMainForm mainForm, string fromFile) : this(mainForm)
        {
            OpenWorkplace(fromFile);
        }

        #region Components managing subs
        public bool NewInputDevice(InputDeviceTypeInfo inputDeviceTypeInfo, out IInputDevice newInputDevice)
        {
            if (inputDeviceTypeInfo == null)
                throw new NullReferenceException();

            newInputDevice = inputDeviceTypeInfo != null ?
                InputDeviceBuilder.GetNew(inputDeviceTypeInfo) : null;

            if (newInputDevice as IFileInputable != null)
            {
                var openingFileDialogArgs = new OpenFileByUserEventArgs()
                {
                    InitialDirectory = (MainForm.SelectedIDE.ProgramFile.FileName.IsNotNullOrEmpty())
                        ? Path.GetDirectoryName(MainForm.SelectedIDE.ProgramFile.FileName)
                        : (MainForm.SelectedIDE.ParentWorkplace.MainForm.RecentFiles.Count > 2)
                            ? Path.GetDirectoryName(MainForm.SelectedIDE.ParentWorkplace.MainForm.RecentFiles[0])
                            : Path.GetDirectoryName(Application.ExecutablePath),
                    Filter = "Text Files (*.txt)|*.txt|Binary Files (*.bin)|*.txt|All Files (*.*)|*.*",
                    FilterIndex = 3,
                    RestoreDirectory = true
                };

                OpeningFileByUser?.Invoke(this, openingFileDialogArgs);

                if (openingFileDialogArgs.Answer == DialogResult.OK)
                {
                    try
                    {
                        FileInfo fInfo = new FileInfo(openingFileDialogArgs.FileName);
                        if (fInfo.Exists)
                        {
                            (newInputDevice as IFileInputable).FileName = openingFileDialogArgs.FileName;
                        }
                        else
                            return false;
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (newInputDevice != null)
            {
                this.InputDevices.Add(newInputDevice);
                newInputDevice.ParentWorkplace = this;
                newInputDevice.Id = this.InputDevices.Count - 1;
            }
            if (newInputDevice as IOutputDevice != null)
            {
                this.OutputDevices.Add(newInputDevice as IOutputDevice);
            }
            return true;
        }

        public bool NewOutputDevice(OutputDeviceTypeInfo outputDeviceTypeInfo, out IOutputDevice newOutputDevice)
        {
            if (outputDeviceTypeInfo == null)
                throw new NullReferenceException();

            newOutputDevice = outputDeviceTypeInfo != null ?
                OutputDeviceBuilder.GetNew(outputDeviceTypeInfo) : null;

            if (newOutputDevice as IFileOutputable != null)
            {
                var openingFileDialogArgs = new OpenFileByUserEventArgs()
                {
                    InitialDirectory = (MainForm.SelectedIDE.ProgramFile.FileName.IsNotNullOrEmpty())
                        ? Path.GetDirectoryName(MainForm.SelectedIDE.ProgramFile.FileName)
                        : (MainForm.SelectedIDE.ParentWorkplace.MainForm.RecentFiles.Count > 2)
                            ? Path.GetDirectoryName(MainForm.SelectedIDE.ParentWorkplace.MainForm.RecentFiles[0])
                            : Path.GetDirectoryName(Application.ExecutablePath),
                    Filter = "Text Files (*.txt)|*.txt|Binary Files (*.bin)|*.txt|All Files (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true
                };

                OpeningFileByUser?.Invoke(this, openingFileDialogArgs);

                if (openingFileDialogArgs.Answer == DialogResult.OK)
                {
                    try
                    {
                        if (!(System.IO.File.Exists(openingFileDialogArgs.FileName)
                              && (System.IO.File.GetAttributes(openingFileDialogArgs.FileName) & FileAttributes.ReadOnly) != 0))
                        {
                            (newOutputDevice as IFileOutputable).FileName = openingFileDialogArgs.FileName;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            if (newOutputDevice != null)
            {
                this.OutputDevices.Add(newOutputDevice);
                newOutputDevice.ParentWorkplace = this;
                newOutputDevice.Id = this.OutputDevices.Count - 1;
            }
            if (newOutputDevice as IInputDevice != null)
            {
                this.InputDevices.Add(newOutputDevice as IInputDevice);
            }
            return true;
        }

        public void InitializeNewComponentManager()
        {
            if (!IsInitialized)
            {
                IComponent defaultComponentManager = NewCommonComponent
                        (ComponentBuildersService.CommonBuilder.RegisteredTypes.Find
                        (i => i.TypeFullName == typeof(DefaultComponentManager).FullName));
                (defaultComponentManager as ComponentManager).Tag = null;
                (defaultComponentManager as ComponentManager).Initialize(this);
            }
            else
            {
                IExtraWorkplaceComponent componentManager = NewExtraWorkplaceComponent
                        (WorkplaceExtraBuilder.RegisteredTypes.Find
                        (i => i.TypeFullName == typeof(ComponentManager).FullName));
                componentManager.Tag = this;
                componentManager.Initialize(this);
            }
        }

        public IExtraWorkplaceComponent InitializeNewSettingWindow(IComponent componentToSet)
        {
            IExtraWorkplaceComponent settingWindow = NewExtraWorkplaceComponent
            (WorkplaceExtraBuilder.RegisteredTypes.Find
                (i => i.TypeFullName == typeof(SettingWindow).FullName));
            settingWindow.Tag = componentToSet;
            settingWindow.Initialize(this);
            return settingWindow;
        }

        public IExtraWorkplaceComponent NewExtraWorkplaceComponent(ComponentTypeInfo extraComponentType)
        {
            IExtraWorkplaceComponent newExtra = WorkplaceExtraBuilder.GetNew(extraComponentType);
            newExtra.ParentWorkplace = this;
            Form newExtraForm = newExtra as Form;
            if (newExtraForm != null)
            {
                newExtraForm.MdiParent = MainForm as Form;
                newExtraForm.FormClosing += this.ExtraWorkplaceComponent_Closing;
            }
            ExtraComponents.Add(newExtra);

            if (newExtra.GetType().GetCustomAttribute<InitializeAfterCreateAttribute>()?.InitializeAfterCreate ?? false)
                newExtra.Initialize(this);

            return newExtra;
        }

        public IComponent NewCommonComponent(ComponentTypeInfo commonComponentType)
        {
            IComponent newCommon = ComponentBuildersService.CommonBuilder.GetNew(commonComponentType);
            Form newCommonForm = newCommon as Form;
            if (newCommonForm != null)
            {
                newCommonForm.MdiParent = MainForm as Form;
                newCommonForm.FormClosing += this.CommonComponent_Closing;
            }
            CommonComponents.Add(newCommon);

            return newCommon;
        }

        // // //

        private void ExtraWorkplaceComponent_Closing(object sender, EventArgs e)
        {
            ExtraComponents.Remove(sender as IExtraWorkplaceComponent);
            (sender as IDisposable)?.Dispose();
        }

        private void CommonComponent_Closing(object sender, EventArgs e)
        {
            CommonComponents.Remove(sender as IComponent);
            Form componentForm = sender as Form;
            if (componentForm != null)
            {
                componentForm.FormClosing -= this.CommonComponent_Closing;
                componentForm.Close();
            }
            (sender as IDisposable)?.Dispose();
        }
        #endregion

        #region Config File subs
        public void LoadConfigFile(string fileName)
        {
            throw new NotImplementedException();
        }

        public void SaveConfigFile()
        {
            throw new NotImplementedException();
        }

        public void SaveConfigFile(string fileName)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Workplace service subs
        public void InitializeWorkplace()
        {
            if (IsInitialized)
                throw new InvalidOperationException();

            WorkplaceIdCounter += 1;
            Id = WorkplaceIdCounter;
            CustomName = "";
            _CustomTitle = "";

            VirtualMachineBuilder = new ComponentBuilder<IVirtualMachine, VirtualMachineTypeInfo>("multiIDE.Machines"
                    , ComponentBuildersService.DefaultVirtualMachineBuilder.RegisteredTypes);
            CodeEditorBuilder = new ComponentBuilder<ICodeEditor, CodeEditorTypeInfo>("multiIDE.CodeEditors"
                    , ComponentBuildersService.DefaultCodeEditorBuilder.RegisteredTypes);
            InputPortBuilder = new ComponentBuilder<IInputPort, InputPortTypeInfo>("multiIDE.IOPorts"
                    , ComponentBuildersService.DefaultInputPortBuilder.RegisteredTypes);
            OutputPortBuilder = new ComponentBuilder<IOutputPort, OutputPortTypeInfo>("multiIDE.IOPorts"
                    , ComponentBuildersService.DefaultOutputPortBuilder.RegisteredTypes);
            IdeExtraBuilder = new ComponentBuilder<IExtraIdeComponent, ComponentTypeInfo>("multiIDE.Extras"
                    , ComponentBuildersService.DefaultIdeExtraBuilder.RegisteredTypes);
            InputDeviceBuilder = new ComponentBuilder<IInputDevice, InputDeviceTypeInfo>("multiIDE.IODevices"
                    , ComponentBuildersService.DefaultInputDeviceBuilder.RegisteredTypes);
            OutputDeviceBuilder = new ComponentBuilder<IOutputDevice, OutputDeviceTypeInfo>("multiIDE.IODevices"
                    , ComponentBuildersService.DefaultOutputDeviceBuilder.RegisteredTypes);
            WorkplaceExtraBuilder = new ComponentBuilder<IExtraWorkplaceComponent, ComponentTypeInfo>("multiIDE.Extras"
                    , ComponentBuildersService.DefaultWorkplaceExtraBuilder.RegisteredTypes);

            IdeIdCounter = 0;
            MachinesIdCounter = 0;
            IDEs = new List<IIDE>();
            InputDevices = new List<IInputDevice>();
            OutputDevices = new List<IOutputDevice>();
            ExtraComponents = new List<IWorkplaceComponent>();
            _ConfigFile = new SomeFile()
            {
                FileName = "",
                ShortFileName = "",
                IsChangesSaved = true
            };

            IsInitialized = true;
            OnGotUpdated();
            MainFormSelectIDE(null);
        }

        public void OpenWorkplace(string fileName)
        {
            throw new NotImplementedException();
        }

        public void SaveWorkplace()
        {
            throw new NotImplementedException();
        }

        public void SaveWorkplace(string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task DisposeWorkplace(bool immediately = false)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            // closing IDEs in reverse order
            while (IDEs.Count > 0)
            {
                await CloseIDE(IDEs.Last(), immediately);
            }

            bool locked;
            foreach (IInputDevice inputDevice in InputDevices)
                if (inputDevice != null && inputDevice.IsInitialized)
                {
                    locked = false;
                    if (inputDevice.IsLockingRequiredForDispose()) IOmonitor.IdleForLock(inputDevice, out locked);
                    inputDevice.Dispose();
                    if (locked) IOmonitor.Unlock(inputDevice);
                }
            foreach (IOutputDevice outputDevice in OutputDevices)
                if (outputDevice != null && outputDevice.IsInitialized)
                {
                    locked = false;
                    if (outputDevice.IsLockingRequiredForDispose()) IOmonitor.IdleForLock(outputDevice, out locked);
                    outputDevice.Dispose();
                    if (locked) IOmonitor.Unlock(outputDevice);
                }
            foreach (IExtraWorkplaceComponent extraComponent in ExtraComponents
                                                    .Where(e => e is IDisposable))
                (extraComponent as IDisposable)?.Dispose();

            IsInitialized = false;
            OnGotUpdated();
        }

        // // Driven by user Workplace subs:

        public async Task<bool> NewWorkplaceByUser()
        {
            if (IsInitialized)
                if (!await CloseWorkplaceByUser())
                    return false;

            InitializeWorkplace();
            return true;
        }

        public async Task<bool> OpenWorkplaceByUser()
        {
            throw new NotImplementedException();
        }

        public bool SaveWorkplaceByUser()
        {
            if (_ConfigFile.FileName.IsNotNullOrEmpty())
            {
                try
                {
                    SaveWorkplace();
                    return true;
                }
                catch (ArgumentException ex)
                {
                    OnShowingMessage("Cant save program in " + _ConfigFile.FileName + Environment.NewLine + ex.Message);
                    return false;
                }
            }
            else
                return SaveWorkplaceAsByUser();
        }

        public bool SaveWorkplaceAsByUser()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CloseWorkplaceByUser(bool immediately = false)
        {
            if (!IsInitialized)
                return true;

            while (IDEs.Count > 0)
            {
                if (!(await CloseIDEbyUser(IDEs.Last(), true))) return false;
            }

            if (!ConfigFile.IsChangesSaved)
            {
                DialogResult answer =
                    OnAskingUser("Save changes in file "
                                 + ConfigFile.ShortFileName + " before closing the Workplace?", "Unsaved changes in " + Title, MessageBoxButtons.YesNoCancel);
                switch (answer)
                {
                    case DialogResult.Yes:
                        if (!SaveWorkplaceByUser()) return false;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            await DisposeWorkplace(true);

            return true;
        }
        #endregion

        #region Application service subs
        public async Task Exit(bool immediately = false)
        {
            if (this.IsInitialized && !immediately)
                await DisposeWorkplace(true);

            foreach (IComponent commonComponent in CommonComponents.Where(c => c is IDisposable))
                (commonComponent as IDisposable)?.Dispose();

            Application.Exit();
        }

        public async Task<bool> ExitByUser(bool immediately = false)
        {
            if (this.IsInitialized && !(await CloseWorkplaceByUser(immediately)))
                return false;

            await Exit(immediately);
            return true;
        }
        #endregion

        #region MainForm service subs
        public void MainFormActivate()
        {
            MainForm.Activate();
        }

        public void MainFormSelectIDE(IIDE ide)
        {
            MainForm.SelectIDE(ide);
        }
        #endregion

        #region IDEs service subs
        public IIDE NewIDE(IVirtualMachine machine, ICodeEditor codeEditor
                            , IInputPort inputPort = null, int withInputPortIndex = -1
                            , IOutputPort outputPort = null, int withOutputPortIndex = -1
                            , bool withConsole = false)
        {
            if (!IsInitialized)
                throw new InvalidOperationException();

            (codeEditor as Form).MdiParent = MainForm as Form;
            IIDE newIde = new IDE(machine, codeEditor);
            newIde.ParentWorkplace = this;
            newIde.ShowingMessage += MainSymbolService.ShowMessage;
            newIde.AskingUser += MainSymbolService.AskUser;
            newIde.OpeningFileByUser += MainSymbolService.OpenFileByUser;
            newIde.SavingFileByUser += MainSymbolService.SaveFileByUser;

            if (inputPort != null && withInputPortIndex >= -1)
                newIde.MachineInsertInputPort(withInputPortIndex, inputPort);
            if (outputPort != null && withOutputPortIndex >= -1)
                newIde.MachineInsertOutputPort(withOutputPortIndex, outputPort);

            if (withConsole && (inputPort != null || outputPort != null))
            {
                IInputDevice newConsoleAsInputDevice;
                IOutputDevice newConsoleAsOutputDevice = null;
                if (inputPort != null)
                {
                    InputDeviceTypeInfo consoleDeviceTypeInfo =
                        InputDeviceBuilder.RegisteredTypes.Find(i => i.TypeFullName == typeof(ConsoleDevice).FullName);
                    if (consoleDeviceTypeInfo != null)
                    {
                        NewInputDevice(consoleDeviceTypeInfo, out newConsoleAsInputDevice);
                        newConsoleAsOutputDevice = newConsoleAsInputDevice as IOutputDevice;
                        newIde.MachineSetInputDevice(inputPort, newConsoleAsInputDevice);
                    }
                }
                if (outputPort != null)
                {
                    if (newConsoleAsOutputDevice != null)
                    {
                        newIde.MachineSetOutputDevice(outputPort, newConsoleAsOutputDevice);
                    }
                    else
                    {
                        OutputDeviceTypeInfo consoleDeviceTypeInfo =
                            OutputDeviceBuilder.RegisteredTypes.Find(i => i.TypeFullName == typeof(ConsoleDevice).FullName);
                        if (consoleDeviceTypeInfo != null)
                        {
                            NewOutputDevice(consoleDeviceTypeInfo, out newConsoleAsOutputDevice);
                            newIde.MachineSetOutputDevice(outputPort, newConsoleAsOutputDevice);
                        }
                    }
                }
            }

            newIde.Machine.Id = MachinesIdCounter++;
            newIde.Machine.CustomName = newIde.Machine.ToString();

            newIde.Id = IdeIdCounter++;
            newIde.ProgramFile.ShortFileName = GetNewProgramFileShortName(newIde);
            newIde.Title = GetNewIDEtitle(newIde);
            newIde.ConfigFile.ShortFileName = newIde.Title;

            IDEs.Add(newIde);
            OnGotUpdated();

            return newIde;
        }

        public IIDE OpenIDE(string fileName)
        {
            throw new NotImplementedException();
        }

        public void SaveIDE(IIDE ide)
        {
            throw new NotImplementedException();
        }

        public void SaveIDEas(IIDE ide, string fileName)
        {
            throw new NotImplementedException();
        }

        public async Task CloseIDE(IIDE ide, bool immediately = false)
        {
            if (ide == null)
                throw new NullReferenceException();

            if (MainForm.SelectedIDE == ide)
            {
                int i = IDEs.IndexOf(ide);
                if (i + 1 < IDEs.Count)
                    MainForm.SelectIDE(IDEs[i + 1]);
                else if (i > 0)
                    MainForm.SelectIDE(IDEs[i - 1]);
                else
                    MainForm.SelectIDE(null);
            }
            IDEs.Remove(ide);

            ide.ShowingMessage -= MainSymbolService.ShowMessage;
            ide.AskingUser -= MainSymbolService.AskUser;
            ide.OpeningFileByUser -= MainSymbolService.OpenFileByUser;
            ide.SavingFileByUser -= MainSymbolService.SaveFileByUser;

            if (!immediately)
                await ide.DisposeAsync(immediately);
            else
                ide.DisposeAsync(immediately);

            OnGotUpdated();
        }

        // // Driven by user IDE subs:

        public bool NewIDEbyUser(out IIDE newIde)
        {
            if (VirtualMachineBuilder?.VisibleTypes.Count < 1)
            {
                OnShowingMessage("There're no available Virtual Machine types to add in a new IDE. Check types settings via Component Manager."
                        , "Workplace Configuration Warning in " + Title, MessageBoxIcon.Exclamation);
                newIde = null;
                return false;
            }
            if (CodeEditorBuilder?.VisibleTypes.Count < 1)
            {
                OnShowingMessage("There're no available Code Editor types to add in a new IDE. Check types settings via Component Manager."
                        , "Workplace Configuration Warning in " + Title, MessageBoxIcon.Exclamation);
                newIde = null;
                return false;
            }
            //

            IVirtualMachine machine = null;
            ICodeEditor codeEditor = null;
            IInputPort inputPort = null;
            int withInputPortIndex = -1;
            IOutputPort outputPort = null;
            int withOutputPortIndex = -1;
            bool withConsole = false;
            newIde = null;

            DialogResult newIDEdialogResult;
            using (var newIDEdialog = new NewIdeDialog(VirtualMachineBuilder.VisibleTypes, CodeEditorBuilder.VisibleTypes
                    , InputPortBuilder.VisibleTypes, OutputPortBuilder.VisibleTypes
                    , InputDeviceBuilder.VisibleTypes.Any(t => t.TypeName == typeof(ConsoleDevice).Name)
                    || OutputDeviceBuilder.VisibleTypes.Any(t => t.TypeName == typeof(ConsoleDevice).Name)))
            {
                newIDEdialogResult = newIDEdialog.ShowDialog(MainForm as Form);
                if (newIDEdialogResult == DialogResult.OK)
                {
                    machine = VirtualMachineBuilder.GetNew(VirtualMachineBuilder.VisibleTypes[newIDEdialog.ChosenMachineTypeIndex]);
                    machine.CurrentLanguage = newIDEdialog.ChosenLanguage;
                    codeEditor = CodeEditorBuilder.GetNew(CodeEditorBuilder.VisibleTypes[newIDEdialog.ChosenCodeEditorTypeIndex]);
                    inputPort = newIDEdialog.ChosenInputPortTypeFullName.IsNotNullOrEmpty()
                            ? InputPortBuilder.GetNew(InputPortBuilder.VisibleTypes[newIDEdialog.ChosenInputPortTypeIndex]) : null;
                    withInputPortIndex = newIDEdialog.ChosenInputPortIndex;
                    outputPort = newIDEdialog.ChosenOutputPortTypeFullName.IsNotNullOrEmpty()
                            ? OutputPortBuilder.GetNew(OutputPortBuilder.VisibleTypes[newIDEdialog.ChosenOutputPortTypeIndex]) : null;
                    withOutputPortIndex = newIDEdialog.ChosenOutputPortIndex;
                    withConsole = newIDEdialog.ChosenWithConsole;
                }
            }
            if (newIDEdialogResult != DialogResult.OK)
                return false;

            newIde = NewIDE(machine, codeEditor, inputPort, withInputPortIndex
                            , outputPort, withOutputPortIndex, withConsole);

            return true;
        }

        public bool OpenIDEbyUser(out IIDE ide)
        {
            throw new NotImplementedException();
        }

        public bool SaveIDEbyUser(IIDE ide)
        {
            throw new NotImplementedException();
        }

        public bool SaveIDEasByUser(IIDE ide)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CloseIDEbyUser(IIDE ide, bool immediately = false) // = null, DialogResult autoSave = DialogResult.None)
        {
            if (ide == null)
                throw new NullReferenceException();

            if (ide.Machine.Status != VirtualMachineRunningStatus.StandBy)
            {
                IStateShottable ssm = ide.Machine as IStateShottable;
                if (ssm != null)
                {
                    DialogResult answer = //(autoSave != DialogResult.None) ? autoSave : 
                            OnAskingUser("Machine "
                            + ((ide.Machine.CustomName.IsNotNullOrEmpty()) ? ide.Machine.CustomName : ide.Machine.DefaultName)
                            + " is running now. Pause it and save state before closing the IDE?", "Unsaved changes in " + ide.Title, MessageBoxButtons.YesNoCancel);
                    switch (answer)
                    {
                        case DialogResult.Yes:
                            await ssm.PauseAsync();
                            if (!ide.SaveMachineStateByUser()) return false;
                            break;
                        case DialogResult.Cancel:
                            return false;
                    }
                }
            }

            if (!ide.ProgramFile.IsChangesSaved)
            {
                DialogResult answer = //(autoSave != DialogResult.None) ? autoSave : 
                        OnAskingUser("Save changes in file "
                        + ide.ProgramFile.ShortFileName + " before closing the IDE?", "Unsaved changes in " + ide.Title, MessageBoxButtons.YesNoCancel);
                switch (answer)
                {
                    case DialogResult.Yes:
                        if (!ide.SaveProgramFileByUser()) return false;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            if (!ide.ConfigFile.IsChangesSaved)
            {
                DialogResult answer = //(autoSave != DialogResult.None) ? autoSave : 
                        OnAskingUser("Save changes in file "
                        + ide.ConfigFile.ShortFileName + " before closing the IDE?", "Unsaved changes in " + ide.Title, MessageBoxButtons.YesNoCancel);
                switch (answer)
                {
                    case DialogResult.Yes:
                        if (!SaveIDEbyUser(ide)) return false;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            await CloseIDE(ide, immediately);

            return true;
        }

        // // Naming service subs:

        public string GetNewProgramFileShortName(IIDE ide)
        {
            if (ide == null)
                throw new NullReferenceException();

            string name;
            name = "New_";
            name += (ide.Machine.CurrentLanguage.IsNotNullOrEmpty()) ? ide.Machine.CurrentLanguage : ide.Machine.BaseLanguage;
            name += "_program";

            int lastnumber = -1;
            int num = -1;
            foreach (IIDE ide_ in IDEs)
            {
                if (ide_.ProgramFile.ShortFileName.IndexOf(name) == 0)
                    if ((ide_.ProgramFile.ShortFileName + "|").Substring(name.Length) == "|")
                    {
                        num = 1;
                    }
                    else if (ide_.ProgramFile.ShortFileName.Length - name.Length > 3
                             && ide_.ProgramFile.ShortFileName.Substring(name.Length, 2) == " ("
                             && XString.Right(ide_.ProgramFile.ShortFileName, 1) == ")")
                    {
                        string snum = ide_.ProgramFile.ShortFileName.Substring(name.Length + 2);
                        snum = snum.Substring(0, snum.Length - 1);
                        if (!Int32.TryParse(snum, out num))
                            num = -1;
                    }
                if (num > lastnumber) lastnumber = num;
            }
            lastnumber++;
            name += (lastnumber > 1) ? " (" + lastnumber.ToString() + ")" : "";

            return name;
        }

        public string GetNewIDEtitle(IIDE ide)
        {
            if (ide == null)
                throw new NullReferenceException();

            string name;
            name = ((ide.Machine.CustomName.IsNotNullOrEmpty()) ? ide.Machine.CustomName : ide.Machine.DefaultName);
            name += " IDE";

            int lastnumber = -1;
            int num = -1;
            foreach (IIDE ide_ in IDEs)
            {
                if (ide_.Title.IndexOf(name) == 0)
                    if ((ide_.Title + "|").Substring(name.Length) == "|")
                    {
                        num = 1;
                    }
                    else if (ide_.Title.Length - name.Length > 3
                             && ide_.Title.Substring(name.Length, 2) == " ("
                             && XString.Right(ide_.Title, 1) == ")")
                    {
                        string snum = ide_.Title.Substring(name.Length + 2);
                        snum = snum.Substring(0, snum.Length - 1);
                        if (!Int32.TryParse(snum, out num))
                            num = -1;
                    }
                if (num > lastnumber) lastnumber = num;
            }
            lastnumber++;
            name += (lastnumber > 1) ? " (" + lastnumber.ToString() + ")" : "";

            return name;
        }
        #endregion

        #region IO Devices service subs
        public void InitializeIOdeviceInIndividualThread(IInputDevice inputDevice)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Name = $"{inputDevice} Input Thread";
                IReinitializeable reinitInputDevice = inputDevice as IReinitializeable;

                bool locked = false;
                try
                {
                    if (!inputDevice.IsInitialized)
                    {
                        if (inputDevice.IsLockingRequiredForInitialize()) IOmonitor.IdleForLock(inputDevice, out locked);
                        inputDevice.Initialize(this);
                    }
                    else if (reinitInputDevice != null)
                    {
                        if (inputDevice.IsLockingRequiredForInitialize()
                            || inputDevice.IsLockingRequiredForDispose()) IOmonitor.IdleForLock(inputDevice, out locked);
                        reinitInputDevice.Reinitialize(this, true);
                    }
                    else
                        return;
                }
                finally
                {
                    if (locked) IOmonitor.Unlock(inputDevice);
                }

                IOmonitor.IdleWhile(() => inputDevice.IsInitialized);
            });
        }

        public void InitializeIOdeviceInIndividualThread(IOutputDevice outputDevice)
        {
            Task.Factory.StartNew(() =>
            {
                Thread.CurrentThread.Name = $"{outputDevice} Output Thread";
                IReinitializeable reinitOutputDevice = outputDevice as IReinitializeable;

                bool locked = false;
                try
                {
                    if (!outputDevice.IsInitialized)
                    {
                        if (outputDevice.IsLockingRequiredForInitialize()) IOmonitor.IdleForLock(outputDevice, out locked);
                        outputDevice.Initialize(this);
                    }
                    else if (reinitOutputDevice != null)
                    {
                        if (outputDevice.IsLockingRequiredForInitialize()
                            || outputDevice.IsLockingRequiredForDispose()) IOmonitor.IdleForLock(outputDevice, out locked);
                        reinitOutputDevice.Reinitialize(this, true);
                    }
                    else
                        return;
                }
                finally
                {
                    if (locked) IOmonitor.Unlock(outputDevice);
                }

                IOmonitor.IdleWhile(() => outputDevice.IsInitialized);
            });
        }
        #endregion
    }
}
