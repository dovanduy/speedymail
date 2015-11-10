using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using Microsoft.Office.Interop.Excel;
using System.Net.Mail;
using System.Diagnostics;

using System.Configuration;
using System.IO;

using mshtml;
using Microsoft.ConsultingServices.HtmlEditor;

namespace SpeedyMail
{
    public partial class speedyMailMain : Form
    {
        //Variable Def
        #region Variables
            private int option = 0;
            private BackgroundWorker bw1 = new BackgroundWorker();
            private string PTS="";
            private int tt;
            private mshtml.HTMLDocumentClass doc;
            private string totList="";
            private string mymessage = "";
            private string smtpTitle = "";
            private int totz = 0;
            private string globalSubject = "";
            private string fileName = "";
            private string SupersmtpServer = "";
            private string fromAddress = "";
            private string fromName = "";
            private string credentialpassword = "";

            SQLiteDatabase db;
        #endregion


        public string Mid(string src, Int32 start, Int32 length)
        {
            Int32 ctr;
            string tmp = "";
            Int32 ln = 0;
            Int32 st = 0;
            if (start < 0) { st = 0; } else { st = start; }

            if (st >= 0)
            {
                if (length >= src.Length) { ln = src.Length; }
                else { ln = length; }

                for (ctr = 0; ctr <= src.Length; ctr++)
                {
                    if (ctr >= (st - 1) && ctr <= (ln - 1))
                    {
                        tmp = tmp + src.Substring(ctr, 1);
                    }
                }

            }

            return tmp;
        }
        
        public speedyMailMain()
        {
            InitializeComponent();
            bw1.WorkerReportsProgress = true;
            bw1.WorkerSupportsCancellation = true;
            bw1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            bw1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            bw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);
        }        
        string returnImage(string pt)
        {
            string tempString = txtStatus.Text + @"\" + txtAddressList.Text;

            //if (pt.Length == 1) tempString += "000";
            //if (pt.Length == 2) tempString += "00";
            //if (pt.Length == 3) tempString += "0";

            return tempString + pt + ".jpg";
        }

        private Boolean SendMail(string mailTo, string myMessage, string mySubject, string fromWhom, string attachments)
        {

            SmtpClient o_Client = new SmtpClient(SupersmtpServer);
            MailAddress o_FromAddress = new MailAddress(fromAddress, fromName);
            MailAddress o_ToAddress = new MailAddress(mailTo);
            MailMessage o_Message = new MailMessage(o_FromAddress, o_ToAddress);


            o_Client.Credentials = new System.Net.NetworkCredential(fromAddress, credentialpassword);
            //o_Client.DeliveryMethod = SmtpDeliveryMethod.Network;

            if (!string.IsNullOrEmpty(attachments))
            {
                Attachment att = new Attachment(attachments);
                o_Message.Attachments.Add(att);
            }
            o_Message.Subject = mySubject;
            o_Message.Body = myMessage;
            o_Message.IsBodyHtml = true;


            try
            {
                //System.Threading.Thread.Sleep(100);
                o_Client.Send(o_Message);
                return true;
            }
            catch
            {
                return false;
            }

            

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {

            Int32 overallTotal;

            string tps = "";
            int cnt = 0;

            
            overallTotal = cnt + totz;
            int index = 0;
            int total = overallTotal;
            int count = 0;
            float ptst = 0;
            tt = 0;

            #region ExcelImport ---
                if (fileName != "" && fileName.Contains(".xls"))
                {
                    Microsoft.Office.Interop.Excel.ApplicationClass app = new ApplicationClass();
                    Microsoft.Office.Interop.Excel.Workbook workBook = app.Workbooks.Open(fileName, 0, true, 5,
                                                                 "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows,
                                                                 "\t", false, false, 0, true, 1, 0);
                    Microsoft.Office.Interop.Excel.Worksheet workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                    int nInLastRow = workSheet.Cells.Find("*", System.Reflection.Missing.Value,
                                                           System.Reflection.Missing.Value, System.Reflection.Missing.Value, Microsoft.Office.Interop.Excel.XlSearchOrder.xlByRows, Microsoft.Office.Interop.Excel.XlSearchDirection.xlPrevious, false, System.Reflection.Missing.Value, System.Reflection.Missing.Value).Row;

                    

                    object rowIndex = 1;
                    object colIndex1 = 1;
                    object colIndex2 = 2;

                    overallTotal += nInLastRow;

                    total = overallTotal;


                    string newtext = "";
    
                        string message =mymessage;
                        while (((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, colIndex1]).Value2 != null)
                        {
                            rowIndex = 2 + index;
                            PTS = "";

                            try
                            {

                                string firstName = ((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, colIndex1]).Value2.ToString();
                                string email = ((Microsoft.Office.Interop.Excel.Range)workSheet.Cells[rowIndex, colIndex2]).Value2.ToString();

                                if (SendMail(email, message, globalSubject, fromName, ""))
                                {
                                    newtext = firstName + ", " + email + " --> Email Sent!\r\n";                                    
                                    tt += 1;
                                }
                                else
                                {
                                    newtext = firstName + ", " + email + " --> Email Failed!\r\n";
                                    listBox1.Items.Add(email);
                                }

                            }
                            catch (Exception ex)
                            {
                                //PTS += @"\r\nError Occured : Index " + index.ToString() + " --> " + ex.Message.ToString() + @"\r\n";
                            }
                            index++;
                            count++;

                            ptst = count * 100;
                            ptst /= total;

                            int val = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(ptst)));
                            bw1.ReportProgress(val, newtext);
                        }


                }
            #endregion

