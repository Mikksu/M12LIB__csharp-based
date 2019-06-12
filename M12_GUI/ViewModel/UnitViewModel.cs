using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using M12.Base;
using M12.Definitions;
using System;
using System.Threading;
using System.Threading.Tasks;
using static M12.Base.UnitSettings;

namespace M12_GUI.ViewModel
{
    public class UnitViewModel : ViewModelBase
    {
        #region Variables

        M12.Controller _controller;

        string _caption = "";
        UnitID _id = UnitID.INVALID;
        int _absPosition = 0;
        int _stepsToMove = 0;
        int _speed = 100;

        Version _version;

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

        
        bool _isInited = false;
        public bool IsInited
        {
            get => _isInited;
            set
            {
                _isInited = value;
                RaisePropertyChanged();
            }
        }

        bool _isHomed = false;
        public bool IsHomed
        {
            get => _isHomed;
            set
            {
                _isHomed = value;
                RaisePropertyChanged();
            }
        }

        bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged();
            }
        }

        Errors _unitErr = Errors.ERR_NONE;
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

        ModeEnum _unitMode = ModeEnum.TwoPulse;
        public ModeEnum UnitMode
        {
            get => _unitMode;
            set
            {
                _unitMode = value;
                RaisePropertyChanged();
            }
        }

        PulsePinEnum _plsPin = PulsePinEnum.CCW;
        public PulsePinEnum PulsePin
        {
            get => _plsPin;
            set
            {
                _plsPin = value;
                RaisePropertyChanged();
            }
        }

        bool _flipDir = false;
        public bool FlipMoveDir
        {
            get => _flipDir;
            set
            {
                _flipDir = value;
                RaisePropertyChanged();
            }
        }

        bool _flipLS = false;
        public bool FlipLS
        {
            get => _flipLS;
            set
            {
                _flipLS = value;
                RaisePropertyChanged();
            }
        }

        bool _enTimming = false;
        public bool IsDetectTimmingSignal
        {
            get => _enTimming;
            set
            {
                _enTimming = value;
                RaisePropertyChanged();
            }
        }

        ActiveLevelEnum _activeOfLS = ActiveLevelEnum.Low;
        public ActiveLevelEnum LSActiveLevel
        {
            get => _activeOfLS;
            set
            {
                _activeOfLS = value;
                RaisePropertyChanged();
            }
        }

        bool _isFlipIOActiveLevel = false;
        public bool IsFlipIOActiveLevel
        {
            get => _isFlipIOActiveLevel;
            set
            {
                _isFlipIOActiveLevel = value;
                RaisePropertyChanged();
            }
        }

        int _acc = 500;
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
                    M12.SaveUnitENV(this.ID);
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
                            M12.Home(this.ID, 5, 50, 2000);
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
