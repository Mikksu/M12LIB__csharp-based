using System;
using System.Collections.Generic;
using System.IO;
using STM32F407;

namespace M12.Packages
{
    public class RxPackage : IDisposable
    {
        const int MAX_BUFFER_SIZE = 256;
        const byte HEADER = 0x7E;
        const byte API_IDENTIFIER_DATA = 0x69;
        const int SIZE_HEADER = 1;
        const int SIZE_LENGTH = 2;
        const int SIZE_API_ID = 1;
        const int SIZE_FRAME_ID = 2;
        const int SIZE_COMMAND = 1;
        const int SIZE_CRC = 4;
        const int SIZE_OVERALL_COST = SIZE_HEADER + SIZE_LENGTH + SIZE_CRC;

        byte[] buffer;
        MemoryStream memStream;
        BinaryWriter bwr;

        public EventHandler<byte[]> OnPackageReceived;

        public RxPackage()
        {
            IsHeaderReceived = false;
            LengthExpected = -1;
            IsPackageFound = false;
            IsPassCRC = false;

            buffer = new byte[MAX_BUFFER_SIZE];
            memStream = new MemoryStream();
            bwr = new BinaryWriter(memStream);

        }

        #region Properties

        private bool IsHeaderReceived { get; set; }

        private int LengthExpected { get; set; }

        public bool IsPackageFound { get; private set; }

        public bool IsPassCRC { get; private set; }

        public M12.Commands.CommandDef Command { get; private set; }

        public byte API_Identifier { get; private set; }

        public ushort FrameID { get; private set; }

        public UInt32 CRC { get; private set; }

        public byte[] Payload { get; private set; }


        #endregion
        
        #region Methods

        public void AddData(byte data)
        {
            if (data == HEADER && IsHeaderReceived == false)
            {
                /*
                * If the package header is received. package is null 
                * this is the header identifier rather than the payload.
                */

                bwr.Write(data);
                IsHeaderReceived = true;
            }
            else if (IsHeaderReceived)
            {
                if (memStream.Length > 0)
                {
                    /*
                     * This is the payload, put it into the byte array.
                     */

                    bwr.Write(data);

                    // The length of the package has received.
                    if (memStream.Length == 3)
                    {
                        var tmp = memStream.ToArray();
                        LengthExpected = tmp[1] + tmp[2] * 256;
                    }
                    else if (LengthExpected > 0)
                    {
                        // We received a complete package
                        if (memStream.Length == LengthExpected + SIZE_OVERALL_COST)
                        {
                            //TODO too verbose without pointer?
                            buffer = memStream.ToArray();

                            var crc_expected = new CRC32().Calculate(buffer, LengthExpected + SIZE_OVERALL_COST - SIZE_CRC);
                            var crc_bytes = new byte[4];
                            Buffer.BlockCopy(buffer, LengthExpected + SIZE_OVERALL_COST - SIZE_CRC, crc_bytes, 0, 4);
                            var crc_origin = BitConverter.ToUInt32(crc_bytes, 0);

                            if (crc_expected == crc_origin)
                            {
                                memStream.Seek(0, SeekOrigin.Begin);

                                using (BinaryReader reader = new BinaryReader(memStream))
                                {
                                    // drop Header
                                    reader.ReadByte();

                                    // drop length
                                    reader.ReadUInt16();

                                    // API Identifier
                                    this.API_Identifier = reader.ReadByte();

                                    // Frame ID
                                    this.FrameID = reader.ReadUInt16();

                                    // Command
                                    this.Command = (Commands.CommandDef)reader.ReadByte();

                                    // payload
                                    this.Payload = reader.ReadBytes(LengthExpected - SIZE_API_ID - SIZE_FRAME_ID - SIZE_COMMAND);
                                }

                                IsPassCRC = true;
                                this.CRC = crc_expected;

                                OnPackageReceived?.Invoke(this, Payload);
                                
                            }
                            else
                            {
                                IsPassCRC = false;
                            }

                            IsPackageFound = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Return the bytes arrary of the received package.
        /// </summary>
        /// <returns></returns>
        public byte[] ToArray()
        {
            if (IsPackageFound)
                return memStream.ToArray();
            else
                return null;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    bwr.Close();
                    memStream.Close();
                    buffer = null;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~UartPackageRx() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
}
