﻿using Common;
namespace SmartWaterSystem
{
    partial class FrmDataAnalysis
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmDataAnalysis));
            this.groupBox1 = new DevExpress.XtraEditors.GroupControl();
            this.txtRemark = new DevExpress.XtraEditors.MemoEdit();
            this.label6 = new System.Windows.Forms.Label();
            this.txtRecID = new DevExpress.XtraEditors.TextEdit();
            this.label4 = new System.Windows.Forms.Label();
            this.txtPickSpan = new DevExpress.XtraEditors.TextEdit();
            this.txtRecNum = new DevExpress.XtraEditors.TextEdit();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtRecTime = new DevExpress.XtraEditors.TextEdit();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.txtRecTime1 = new DevExpress.XtraEditors.TextEdit();
            this.label16 = new System.Windows.Forms.Label();
            this.txtComTime = new DevExpress.XtraEditors.TextEdit();
            this.label17 = new System.Windows.Forms.Label();
            this.groupBox2 = new DevExpress.XtraEditors.GroupControl();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLeakHz = new DevExpress.XtraEditors.TextEdit();
            this.txtLeakNoise = new DevExpress.XtraEditors.TextEdit();
            this.txtMinHz = new DevExpress.XtraEditors.TextEdit();
            this.txtMinNoise = new DevExpress.XtraEditors.TextEdit();
            this.txtMaxHz = new DevExpress.XtraEditors.TextEdit();
            this.txtEnergyValue = new DevExpress.XtraEditors.TextEdit();
            this.txtMaxNoise = new DevExpress.XtraEditors.TextEdit();
            this.label2 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.groupBox4 = new DevExpress.XtraEditors.GroupControl();
            this.txtNum = new DevExpress.XtraEditors.TextEdit();
            this.label7 = new System.Windows.Forms.Label();
            this.c1Chart1 = new C1.Win.C1Chart.C1Chart();
            this.txtCurSeriesValue = new DevExpress.XtraEditors.TextEdit();
            this.colorPanel1 = new ColorPanel();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecID.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPickSpan.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecNum.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecTime1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtComTime.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtLeakHz.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLeakNoise.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMinHz.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMinNoise.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxHz.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEnergyValue.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxNoise.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox4)).BeginInit();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNum.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Chart1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurSeriesValue.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtRemark);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.txtRecID);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.txtPickSpan);
            this.groupBox1.Controls.Add(this.txtRecNum);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.txtRecTime);
            this.groupBox1.Controls.Add(this.label14);
            this.groupBox1.Controls.Add(this.label15);
            this.groupBox1.Controls.Add(this.txtRecTime1);
            this.groupBox1.Controls.Add(this.label16);
            this.groupBox1.Controls.Add(this.txtComTime);
            this.groupBox1.Controls.Add(this.label17);
            this.groupBox1.Location = new System.Drawing.Point(11, 438);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(371, 160);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.Text = "记录仪参数";
            // 
            // txtRemark
            // 
            this.txtRemark.Location = new System.Drawing.Point(10, 91);
            this.txtRemark.Name = "txtRemark";
            this.txtRemark.Properties.ReadOnly = true;
            this.txtRemark.Properties.ScrollBars = System.Windows.Forms.ScrollBars.None;
            this.txtRemark.Size = new System.Drawing.Size(131, 55);
            this.txtRemark.TabIndex = 59;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 14);
            this.label6.TabIndex = 58;
            this.label6.Text = "备注";
            // 
            // txtRecID
            // 
            this.txtRecID.Location = new System.Drawing.Point(88, 34);
            this.txtRecID.Name = "txtRecID";
            this.txtRecID.Properties.ReadOnly = true;
            this.txtRecID.Size = new System.Drawing.Size(52, 20);
            this.txtRecID.TabIndex = 57;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 35);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 14);
            this.label4.TabIndex = 56;
            this.label4.Text = "记录仪编号";
            // 
            // txtPickSpan
            // 
            this.txtPickSpan.Location = new System.Drawing.Point(228, 96);
            this.txtPickSpan.Name = "txtPickSpan";
            this.txtPickSpan.Properties.ReadOnly = true;
            this.txtPickSpan.Size = new System.Drawing.Size(29, 20);
            this.txtPickSpan.TabIndex = 55;
            // 
            // txtRecNum
            // 
            this.txtRecNum.Location = new System.Drawing.Point(228, 126);
            this.txtRecNum.Name = "txtRecNum";
            this.txtRecNum.Properties.ReadOnly = true;
            this.txtRecNum.Size = new System.Drawing.Size(86, 20);
            this.txtRecNum.TabIndex = 54;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(261, 99);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(48, 14);
            this.label11.TabIndex = 53;
            this.label11.Text = "分钟/次";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(156, 97);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(55, 14);
            this.label10.TabIndex = 52;
            this.label10.Text = "采集间隔";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(156, 127);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(55, 14);
            this.label9.TabIndex = 51;
            this.label9.Text = "记录数量";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(263, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(19, 14);
            this.label8.TabIndex = 50;
            this.label8.Text = "点";
            // 
            // txtRecTime
            // 
            this.txtRecTime.Location = new System.Drawing.Point(228, 64);
            this.txtRecTime.Name = "txtRecTime";
            this.txtRecTime.Properties.ReadOnly = true;
            this.txtRecTime.Size = new System.Drawing.Size(29, 20);
            this.txtRecTime.TabIndex = 49;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(155, 65);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(55, 14);
            this.label14.TabIndex = 48;
            this.label14.Text = "记录时间";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(342, 67);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(19, 14);
            this.label15.TabIndex = 47;
            this.label15.Text = "点";
            // 
            // txtRecTime1
            // 
            this.txtRecTime1.Location = new System.Drawing.Point(308, 64);
            this.txtRecTime1.Name = "txtRecTime1";
            this.txtRecTime1.Properties.ReadOnly = true;
            this.txtRecTime1.Size = new System.Drawing.Size(29, 20);
            this.txtRecTime1.TabIndex = 46;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(263, 67);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(35, 14);
            this.label16.TabIndex = 45;
            this.label16.Text = "点 至";
            // 
            // txtComTime
            // 
            this.txtComTime.Location = new System.Drawing.Point(228, 34);
            this.txtComTime.Name = "txtComTime";
            this.txtComTime.Properties.ReadOnly = true;
            this.txtComTime.Size = new System.Drawing.Size(29, 20);
            this.txtComTime.TabIndex = 44;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(155, 35);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(55, 14);
            this.label17.TabIndex = 43;
            this.label17.Text = "通讯时间";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.txtLeakHz);
            this.groupBox2.Controls.Add(this.txtLeakNoise);
            this.groupBox2.Controls.Add(this.txtMinHz);
            this.groupBox2.Controls.Add(this.txtMinNoise);
            this.groupBox2.Controls.Add(this.txtMaxHz);
            this.groupBox2.Controls.Add(this.txtEnergyValue);
            this.groupBox2.Controls.Add(this.txtMaxNoise);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label18);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Location = new System.Drawing.Point(388, 438);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(261, 160);
            this.groupBox2.TabIndex = 56;
            this.groupBox2.Text = "数据分析";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(164, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 14);
            this.label3.TabIndex = 61;
            this.label3.Text = "噪声频率(Hz)";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(81, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 14);
            this.label1.TabIndex = 60;
            this.label1.Text = "噪声幅度(%)";
            // 
            // txtLeakHz
            // 
            this.txtLeakHz.Location = new System.Drawing.Point(166, 101);
            this.txtLeakHz.Name = "txtLeakHz";
            this.txtLeakHz.Properties.ReadOnly = true;
            this.txtLeakHz.Size = new System.Drawing.Size(65, 20);
            this.txtLeakHz.TabIndex = 59;
            // 
            // txtLeakNoise
            // 
            this.txtLeakNoise.Location = new System.Drawing.Point(83, 101);
            this.txtLeakNoise.Name = "txtLeakNoise";
            this.txtLeakNoise.Properties.ReadOnly = true;
            this.txtLeakNoise.Size = new System.Drawing.Size(65, 20);
            this.txtLeakNoise.TabIndex = 58;
            // 
            // txtMinHz
            // 
            this.txtMinHz.Location = new System.Drawing.Point(166, 72);
            this.txtMinHz.Name = "txtMinHz";
            this.txtMinHz.Properties.ReadOnly = true;
            this.txtMinHz.Size = new System.Drawing.Size(65, 20);
            this.txtMinHz.TabIndex = 57;
            // 
            // txtMinNoise
            // 
            this.txtMinNoise.Location = new System.Drawing.Point(83, 72);
            this.txtMinNoise.Name = "txtMinNoise";
            this.txtMinNoise.Properties.ReadOnly = true;
            this.txtMinNoise.Size = new System.Drawing.Size(65, 20);
            this.txtMinNoise.TabIndex = 56;
            // 
            // txtMaxHz
            // 
            this.txtMaxHz.Location = new System.Drawing.Point(166, 43);
            this.txtMaxHz.Name = "txtMaxHz";
            this.txtMaxHz.Properties.ReadOnly = true;
            this.txtMaxHz.Size = new System.Drawing.Size(65, 20);
            this.txtMaxHz.TabIndex = 55;
            // 
            // txtEnergyValue
            // 
            this.txtEnergyValue.Location = new System.Drawing.Point(83, 133);
            this.txtEnergyValue.Name = "txtEnergyValue";
            this.txtEnergyValue.Properties.ReadOnly = true;
            this.txtEnergyValue.Size = new System.Drawing.Size(148, 20);
            this.txtEnergyValue.TabIndex = 54;
            // 
            // txtMaxNoise
            // 
            this.txtMaxNoise.Location = new System.Drawing.Point(83, 43);
            this.txtMaxNoise.Name = "txtMaxNoise";
            this.txtMaxNoise.Properties.ReadOnly = true;
            this.txtMaxNoise.Size = new System.Drawing.Size(65, 20);
            this.txtMaxNoise.TabIndex = 54;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 14);
            this.label2.TabIndex = 52;
            this.label2.Text = "漏水噪声";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(16, 136);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(55, 14);
            this.label18.TabIndex = 43;
            this.label18.Text = "能量强度";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 14);
            this.label5.TabIndex = 48;
            this.label5.Text = "最小噪声";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(17, 46);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(55, 14);
            this.label12.TabIndex = 43;
            this.label12.Text = "最大噪声";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.txtNum);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Location = new System.Drawing.Point(388, 373);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(261, 59);
            this.groupBox4.TabIndex = 58;
            this.groupBox4.Text = "采集信息";
            // 
            // txtNum
            // 
            this.txtNum.Location = new System.Drawing.Point(128, 29);
            this.txtNum.Name = "txtNum";
            this.txtNum.Properties.ReadOnly = true;
            this.txtNum.Size = new System.Drawing.Size(86, 20);
            this.txtNum.TabIndex = 62;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(29, 31);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(79, 14);
            this.label7.TabIndex = 53;
            this.label7.Text = "当前采集次数";
            // 
            // c1Chart1
            // 
            this.c1Chart1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(235)))), ((int)(((byte)(236)))), ((int)(((byte)(239)))));
            this.c1Chart1.Font = new System.Drawing.Font("Tahoma", 9F);
            this.c1Chart1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(31)))), ((int)(((byte)(53)))));
            this.c1Chart1.Location = new System.Drawing.Point(1, 3);
            this.c1Chart1.Name = "c1Chart1";
            this.c1Chart1.PropBag = resources.GetString("c1Chart1.PropBag");
            this.c1Chart1.Size = new System.Drawing.Size(597, 364);
            this.c1Chart1.TabIndex = 61;
            this.c1Chart1.Paint += new System.Windows.Forms.PaintEventHandler(this.c1Chart1_Paint);
            this.c1Chart1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1Chart1_MouseMove);
            // 
            // txtCurSeriesValue
            // 
            this.txtCurSeriesValue.Location = new System.Drawing.Point(604, 347);
            this.txtCurSeriesValue.Name = "txtCurSeriesValue";
            this.txtCurSeriesValue.Properties.ReadOnly = true;
            this.txtCurSeriesValue.Size = new System.Drawing.Size(48, 20);
            this.txtCurSeriesValue.TabIndex = 63;
            this.txtCurSeriesValue.TabStop = false;
            // 
            // colorPanel1
            // 
            this.colorPanel1.Location = new System.Drawing.Point(600, 59);
            this.colorPanel1.Name = "colorPanel1";
            this.colorPanel1.Size = new System.Drawing.Size(59, 268);
            this.colorPanel1.TabIndex = 62;
            // 
            // FrmDataAnalysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(659, 605);
            this.Controls.Add(this.txtCurSeriesValue);
            this.Controls.Add(this.colorPanel1);
            this.Controls.Add(this.c1Chart1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FrmDataAnalysis";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "噪声数据分析";
            this.Load += new System.EventHandler(this.FrmDataAnalysis_Load);
            ((System.ComponentModel.ISupportInitialize)(this.groupBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecID.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPickSpan.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecNum.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecTime1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtComTime.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtLeakHz.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLeakNoise.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMinHz.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMinNoise.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxHz.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtEnergyValue.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxNoise.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBox4)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNum.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Chart1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCurSeriesValue.Properties)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.GroupControl groupBox1;
        private DevExpress.XtraEditors.TextEdit txtPickSpan;
        private DevExpress.XtraEditors.TextEdit txtRecNum;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private DevExpress.XtraEditors.TextEdit txtRecTime;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private DevExpress.XtraEditors.TextEdit txtRecTime1;
        private System.Windows.Forms.Label label16;
        private DevExpress.XtraEditors.TextEdit txtComTime;
        private System.Windows.Forms.Label label17;
		private DevExpress.XtraEditors.GroupControl groupBox2;
        private DevExpress.XtraEditors.TextEdit txtMinHz;
        private DevExpress.XtraEditors.TextEdit txtMinNoise;
        private DevExpress.XtraEditors.TextEdit txtMaxHz;
		private DevExpress.XtraEditors.TextEdit txtMaxNoise;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit txtRecID;
        private System.Windows.Forms.Label label4;
        private DevExpress.XtraEditors.MemoEdit txtRemark;
		private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ErrorProvider errorProvider;
		private DevExpress.XtraEditors.TextEdit txtLeakHz;
		private DevExpress.XtraEditors.TextEdit txtLeakNoise;
		private System.Windows.Forms.Label label2;
		private DevExpress.XtraEditors.GroupControl groupBox4;
		private DevExpress.XtraEditors.TextEdit txtNum;
		private System.Windows.Forms.Label label7;
        private DevExpress.XtraEditors.TextEdit txtEnergyValue;
        private System.Windows.Forms.Label label18;
        private C1.Win.C1Chart.C1Chart c1Chart1;
        private ColorPanel colorPanel1;
        private DevExpress.XtraEditors.TextEdit txtCurSeriesValue;



    }
}