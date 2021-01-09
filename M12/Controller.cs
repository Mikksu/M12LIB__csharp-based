using M12.Base;
using M12.Commands.Alignment;
using M12.Commands.Analog;
using M12.Commands.General;
using M12.Commands.IO;
using M12.Commands.Memory;
using M12.Commands.Motion;
using M12.Definitions;
using M12.Exceptions;
using M12.Interfaces;
using M12.Packages;
using M12.ProgressReport;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace M12
{
    public sealed class Controller : IDisposable
    {
        #region Definitions

        /// <summary>
        /// The timeout value of the read method in millisecond.
        /// </summary>
        private const int DEFAULT_READ_TIMEOUT = 5000;

        /// <summary> 
        /// The timeout value of the wait method of the long-duration-operation in millisecond such as home, move.
        /// </summary>
        private const int DEFAULT_WAIT_BUSY_TIMEOUT = 5000;

        /// <summary>
        /// The timeout value of the waiting of alignment process.
        /// </summary>
        // private const int DEFAULT_WAIT_ALIGNMENT_TIMEOUT = 60000;

        #endregion

        #region Public Delegate Definitions

        public event EventHandler<UnitState> OnUnitStateUpdated;

        #endregion

        #region Variables

        private readonly SerialPort _port;

        private readonly object _lockController = new object();

        private readonly IProgress<UnitState> _unitStateChangedProgress;

        #endregion

        #region Constructors

        public Controller(string portName, int baudRate)
        {
            _port = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);

            // unit state updated and raise the 
            _unitStateChangedProgress = new Progress<UnitState>(state =>
            {
                OnUnitStateUpdated?.Invoke(this, state);
            });
        }

        #endregion

        #region Properties

        public bool IsOpened => _port.IsOpen;

        #endregion

        #region User APIs

        // ReSharper disable once MemberCanBePrivate.Global
        public SystemLastError GetLastError()
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandGetLastError());
                Read(out package, CancellationToken.None);
            }
            var err = new SystemLastError(package.Payload);
            return err;
        }

        /// <summary>
        /// Get the system information including maximum unit, firmware version.
        /// </summary>
        /// <returns></returns>
        public SystemInformation GetSystemInfo()
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandGetSystemInfo());
                Read(out package, CancellationToken.None);
            }

            var sysInfo = new SystemInformation(package.Payload);
            return sysInfo;
        }

        /// <summary>
        /// Get the state of the system including IsEmergencyButtonPressed, IsBusy, etc.
        /// </summary>
        /// <returns></returns>
        public SystemState GetSystemState()
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandGetSystemState());
                Read(out package, CancellationToken.None);
            }
            
            var state = new SystemState(package.Payload);
            return state;

        }

        /// <summary>
        /// Get the settings of the specified channel including Mode, IsFlipDir, etc.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public UnitSettings GetUnitSettings(UnitID unitId)
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandGetUnitSettings(unitId));
                Read(out package, CancellationToken.None);
            }

            var settings = new UnitSettings(package.Payload);
            return settings;
        }

        /// <summary>
        /// Get the state of the specified channel including IsHomed, IsBusy, Error, Position, etc.
        /// </summary>
        /// <param name="unitId"></param>
        /// <returns></returns>
        public UnitState GetUnitState(UnitID unitId)
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandGetUnitState(unitId));
                Read(out package, CancellationToken.None);
            }

            var state = new UnitState(package.Payload);
            return state;
        }

        /// <summary>
        /// Set the general acceleration of the specified channel.
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="accelerationSteps"></param>
        public void SetAccelerationSteps(UnitID unitId, ushort accelerationSteps)
        {
            if (accelerationSteps <= GlobalDefinition.MAX_ACC_STEPS)
            {
                lock (_lockController)
                {
                    Send(new CommandSetAccSteps(unitId, accelerationSteps));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(accelerationSteps), "the acceleration is too large.");
            }
        }

        /// <summary>
        /// Set the mode of the specified channel.
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="settings"></param>
        public void ChangeUnitSettings(UnitID unitId, UnitSettings settings)
        {
            if (settings != null)
            {
                lock (_lockController)
                {
                    Send(new CommandSetMode(unitId, settings));
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException( nameof(settings), "the settings object can not be null.");
            }
        }

        /// <summary>
        /// Enable or disable the specified CSS interrupt.
        /// NOTE: The CSS INT will be disabled automatically after it's triggered, so it should
        /// be enabled again if you want to it work the next time.
        /// </summary>
        /// <param name="css"></param>
        /// <param name="isEnabled"></param>
        public void SetCssEnable(CSSCH css, bool isEnabled)
        {
            lock(_lockController)
            {
                Send(new CommandSetCSSEnable(css, isEnabled));
            }
        }

        /// <summary>
        /// Set the threshold of the CSS interrupt.
        /// </summary>
        /// <param name="css"></param>
        /// <param name="lVth">The voltage of the low threshold in mV</param>
        /// <param name="hVth">The voltage of the high threshold in mV</param>
        public void SetCssThreshold(CSSCH css, ushort lVth, ushort hVth)
        {
            lock (_lockController)
            {
                Send(new CommandSetCSSThreshold(css, lVth, hVth));
            }
        }

        /// <summary>
        /// Set the status of the specified digital output port.
        /// </summary>
        /// <param name="channel">DOUT1 to DOUT8</param>
        /// <param name="status">OFF:0; ON:1</param>
        public void SetDout(DigitalOutput channel, DigitalIOStatus status)
        {
            lock (_lockController)
            {
                Send(new CommandSetDOUT(channel, status));
            }
        }

        /// <summary>
        /// Read the status of all the digital output ports.
        /// </summary>
        /// <returns></returns>
        public DigitalOutputStatus ReadDout()
        {
            RxPackage package;

            lock(_lockController)
            {
                Send(new CommandReadDOUT());
                Read(out package, CancellationToken.None);
            }

            var status = new DigitalOutputStatus(package.Payload);
            return status;
        }

        /// <summary>
        /// Read the status of the specified digital output port.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public DigitalIOStatus ReadDout(DigitalOutput channel)
        {
            var stat = ReadDout();
            return stat.Integrated[(int)channel - 1];
        }

        /// <summary>
        /// Read the status of the digital input IOs.
        /// </summary>
        /// <returns></returns>
        public DigitalInputStatus ReadDin()
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandReadDIN());
                Read(out package, CancellationToken.None);
            }

            var input = new DigitalInputStatus(package.Payload);
            return input;
        }

        /// <summary>
        /// Read the values of the specified channels of the inner ADC.
        /// </summary>
        /// <param name="channelEnabled">Concat multiple channels with `|` operator.</param>
        /// <returns></returns>
        public double[] ReadAdc(ADCChannels channelEnabled)
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandReadADC(channelEnabled));
                Read(out package, CancellationToken.None);
            }

            var adc = new ADCValues(channelEnabled, package.Payload);

            // convert adc raw-data to mV.
            var valConv = new List<double>();
            foreach (var v in adc.Values)
                valConv.Add(ConvertAdcRawTomV(v));

            return valConv.ToArray();
        }

        /// <summary>
        /// Set the OSR of the ADC7606.
        /// </summary>
        /// <param name="osr"></param>
        public void SetOsr(ADC_OSR osr)
        {
            lock (_lockController)
            {
                Send(new CommandSetOSR(osr));
            }

            Thread.Sleep(100);
        }

        /// <summary>
        /// Save the ENV of the specified Unit to the flash.
        /// </summary>
        /// <param name="unitId"></param>
        public void SaveUnitEnv(UnitID unitId)
        {
            lock (_lockController)
            {
                Send(new CommandSaveUnitENV(unitId));
            }

            Thread.Sleep(100);
        }

        #region Motion Control

        /// <summary>
        /// Home the specified channel.
        /// </summary>
        /// <param name="unitId">ID of Unit</param>
        /// <param name="lowSpeed">The speed of stage 2</param>
        /// <param name="highSpeed">The speed of stage 1</param>
        /// <param name="acc"></param>
        public void Home(UnitID unitId, byte lowSpeed = 5, byte highSpeed = 5, ushort acc = 500)
        {
            Errors err;

            // check arguments
            if (lowSpeed > 100)
                lowSpeed = 100;
            else if (lowSpeed == 0)
                lowSpeed = 1;

            if (highSpeed > 100)
                highSpeed = 100;
            else if (highSpeed == 0)
                highSpeed = 1;

            lock (_lockController)
            {
                Send(new CommandHome(unitId, acc, lowSpeed, highSpeed));
            }

            try
            {
                err = WaitByUnitState(unitId, 500);
            }
            catch (TimeoutException)
            {
                lock (_lockController)
                {
                    Send(new CommandStop(unitId));
                }
                throw;
            }

            if (err != Errors.ERR_NONE)
                throw new UnitErrorException(unitId, err);
        }

        /// <summary>
        /// Move the specified channel.
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="steps"></param>
        /// <param name="speed"></param>
        public void Move(UnitID unitId, int steps, byte speed)
        {
            Errors err;

            // check arguments
            if (speed > 100)
                speed = 100;
            else if (speed == 0)
                speed = 1;

            lock (_lockController)
            {
                Send(new CommandMove(unitId, steps, speed));
            }

            try
            {
                err = WaitByUnitState(unitId, 50);
            }
            catch(TimeoutException)
            {
                lock(_lockController)
                {
                    Send(new CommandStop(unitId));
                }
                throw;
            }

            if (err != Errors.ERR_NONE)
                throw new UnitErrorException(unitId, err);
        }

        /// <summary>
        /// Fast move the specified axis.
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="steps"></param>
        /// <param name="speed"></param>
        /// <param name="microsteps"></param>
        public void FastMove(UnitID unitId, int steps, byte speed, ushort microsteps)
        {
            Errors err;

            // check arguments
            if (speed > 100)
                speed = 100;
            else if (speed == 0)
                speed = 1;

            lock (_lockController)
            {
                Send(new CommandFastMove(unitId, steps, speed, microsteps));
            }

            try
            {
                err = WaitByUnitState(unitId, 50);
            }
            catch (TimeoutException)
            {
                lock (_lockController)
                {
                    Send(new CommandStop(unitId));
                }
                throw;
            }

            if (err != Errors.ERR_NONE)
                throw new UnitErrorException(unitId, err);
        }

        /// <summary>
        /// Move the specified channel and capture the ADC value.
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="steps"></param>
        /// <param name="speed"></param>
        /// <param name="triggerInterval"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void MoveTriggerAdc(UnitID unitId, int steps, byte speed, ushort triggerInterval)
        {
            Errors err;

            // check arguments
            if (speed > 100)
                speed = 100;
            else if (speed == 0)
                speed = 1;

            lock (_lockController)
            {
                Send(new CommandMoveTriggerADC(unitId, steps, speed, triggerInterval));
            }

            try
            {
                err = WaitByUnitState(unitId);
            }
            catch (TimeoutException)
            {
                lock (_lockController)
                {
                    Send(new CommandStop(unitId));
                }
                throw;
            }

            if (err != Errors.ERR_NONE)
                throw new UnitErrorException(unitId, err);
        }

        /// <summary>
        /// Stop the moving channel.
        /// </summary>
        /// <param name="unitId"></param>
        public void Stop(UnitID unitId)
        {
            lock (_lockController)
            {
                Send(new CommandStop(unitId));
            }
        }

        #endregion

        #region Trigger Source Config

        /// <summary>
        /// Config the ADC Trigger.
        /// </summary>
        /// <param name="channelEnabled"></param>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ConfigAdcTrigger(ADCChannels channelEnabled)
        {
            lock (_lockController)
            {
                Send(new CommandConfigADCTrigger(channelEnabled));
            }
        }

        #endregion

        #region Alignment

        /// <summary>
        /// Perform the fast-1D alignment with one analog capture channel activated.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="range"></param>
        /// <param name="interval"></param>
        /// <param name="speed"></param>
        /// <param name="analogCapture"></param>
        /// <param name="scanResults"></param>
        public void StartFast1D(UnitID unit, int range, ushort interval, byte speed, ADCChannels analogCapture, out List<Point2D> scanResults)
        {
            StartFast1D(unit, range, interval, speed, analogCapture, out scanResults, 0, out _);
        }

        /// <summary>
        /// Perform the fast-1D alignment with two analog capture channel activated.
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="range"></param>
        /// <param name="interval"></param>
        /// <param name="speed"></param>
        /// <param name="analogCapture"></param>
        /// <param name="scanResults"></param>
        /// <param name="analogCapture2"></param>
        /// <param name="scanResults2"></param>
        public void StartFast1D(UnitID unit, int range, ushort interval, byte speed, ADCChannels analogCapture,
            out List<Point2D> scanResults, ADCChannels analogCapture2, out List<Point2D> scanResults2)
        {
            if(GlobalDefinition.NumberOfSetBits((int)analogCapture) != 1) 
                throw new ArgumentException("only 1 analog capture channel can be activated.", nameof(analogCapture));

            if(GlobalDefinition.NumberOfSetBits((int)analogCapture2) != 1)
                throw new ArgumentException("only 1 analog capture channel can be activated.", nameof(analogCapture2));

            var dir = range < 0 ? -1 : 1;

            scanResults = new List<Point2D>();

            scanResults2 = null;
            if(analogCapture2 != 0)
                scanResults2 = new List<Point2D>();

            ConfigAdcTrigger(analogCapture | analogCapture2);

            ClearMemory();

            MoveTriggerAdc(unit, range, speed, interval);

            // read sampling points from the memory.
            var adcValues = ReadMemoryAll().ToList();

            var x = 0;
            var indexOfAdcValues = 0;

            while (true)
            {
                scanResults.Add(indexOfAdcValues > (adcValues.Count - 1)
                    ? new Point2D(x, 0)
                    : new Point2D(x, adcValues[indexOfAdcValues++]));

                if (analogCapture2 != 0)
                {
                    scanResults2?.Add(indexOfAdcValues > (adcValues.Count - 1)
                        ? new Point2D(x, 0)
                        : new Point2D(x, adcValues[indexOfAdcValues++]));
                }

                x += (dir * interval);

                if (Math.Abs(x) >= Math.Abs(range))
                {
                    break;
                }
            }

            #region Convert negative coordinates to positive coordinates.

            var minPos = scanResults.Min(p => p.X);
            foreach (var p in scanResults)
            {
                p.X -= minPos;
            }

            if (analogCapture2 != 0 && scanResults2 != null)
            {
                minPos = scanResults2.Min(p => p.X);
                foreach (var p in scanResults2)
                {
                    p.X -= minPos;
                }
            }

            #endregion
            
            // ran too fast, some ADC value missed.
            var pointsDesired = (int) Math.Ceiling((double) range / interval);
            if (pointsDesired != scanResults.Count)
                throw new ADCSamplingPointMissException(pointsDesired,  scanResults.Count);
        }

        /// <summary>
        /// Perform the blind-search.
        /// </summary>
        /// <param name="horizontalArgs">The arguments of the horizontal axis.</param>
        /// <param name="verticalArgs">The arguments of the vertical axis.</param>
        /// <param name="adcUsed">Note: only one ADC channel can be used to sample.</param>
        /// <param name="scanResults">Return the intensity-to-position points.</param>
        /// <param name="progressReportHandle"></param>
        public void StartBlindSearch(BlindSearchArgs horizontalArgs, BlindSearchArgs verticalArgs, ADCChannels adcUsed,
            out List<Point3D> scanResults, IProgress<BlindSearchProgressReport> progressReportHandle = null)
        {
            //! The memory is cleared automatically, you don't have to clear it manually.

            // arguments checking.
            if(GlobalDefinition.NumberOfSetBits((int)adcUsed) != 1)
                throw new ArgumentException("only 1 analog capture channel can be activated.", nameof(adcUsed));

            if (horizontalArgs.Gap < horizontalArgs.Interval)
                throw new ArgumentException($"the capture interval of {horizontalArgs.Unit} should be less than the gap.");

            if (verticalArgs.Gap < verticalArgs.Interval)
                throw new ArgumentException($"the capture interval of {verticalArgs.Unit} should be less than the gap.");

            scanResults = new List<Point3D>();

            ConfigAdcTrigger(adcUsed);

            // report progress.
            progressReportHandle?.Report(new BlindSearchProgressReport(BlindSearchProgressReport.ProgressStage.SCAN, 0));

            lock (_lockController)
            {
                Send(new CommandBlindSearch(horizontalArgs, verticalArgs));
            }
                        
            var err = WaitBySystemState(200, 120000, new List<UnitID>() { horizontalArgs.Unit, verticalArgs.Unit });

            if (err.Error != Errors.ERR_NONE)
            {
                throw new SystemErrorException(err);
            }
            else
            {
                //var values = new List<double>(new double[100000]);
                // read the sampling points from the memory.
                var adcValues = ReadMemoryAll(new Progress<MemoryReadProgressReport>(e =>
                {
                    // report transmission progress.
                    progressReportHandle?.Report(new BlindSearchProgressReport(BlindSearchProgressReport.ProgressStage.TRANS, e.Complete));
                })).ToList();
                
                var indexOfAdcValues = 0;
                var cycle = 0;
                double x = 0, y = 0;

                // rebuild the relationship between the position and the intensity.
                while (true)
                {
                    var seq = cycle % 4;

                    BlindSearchArgs activeParam;
                    int moveDirection;
                    switch (seq)
                    {
                        case 0:     // move horizontal axis (x) to positive direction (right)
                            activeParam = horizontalArgs;
                            moveDirection = 1;
                            break;

                        case 1:
                            activeParam = verticalArgs;
                            moveDirection = 1;
                            break;

                        case 2:
                            activeParam = horizontalArgs;
                            moveDirection = -1;
                            break;

                        case 3:
                            activeParam = verticalArgs;
                            moveDirection = -1;
                            break;
                        
                        default:
                            throw new InvalidOperationException("unknown sequence in the blind search.");
                    }

                    var steps = moveDirection * (activeParam.Gap * (cycle / 2 + 1));

                    if (Math.Abs(steps) > activeParam.Range)
                        break;

                    // for the horizontal axis.
                    if (activeParam == horizontalArgs)
                    {
                        var originPos = x;

                        while (true)
                        {
                            scanResults.Add(indexOfAdcValues > (adcValues.Count - 1)
                                ? new Point3D(x, y, 0)
                                : new Point3D(x, y, adcValues[indexOfAdcValues++]));

                            x += moveDirection * activeParam.Interval;

                            if (!(Math.Abs(x - originPos) >= Math.Abs(steps))) continue;
                            
                            x = originPos + steps;
                            break;
                        }
                    }
                    else if (activeParam == verticalArgs)
                    {
                        var originPos = y;

                        while (true)
                        {
                            scanResults.Add(indexOfAdcValues > (adcValues.Count - 1)
                                ? new Point3D(x, y, 0)
                                : new Point3D(x, y, adcValues[indexOfAdcValues++]));

                            y += moveDirection * activeParam.Interval;

                            if (!(Math.Abs(y - originPos) >= Math.Abs(steps))) continue;
                            
                            y = originPos + steps;
                            break;
                        }
                    }

                    cycle++;

                }

                //// output debug data.
                //StringBuilder sb = new StringBuilder();
                //foreach(var point in ScanResults)
                //{
                //    sb.Append($"{point.X}\t{point.Y}\t{point.Z}\r\n");
                //}

                if (scanResults.Count != adcValues.Count)
                    throw new ADCSamplingPointMissException(scanResults.Count, adcValues.Count);
            }
        }

        #endregion  

        #region Memory Operation

        /// <summary>
        /// Get the length of the valid memory.
        /// </summary>
        /// <returns></returns>
        public uint GetMemoryLength()
        {
            RxPackage package;

            lock (_lockController)
            {
                Send(new CommandGetMemoryLength());
                Read(out package, CancellationToken.None);
            }

            using (var stream = new MemoryStream(package.Payload))
            {
                using (var reader = new BinaryReader(stream))
                {
                    return reader.ReadUInt32();
                }
            }
        }

        

        /// <summary>
        /// Read ADC values from the memory.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="lengthToRead"></param>
        /// <returns></returns>
        public IEnumerable<double> ReadMemory(uint offset, uint lengthToRead)
        {
           var raw = new List<double>();

           lock (_lockController)
           {
               Send(new CommandReadMemory(offset, lengthToRead));
               Read(out var package, CancellationToken.None);

               var block = new MemoryBlock(package.Payload);
               foreach (var val in block.Values)
               {
                   raw.Add(ConvertAdcRawTomV(val));
               }

               //// continue to read all blocks.
               //while (true)
               //{
               //    Read(out package, CancellationToken.None);

               //    MemoryBlock block = new MemoryBlock(package.Payload);

               //    if (block.Length == 0) // the last block has received.
               //        break;
               //    else
               //    {
               //        foreach(var val in block.Values)
               //        {
               //            raw.Add(ConvertADCRawTomV(val));
               //        }
               //    }

               //}
           }

           return raw;
        }

        /// <summary>
        /// Read all ADC values.
        /// </summary>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public IEnumerable<double> ReadMemoryAll(IProgress<MemoryReadProgressReport> progressReportHandle = null)
        {
            var len = GetMemoryLength();

            var buff = new List<double>((int)len);

            const int blockLen = 128; // read 128 points each time.
            var cycle0 = len / blockLen;
            var cycle1 = len % blockLen;
            var offset = 0;
            var retry = 0;

            for (var i = 0; i < cycle0; i++)
            {
                try
                {
                    var mem = ReadMemory((uint)offset, blockLen);
                    buff.AddRange(mem);
                    offset += blockLen;
                    retry = 0;

                    // report progress.
                    progressReportHandle?.Report(new MemoryReadProgressReport(offset, (int)len));
                }
                catch(Exception)
                {
                    retry++;
                    if (retry > 3)
                        throw;
                    else
                        Thread.Sleep(5);
                }
            }

            if(cycle1 > 0)
            {
                try
                {
                    var mem = ReadMemory((uint)offset, cycle1);
                    buff.AddRange(mem);

                    // report progress.
                    progressReportHandle?.Report(new MemoryReadProgressReport(offset, (int)len));
                }
                catch(Exception)
                {
                    retry++;
                    if (retry > 3)
                        throw;
                    else
                        Thread.Sleep(100);
                }
            }

            return buff;
        }

        /// <summary>
        /// Free the memory.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public void ClearMemory()
        {
            lock (_lockController)
            {
                Send(new CommandClearMemory());
            }
        }

        #endregion

        #endregion

        #region Private Methods

        public void Open()
        {
            if (_port.IsOpen == false)
                _port.Open();
        }

        public void Close()
        {
            if (this._port != null && this._port.IsOpen)
                _port.Close();
        }

        /// <summary>
        /// Send binary data to serial port.
        /// </summary>
        /// <param name="command"></param>
        void Send(ICommand command)
        {
            _port.DiscardInBuffer();
            _port.DiscardOutBuffer();

            var data = command.ToArray();

            _port.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Read package from UART port synchronously with specified timeout value.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <param name="timeout"></param>
        /// <param name="package"></param>
        /// <returns></returns>
        private void Read(out RxPackage package, CancellationToken cancellationToken, int timeout = DEFAULT_READ_TIMEOUT)
        {
            package = new RxPackage();

            var start = DateTime.Now;
            while (true)
            {
                var span = DateTime.Now - start;
                if (span.TotalMilliseconds > timeout)
                    throw new TimeoutException("timeout to read package from the serial port.");

                if (cancellationToken != CancellationToken.None && cancellationToken.IsCancellationRequested)
                    throw new OperationCanceledException("operation has cancelled.");

                // if one or more bytes are received, push to RxPackageParser.
                var len = _port.BytesToRead;
                if (len <= 0) continue;
                
                var data = new byte[len];
                _port.Read(data, 0, len);

                foreach (var t in data)
                {
                    package.AddData(t);

                    if (package.IsPackageFound)
                    {
                        break;
                    }
                }

                if (package.IsPackageFound)
                    break;
            }

            if (package.IsPassCRC == false)
            {
                throw new Exception("the CRC of the package is error.");
            }
        }

        /// <summary>
        /// Convert the raw-data of the ADC to mV.
        /// </summary>
        /// <param name="adcVal"></param>
        /// <returns></returns>
        private static double ConvertAdcRawTomV(short adcVal)
        {
            return adcVal / 32768.0 * GlobalDefinition.ADCVref;
        }

        /// <summary>
        /// Waiting for accomplishment of the time-consuming operations by the state of the specified Unit.
        /// </summary>
        /// <param name="unitId"></param>
        /// <param name="loopInterval"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public Errors WaitByUnitState(UnitID unitId, int loopInterval = 100, int timeout = DEFAULT_WAIT_BUSY_TIMEOUT)
        {
            var sw = new Stopwatch();
            UnitState state;

            var lastPosition = 0;
            
            Thread.Sleep(50);

            while (true)
            {
                sw.Restart();

                state = GetUnitState(unitId);

                // if the abs-position is changed, reset the timeout clock.
                if (state.AbsPosition != lastPosition)
                {
                    sw.Restart();
                    lastPosition = state.AbsPosition;
                }

                // raise event after the unit state updated.
                _unitStateChangedProgress.Report(state);
                
                if (state.IsBusy == false)
                    break;

                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds > timeout)
                    throw new TimeoutException("it's timeout to wait the long-duration-operation.");

                Thread.Sleep(loopInterval);
                
            }

            return state.Error;
        }

        /// <summary>
        /// Waiting for accomplishment of the time-consuming operations by the state of the entire system.
        /// </summary>
        /// <param name="loopInterval"></param>
        /// <param name="timeout"></param>
        /// <param name="positionRefreshingList">The list contains the axes whose position needs to be updated in real time.</param>
        /// <returns></returns>
        // ReSharper disable once MemberCanBePrivate.Global
        public SystemLastError WaitBySystemState(int loopInterval = 100, int timeout = DEFAULT_WAIT_BUSY_TIMEOUT,
            IEnumerable<UnitID> positionRefreshingList = null)
        {
            var sw = new Stopwatch();
            Thread.Sleep(50);

            while (true)
            {
                sw.Restart();

                var state = GetSystemState();
                if (state.IsSystemBusy == false)
                    break;

                sw.Stop();
                if (sw.Elapsed.TotalMilliseconds > timeout)
                    return new SystemLastError(UnitID.INVALID, Errors.ERR_TIMEOUT);

                //! adding delay here to ensure to get the latest state(position).
                Thread.Sleep(loopInterval);

                //! update the real-time abs-position if the list is not null.
                //! 2020/2//29 
                // An issue is found that the latest abs-position is not synced after 
                // the blind-search done since there are NO processes to read the position
                // back.
                if(positionRefreshingList == null)
                    throw new ArgumentNullException(nameof(positionRefreshingList));
                
                positionRefreshingList.ToList().ForEach(unitId =>
                {
                    try
                    {
                        var unitState = GetUnitState(unitId);
                        _unitStateChangedProgress.Report(unitState);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception($"unable to read the unit position.", ex);
                    }
                    finally
                    {
                        Thread.Sleep(10);
                    }

                });

            }

            var err = GetLastError();
            return err;
        }


        #region IDisposable Support
        private bool _disposedValue; // To detect redundant calls

        private void Dispose(bool disposing)
        {
            if (_disposedValue) return;
            
            if (disposing)
            {
                // TODO: dispose managed state (managed objects).
                Close();
            }

            // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
            // TODO: set large fields to null.

            _disposedValue = true;
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~Controller() {
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

        #region Events

        #endregion
    }
}
