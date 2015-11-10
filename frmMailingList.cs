using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using Microsoft.Office.Interop.Excel;
using System.Threading;

namespace SpeedyMail
{
    public partial class frmMailingList : Form
    {
        SQLiteDatabase db;
        private BackgroundWorker bw1 = new BackgroundWorker();
        private int isWhat = 0;
        private Boolean hasError = false;
        private string errorString = "";
        private string selectedList = "";
        private Boolean isCancelled = false;
        private int count = 0;
        private int count2 = 0;
        private string FN = "";
        public frmMailingList()
        {
            InitializeComponent();
            bw1.WorkerReportsProgress = true;
            bw1.WorkerSupportsCancellation = true;
            bw1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            bw1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }

        
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Microsoft.Office.Interop.Excel.ApplicationClass app = new ApplicationClass();
            
            
            Microsoft.Office.Interop.Excel.Workbook workBook = app.Workbooks.Open(FN, 0, true, 5,
                                                         "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows,
                                                         "\t", false, false, 0, true, 1, 0);

            Microsoft.Office.Interop.Excel.Worksheet workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
            
            int nInLastRow = workSheet.Cells.Find("*", System.Reflection.Missing.Value,
                                                       System.Reflection.Missing.Value, System.Reflection.Missing.Value, Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;


            int index = 0;
            float ptst = 0;

            object rowIndex = 1;
            object colIndex1 = 1;
            object colIndex2 = 2;
            object colIndex3 = 3;

            int total = nInLastRow;
            
            //try
            //{

            
            while (((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, colIndex1]).Value2 != null)
            {
                rowIndex = 2 + index;            

                try
                {

                    string firstName = ((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, colIndex1]).Value2.ToString();
                    string email = ((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, colIndex2]).Value2.ToString();
                    string company = "";
                    string bounce = "NO";          
                    //Thread.Sleep(1);

                    db = new SQLiteDatabase();
                    Dictionary<String, String> data = new Dictionary<String, String>();
                    data.Add("name", firstName);
                    data.Add("email", email);
                    data.Add("companyName", company);
                    data.Add("groupName", selectedList);
                    data.Add("bounce", bounce);
                    
                    db.InsertNoError("tblEmail", data);
                    count2++;
                }
                catch (Exception ex)
                {
                    hasError = true;
                    errorString = ex.Message;
                }
                index++;
                count++;

                ptst = count * 100;
                ptst /= total;

                int val = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(ptst)));
                bw1.ReportProgress(val);

                if (isCancelled)
                {
                    e.Cancel = true;                    
                    break;
                }
                if ((bw1.CancellationPending == true))
                {
                    e.Cancel = true;
                    
                    break;
                }
            }



        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;            
            txtStatus.Text = "Status : " + e.ProgressPercentage + "%";
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //toggleButtons(true);
            progressBar1.Value = 0;
            progressBar1.Visible = false;
            loadGrid(txtList.SelectedItem.ToString());
            if (isCancelled) txtStatus.Text = "Status : Transfer Aborted";
            
            btnImport.Visible = true;
            button1.Visible = false;
            btnNewAccount.Enabled = true;
            btnDeleteGroup.Enabled = true;
            btnDelete.Enabled = true;
            btnSaveAccount.Enabled = true;
            txtList.Enabled = true;
            btnAddNewMail.Enabled = true;
            txtStatus.Text = "Status : Transfer Finished, " + count2 + " emails added.";
            btnBrowseFile.Enabled = true;


