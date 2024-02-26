using CkasTool_MVVM.DataAccess;
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
using System.Windows.Threading;

namespace CkasTool_MVVM.ViewModels
{
    public class RecordViewModel : BaseViewModel
    {
        private string _btnRecordContent;
        public string BtnRecordContent { get => _btnRecordContent; set {  _btnRecordContent = value; OnPropertyChanged(); } }

        private string _textRecord;
        public string TextRecord { get => _textRecord; set { _textRecord = value; OnPropertyChanged(); } }

        private List<string> _textRecordToSave;
        public List<string> TextRecordToSave { get => _textRecordToSave; set { _textRecordToSave = value; OnPropertyChanged(); } }

        public RelayCommand StartRecordCommand { get; set; }
        public RelayCommand ClosingCommand { get; set; }

        private CancellationTokenSource _cts;

        public RecordViewModel()
        {
            BtnRecordContent = "Start Record";
            TextRecord = String.Empty;
            TextRecordToSave = new List<string>();

            StartRecordCommand = new RelayCommand(data => StartRecordExecuted(data), data => true);
            ClosingCommand = new RelayCommand(data => ClosingExecuted(data), data => true);

            BackgroundTcp();
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

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text file (*.txt)|*.txt|C# file (*.cs)|*.cs";
                saveFileDialog.Title = "Save Record File";
                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, append: true))
                    {
                        foreach (string item in TextRecordToSave)
                        {
                            writer.WriteLine(item);
                        }
                        writer.Flush();
                    }
                }
                TextRecordToSave.Clear();
                TextRecord = String.Empty;
            }
        }

        private void ClosingExecuted (object data)
        { 
            BtnRecordContent = "Start Record";
            TextRecordToSave.Clear();
            TextRecord = String.Empty;
        }

        private async Task BackgroundTcp()
        {
            try
            {
                if (!TcpConnection.Instance.Connected)
                {
                    throw new Exception("No TCP connection, Please connect again");
                }

                byte[] responseBytes = new byte[4096];
                char[] responseChars = new char[4096];

                int prev_stamp = 0;
                while (true)
                {

                    int bytesReceived = await TcpConnection.Instance.ReceiveAsync(responseBytes, SocketFlags.None);

                    if (bytesReceived == 0)
                    {
                        throw new Exception("No data received");
                    }
                    int charCount = Encoding.ASCII.GetChars(responseBytes, 0, bytesReceived, responseChars, 0);
                    string dataReceived = new string(responseChars, 0, charCount);
                    dataReceived = $"[{dataReceived}]";
                    List<Carla> carlaTelemetry = JsonConvert.DeserializeObject<List<Carla>>(dataReceived);
                    if (carlaTelemetry == null)
                    {
                        throw new Exception("Wrong data");
                    }
                    if (BtnRecordContent == "Stop Record")
                    {
                        foreach (Carla carla in carlaTelemetry)
                        {
                            int stampWrite = Int32.Parse(carla.stamp) - prev_stamp;
                            prev_stamp = Int32.Parse(carla.stamp);

                            TextRecord += stampWrite + " " + dataReceived + Environment.NewLine;
                            TextRecordToSave.Add($"{stampWrite} {carla.linear_acceleration[0]} {carla.linear_acceleration[1]} " +
                            $"{carla.linear_acceleration[2]} {carla.angular_velocity[0]} {carla.angular_velocity[1]} " +
                            $"{carla.angular_velocity[2]} {carla.position[0]} {carla.position[1]} {carla.position[2]} " +
                            $"{carla.orientation[0]} {carla.orientation[1]} {carla.orientation[2]} {carla.velocity}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BtnRecordContent = "Start Record";
                TextRecordToSave.Clear();
                TextRecord = String.Empty;

                ErrorWindow errorWindow = new ErrorWindow();
                var errorVm = errorWindow.DataContext as ErrorViewModel;
                errorVm.ErrorData = ex.Message;
                errorWindow.ShowDialog();
            }

        }
    }
}
