//#define _DEBUG_
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Timers;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using MLApp;

// 識別対応
// 1      2      3      4       5       6       7       8       9       10
// 0      1      2      3       4       5       6       7       8        9
//"静止", "円", "四角", "三角", "振り", "殴り", "ぐる", "静止2, "静止3, 静止4
// 0      1      2      3       4
//"静止", "円", "振り", "殴り", "ぐる",

namespace _HandMotionAirVR
{
	public partial class Form1 : Form
	{
		string DEBUG_STR = null;

		public int COM_PORT = 6; // デフォルトのCOMポート番号
		const int SENSOR_NUM = 2; // センサの個数
		const int RESULT_MOTION_TYPE = 10;
		const int DATA_SIZE_3SEC = 111;
		const int DATA_SIZE_1SEC = 37;
		const int DATA_SIZE_2SEC = 74;
		const int DATA_SIZE_15SEC = 55;
		const int DATA_SIZE_25SEC = 92;
		public const uint BAUD_RATE = 115200;
		public string[] SensorMacAddress = { "810208A1", "810209F8" };
		static public string UserProfile = Environment.GetEnvironmentVariable("USERPROFILE");
		static public string HandMotionDir = UserProfile + @"\Documents\HandMotionAir";
		static public string MfileDir = HandMotionDir + @"\Mfiles";
		static public string NeuralNetDir3sec = @"nn_machine";
		static public string NeuralNetDir1sec = @"nn_machine_1sec_10out";
		static public string NeuralNetDir15sec = @"nn_machine_15sec";
		static public string NeuralNetDir2sec = @"nn_machine_2sec";
		static public string NeuralNetDir25sec = @"nn_machine_25";
		string NeuralNetDataDir = NeuralNetDir3sec;
		int ClientCommandNo = 0;
		int PreviousClientCommandNo = -1;

		string ipString = "127.0.0.1";
		int port = 50377;
		string stCurrentDir = null;
		public const string UnityExecName = "Robotis_vsido_connect";
//		public const string UnityExecName = "HandAction001";

		string UnityExecNameFullPath = null;
		Process UnityProcess = null;

		System.IO.MemoryStream ms = null;
		System.Net.Sockets.NetworkStream ns = null;
		System.Net.Sockets.TcpListener listener = null;
		System.Net.Sockets.TcpClient client = null;
		System.Text.Encoding enc = null;

		Thread ServerThread = null;
		Thread ServerWaitingThread = null;
		string resMsg = "ready";
		string HostIP_Port = null;
		string ClientIP_Port = null;
		string ClientSendMsg = null;
		string[] FromClientMessage = null;


		bool cancel_flag = false;
		double STABLE_RECOGNIZE_THRESHOLD = 0.8;
		double CIRCLE_RECOGNIZE_THRESHOLD = 0.99999;
		double WAVE_RECOGNIZE_THRESHOLD = 0.9;
		double PUNCH_RECOGNIZE_THRESHOLD = 0.99;
		double ROTATE_RECOGNIZE_THRESHOLD = 0.95;
		double[] THRESHOLDS = new double[5];

		int SIZE;
		int MATLAB_EXECUTE_INTERVAL = 100;
		int SEND_CLIENT_INTERVAL = 500;
		int Counter = 0;
		int[] SensorCounter = new int[SENSOR_NUM] { 0, 0 };
		int[] CurrentSensorCount = new int [SENSOR_NUM] { 0, 0 };
		int[,] x_array;
		int[,] y_array;
		int[,] z_array;
		int[,] formatted_x_array;
		int[,] formatted_y_array;
		int[,] formatted_z_array;
		double[] double_x1_array;
		double[] double_y1_array;
		double[] double_z1_array;
		double[] double_x2_array;
		double[] double_y2_array;
		double[] double_z2_array;

		Rs232 SerialObj = new Rs232();
		byte[] bufbuf = new byte[512];
		private bool SENSOR_ACQUISITION_LOOP = true;

		Thread inputThread = null;
		Thread MatlabExecThread = null;
		System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

		string ComPort = null;
		string[] MotionPatternString = { "静止", "四角", "振り", "殴り", "ぐる", "---" };

		const int STABLE = 0;
		const int CIRCLE = 1;
		const int SQUARE = 2;
		const int TRIANGLE = 3;
		const int WAVE = 4;
		const int PUNCH = 5;
		const int ROTATE = 6;

