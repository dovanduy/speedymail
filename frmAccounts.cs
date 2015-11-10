using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;

namespace SpeedyMail
{
    
    public partial class frmAccounts : Form
    {        

        SQLiteDatabase db;
        private int isWhat = 0;


        public frmAccounts()
        {
     
            InitializeComponent();            
        }

        void toggleButtons(Boolean BT)
        {
            txtAccountName.Enabled = BT;
            txtEmail.Enabled = BT;
            txtFullName.Enabled = BT;
            txtPassword.Enabled = BT;
            txtSMTPServer.Enabled = BT;
            btnSave.Enabled = BT;
            btnCancel.Visible = BT;


            btnNew.Enabled = !BT;
            btnEdit.Enabled = !BT;
            btnDelete.Enabled = !BT;

        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            toggleButtons(true);
            txtAccountName.Focus();
            isWhat = 1;
        }

        void loadAccounts()
        {
            int count = 0;
            listBox1.Items.Clear();

            try
            {
                db = new SQLiteDatabase();
                DataTable recipe;
                String query = "select title from tblEmailAccount";
                recipe = db.GetDataTable(query);
                // The results can be directly applied to a DataGridView control

                foreach (DataRow r in recipe.Rows)
                {
                    count += 1;
                    listBox1.Items.Add(r["title"].ToString());
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
                listBox1.Enabled = false;
                btnDelete.Enabled = false;
                btnEdit.Enabled = false;
            }
        }

        private void frmAccounts_Load(object sender, EventArgs e)
        {
            toggleButtons(false);

            loadAccounts();

            btnEdit.Enabled = false;
            btnDelete.Enabled = false;

            
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            

            isWhat = 0;
            txtAccountName.Text = "";
            txtEmail.Text = "";
            txtFullName.Text = "";
            txtPassword.Text = "";
            txtSMTPServer.Text = "";
            toggleButtons(true);
            txtAccountName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string err = "";

            if (txtAccountName.Text == "") err += "Provide a valid Account Name.\r\n";
            if (txtEmail.Text == "") err += "Provide a valid Email Account.\r\n";
            if (txtFullName.Text == "") err += "Provide a valid Name.\r\n";
            if (txtPassword.Text == "") err += "Provide a valid Account Password.\r\n";
            if (txtSMTPServer.Text == "") err += "Provide a valid SMTP Server.\r\n";

            if (err != "")
            {
                MessageBox.Show(err, "Error Found.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }
            else
            {
                db = new SQLiteDatabase();
                Dictionary<String, String> data = new Dictionary<String, String>();
                data.Add("title", txtAccountName.Text);
                data.Add("smtp", txtSMTPServer.Text);
                data.Add("username", txtEmail.Text);
                data.Add("password", txtPassword.Text);
                data.Add("fullname", txtFullName.Text);
                try
                {
                    if (isWhat == 0)
                    {
                        db.Insert("tblEmailAccount", data);
                        MessageBox.Show("New Email SMTP Account created!", "Success.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    else
                    {
                        db.Update("tblEmailAccount", data, String.Format("tblEmailAccount.title = '{0}'", this.txtTitle.Text));                      
                        MessageBox.Show("Account updated succesfully!", "Success.", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                    
                    toggleButtons(false);
                    loadAccounts();
                    btnEdit.Enabled = false;
                    btnDelete.Enabled = false;
                    listBox1.Enabled = true;

                    speedyMailMain form1 = (speedyMailMain)Application.OpenForms["MDIParent1"];                    
                    form1.loadAccounts();
                }
                catch (Exception crap)
                {
                    MessageBox.Show(crap.Message);
                }

            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

            txtAccountName.Text = "";
            txtEmail.Text = "";
            txtFullName.Text = "";
            txtPassword.Text = "";
            txtSMTPServer.Text = "";
            toggleButtons(false);
            loadAccounts();
            btnDelete.Enabled = false;
            btnEdit.Enabled = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            loadValues(listBox1.SelectedItem.ToString());
        }

        void loadValues(string titolo)
        {
            int count = 0;
            

            try
            {
                db = new SQLiteDatabase();
                DataTable recipe;
                String query = "select * from tblEmailAccount where title = '" + titolo + "'";
                recipe = db.GetDataTable(query);
                // The results can be directly applied to a DataGridView control

                foreach (DataRow r in recipe.Rows)
                {
                    count += 1;
                    txtTitle.Text = r["title"].ToString();
                    txtAccountName.Text = r["title"].ToString();
                    txtEmail.Text = r["username"].ToString();
                    txtFullName.Text = r["fullname"].ToString();
                    txtPassword.Text = r["password"].ToString();
                    txtSMTPServer.Text = r["smtp"].ToString();
                    btnDelete.Enabled = true;
                    btnEdit.Enabled = true;
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
                listBox1.Enabled = false;
                btnDelete.Enabled = false;
                btnEdit.Enabled = false;
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete the selected account?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.Yes)
            {
                db = new SQLiteDatabase();
                db.Delete("tblEmailAccount", String.Format("title = '{0}'", txtTitle.Text));
                loadAccounts();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }



    }

    
}
