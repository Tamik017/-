using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Магниторазведка
{
    public partial class Form1 : Form
    {
        private string conn = "Data Source = DBSRV\\AG2023;Initial Catalog = magneticSyrat; Integrated Security = True";
        //"Data Source=ALBEDUS\\SQLEXPRESS;Initial Catalog=Magnetic2;Integrated Security=True";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            List<string> projectNames = new List<string>();
            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("SELECT projectName FROM projects", connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            projectNames.Add(reader.GetString(0));
                        }
                    }
                }
            }

            foreach (string projectName in projectNames)
            {
                Button newButton = new Button();
                newButton.AutoEllipsis = true;
                newButton.BackColor = System.Drawing.Color.DarkBlue;
                newButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                newButton.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                newButton.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
                newButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
                newButton.Margin = new System.Windows.Forms.Padding(2);
                newButton.Size = new System.Drawing.Size(100, 30);
                newButton.UseVisualStyleBackColor = false;
                newButton.Text = projectName;
                newButton.Click += new EventHandler(ProjectButton_Click);
                flowLayoutPanel1.Controls.Add(newButton);
            }
        }

        private void ProjectButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;
            string projectName = clickedButton.Text;
            Form2 detailsForm = new Form2(projectName);
            this.Hide();
            detailsForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string projectName = Microsoft.VisualBasic.Interaction.InputBox("Введите название проекта", "Добавить проект", "");
            if (!string.IsNullOrEmpty(projectName))
            {
                string query = "SELECT COUNT(*) FROM projects WHERE projectName = @projectName";
                int count = 0;
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@projectName", projectName);
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }

                if (count > 0)
                {
                    MessageBox.Show("Проект с таким именем уже существует");
                }

                else
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();
                        string insertQuery = "INSERT INTO projects (projectName) VALUES (@projectName)";

                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@projectName", projectName);
                            command.ExecuteNonQuery();
                        }
                    }

                    Button newButton = new Button();
                    newButton.AutoEllipsis = true;
                    newButton.BackColor = System.Drawing.Color.DarkBlue;
                    newButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                    newButton.Font = new System.Drawing.Font("Microsoft YaHei", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
                    newButton.ForeColor = System.Drawing.SystemColors.ButtonHighlight;
                    newButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
                    newButton.Margin = new System.Windows.Forms.Padding(2);
                    newButton.Size = new System.Drawing.Size(100, 30);
                    newButton.UseVisualStyleBackColor = false;
                    newButton.Text = projectName;
                    newButton.Click += new EventHandler(ProjectButton_Click);
                    flowLayoutPanel1.Controls.Add(newButton);
                }
            }

            else
            {
                MessageBox.Show("Название проекта не может быть пустым", "Ошибка");
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

            
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}