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

        private List<int[]> _dataToReplay;
        public List<int[]> DataToReplay { get => _dataToReplay; set { _dataToReplay = value; OnPropertyChanged(); } }

        public Task TaskSendData;
        public CancellationTokenSource _ctsReplay;

        public RelayCommand StartReplayCommand { get; set; }
        public RelayCommand SelectFileCommand { get; set; }
        public RelayCommand ClosingCommand { get; set; }


        public ReplayViewModel()
        {
            BtnReplayContent = "Start Replay";
            DataToReplay = new List<int[]>();
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
                            string cmd = MCode.Move_Cartesian(MCode.modeMoveCartesian.STATIC, item[1], item[2], item[3], item[4], item[5], item[6]);
                            await Task.Delay(item[0], _ctsReplay.Token);
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
                    int timeStamp = 0;
                    foreach(Carla carla in carlaTelemetry)
                    {
                        ContentFileSelected += $"=>> Time: {carla.stamp} **Linear Acceleration: {carla.linear_acceleration[0]} " +
                                $"{carla.linear_acceleration[1]} " +
                                $"{carla.linear_acceleration[2]} **Angular Velocity: {carla.angular_velocity[0]} {carla.angular_velocity[1]} " +
                                $"{carla.angular_velocity[2]} **Position: {carla.position[0]} {carla.position[1]} {carla.position[2]} " +
                                $"**Orientation: {carla.orientation[0]} {carla.orientation[1]} {carla.orientation[2]} **Velocity: {carla.velocity}" + Environment.NewLine;

                        //Add Data to replay
                        int stampWrite = Int32.Parse(carla.stamp) - timeStamp;
                        timeStamp = Int32.Parse(carla.stamp);
                        int[] data1 = new int[] { stampWrite,Int32.Parse(carla.position[0]), Int32.Parse(carla.position[1]), Int32.Parse(carla.position[2]),
                        Int32.Parse(carla.orientation[0]), Int32.Parse(carla.orientation[1]), Int32.Parse(carla.orientation[2])};
                        DataToReplay.Add(data1);
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
