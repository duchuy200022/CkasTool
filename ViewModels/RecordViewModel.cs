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
using System.Net.Sockets;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CkasTool_MVVM.ViewModels
{
    public class RecordViewModel : BaseViewModel
    {
        private string _btnRecordContent;
        public string BtnRecordContent { get => _btnRecordContent; set {  _btnRecordContent = value; OnPropertyChanged(); } }

        private string _textRecord;
        public string TextRecord { get => _textRecord; set { _textRecord = value; OnPropertyChanged(); } }


        private string _textRecordToSave;
        public string TextRecordToSave { get => _textRecordToSave; set { _textRecordToSave = value; OnPropertyChanged(); } }

        public RelayCommand StartRecordCommand { get; set; }
        public RelayCommand ClosingCommand { get; set; }

        private CancellationTokenSource _cts;

        public Action CloseAction { get; set; }

        public int TimeStamp {  get; set; }

        public Task taskTcp { get; set; }

        public RecordViewModel()
        {
            BtnRecordContent = "Start Record";
            TextRecord = String.Empty;
            TextRecordToSave = String.Empty;

            StartRecordCommand = new RelayCommand(data => StartRecordExecuted(data), data => true);
            ClosingCommand = new RelayCommand(data => ClosingExecuted(data), data => true);

            TimeStamp = 0;
            if (TcpConnection.Instance.Connected)
            {
                BackgroundTcp();
            }
        }

        private async void StartRecordExecuted (object data)
        {
            var win = data as RecordWindow;
            if (BtnRecordContent == "Start Record")
            {
                BtnRecordContent = "Stop Record";
            }
            else
            {
                BtnRecordContent = "Start Record";
                SaveData();
            }
        }

        private void ClosingExecuted (object data)
        {
            BtnRecordContent = "Start Record";
            SaveData();
            _cts.Cancel();
            _cts.Dispose();
        }

        private async Task BackgroundTcp()
        {
            try
            {
                _cts = new CancellationTokenSource();
                taskTcp = Task.Run(async () =>
                {
                    byte[] responseBytes = new byte[4096];
                    char[] responseChars = new char[4096];

                    TimeStamp = 0;
                    int i = 1;
                    while (true)
                    {
                        int bytesReceived = await TcpConnection.Instance.ReceiveAsync(responseBytes, SocketFlags.None, _cts.Token);
                        if (bytesReceived == 0)
                        {
                            throw new Exception("Tcp connection had been close by server");
                        }
                        int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);
                        string dataReceived = new string(responseChars, 0, charCount);
                        string dataText = $"[{dataReceived}]";
                        List<Carla> carlaTelemetry = JsonConvert.DeserializeObject<List<Carla>>(dataText);
                        if (carlaTelemetry == null)
                        {
                            throw new Exception("Wrong data format");
                        }
                        if (BtnRecordContent == "Stop Record")
                        {
                            foreach (Carla carla in carlaTelemetry)
                            {
                                int stampWrite = Int32.Parse(carla.stamp) - TimeStamp;
                                TimeStamp = Int32.Parse(carla.stamp);

                                TextRecord += $"Message {i} =>> Time: {carla.stamp} **Linear Acceleration: {carla.linear_acceleration[0]} " +
                                $"{carla.linear_acceleration[1]} " +
                                $"{carla.linear_acceleration[2]} **Angular Velocity: {carla.angular_velocity[0]} {carla.angular_velocity[1]} " +
                                $"{carla.angular_velocity[2]} **Position: {carla.position[0]} {carla.position[1]} {carla.position[2]} " +
                                $"**Orientation: {carla.orientation[0]} {carla.orientation[1]} {carla.orientation[2]} **Velocity: {carla.velocity}" + Environment.NewLine;

                                TextRecordToSave += dataReceived;
                                i++;
                            }
                        }
                    }
                }, _cts.Token);
                await Task.WhenAll(taskTcp);
            }
            catch (Exception ex)
            {
                if(taskTcp.IsCanceled)
                {
                    Mediator.Mediator.NotifyColleagues(Event.TcpError, "Record had been cancelled");
                    return;
                }
                else
                {
                    Mediator.Mediator.NotifyColleagues(Event.TcpError, ex.Message.ToString());
                    CloseAction();
                    return;
                }           
            }
        }

        private void SaveData()
        {
            if (!string.IsNullOrEmpty(TextRecord))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs|Json file (*.json)|*.json";
                saveFileDialog.Title = "Save Record File";
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.Write($"[{TextRecordToSave}]");
                    }
                }
            }
            TextRecordToSave = String.Empty;
            TextRecord = String.Empty;
        }
    }
}
