using CkasTool_MVVM.CkasCommand;
using CkasTool_MVVM.DataAccess;
using CkasTool_MVVM.Mediator;
using CkasTool_MVVM.Models;
using CkasTool_MVVM.ViewModels.Utilities;
using CkasTool_MVVM.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace CkasTool_MVVM.ViewModels
{
    public class ReplayViewModel : BaseViewModel
    {
        private string _btnReplayContent;
        public string BtnReplayContent { get => _btnReplayContent; set { _btnReplayContent = value; OnPropertyChanged(); } }

        private bool _isBtnStartAvail;
        public bool IsBtnStartAvail { get => _isBtnStartAvail; set { _isBtnStartAvail = value; OnPropertyChanged(); } }

        private string _fileSelected;
        public string FileSelected { get => _fileSelected; set { _fileSelected = value; OnPropertyChanged(); } }

        private string _contentFileSelected;
        public string ContentFileSelected { get => _contentFileSelected; set { _contentFileSelected = value; OnPropertyChanged(); } }

        private bool _isTxtContentAvail;
        public bool IsTxtContentAvail { get => _isTxtContentAvail; set { _isTxtContentAvail = value; OnPropertyChanged(); } }

        private bool _isBtnSelectAvail;
        public bool IsBtnSelectAvail { get => _isBtnSelectAvail; set { _isBtnSelectAvail = value; OnPropertyChanged(); } }

        private bool _isTxtSelectAvail;
        public bool IsTxtSelectAvail { get => _isTxtSelectAvail; set { _isTxtSelectAvail = value; OnPropertyChanged(); } }

        private List<string[]> _dataToReplay;
        public List<string[]> DataToReplay { get => _dataToReplay; set { _dataToReplay = value; OnPropertyChanged(); } }

        public Task TaskSendData;
        public CancellationTokenSource _ctsReplay;

        public RelayCommand StartReplayCommand { get; set; }
        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand ClosingCommand { get; set; }


        public ReplayViewModel()
        {
            BtnReplayContent = "Start Replay";
            DataToReplay = new List<string[]>();
            IsBtnSelectAvail = true;
            IsTxtSelectAvail = true;
            IsBtnStartAvail = false;
            IsTxtContentAvail = false;

            StartReplayCommand = new RelayCommand(data => StartReplayExecuted(data), data => IsBtnStartAvail);
            SelectFileCommand = new RelayCommand(data => SelectFileExecuted(data), data => IsBtnSelectAvail);
            ClosingCommand = new RelayCommand(data => ClosingExecuted(data), data => true);

        }

        private async void StartReplayExecuted(object data)
        {
            
            try
            {
                if(BtnReplayContent == "Start Replay")
                {
                    BtnReplayContent = "Stop Replay";
                    DisableElement();
                    _ctsReplay = new CancellationTokenSource();
                    TaskSendData = Task.Run(async () =>
                    {
                        foreach (var item in DataToReplay)
                        {
                            string cmd = MCode.Move_Cartesian(MCode.modeMoveCartesian.DYNAMIC, mroll: item[7], mpitch: item[8],
                                myaw: item[9], m_axi: item[1], m_ayi: item[2], m_azi: item[3], m_wx: item[4],
                                m_wy: item[5], m_wz: item[6]);
                            await Task.Delay(Int32.Parse(item[0]), _ctsReplay.Token);
                            SerialConnection.Instance.WriteLine(cmd);
                        }
                        HandleSendDataSuccess();

                    }, _ctsReplay.Token);
                    await Task.WhenAll(TaskSendData);
                }
                else
                {
                    BtnReplayContent = "Start Replay";
                    _ctsReplay.Cancel();
                }
            }
            catch(Exception ex)
            {
                _ctsReplay.Dispose();
                BtnReplayContent = "Start Replay";
                EnableElement();

                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();

            }
        }

        private void SelectFileExecuted(object data)
        {
            try
            {
                ContentFileSelected = string.Empty;
                DataToReplay.Clear();
                IsBtnStartAvail = false;
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs|Json file (*.json)|*.json";
                openFileDialog.Title = "Select File To Replay";
                if (openFileDialog.ShowDialog() == true)
                {
                    FileSelected = openFileDialog.FileName;
                    DisableElement();
                    string contentFile = File.ReadAllText(FileSelected);
                    List<Carla> carlaTelemetry = JsonConvert.DeserializeObject<List<Carla>>(contentFile);
                    if (carlaTelemetry == null)
                    {
                        throw new Exception("Wrong data format");
                    }
                    decimal timeStamp = 0;
                    int i = 0;
                    foreach(Carla carla in carlaTelemetry)
                    {
                        ContentFileSelected += $"=>> Time: {carla.stamp} **Linear Acceleration: {carla.linear_acceleration_x} " +
                                $"{carla.linear_acceleration_y} " +
                                $"{carla.linear_acceleration_z} " +
                                $"**Angular Velocity: {carla.angular_velocity_x} {carla.angular_velocity_y} " +
                                $"{carla.angular_velocity_z}" +
                                $"**Orientation: {carla.orientation_roll} {carla.orientation_pitch} {carla.orientation_yaw} " +
                                Environment.NewLine;

                        //Add Data to replay
                        decimal stampWrite = Decimal.Ceiling(decimal.Parse(carla.stamp, NumberStyles.Float)/1000 - timeStamp);
                        

                        if(i == 0)
                        {
                            stampWrite = 0;
                        }
                        timeStamp = decimal.Parse(carla.stamp, NumberStyles.Float) / 1000;

                        decimal angular_velocity_z = decimal.Parse(carla.angular_velocity_z, NumberStyles.Float);
                        decimal angular_velocity_x = decimal.Parse(carla.angular_velocity_x, NumberStyles.Float);
                        decimal angular_velocity_y = decimal.Parse(carla.angular_velocity_y, NumberStyles.Float);
                        decimal linear_acceleration_x = decimal.Parse(carla.linear_acceleration_x, NumberStyles.Float);
                        decimal linear_acceleration_y = decimal.Parse(carla.linear_acceleration_y, NumberStyles.Float);
                        decimal linear_acceleration_z = decimal.Parse(carla.linear_acceleration_z, NumberStyles.Float);
                        decimal orientation_roll = decimal.Parse(carla.orientation_roll, NumberStyles.Float);
                        decimal orientation_pitch = decimal.Parse(carla.orientation_pitch, NumberStyles.Float);
                        decimal orientation_yaw = decimal.Parse(carla.orientation_yaw, NumberStyles.Float);

                        string[] data1 = new string[] { stampWrite.ToString(),linear_acceleration_x.ToString(),
                            linear_acceleration_y.ToString(), linear_acceleration_z.ToString(),
                        angular_velocity_x.ToString(), angular_velocity_y.ToString(), angular_velocity_z.ToString(), 
                            orientation_roll.ToString(),orientation_pitch.ToString(), orientation_yaw.ToString()};
                        DataToReplay.Add(data1);
                        i++;
                    }
                }
                EnableElement();
                IsBtnStartAvail = true;
            }
            catch (Exception ex)
            {
                EnableElement();
                IsBtnStartAvail = true;
                ContentFileSelected = string.Empty;
                IsBtnStartAvail = false;
                DataToReplay.Clear();
                ContentFileSelected = String.Empty;
                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }
        }

        private void DisableElement()
        {
            IsBtnSelectAvail = false;
            IsTxtSelectAvail = false;
        }

        private void EnableElement()
        {
            IsBtnSelectAvail = true;
            IsTxtSelectAvail = true;
        }

        private void HandleSendDataSuccess()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                EnableElement();
                BtnReplayContent = "Start Replay";

                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = "Send Data Success";
                errorVm.Title = "Inform";
                errorWindow.ShowDialog();
            });
        }

        private void ClosingExecuted(object data)
        {
            if(TaskSendData != null && !TaskSendData.IsCanceled)
            {
                _ctsReplay.Cancel();
            }
        }
    }
}
