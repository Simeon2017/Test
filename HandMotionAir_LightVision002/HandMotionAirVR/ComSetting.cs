using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace _HandMotionAirVR
{
	public partial class ComSetting : Form
	{
		public Form1 form1;
		string ComPort;

		public ComSetting()
		{
			InitializeComponent();
		}
		private void ComSetting_Load(object sender, EventArgs e)
		{
			this.Text = "TOCOS 初期化";
			label1.Text = "TOCOSが接続されているCOMを選択して\n「初期化」を押してください\n\nセンサ1,2のMACアドレスを選択してください";

			comboBox1.SelectedIndex = form1.COM_PORT - 1;
			ComPort = comboBox1.Text;

			comboBox2.SelectedIndex = 0;
			comboBox3.SelectedIndex = 1;

			form1.SensorMacAddress[0] = (string)comboBox2.SelectedItem;
			form1.SensorMacAddress[1] = (string)comboBox3.SelectedItem;

			this.ActiveControl = this.button2;
			this.MaximizeBox = false;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			form1.COM_PORT = comboBox1.SelectedIndex + 1;
			ComPort = comboBox1.Text;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string ComPortTmp = comboBox1.Text;
			serialPort1.NewLine = "\r\n";
			serialPort1.BaudRate = (int)Form1.BAUD_RATE;
			serialPort1.PortName = ComPortTmp;
			serialPort1.Open();

			for (int i = 0; i < 2; i++)
			{
				string line = serialPort1.ReadLine();
			}
			serialPort1.Close();
			serialPort1.Dispose(); 

			this.Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			form1.SensorMacAddress[0] = (string)comboBox2.SelectedItem;
		}

		private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
		{
			form1.SensorMacAddress[1] = (string)comboBox3.SelectedItem;
		}
	}
}