		double[] ResultMotion = new double[5];
		double[] ResultMotionTMP = new double[RESULT_MOTION_TYPE];
		int ResultIDX = 1;
		System.Array prresult = new double[1, RESULT_MOTION_TYPE + 1];
		System.Array piresult = new double[1, RESULT_MOTION_TYPE + 1];
		const int RESULT_IDX_NO = 10;
		public MLApp.MLApp matlab = new MLApp.MLApp();
		Label[] labels = new Label[10];
		bool isSensorDataDisplay = false;

		private System.Timers.Timer AccurateTimer;

		public Form1()
		{
			InitializeComponent();

			AccurateTimer = new System.Timers.Timer();
			AccurateTimer.AutoReset = true;
			AccurateTimer.Elapsed += new ElapsedEventHandler(OnTimerEvent);

			if (radioButton1.Checked)
			{
				SIZE = DATA_SIZE_1SEC;
				NeuralNetDataDir = NeuralNetDir1sec;
			}
			else if (radioButton2.Checked)
			{
				SIZE = DATA_SIZE_15SEC;
				NeuralNetDataDir = NeuralNetDir15sec;
			}
			else if (radioButton3.Checked)
			{
				SIZE = DATA_SIZE_2SEC;
				NeuralNetDataDir = NeuralNetDir2sec;
			}
			else if (radioButton4.Checked)
			{
				SIZE = DATA_SIZE_25SEC;
				NeuralNetDataDir = NeuralNetDir25sec;
			}
			else if (radioButton5.Checked)
			{
				SIZE = DATA_SIZE_3SEC;
				NeuralNetDataDir = NeuralNetDir3sec;
			}
#if _DEBUG_
			stCurrentDir = @"C:\Users\Simeon\ゆうきたんHandActionVR";
#else
			stCurrentDir = System.Environment.CurrentDirectory;
#endif
			UnityExecNameFullPath = stCurrentDir + @"\Unity\" + UnityExecName;
//			UnityExecNameFullPath = @"C:\Users\Simeon\Documents\UnityProjects\HandAction_HakoVision_001\bin\" + UnityExecName;


			textBox9.Text = MATLAB_EXECUTE_INTERVAL.ToString();
			textBox10.Text = STABLE_RECOGNIZE_THRESHOLD.ToString();
			textBox11.Text = CIRCLE_RECOGNIZE_THRESHOLD.ToString();
			textBox12.Text = WAVE_RECOGNIZE_THRESHOLD.ToString();
			textBox13.Text = PUNCH_RECOGNIZE_THRESHOLD.ToString();
			textBox14.Text = ROTATE_RECOGNIZE_THRESHOLD.ToString();

			textBox15.Text = SEND_CLIENT_INTERVAL.ToString();
			THRESHOLDS[0] = STABLE_RECOGNIZE_THRESHOLD;
			THRESHOLDS[1] = CIRCLE_RECOGNIZE_THRESHOLD;
			THRESHOLDS[2] = WAVE_RECOGNIZE_THRESHOLD;
			THRESHOLDS[3] = PUNCH_RECOGNIZE_THRESHOLD;
			THRESHOLDS[4] = ROTATE_RECOGNIZE_THRESHOLD;

			label23.Text = null;
			label32.Text = "Client:";

			#region Label Array
			labels[0] = label14;
			labels[1] = label15;
			labels[2] = label18;
			labels[3] = label19;
			labels[4] = label20;
			labels[5] = label12;
			labels[6] = label21;
			labels[7] = label24;
			labels[8] = label25;
			labels[9] = label26;
			#endregion
			matlab.MinimizeCommandWindow();
			matlab.Execute(@"cd " + MfileDir);
			matlab.Execute(@"cd " + NeuralNetDataDir);
			matlab.Execute("load nn_system");
			matlab.Execute(@"cd " + MfileDir);
		}

