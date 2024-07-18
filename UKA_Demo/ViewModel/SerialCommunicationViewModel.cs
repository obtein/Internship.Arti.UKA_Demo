using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UKA_Demo.Model;

namespace UKA_Demo.ViewModel
{

    public class SerialCommunicationViewModel : ObservableObject
    {
        // Class Variables
        public event Action<string> DataReceived;

        private SerialCommunicationModel serialCommunicationModel;

        #region Variables-SerialCommunicationViewModel

        #endregion

        #region Variables-SerialCommunicationModel
        private string? deviceID;
        private readonly SerialPort? serialPort;
        private StringBuilder? dataReceiverBuffer;
        private bool isCommunicationActive;
        private string? portName;
        private int? baudRate;
        private Parity? parity;
        private StopBits? stopBit;
        private Handshake? handShake;
        private int? readTimeOut;
        private int? writeTimeOut;

        public string? DeviceID
        {
            get => deviceID;
            set => SetProperty( ref deviceID, value );
        }
        public SerialPort? SerialPort => serialPort;
        public StringBuilder? DataReceiverBuffer
        {
            get => dataReceiverBuffer;
            set => SetProperty( ref dataReceiverBuffer, value );
        }
        public bool IsCommunicationActive
        {
            get => isCommunicationActive;
            set => SetProperty( ref isCommunicationActive, value );
        }
        public string? PortName
        {
            get => portName;
            set => SetProperty( ref portName, value );
        }
        public int? BaudRate
        {
            get => baudRate;
            set => SetProperty( ref baudRate, value );
        }
        public Parity? Parity
        {
            get => parity;
            set => SetProperty( ref parity, value );
        }
        public StopBits? StopBit
        {
            get => stopBit;
            set => SetProperty( ref stopBit, value );
        }
        public Handshake? HandShake
        {
            get => handShake;
            set => SetProperty( ref handShake, value );
        }
        public int? ReadTimeOut
        {
            get => readTimeOut;
            set => SetProperty( ref readTimeOut, value );
        }
        public int? WriteTimeOut
        {
            get => writeTimeOut;
            set => SetProperty( ref writeTimeOut, value );
        }
        #endregion


        #region SerialCommunicationModelTemplate

        #region Methods-SerialCommunication
        public void Open () => serialPort.Open();
        public void Close () => serialPort.Close();
        public void SendData ( string data )
        {
            if ( serialPort.IsOpen )
            {
                serialPort.WriteLine( data );
            }
        }
        private async Task OnDataReceivedAsync ()
        {
            string hexData = await Task.Run( () => serialPort.ReadExisting().ToString() );
            await ProcessReceivedDataAsync( hexData );
        }
        private async Task ProcessReceivedDataAsync ( string hexData )
        {
            await Task.Run( () =>
            {
                if ( hexData == "0x02" )
                {
                    isCommunicationActive = true;
                    dataReceiverBuffer.Clear();
                }

                if ( isCommunicationActive && hexData != "0x02" && hexData != "0x03" )
                {
                    dataReceiverBuffer.Append( hexData );
                }

                if ( hexData == "0x03" && isCommunicationActive )
                {
                    isCommunicationActive = false;
                    string completeHexMessage = dataReceiverBuffer.ToString();
                    //string convertedData = ConvertHexToString( completeHexMessage );
                    DataReceived?.Invoke( completeHexMessage );
                }

            } );
        }

        #endregion
        #endregion


        
    }
}
