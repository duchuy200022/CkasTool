using CkasTool_MVVM.CkasCommand;
using CkasTool_MVVM.DataAccess;
using CkasTool_MVVM.Models;
using CkasTool_MVVM.ViewModels.Utilities;
using CkasTool_MVVM.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Runtime.InteropServices.Marshalling;
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
        public RelayCommand RefreshPortCommand { get; set; }
        public RelayCommand JoggingWindowCommand { get; set; }
        public RelayCommand RealtimeCommand { get; set; }
        public RelayCommand RecordWindowCommand { get; set; }

        public CancellationTokenSource _ctsRealTime {  get; set; }
        

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
        private bool _isCkasConnect;
        public bool IsCkasConnect { get => _isCkasConnect; set { _isCkasConnect = value; OnPropertyChanged(); } }
        #endregion

        #region Property_btn_carla
        private string _btnCarlaContent;
        public string BtnCarlaContent { get => _btnCarlaContent; set { _btnCarlaContent = value; OnPropertyChanged(); } }
        private bool _isBtnCarlaAvail;
        public bool IsBtnCarlaAvail { get => _isBtnCarlaAvail; set { _isBtnCarlaAvail = value; OnPropertyChanged(); } }
        private bool _isCarlaConnect;
        public bool IsCarlaConnect { get => _isCarlaConnect; set { _isCarlaConnect = value; OnPropertyChanged(); } }
        #endregion

        private string _btnRealtimeContent;
        public string BtnRealtimeContent { get => _btnRealtimeContent; set { _btnRealtimeContent = value; OnPropertyChanged(); } }

        

        public MainViewModel()
        {
            PortCkasList = SerialPort.GetPortNames();
            if(PortCkasList.Count() > 0)
            {
                PortCkasSelected = PortCkasList.First();
            }
            CarlaIp = "127.0.0.1:5000";
            IsBtnCkasAvail = true;
            IsBtnCarlaAvail=true;
            BtnCkasContent = "Connect";
            BtnCarlaContent = "Connect";
            BtnRealtimeContent = "Start Realtime";
            IsCkasConnect = false;
            IsCarlaConnect = false;

            ConnectCkasCommand = new RelayCommand(data => ConnectCkasExecuted(data), data => { return IsBtnCkasAvail; });
            ConnectCarlaCommand = new RelayCommand(data => ConnectCarlaExecuted(data), data => { return IsBtnCarlaAvail; });
            RefreshPortCommand = new RelayCommand(data => RefreshPortExcuted(data), data => true);
            JoggingWindowCommand = new RelayCommand(data => JoggingWindowExcuted(data), data => { return IsCkasConnect; });
            RealtimeCommand = new RelayCommand(data => RealtimeExecuted(data), data => { return IsCkasConnect && IsCarlaConnect; });


            RecordWindowCommand = new RelayCommand (data => RecordWindowExecuted(data), data => { return IsCarlaConnect; } );
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
                        IsCkasConnect=true;
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
                    IsCkasConnect = false;
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
                        IsCarlaConnect = true;
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
                    IsCarlaConnect = false;
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

        private void RefreshPortExcuted (object data)
        {
            try
            {
                PortCkasList = SerialPort.GetPortNames();
                if (PortCkasList.Count() > 0)
                {
                    PortCkasSelected = PortCkasList.First();
                }
                PortCkasSelected = PortCkasList.First();
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
        }

        private void JoggingWindowExcuted (object data)
        {
            try
            {
                JoggingWindow joggingWindow = new JoggingWindow();
                joggingWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }  
        }

        private async void RealtimeExecuted (object data)
        {
            try
            {
                if(BtnRealtimeContent == "Start Realtime")
                {
                    _ctsRealTime = new CancellationTokenSource();
                    BtnRealtimeContent = "Stop Realtime";
                    await Task.Run(async () =>
                    {
                        byte[] responseBytes = new byte[4096];
                        char[] responseChars = new char[4096];
                        while (true)
                        {
                            //
                            _ctsRealTime.Token.ThrowIfCancellationRequested();
                            int bytesReceived = await TcpConnection.Instance.ReceiveAsync(responseBytes, SocketFlags.None, _ctsRealTime.Token);
                            if (bytesReceived == 0)
                            {
                                throw new Exception("No data received");
                            }
                            //
                            _ctsRealTime.Token.ThrowIfCancellationRequested();
                            int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);
                            string dataReceived = new string(responseChars, 0, charCount);
                            dataReceived = $"[{dataReceived}]";
                            List<Carla> carlaTelemetry = JsonConvert.DeserializeObject<List<Carla>>(dataReceived);
                            if (carlaTelemetry == null)
                            {
                                throw new Exception("Wrong data");
                            }
                            //
                            _ctsRealTime.Token.ThrowIfCancellationRequested();
                            foreach (Carla carla in carlaTelemetry)
                            {
                                string cmd = MCode.Move_Cartesian(MCode.modeMoveCartesian.STATIC, Int32.Parse(carla.position[0]),
                                    Int32.Parse(carla.position[1]), Int32.Parse(carla.position[2]),
                                    Int32.Parse(carla.orientation[0]), Int32.Parse(carla.orientation[1]), Int32.Parse(carla.orientation[2]));
                                SerialConnection.Instance.WriteLine(cmd);
                            }
                        }

                    }, _ctsRealTime.Token);
                    
                }
                else
                {
                    _ctsRealTime.Cancel();
                    BtnRealtimeContent = "Start Realtime";
                }
            }
            catch(AggregateException agex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = "Realtime has been stopped";
                errorWindow.ShowDialog();
            }
            catch(Exception ex)
            {
                ResetWindow();
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
        }

        private void RecordWindowExecuted(object data)
        {
            try
            {
                RecordWindow recordWindow = new RecordWindow();
                recordWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
        }

        private void ResetWindow()
        {
            PortCkasList = SerialPort.GetPortNames();
            if (PortCkasList.Count() > 0)
            {
                PortCkasSelected = PortCkasList.First();
            }
            CarlaIp = "127.0.0.1:5000";
            IsBtnCkasAvail = true;
            IsBtnCarlaAvail = true;
            BtnCkasContent = "Connect";
            BtnCarlaContent = "Connect";
            BtnRealtimeContent = "Start Realtime";
            IsCkasConnect = false;
            IsCarlaConnect = false;

            SerialConnection.Instance.Close();
            SerialConnection.Instance = null;

            TcpConnection.Instance.Close();
            TcpConnection.Instance = null;
        }
        
    }
}