		void ReadInput()
		#region Serial Read Thread
		{
			int x, y, z;
			while (inputThread.IsAlive)
			{
				if(SENSOR_ACQUISITION_LOOP)
				{
					int buf_count = 0;
					bool LoopFlag = true;
					while (LoopFlag)
					{
						byte[] bufbuf = new byte[1];
						buf_count = SerialObj.Read(bufbuf, 0, 1);
						if (bufbuf[0] == ':')
						{
							buf_count = SerialObj.Read(bufbuf, 0, 1);
							if (bufbuf[0] == ':')
							{
								LoopFlag = false;
							}
						}
					}

					byte[] sbuf = new byte[1];
					char[] str_buf = new char[128];

					int cnt = 0;
					LoopFlag = true;
					while (LoopFlag)
					{
						buf_count = SerialObj.Read(sbuf, 0, 1);
						if (sbuf[0] == (byte)0x0d)
						{
							buf_count = SerialObj.Read(sbuf, 0, 1);
							if (sbuf[0] == (byte)0x0a)
							{
								LoopFlag = false;
							}
						}
						str_buf[cnt] = (char)sbuf[0];
						cnt++;
					}

					string line = new string(str_buf);
					int line_length = line.Length;

					char[] x_chara = new char[5];
					char[] y_chara = new char[5];
					char[] z_chara = new char[5];

					int SensorNo = -1;
					char[] SensMacAddress = new char[8];

					for (int No = 0; No < SENSOR_NUM; No++)
					{

						for (int i = 0; i < line_length; i++)
						{
							if (line[i] == 'e' && line[i + 1] == 'd')
							{
								for (int n = 0; n < 8; n++)
								{
									SensMacAddress[n] = line[n + i + 3];
								}
							}
						}
						string SensorMacAddressTMP = new string(SensMacAddress);
						for(int n = 0; n < SENSOR_NUM; n++)
						{
							if (SensorMacAddressTMP == SensorMacAddress[n])
							{
								SensorNo = n;
							}
						}

						for (int i = 0; i < line_length; i++)
						{
							if (line[i] == 'x')
							{
								LoopFlag = true;
								int n = 0;
								while (LoopFlag)
								{
									char tst = line[i + n + 2];
									if (tst == ':')
									{
										LoopFlag = false;
									}
									else
									{
										x_chara[n] = tst;
									}
									n++;
								}
							}
							if (line[i] == 'y')
							{
								LoopFlag = true;
								int n = 0;
								while (LoopFlag)
								{
									char tst = line[i + n + 2];
									if (tst == ':')
									{
										LoopFlag = false;
									}
									else
									{
										y_chara[n] = tst;
									}
									n++;
								}
							}
							if (line[i] == 'z')
							{
								LoopFlag = true;
								int n = 0;
								while (LoopFlag)
								{
									char tst = line[i + n + 2];
									if (tst == '\n')
									{
										LoopFlag = false;
									}
									else
									{
										z_chara[n] = tst;
									}
									n++;
								}
							}
						}
					}

					x = int.Parse(new string(x_chara));
					y = int.Parse(new string(y_chara));
					z = int.Parse(new string(z_chara));
					if (isSensorDataDisplay)
					{
						setText(SensorNo, x, y, z, Counter);
					}
					if (SensorCounter[SensorNo] < SIZE)
					{
						x_array[SensorNo, SensorCounter[SensorNo]] = x;
						y_array[SensorNo, SensorCounter[SensorNo]] = y;
						z_array[SensorNo, SensorCounter[SensorNo]] = z;
					}
					SensorCounter[SensorNo]++;
					CurrentSensorCount[SensorNo] = SensorCounter[SensorNo];

					if(SensorCounter[SensorNo] >= SIZE)
					{
//						SENSOR_ACQUISITION_LOOP = false;
						SensorCounter[SensorNo] = 0;
					}
				}
				else
				{
					sw.Stop();
					SerialObj.Close();
					if (!cancel_flag)
					{
						cancelText(0, 0, 0, 0, 0);
					}
					endText(0, 0, 0, 0, 0);
					Counter = 0;
					inputThread.Abort();
				}
				Counter++;
			}
		}

		delegate void myText(int SensoNo, int x, int y, int z, int Counter);

		private void setText(int SensorNo, int x, int y, int z, int Counter)
		{
			if (this.textBox1.InvokeRequired)
			{
				myText d = new myText(setText);
				this.Invoke(d, new object[] { SensorNo, x, y, z, Counter });
			}
			else
			{
				if (SensorNo == 0)
				{
					this.textBox1.Text = x.ToString();
					this.textBox2.Text = y.ToString();
					this.textBox3.Text = z.ToString();
				}
				else if (SensorNo == 1)
				{
					this.textBox6.Text = x.ToString();
					this.textBox7.Text = y.ToString();
					this.textBox8.Text = z.ToString();
				}
					this.textBox4.Text = Counter.ToString();
			}
		}

