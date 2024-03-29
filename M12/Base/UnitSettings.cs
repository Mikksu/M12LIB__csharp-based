﻿using M12.Definitions;
using System;
using System.Collections;
using System.IO;

namespace M12.Base
{
    public class UnitSettings
    {
        public UnitSettings(byte[] Data)
        {
            using (MemoryStream stream = new MemoryStream(Data))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    UnitID = reader.ReadByte();

                    BitArray bits = new BitArray(reader.ReadBytes(1));
                    Mode = bits[0] == false ? ModeEnum.OnePulse : ModeEnum.TwoPulse;
                    PulsePin = bits[1] == false ? PulsePinEnum.CW : PulsePinEnum.CCW;
                    IsFlipDIR = bits[2];
                    IsFlipLimitSensor = bits[3];
                    IsDetectTimming = bits[4];
                    LimitSensorActiveLevel = bits[5] == false ? ActiveLevelEnum.High : ActiveLevelEnum.Low;
                    IsFlipIOActiveLevel = bits[6];

                    GeneralAcceleration = reader.ReadUInt16();
                }
            }
        }

        public UnitSettings(ModeEnum Mode, PulsePinEnum PulsePin, 
            bool IsFlipDIR, bool IsFlipLimitSensor, bool IsDetectTimming, 
            ActiveLevelEnum LSActiveLevel, bool IsFlipIOActiveLevel)
        {
            this.Mode = Mode;
            this.PulsePin = PulsePin;
            this.IsFlipDIR = IsFlipDIR;
            this.IsFlipLimitSensor = IsFlipLimitSensor;
            this.IsDetectTimming = IsDetectTimming;
            LimitSensorActiveLevel = LSActiveLevel;
            this.IsFlipIOActiveLevel = IsFlipIOActiveLevel;
        }

        public int UnitID { get; set; }
        public ModeEnum Mode { get; set; }
        public PulsePinEnum PulsePin { get; set; }
        public bool IsFlipDIR { get; set; }
        public bool IsFlipLimitSensor { get; set; }
        public bool IsDetectTimming { get; set; }
        public ActiveLevelEnum LimitSensorActiveLevel { get; set; }
        public bool IsFlipIOActiveLevel { get; set; }
        
        public UInt16 GeneralAcceleration { get; set; }

        #region Methods

        public byte[] ToArray()
        {
            byte ret = 0;

            ret |= (byte)((Mode == ModeEnum.OnePulse ? 0 : 1) << 0);
            ret |= (byte)((PulsePin == PulsePinEnum.CW ? 0 : 1) << 1);
            ret |= (byte)((IsFlipDIR == false ? 0 : 1) << 2);
            ret |= (byte)((IsFlipLimitSensor == false ? 0 : 1) << 3);
            ret |= (byte)((IsDetectTimming == false ? 0 : 1) << 4);
            ret |= (byte)((LimitSensorActiveLevel == ActiveLevelEnum.High ? 0 : 1) << 5);
            ret |= (byte)((IsFlipIOActiveLevel == false ? 0 : 1) << 6);

            return new byte[] { ret };
        }

        public override string ToString()
        {
            return $"{Enum.GetName(typeof(ModeEnum), Mode)}, " +
                $"Pulse Pin {Enum.GetName(typeof(PulsePinEnum), PulsePin)}, " +
                $"{(IsFlipDIR ? "DIR Flipped" : "DIR Default") }, " +
                $"{(IsFlipLimitSensor ? "LS Flipped" : "LS Default")}, " +
                $"{(IsDetectTimming ? "Detect Timming" : "Ignore Timming")}, " +
                $"LS {Enum.GetName(typeof(ActiveLevelEnum), LimitSensorActiveLevel)}, " +
                $"{(IsFlipIOActiveLevel? "IO ActLev is flipped": "IO ActLev default")}," +
                $"Move Acc: {GeneralAcceleration} steps";
        }

        #endregion
    }
}
