using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using Microsoft.VisualStudio.CommandBars;

using Extensibility;
using EnvDTE;
using EnvDTE80;

using TFS = Microsoft.TeamFoundation.WorkItemTracking.Client;


namespace VSTSTimeTrackerAddin
{
    public partial class VisualUserControl : UserControl
    {
        #region Variables
        private DTE2 mDte;			// Reference to the Visual Studio DTE object

        private bool auth = false;
        private DateTime dtStartTime;

        private TeamFoundationWrapper tfw = new TeamFoundationWrapper();

        #endregion
        
        #region Properties
        public DTE2 DTE
        {
            get { return mDte; }
            set { mDte = value; }
        }

        //public TeamFoundationWrapper Tfw
        //{
        //    get { return tfw; }
        //}
        #endregion

        #region Constructor

        public VisualUserControl()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (btnConnect.Text == "Ligar")
            {
                ConnectToServer();
            }
            else
            {
                DisconnectFromServer();
            }
        }
        private void btnTimeCtrl_Click(object sender, EventArgs e)
        {
            if (!tmrTimeCounter.Enabled)
            {
                // Começar a contar os tempos...
                dtStartTime = DateTime.Now;

                // caso já tenhamos um tempo parcial. Fazer a contabilização com o mesmo
                if (txtTime.Text.Length > 0)
                {
                    dtStartTime = TimeUtils.GetStartDateTimeFromText (txtTime.Text, dtStartTime);
                }

                btnTimeCtrl.Text = "Parar";
                txtTime.Enabled = false;
                tmrTimeCounter.Enabled = true;
                btnSave.Enabled = false;
            }
            else
            {
                // clicado o botão parar
                btnTimeCtrl.Text = "Contar";
                txtTime.Enabled = true;
                tmrTimeCounter.Enabled = false;
                btnSave.Enabled = true;
            }
        }

        private void tmrTimeCounter_Tick(object sender, EventArgs e)
        {
            DateTime temp = DateTime.Now;
            //  temp - dtStartTime
            TimeSpan ts = temp - dtStartTime;

            double aux = 0;
            double h = ts.TotalHours;
            int hours = (int)Math.Truncate(h);

            aux = (h - hours) * 60; // obter minutos
            int mins = (int)Math.Truncate(aux);

            aux = (aux - mins) * 60; // obter segundos
            int secs = (int)Math.Floor(aux);

            string timeStr = hours.ToString("00") + ":" + mins.ToString("00") + ":" + secs.ToString("00");

            txtTime.Text = timeStr;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            TimeSpan tsTime = TimeSpan.Zero;
            // 1. validar que o tempo introduzido é válido...
            try
            {
                tsTime = TimeUtils.GetTimeSpanFromTimeString(txtTime.Text);
            }
            catch (Exception)
            {
                MessageBox.Show("Deverá introduzir um tempo válido, no formato hh:mm:ss.");
                return;
            }

            // 2. Armazenar o tempo em base de dados
            TFS::Project tProj = (TFS::Project)ddlProject.SelectedItem;
            TFS::WorkItem wItem = (TFS::WorkItem)ddlItem.SelectedItem;

            bool res = tfw.StoreProjWorkItemCompletedWorkTime(tProj.Name, wItem.Id.ToString(), tsTime);

            if (res)
            {
                MessageBox.Show("Tempo armazenado no TFS!");
            }
            else
            {
                MessageBox.Show("Ocorreu um erro ao armazenar o tempo no TFS!");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //DTE.AddIns.Item(null).clo
            //this.Hide();
        }

        private void ddlProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!auth)
                return;

            TFS::Project tProj = (TFS::Project)ddlProject.SelectedItem;
            List<TFS::WorkItem> iList = tfw.GetProjWorkItems(tProj.Name);

            ddlItem.DataSource = iList;
            ddlItem.DisplayMember = "Title";
            ddlItem.ValueMember = "Id";
            ddlItem.SelectedIndex = 0; // vai chamar o próximo evento...
        }

        private void ddlItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!auth)
                return;

            // Quando o WorkItem muda vamos obter o seu tempo armazenado...

            TFS::Project tProj = (TFS::Project)ddlProject.SelectedItem;
            TFS::WorkItem wItem = (TFS::WorkItem)ddlItem.SelectedItem;

            string decTimeStr = tfw.GetProjWorkItemCompletedWorkTime(tProj.Name, wItem.Id.ToString());
            if (decTimeStr == null)
                decTimeStr = "0";
            TimeSpan tsTime = TimeUtils.GetTimeSpanFromDecimal(Decimal.Parse(decTimeStr));
            txtTime.Text = TimeUtils.GetTimeStringFromTimeSpan(tsTime);
        }

        #endregion



        #region Event Functions


        private void ConnectToServer()
        {
            if (txtServidor.Text.Length==0)
            {
                MessageBox.Show("Deverá indicar o nome do servidor!");
                txtServidor.Focus();
                return;
            }

            // Mostrar Form de Logon
            Logon frmLog = new Logon();

            if (frmLog.ShowDialog() == DialogResult.OK)
            {
                auth = tfw.LogOn( txtServidor.Text, 
                    new System.Net.NetworkCredential(frmLog.Username, frmLog.Password));

                if ( !auth )
                {
                    MessageBox.Show("Não foi possível autenticar no servidor TFS!");
                    txtServidor.Focus();
                    frmLog.Dispose();
                    return;
                }

                // Estamos autenticados!

                // desactivar possibilidade de alterar nome servidor
                txtServidor.Enabled = false;
                btnConnect.Text = "Desligar";
                
                // obter informação do servidor TFS e activar os controlos
                GetAllTfsInfo();
            }
            else
            {
                // utilizador clicou cancel
                frmLog.Dispose();                
            }
        }

        private void DisconnectFromServer()
        { 
            // deixamos de estar autenticados
            tfw.LogOff();
            auth = false;            

            // Cleanup all server info and disable issue tracking form controls
            ddlProject.Enabled = false;            
            ddlItem.Enabled = false;
            txtTime.Enabled = false;
            btnTimeCtrl.Enabled = false;
            btnSave.Enabled = false;

            txtTime.Text = "";
            ddlProject.DataSource = null;
            ddlItem.DataSource = null;

            txtServidor.Enabled = true;
            btnConnect.Text = "Ligar";
        }

        private void GetAllTfsInfo()
        { 
            // obter a lista de projectos
            List<TFS::Project> pList = tfw.GetProjects();

            ddlProject.DataSource = pList;
            ddlProject.DisplayMember = "Name";
            ddlProject.ValueMember = "ID";
            ddlProject.SelectedIndex = 0; // irá despoletar o evento de actualização do ddlitem

            EnableProjTimeCtrls(true);

            btnSave.Enabled = true;
                       
        }

        private void EnableProjTimeCtrls(bool enable)
        {
            ddlProject.Enabled = enable;
            ddlItem.Enabled = enable;
            txtTime.Enabled = enable;
            btnTimeCtrl.Enabled = enable;
        }


        #endregion






    }
}