		private void cancelText(int SensorNo, int x, int y, int z, int Counter)
		{
			if (this.textBox1.InvokeRequired)
			{
				myText d = new myText(cancelText);
				this.Invoke(d, new object[] { SensorNo, x, y, z, Counter });
			}
			else
			{
				this.button1.Enabled = true;
				this.button3.Enabled = true;
				this.textBox1.Text = "Push Save";
				this.textBox2.Text = "Push Save";
				this.textBox3.Text = "Push Save";
			}
		}

		private void endText(int SensorNo, int x, int y, int z, int Counter)
		{
			if (this.textBox1.InvokeRequired)
			{
				myText d = new myText(endText);
				this.Invoke(d, new object[] {SensorNo, x, y, z, Counter});
			}
			else
			{
				this.button2.Enabled = false;
				this.textBox5.Text = sw.ElapsedMilliseconds.ToString();
			}
		}
		#endregion

		private void button1_Click(object sender, EventArgs e)
		{
			button1.Enabled = false;
			button2.Enabled = true;
			button3.Enabled = false;
			radioButton1.Enabled = false;
			radioButton2.Enabled = false;
			radioButton3.Enabled = false;
			radioButton4.Enabled = false;
			radioButton5.Enabled = false;
			textBox9.Enabled = false;
			button6.Enabled = false;
			textBox10.Enabled = false;
			textBox11.Enabled = false;
			textBox12.Enabled = false;
			textBox13.Enabled = false;
			textBox14.Enabled = false;
			textBox15.Enabled = false;
			button7.Enabled = false;
			button9.Enabled = false;
			Microsoft.VisualBasic.Interaction.AppActivate(UnityProcess.Id);

			if(SerialObj.Open(ComPort))
			{
				textBox1.Text = "Open";
			}
			else
			{
				textBox1.Text = "Error";
				MessageBox.Show("プログラムを再起動してください");
				Environment.Exit(0);
			}

			bool res = SerialObj.Init(BAUD_RATE, 8, 0, 1);

			textBox1.Text = "判定中";
			textBox2.Text = "判定中";
			textBox3.Text = "判定中";

			x_array = new int[SENSOR_NUM, SIZE];
			y_array = new int[SENSOR_NUM, SIZE];
			z_array = new int[SENSOR_NUM, SIZE];
			formatted_x_array = new int[SENSOR_NUM, SIZE];
			formatted_y_array = new int[SENSOR_NUM, SIZE];
			formatted_z_array = new int[SENSOR_NUM, SIZE];
			double_x1_array = new double[SIZE];
			double_y1_array = new double[SIZE];
			double_z1_array = new double[SIZE];
			double_x2_array = new double[SIZE];
			double_y2_array = new double[SIZE];
			double_z2_array = new double[SIZE];


			inputThread = new Thread(ReadInput);
			inputThread.Priority = ThreadPriority.AboveNormal;
			inputThread.Start();

			cancel_flag = false;
			Counter = 0;
			for (int n = 0; n < SENSOR_NUM; n++)
			{
				SensorCounter[n] = 0;
			}
			SENSOR_ACQUISITION_LOOP = true;
			sw.Reset();
			sw.Start();
			AccurateTimer.Enabled = true;
			AccurateTimer.Interval = MATLAB_EXECUTE_INTERVAL;
			AccurateTimer.Start();

			timer1.Interval = SEND_CLIENT_INTERVAL;
			timer1.Start();

		}

