using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.ComponentModel;
namespace UKA_Demo.Model
{
    public class SerialCommunicationModel
    {
        public event Action<string> DataReceived;

        #region privateVariables
        /// <summary>
        /// Declaring variables needed to create a serial connection
        /// </summary>

        private string deviceID;
        private readonly SerialPort serialPort;
        private StringBuilder dataReceiverBuffer;
        private bool isCommunicationActive;
        private string portName;
        private readonly int[] baudRates = { 110, 300, 600, 1200, 2400, 4800, 9600, 14400, 19200, 38400, 57600, 115200, 128000, 256000 };
        private int baudRate;
        private readonly Parity [] parityList = {Parity.Even, Parity.None,Parity.Odd, Parity.Mark,Parity.Space };
        private Parity parity;
        private readonly StopBits [] stopBits = { System.IO.Ports.StopBits.None, System.IO.Ports.StopBits.One, System.IO.Ports.StopBits.OnePointFive, System.IO.Ports.StopBits.Two };
        private StopBits stopBit;
        private readonly Handshake [] handShakes = {Handshake.None, Handshake.XOnXOff, Handshake.RequestToSend,Handshake.RequestToSendXOnXOff };
        private Handshake handShake;
        private int readTimeOut;
        private int writeTimeOut;
        #endregion

        #region GettersSetters
        public string PortName
        {
            get => portName;
            set => portName =  value ;
        }

        public SerialPort SerialPort => serialPort;

        public StringBuilder DataReceiverBuffer
        {
            get => dataReceiverBuffer;
            set => dataReceiverBuffer = value;
        }
        public bool IsCommunicationActive
        {
            get => isCommunicationActive;
            set => isCommunicationActive =  value ;
        }
        public string DeviceID
        {
            get => deviceID;
            set => deviceID =  value ;
        }
        public int ReadTimeOut
        {
            get => readTimeOut;
            set => readTimeOut = value;
        }
        public int WriteTimeOut
        {
            get => writeTimeOut;
            set => writeTimeOut =  value ;
        }

        public int [] BaudRate => baudRates;

        public Parity [] ParityList => parityList;

        public StopBits [] StopBits => stopBits;

        public Handshake [] HandShakes => handShakes;

        public int SBaudRate
        {
            get => baudRate;
            set => baudRate =  value ;
        }
        public Parity Parity
        {
            get => parity;
            set => parity =  value ;
        }
        public StopBits StopBit
        {
            get => stopBit;
            set => stopBit =  value ;
        }
        public Handshake HandShake
        {
            get => handShake;
            set => handShake =  value ;
        }
        #endregion

        #region Constructor
        public SerialCommunicationModel ( 
            string deviceID,string portName, int baudRateIndex, 
            int parityListIndex, int stopBitsIndex, 
            int handShakesIndex, int readTimeOut, int writeTimeOut 
            )
        {
            this.SBaudRate = baudRates [baudRateIndex];
            this.serialPort = new SerialPort(portName,this.SBaudRate);
            this.deviceID = deviceID;
            this.dataReceiverBuffer = new StringBuilder();
            serialPort.Parity = parityList[parityListIndex];
            serialPort.StopBits = stopBits[stopBitsIndex];
            serialPort.Handshake = handShakes[handShakesIndex];
            serialPort.ReadTimeout = readTimeOut;
            serialPort.WriteTimeout = writeTimeOut;
            serialPort.DataReceived += async ( sender, e ) => await OnDataReceivedAsync();
            IsCommunicationActive = false;
            SendData("hello");
        }
        #endregion

        #region Methods
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

        private string ConvertHexToString ( string hexData )
        {
            try
            {
                byte [] bytes = new byte [hexData.Length / 2];
                for ( int i = 0; i < hexData.Length; i += 2 )
                {
                    bytes [i / 2] = Convert.ToByte( hexData.Substring( i, 2 ), 16 );
                }
                return Encoding.UTF8.GetString( bytes );
            }
            catch ( Exception ex )
            {
                return $"Error converting hex to string: {ex.Message}";
            }
        }

        #endregion
    }
}
