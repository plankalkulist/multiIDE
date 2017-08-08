using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel;
using multiIDE.Machines;
using XSpace;
using System.Windows.Forms;
using System.Linq;
using System.Reflection;
using multiIDE.Extras;
using System.Threading;

namespace multiIDE
{
    /// <summary>
    /// Binds all objects associated with one machine together for group managing via the multiIDE form.
    /// </summary>
    public sealed class IDE : IIDE
    {
        #region Profile properties
        [Category("Profile"), ReadOnly(true)]
        public string DefaultName => "IDE";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Version => "1.0.0.0";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Description => "Binds all objects associated with one machine together for group managing via the multiIDE form.";
        //
        [Category("Profile"), ReadOnly(true)]
        public string Author => "";
        #endregion

        #region Environment properties
        [Browsable(false)]
        public IWorkplace ParentWorkplace { get; set; }
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
                        : ((_Machine.CustomName.IsNotNullOrEmpty()) ? _Machine.CustomName
                        : _Machine.ToString()) + " IDE";
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
        [Category("Essential"), ReadOnly(true)]
        public IVirtualMachine Machine
        {
            get { return _Machine; }
            set { _Machine = value; }
        }
        //
        [Category("Essential"), ReadOnly(true)]
        public ICodeEditor CodeEditor
        {
            get { return _CodeEditor; }
            set { _CodeEditor = value; }
        }
        //
        [Browsable(false)]
        public List<IExtraIdeComponent> ExtraComponents
        {
            get { return _ExtraComponents; }
            set { _ExtraComponents = value; }
        }
        //
        [Browsable(false)]
        public SomeFile ConfigFile
        {
            get { return _ConfigFile; }
            set { _ConfigFile = value; }
        }
        //
        [Browsable(false)]
        public SomeFile ProgramFile
        {
            get { return _ProgramFile; }
            set { _ProgramFile = value; }
        }
        //
        [Browsable(false)]
        public SomeFile MachineStateFile
        {
            get { return _MachineStateFile; }
            set { _MachineStateFile = value; }
        }
        //
        [Category("Machine"), ReadOnly(false)]
        public string ProgramLanguage
        {
            get
            {
                return (_Machine.CurrentLanguage.IsNotNullOrEmpty()) ? _Machine.CurrentLanguage : _Machine.TargetLanguage;
            }
            set
            {
                //try!
                _Machine.CurrentLanguage = value;
            }
        }
        //
        [Category("Machine"), ReadOnly(false)]
        public bool AutoResetOnStart { get; set; } = true;
        //
        private IVirtualMachine _Machine;
        private ICodeEditor _CodeEditor;
        private List<IExtraIdeComponent> _ExtraComponents;
        private SomeFile _ConfigFile;
        private SomeFile _ProgramFile;
        private SomeFile _MachineStateFile;
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
        public DialogResult OnAskingUser(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon = MessageBoxIcon.None, MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1)
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

        public override string ToString() => CustomName.IsNotNullOrEmpty()
                ? CustomName : DefaultName + " id" + Id.ToString();

        /////

        private IDE()
        {
            _ExtraComponents = new List<IExtraIdeComponent>();
            _ConfigFile = new SomeFile()
            {
                FileName = "",
                ShortFileName = "",
                IsChangesSaved = true
            };
            _ProgramFile = new SomeFile()
            {
                FileName = "",
                ShortFileName = "",
                IsChangesSaved = true
            };
            _MachineStateFile = new SomeFile()
            {
                FileName = "",
                ShortFileName = "",
                IsChangesSaved = true
            };
        }

        public IDE(string fromFile) : this()
        {
            LoadConfigFile(fromFile);
        }

        public IDE(IVirtualMachine machine, ICodeEditor codeEditor
                , IInputPort actionInputPort = null, IOutputPort actionOutputPort = null)
                : this()
        {
            _Machine = machine;
            _Machine.ParentIDE = this;
            _CodeEditor = codeEditor;
            _CodeEditor.ParentIDE = this;
            _CodeEditor.CodeChanged += CodeEditor_CodeChanged;
            if (actionInputPort != null)
            {
                _Machine.ActionInputPort = actionInputPort;
                _Machine.ActionInputPort.ParentIDE = this;
            }
            if (actionOutputPort != null)
            {
                _Machine.ActionOutputPort = actionOutputPort;
                _Machine.ActionOutputPort.ParentIDE = this;
            }
        }