		private void button2_Click(object sender, EventArgs e)
		{
			cancel_flag = true;
			this.button1.Enabled = true;
			this.button3.Enabled = true;
			textBox1.Text = null;
			textBox2.Text = null;
			textBox3.Text = null;
			radioButton1.Enabled = true;
			radioButton2.Enabled = true;
			radioButton3.Enabled = true;
			radioButton4.Enabled = true;
			radioButton5.Enabled = true;
			textBox9.Enabled = true;
			button6.Enabled = true;
			textBox10.Enabled = true;
			textBox11.Enabled = true;
			textBox12.Enabled = true;
			textBox13.Enabled = true;
			textBox14.Enabled = true;
			textBox15.Enabled = true;
			button7.Enabled = true;
			button9.Enabled = true;

			timer1.Stop();
			Counter = 0;
			SensorCounter[0] = 0;
			SensorCounter[1] = 0;
			inputThread.Abort();
			SerialObj.Close();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			string file_name = null;
			FormatDataOrder();
			DialogResult dr = saveFileDialog1.ShowDialog();
			if (dr == System.Windows.Forms.DialogResult. OK)
			{
				file_name = saveFileDialog1.FileName;
				StreamWriter writer = new StreamWriter(file_name, false, Encoding.GetEncoding("Shift_JIS"));
				for (int cnt = 0; cnt < SIZE; cnt++)
				{
					for (int Sns = 0; Sns < SENSOR_NUM; Sns++)
					{
						writer.Write(x_array[Sns, cnt].ToString());
						writer.Write(", ");
						writer.Write(y_array[Sns, cnt].ToString());
						writer.Write(", ");
						writer.Write(z_array[Sns, cnt].ToString());
						writer.Write(", ");
						writer.Write(formatted_x_array[Sns, cnt].ToString());
						writer.Write(", ");
						writer.Write(formatted_y_array[Sns, cnt].ToString());
						writer.Write(", ");
						writer.Write(formatted_z_array[Sns, cnt].ToString());
						
						if (Sns != SENSOR_NUM - 1)
						{
							writer.Write(", ");
						}
					}
					writer.WriteLine();
				}
				writer.Close();
			}
			else if (dr == System.Windows.Forms.DialogResult.Cancel)
			{
				MessageBox.Show("キャンセルされました");
			}
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			AccurateTimer.Stop();
			if (inputThread != null && inputThread.IsAlive)
			{
				inputThread.Abort();
				SerialObj.Close();
			}

			if (ServerThread != null && ServerThread.IsAlive)
			{
				ns.Close();
				client.Close();
				listener.Stop();
				ServerThread.Abort();
				UnityProcess.Kill();
				UnityProcess.Close();
				UnityProcess.Dispose();
			}
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			SerialPortInitForTOCOS();
			comboBox1.SelectedIndex = COM_PORT - 1;
			ComPort = @"\\.\" + comboBox1.Text;
			for (int cnt = 0; cnt < 5; cnt++)
			{
				labels[cnt].Text = null;
			}
			label29.Text = null;
			label29.ForeColor = Color.Blue;
			button3.Enabled = false;
			this.ActiveControl = this.button8;
			this.MaximizeBox = false;
			button1.Enabled = false;
			button2.Enabled = false;

			enc = System.Text.Encoding.UTF8;
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComPort = @"\\.\" + comboBox1.Text;
			COM_PORT = comboBox1.SelectedIndex + 1;
		}

		private void button4_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		void SerialPortInitForTOCOS()
		{
			ComSetting comSetting = new ComSetting();
			comSetting.form1 = this;
			comSetting.ShowDialog(); // ComSettingのモーダル表示
		}

		private void button5_Click(object sender, EventArgs e)
		{
			SerialPortInitForTOCOS();
		}

		private void FormatDataOrder()
		{
			int[] fcnt = new int[SENSOR_NUM] { 0, 0 };
			fcnt[0] = CurrentSensorCount[0];
			fcnt[1] = CurrentSensorCount[1];

			for (int Sns = 0; Sns < SENSOR_NUM; Sns++)
			{
				for (int cnt = 0; cnt < SIZE; cnt++)
				{
					try
					{
						formatted_x_array[Sns, cnt] = x_array[Sns, fcnt[Sns]];
						formatted_y_array[Sns, cnt] = y_array[Sns, fcnt[Sns]];
						formatted_z_array[Sns, cnt] = z_array[Sns, fcnt[Sns]];
						if (fcnt[Sns] >= SIZE - 1)
						{
							fcnt[Sns] = 0;
						}
						else
						{
							fcnt[Sns]++;
						}
					}
					catch
					{
						;
					}
				}
			}
		}

