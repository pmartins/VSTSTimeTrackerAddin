using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace VSTSTimeTrackerAddin
{
    public partial class Logon : Form
    {
        public Logon()
        {
            InitializeComponent();
        }

        private string username = null;
        private string password = null;

        public string Username
        {
            get
            {
                return username;
            }
        }
        public string Password
        {
            get
            {
                return password;
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            username = txtUsername.Text;
            password = txtPassword.Text;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }
    }
}