            #region EmailList

                if (totz > 0)
                {
                    //try
                    //{
                        db = new SQLiteDatabase();
                        System.Data.DataTable recipe;
                        String query = "select * from tblemail where groupname = '" + smtpTitle + "'";
                        recipe = db.GetDataTable(query);

                        total = totz;
                        count = 0;                        
                        index = 0;
                        ptst = 0;

                        string newText = "";

                        foreach (DataRow r in recipe.Rows)
                        {
                            PTS = "";
                            string em = "", nm = "";
                            em = r["email"].ToString();
                            nm = r["name"].ToString();

                            try
                            {

                                if (SendMail(r["email"].ToString(), mymessage, globalSubject, fromName, ""))
                                {                                    
                                   newText = r["name"].ToString() + ", " + r["email"].ToString() + " --> Email Sent!\r\n";                                    
                                }
                                else
                                {
                                    newText = r["name"].ToString() + ", " + r["email"].ToString() + " --> Email Failed!\r\n";
                                    listBox1.Items.Add(r["email"].ToString());
                                }

                            }
                            
                            catch (Exception ex)
                            {
                               // newText += @"\r\nError Occured : Index " + index.ToString() + " --> " + ex.Message.ToString() + @"\r\n";
                                newText = "";
                            }

                            tt += 1;
                            index++;
                            count++;

                            ptst = count * 100;
                            ptst /= total;

                            PTS = em + "\r\n";

                            int val = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(ptst)));
                            bw1.ReportProgress(val, newText);

                            
                        }


                    //}
                    //catch (Exception fail)
                    //{
                    //    PTS += @"\r\n" + fail.Message.ToString() + "\r\n";

                    //}
                }                

            #endregion

            #region ManualEnter

                if (totList != "")
                {
                    try
                    {
                        
                        tps = totList;
                        
                        string[] words = tps.Split(',');

                        total = words.Length;
                        count = 0;
                        string newtext = "";

                        foreach (string word in words)
                        {
                        
                            if (SendMail(word.Trim(), mymessage, globalSubject, fromName, ""))
                            {
                                newtext = word.Trim() + ", " + word.Trim() + " --> Email Sent!\r\n";
                                tt += 1;
                            }
                            else
                            {
                                newtext = word.Trim() + ", " + word.Trim() + " --> Email Failed!\r\n";
                                listBox1.Items.Add(word.Trim());

                            }


                            index++;
                            count++;

                            ptst = count * 100;
                            ptst /= total;

                            int val = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(ptst)));
                            bw1.ReportProgress(val, newtext);
                        }
                    }
                    catch (Exception op)
                    {
                        PTS += @"\r\n" + op.Message + @"\r\n";
                    }
                }
                
            #endregion

        }
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            txtLog.Text += (string)e.UserState;
            //label2.Text = "Status : " + e.ProgressPercentage + "%";
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            toggleButtons(true);
            progressBar1.Value = 0;
            txtStatus.Text = "Total : " + tt;
            progressBar1.Visible = false;
            btnSaveLog.Visible = true;
            label2.Text = "Success!";

            
            //db = new SQLiteDatabase();
            //foreach (string item in listBox1.Items)
            //{
            //    db.ExecuteNonQuery("update tblemail set bounce='YES' where email = '" + item + "'");
            //}
        }
        
        
        void toggleButtons(Boolean BT)
        {
            button1.Enabled = BT;
            btnSend.Enabled = BT;
            btnQuitApp.Enabled = BT;
            textBox1.Enabled = BT;
            txtAddressList.Enabled = BT;
            htmlEditorControl.Enabled = BT;
            txtSubject.Enabled = BT;
            btnStartCampaign.Enabled = BT;
            btnLoadTemplate.Enabled = BT;
            btnShowMailingList.Enabled = BT;
            btnApplicationSettings.Enabled = BT;
            //txtLog.Enabled = BT;
        }
        void addLog(string message)
        {
            txtLog.Text += "[" + DateTime.Now.ToString() + "] ...// " + message + "\r\n";
        }
        
        public void loadAccounts()
        {
            
            int count = 0;
            txtAccountList.Items.Clear();
            txtAccountList.Text = "";
            try
            {
                db = new SQLiteDatabase();
                System.Data.DataTable recipe;
                String query = "select title from tblEmailAccount";
                recipe = db.GetDataTable(query);
                // The results can be directly applied to a DataGridView control

                foreach (DataRow r in recipe.Rows)
                {
                    count += 1;
                    txtAccountList.Items.Add(r["title"].ToString());
                    label2.Text = "Please provide a valid Mail Server";
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
                label2.Text = "No Server account found.";
                btnSend.Enabled = false;
                txtAccountList.Enabled = false;
            }
            else
            {
                label2.Text = "Ready";
                btnSend.Enabled = true;
                txtAccountList.Enabled = true;
            }
        }
        public void loadEmailGroup()
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
            }
            else
            {
                txtList.Enabled = true;
            }
        }
        
        private void button4_Click(object sender, EventArgs e)
        {
            doc = null;
            tableMain.Visible = true;
            txtLog.Text = "[" + DateTime.Now.ToString() + "] ....// Starting new campaign.\r\n";
            progressBar1.Visible = false;
            htmlEditorControl.BodyHtml = "";
            btnSend.Enabled = true;
            loadAccounts();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Select DB";
            fdlg.InitialDirectory = @"c:\";
            fdlg.Filter = "All files (*.*)|*.*|Excel files (*.xls)|*.xls";
            fdlg.FilterIndex = 2;
            fdlg.RestoreDirectory = true;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = fdlg.FileName;
                txtStatus.Text = System.IO.Path.GetDirectoryName(fdlg.FileName);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (txtSubject.Text == "")
            {
                MessageBox.Show("Please provide subject");
                return;
            }
            if (htmlEditorControl.BodyHtml == "")
            {
                MessageBox.Show("Please provide a valid HTML entry");
                return;
            }

            if (textBox1.Text != "" || txtAddressList.Text != "" || txtList.Text != "")
            {

                toggleButtons(false);

                try
                {
                    smtpTitle = txtList.SelectedItem.ToString();
                }
                catch
                { }
                totList = txtAddressList.Text;
                mymessage = htmlEditorControl.BodyHtml.ToString();
                globalSubject = txtSubject.Text;
                if (bw1.IsBusy != true)
                {
                    progressBar1.Visible = true;
                    bw1.RunWorkerAsync();
                }
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            this.Dispose();
            System.Windows.Forms.Application.Exit();

        }

        private void MDIParent1_Load(object sender, EventArgs e)
        {
            IsMdiContainer = true;
            btnSaveLog.Visible = false;
            tableMain.Visible = false;
            progressBar1.Visible = false;
            loadEmailGroup();
            loadAccounts();
            linkLabel1.Links.Add(0, 8, "www.tischools.cc/email_ImportTemplate.xls");
   
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.OpenDefaultFilePrompt();
            this.htmlEditorControl.Focus();
            tableMain.Visible = true;
            txtLog.Text = "";
            addLog("Starting new campaign");
            addLog("Loaded template");
            btnSend.Enabled = true;
            loadAccounts();
        }

        private void bEditHTML_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.HtmlContentsEdit();
            this.htmlEditorControl.Focus();
        }

        private void bViewHtml_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.HtmlContentsView();
            this.htmlEditorControl.Focus();
        }

        private void bBackground_Click(object sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Color color = dialog.Color;
                    this.htmlEditorControl.BodyBackColor = color;
                }
            }
            this.htmlEditorControl.Focus();
        }

        private void bForeground_Click(object sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Color color = dialog.Color;
                    this.htmlEditorControl.BodyForeColor = color;
                }
            }
            this.htmlEditorControl.Focus();
        }

        private void bHeading_Click(object sender, EventArgs e)
        {
            int headingRef = this.listHeadings.SelectedIndex + 1;
            if (headingRef > 0)
            {
                HtmlHeadingType headingType = (HtmlHeadingType)headingRef;
                this.htmlEditorControl.InsertHeading(headingType);
            }
            this.htmlEditorControl.Focus();
        }

        private void bFormatted_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.InsertFormattedBlock();
        }

        private void bNormal_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.InsertNormalBlock();
        }

        private void bSaveHtml_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.SaveFilePrompt();
            this.htmlEditorControl.Focus();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.htmlEditorControl.OpenFilePrompt();
            this.htmlEditorControl.Focus();
            tableMain.Visible = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            frmAccounts frm = new frmAccounts();
            frm.ShowDialog();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmMailingList frm = new frmMailingList();            

            frm.ShowDialog();
            frm.Activate();
        }

        private void txtAccountList_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                db = new SQLiteDatabase();
                System.Data.DataTable recipe;
                String query = "select * from tblEmailAccount where title = '" + txtAccountList.SelectedItem.ToString() + "'";
                recipe = db.GetDataTable(query);


                foreach (DataRow r in recipe.Rows)
                {
                    credentialpassword = r["password"].ToString();
                    SupersmtpServer = r["smtp"].ToString();
                    fromAddress = r["username"].ToString();
                    fromName = r["fullname"].ToString();                    
                }

                btnSend.Enabled = true;
            }
            catch (Exception fail)
            {
                String error = "The following error has occurred:\n\n";
                error += fail.Message.ToString() + "\n\n";
                MessageBox.Show(error);
                this.Close();
                btnSend.Enabled = false;
            }
            

        }

        private void txtList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int count = 0;
            try
            {
                db = new SQLiteDatabase();
                System.Data.DataTable recipe;
                String query = "select count(*) as xxx from tblemail where groupname = '" + txtList.SelectedItem.ToString() + "'";
                recipe = db.GetDataTable(query);


                foreach (DataRow r in recipe.Rows)
                {
                    count += 1;
                    txtCount.Text = r["xxx"].ToString();
                }


            }
            catch (Exception fail)
            {
                String error = "The following error has occurred:\n\n";
                error += fail.Message.ToString() + "\n\n";
                MessageBox.Show(error);
                this.Close();
            }


            try
            {
                totz = Convert.ToInt32(txtCount.Text);
            }
            catch
            {
                totz = 0;
            }
        }

        private void btnDeleteGroup_Click(object sender, EventArgs e)
        {
            if (txtSubject.Text == "")
            {
                MessageBox.Show("Please provide subject");
                return;
            }
            if (htmlEditorControl.BodyHtml == "")
            {
                MessageBox.Show("Please provide a valid HTML entry");
                return;
            }
            option = 1;
            if (textBox1.Text != "")
            {
                toggleButtons(false);
                fileName = textBox1.Text;

                mymessage = htmlEditorControl.BodyHtml.ToString();
                globalSubject = txtSubject.Text;
                if (bw1.IsBusy != true)
                {
                    progressBar1.Visible = true;
                    bw1.RunWorkerAsync();
                }
            }
            else
            {
                MessageBox.Show("Please provide a valid excel file.");
                return;
            }

            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (txtSubject.Text == "")
            {
                MessageBox.Show("Please provide subject");
                return;
            }
            if (htmlEditorControl.BodyHtml == "")
            {
                MessageBox.Show("Please provide a valid HTML entry");
                return;
            }

            option = 2;
            if (txtList.Text != "")
            {
                toggleButtons(false);
                try
                {
                    smtpTitle = txtList.SelectedItem.ToString();
                }
                catch
                { }
                
                mymessage = htmlEditorControl.BodyHtml.ToString();
                globalSubject = txtSubject.Text;
                if (bw1.IsBusy != true)
                {
                    progressBar1.Visible = true;
                    bw1.RunWorkerAsync();
                }
            }
            else
            {
                MessageBox.Show("Please provide a valid mailing list.");
                return;
            }


            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (txtSubject.Text == "")
            {
                MessageBox.Show("Please provide subject");
                return;
            }
            if (htmlEditorControl.BodyHtml == "")
            {
                MessageBox.Show("Please provide a valid HTML entry");
                return;
            }

            option = 3;
            if (txtAddressList.Text != "")
            {
                toggleButtons(false);
                
                totList = txtAddressList.Text;
                mymessage = htmlEditorControl.BodyHtml.ToString();
                globalSubject = txtSubject.Text;
                
                if (bw1.IsBusy != true)
                {
                    progressBar1.Visible = true;
                    bw1.RunWorkerAsync();
                }
            }
            else
            {
                MessageBox.Show("Please provide a valid email(s).");
                return;
            }


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            if (tableMain.Visible == false)
            {
                MessageBox.Show("You did not create any email campaign. \r\nClick Start New Campaign button / Load Template \r\nbutton to start new campaign", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            try
            {
                   
                if (txtAccountList.SelectedItem.ToString() != "")
                {

                    if (txtSubject.Text == "")
                    {
                        MessageBox.Show("Please provide a valid subject", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                    if (htmlEditorControl.BodyHtml == "")
                    {
                        MessageBox.Show("Please provide a valid HTML body text", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }

                    if (textBox1.Text != "" || txtAddressList.Text != "" || txtList.Text != "")
                    {

                        toggleButtons(false);
                        fileName = textBox1.Text;

                        try
                        {
                            smtpTitle = txtList.SelectedItem.ToString();
                        }
                        catch
                        {
                            smtpTitle = "";
                        }

                        totList = txtAddressList.Text;
                        mymessage = htmlEditorControl.BodyHtml.ToString();
                        globalSubject = txtSubject.Text;
                        label2.Text = "Processing. Please wait";
                        if (bw1.IsBusy != true)
                        {
                            progressBar1.Visible = true;
                            bw1.RunWorkerAsync();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Hmmm? I guess you forgot to select mailing list or enter email targets. Please check target recipiets settings.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Please provide a valid email account.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            

            SaveFileDialog folderBrowserDialog3save = new SaveFileDialog();
            folderBrowserDialog3save.Filter = "Text File|*.txt";
            folderBrowserDialog3save.Title = "Save Log";
            

            folderBrowserDialog3save.DefaultExt = ".txt";
            if (folderBrowserDialog3save.ShowDialog() == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(folderBrowserDialog3save.FileName);
                TextWriter tw = new StreamWriter(folderBrowserDialog3save.FileName);
                // write a line of text to the file
                tw.WriteLine(txtLog.Text);
                // close the stream
                tw.Close();
                MessageBox.Show("Saved to " + folderBrowserDialog3save.FileName, "Saved Log File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void panel11_Paint(object sender, PaintEventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.Link.LinkData.ToString());
        }


    }
}
