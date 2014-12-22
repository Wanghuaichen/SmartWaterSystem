﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using Protocol;

namespace NoiseAnalysisSystem
{
    public partial class UcRecMgr : DevExpress.XtraEditors.XtraUserControl
    {
        private FrmSystem main;
        private bool isSetting;

        public UcRecMgr(FrmSystem frm)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            main = frm;
        }

        #region 输入验证
        /// <summary>
        /// 验证记录仪管理选项卡输入是否正确
        /// </summary>
        private bool ValidateRecorderManageInput(out string msg)
        {
            bool ok;
            msg = string.Empty;
            if (string.IsNullOrEmpty(txtRecNote.Text))
            {
                txtRecNote.Focus();
                txtRecNote.SelectAll();
                msg = "记录仪备注未输入！";
                return false;
            }
            ok = MetarnetRegex.IsUint(txtRecID.Text);
            if (!ok)
            {
                txtRecID.Focus();
                txtRecID.SelectAll();
                msg = "记录仪编号设置错误！";
                return ok;
            }
            ok = MetarnetRegex.IsTime(txtComTime.Text);
            if (!ok)
            {
                txtComTime.Focus();
                txtComTime.SelectAll();
                msg = "通讯时间设置错误！";
                return ok;
            }
            ok = MetarnetRegex.IsTime(txtRecTime.Text);
            if (!ok)
            {
                txtRecTime.Focus();
                txtRecTime.SelectAll();
                msg = "记录时间设置错误！";
                return ok;
            }
            ok = MetarnetRegex.IsUint(txtLeakValue.Text);
            if (!ok)
            {
                txtLeakValue.Focus();
                txtLeakValue.SelectAll();
                msg = "报漏幅度值设置错误！";
                return ok;
            }

            //if (cbConStart.Checked)
            //{
            ok = MetarnetRegex.IsUint(txtConId.Text);
            if (!ok)
            {
                txtConId.Focus();
                txtConId.SelectAll();
                msg = "控制器编号设置错误！";
                return ok;
            }
            ok = MetarnetRegex.IsUint(txtConPort.Text);
            if (!ok)
            {
                txtConPort.Focus();
                txtConPort.SelectAll();
                msg = "远传端口设置错误！";
                return ok;
            }
            ok = MetarnetRegex.IsIPv4(txtConAdress.Text);
            if (!ok)
            {
                txtConAdress.Focus();
                txtConAdress.SelectAll();
                msg = "远传地址设置错误！";
                return ok;
            }
            //}

            // 通讯时间与记录时间不能重叠
            int comTime = Convert.ToInt32(txtComTime.Text);
            int recTime1 = Convert.ToInt32(txtRecTime.Text);
            int recTime2 = Convert.ToInt32(txtRecTime1.Text);

            if (comTime == recTime1 || comTime == recTime2 || (comTime > recTime1 && comTime < recTime2))
            {
                txtComTime.Focus();
                txtComTime.SelectAll();
                msg = "通讯时间/记录时间设置重叠！";
                return false;
            }

            return ok;
        }
        #endregion

        /// <summary>
        /// 绑定记录仪列表
        /// </summary>
        public void BindRecord()
        {
            DataTable dt = new DataTable("RecordTable");
            DataColumn col = dt.Columns.Add("选择");
            col.DataType = System.Type.GetType("System.Boolean"); ;
            dt.Columns.Add("编号");
            dt.Columns.Add("漏水警戒值");
            dt.Columns.Add("通讯时间");
            dt.Columns.Add("采集时间");
            dt.Columns.Add("远传功能");
            dt.Columns.Add("添加时间");
            dt.Columns.Add("备注");

            for (int i = 0; i < GlobalValue.recorderList.Count; i++)
            {
                NoiseRecorder rec = GlobalValue.recorderList[i];
                dt.Rows.Add(new object[] { false, rec.ID, rec.LeakValue, rec.CommunicationTime, rec.RecordTime, rec.ControlerPower, rec.AddDate, rec.Remark });
            }

            // 绑定数据
            MethodInvoker mi = (new MethodInvoker(() =>
            {
                gridControlRec.DataSource = dt;
            }));

            this.Invoke(mi);
        }

