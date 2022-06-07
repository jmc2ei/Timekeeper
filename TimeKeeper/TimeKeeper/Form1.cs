using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Data.OleDb;
using Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;

namespace TimeKeeper
{
    public partial class Form1 : Form
    {
        SqlDataAdapter da;
        SqlConnection con;
        SqlCommand com;

        DateTime startTime;
        DateTime endTime;
        TimeSpan totalTime;

        DateTime breakStart;
        DateTime breakEnd;
        TimeSpan breakTime;
        TimeSpan totalBreakTime;
        bool breakHasStarted;

        int numOfEntries;
        Entry[] entries;
        int counter = 0;

        public Form1()
        {
            InitializeComponent();

            breakHasStarted = false;
            totalBreakTime = TimeSpan.Zero;

            numOfEntries = 0;
            entries = new Entry[10];

            button3.Enabled = false;    //end button
            button4.Enabled = false;    //break button
        }

        public class Entry
        {
            private DateTime date;
            private string name;
            private string id;
            private string description;
            private bool billable;
            private DateTime start;
            private DateTime end;
            private TimeSpan total;
            private TimeSpan totalBreak;

            public Entry(DateTime entryDate, string projectName, string taskID, string projectDescription, bool isBillable, DateTime startTime, DateTime endTime, TimeSpan totalTime, TimeSpan totalBreakTime)
            {
                date = entryDate;
                name = projectName;
                id = taskID;
                description = projectDescription;
                billable = isBillable;
                start = startTime;
                end = endTime;
                total = totalTime;
                totalBreak = totalBreakTime;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'timesDataDataSet.TimesDataTable' table. You can move, or remove it, as needed.
            this.timesDataTableTableAdapter.Fill(this.timesDataDataSet.TimesDataTable);
            timer1.Enabled = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label1.Text = DateTime.Now.ToString("dd-MM-yyy      hh:mm:ss");
        }

        //invoice
        private void button1_Click(object sender, EventArgs e)
        {
            con = new SqlConnection("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\TimesData.mdf;Integrated Security=True");
            con.Open();
            com = new SqlCommand("INSERT INTO TimesDataTable (Date,Start,[End]) VALUES (@Date,@Start,@End)", con);
            com.Parameters.AddWithValue("@Date", textBox2.Text);
            com.Parameters.AddWithValue("@Start", textBox2.Text);
            com.Parameters.AddWithValue("@End", textBox2.Text);

            com.ExecuteNonQuery();
            con.Close();

        }

        //start entry
        private void button2_Click(object sender, EventArgs e)
        {
            startTime = DateTime.Now;
            textBox4.Text = startTime.ToString("hh:mm");

            dateTimePicker1.Enabled = false;    //entry date
                                                //start time
                                                //textBox1.Enabled = false;           //project name
            textBox2.Enabled = false;           //task id
            textBox3.Enabled = false;           //description
            checkBox1.Enabled = false;          //billable checkbox

            button2.Enabled = false;    //start button
            button3.Enabled = true;     //end button
            button4.Enabled = true;     //break button

        }

        //end entry
        private void button3_Click(object sender, EventArgs e)
        {
            endTime = DateTime.Now;
            textBox5.Text = endTime.ToString("hh:mm");
            totalTime = endTime.Subtract(startTime);



            label8.Text = "Total Time: " + totalTime.ToString();
            label7.Text = "Total Billable Time: " + (totalTime - totalBreakTime).ToString();

            dataGridView1.Rows.Add();
            dataGridView1.Rows[counter].Cells[0].Value = dateTimePicker1.Value.ToString("MM/dd/yyyy");
            dataGridView1.Rows[counter].Cells[1].Value = textBox2.Text;
            if (checkBox1.Checked)
                dataGridView1.Rows[counter].Cells[2].Value = "TRUE";
            else
                dataGridView1.Rows[counter].Cells[2].Value = "FALSE";
            dataGridView1.Rows[counter].Cells[3].Value = textBox4.Text;
            dataGridView1.Rows[counter].Cells[4].Value = textBox5.Text;
            dataGridView1.Rows[counter].Cells[5].Value = totalTime.Hours;
            dataGridView1.Rows[counter].Cells[6].Value = totalTime.Minutes;
            dataGridView1.Rows[counter].Cells[7].Value = textBox3.Text;

            counter += 1;

            textBox2.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox3.Text = "";

            totalTime = TimeSpan.Zero;
            totalBreakTime = TimeSpan.Zero;

            dateTimePicker1.Enabled = true;    //entry date
                                               //start time
                                               //textBox1.Enabled = true;           //project name
            textBox2.Enabled = true;           //task id
            textBox3.Enabled = true;           //description
            checkBox1.Enabled = true;          //billable checkbox

            button2.Enabled = true;    //start button
            button3.Enabled = false;     //end button
            button4.Enabled = false;    //break button

        }

