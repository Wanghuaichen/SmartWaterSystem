﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;

namespace NoiseAnalysisSystem
{
    public partial class UcGroupMgr : DevExpress.XtraEditors.XtraUserControl
    {
        private FrmSystem main;
        public UcGroupMgr(FrmSystem frm)
        {
            InitializeComponent();
            this.main = frm;
        }
        
        /// <summary>
        /// 绑定树形列表
        /// </summary>
        public void BindTree()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("KeyFieldName");
            dt.Columns.Add("ParentFieldName");
            dt.Columns.Add("ID");
            dt.Columns.Add("Remark");
            dt.Columns.Add("Name");

            int pFlag = 0;
            int cFlag = 0;
            int tFlag = 0;
            for (int i = 0; i < GlobalValue.groupList.Count; i++)
            {
                DataRow dr = dt.NewRow();
                dr["KeyFieldName"] = tFlag;
                dr["ParentFieldName"] = DBNull.Value;
                dr["ID"] = GlobalValue.groupList[i].ID;
                dr["Remark"] = GlobalValue.groupList[i].Remark;
                dr["Name"] = GlobalValue.groupList[i].Name;
                cFlag = tFlag + 1;
                pFlag = tFlag;
                dt.Rows.Add(dr);
                for (int j = 0; j < GlobalValue.groupList[i].RecorderList.Count; j++)
                {
                    DataRow dr1 = dt.NewRow();
                    dr1["KeyFieldName"] = cFlag;
                    dr1["ParentFieldName"] = pFlag;
                    dr1["ID"] = GlobalValue.groupList[i].RecorderList[j].ID;
                    dr1["Remark"] = GlobalValue.groupList[i].RecorderList[j].Remark;
                    dt.Rows.Add(dr1);
                    cFlag++;
                    tFlag++;
                }
                pFlag++;
                tFlag++;
            }

            treeList1.DataSource = dt;
            treeList1.ParentFieldName = "ParentFieldName";
            treeList1.KeyFieldName = "KeyFieldName";

            treeList1.ExpandAll();
        }

        /// <summary>
        /// 绑定可用记录仪列表
        /// </summary>
        public void BindListBox()
        {
            listBoxRec.Items.Clear();
            for (int i = 0; i < GlobalValue.recorderList.Count; i++)
            {
                if (GlobalValue.recorderList[i].GroupState == 0)
                    listBoxRec.Items.Add(GlobalValue.recorderList[i].ID.ToString());
            }
        }

        private void treeList1_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (e.Node == null)
                return;