        public async void Dispose()
        {
            await DisposeAsync();
        }

        public async Task DisposeAsync(bool immediately = false)
        {
            if (_Machine.Status != VirtualMachineRunningStatus.StandBy)
                if (!immediately)
                    await _Machine.BreakAsync();
                else
                    await _Machine.TerminateAsync();

            IMultiIOable mpvm = _Machine as IMultiIOable;
            if (mpvm != null)
            {
                foreach (IInputPort port in mpvm.InputPorts)
                    (port as IDisposable)?.Dispose();
                foreach (IOutputPort port in mpvm.OutputPorts)
                    (port as IDisposable)?.Dispose();
            }
            else
            {
                if (_Machine.ActionInputPort != null)
                    (_Machine.ActionInputPort as IDisposable)?.Dispose();
                if (_Machine.ActionOutputPort != null)
                    (_Machine.ActionOutputPort as IDisposable)?.Dispose();
            }
            (_Machine as IDisposable)?.Dispose();

            if (_ExtraComponents != null)
            {
                foreach (IExtraIdeComponent extraComponent in ExtraComponents
                                                        .Where(e => e is IDisposable))
                    (extraComponent as IDisposable)?.Dispose();
            }
            (_CodeEditor as IDisposable)?.Dispose();
        }

        #region Components managing subs
        public IInputPort NewInputPort(InputPortTypeInfo inputPortTypeInfo)
        {
            return ParentWorkplace.InputPortBuilder.GetNew(inputPortTypeInfo);
        }

        public IOutputPort NewOutputPort(OutputPortTypeInfo outputPortTypeInfo)
        {
            return ParentWorkplace.OutputPortBuilder.GetNew(outputPortTypeInfo);
        }

        public IIdeComponent InitializeNewSettingWindow(IIdeComponent ideComponentToSet)
        {
            IExtraIdeComponent settingWindow = NewExtraIdeComponent
                    (ParentWorkplace.IdeExtraBuilder.RegisteredTypes.Find
                    (i => i.TypeFullName == typeof(SettingWindow).FullName));
            settingWindow.Tag = ideComponentToSet;
            settingWindow.Initialize(this);
            return settingWindow;
        }

        public IExtraIdeComponent NewExtraIdeComponent(ComponentTypeInfo extraComponentTypeInfo)
        {
            IExtraIdeComponent newExtra = ParentWorkplace.IdeExtraBuilder.GetNew(extraComponentTypeInfo);
            newExtra.ParentIDE = this;
            Form newExtraForm = newExtra as Form;
            if (newExtraForm != null)
            {
                newExtraForm.MdiParent = ParentWorkplace.MainForm as Form;
                newExtraForm.FormClosing += this.ExtraIdeComponent_Closing;
            }
            ExtraComponents.Add(newExtra);

            if (newExtra.GetType().GetCustomAttribute<InitializeAfterCreateAttribute>()?.InitializeAfterCreate ?? false)
                newExtra.Initialize(this);

            return newExtra;
        }

        // // //

        private void ExtraIdeComponent_Closing(object sender, EventArgs e)
        {
            ExtraComponents.Remove(sender as IExtraIdeComponent);
            (sender as IDisposable).Dispose();
        }
        #endregion

