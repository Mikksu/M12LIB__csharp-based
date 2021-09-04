using System;
using System.Collections;
using System.IO;
using M12.Definitions;

namespace M12.Base
{
    public class UnitState
    {
        public UnitState(byte[] data)
        {
            using (var stream = new MemoryStream(data))
            {
                using (var reader = new BinaryReader(stream))
                {
                    UnitID = reader.ReadByte();

                    var bits = new BitArray(reader.ReadBytes(1));
                    IsInitialized = bits[0];
                    IsHomed = bits[1];
                    IsBusy = bits[2];

                    Error = (Errors)reader.ReadByte();

                    AbsPosition = reader.ReadInt32();
                }
            }
        }

        public int UnitID { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsHomed { get; private set; }
        public bool IsBusy { get; private set; }

        public int AbsPosition { get; private set; }

        public Errors Error { get; private set; }

        public override string ToString()
        {
            return $"Unit ID {UnitID}, " +
                $"{(IsInitialized ? "Initialized" : "Not Initialized")}, " +
                $"{(IsHomed ? "Homed" : "Not Homed")}, " +
                $"{(IsBusy ? "Busy" : "Idle")}, " +
                $"{Enum.GetName(typeof(Errors), Error)}, " +
                $"ABS Position: {AbsPosition}";
        }
    }
}
