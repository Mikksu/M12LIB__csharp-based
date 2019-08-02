using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using M12.Base;
using M12.Commands.Alignment;
using M12.Definitions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace M12_GUI.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        #region Variables

        M12.Controller m12;

        CancellationTokenSource cts;

        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}

            PortNames = SerialPort.GetPortNames();
            Units = new ObservableCollection<UnitViewModel>();

            InputIOStatus = new IOStatusViewModel[]
            {
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel()
            };

            OutputIOStatus = new IOStatusViewModel[12]
            {
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel(),
                new IOStatusViewModel()
            };

            AnalogInputValue = new AnalogInputViewModel[8]
            {
                new AnalogInputViewModel(),
                new AnalogInputViewModel(),
                new AnalogInputViewModel(),
                new AnalogInputViewModel(),
                new AnalogInputViewModel(),
                new AnalogInputViewModel(),
                new AnalogInputViewModel(),
                new AnalogInputViewModel()
            };

        }

        #region Properties

        string[] _portNames = null;

        public string[] PortNames
        {
            get
            {
                return _portNames;
            }
            private set
            {
                _portNames = value;
                RaisePropertyChanged();
            }
        }


        string _selectedPort = "";
        public string SelectedPort
        {
            get
            {
                return _selectedPort;
            }
            set
            {
                _selectedPort = value;
                RaisePropertyChanged();
            }
        }

        Version _fwVersion;
        public Version FWVersion
        {
            get => _fwVersion;
            private set
            {
                _fwVersion = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<UnitViewModel> Units
        {
            get;
            private set;
        }

        UnitViewModel _selectedUnit = null;
        public UnitViewModel SelectedUnit
        {
            get => _selectedUnit;
            set
            {
                _selectedUnit = value;

                if (_selectedUnit != null)
                {
                    var state = m12.GetUnitState(_selectedUnit.ID);

                    _selectedUnit.IsInited = state.IsInitialized;
                    _selectedUnit.IsHomed = state.IsHomed;
                    _selectedUnit.IsBusy = state.IsBusy;
                    _selectedUnit.UnitError = state.Error;
                    _selectedUnit.AbsPosition = state.AbsPosition;

                    var settings = m12.GetUnitSettings(_selectedUnit.ID);
                    _selectedUnit.UnitMode = settings.Mode;
                    _selectedUnit.PulsePin = settings.PulsePin;
                    _selectedUnit.FlipMoveDir = settings.IsFlipDIR;
                    _selectedUnit.FlipLS = settings.IsFlipLimitSensor;
                    _selectedUnit.IsDetectTimmingSignal = settings.IsDetectTimming;
                    _selectedUnit.LSActiveLevel = settings.LimitSensorActiveLevel;
                    _selectedUnit.IsFlipIOActiveLevel = settings.IsFlipIOActiveLevel;

                }


                RaisePropertyChanged();
            }
        } 

        public IOStatusViewModel[] InputIOStatus { get; }

        public IOStatusViewModel[] OutputIOStatus { get; }

        public AnalogInputViewModel[] AnalogInputValue { get; }


        public int Fast1D_Range { get; set; } = 10000;

        public int Fast1D_Interval { get; set; } = 50;

        public int Fast1D_Speed { get; set; } = 100;

        public ObservableCollection<Point2D> CurveFast1D { get; } = new ObservableCollection<Point2D>();

        public ObservableCollection<Point3D> CurveBlindSearch { get; } = new ObservableCollection<Point3D>();

        #endregion

        #region Methods

        private void StartBackgroundTask(CancellationToken CancelToken)
        {
            Task.Run(() =>
            {
                while(true)
                {
                    if(m12 != null && m12.IsOpened)
                    {
                        try
                        {
                            var ret = m12.ReadDIN();
                            for (int i = 0; i < ret.Integrated.Length; i++)
                            {
                                InputIOStatus[i].IsON = ret.Integrated[i] == DigitalIOStatus.ON ? true : false;
                            }
                        }
                        catch(Exception)
                        {

                        }

                        try
                        {
                            var ret = m12.ReadADC(
                                ADCChannels.CH1 | 
                                ADCChannels.CH2 | 
                                ADCChannels.CH3 | 
                                ADCChannels.CH4 | 
                                ADCChannels.CH5 | 
                                ADCChannels.CH6 | 
                                ADCChannels.CH7 | 
                                ADCChannels.CH8);
                            for (int i = 0; i < 8; i++)
                            {
                                this.AnalogInputValue[i].Value = ret[i];
                            }
                        }
                        catch(Exception ex)
                        {

                        }
                    }


                    if (CancelToken.IsCancellationRequested)
                        return;

                    Thread.Sleep(10);
                }
            });
        }

        private void StopCatptureIntputIOStatus()
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private void __Sync_OutputStatus_to_Button(DigitalOutputStatus Status)
        {
            for (int i = 0; i < Status.Integrated.Length; i++)
            {
                OutputIOStatus[i].IsON = Status.Integrated[i] == DigitalIOStatus.ON ? true : false;
            }
        }


        #endregion

        #region Commands

        public RelayCommand RefreshPortNamesCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    PortNames = SerialPort.GetPortNames();
                });
            }
        }

        public RelayCommand ConnectCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (m12 == null)
                            m12 = new M12.Controller(SelectedPort, 115200);

                        Units.Clear();

                        m12.Open();
                        var info = m12.GetSystemInfo();


                        for (int i = 1; i <= info.MaxUnit; i++)
                        {
                            Units.Add(new UnitViewModel()
                            {
                                M12 = m12,
                                Caption = i.ToString(),
                                ID = (UnitID)i,
                                FWversion = info.UnitFwInfo[i - 1].FirmwareVersion,
                            });
                        }

                        var stat = m12.ReadDOUT();
                        __Sync_OutputStatus_to_Button(stat);

                        cts = new CancellationTokenSource();
                        StartBackgroundTask(cts.Token);
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
                    }

                });
            }
        }

        public RelayCommand CloseCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        if (m12 != null && m12.IsOpened)
                        {
                            StopCatptureIntputIOStatus();

                            m12.Close();
                            Units.Clear();
                        }
                    }
                    catch(Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
                    }
                    finally
                    {
                        m12 = null;
                    }
                });
            }
        }
        
        public RelayCommand RunFast1DCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        CurveFast1D.Clear();

                        cts.Cancel();
                        Thread.Sleep(10);

                        m12.Move(this.SelectedUnit.ID, -this.Fast1D_Range / 2, (byte)Fast1D_Speed);
                        m12.StartFast1D(this.SelectedUnit.ID, Fast1D_Range, (ushort)Fast1D_Interval, (byte)Fast1D_Speed, M12.Definitions.ADCChannels.CH3, out List<Point2D> RetValues);
                        m12.Move(this.SelectedUnit.ID, -this.Fast1D_Range / 2, (byte)Fast1D_Speed);

                        foreach (var point in RetValues)
                        {
                            CurveFast1D.Add(point);
                        }


                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
                    }
                    finally
                    {
                        cts = new CancellationTokenSource();
                        StartBackgroundTask(cts.Token);
                    }
                });
            }
        }

        public RelayCommand RunBlindSearchCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        CurveBlindSearch.Clear();

                        m12.Move(M12.Definitions.UnitID.U2, 500, 20);

                        BlindSearchArgs arg_h, arg_v;
                        arg_h = new BlindSearchArgs(M12.Definitions.UnitID.U4, 1000, 10, 10, 20);
                        arg_v = new BlindSearchArgs(M12.Definitions.UnitID.U3, 1000, 10, 10, 20);

                        m12.StartBlindSearch(arg_h, arg_v, M12.Definitions.ADCChannels.CH2, out List<Point3D> result);

                        foreach (var point in result)
                            CurveBlindSearch.Add(point);
                    }
                    catch (Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
                    }

                });
            }
        }

        public RelayCommand<int> SetOutputIO
        {
            get
            {
                return new RelayCommand<int>(ch =>
                {
                    try
                    {
                        var stat = m12.ReadDOUT();

                        m12.SetDOUT((DigitalOutput)ch, stat.Integrated[ch - 1] == DigitalIOStatus.OFF ? DigitalIOStatus.ON : DigitalIOStatus.OFF);

                        stat = m12.ReadDOUT();

                        __Sync_OutputStatus_to_Button(stat);
                    }
                    catch(Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
                    }
                });
            }
        }

        #endregion

        #region Private Methods

        void ShowErrorMessageBox(string Error)
        {
            MessageBox.Show(Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}