        #region Environment subs
        private void CodeEditor_CodeChanged(object sender, EventArgs e)
        {
            if (_ProgramFile.IsChangesSaved)
            {
                _ProgramFile.IsChangesSaved = false;
                OnGotUpdated();
            }
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

        #region Program File subs
        public void NewProgramFile(string shortName = "")
        {
            _ProgramFile.FileName = "";
            _ProgramFile.ShortFileName = shortName;
            _ProgramFile.IsChangesSaved = true;
            _ProgramFile.IsReadOnly = false;

            CodeEditor.ClearCode();

            OnGotUpdated();
        }

        public void OpenProgramFile(string fileName)
        {
            string text;
            var fInfo = new FileInfo(fileName);
            using (var reader = new StreamReader(fileName))
            {
                text = reader.ReadToEnd();
            }
            _ProgramFile.FileName = fileName;
            _ProgramFile.ShortFileName = Path.GetFileName(fileName);
            _ProgramFile.IsReadOnly = fInfo.IsReadOnly;

            CodeEditor.CurrentCode = text;
            _ProgramFile.IsChangesSaved = true;

            OnGotUpdated();
        }

        public void SaveProgramFile()
        {
            using (var writer = new StreamWriter(ProgramFile.FileName, false))
            {
                writer.Write(CodeEditor.CurrentCode);
            }
            _ProgramFile.IsChangesSaved = true;

            OnGotUpdated();
        }

        public void SaveProgramFile(string fileName)
        {
            using (var writer = new StreamWriter(fileName, false))
            {
                writer.Write(CodeEditor.CurrentCode);
            }

            OpenProgramFile(fileName);
        }

        // // Driven by user Program file subs:

        public bool NewProgramFileByUser()
        {
            if (!ProgramFile.IsChangesSaved)
            {
                DialogResult answer =
                            OnAskingUser("Save changes in file "
                                + ProgramFile.ShortFileName + " before creating a new one?", "Unsaved changes in " + Title, MessageBoxButtons.YesNoCancel);
                switch (answer)
                {
                    case DialogResult.Yes:
                        if (!SaveProgramFileByUser()) return false;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            NewProgramFile(ParentWorkplace.GetNewProgramFileShortName(this));

            OnGotUpdated();
            return true;
        }

        public bool OpenProgramFileByUser()
        {
            if (!ProgramFile.IsChangesSaved)
            {
                DialogResult answer =
                        OnAskingUser("Save changes in file "
                                   + ProgramFile.ShortFileName + " before creating a new one?", "Unsaved changes in " + Title, MessageBoxButtons.YesNoCancel);
                switch (answer)
                {
                    case DialogResult.Yes:
                        if (!SaveProgramFileByUser()) return false;
                        break;
                    case DialogResult.Cancel:
                        return false;
                }
            }

            var openingFileDialogArgs = new OpenFileByUserEventArgs()
            {
                InitialDirectory = (ProgramFile.FileName.IsNotNullOrEmpty())
                        ? Path.GetDirectoryName(ProgramFile.FileName)
                        : (ParentWorkplace.MainForm.RecentFiles.Count > 2)
                        ? Path.GetDirectoryName(ParentWorkplace.MainForm.RecentFiles[0])
                        : Path.GetDirectoryName(Application.ExecutablePath),
                Filter = Machine.ProgramFileFilter + "|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FilterIndex = Array.IndexOf(Machine.SupportedLanguages.Split(';',
                        ','), Machine.CurrentLanguage) + 1,
                RestoreDirectory = true
            };

            OpeningFileByUser?.Invoke(this, openingFileDialogArgs);

            if (openingFileDialogArgs.Answer == DialogResult.OK)
            {
                try
                {
                    OpenProgramFile(openingFileDialogArgs.FileName);
                    ParentWorkplace.MainForm.AddRecentFile(openingFileDialogArgs.FileName);
                    return true;
                }
                catch (Exception ex)
                {
                    OnShowingMessage("Error: Could not read file from disk. Original error: " + ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool SaveProgramFileByUser()
        {
            if (ProgramFile.FileName.IsNotNullOrEmpty() && !ProgramFile.IsReadOnly)
            {
                try
                {
                    SaveProgramFile();
                    return true;
                }
                catch (ArgumentException ex)
                {
                    OnShowingMessage("Cant save program in " + ProgramFile.FileName + Environment.NewLine + ex.Message);
                    return false;
                }
            }
            else
                return SaveProgramFileAsByUser();
        }

        public bool SaveProgramFileAsByUser()
        {
            var savingFileByUserEventArgs = new SaveFileByUserEventArgs
            {
                FileName = ProgramFile.ShortFileName,
                InitialDirectory = (ProgramFile.FileName.IsNotNullOrEmpty())
                        ? Path.GetDirectoryName(ProgramFile.FileName)
                        : (ParentWorkplace.MainForm.RecentFiles.Count > 2)
                        ? Path.GetDirectoryName(ParentWorkplace.MainForm.RecentFiles[0])
                        : Path.GetDirectoryName(Application.ExecutablePath),
                Filter = Machine.ProgramFileFilter + "|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FilterIndex = Array.IndexOf(Machine.SupportedLanguages.Split(';', ',')
                        , Machine.CurrentLanguage) + 1,
                RestoreDirectory = true
            };

            SavingFileByUser?.Invoke(this, savingFileByUserEventArgs);

            if (savingFileByUserEventArgs.Answer == DialogResult.OK)
            {
                try
                {
                    SaveProgramFile(savingFileByUserEventArgs.FileName);
                    return true;
                }
                catch (ArgumentException ex)
                {
                    OnShowingMessage("Can't save program in " + ProgramFile.FileName + Environment.NewLine + ex.Message);
                    return false;
                }
            }
            else
                return false;
        }
        #endregion

        #region Machine Setting subs
        /// <summary>
        /// Adds or replaces (including deleting) specified IO port of the IDE's machine.
        /// </summary>
        /// <param name="associatedInputPortIndex">Index of machine Input Port replacing (replacing with null = deleting) port-object to or '-1' if replacing Action Input Port or '-2' if adding to free slot.</param>
        /// <param name="inputPort"></param>
        /// <returns></returns>
        public int MachineInsertInputPort(int associatedInputPortIndex = -2, IInputPort inputPort = null)
        {
            if (!(associatedInputPortIndex >= -2 && associatedInputPortIndex < 256))
                throw new ArgumentOutOfRangeException("associatedInputPortIndex", associatedInputPortIndex, "The value is out of range.");
            if (associatedInputPortIndex == -2 && inputPort == null)
                throw new IOPortNullReferenceException("Impossible to add a null Input Port reference.");

            IMultiIOable multiIOableVirtualMachine = Machine as IMultiIOable;
            if (associatedInputPortIndex == -2)
            // seeking for last exist input port, connecting after it:
            {
                if (multiIOableVirtualMachine != null)
                {
                    int i, index;
                    index = -1;
                    for (i = 255; i > -1; i--)
                        if (multiIOableVirtualMachine.InputPorts[i] != null)
                            break;
                    if (i < 255)
                    {
                        index = i + 1;
                    }
                    else
                    {
                        for (i = 254; i > -1; i--)
                            if (multiIOableVirtualMachine.InputPorts[i] == null)
                                break;
                        if (i > -1)
                        {
                            index = i;
                        }
                    }

                    if (index > -1)
                    {
                        inputPort.ParentIDE = this;
                        inputPort.Id = index;
                        multiIOableVirtualMachine.InputPorts[index] = inputPort;
                        return index;
                    }
                    else
                    {
                        throw new NoFreeIOPortSlotsException("Attempt to add input port failed: there are no free input port slots in the machine.");
                    }
                }
                else
                {
                    if (Machine.ActionInputPort == null)
                    {
                        lock (inputPort)
                        {
                            inputPort.ParentIDE = this;
                            inputPort.Id = -1;
                        }
                        Machine.ActionInputPort = inputPort;
                        return -1;
                    }
                    else
                    {
                        throw new NoFreeIOPortSlotsException("ActionInputPort isn't free.");
                    }
                }
            }
            else if (associatedInputPortIndex == -1)
            // Inserting ActionInputPort:
            {
                if (inputPort != null)
                {
                    lock (inputPort)
                    {
                        inputPort.ParentIDE = this;
                        inputPort.Id = multiIOableVirtualMachine?.ActionInputPortIndex ?? -1;
                    }
                }
                Machine.ActionInputPort = inputPort;
                return -1;
            }
            else
            // Inserting input port with specified number:
            {
                if (multiIOableVirtualMachine != null)
                {
                    if (inputPort != null)
                    {
                        lock (inputPort)
                        {
                            inputPort.ParentIDE = this;
                            inputPort.Id = associatedInputPortIndex;
                        }
                    }
                    multiIOableVirtualMachine.InputPorts[associatedInputPortIndex] = inputPort;
                    return associatedInputPortIndex;
                }
                else
                {
                    throw new NotSupportedByMachineException("There's no input ports instead of ActionInputPort.");
                }
            }
        }

        /// <summary>
        /// Adds or replaces (including deleting) Action Input Port of the IDE's machine.
        /// </summary>
        /// <param name="inputPort"></param>
        /// <returns></returns>
        public int MachineInsertInputPort(IInputPort inputPort)
        {
            if (inputPort != null)
                lock (inputPort)
                {
                    inputPort.ParentIDE = this;
                }
            Machine.ActionInputPort = inputPort;
            return -1;
        }

        /// <summary>
        /// Adds or replaces (including deleting) specified IO port of the IDE's machine.
        /// </summary>
        /// <param name="associatedOutputPortIndex">Index of machine Output Port replacing (replacing with null = deleting) port-object to or '-1' if replacing Action Output Port or '-2' if adding to free slot.</param>
        /// <param name="outputPort"></param>
        /// <returns></returns>
        public int MachineInsertOutputPort(int associatedOutputPortIndex = -2, IOutputPort outputPort = null)
        {
            if (!(associatedOutputPortIndex >= -2 && associatedOutputPortIndex < 256))
                throw new ArgumentOutOfRangeException("associatedOutputPortIndex", associatedOutputPortIndex, "The value is out of range.");
            if (associatedOutputPortIndex == -2 && outputPort == null)
                throw new IOPortNullReferenceException("Impossible to add a null Output Port reference.");

            IMultiIOable multiIOableVirtualMachine = Machine as IMultiIOable;
            if (associatedOutputPortIndex == -2)
            // seeking for last exist output port, connecting after it:
            {
                if (multiIOableVirtualMachine != null)
                {
                    int i, index;
                    index = -1;
                    for (i = 255; i > -1; i--)
                        if (multiIOableVirtualMachine.OutputPorts[i] != null)
                            break;
                    if (i < 255)
                    {
                        index = i + 1;
                    }
                    else
                    {
                        for (i = 254; i > -1; i--)
                            if (multiIOableVirtualMachine.OutputPorts[i] == null)
                                break;
                        if (i > -1)
                        {
                            index = i;
                        }
                    }

                    if (index > -1)
                    {
                        outputPort.ParentIDE = this;
                        outputPort.Id = index;
                        multiIOableVirtualMachine.OutputPorts[index] = outputPort;
                        return index;
                    }
                    else
                    {
                        throw new NoFreeIOPortSlotsException("Attempt to add output port failed: there are no free output port slots in the machine.");
                    }
                }
                else
                {
                    if (Machine.ActionOutputPort == null)
                    {
                        lock (outputPort)
                        {
                            outputPort.ParentIDE = this;
                            outputPort.Id = -1;
                        }
                        Machine.ActionOutputPort = outputPort;
                        return -1;
                    }
                    else
                    {
                        throw new NoFreeIOPortSlotsException("ActionOutputPort isn't free.");
                    }
                }
            }
            else if (associatedOutputPortIndex == -1)
            // Inserting ActionOutputPort:
            {
                if (outputPort != null)
                {
                    lock (outputPort)
                    {
                        outputPort.ParentIDE = this;
                        outputPort.Id = multiIOableVirtualMachine?.ActionOutputPortIndex ?? -1;
                    }
                }
                Machine.ActionOutputPort = outputPort;
                return -1;
            }
            else
            // Inserting output port with specified number:
            {
                if (multiIOableVirtualMachine != null)
                {
                    if (outputPort != null)
                    {
                        lock (outputPort)
                        {
                            outputPort.ParentIDE = this;
                            outputPort.Id = associatedOutputPortIndex;
                        }
                    }
                    multiIOableVirtualMachine.OutputPorts[associatedOutputPortIndex] = outputPort;
                    return associatedOutputPortIndex;
                }
                else
                {
                    throw new NotSupportedByMachineException("There's no output ports instead of ActionOutputPort.");
                }
            }
        }

        /// <summary>
        /// Adds or replaces (including deleting) Action Output Port of the IDE's machine.
        /// </summary>
        /// <param name="outputPort"></param>
        /// <returns></returns>
        public int MachineInsertOutputPort(IOutputPort outputPort)
        {
            if (outputPort != null)
                lock (outputPort)
                {
                    outputPort.ParentIDE = this;
                }
            Machine.ActionOutputPort = outputPort;
            return -1;
        }

        // // //

        public void MachineSetInputDevice(int associatedInputPortIndex, IInputDevice inputDevice = null)
        {
            if (!(associatedInputPortIndex >= -1 && associatedInputPortIndex < 256))
                throw new ArgumentOutOfRangeException();

            IMultiIOable multiIOableVirtualMachine = Machine as IMultiIOable;
            if (associatedInputPortIndex == -1)
            {
                if (Machine.ActionInputPort != null)
                    Machine.ActionInputPort.InputDevice = inputDevice;
                else
                    throw new IOPortNullReferenceException();
            }
            else if (multiIOableVirtualMachine != null)
            {
                if (multiIOableVirtualMachine.InputPorts[associatedInputPortIndex] != null)
                    multiIOableVirtualMachine.InputPorts[associatedInputPortIndex].InputDevice = inputDevice;
                else
                    throw new IOPortNullReferenceException();
            }
            else
            {
                throw new NotSupportedByMachineException();
            }
        }

        public void MachineSetInputDevice(IInputPort inputPort, IInputDevice inputDevice = null)
        {
            if (inputPort == null)
                throw new IOPortNullReferenceException();
            inputPort.InputDevice = inputDevice;
        }

        public void MachineSetInputDevice(IInputDevice inputDevice = null)
        {
            if (Machine.ActionInputPort != null)
                Machine.ActionInputPort.InputDevice = inputDevice;
            else
                throw new IOPortNullReferenceException();
        }

        public void MachineSetOutputDevice(int associatedOutputPortIndex, IOutputDevice outputDevice = null)
        {
            if (!(associatedOutputPortIndex >= -1 && associatedOutputPortIndex < 256))
                throw new ArgumentOutOfRangeException();

            IMultiIOable multiIOableVirtualMachine = Machine as IMultiIOable;
            if (associatedOutputPortIndex == -1)
            {
                if (Machine.ActionOutputPort != null)
                    Machine.ActionOutputPort.OutputDevice = outputDevice;
                else
                    throw new IOPortNullReferenceException();
            }
            else if (multiIOableVirtualMachine != null)
            {
                if (multiIOableVirtualMachine.OutputPorts[associatedOutputPortIndex] != null)
                    multiIOableVirtualMachine.OutputPorts[associatedOutputPortIndex].OutputDevice = outputDevice;
                else
                    throw new IOPortNullReferenceException();
            }
            else
            {
                throw new NotSupportedByMachineException();
            }
        }

        public void MachineSetOutputDevice(IOutputPort outputPort, IOutputDevice outputDevice = null)
        {
            if (outputPort == null)
                throw new IOPortNullReferenceException();
            outputPort.OutputDevice = outputDevice;
        }

        public void MachineSetOutputDevice(IOutputDevice outputDevice = null)
        {
            if (Machine.ActionOutputPort != null)
                Machine.ActionOutputPort.OutputDevice = outputDevice;
            else
                throw new IOPortNullReferenceException();
        }
        #endregion

        #region Machine Running subs
        public async Task<bool> MachineStartAsync(VirtualMachineRunningStatus withStatus = VirtualMachineRunningStatus.Runtime)
        {
            VirtualMachineRunResult completeRunning;

            try
            {
                if (!IsMachineConfigurationFine())
                    return false;

                if (Machine.Status == VirtualMachineRunningStatus.StandBy)
                {
                    if (AutoResetOnStart) await Machine.ResetAsync();
                    Machine.LoadProgramCode(CodeEditor.GetProgramCode());

                    IStartWithSpecifiedStatusable startWithSpecifiedStatusableMachine = Machine
                            as IStartWithSpecifiedStatusable;
                    if (startWithSpecifiedStatusableMachine == null
                            || withStatus == VirtualMachineRunningStatus.Runtime)
                        completeRunning = await Machine.StartAsync();
                    else
                        completeRunning = await startWithSpecifiedStatusableMachine.StartAsync(withStatus);

                    // logging:
                    //
                    //
                }
                else
                {
                    await Machine.StartAsync();
                }
            }
            catch (CodePreprocessingException ex)
            {
                OnShowingMessage("A preprocessor exception prevents the machine to Start running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Code Preprocessing Exception in " + CodeEditor.Title, MessageBoxIcon.Error);
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design caused a stop running at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to Start/Continue the machine running at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (UnhandledMachineProgramRuntimeErrorException ex)
            {
                OnShowingMessage($"Unhandled guest program error caused a stop running at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Unhandled Machine Program Runtime Error in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (RunningTaskHasBeenTerminatedException ex)
            {
                OnShowingMessage($"Machine running has been Terminated at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Exclamation);
                return false;
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"An exception caused a stop running at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }

            return true;
        }

        public async Task MachinePauseAsync()
        {
            try
            {
                if (Machine is IPausable)
                    await (Machine as IPausable).PauseAsync();
                else
                    OnShowingMessage("Machine does not support the Pause action."
                                     , "Machine Warning in " + Machine.Title, MessageBoxIcon.Exclamation);
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to Pause the machine running at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"Machine running exception at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }
        }

        public async Task MachineStepAsync()
        {
            try
            {
                ISteppable steppableMachine = Machine as ISteppable;
                if (steppableMachine != null)
                {
                    if (Machine.Status != VirtualMachineRunningStatus.StandBy)
                        await steppableMachine.StepAsync();
                    else
                        MachineStartAsync(VirtualMachineRunningStatus.Pausing);
                }
                else
                {
                    OnShowingMessage("Machine does not support the Step action."
                                     , "Machine Warning in " + Machine.Title, MessageBoxIcon.Exclamation);
                }
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to run the machine by Step at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"Machine running exception at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }
        }

        public async Task MachineStepOverAsync()
        {
            try
            {
                IStepOverable stepOverableMachine = Machine as IStepOverable;
                if (stepOverableMachine != null)
                {
                    if (Machine.Status != VirtualMachineRunningStatus.StandBy)
                        await stepOverableMachine.StepOverAsync();
                    else
                        MachineStartAsync(VirtualMachineRunningStatus.Pausing);
                }
                else
                {
                    OnShowingMessage("Machine does not support the Step Over action."
                                     , "Machine Warning in " + Machine.Title, MessageBoxIcon.Exclamation);
                }
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to run the machine by StepOver at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"Machine running exception at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }
        }

        public async Task MachineBreakAsync()
        {
            try
            {
                await Machine.BreakAsync();
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to Break the machine running at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design exception (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"Machine running exception at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }
        }

        public async Task MachineTerminateAsync()
        {
            try
            {
                await Machine.TerminateAsync();
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to Terminate the machine running at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design exception (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"Machine running exception at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }
        }

        public async Task MachineResetAsync()
        {
            try
            {
                await Machine.ResetAsync();
            }
            catch (MachineNotProgrammedYetException ex)
            {
                OnShowingMessage("Machine is not programmed yet."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (ImpossibleActionAtTheMomentException ex)
            {
                OnShowingMessage($"It is impossible to Break the machine running at the moment (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineSettingException ex)
            {
                OnShowingMessage("Invalid machine setting."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Setting Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineDesignRuntimeException ex)
            {
                OnShowingMessage($"Machine or IDE or both design exception (NextSymbol = {ex.NextSymbol + 1}, ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Design Runtime Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (MachineRunningException ex)
            {
                OnShowingMessage($"Machine running exception at {ex.NextSymbol + 1} (ActionCell = {ex.ActionCell})."
                                 + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Machine Running Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (NullReferenceException ex)
            {
                OnShowingMessage($"A null reference exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "Null Reference Exception in " + Machine.Title, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                OnShowingMessage($"An extraordinary exception caused a stop running."
                        + Environment.NewLine + "Details:" + Environment.NewLine + Environment.NewLine + ex.Message, "EXCEPTION in " + Machine.Title, MessageBoxIcon.Error);
            }
        }

        public async Task<bool> MachineRestartAsync()
        {
            if (Machine.Status != VirtualMachineRunningStatus.StandBy)
            {
                await MachineBreakAsync();
            }
            return await MachineStartAsync();
        }

        // // //

        private bool IsMachineConfigurationFine()
        {
            string omissions = "";

            if (String.IsNullOrEmpty(CodeEditor.CurrentCode))
                omissions += $";{Environment.NewLine}   ● Program Code is empty";

            IMultiIOable multiIOable = Machine as IMultiIOable;
            if (multiIOable == null)
            {
                if (Machine.ActionInputPort == null)
                    omissions += $";{Environment.NewLine}   ● There is no Input Port in the machine";
                else if (Machine.ActionInputPort.InputDevice == null)
                    omissions += $";{Environment.NewLine}   ● There is no Input Device connected to the machine";
                if (Machine.ActionOutputPort == null)
                    omissions += $";{Environment.NewLine}   ● There is no Output Port in the machine";
                else if (Machine.ActionOutputPort.OutputDevice == null)
                    omissions += $";{Environment.NewLine}   ● There is no Output Device connected to the machine";
            }
            else
            {
                if (multiIOable.InputPorts.All(p => p == null))
                    omissions += $";{Environment.NewLine}   ● There are no Input Ports in the machine";
                else if (multiIOable.InputPorts.All(p => p?.InputDevice == null))
                    omissions += $";{Environment.NewLine}   ● There are no Input Devices connected to the machine";
                if (multiIOable.OutputPorts.All(p => p == null))
                    omissions += $";{Environment.NewLine}   ● There are no Output Ports in the machine";
                else if (multiIOable.OutputPorts.All(p => p?.OutputDevice == null))
                    omissions += $";{Environment.NewLine}   ● There are no Output Devices connected to the machine";
            }

            return String.IsNullOrEmpty(omissions) || OnAskingUser
                    ($"You're about to Start running {Machine.Title}, while the configuration seems to be incomplete:"
                    + omissions.Substring(1) + "." + Environment.NewLine + Environment.NewLine
                    + "Do you want to proceed anyway?", "Warning In " + Title
                    , MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
                    == DialogResult.Yes;
        }

        private void AssociatedMachine_Error(object sender, VirtualMachineRunningErrorId errorNumber, out VirtualMachineRunningErrorReaction reaction)
        {
            DialogResult answer = OnAskingUser("Error occured while running the program " + ProgramFile.ShortFileName
                                               + " at symbol " + (Machine.NextSymbol + 1).ToString(), Title + " Program Running Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Exclamation);

            switch (answer)
            {
                case DialogResult.Abort:
                    reaction = VirtualMachineRunningErrorReaction.Pause;
                    break;
                case DialogResult.Retry:
                    reaction = VirtualMachineRunningErrorReaction.TryAgain;
                    break;
                case DialogResult.Ignore:
                    reaction = VirtualMachineRunningErrorReaction.Miss;
                    break;
                default:
                    reaction = VirtualMachineRunningErrorReaction.Undefined;
                    break;
            }
        }
        #endregion

        #region Machine State subs
        public void CopyMachineStateToBuffer()
        {
            throw new NotImplementedException();
        }

        public void PasteMachineStateFromBuffer()
        {
            throw new NotImplementedException();
        }

        public void SaveMachineState()
        {
            throw new NotImplementedException();
        }

        public void SaveMachineState(string fileName)
        {
            throw new NotImplementedException();
        }

        public void LoadMachineStateFromFile(string fileName)
        {
            throw new NotImplementedException();
        }

        // // Driven by user State file subs:

        public bool SaveMachineStateByUser()
        {
            throw new NotImplementedException();
        }

        public bool SaveMachineStateByUser(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool LoadMachineStateFromFileByUser(string fileName)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
