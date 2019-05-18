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

        public ObservableCollection<Point2D> CurveFast1D { get; } = new ObservableCollection<Point2D>();

        public ObservableCollection<Point3D> CurveBlindSearch { get; } = new ObservableCollection<Point3D>();

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
                            m12.Close();
                            Units.Clear();
                        }
                    }
                    catch(Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
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
                        m12.StartFast1D(M12.Definitions.UnitID.U2, 10000, 10, 20, M12.Definitions.ADCChannels.CH2, out List<Point2D> RetValues);
                        m12.Move(M12.Definitions.UnitID.U2, -10000, 20);
                        foreach(var point in RetValues)
                        {
                            CurveFast1D.Add(point);
                        }
                    }
                    catch(Exception ex)
                    {
                        ShowErrorMessageBox(ex.Message);
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

        #endregion

        #region Private Methods

        void ShowErrorMessageBox(string Error)
        {
            MessageBox.Show(Error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        #endregion
    }
}