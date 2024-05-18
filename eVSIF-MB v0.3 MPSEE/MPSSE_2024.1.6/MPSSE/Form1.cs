using FTD2XX_NET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MPSSE
{
    public partial class Form1 : Form
    {
        FTD2XX_NET.FTDI ftdi = new FTD2XX_NET.FTDI();

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ftdi.Close();
            base.OnClosing(e);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var stat = ftdi.OpenByIndex((uint)0);
            if (stat == FTDI.FT_STATUS.FT_OK)
            {
                uint writtenByte = 0;

                ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_RESET);
                ftdi.SetBitMode(0x00, FTDI.FT_BIT_MODES.FT_BIT_MODE_MPSSE);
                //ftdi.SetBaudRate(3000000);
                ftdi.SetBaudRate(1000 * 1000);
                ftdi.SetTimeouts(500, 500);
                ftdi.SetLatency(0);
                //https://www.ftdichip.com/Documents/AppNotes/AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf
                //3.6 Set / Read Data Bits High / Low Bytes
                ftdi.Write(new byte[] {
                    0x80,               //3.6 Set / Read Data Bits High / Low Bytes
                    0x40,                // Set D0-3=0, D4-7(GPIO)=0,0,1,0
                    0xFB},               // D0(TCK/SK), D1(TDI/D0), D3(TMS/CS) output, D2(TDO/DI) input, D4-7(GPIOL0-3) output
                    3,  // Write 3 bytes
                    ref writtenByte);

                //3.8.2 Set clk divisor (FT232H/FT2232H/FT4232H)
                //TCK period = 12MHz / ((1 +[(0xValueH * 256) OR 0xValueL] ) *2) 
                ftdi.Write(new byte[] {
                    0x86,               //3.8.2 Set clk divisor (FT232H/FT2232H/FT4232H)
                    0x05,                // ValueL  == 1MHz
                    0x00},               // ValueH  
                    3,  // Write 3 bytes
                    ref writtenByte);

                /*
                //3.7 Loopback Commands
                //Disconnect TDI to TDO for Loopback 
                ftdi.Write(new byte[] { 0x85 },
                    1,  // Write 1 bytes
                    ref writtenByte);
                */

                //3.4.4 Clock Data Bits Out on -ve clock edge LSB first(no read)
                ftdi.Write(new byte[] {
                0x1B,                //3.4.4 Clock Data Bits Out on -ve clock edge LSB first(no read)
                0x07,                // Send 8bits data length
                0x01},               // /CS0 = 1(Hi)
                    3,  // Write 3 bytes
                    ref writtenByte);

                //3.6.1 Set Data bits LowByte 
                /*
                //A0=0 Write register address
                ftdi.Write(new byte[] {
                    0x80,                //3.6.1 Set Data bits LowByte
                    0x40,                // Set D0-3=0 D4-7(GPIO)(A0=0, A1=0, /IC=1, GPIOL3=0)
                    0xFB},               // D0(TCK/SK), D1(TDI/D0), D3(TMS/CS) output, D2(TDO/DI) input, D4-7(GPIOL0-3) output
                        3,  // Write 3 bytes
                        ref writtenByte);
                
                ftdi.Write(new byte[] {
                    0x80,                //3.6.1 Set Data bits LowByte
                    0xC0,                // Set D0-3=0 D4-7(GPIO)(A0=0, A1=0, /IC=1, /CS=1)
                    0xFB},               // D0(TCK/SK), D1(TDI/D0), D3(TMS/CS) output, D2(TDO/DI) input, D4-7(GPIOL0-3) output
                        3,  // Write 3 bytes
                        ref writtenByte);
                */
                /*
                //3.6.2 Set Data bits High Byte
                ftdi.Write(new byte[] {
                    0x82,                //3.6.2 Set Data bits High Byte
                    0x00,                // Set AC0-7=0
                    0xFF},               // All pin is Output
                        3,  // Write 3 bytes
                        ref writtenByte);
                */

                setADPin(0, false, false, true, false);
                setACPin(0);

                MessageBox.Show("Initialize completed.");
            }
            else
            {
                MessageBox.Show("Failed to connect to FTDI");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            uint writtenByte = 0;

            //https://www.ftdichip.com/Documents/AppNotes/AN_108_Command_Processor_for_MPSSE_and_MCU_Host_Bus_Emulation_Modes.pdf

            //3.4.4 Clock Data Bits Out on -ve clock edge LSB first(no read)
            var stat = ftdi.Write(new byte[] {
                0x1B,                //3.4.4 Clock Data Bits Out on -ve clock edge LSB first(no read)
                0x07,                // Send 8bits data length
                0x00},               // /CS0 = 0(Lo)
                3,  // Write 3 bytes
                ref writtenByte);

            //https://www.smspower.org/maxim/Documents/YM2413ApplicationManual
            //3.6.1 Set Data bits LowByte
            {   // Select OPLL Address

                //A0=0 Write register address
                ftdi.Write(new byte[] {
                    0x80,                //3.6.1 Set Data bits LowByte
                    0x40,                // Set D0-3=0 D4-7(GPIO)(A0=0, A1=0, /IC=1, GPIOL3=0)
                    0xFB},               // D0(TCK/SK), D1(TDI/D0), D3(TMS/CS) output, D2(TDO/DI) input, D4-7(GPIOL0-3) output
                        3,  // Write 3 bytes
                        ref writtenByte);
                //3.6.2 Set Data bits High Byte
                ftdi.Write(new byte[] {
                    0x82,                //3.6.2 Set Data bits High Byte
                    30,                  // Set AC0-7(OPLL Register #30 (Inst,Vol))
                    0xFF},               // All pin is Output
                        3,  // Write 3 bytes
                        ref writtenByte);
            }
            {   // Write OPLL Data

                //A0=1 Write data value
                ftdi.Write(new byte[] {
                    0x80,                //3.6.1 Set Data bits LowByte
                    0x50,                // Set D0-3=0 D4-7(GPIO)(A0=1, A1=0, /IC=1, GPIOL3=0)
                    0xFB},               // D0(TCK/SK), D1(TDI/D0), D3(TMS/CS) output, D2(TDO/DI) input, D4-7(GPIOL0-3) output
                        3,  // Write 3 bytes
                        ref writtenByte);
                //3.6.2 Set Data bits High Byte
                ftdi.Write(new byte[] {
                    0x82,               //3.6.2 Set Data bits High Byte
                    0xFF,                // Set Value(Inst=Gt,Vol=15))
                    0xFF},               // All pin is Output
                        3,  // Write 3 bytes
                        ref writtenByte);
            }

            //3.4.4 Clock Data Bits Out on -ve clock edge LSB first(no read)
            stat = ftdi.Write(new byte[] {
                0x1B,                //3.4.4 Clock Data Bits Out on -ve clock edge LSB first(no read)
                0x07,                // Send 8bits data length
                0x01},               // /CS0 = 1(Hi)
                3,  // Write 3 bytes
                ref writtenByte);

            if (stat == FTDI.FT_STATUS.FT_OK)
            {
                MessageBox.Show("Sent data to FTDI");
            }
            else
            {
                MessageBox.Show("Failed to send data to FTDI");
            }
        }

        private uint setADPin(int slotId, bool A0, bool A1, bool cs, bool reset)
        {
            uint writtenByte = 0;

            byte HC138_A012 = (byte)(slotId & 0x7);
            byte HC138_CS = (byte)((slotId & 0x8) << 4);
            if (cs)
            {
                HC138_A012 = 0x07;
                HC138_CS = 0x80;
            }

            byte FM_A0 = (byte)(A0 ? 0x10 : 0x00);
            byte FM_A1 = (byte)(A1 ? 0x20 : 0x00);

            byte FM_RESET = (byte)(reset ? 0x40 : 0x00);

            byte AD = (byte)(HC138_CS | FM_RESET | FM_A1 | FM_A0 | HC138_A012);

            //A0=0 Write register address
            ftdi.Write(new byte[] {
                    0x80,                //3.6.1 Set Data bits LowByte
                    AD ,                 // Set AD
                    0xFB},               // AD0,AD1,AD3 output, AD2 input, AD4-7 output
                    3,  // Write 3 bytes
                    ref writtenByte);

            return writtenByte;
        }

        private uint setACPin(byte data)
        {
            uint writtenByte = 0;

            //3.6.2 Set Data bits High Byte
            ftdi.Write(new byte[] {
                    0x82,                //3.6.2 Set Data bits High Byte
                    data,                  // Set AC0-7(OPLL Register #30 (Inst,Vol))
                    0xFF},               // All pin is Output
                    3,  // Write 3 bytes
                    ref writtenByte);

            return writtenByte;
        }

        private uint writeData(int slot, byte address, byte data)
        {
            uint writtenByte = 0;

            //Set A0 and set /CS15=Lo(/CSn=Hi)
            setADPin(0, false, false, true, false);

            //Set FM Address
            setACPin(address);
            //Set /CSn=Lo after Tas, and the target card slot is selected.
            setADPin(slot, false, false, false, false);
            //Set A0 and set /CS15=Lo(/CSn=Hi)
            setADPin(0, false, false, true, false);     // Written address to FM

            //Set FM Data
            setACPin(data);
            //Set /CSn=Lo after Tas, and the target card slot is selected.
            setADPin(slot, true, false, false, false);
            //Set A0 and set /CS15=Lo(/CSn=Hi)
            setADPin(0, false, false, true, false);     // Written data to FM

            return writtenByte;
        }

        async private void button3_Click(object sender, EventArgs e)
        {
            writeData(0, 30, 0xFF); //Set inst and volume ch1
            writeData(0, 20, 0x16); //Key on ch1

            await Task.Delay(500);

            writeData(0, 20, 0x06); //Key on ch1
        }
    }
}
