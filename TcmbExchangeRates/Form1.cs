using Dapper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcmbExchangeRates
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
        }

        SqlConnection con = new SqlConnection();
        private void Form1_Load(object sender, EventArgs e)
        {

        }
        void SetControls()
        {
            if (con.State == ConnectionState.Open)
            {
                txtServer.Enabled = false;
                txtDatabase.Enabled = false;
                txtUser.Enabled = false;
                txtPass.Enabled = false;
                btnConnect.Text = "Bağlantıyı kes";
                btnTransfer.Enabled = true;
            }
            else
            {
                txtServer.Enabled = true;
                txtDatabase.Enabled = true;
                txtUser.Enabled = true;
                txtPass.Enabled = true;
                btnConnect.Text = "Bağlan";
                btnTransfer.Enabled = false;
            }
        }
        SqlConnection GetConnection(bool TrustedConnection)
        {
            if (TrustedConnection)
            {//Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;
                return new SqlConnection($@"Server={txtServer.Text};Database=MikroDB_V15;Trusted_Connection=True;");
            }
            return new SqlConnection($@"Server={txtServer.Text};Database={txtDatabase.Text};User ID={txtUser.Text};Password={txtPass.Text};Trusted_Connection=False;");
        }
        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (con.State == ConnectionState.Open)
                {
                    con.Close();
                }
                else
                {  
                    con = GetConnection(txtDatabase.Text == ""); 
                    con.Open();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Bağlantı Kurulamadı. \n" + ex.Message);
            }
            SetControls();
        }

        private void btnGetExchanges_Click(object sender, EventArgs e)
        {
            DateTime date = dateTimePicker1.Value;
          
            DataTable dt = Exchange.GetDataTableAllCurrenciesHistoricalExchangeRates(date);

            while (dt.Rows.Count == 0)
            {
                date = date.AddDays(-1);
                dt = Exchange.GetDataTableAllCurrenciesHistoricalExchangeRates(date);

            } 
            dataGridView1.DataSource = dt;
        }

        private void btnTransfer_Click(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(Transfer));
            t.Start();

        }
        class KurIsimleri
        {
            public string Kur_sembol { get; set; }
            public int Kur_No { get; set; }
        }
        void Transfer()
        {
            try
            {
                progressBar1.Value = 0;
                progressBar1.Maximum = dataGridView1.Rows.Count;
                string kurIsimleriQuery = "SELECT Kur_No,Kur_sembol  FROM  KUR_ISIMLERI";
                List<KurIsimleri> kurIsimleriList;

                string maxRecNoQuery = "select MAX(dov_RECid_RECno) from DOVIZ_KURLARI";
                int MaxRecNo = 0;
                string InsertQuery = @"INSERT INTO DOVIZ_KURLARI
           (dov_RECid_DBCno
           ,dov_RECid_RECno
           ,dov_SpecRecNo
           ,dov_iptal
           ,dov_fileid
           ,dov_hidden
           ,dov_kilitli
           ,dov_degisti
           ,dov_checksum
           ,dov_create_user
           ,dov_create_date
           ,dov_lastup_user
           ,dov_lastup_date
           ,dov_special1
           ,dov_special2
           ,dov_special3
           ,dov_no
           ,dov_tarih
           ,dov_fiyat1
           ,dov_fiyat2
           ,dov_fiyat3
           ,dov_fiyat4)
     VALUES
           (@dov_RECid_DBCno
           ,@dov_RECid_RECno
           ,@dov_SpecRecNo
           ,@dov_iptal
           ,@dov_fileid
           ,@dov_hidden
           ,@dov_kilitli
           ,@dov_degisti
           ,@dov_checksum
           ,@dov_create_user
           ,@dov_create_date
           ,@dov_lastup_user
           ,@dov_lastup_date
           ,@dov_special1
           ,@dov_special2
           ,@dov_special3
           ,@dov_no
           ,@dov_tarih
           ,@dov_fiyat1
           ,@dov_fiyat2
           ,@dov_fiyat3
           ,@dov_fiyat4)";

                using (var connection = GetConnection(txtDatabase.Text == ""))
                {
                    kurIsimleriList = connection.Query<KurIsimleri>(kurIsimleriQuery).ToList();
                    MaxRecNo = con.QueryFirstOrDefault<int>(maxRecNoQuery);
                    MaxRecNo = MaxRecNo + 1;

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        if (kurIsimleriList.Where(a => a.Kur_sembol.Equals(dataGridView1.Rows[i].Cells[1].Value.ToString())).SingleOrDefault() == null)
                        {
                            progressBar1.Value = i + 1;
                            continue;
                        }
                        DynamicParameters p = new DynamicParameters();
                        p.Add("dov_RECid_DBCno", 0);
                        p.Add("dov_RECid_RECno", MaxRecNo + i);
                        p.Add("dov_SpecRecNo", 0);
                        p.Add("dov_iptal", false);
                        p.Add("dov_fileid", 1007);
                        p.Add("dov_hidden", false);
                        p.Add("dov_kilitli", false);
                        p.Add("dov_degisti", false);
                        p.Add("dov_checksum", 0);
                        p.Add("dov_create_user", 2);
                        p.Add("dov_create_date", DateTime.Now);
                        p.Add("dov_lastup_user", 2);
                        p.Add("dov_lastup_date", DateTime.Now);
                        p.Add("dov_special1", "");
                        p.Add("dov_special2", "");
                        p.Add("dov_special3", "");
                        p.Add("dov_no", kurIsimleriList.Where(a => a.Kur_sembol.Equals(dataGridView1.Rows[i].Cells[1].Value.ToString())).SingleOrDefault().Kur_No);
                        p.Add("dov_tarih", dateTimePicker1.Value.Date);
                        p.Add("dov_fiyat1", Convert.ToDecimal(dataGridView1.Rows[i].Cells[3].Value));
                        p.Add("dov_fiyat2", Convert.ToDecimal(dataGridView1.Rows[i].Cells[4].Value));
                        p.Add("dov_fiyat3", Convert.ToDecimal(dataGridView1.Rows[i].Cells[5].Value));
                        p.Add("dov_fiyat4", Convert.ToDecimal(dataGridView1.Rows[i].Cells[6].Value));
                        con.Query(InsertQuery, p);
                        progressBar1.Value = i + 1;
                    }
                }
                MessageBox.Show(dateTimePicker1.Value.ToString("dd.MM.yyyy") + " aktarım başaralı");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+"\n" + ex.StackTrace);
            }
          

        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(dataGridView1.Rows[0].Cells[2].Value.ToString());
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            progressBar1.Value = 0;
        }
    }
}