            if (hasError) MessageBox.Show("Error occured:\r\n" + errorString);
        }


        void toggleButtons(Boolean BT)
        {
            txtList.Enabled = BT;
            btnNewAccount.Enabled = BT;
            panelMain.Enabled = !BT;
        }

        protected override void OnFormClosing(FormClosingEventArgs e) {
            if (bw1.IsBusy)
            {
                
                if (MessageBox.Show("Are you sure you want to close this form? Current email transfer will be aborted once you close this form. Click NO to cancel", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.No)
                {
                    e.Cancel = true;                    
                }
                else
                {
                    bw1.CancelAsync();
                    isCancelled = true;
                    bw1.CancelAsync();
                }

            }
}
        private void frmMailingList_Load(object sender, EventArgs e)
        {
            toggleButtons(true);
            loadAccounts();
            progressBar1.Visible = false;
            button1.Visible = false;
        }

        private void btnNewAccount_Click(object sender, EventArgs e)
        {
            toggleButtons(false);
            txtGroupName.Text = "";
            txtGroupName.Focus();
            btnDeleteGroup.Enabled = false;
            btnBrowseFile.Enabled = false;
            btnDelete.Enabled = false;
            btnImport.Enabled = false;
            btnAddNewMail.Enabled = false;
            isWhat = 0;     
        }

        void loadAccounts()
        {
            int count = 0;
            txtList.Items.Clear();
            

            try
            {
                db = new SQLiteDatabase();
                System.Data.DataTable recipe;
                String query = "select * from tblGroup";
                recipe = db.GetDataTable(query);
               

                foreach (DataRow r in recipe.Rows)
                {
                    count += 1;
                    txtList.Items.Add(r["groupname"].ToString());
                }


            }
            catch (Exception fail)
            {
                String error = "The following error has occurred:\n\n";
                error += fail.Message.ToString() + "\n\n";
                MessageBox.Show(error);
                this.Close();
            }
            if (count == 0)
            {
                txtList.Enabled = false;
                panelMain.Enabled = false;
            }
        }

        void updateEmails(string groupName)
        {
            db = new SQLiteDatabase();
            Dictionary<String, String> data = new Dictionary<String, String>();
            data.Add("groupName", groupName);
            db.Update("tblEmail", data, String.Format("tblEmail.groupName = '{0}'", this.txtList.SelectedItem.ToString())); 

        }
        private void btnSaveAccount_Click(object sender, EventArgs e)
        {
            string err = "";

            if (txtGroupName.Text == "") err += "Provide a valid Group Name.\r\n";
            
            if (err != "")
            {
                MessageBox.Show(err, "Error Found.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                db = new SQLiteDatabase();
                Dictionary<String, String> data = new Dictionary<String, String>();
                data.Add("groupName", txtGroupName.Text);
                
                try
                {
                    if (isWhat == 0)
                    {
                        db.Insert("tblGroup", data);
                        MessageBox.Show("New Mail Group Account created!", "Success.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        isWhat = 1;
                    }
                    else
                    {
                        updateEmails(txtGroupName.Text);
                        db.Update("tblGroup", data, String.Format("tblGroup.groupName = '{0}'", this.txtList.SelectedItem.ToString()));                        
                        MessageBox.Show("Group updated succesfully!", "Success.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        isWhat = 1;
                    }

                    
                    loadAccounts();
                    txtGroupName.Focus();
                    btnDeleteGroup.Enabled = true;
                    btnBrowseFile.Enabled = true;
                    btnDelete.Enabled = true;
                    btnImport.Enabled = true;
                    btnNewAccount.Enabled = true;
                    txtList.Enabled = true;
                    btnAddNewMail.Enabled = true;
                    txtList.SelectedItem = txtGroupName.Text;

                    speedyMailMain form1 = (speedyMailMain)System.Windows.Forms.Application.OpenForms["MDIParent1"];
                    form1.loadEmailGroup();
                }
                catch (Exception crap)
                {
                    MessageBox.Show(crap.Message);
                }

            }
        }

        void loadValues(string titolo)
        {
            int count = 0;


            try
            {
                db = new SQLiteDatabase();
                System.Data.DataTable recipe;
                String query = "select * from tblGroup where groupName = '" + titolo + "'";
                recipe = db.GetDataTable(query);
                // The results can be directly applied to a DataGridView control

                foreach (DataRow r in recipe.Rows)
                {
                    count += 1;
                    txtGroupName.Text = r["groupName"].ToString();                    
                }


            }
            catch (Exception fail)
            {
                String error = "The following error has occurred:\n\n";
                error += fail.Message.ToString() + "\n\n";
                MessageBox.Show(error);
                this.Close();
            }
            if (count == 0)
            {
                txtList.Enabled = false;
                
            }
        }

        void loadGrid(string titolo)
        {
            try
            {
                db = new SQLiteDatabase();
                System.Data.DataTable mailz;
                String query = "select Name \"Name\" , Email \"E-Mail\", CompanyName \"Company Name\" from tblEmail where groupName = '" + titolo + "'";
                mailz = db.GetDataTable(query);

                dataGridView1.DataSource = mailz;
                
                /*
                // Or looped through for some other reason
                foreach (DataRow r in recipe.Rows)
                {
                    MessageBox.Show(r["Name"].ToString());
                    MessageBox.Show(r["Description"].ToString());
                    MessageBox.Show(r["Prep Time"].ToString());
                    MessageBox.Show(r["Cooking Time"].ToString());
                }
	
                */
                dataGridView1.Columns[0].Width = 120;
                dataGridView1.Columns[1].Width = 120;
                dataGridView1.Columns[0].Width = 120;
                try
                {
                    int xxx = dataGridView1.RowCount - 1;
                    txtTotal.Text = "Total Records : " + xxx.ToString();
                }
                catch { }
            }
            catch (Exception fail)
            {
                String error = "The following error has occurred:\n\n";
                error += fail.Message.ToString() + "\n\n";
                MessageBox.Show(error);
                this.Close();
            }

        }

        private void txtList_SelectedIndexChanged(object sender, EventArgs e)
        {
            isWhat = 1;
            toggleButtons(false);
            txtList.Enabled = true;
            btnNewAccount.Enabled = true;
            loadValues(txtList.SelectedItem.ToString());
            loadGrid(txtList.SelectedItem.ToString());
        }

        private void btnBrowseFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select Excel File";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "All files (*.*)|*.*|Excel files (*.xls)|*.xls";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txtFile.Text = fdlg.FileName;                
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            FN = txtFile.Text;
            if (txtFile.Text != "" && txtFile.Text.Contains("xls"))
            {
                if (bw1.IsBusy != true)
                {
                    selectedList = txtList.SelectedItem.ToString();
                    progressBar1.Visible = true;
                    bw1.RunWorkerAsync();
                    btnImport.Visible = false;
                    button1.Visible = true;
                    btnNewAccount.Enabled = false;
                    btnDeleteGroup.Enabled = false;
                    btnDelete.Enabled = false;
                    btnBrowseFile.Enabled = false;
                    btnSaveAccount.Enabled = false;
                    btnAddNewMail.Enabled = false;
                    txtList.Enabled = false;
                }
            }
            else
            {
                MessageBox.Show("Please select excel file", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void panelMain_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if (bw1.IsBusy)
            {
                
                if (MessageBox.Show("Are you sure you want to cancel transfer? Current email transfer will be aborted. Click NO to cancel", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation) == System.Windows.Forms.DialogResult.No)
                {
                    isCancelled = false;
                }
                else
                {
                    isCancelled = true;
                    bw1.CancelAsync();
                    
                }

            }
        }

        private void btnCloseForm_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
                      
        }

        private void dataGridView1_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {            
            int i = dataGridView1.SelectedCells[0].RowIndex;
            txtTemp.Text = dataGridView1.Rows[i].Cells[1].Value.ToString(); 
        }

        private void btnAddNewMail_Click(object sender, EventArgs e)
        {

            string err = "";

            if (txtName.Text == "") err += "Provide a valid Name.\r\n";
            if (txtEmail.Text == "") err += "Provide a valid Email Account.\r\n";
            if (txtCompany.Text == "") err += "Provide a company Name.\r\n";

            if (err != "")
            {
                MessageBox.Show(err, "Error Found.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                db = new SQLiteDatabase();
                Dictionary<String, String> data = new Dictionary<String, String>();
                data.Add("name", txtName.Text);
                data.Add("email", txtEmail.Text);
                data.Add("companyName", txtCompany.Text);
                data.Add("groupName", txtList.SelectedItem.ToString());
                db.Insert("tblEmail", data);
                MessageBox.Show("New Email added!", "Success.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                loadGrid(txtList.SelectedItem.ToString());
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (txtTemp.Text != "")
            {
                if (MessageBox.Show("Are you sure you want to delete the selected account (" + txtTemp.Text + ") ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    db = new SQLiteDatabase();
                    db.Delete("tblEmail", String.Format("email = '{0}'", txtTemp.Text));
                    loadGrid(txtList.SelectedItem.ToString());
                }
            }
        }

        private void btnDeleteGroup_Click(object sender, EventArgs e)
        {
            if (txtList.SelectedItem.ToString() != "")
            {
                if (MessageBox.Show("Are you sure you want to delete the selected account (" + txtList.Text + ") ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
                {
                    db = new SQLiteDatabase();
                    db.Delete("tblEmail", String.Format("groupName = '{0}'", txtList.SelectedItem.ToString()));
                    loadGrid(txtList.SelectedItem.ToString());
                }
            }
        }
    }
        
}
