namespace _HandMotionAirVR
{
	partial class ComSetting
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.label1 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.comboBox3 = new System.Windows.Forms.ComboBox();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(22, 26);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 12);
			this.label1.TabIndex = 0;
			this.label1.Text = "label1";
			// 
			// comboBox1
			// 
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "COM2",
            "COM10",
            "COM11",
            "COM12",
            "COM13",
            "COM14",
            "COM15"});
			this.comboBox1.Location = new System.Drawing.Point(136, 98);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(70, 20);
			this.comboBox1.TabIndex = 1;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(45, 211);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 2;
			this.button1.Text = "初期化";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(160, 211);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 3;
			this.button2.Text = "閉じる";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(80, 102);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(54, 12);
			this.label2.TabIndex = 4;
			this.label2.Text = "COM設定";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(65, 135);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(69, 12);
			this.label3.TabIndex = 5;
			this.label3.Text = "センサ1 MAC";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Enabled = false;
			this.label4.Location = new System.Drawing.Point(65, 163);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(69, 12);
			this.label4.TabIndex = 5;
			this.label4.Text = "センサ2 MAC";
			// 
			// comboBox2
			// 
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Items.AddRange(new object[] {
            "810208A1",
            "810209F8",
            "8102096F",
            "81020997"});
			this.comboBox2.Location = new System.Drawing.Point(141, 132);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(84, 20);
			this.comboBox2.TabIndex = 6;
			this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
			// 
			// comboBox3
			// 
			this.comboBox3.Enabled = false;
			this.comboBox3.FormattingEnabled = true;
			this.comboBox3.Items.AddRange(new object[] {
            "810208A1",
            "810209F8",
            "8102096F",
            "81020997"});
			this.comboBox3.Location = new System.Drawing.Point(141, 163);
			this.comboBox3.Name = "comboBox3";
			this.comboBox3.Size = new System.Drawing.Size(84, 20);
			this.comboBox3.TabIndex = 7;
			this.comboBox3.SelectedIndexChanged += new System.EventHandler(this.comboBox3_SelectedIndexChanged);
			// 
			// ComSetting
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(284, 269);
			this.Controls.Add(this.comboBox3);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.label1);
			this.Name = "ComSetting";
			this.Text = "ComSetting";
			this.Load += new System.EventHandler(this.ComSetting_Load);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox comboBox1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label2;
		private System.IO.Ports.SerialPort serialPort1;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox comboBox2;
		private System.Windows.Forms.ComboBox comboBox3;
	}
}