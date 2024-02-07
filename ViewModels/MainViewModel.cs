using CkasTool_MVVM.DataAccess;
using CkasTool_MVVM.ViewModels.Utilities;
using CkasTool_MVVM.Views;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace CkasTool_MVVM.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand ConnectCkasCommand { get; set; }
        public RelayCommand ConnectCarlaCommand { get; set; }

        #region Property_portCkas
        private IEnumerable<string> _portCkasList;
        public IEnumerable<string> PortCkasList { get { return _portCkasList; } set { _portCkasList = value; OnPropertyChanged(); } }
        private string _portCkasSelected;
        public string PortCkasSelected { get { return _portCkasSelected; } set { _portCkasSelected = value;  OnPropertyChanged(); } }
        #endregion

        #region Property_Carla_Ip
        private string _carlaIp;
        public string CarlaIp { get => _carlaIp; set { _carlaIp = value; OnPropertyChanged(); } }
        #endregion

        #region Property_btn_ckas
        private string _btnCkasContent;
        public string BtnCkasContent { get => _btnCkasContent; set { _btnCkasContent = value; OnPropertyChanged(); } }
        private bool _isBtnCkasAvail;
        public bool IsBtnCkasAvail { get => _isBtnCkasAvail; set { _isBtnCkasAvail = value; OnPropertyChanged(); } }
        #endregion

        #region Property_btn_carla
        private string _btnCarlaContent;
        public string BtnCarlaContent { get => _btnCarlaContent; set { _btnCarlaContent = value; OnPropertyChanged(); } }
        private bool _isBtnCarlaAvail;
        public bool IsBtnCarlaAvail { get => _isBtnCarlaAvail; set { _isBtnCarlaAvail = value; OnPropertyChanged(); } }
        #endregion
        public MainViewModel()
        {
            PortCkasList = SerialPort.GetPortNames();
            PortCkasSelected = PortCkasList.First();
            CarlaIp = "127.0.0.1:5000";
            IsBtnCkasAvail = true;
            IsBtnCarlaAvail=true;
            BtnCkasContent = "Connect";
            BtnCarlaContent = "Connect";

            ConnectCkasCommand = new RelayCommand(data => ConnectCkasExecuted(data), data => { return IsBtnCkasAvail; });
            ConnectCarlaCommand = new RelayCommand(data => ConnectCarlaExecuted(data), data => { return IsBtnCarlaAvail; });
        }

        private void ConnectCkasExecuted (object data)
        {
            
            try
            {
                if(BtnCkasContent == "Connect")
                {
                    if(string.IsNullOrEmpty(PortCkasSelected)) { throw new Exception("No Com Port be selected"); }
                    IsBtnCkasAvail = false;
                    SerialConnection.Instance.PortName = PortCkasSelected.ToString();
                    SerialConnection.Instance.Open();
                    
                    if (SerialConnection.Instance.IsOpen)
                    {
                        BtnCkasContent = "Disconnect";
                    }
                    else
                    {
                        throw new Exception("Connect Com Port unsuccess");
                    }
                }
                else
                {
                    IsBtnCkasAvail = false;
                    SerialConnection.Instance.Close();
                    BtnCkasContent = "Connect";
                }
            }
            catch (Exception ex) {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
            IsBtnCkasAvail = true;
        }
        private void ConnectCarlaExecuted(object data)
        {
            try
            {
                if(BtnCarlaContent == "Connect")
                {
                    IsBtnCarlaAvail = false;
                    Match match = Regex.Match(CarlaIp, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}:\d+\b");
                    if (!match.Success) { throw new Exception("Wrong format of data address. Please input again!"); }

                    string[] address = CarlaIp.Split(':');
                    TcpConnection.Instance.Connect(address[0], Int32.Parse(address[1]));

                    if (TcpConnection.Instance.Connected)
                    {
                        BtnCarlaContent = "Disconnect";
                    }
                    else
                    {
                        throw new Exception("Connect Carla unsuccess");
                    }
                }
                else
                {
                    IsBtnCarlaAvail = false;
                    TcpConnection.Instance.Close();
                    TcpConnection.Instance.Dispose();
                    TcpConnection.Instance = null;
                    BtnCarlaContent = "Connect";
                }
            }
            catch(Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
            IsBtnCarlaAvail = true;
        }
    }
}