            if (e.Node.Level == 0)
            {
                btnDelRecFromGroup.Enabled = false;

                foreach (DevExpress.XtraTreeList.Nodes.TreeListNode item in e.Node.Nodes)
                {
                    item.CheckState = e.Node.CheckState;
                }

                if (treeList1.GetAllCheckedNodes().Count > 0)
                {
                    btnDeleteGroup.Enabled = true;
                    btnDelRecFromGroup.Enabled = true;
                }
                else
                {
                    btnDeleteGroup.Enabled = false;
                    btnDelRecFromGroup.Enabled = false;
                }
            }
            else if (e.Node.Level == 1)
            {
                if (treeList1.GetAllCheckedNodes().Count > 0)
                {
                    btnDelRecFromGroup.Enabled = true;
                }
                else
                {
                    btnDelRecFromGroup.Enabled = false;
                }
            }
        }

        private void simpleButtonSelectAll_Click(object sender, EventArgs e)
        {
            foreach (DevExpress.XtraTreeList.Nodes.TreeListNode item in treeList1.Nodes)
            {
                item.CheckState = CheckState.Checked;

                DevExpress.XtraTreeList.NodeEventArgs arg = new DevExpress.XtraTreeList.NodeEventArgs(item);
                treeList1_AfterCheckNode(treeList1, arg);
            }
        }

        private void simpleButtonUnSelect_Click(object sender, EventArgs e)
        {
            foreach (DevExpress.XtraTreeList.Nodes.TreeListNode item in treeList1.Nodes)
            {
                if (item.Checked)
                    item.CheckState = CheckState.Unchecked;
                else
                    item.CheckState = CheckState.Checked;

                DevExpress.XtraTreeList.NodeEventArgs arg = new DevExpress.XtraTreeList.NodeEventArgs(item);
                treeList1_AfterCheckNode(treeList1, arg);
            }
        }

        private void treeList1_AfterFocusNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (e.Node.Level == 0)
            {
                DataRowView drv = treeList1.GetDataRecordByNode(e.Node) as DataRowView;
                txtGroupID.Text = drv["ID"].ToString();
                txtGroupName.Text = drv["Name"].ToString();
                txtGroupNote.Text = drv["Remark"].ToString();

                btnAlterGroupSet.Enabled = true;

                if (listBoxRec.Items.Count == 0)
                    btnImportRec.Enabled = false;
                else
                    btnImportRec.Enabled = true;

                if (e.Node.Nodes.Count > 0)
                    groupControl3.Enabled = true;
                else
                    groupControl3.Enabled = false;
            }
            else
            {
                groupControl3.Enabled = false;
                btnAlterGroupSet.Enabled = false;
                btnImportRec.Enabled = false;
            }
        }

        private void btnAlterGroupSet_Click(object sender, EventArgs e)
        {
            try
            {
                NoiseRecorderGroup alterGrp = new NoiseRecorderGroup();
                alterGrp.ID = Convert.ToInt32(txtGroupID.Text);
                alterGrp.Name = txtGroupName.Text;
                alterGrp.Remark = txtGroupNote.Text;
                int query = NoiseDataBaseHelper.UpdateGroup(alterGrp);

                if (query != -1)
                {
                    GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                    XtraMessageBox.Show("更新成功！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                    //LoadGroupList();
                    BindTree();
                }
                else
                    throw new Exception("数据入库发生错误。");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("更新失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAddGroup_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtGroupName.Text) || string.IsNullOrEmpty(txtGroupNote.Text))
                {
                    XtraMessageBox.Show("分组名称/分组备注未输入！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                NoiseRecorderGroup newGrp = new NoiseRecorderGroup();
                newGrp.Name = txtGroupName.Text;
                newGrp.Remark = txtGroupNote.Text;
                int query = NoiseDataBaseHelper.AddGroup(newGrp);

                if (query != -1)
                {
                    GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                    BindTree();
                }
                else
                    throw new Exception("数据入库发生错误。");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("添加失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDeleteGroup_Click(object sender, EventArgs e)
        {
            List<DevExpress.XtraTreeList.Nodes.TreeListNode> nodes = treeList1.GetAllCheckedNodes();
            if (nodes.Count == 0)
                return;

            DialogResult dr = XtraMessageBox.Show("确定删除所选分组？", GlobalValue.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (dr == System.Windows.Forms.DialogResult.Yes)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Level == 0)
                    {
                        DataRowView drv = treeList1.GetDataRecordByNode(nodes[i]) as DataRowView;
                        int query = NoiseDataBaseHelper.DeleteGroup(Convert.ToInt32(drv["ID"]));
                        if (query == -1)
                            throw new Exception("数据入库发生错误。");
                    }
                }
                GlobalValue.ClearText(groupControl2);
                GlobalValue.ClearText(groupControl3);
                GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                BindTree();
                GlobalValue.reReadIdList.Clear();
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

        private void nUpDownSamSpan_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown obj = sender as NumericUpDown;
            switch (obj.Name)
            {
                case "nUpDownSamSpan":
                    txtRecNum.Text = (GlobalValue.Time * 60 / obj.Value).ToString();
                    break;
                default:
                    break;
            }
        }

        private void btnNow_Click(object sender, EventArgs e)
        {
            dateTimePicker.Value = DateTime.Now;
        }

        // 分配记录仪
        private void btnImportRec_Click(object sender, EventArgs e)
        {
            if (treeList1.Selection != null)
            {
                if (treeList1.Selection[0].Level == 1)
                    return;

                if (listBoxRec.Items.Count == 0)
                {
                    XtraMessageBox.Show("当前无记录仪可分配！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (listBoxRec.SelectedItems.Count == 0)
                {
                    XtraMessageBox.Show("请选择需要分配的记录仪！", GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                NoiseRecorderGroup gp = (from temp in GlobalValue.groupList
                                         where temp.ID == Convert.ToInt32(treeList1.Selection[0].GetValue("ID"))
                                         select temp).ToList()[0];

                for (int i = 0; i < listBoxRec.SelectedItems.Count; i++)
                {
                    NoiseRecorder tmp = (from item in GlobalValue.recorderList.AsEnumerable()
                                         where item.ID.ToString() == listBoxRec.SelectedItems[i].ToString()
                                         select item).ToList()[0];

                    tmp.GroupState = 1;
                    NoiseDataBaseHelper.AddRecorderGroupRelation(tmp.ID, gp.ID);
                    NoiseDataBaseHelper.UpdateRecorder(tmp);

                }

                gp.RecorderList = NoiseDataBaseHelper.GetRecordersByGroupId(gp.ID);
                GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                BindTree();
                BindListBox();
            }
        }

        // 移除记录仪
        private void btnDelRecFromGroup_Click(object sender, EventArgs e)
        {
            try
            {
                //DialogResult dr = MessageBox.Show("确定移除该记录仪？", this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                //if (dr == System.Windows.Forms.DialogResult.Yes)
                //{

                List<DevExpress.XtraTreeList.Nodes.TreeListNode> nodes = treeList1.GetAllCheckedNodes();
                foreach (DevExpress.XtraTreeList.Nodes.TreeListNode item in nodes)
                {
                    if (item.Level == 1)
                    {
                        int recID = Convert.ToInt32(item.GetValue("ID"));
                        int gID = Convert.ToInt32(item.ParentNode.GetValue("ID"));

                        for (int i = 0; i < GlobalValue.recorderList.Count; i++)
                        {
                            if (GlobalValue.recorderList[i].ID == recID)
                            {
                                GlobalValue.recorderList[i].GroupState = 0;
                                NoiseDataBaseHelper.UpdateRecorder(GlobalValue.recorderList[i]);
                                break;
                            }
                        }

                        int query = NoiseDataBaseHelper.DeleteOneRelation(recID, gID);
                        if (query != -1)
                        {

                        }
                    }
                }

                btnDelRecFromGroup.Enabled = false;
                //NoiseRecorderGroup gp = (from temp in GlobalValue.groupList
                //                         where temp.ID == gID
                //                         select temp).ToList()[0];
                //gp.RecorderList = NoiseDataBaseHelper.GetRecordersByGroupId(gp.ID);
                GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
                GlobalValue.groupList = NoiseDataBaseHelper.GetGroups();
                BindTree();
                BindListBox();
                GlobalValue.reReadIdList.Clear();
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show("移除失败：" + ex.Message, GlobalValue.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnImportApply_Click(object sender, EventArgs e)
        {
            NoiseRecorderGroup gp = (from temp in GlobalValue.groupList
                                     where temp.ID == Convert.ToInt32(btnImportRec.Tag)
                                     select temp).ToList()[0];

            for (int i = 0; i < listBoxRec.SelectedItems.Count; i++)
            {
                NoiseRecorder tmp = (from item in GlobalValue.recorderList.AsEnumerable()
                                     where item.ID.ToString() == listBoxRec.SelectedItems[i].ToString()
                                     select item).ToList()[0];

                tmp.GroupState = 1;
                NoiseDataBaseHelper.AddRecorderGroupRelation(tmp.ID, gp.ID);
                NoiseDataBaseHelper.UpdateRecorder(tmp);

            }

            gp.RecorderList = NoiseDataBaseHelper.GetRecordersByGroupId(gp.ID);
            GlobalValue.recorderList = NoiseDataBaseHelper.GetRecorders();
            BindTree();
            listBoxRec.Items.Clear();
        }
    }
}