        // 读取记录仪编号
        private void btnGetRecID_Click(object sender, EventArgs e)
        {
            Control cl = sender as Control;
            try
            {
                cl.Enabled = false;
                int id = GlobalValue.log.ReadNoiseLogID();

                this.txtRecID.Text = id.ToString();
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("读取失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                cl.Enabled = true;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!isSetting)
            {
                short id = 0;
                bool isError = false;
                try
                {
                    main.DisableRibbonBar();
                    main.DisableNavigateBar();
                    id = Convert.ToInt16(txtRecID.Text);
                    btnStart.Enabled = false;

                    short[] Originaldata = null;
                    GlobalValue.log.CtrlStartOrStop(id, true, out Originaldata);
                    main.ShowWaitForm("", "正在启动...");
                    lblRecState.Text = "运行状态  正在启动";

                    NoiseRecorder rec = (from item in GlobalValue.recorderList.AsEnumerable()
                                         where item.ID == id
                                         select item).ToList()[0];
                    rec.Power = 1;

                    NoiseDataBaseHelper.UpdateRecorder(rec);
                    if (NoiseDataBaseHelper.SaveStandData(rec.GroupID, rec.ID, Originaldata) < 0)
                    {
                        XtraMessageBox.Show("保存记录仪数据失败!", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        isError = true;
                    }
                }
                catch (TimeoutException)
                {
                    XtraMessageBox.Show("记录仪" + id + "读取超时！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isError = true;
                }
                catch (ArgumentNullException)
                {
                    XtraMessageBox.Show("记录仪" + id + "数据为空！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    isError = true;
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isError = true;
                }
                finally
                {
                    main.EnableRibbonBar();
                    main.EnableNavigateBar();
                    btnStart.Enabled = true;
                    main.HideWaitForm();
                    if (!isError)
                    {
                        lblRecState.Text = "运行状态  已启动";
                        btnStart.Enabled = false;
                        btnStop.Enabled = true;
                    }
                    else
                    {
                        lblRecState.Text = "运行状态  未知";
                        btnStart.Enabled = true;
                        btnStop.Enabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Test
        /// </summary>
        /// <returns>获取随机数组(0-255)[32]</returns>
        private short[] GetRandomShortArray()
        {
            List<short> lstRd = new List<short>();
            Random rd = new Random();
            for (int i = 0; i < 32; i++)
                lstRd.Add((short)rd.Next(0, 255));

            return lstRd.ToArray();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            if (!isSetting)
            {
                bool isError = false;
                try
                {
                    main.DisableRibbonBar();
                    main.DisableNavigateBar();
                    btnStop.Enabled = false; ;
                    short id = Convert.ToInt16(txtRecID.Text);
                    short[] origitydata = null;
                    GlobalValue.log.CtrlStartOrStop(id, false, out origitydata);
                    main.ShowWaitForm("", "正在停止...");
                    lblRecState.Text = "运行状态  正在停止";

                    NoiseRecorder rec = (from item in GlobalValue.recorderList.AsEnumerable()
                                         where item.ID == id
                                         select item).ToList()[0];
                    rec.Power = 0;
                    NoiseDataBaseHelper.UpdateRecorder(rec);
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show(ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    isError = true;
                }
                finally
                {
                    main.EnableRibbonBar();
                    main.EnableNavigateBar();
                    btnStop.Enabled = true;
                    main.HideWaitForm();
                    if (!isError)
                    {
                        lblRecState.Text = "运行状态  已停止";
                        btnStart.Enabled = true;
                        btnStop.Enabled = false;
                    }
                }
            }
        }

        // 时间同步
        private void btnNow_Click(object sender, EventArgs e)
        {
            dateTimePicker.Value = DateTime.Now;
        }

        // 添加记录仪
        private void btnAddRec_Click(object sender, EventArgs e)
        {
            try
            {
                string msg = string.Empty;
                if (!ValidateRecorderManageInput(out msg))
                {
                    throw new Exception(msg);
                }

                NoiseRecorder newRec = new NoiseRecorder();
                newRec.ID = Convert.ToInt32(txtRecID.Text);
                newRec.AddDate = DateTime.Now;
                newRec.CommunicationTime = Convert.ToInt32(txtComTime.Text);
                if (comboBoxDist.SelectedIndex == 1)
                    newRec.ControlerPower = 1;
                else if (comboBoxDist.SelectedIndex == 0)
                    newRec.ControlerPower = 0;
                else
                    throw new Exception("未选择远传功能！");

                newRec.LeakValue = Convert.ToInt32(txtLeakValue.Text);
                newRec.Remark = txtRecNote.Text;
                newRec.PickSpan = Convert.ToInt32(nUpDownSamSpan.Value);
                newRec.RecordTime = Convert.ToInt32(txtRecTime.Text);

                int query = NoiseDataBaseHelper.AddRecorder(newRec);
                if (query != -1)
                {
                    GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                    BindRecord();
                }
                else
                    throw new Exception("数据入库时发生错误。");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("添加失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 删除勾选的记录仪
        private void btnDeleteRec_Click(object sender, EventArgs e)
        {
            try
            {
                bool flag = false;

                for (int i = 0; i < gridViewRecordList.RowCount; i++)
                {
                    if ((bool)gridViewRecordList.GetRowCellValue(i, "选择"))
                    {
                        NoiseDataBaseHelper.DeleteRecorder(Convert.ToInt32(gridViewRecordList.GetRowCellValue(i, "编号")));
                        flag = true;
                    }
                }
                if (flag)
                {
                    DialogResult dr = XtraMessageBox.Show("确定删除勾选的记录仪？", GlobalValue.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == System.Windows.Forms.DialogResult.Yes)
                    {
                        GlobalValue.ClearText(groupControl1);
                        GlobalValue.ClearText(groupControl1);
                        GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                        GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                        //LoadRecorderList();
                        //LoadGroupTree();
                        //LoadGroupList();
                        BindRecord();
                        GlobalValue.reReadIdList.Clear();
                    }
                    else
                        return;
                }
                else
                    XtraMessageBox.Show("未勾选任何记录仪！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("删除失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 读取模板参数
        private void btnReadT_Click(object sender, EventArgs e)
        {
            txtComTime.Text = AppConfigHelper.GetAppSettingValue("ComTime_Template");
            txtRecTime.Text = AppConfigHelper.GetAppSettingValue("RecTime_Template");
            nUpDownSamSpan.Value = Convert.ToInt32(AppConfigHelper.GetAppSettingValue("Span_Template"));
            txtRecNum.Text = (GlobalValue.Time * 60 / Convert.ToInt32(AppConfigHelper.GetAppSettingValue("Span_Template"))).ToString();
            txtLeakValue.Text = AppConfigHelper.GetAppSettingValue("LeakValue_Template");

            int power = Convert.ToInt32(AppConfigHelper.GetAppSettingValue("Power_Template"));
            comboBoxEditPower.SelectedIndex = power;

            int conPower = Convert.ToInt32(AppConfigHelper.GetAppSettingValue("ControlPower_Template"));
            if (conPower == 1)
            {
                comboBoxDist.SelectedIndex = conPower;
                txtConPort.Text = AppConfigHelper.GetAppSettingValue("Port_Template");
                txtConAdress.Text = AppConfigHelper.GetAppSettingValue("Adress_Template");
            }
            else
                comboBoxDist.SelectedIndex = conPower;
        }

        // 读取设备参数
        private void btnReadSet_Click(object sender, EventArgs e)
        {
            new Action(() =>
            {
                main.EnableRibbonBar();
                main.EnableNavigateBar();
                main.ShowWaitForm("", "正在读取设备参数...");
                main.barStaticItemWait.Caption = "正在读取设备参数...";
                Control cl = sender as Control;
                try
                {
                    if (txtRecID.Text == string.Empty)
                        throw new Exception("请检查记录仪编号是否正确。");

                    cl.Enabled = false;
                    short id = Convert.ToInt16(txtRecID.Text);//device.ReadNoiseLogID();

                    // 读取记录时间段
                    byte[] tt = GlobalValue.log.ReadStartEndTime(id);
                    txtRecTime.Text = tt[0].ToString();
                    txtRecTime1.Text = tt[1].ToString();

                    // 读取采集间隔
                    nUpDownSamSpan.Value = GlobalValue.log.ReadInterval(id);

                    // 读取远传通讯时间
                    this.txtComTime.Text = GlobalValue.log.ReadRemoteSendTime(id).ToString();

                    // 读取远传功能
                    if (GlobalValue.log.ReadRemote(id))
                    {
                        comboBoxDist.SelectedIndex = 1;
                        NoiseCtrl ctrl = new NoiseCtrl();

                        // 读取远传端口
                        this.txtConPort.Text = ctrl.ReadPort(id).ToString();

                        // 读取远传地址
                        this.txtConAdress.Text = ctrl.ReadIP(id);
                    }
                    else
                    {
                        comboBoxDist.SelectedIndex = 0;
                    }

                    // 读取时间
                    byte[] tt1 = GlobalValue.log.ReadTime(id);
                    this.dateTimePicker.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, tt1[0], tt1[1], tt1[2]);
                    main.barStaticItemWait.Caption = "读取成功";
                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("读取失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    main.barStaticItemWait.Caption = "读取失败"; ;
                }
                finally
                {
                    main.EnableRibbonBar();
                    main.EnableNavigateBar();
                    main.HideWaitForm();
                    cl.Enabled = true;
                }
            }).BeginInvoke(null, null);
        }

        // 应用设置
        private void btnApplySet_Click(object sender, EventArgs e)
        {
            new Action(() =>
            {
                isSetting = true;
                Control cl = sender as Control;
                try
                {
                    string msg = string.Empty;
                    if (!ValidateRecorderManageInput(out msg))
                    {
                        throw new Exception(msg);
                    }

                    main.DisableRibbonBar();
                    main.DisableNavigateBar();
                    main.ShowWaitForm("", "正在应用当前设置...");
                    main.barStaticItemWait.Caption = "正在应用当前设置...";

                    cl.Enabled = false;
                    short id = Convert.ToInt16(txtRecID.Text);
                    NoiseRecorder alterRec = (from item in GlobalValue.recorderList
                                              where item.ID == id
                                              select item).ToList()[0];

                    alterRec.ID = Convert.ToInt32(txtRecID.Text);
                    alterRec.LeakValue = Convert.ToInt32(txtLeakValue.Text);
                    alterRec.Remark = txtRecNote.Text;

                    // 设置记录时间段
                    GlobalValue.log.WriteStartEndTime(id, Convert.ToInt32(txtRecTime.Text), Convert.ToInt32(txtRecTime1.Text));
                    alterRec.RecordTime = Convert.ToInt32(txtRecTime.Text);

                    // 设置采集间隔
                    GlobalValue.log.WriteInterval(id, (int)nUpDownSamSpan.Value);
                    alterRec.PickSpan = Convert.ToInt32(nUpDownSamSpan.Value);

                    // 设置远传通讯时间
                    GlobalValue.log.WriteRemoteSendTime(id, Convert.ToInt32(txtComTime.Text));
                    alterRec.CommunicationTime = Convert.ToInt32(txtComTime.Text);

                    // 设置远传功能
                    if (comboBoxDist.SelectedIndex == 1)
                    {
                        GlobalValue.log.WriteRemoteSwitch(id, true);

                        NoiseCtrl ctrl = new NoiseCtrl();

                        // 设置远传IP
                        ctrl.Write_IP(id, txtConAdress.Text);
                        // 设置远传端口
                        ctrl.WritePortName(id, txtConPort.Text);

                        alterRec.ControlerPower = 1;
                        DistanceController alterCtrl = new DistanceController();
                        alterCtrl.ID = Convert.ToInt32(txtConId.Text);
                        alterCtrl.RecordID = alterRec.ID;
                        alterCtrl.Port = Convert.ToInt32(txtConPort.Text);
                        alterCtrl.IPAdress = txtConAdress.Text;

                        NoiseDataBaseHelper.UpdateControler(alterCtrl);
                    }
                    else
                    {
                        GlobalValue.log.WriteRemoteSwitch(id, false);
                        alterRec.ControlerPower = 0;
                    }
                    short[] origitydata = null;
                    // 设置开关
                    if (comboBoxEditPower.SelectedIndex == 1)
                    {
                        GlobalValue.log.CtrlStartOrStop(id, true, out origitydata);
                        alterRec.Power = 1;
                    }
                    else if (comboBoxEditPower.SelectedIndex == 0)
                    {
                        GlobalValue.log.CtrlStartOrStop(id, false, out origitydata);
                        alterRec.Power = 0;
                    }

                    // 设置记录仪时间
                    GlobalValue.log.WriteTime(id, this.dateTimePicker.Value);

                    // 更新设置入库
                    int query = NoiseDataBaseHelper.UpdateRecorder(alterRec);
                    if (query != -1)
                    {
                        XtraMessageBox.Show("设置成功！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                        GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                        BindRecord();
                    }
                    else
                        throw new Exception("数据入库发生错误。");
                    main.barStaticItemWait.Caption = "当前设置已应用到记录仪" + id;

                }
                catch (Exception ex)
                {
                    XtraMessageBox.Show("设置失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    main.barStaticItemWait.Caption = "设置失败";
                }
                finally
                {
                    main.EnableRibbonBar();
                    main.EnableNavigateBar();
                    main.HideWaitForm();
                    cl.Enabled = true;
                    isSetting = false;
                }
            }).BeginInvoke(null, null);
        }

        private void gridViewRecordList_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.Caption == "远传功能")
            {
                if (Convert.ToInt32(e.Value) == 1)
                    e.DisplayText = "开";
                else
                    e.DisplayText = "关";
            }
        }

        private void UcRecMgr_Load(object sender, EventArgs e)
        {
            //BindRecord();
            this.timer1.Interval = 1000;   //1s
            this.timer1.Tick += new EventHandler(timer1_Tick);
            this.timer1.Enabled = true;
        }

        void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                DateTime datenow = this.dateTimePicker.Value;
                if (datenow.CompareTo(DateTime.Now.AddDays(-1)) > -1)
                    this.dateTimePicker.Value = datenow.AddSeconds(1);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void gridViewRecordList_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            txtConAdress.Text = "";
            txtConId.Text = "";
            txtConPort.Text = "";

            NoiseRecorder rec = (from item in GlobalValue.recorderList.AsEnumerable()
                                 where item.ID == Convert.ToInt32(gridViewRecordList.GetFocusedRowCellValue("编号"))
                                 select item).ToList()[0];

            txtRecID.Text = rec.ID.ToString();
            txtComTime.Text = rec.CommunicationTime.ToString();
            txtRecTime.Text = rec.RecordTime.ToString();

            nUpDownSamSpan.Value = rec.PickSpan;
            txtRecNum.Text = (GlobalValue.Time * 60 / rec.PickSpan).ToString();
            txtLeakValue.Text = rec.LeakValue.ToString();
            txtRecNote.Text = rec.Remark;
            if (rec.Power == 1)
            {
                lblRecState.Text = "运行状态  已启动";
                comboBoxEditPower.SelectedIndex = rec.Power;
            }
            else if (rec.Power == 0)
            {
                lblRecState.Text = "运行状态  已停止";
                comboBoxEditPower.SelectedIndex = rec.Power;
            }
            else
            {
                lblRecState.Text = "运行状态  未知";
                comboBoxEditPower.SelectedIndex = 2;
            }


            if (rec.ControlerPower == 1)
            {
                DistanceController dc = (from item in GlobalValue.controllerList.AsEnumerable()
                                         where item.ID == rec.ID
                                         select item).ToList()[0];

                txtConId.Text = dc.ID.ToString();
                txtConPort.Text = dc.Port.ToString();
                txtConAdress.Text = dc.IPAdress.ToString();
            }
            comboBoxDist.SelectedIndex = rec.ControlerPower;

            btnDeleteRec.Enabled = true;

            if (GlobalValue.portUtil.IsOpen)
            {
                btnStart.Enabled = true;
                btnStop.Enabled = true;
                btnApplySet.Enabled = true;
                btnNow.Enabled = true;
            }

            btnDeleteRec.Enabled = true;

            if (!btnUpdate.Enabled)
                btnUpdate.Enabled = true;
        }

        private void comboBoxDist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxDist.SelectedIndex == 1)
                groupControl4.Enabled = true;
            else
                groupControl4.Enabled = false;
        }

        private void gridViewRecordList_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e.Column.Caption == "选择")
            {
                //for (int i = 0; i < gridViewRecordList.RowCount; i++)
                //{ 
                //    if(()gridViewRecordList.GetRowCellValue(i,"选择")
                //}
            }
        }

        private void nUpDownSamSpan_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown obj = sender as NumericUpDown;
            switch (obj.Name)
            {
                case "nUpDownSamSpan":
                    txtRecNum.Text = (Math.Floor(GlobalValue.Time * 60 / obj.Value)).ToString();
                    break;
                default:
                    break;
            }
        }

        private void txtRecTime_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DevExpress.XtraEditors.TextEdit obj = sender as DevExpress.XtraEditors.TextEdit;
                int t = 0;
                int t1 = 0;
                switch (obj.Name)
                {
                    case "txtRecTime":
                        t = Convert.ToInt32(obj.Text);
                        t1 = t + GlobalValue.Time;
                        if (t1 > 24)
                            txtRecTime1.Text = (t1 - 24).ToString();
                        else
                            txtRecTime1.Text = (t + GlobalValue.Time).ToString();
                        break;
                }
            }
            catch (Exception)
            {

            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtLeakValue.Text) || string.IsNullOrEmpty(txtRecNote.Text))
            {
                XtraMessageBox.Show("记录仪信息不完整！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int id = Convert.ToInt32(gridViewRecordList.GetFocusedRowCellValue("编号"));

            NoiseRecorder alterRec = (from item in GlobalValue.recorderList
                                      where item.ID == id
                                      select item).ToList()[0];
            alterRec.LeakValue = Convert.ToInt32(txtLeakValue.Text);
            alterRec.Remark = txtRecNote.Text;

            int query = NoiseDataBaseHelper.UpdateRecorder(alterRec);
            if (query != -1)
            {
                XtraMessageBox.Show("更新成功！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                BindRecord();
            }
        }

        private void txtRecID_TextChanged(object sender, EventArgs e)
        {
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = true;
            this.lblRecState.Text = "运行状态  未知";
        }

    }
}