		private void OnTimerEvent(object source, ElapsedEventArgs e)
		{
			CurrentSensorCount[0] = SensorCounter[0];
			CurrentSensorCount[1] = SensorCounter[1];
			FormatDataOrder();
			if (MatlabExecThread == null || !MatlabExecThread.IsAlive)
			{
				MatlabExecThread = new Thread(MatlabExec);
				MatlabExecThread.Priority = ThreadPriority.Highest;
				MatlabExecThread.IsBackground = true;
				MatlabExecThread.Start();
			}
			ResultDisplay();
		}

		delegate void MyDisplay();

		private void ResultDisplay()
		{
			if (this.label29.InvokeRequired)
			{
				MyDisplay d = new MyDisplay(ResultDisplay);
				try
				{
					this.Invoke(d);
				}
				catch
				{
					;
				}
			}
			else
			{
				SetLabelColor();
				for (int cnt = 0; cnt < 5; cnt++)
				{
					labels[cnt].Text = ResultMotion[cnt].ToString("F3");
				}
				if (ResultMotion[ResultIDX - 1] >= THRESHOLDS[ResultIDX - 1])
				{
					label29.Text = MotionPatternString[ResultIDX - 1];
					ClientCommandNo = ResultIDX - 1;
				}
				else
				{
//					label29.Text = MotionPatternString[5];
					label29.Text = MotionPatternString[0];
					ClientCommandNo = 0;
				}
			}
		}

		void SetLabelColor()
		#region Color IDX
		{
			for(int cnt = 0; cnt < 5; cnt++)
			{
				if(cnt == (ResultIDX -1))
				{
					labels[cnt].ForeColor = Color.Red;
					labels[cnt + 5].ForeColor = Color.Red;
				}
				else
				{
					labels[cnt].ForeColor = Color.Black;
					labels[cnt + 5].ForeColor = Color.Black;
				}
			}
		}
		#endregion

		void MatlabExec()
		{
			for (int cnt = 0; cnt < SIZE; cnt++)
			{
				double_x1_array[cnt] = (double)formatted_x_array[0, cnt];
				double_y1_array[cnt] = (double)formatted_y_array[0, cnt];
				double_z1_array[cnt] = (double)formatted_z_array[0, cnt];
				double_x2_array[cnt] = (double)formatted_x_array[1, cnt];
				double_y2_array[cnt] = (double)formatted_y_array[1, cnt];
				double_z2_array[cnt] = (double)formatted_z_array[1, cnt];
			}

			matlab.PutWorkspaceData("x1_data", "base", double_x1_array);
			matlab.PutWorkspaceData("y1_data", "base", double_y1_array);
			matlab.PutWorkspaceData("z1_data", "base", double_z1_array);
			matlab.PutWorkspaceData("x2_data", "base", double_x2_array);
			matlab.PutWorkspaceData("y2_data", "base", double_y2_array);
			matlab.PutWorkspaceData("z2_data", "base", double_z2_array);

			matlab.Execute("hand_motion_classify");
			matlab.GetFullMatrix("recognition_result", "base", ref prresult, ref piresult);
			for (int cnt = 0; cnt < RESULT_MOTION_TYPE; cnt++)
			{
				ResultMotionTMP[cnt] = (double)prresult.GetValue(0, cnt);
				if (cnt == 0)
				{
					ResultMotion[0] = ResultMotionTMP[cnt];
				}
				else if (cnt == 1)
				{
					ResultMotion[0] = ResultMotionTMP[cnt];
				}
				else if (cnt == 2)
				{
					ResultMotion[1] = ResultMotionTMP[cnt];
				}
				else if (cnt == 3)
				{
					ResultMotion[0] = ResultMotionTMP[cnt];
				}
				else if (cnt == 4)
				{
					ResultMotion[2] = ResultMotionTMP[cnt];
				}
				else if (cnt == 5)
				{
					ResultMotion[3] = ResultMotionTMP[cnt];
				}
				else if (cnt == 6)
				{
					ResultMotion[4] = ResultMotionTMP[cnt];
				}
				else if (cnt == 7 || cnt == 8 || cnt == 9)
				{
					ResultMotion[0] = ResultMotionTMP[cnt];
				}
				else
				{
					ResultMotion[cnt] = ResultMotionTMP[cnt];
				}
			}
			ResultIDX = (int)((double)prresult.GetValue(0, RESULT_IDX_NO));
			if (ResultIDX == 1)
			{
				ResultIDX = 1;
			}
			else if (ResultIDX == 2)
			{
				ResultIDX = 1;
			}
			else if (ResultIDX == 3)
			{
				ResultIDX = 2;
			}
			else if (ResultIDX == 4)
			{
				ResultIDX = 1;
			}
			else if (ResultIDX == 5)
			{
				ResultIDX = 3;
			}
			else if (ResultIDX == 6)
			{
				ResultIDX = 4;
			}
			else if (ResultIDX == 7)
			{
				ResultIDX = 5;
			}
			else if (ResultIDX >= 8)
			{
				ResultIDX = 1;
			}
		}

