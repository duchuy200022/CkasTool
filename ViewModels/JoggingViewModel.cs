using CkasTool_MVVM.CkasCommand;
using CkasTool_MVVM.DataAccess;
using CkasTool_MVVM.ViewModels.Utilities;
using CkasTool_MVVM.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CkasTool_MVVM.ViewModels
{
    public class JoggingViewModel : BaseViewModel
    {
        private int _xValue;
        public int XValue { get { return _xValue; } set { _xValue = value; OnPropertyChanged(); } }

        private int _yValue;
        public int YValue { get { return _yValue; } set { _yValue = value; OnPropertyChanged(); } }

        private int _zValue;
        public int ZValue { get { return _zValue; } set { _zValue = value; OnPropertyChanged(); } }

        private int _rollValue;
        public int RollValue { get { return _rollValue; } set { _rollValue = value; OnPropertyChanged(); } }

        private int _pitchValue;
        public int PitchValue { get { return _pitchValue; } set { _pitchValue = value; OnPropertyChanged(); } }

        private int _yawValue;
        public int YawValue { get { return _yawValue; } set { _yawValue = value; OnPropertyChanged(); } }

        public RelayCommand SendDataToCkasCommand { get; set; }

        public JoggingViewModel()
        {
            XValue = 0;
            YValue = 0;
            ZValue = 0;
            RollValue = 0;
            PitchValue = 0;
            YawValue = 0;

            SendDataToCkasCommand = new RelayCommand(data => SendDataToCkasExecuted(data), data => true);
        }

        private void SendDataToCkasExecuted(object data)
        {
            try
            {
                string cmd = MCode.Move_Cartesian(MCode.modeMoveCartesian.STATIC, XValue, YValue, ZValue, RollValue, PitchValue, YawValue);
                SerialConnection.Instance.WriteLine(cmd);
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
        }
    }
}
