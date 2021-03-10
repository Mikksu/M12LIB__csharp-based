using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using M12.Base;
using M12.Definitions;
using System;
using System.Threading.Tasks;

namespace M12_GUI.ViewModel
{
    public class UnitViewModel : ViewModelBase
    {
        #region Variables

        private M12.Controller _controller;

        private string _caption = "";
        private UnitID _id = UnitID.INVALID;
        private int _absPosition = 0;
        private int _stepsToMove = 0;
        private int _speed = 100, _homeSpeedLow = 20, _homeSpeedHigh = 100;

        private Version _version;

        #endregion

        public M12.Controller M12
        {
            get => _controller;
            set
            {
                _controller = value;

                _controller.OnUnitStateUpdated += (s, e) =>
                {
                    this.IsInited = e.IsInitialized;
                    this.IsHomed = e.IsHomed;
                    this.IsBusy = e.IsBusy;
                    this.UnitError = e.Error;
                    this.AbsPosition = e.AbsPosition;
                };

                RaisePropertyChanged();
            }
        }

        #region Unit Status Properties

        private bool _isInited = false;
        public bool IsInited
        {
            get => _isInited;
            set
            {
                _isInited = value;
                RaisePropertyChanged();
            }
        }

        private bool _isHomed = false;
        public bool IsHomed
        {
            get => _isHomed;
            set
            {
                _isHomed = value;
                RaisePropertyChanged();
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        private Errors _unitErr = Errors.ERR_NONE;
        public Errors UnitError
        {
            get => _unitErr;
            set
            {
                _unitErr = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        #region Unit Settings Properties

        private ModeEnum _unitMode = ModeEnum.TwoPulse;
        public ModeEnum UnitMode
        {
            get => _unitMode;
            set
            {
                _unitMode = value;
                RaisePropertyChanged();
            }
        }

        private PulsePinEnum _plsPin = PulsePinEnum.CCW;
        public PulsePinEnum PulsePin
        {
            get => _plsPin;
            set
            {
                _plsPin = value;
                RaisePropertyChanged();
            }
        }

        private bool _flipDir = false;
        public bool FlipMoveDir
        {
            get => _flipDir;
            set
            {
                _flipDir = value;
                RaisePropertyChanged();
            }
        }

        private bool _flipLS = false;
        public bool FlipLS
        {
            get => _flipLS;
            set
            {
                _flipLS = value;
                RaisePropertyChanged();
            }
        }

        private bool _enTimming = false;
        public bool IsDetectTimmingSignal
        {
            get => _enTimming;
            set
            {
                _enTimming = value;
                RaisePropertyChanged();
            }
        }

        private ActiveLevelEnum _activeOfLS = ActiveLevelEnum.Low;
        public ActiveLevelEnum LSActiveLevel
        {
            get => _activeOfLS;
            set
            {
                _activeOfLS = value;
                RaisePropertyChanged();
            }
        }

        private bool _isFlipIOActiveLevel = false;
        public bool IsFlipIOActiveLevel
        {
            get => _isFlipIOActiveLevel;
            set
            {
                _isFlipIOActiveLevel = value;
                RaisePropertyChanged();
            }
        }

        private int _acc = 500;
        public int Acceleration
        {
            get => _acc;
            set
            {
                _acc = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                RaisePropertyChanged();
            }
        }


        public UnitID ID
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        public int AbsPosition
        {
            get => _absPosition;
            set
            {
                _absPosition = value;
                RaisePropertyChanged();
            }
        }

        public int StepsToMove
        {
            get => _stepsToMove;
            set
            {
                _stepsToMove = value;
                RaisePropertyChanged();
            }
        }

        public int Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                RaisePropertyChanged();
            }
        }

        public int HomeSpeedLow
        {
            get => _homeSpeedLow;
            set
            {
                _homeSpeedLow = value;
                RaisePropertyChanged();
            }
        }

        public int HomeSpeedHigh
        {
            get => _homeSpeedHigh;
            set
            {
                _homeSpeedHigh = value;
                RaisePropertyChanged();
            }
        }

        public Version FWversion
        {
            get => _version;
            set
            {
                _version = value;
                RaisePropertyChanged();
            }
        }

        #region Commands

        public RelayCommand ChangeUnitSettingsCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    UnitSettings settings = new UnitSettings(this.UnitMode, this.PulsePin, this.FlipMoveDir, this.FlipLS, this.IsDetectTimmingSignal, this.LSActiveLevel, this.IsFlipIOActiveLevel);
                    M12.ChangeUnitSettings(this.ID, settings);
                    M12.SaveUnitEnv(this.ID);
                });
            }
        }

        public RelayCommand SetAccelerationCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    M12.SetAccelerationSteps(this.ID, (ushort)this.Acceleration);
                });
            }
        }

        public RelayCommand HomeCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            M12.Home(this.ID, 20, 100, 2000);
                        });
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send<GenericMessage<Exception>>(new GenericMessage<Exception>(ex));
                    }
                });
            }
        }

        public RelayCommand MoveBackwardCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            M12.Move(this.ID, -this.StepsToMove, (byte)this.Speed);
                        });
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send<GenericMessage<Exception>>(new GenericMessage<Exception>(ex));
                    }
                });
            }
        }

        public RelayCommand MoveForwardCommand
        {
            get
            {
                return new RelayCommand(async () =>
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            M12.Move(this.ID, this.StepsToMove, (byte)this.Speed);
                        });
                    }
                    catch(Exception ex)
                    {
                        Messenger.Default.Send<GenericMessage<Exception>>(new GenericMessage<Exception>(ex));
                    }
                }); 
            }
        }

        public RelayCommand StopCommand
        {
            get
            {
                return new RelayCommand(() =>
                {
                    try
                    {
                        M12.Stop(this.ID);
                    }
                    catch (Exception ex)
                    {
                        Messenger.Default.Send<GenericMessage<Exception>>(new GenericMessage<Exception>(ex));
                    }
                });
            }
        }

        #endregion

        public override string ToString()
        {
            return $"{Caption}, {FWversion}";
        }


    }
}