		private void button6_Click(object sender, EventArgs e)
		{
			MATLAB_EXECUTE_INTERVAL = int.Parse(textBox9.Text);
		}

		private void button7_Click(object sender, EventArgs e)
		{
			STABLE_RECOGNIZE_THRESHOLD = double.Parse(textBox10.Text);
			CIRCLE_RECOGNIZE_THRESHOLD = double.Parse(textBox11.Text);
			WAVE_RECOGNIZE_THRESHOLD = double.Parse(textBox12.Text);
			PUNCH_RECOGNIZE_THRESHOLD = double.Parse(textBox13.Text);
			ROTATE_RECOGNIZE_THRESHOLD = double.Parse(textBox14.Text);
			THRESHOLDS[0] = STABLE_RECOGNIZE_THRESHOLD;
			THRESHOLDS[1] = CIRCLE_RECOGNIZE_THRESHOLD;
			THRESHOLDS[2] = WAVE_RECOGNIZE_THRESHOLD;
			THRESHOLDS[3] = PUNCH_RECOGNIZE_THRESHOLD;
			THRESHOLDS[4] = ROTATE_RECOGNIZE_THRESHOLD;
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if (checkBox1.Checked)
			{
				isSensorDataDisplay = true;
			}
			else
			{
				isSensorDataDisplay = false;
				textBox1.Text = "判定中";
				textBox2.Text = "判定中";
				textBox3.Text = "判定中";
				textBox4.Text = null;
				textBox6.Text = null;
				textBox7.Text = null;
				textBox8.Text = null;
			}
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton1.Checked)
			{
				SIZE = DATA_SIZE_1SEC;
				matlab.Execute(@"cd " + MfileDir);
				matlab.Execute(@"cd " + NeuralNetDir1sec);
				matlab.Execute("load nn_system");
				matlab.Execute(@"cd " + MfileDir);
			}
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton2.Checked)
			{
				SIZE = DATA_SIZE_15SEC;
				matlab.Execute(@"cd " + MfileDir);
				matlab.Execute(@"cd " + NeuralNetDir15sec);
				matlab.Execute("load nn_system");
				matlab.Execute(@"cd " + MfileDir);
			}
		}

		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton3.Checked)
			{
				SIZE = DATA_SIZE_2SEC;
				matlab.Execute(@"cd " + MfileDir);
				matlab.Execute(@"cd " + NeuralNetDir2sec);
				matlab.Execute("load nn_system");
				matlab.Execute(@"cd " + MfileDir);
			}
		}

		private void radioButton4_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton4.Checked)
			{
				SIZE = DATA_SIZE_25SEC;
				matlab.Execute(@"cd " + MfileDir);
				matlab.Execute(@"cd " + NeuralNetDir25sec);
				matlab.Execute("load nn_system");
				matlab.Execute(@"cd " + MfileDir);
			}
		}

		private void radioButton5_CheckedChanged(object sender, EventArgs e)
		{
			if (radioButton5.Checked)
			{
				SIZE = DATA_SIZE_3SEC;
				matlab.Execute(@"cd " + MfileDir);
				matlab.Execute(@"cd " + NeuralNetDir3sec);
				matlab.Execute("load nn_system");
				matlab.Execute(@"cd " + MfileDir);
			}
		}

		private void button8_Click(object sender, EventArgs e)
		{
			ServerWaitingThread = new Thread(ServerWaiting);
			ServerWaitingThread.Priority = ThreadPriority.Lowest;
			ServerWaitingThread.Start();
			button8.Enabled = false;
//			UnityProcess = Process.Start(UnityExecNameFullPath);
			UnityProcess = Process.Start(UnityExecNameFullPath, "-popupwindow");

		}

		void ServerWaiting()
		{
			System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);
			listener = new System.Net.Sockets.TcpListener(ipAdd, port);
			listener.Start();
			HostIP_Port = ((IPEndPoint)listener.LocalEndpoint).Address.ToString() + " : " + ((IPEndPoint)listener.LocalEndpoint).Port.ToString();
			client = listener.AcceptTcpClient();
			ClientIP_Port = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString() + " : " + ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port.ToString();
			DispIPInfo();

			DEBUG_STR = "Client: Connect\n" + ClientIP_Port;
			DispIPInfo();

			ServerThread = new Thread(ServerReadWrite);
			ServerThread.Priority = ThreadPriority.Lowest;
			ServerThread.Start();
		}

		delegate void MyText();
		private void DispIPInfo()
		{
			if (this.label32.InvokeRequired)
			{
				MyText d = new MyText(DispIPInfo);
				this.Invoke(d);
			}
			else
			{
//				label32.Text = "Client: Connected" + ClientIP_Port;
				label32.Text = DEBUG_STR;
				button1.Enabled = true;
				button2.Enabled = true;
			}
		}

		void ServerReadWrite()
		{
			while (true)
			{
//				resMsg = null;
				ns = client.GetStream();
				ns.ReadTimeout = Timeout.Infinite;
				ns.WriteTimeout = Timeout.Infinite;

				bool disconnected = false;
				ms = new System.IO.MemoryStream();
				byte[] resBytes = new byte[256];
				int resSize = 0;
				do
				{
					try
					{
						resSize = ns.Read(resBytes, 0, resBytes.Length);
						if (resSize == 0)
						{
							disconnected = true;
							Console.WriteLine("クライアントが切断しました。");
							ServerThread.Abort();
							break;
						}
						ms.Write(resBytes, 0, resSize);
					}
					catch
					{
						;
					}
				} while (ns.DataAvailable || resBytes[resSize - 1] != '\n');

				resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
				ms.Close();
				resMsg = resMsg.TrimEnd('\n');

				FromClientMessage = resMsg.Split(':');
//				string rcvMsgNo = FromClientMessage[0] + ";" + FromClientMessage[1];
//				string rcvMsg = FromClientMessage[2];
//				PutServerText();
				if (!disconnected)
				{
					string sendMsg = resMsg.Length.ToString();
					byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
					Console.WriteLine(sendMsg);
				}
			}
		}

		private void PutServerText()
		{
			if (this.label1.InvokeRequired)
			{
				MyText d = new MyText(PutServerText);
				this.Invoke(d);
			}
			else
			{
				;
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			string Cmd;
			string Mes;

			if(ClientCommandNo == 0)
			{
				Cmd = "0000";
				Mes = null;
			}
			else if (ClientCommandNo == 1)
			{
				Cmd = "0001";
				Mes = "うつよっ  ばーーん";
			}
			else if (ClientCommandNo == 2)
			{
				Cmd = "0002";
				Mes = "やっほーー";
			}
			else if (ClientCommandNo == 3)
			{
				Cmd = "0003";
				Mes = "えっ   なんだろー";
			}
			else if (ClientCommandNo == 4)
			{
				Cmd = "0004";
				Mes = "ぐるぐる";
			}
			else
			{
				Cmd = "0000";
				Mes = null;
			}

			ClientSendMsg = Cmd + ";" + Mes;

			///////////////////////////////////////////////
//			if (ClientCommandNo != PreviousClientCommandNo && resMsg != "busy")
			if (resMsg != "busy")
			{
				label23.Text = "toClient: " + Cmd + "\nC= " + ClientCommandNo.ToString() + " P = " + PreviousClientCommandNo + "\ncStatus: " + resMsg;
				toClientSend();
				pictureBox1.BackColor = Color.White;
			}
			else
			{
				label23.Text = "toClient: " + "    " + "\nC= " + " " + " P = " + " " + "\ncStatus: " + resMsg;
				pictureBox1.BackColor = Color.MistyRose;
			}
			PreviousClientCommandNo = ClientCommandNo;
		}

		void toClientSend()
		{
			byte[] sendBytes = enc.GetBytes(ClientSendMsg + '\n');
			ns.Write(sendBytes, 0, sendBytes.Length);
		}

		private void button9_Click(object sender, EventArgs e)
		{
			SEND_CLIENT_INTERVAL = int.Parse(textBox15.Text);
		}
	}
}