        //break
        private void button4_Click(object sender, EventArgs e)
        {
            if (breakHasStarted)    //end of break
            {
                breakEnd = DateTime.Now;
                breakTime = breakEnd.Subtract(breakStart);
                totalBreakTime += breakTime;
                breakHasStarted = false;
                button3.Enabled = true;
            }
            else                    //start of break
            {
                breakStart = DateTime.Now;
                breakHasStarted = true;
                button3.Enabled = false;
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            textBox_Sheet.ReadOnly = false;
            textBox_Sheet.Text = "Sheet1";
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.Refresh();
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.textBox_path.Text = openFileDialog1.FileName;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {

            string PathConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source =" + textBox_path.Text + ";Extended Properties=\"Excel 8.0;HDR=Yes;\";";
            OleDbConnection conn = new OleDbConnection(PathConn);
            OleDbDataAdapter myDataAdapter = new OleDbDataAdapter("Select * from [" + textBox_Sheet.Text + "$]", conn);
            System.Data.DataTable dt = new System.Data.DataTable();
            myDataAdapter.Fill(dt);
            dataGridView1.DataSource = dt;
        }

        //export
        private void button6_Click(object sender, EventArgs e)
        {
            saveFileDialog1.InitialDirectory = "C:";
            saveFileDialog1.Title = "Save as Excel File";
            saveFileDialog1.FileName = "";
            saveFileDialog1.Filter = "Excel Files(2003)|*.xls";
            if (saveFileDialog1.ShowDialog() != DialogResult.Cancel)
            {
                Microsoft.Office.Interop.Excel.Application ExcelApp = new Microsoft.Office.Interop.Excel.Application();
                ExcelApp.Application.Workbooks.Add(Type.Missing);
                ExcelApp.Columns.ColumnWidth = 20;
                for (int i = 1; i < dataGridView1.Columns.Count + 1; i++)
                {
                    ExcelApp.Cells[1, i] = dataGridView1.Columns[i - 1].HeaderText;
                }
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    for (int j = 0; j < dataGridView1.Columns.Count; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value != null)
                        {
                            ExcelApp.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value.ToString();
                        }
                        else {
                            ExcelApp.Cells[i + 2, j + 1] = String.Empty;
                        }
                    }
                }
                ExcelApp.ActiveWorkbook.SaveCopyAs(saveFileDialog1.FileName.ToString());
                ExcelApp.ActiveWorkbook.Saved = true;
                ExcelApp.Quit();
            }

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        //create entry
        private void button6_Click_1(object sender, EventArgs e)
        {
            startTime = DateTime.Parse(textBox4.Text);
            endTime = DateTime.Parse(textBox5.Text);
            totalTime = endTime.Subtract(startTime);

            label8.Text = "Total Time: " + totalTime.ToString();
            label7.Text = "Total Billable Time: " + (totalTime - totalBreakTime).ToString();


            dataGridView1.Rows.Add();
            dataGridView1.Rows[counter].Cells[0].Value = dateTimePicker1.Value.ToString("MM/dd/yyyy");
            dataGridView1.Rows[counter].Cells[1].Value = textBox2.Text;
            if (checkBox1.Checked)
                dataGridView1.Rows[counter].Cells[2].Value = "TRUE";
            else
                dataGridView1.Rows[counter].Cells[2].Value = "FALSE";
            dataGridView1.Rows[counter].Cells[3].Value = textBox4.Text;
            dataGridView1.Rows[counter].Cells[4].Value = textBox5.Text;
            dataGridView1.Rows[counter].Cells[5].Value = totalTime.Hours;
            dataGridView1.Rows[counter].Cells[6].Value = totalTime.Minutes;
            dataGridView1.Rows[counter].Cells[7].Value = textBox3.Text;

            counter += 1;

            textBox2.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox3.Text = "";
            totalTime = TimeSpan.Zero;
            totalBreakTime = TimeSpan.Zero;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
