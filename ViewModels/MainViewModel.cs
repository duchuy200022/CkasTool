using CkasTool_MVVM.CkasCommand;
using CkasTool_MVVM.DataAccess;
using CkasTool_MVVM.Mediator;
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
        public RelayCommand ReplayWindowCommand { get; set; }

        public CancellationTokenSource _ctsRealTime {  get; set; }
        

        #region Property_portCkas
        private List<string> _portCkasList;
        public List<string> PortCkasList { get { return _portCkasList; } set { _portCkasList = value; OnPropertyChanged(); } }
        private string _portCkasSelected;
        public string PortCkasSelected { get { return _portCkasSelected; } set { _portCkasSelected = value;  OnPropertyChanged(); } }
        private bool _isPortCkasAvail;
        public bool IsPortCkasAvail { get => _isPortCkasAvail; set { _isPortCkasAvail = value; OnPropertyChanged(); } }
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

        private bool _isBtnRefreshAvail;
        public bool isBtnRefreshAvail { get => _isBtnRefreshAvail; set { _isBtnRefreshAvail = value; OnPropertyChanged(); } }

        private bool _isBtnRecordAvail;
        public bool IsBtnRecordAvail { get => _isBtnRecordAvail; set { _isBtnRecordAvail = value; OnPropertyChanged(); } }

        private bool _isBtnReplayAvail;
        public bool IsBtnReplayAvail { get => _isBtnReplayAvail; set { _isBtnReplayAvail = value; OnPropertyChanged(); } }

        private bool _isBtnJoggingAvail;
        public bool IsBtnJoggingAvail { get => _isBtnJoggingAvail; set { _isBtnJoggingAvail = value; OnPropertyChanged(); } }

        private string _btnRealtimeContent;
        public string BtnRealtimeContent { get => _btnRealtimeContent; set { _btnRealtimeContent = value; OnPropertyChanged(); } }

        public Task TaskRealTime { get; set; }

        public MainViewModel()
        {
            PortCkasList = SerialPort.GetPortNames().ToList();
            if(PortCkasList.Count() > 0)
            {
                PortCkasSelected = PortCkasList.First();
            }
            CarlaIp = "127.0.0.1:5000";
            IsBtnCkasAvail = true;
            IsBtnCarlaAvail=true;
            IsPortCkasAvail = true;
            isBtnRefreshAvail = true;
            BtnCkasContent = "Connect";
            BtnCarlaContent = "Connect";
            BtnRealtimeContent = "Start Realtime";
            IsCkasConnect = false;
            IsCarlaConnect = false;
            IsBtnRecordAvail = false;
            IsBtnJoggingAvail = false;
            IsBtnReplayAvail = false;

            ConnectCkasCommand = new RelayCommand(data => ConnectCkasExecuted(data), data => { return IsBtnCkasAvail; });
            ConnectCarlaCommand = new RelayCommand(data => ConnectCarlaExecuted(data), data => { return IsBtnCarlaAvail; });
            RefreshPortCommand = new RelayCommand(data => RefreshPortExcuted(data), data => isBtnRefreshAvail);
            JoggingWindowCommand = new RelayCommand(data => JoggingWindowExcuted(data), data => { return IsBtnJoggingAvail; });
            RealtimeCommand = new RelayCommand(data => RealtimeExecuted(data), data => { return IsCkasConnect && IsCarlaConnect; });
            RecordWindowCommand = new RelayCommand (data => RecordWindowExecuted(data), data => { return IsBtnRecordAvail; } );
            ReplayWindowCommand = new RelayCommand(data => ReplayWindowExecuted(data), data => { return IsBtnReplayAvail; } );

            Mediator.Mediator.Register(Event.TcpError, TcpErrorHandler);
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
                        IsCkasConnect = true;
                        IsBtnReplayAvail = true;
                        IsBtnJoggingAvail = true;
                    }
                    else
                    {
                        throw new Exception("Connect Com Port unsuccess");
                    }
                }
                else
                {
                    IsBtnCkasAvail = false;
                    IsBtnReplayAvail = false;
                    IsBtnJoggingAvail = false;
                    SerialConnection.Instance.Close();
                    BtnCkasContent = "Connect";
                    IsCkasConnect = false;
                }
                IsBtnCkasAvail = true;
            }
            catch (Exception ex) {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
                IsBtnReplayAvail = false;
                IsBtnJoggingAvail = false;
                SerialConnection.Instance.Close();
                BtnCkasContent = "Connect";
                IsCkasConnect = false;
                IsBtnCkasAvail = true;
            }
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
                        IsBtnRecordAvail = true;
                    }
                    else
                    {
                        throw new Exception("Connect Carla unsuccess");
                    }
                }
                else
                {
                    IsBtnCarlaAvail = false;
                    IsBtnRecordAvail = false;
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
                PortCkasList.Clear();
                PortCkasList = SerialPort.GetPortNames().ToList();
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

        private async void RealtimeExecuted(object data)
        {
            try
            {
                if(BtnRealtimeContent == "Start Realtime")
                {
                    BlockWindow();
                    BtnRealtimeContent = "Stop Realtime";
                    _ctsRealTime = new CancellationTokenSource();
                    TaskRealTime = Task.Run(async () =>
                    {
                        byte[] responseBytes = new byte[4096];
                        char[] responseChars = new char[4096];

                        while (true)
                        {
                            int bytesReceived = await TcpConnection.Instance.ReceiveAsync(responseBytes, SocketFlags.None, _ctsRealTime.Token);
                            if (bytesReceived == 0)
                            {
                                throw new Exception("Tcp connection had been close by server");
                            }
                            int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);
                            string dataReceived = new string(responseChars, 0, charCount);
                            string dataText = $"[{dataReceived}]";
                            List<Carla>? carlaTelemetry = JsonConvert.DeserializeObject<List<Carla>>(dataText);
                            if (carlaTelemetry == null)
                            {
                                throw new Exception("Wrong data format");
                            }
                            foreach (Carla carla in carlaTelemetry)
                            {
                                string cmd = MCode.Move_Cartesian(MCode.modeMoveCartesian.DYNAMIC, m_axi: carla.linear_acceleration_x,
                                    m_ayi: carla.linear_acceleration_y, m_azi: carla.linear_acceleration_z,
                                    m_wx: carla.angular_velocity_x, m_wy: carla.angular_velocity_y, m_wz: carla.angular_velocity_z,
                                    mroll: carla.orientation_roll, mpitch: carla.orientation_pitch, myaw: carla.orientation_yaw);
                                SerialConnection.Instance.WriteLine(cmd);
                            }
                        }
                    }, _ctsRealTime.Token);
                    await Task.WhenAll(TaskRealTime);
                }
                else
                {
                    BtnRealtimeContent = "Start Realtime";
                    _ctsRealTime.Cancel();

                    //reconnect TCP
                    IsBtnCarlaAvail = false;
                    TcpConnection.Instance.Close();
                    TcpConnection.Instance = null;

                    string[] address = CarlaIp.Split(':');
                    TcpConnection.Instance.Connect(address[0], Int32.Parse(address[1]));
                    if (TcpConnection.Instance.Connected)
                    {
                        BtnCarlaContent = "Disconnect";
                        IsCarlaConnect = true;
                        IsBtnRecordAvail = true;
                        IsBtnCarlaAvail = true;
                    }
                }
            }
            catch(Exception ex)
            {
                if(!TaskRealTime.IsCanceled)
                {
                    _ctsRealTime.Cancel();
                }
                if(ex.Message == "Tcp connection had been close by server")
                {
                    TcpConnection.Instance.Close();
                    TcpConnection.Instance = null;

                    IsCarlaConnect = false;
                    BtnCarlaContent = "Connect";
                    IsBtnCarlaAvail = true;
                }

                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;

                if (ex.Source == "Newtonsoft.Json")
                {
                    errorVm.ErrorData = "Wrong data format";
                }
                UnlockWindow();
                BtnRealtimeContent = "Start Realtime";
                _ctsRealTime.Dispose();
                
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

        private void ReplayWindowExecuted(object data)
        {
            try
            {
                ReplayWindow replayWindow = new ReplayWindow();
                replayWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
        }

        private void TcpErrorHandler(object data)
        {
            //MessageBox.Show("Tcp Error Handler");
            string message = data as string;

            TcpConnection.Instance.Close();
            TcpConnection.Instance = null;

            IsCarlaConnect = false;
            BtnCarlaContent = "Connect";
            IsBtnCarlaAvail = true;

            if(message != "Tcp connection had been close by server")
            {
                string[] address = CarlaIp.Split(':');
                TcpConnection.Instance.Connect(address[0], Int32.Parse(address[1]));
                if (TcpConnection.Instance.Connected)
                {
                    BtnCarlaContent = "Disconnect";
                    IsCarlaConnect = true;
                    IsBtnRecordAvail = true;
                    IsBtnCarlaAvail = true;
                }
            }

            ErrorWindow errorWindow = new ErrorWindow();
            var errorVm = errorWindow.DataContext as ErrorViewModel;
            
            errorVm.ErrorData = message;
            errorWindow.ShowDialog();
        }

        private void BlockWindow()
        {
            IsBtnCkasAvail = false;
            IsBtnCarlaAvail = false;
            IsBtnRecordAvail = false;
            isBtnRefreshAvail = false;
            IsPortCkasAvail = false;
            IsBtnReplayAvail = false;
            IsBtnJoggingAvail = false;
        }

        private void UnlockWindow()
        {
            IsBtnCkasAvail = true;
            IsBtnCarlaAvail = true;
            IsBtnRecordAvail = true;
            isBtnRefreshAvail = true;
            IsPortCkasAvail = true;
            IsBtnReplayAvail = true;
            IsBtnJoggingAvail = true;
        }

    }
}
