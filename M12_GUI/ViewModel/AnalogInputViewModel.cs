using GalaSoft.MvvmLight;

namespace M12_GUI.ViewModel
{
    public class AnalogInputViewModel : ViewModelBase
    {
        private double _ana_value;

        public double Value
        {
            get
            {
                return _ana_value;
            }
            set
            {
                _ana_value = value;
                RaisePropertyChanged();
            }
        }
    }
}
