using GalaSoft.MvvmLight;

namespace M12_GUI.ViewModel
{
    public class IOStatusViewModel : ViewModelBase
    {
        private bool _is_on;

        public bool IsON
        {
            get
            {
                return _is_on;
            }
            set
            {
                _is_on = value;
                RaisePropertyChanged();
            }
        }
    }
}
