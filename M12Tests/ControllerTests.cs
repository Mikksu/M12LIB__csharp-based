using M12.Base;
using M12.Commands.Alignment;
using M12.Definitions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static M12.Base.UnitSettings;

namespace M12.Tests
{
    [TestFixture()]
    public class ControllerTests
    {
        const string PORT_NAME = "COM16";
        const int BAUDRATE = 115200;
        const UnitID TestUnit1 = UnitID.U1;
        const UnitID TestUnit2 = UnitID.U3;

        const CSSCH TestCSS = CSSCH.CH1;

        [Test]
        public void GetSystemInfoTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                var info = controller.GetSystemInfo();
                controller.Close();

                // output results
                TestContext.WriteLine($"{info.MaxUnit} units supported");
                TestContext.WriteLine($"v{info.FirmwareVersion}");

                for (int i = 0; i < info.MaxUnit; i++)
                {
                    TestContext.WriteLine($"  Unit {i + 1}: v{info.UnitFwInfo[i].FirmwareVersion}");
                }
                Assert.AreEqual(info.MaxUnit, 12);
            }
        }

        [Test]
        public void GetUnitSettingsTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                var settings = controller.GetUnitSettings(TestUnit1);
                controller.Close();

                // output results
                TestContext.WriteLine($"Settings of Unit {TestUnit1}: {settings}");
            }
        }

        [Test]
        public void GetSystemStateTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                var state = controller.GetSystemState();
                controller.Close();

                // output results
                TestContext.WriteLine(state);
            }
        }

        [Test]
        public void GetUnitStateTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                var state = controller.GetUnitState(TestUnit1);
                controller.Close();

                // output results
                TestContext.WriteLine(state);
            }
        }

        [Test]
        public void  CapabilityOfCSSINTTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.Home(TestUnit1);
                controller.Move(TestUnit1, 50000, 100);

                var adc_val = controller.ReadAdc(ADCChannels.CH1);
                TestContext.WriteLine($"V_CSS1 before TOUCHED: {adc_val[0]}mV");

                controller.SetCssThreshold(TestCSS, 1000, 1800);

                for (int i = 0; i < 5; i++)
                {
                    TestContext.WriteLine($"Cycle {i + 1}"); ;
                    try
                    {
                        controller.SetCssEnable(TestCSS, true);
                        controller.Move(TestUnit1, 10000, 5);
                    }
                    catch (Exception ex)
                    {
                        TestContext.WriteLine(ex.Message);
                    }

                    //try
                    //{
                    //    controller.SetCSSEnable(TestCSS, true);
                    //    controller.Move(TestUnit1, -10000, 5);
                    //}
                    //catch (Exception ex)
                    //{
                    //    TestContext.WriteLine(ex.Message);
                    //}

                    adc_val = controller.ReadAdc(ADCChannels.CH1);
                    TestContext.WriteLine($"V_CSS1 after TOUCHED: {adc_val[0]}mV");
                }


                controller.Close();
            }
        }

        [Test()]
        public void SingleUnitSettingAndSavingToENV()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.SetAccelerationSteps(TestUnit1, 500);

                controller.ChangeUnitSettings(TestUnit1,
                    new UnitSettings(ModeEnum.TwoPulse, PulsePinEnum.CW, false, true, true, ActiveLevelEnum.Low, false));

                controller.SaveUnitEnv(TestUnit1);

                controller.Home(TestUnit1);

                controller.Close();

            }


        }

        [Test]
        public void SetAndSaveSettingsToUnit()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                
                for (int unit = 1; unit <= 12; unit++)
                {
                    controller.SetAccelerationSteps((UnitID)unit, 500);

                    controller.ChangeUnitSettings((UnitID)unit,
                        new UnitSettings(ModeEnum.TwoPulse, PulsePinEnum.CW, false, false, true, ActiveLevelEnum.High, false));

                    controller.SaveUnitEnv((UnitID)unit);
                }

                controller.Close();

            }
        }

        [Test]
        public void OutputIOTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                var sta1 = controller.ReadDout();
                TestContext.WriteLine(sta1);

                controller.SetDout(DigitalOutput.DOUT1, DigitalIOStatus.ON);

                var sta2 = controller.ReadDout();
                TestContext.WriteLine(sta2);

                controller.SetDout(DigitalOutput.DOUT1, DigitalIOStatus.OFF);

                var sta3 = controller.ReadDout(DigitalOutput.DOUT1);
                TestContext.WriteLine(sta3);



                controller.Close();
            }
        }

        [Test]
        public void InputIOTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                var ret = controller.ReadDin();

                TestContext.WriteLine(ret);

                controller.Close();
            }
        }

        [Test]
        public void Stop()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.Stop(TestUnit1);

                controller.Close();
            }
        }

        [Test]
        public void HomeSingleUnit()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.Home(TestUnit1, 5, 20);

                controller.Close();
            }
        }

        [Test]
        public void HomeMultiUnitsSimultaneously()
        {
            List<Task> homeTasks = new List<Task>();

            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                // Home all axes
                for(int i = 1; i <= 12; i++)
                {
                    var t = Task.Run(() =>
                    {
                        controller.Home((UnitID)i);
                    });

                    homeTasks.Add(t);

                    Thread.Sleep(100);
                }               

                Task.WaitAll(homeTasks.ToArray());

                controller.Close();

            }
        }

        [Test]
        public void MotionCapabilityTest()
        {
            UnitState stat;

            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                

                // Home
                controller.Home(TestUnit1);

                // emergency stop by the CSS
                controller.SetCssThreshold(TestCSS, 2000, 3000);
                controller.SetCssEnable(TestCSS, true);

                TestContext.WriteLine($"Move to +10000");
                // long-range move
                controller.Move(TestUnit1, 31051, 20);

                stat = controller.GetUnitState(TestUnit1);
                TestContext.WriteLine($"Position: {stat.AbsPosition}");

                // Frequently move
                for (int i = 0; i < 2; i++)
                {
                    TestContext.WriteLine($"Move to +100");
                    controller.Move(TestUnit1, 100, 15);
                    stat = controller.GetUnitState(TestUnit1);
                    TestContext.WriteLine($"Position: {stat.AbsPosition}");
                }

                TestContext.WriteLine($"Move to -200");
                controller.Move(TestUnit1, -200, 15);

                stat = controller.GetUnitState(TestUnit1);
                TestContext.WriteLine($"Position: {stat.AbsPosition}");

                TestContext.WriteLine($"Move to -500");
                controller.Move(TestUnit1, -500, 15);
                stat = controller.GetUnitState(TestUnit1);
                TestContext.WriteLine($"Position: {stat.AbsPosition}");

                controller.Move(TestUnit1, -30000, 30);

                controller.Close();
            }
        }

        [Test()]
        public void FastMoveTest()
        {
            UnitState stat;

            using (var controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();


                // Home
                controller.Home(TestUnit1, 5, 50);

                controller.Move(TestUnit1, 10000, 100);

                controller.FastMove(TestUnit1, 0, 100, 2);

                controller.Close();
            }
        }

        [Test()]
        public void ReadADCTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();
                var val = controller.ReadAdc(Definitions.ADCChannels.CH1);
                controller.Close();

                TestContext.WriteLine($"{val[0]}mV");
            }
        }


        [Test]
        public void ReadMemory()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                var len = controller.GetMemoryLength();
                TestContext.WriteLine($"{len} values available in memory.");

                var mem = controller.ReadMemory(0, len);
                TestContext.WriteLine($"{mem.Count()} values read from memory.");

                controller.Close();

                Assert.AreEqual(mem.Count(), len);
            }
        }

        [Test]
        public void FastAlign1DTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.Home(TestUnit1);

                controller.Move(TestUnit1, 100000, 100);

                controller.StartFast1D(TestUnit1, 10000, 20, 20, ADCChannels.CH3, out List<Point2D> values, ADCChannels.CH4, out List<Point2D> values2);

                TestContext.WriteLine($"{values.Count} values read.");

                controller.Close();
            }
        }

        [Test()]
        public void StartBlindSearchTest()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.Home(TestUnit1, 5, 50);
                controller.Move(TestUnit1, 10000, 100);

                controller.Home(TestUnit2, 5, 50);

                controller.Move(TestUnit2, 10000, 100);

                controller.SetOsr(ADC_OSR.AD7606_OSR_0);

                BlindSearchArgs horiArgs = new BlindSearchArgs(TestUnit1, 400, 100, 100, 20);
                BlindSearchArgs vertArgs = new BlindSearchArgs(TestUnit2, 400, 100, 100, 20);

                Exception exCaptured = null;
                List<Point3D> Results = null;

                for (int i = 0; i < 10; i++)
                {
                    exCaptured = null;

                    try
                    {
                        controller.StartBlindSearch(horiArgs, vertArgs, ADCChannels.CH1, out Results);
                    }
                    catch (Exception ex)
                    {
                        exCaptured = ex;
                    }
                    finally
                    {
                        TestContext.Write($"Cycle {i}, ");
                        if(exCaptured == null)
                            TestContext.WriteLine($"Points {Results.Count}, Succeed!");
                        else
                            TestContext.WriteLine($"Failed!");
                    }
                }

                

                controller.Close();


                //TestContext.WriteLine($"{Results.Count} Points scanned.");
            }
        }

        [Test()]
        public void SetOSR()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.SetOsr(ADC_OSR.AD7606_OSR_2);
                
                controller.Close();
            }
        }

        [Test()]
        public void FastMove()
        {
            using (Controller controller = new Controller(PORT_NAME, BAUDRATE))
            {
                controller.Open();

                controller.Home(TestUnit1);

                controller.Move(TestUnit1, 100001, 100);

                controller.FastMove(TestUnit1, -100001, 10, 20);

                controller.Close();
            }
        }
    }
}