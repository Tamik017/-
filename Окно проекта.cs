using LiveCharts;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Markup;

namespace Магниторазведка
{
    public partial class Form2 : Form
    {
        private string conn = "Data Source = DBSRV\\AG2023;Initial Catalog = magneticSyrat; Integrated Security = True";

        //"Data Source = DBSRV\\AG2023;Initial Catalog = magneticSyrat; Integrated Security = True";

        private string _projectName;

        private SqlDataAdapter dataAdapter1 = null;

        private DataSet dataSet = null;

        private DataTable table1 = null;

        private string previousValueCost;
        private string previousValueDate;
        private string previousValueExecutor;
        private string previousValueProjectName;

        private float previousValueX;
        private float previousValueY;
        private string previousValueAreaName;

        public Form2(string projectName)
        {
            _projectName = projectName;
            InitializeComponent();
            comboBox1.KeyPress += (sender, e) =>
            {
                e.Handled = true;
            };
        }

        private void UpdateData()
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                dataSet = new DataSet();
                dataAdapter1 = new SqlDataAdapter("SELECT a.areaName FROM Area a INNER JOIN Projects p ON a.projectID = p.id WHERE p.projectName = @_projectName;", connection);
                dataAdapter1.SelectCommand.Parameters.AddWithValue("@_projectName", _projectName);
                table1 = new DataTable();
                dataAdapter1.Fill(table1);
                //dataGridView1.DataSource = table1;
                dataGridView1.ColumnHeadersVisible = false;

                DataTable filteredDt = new DataTable();
                filteredDt = table1.Clone();

                foreach (DataRow row in table1.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["areaName"].ToString()))
                    {
                        filteredDt.ImportRow(row);
                    }
                }

                dataGridView1.DataSource = filteredDt;
            }
        }

        private void Окно_проекта_Load(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();

                string query = "SELECT projectName FROM Projects WHERE projectName = @_projectName; " +
                    "SELECT u.fullname FROM Users u INNER JOIN Contracts c ON u.id = c.customer " +
                    "INNER JOIN Projects p ON c.projectID = p.id WHERE p.projectName = @_projectName; " +
                    "SELECT projectEndDate FROM Projects WHERE projectName = @_projectName;" +
                    "SELECT c.price FROM Contracts c INNER JOIN Projects p ON c.projectID = p.id WHERE p.projectName = @_projectName;" +
                    "SELECT fullname FROM Users";

                UpdateData();


                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@_projectName", _projectName);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows && reader.Read())
                        {
                            textBox1.Text = reader["projectName"].ToString();
                            previousValueProjectName = reader["projectName"].ToString();
                        }

                        reader.NextResult();

                        if (reader.HasRows && reader.Read())
                        {
                            comboBox1.Text = reader["fullname"].ToString();
                            previousValueExecutor = reader["fullname"].ToString();
                        }

                        reader.NextResult();


                        if (reader.HasRows && reader.Read())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                DateTime dateValue = reader.GetDateTime(0);
                                textBox3.Text = dateValue.ToShortDateString();
                                previousValueDate = dateValue.ToShortDateString();
                            }
                            else
                            {
                                textBox3.Text = "";
                                previousValueDate = "";
                            }
                        }

                        reader.NextResult();

                        if (reader.HasRows && reader.Read())
                        {
                            textBox2.Text = reader["price"].ToString();
                            previousValueCost = reader["price"].ToString();
                        }

                        reader.NextResult();

                        while (reader.HasRows && reader.Read())
                        {
                            comboBox1.Items.Add(reader["fullname"].ToString());
                        }
                    }
                }
            }
        }


        private Form3 form3Instance;
        private void button12_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

                if (selectedRow.Cells["areaName"].Value != null)
                {
                    string areaName = selectedRow.Cells["areaName"].Value.ToString();

                    int areaID = GetAreaID(areaName);

                    int projectID = GetProjectID();

                    if (form3Instance == null || form3Instance.IsDisposed)
                    {
                        if (areaID != -1 && projectID != -1)
                        {
                            form3Instance = new Form3(areaID, projectID);
                            lastSelectedAreaName = areaName;
                            form3Instance.Show();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось найти выбранную площадь.");
                        }
                    }
                    else
                    {
                        if (areaName == lastSelectedAreaName)
                        {
                            form3Instance.Activate();
                        }
                        else
                        {
                            form3Instance.Close();
                            if (areaID != -1 && projectID != -1)
                            {
                                form3Instance = new Form3(areaID, projectID);
                                lastSelectedAreaName = areaName;
                                form3Instance.Show();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось найти выбранную площадь.");
                            }
                        }
                        }
                    }
                }
            else
            {
                MessageBox.Show("Не выбрана строка с площадью.");
            }
        }


        private int GetAreaID(string areaName)
        {
            int areaID = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    string sqlQuery = @"SELECT a.id FROM Area a INNER JOIN Projects p ON a.projectID = p.id WHERE a.areaName = @AreaName AND p.projectName = @_projectName;";
                    SqlCommand command = new SqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("@AreaName", areaName);
                    command.Parameters.AddWithValue("@_projectName", _projectName);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        areaID = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении areaID: " + ex.Message);
            }

            return areaID;
        }


        private int GetProjectID()
        {
            int projectID = -1;

            try
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    string sqlQuery = "SELECT id FROM Projects WHERE projectName = @_projectName;";
                    SqlCommand command = new SqlCommand(sqlQuery, connection);
                    command.Parameters.AddWithValue("@_projectName", _projectName);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        projectID = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при получении projectID: " + ex.Message);
            }

            return projectID;
        }


        private string lastSelectedAreaName = null;
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
                if (e.RowIndex >= 0)
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];

                    string areaName = row.Cells["areaName"].Value.ToString();

                    string query = "SELECT a.areaName, a.x, a.y FROM Area a JOIN Projects p ON a.projectID = p.id WHERE a.areaName = @areaName AND p.projectName = @_projectName";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@areaName", areaName);
                        cmd.Parameters.AddWithValue("@_projectName", _projectName);

                        connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            textBox4.Text = reader["areaName"].ToString();
                            previousValueAreaName = reader["areaName"].ToString();
                            textBox5.Text = reader["x"].ToString();
                            if (reader["x"] != DBNull.Value)
                            {
                                previousValueX = (float)reader["x"];
                            }
                            else
                            {
                                previousValueX = 0;
                            }
                            textBox6.Text = reader["y"].ToString();
                            if (reader["y"] != DBNull.Value)
                            {
                                previousValueY = (float)reader["y"];
                            }
                            else
                            {
                                previousValueY = 0;
                            }    
                        }
                        reader.Close();
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string newProjectName = textBox1.Text;
            if (!string.IsNullOrWhiteSpace(newProjectName))
            {
                string query = "SELECT COUNT(*) FROM projects WHERE projectName = @projectName";
                int count = 0;
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@projectName", newProjectName);
                        count = Convert.ToInt32(command.ExecuteScalar());
                    }
                }

                if (count > 0)
                {
                    MessageBox.Show("Проект с таким именем уже существует");
                    textBox1.Text = previousValueProjectName;
                }

                else
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();
                        string query1 = "SELECT id FROM projects WHERE projectName = @projectName";
                        using (SqlCommand command = new SqlCommand(query1, connection))
                        {
                            command.Parameters.AddWithValue("@projectName", _projectName);
                            object result = command.ExecuteScalar();

                            if (result != null)
                            {
                                query1 = "UPDATE projects SET projectName = @newProjectName WHERE projectName = @projectName";
                                using (SqlCommand updateCommand = new SqlCommand(query1, connection))
                                {
                                    updateCommand.Parameters.AddWithValue("@newProjectName", newProjectName);
                                    updateCommand.Parameters.AddWithValue("@projectName", _projectName);
                                    updateCommand.ExecuteNonQuery();
                                    MessageBox.Show("Название обновлено!");
                                    _projectName = newProjectName;
                                    previousValueProjectName = newProjectName;
                                }
                            }
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("Название проекта не может быть пустым", "Ошибка");
                textBox1.Text = previousValueProjectName;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string newExecutor = comboBox1.Text;

            if (!string.IsNullOrWhiteSpace(newExecutor))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    string getUserIdQuery = "SELECT id FROM Users WHERE fullname = @newExecutor;";
                    using (SqlCommand getUserIdCommand = new SqlCommand(getUserIdQuery, connection))
                    {
                        getUserIdCommand.Parameters.AddWithValue("@newExecutor", newExecutor);
                        int userId = (int)getUserIdCommand.ExecuteScalar();


                        string getProjectIdQuery = "SELECT id FROM Projects WHERE projectName = @_projectName;";
                        using (SqlCommand getProjectIdCommand = new SqlCommand(getProjectIdQuery, connection))
                        {
                            getProjectIdCommand.Parameters.AddWithValue("@_projectName", _projectName);
                            int projectId = (int)getProjectIdCommand.ExecuteScalar();

                            string insertOrUpdateQuery = "IF EXISTS (SELECT 1 FROM Contracts WHERE projectID = @projectId) " +
                             "BEGIN " +
                             "    UPDATE Contracts SET customer = @userId WHERE projectID = @projectId; " +
                             "END " +
                             "ELSE " +
                             "BEGIN " +
                             "    INSERT INTO Contracts (customer, projectID) VALUES (@userId, @projectId); " +
                             "END;";

                            using (SqlCommand insertOrUpdateCommand = new SqlCommand(insertOrUpdateQuery, connection))
                            {
                                insertOrUpdateCommand.Parameters.AddWithValue("@userId", userId);
                                insertOrUpdateCommand.Parameters.AddWithValue("@projectId", projectId);
                                insertOrUpdateCommand.ExecuteNonQuery();
                                MessageBox.Show("Имя заказчика обновлено!");
                                previousValueExecutor = newExecutor;
                            }
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("Имя заказчика не может быть пустым", "Ошибка");
                comboBox1.Text = previousValueExecutor;
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            string newDate = textBox3.Text;


            if (!string.IsNullOrWhiteSpace(newDate))
            {
                if (DateTime.TryParse(newDate, out DateTime parsedDate))
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string newDateQuery = "SELECT projectEndDate FROM Projects WHERE projectName = @_projectName;";
                        using (SqlCommand getUserIdCommand = new SqlCommand(newDateQuery, connection))
                        {
                            string insertOrUpdateQuery = "UPDATE Projects SET projectEndDate = @newDate WHERE projectName = @_projectName;";

                            using (SqlCommand insertOrUpdateCommand = new SqlCommand(insertOrUpdateQuery, connection))
                            {
                                insertOrUpdateCommand.Parameters.AddWithValue("@newDate", parsedDate);
                                insertOrUpdateCommand.Parameters.AddWithValue("@_projectName", _projectName);
                                insertOrUpdateCommand.ExecuteNonQuery();
                                MessageBox.Show("Дата завершения обновлена!");
                                previousValueDate = newDate;
                            }
                        };
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректную дату", "Ошибка");
                    textBox3.Text = previousValueDate;
                }
            }
            else
            {
                MessageBox.Show("Дата завершения не может быть пустой", "Ошибка");
                textBox3.Text = previousValueDate;
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string newCost = textBox2.Text;

            if (!string.IsNullOrWhiteSpace(newCost))
            {
                if (decimal.TryParse(newCost, out decimal parsedCost))
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string updateQuery = @"UPDATE Contracts 
                                       SET price = @newCost 
                                       FROM Contracts c 
                                       INNER JOIN Projects p ON c.projectID = p.id 
                                       WHERE p.projectName = @_projectName;";

                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@newCost", SqlDbType.Money).Value = parsedCost;
                            updateCommand.Parameters.AddWithValue("@_projectName", _projectName);

                            int rows = updateCommand.ExecuteNonQuery();

                            if (rows == 0)
                            {
                                MessageBox.Show("Сначала добавьте заказчика!", "Ошибка");
                                textBox2.Text = "";
                            }

                            else
                            {
                                MessageBox.Show("Стоимость обновлена!");
                                previousValueCost = newCost;
                            }
                        }
                    }
                }

                else
                {
                    MessageBox.Show("Введите корректную стоимость", "Ошибка");
                    textBox2.Text = previousValueCost;
                }
            }

            else
            {
                MessageBox.Show("Стоимость не может быть пустой", "Ошибка");
                textBox2.Text = previousValueCost;
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string areaName = Interaction.InputBox("Введите название площади", "Новая площадь", "");

            if (!string.IsNullOrEmpty(areaName))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    string sqlCheckQuery = "SELECT COUNT(*) FROM Area a JOIN Projects p ON a.projectID = p.id WHERE a.areaName = @areaName AND p.projectName = @_projectName";
                    SqlCommand checkCommand = new SqlCommand(sqlCheckQuery, connection);
                    checkCommand.Parameters.Add(new SqlParameter("areaName", areaName));
                    checkCommand.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                    int count = (int)checkCommand.ExecuteScalar();

                    if (count == 0)
                    {
                        string projectName = _projectName;

                        string getProjectId = "SELECT id FROM Projects WHERE projectName = @_projectName;";
                        using (SqlCommand getProjectIdCommand = new SqlCommand(getProjectId, connection))
                        {
                            getProjectIdCommand.Parameters.AddWithValue("@_projectName", _projectName);
                            int projectId = (int)getProjectIdCommand.ExecuteScalar();

                            string sqlInsertQuery = "INSERT INTO Area (AreaName, ProjectID) VALUES (@areaName, @projectId)";
                            SqlCommand insertCommand = new SqlCommand(sqlInsertQuery, connection);
                            insertCommand.Parameters.Add(new SqlParameter("areaName", areaName));
                            insertCommand.Parameters.Add(new SqlParameter("projectId", projectId));

                            insertCommand.ExecuteNonQuery();

                            UpdateData();
                        }
                    }

                    else
                    {
                        MessageBox.Show("Площадь с таким названием уже существует");
                    }
                }
            }

            else
            {
                MessageBox.Show("Название площади не может быть пустым", "Ошибка");
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.Rows[dataGridView1.CurrentCell.RowIndex];
            string OriginalAreaName = selectedRow.Cells["areaName"].Value.ToString();

            string areaName = textBox4.Text;
            float x, y;

            if (float.TryParse(textBox5.Text, out x) && float.TryParse(textBox6.Text, out y))
            {
                if (!float.IsNaN(x) && !float.IsNaN(y))
                {
                    string sqlCheckQuery = "SELECT COUNT(*) FROM Area a JOIN Projects p ON a.projectID = p.id WHERE a.areaName = @areaName AND p.projectName = @_projectName AND a.areaName <> @OriginalAreaName";
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();
                        SqlCommand checkCommand = new SqlCommand(sqlCheckQuery, connection);
                        checkCommand.Parameters.Add(new SqlParameter("@areaName", areaName));
                        checkCommand.Parameters.Add(new SqlParameter("@OriginalAreaName", OriginalAreaName));
                        checkCommand.Parameters.Add(new SqlParameter("@_projectName", _projectName));
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count == 0)
                        {
                            string sqlQuery = @"
                        UPDATE Area 
                        SET x = CASE 
                                WHEN @x IS NOT NULL THEN @x
                                ELSE x
                                END,
                            y = CASE 
                                WHEN @y IS NOT NULL THEN @y
                                ELSE y
                                END,
                            areaName = CASE 
                                WHEN @areaName IS NOT NULL THEN @areaName
                                ELSE areaName
                                END
                        FROM Area a
                        INNER JOIN Projects p ON a.projectID = p.id
                        WHERE a.areaName = @OriginalAreaName AND p.projectName = @_projectName;

                        IF @@ROWCOUNT = 0
                        BEGIN
                            INSERT INTO Area (x, y, areaName, projectID)
                            VALUES (@x, @y, @areaName, (SELECT id FROM Projects WHERE projectName = @_projectName))
                        END;

                        SELECT areaName
                        FROM Area
                        WHERE areaName = @OriginalAreaName;";


                            using (SqlCommand updateCommand = new SqlCommand(sqlQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@areaName", areaName);
                                updateCommand.Parameters.AddWithValue("@OriginalAreaName", OriginalAreaName);
                                updateCommand.Parameters.AddWithValue("@x", x);
                                updateCommand.Parameters.AddWithValue("@y", y);
                                updateCommand.Parameters.AddWithValue("@_projectName", _projectName);
                                updateCommand.ExecuteNonQuery();
                                MessageBox.Show("Данные обновлены!");
                                UpdateData();

                                previousValueY = y;
                                previousValueX = x;
                                previousValueAreaName = areaName;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Площадь с таким названием уже существует в данном проекте");
                            textBox4.Text = previousValueAreaName;
                            textBox5.Text = previousValueX.ToString();
                            textBox6.Text = previousValueY.ToString();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректные значения", "Ошибка");
                    textBox4.Text = previousValueAreaName;
                    textBox5.Text = previousValueX.ToString();
                    textBox6.Text = previousValueY.ToString();
                }
            }
            else
            {
                MessageBox.Show("Введите корректные значения", "Ошибка");
                textBox4.Text = previousValueAreaName;
                textBox5.Text = previousValueX.ToString();
                textBox6.Text = previousValueY.ToString();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string newExecutor = comboBox1.Text;

            if (!string.IsNullOrWhiteSpace(newExecutor))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    string getUserIdQuery = "SELECT id FROM Users WHERE fullname = @newExecutor;";
                    using (SqlCommand getUserIdCommand = new SqlCommand(getUserIdQuery, connection))
                    {
                        getUserIdCommand.Parameters.AddWithValue("@newExecutor", newExecutor);
                        int userId = (int)getUserIdCommand.ExecuteScalar();

                        string deleteQuery = "DELETE FROM Contracts WHERE customer = @userId;";

                        using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@userId", userId);
                            deleteCommand.ExecuteNonQuery();
                            MessageBox.Show("Заказчик удален!");
                            previousValueExecutor = string.Empty;
                            comboBox1.Text = "";
                            textBox2.Text = "";
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("Пустое поле", "Ошибка");
                comboBox1.Text = previousValueExecutor;
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string newDate = textBox3.Text;

            if (!string.IsNullOrWhiteSpace(newDate))
            {
                if (DateTime.TryParse(newDate, out DateTime parsedDate))
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string newDateQuery = "SELECT projectEndDate FROM Projects WHERE projectName = @projectName;";
                        using (SqlCommand getUserIdCommand = new SqlCommand(newDateQuery, connection))
                        {
                            string insertOrUpdateQuery = "UPDATE Projects SET projectEndDate = NULL WHERE projectName = @projectName;";

                            using (SqlCommand insertOrUpdateCommand = new SqlCommand(insertOrUpdateQuery, connection))
                            {
                                insertOrUpdateCommand.Parameters.AddWithValue("@projectName", _projectName);
                                insertOrUpdateCommand.ExecuteNonQuery();
                                MessageBox.Show("Дата завершения удалена!");
                                previousValueDate = string.Empty;
                                textBox3.Text = "";
                            }
                        };
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректную дату", "Ошибка");
                    textBox3.Text = previousValueDate;
                }
            }
            else
            {
                MessageBox.Show("Пустое поле", "Ошибка");
                textBox3.Text = previousValueDate;
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            string newCost = textBox2.Text;

            if (!string.IsNullOrWhiteSpace(newCost))
            {
                if (decimal.TryParse(newCost, out decimal parsedCost))
                {
                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string updateQuery = @"UPDATE Contracts 
                                       SET price = NULL 
                                       FROM Contracts c 
                                       INNER JOIN Projects p ON c.projectID = p.id 
                                       WHERE p.projectName = @_projectName;";

                        using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@_projectName", _projectName);

                            int rows = updateCommand.ExecuteNonQuery();

                            if (rows == 0)
                            {
                                MessageBox.Show("Сначала добавьте заказчика!", "Ошибка");
                            }

                            else
                            {
                                MessageBox.Show("Стоимость удалена!");
                                previousValueCost = null;
                                textBox2.Text = "";
                            }
                        }
                    }
                }

                else
                {
                    MessageBox.Show("Введите корректную стоимость", "Ошибка");
                    textBox2.Text = previousValueCost;
                }
            }

            else
            {
                MessageBox.Show("Пустое поле", "Ошибка");
                textBox2.Text = previousValueCost;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];

            if (selectedRow.Cells["areaName"].Value != null)
            {
                string areaName = selectedRow.Cells["areaName"].Value.ToString();

            if (!string.IsNullOrEmpty(areaName))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    string getProjectIdQuery = "SELECT id FROM Projects WHERE projectName = @projectName;";
                        using (SqlCommand getProjectIdCommand = new SqlCommand(getProjectIdQuery, connection))
                        {
                            getProjectIdCommand.Parameters.AddWithValue("@projectName", _projectName);
                            object projectId = getProjectIdCommand.ExecuteScalar();

                            if (projectId != null && projectId != DBNull.Value)
                            {
                                int projectIdToDelete = (int)projectId;


                                string sqlDeleteQuery2 = @"
                            DELETE pt
                            FROM Points pt
                            JOIN Profiles pr ON pt.profileID = pr.id
                            JOIN Area a ON pr.areaID = a.id
                            JOIN Projects p ON a.projectID = p.id
                            WHERE a.areaName = @areaName AND p.projectName = @_projectName;
                            ";

                                SqlCommand deleteCommand2 = new SqlCommand(sqlDeleteQuery2, connection);
                                deleteCommand2.Parameters.Add(new SqlParameter("areaName", areaName));
                                deleteCommand2.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                                deleteCommand2.ExecuteNonQuery();


                                string sqlDeleteQuery1 = @"
                            DELETE pr
                            FROM Profiles pr
                            JOIN Area a ON pr.areaID = a.id
                            JOIN Projects p ON a.projectID = p.id
                            WHERE a.areaName = @areaName AND p.projectName = @_projectName;
                            ";

                                SqlCommand deleteCommand1 = new SqlCommand(sqlDeleteQuery1, connection);
                                deleteCommand1.Parameters.Add(new SqlParameter("areaName", areaName));
                                deleteCommand1.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                                deleteCommand1.ExecuteNonQuery();


                                string sqlDeleteQuery3 = @"
                            DELETE a
                            FROM Area a
                            JOIN Projects p ON a.projectID = p.id
                            WHERE a.areaName = @areaName AND p.projectName = @_projectName;
                            ";

                                SqlCommand deleteCommand3 = new SqlCommand(sqlDeleteQuery3, connection);
                                deleteCommand3.Parameters.Add(new SqlParameter("areaName", areaName));
                                deleteCommand3.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                                deleteCommand3.ExecuteNonQuery();


                                UpdateData();

                                textBox4.Text = "";
                                textBox5.Text = "";
                                textBox6.Text = "";
                            }
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("Выберите площадь для удаления", "Ошибка");
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_projectName))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    string getProjectIdQuery = "SELECT id FROM Projects WHERE projectName = @projectName;";
                    using (SqlCommand getProjectIdCommand = new SqlCommand(getProjectIdQuery, connection))
                    {
                        getProjectIdCommand.Parameters.AddWithValue("@projectName", _projectName);
                        object projectIdObj = getProjectIdCommand.ExecuteScalar();

                        if (projectIdObj != null && projectIdObj != DBNull.Value)
                        {
                            int projectIdToDelete = (int)projectIdObj;

                            string sqlDeleteQuery5 = @"
                            DELETE c
                            FROM Contracts c
                            WHERE c.projectID = @projectId;
                        ";

                            SqlCommand deleteCommand5 = new SqlCommand(sqlDeleteQuery5, connection);
                            deleteCommand5.Parameters.AddWithValue("@projectId", projectIdToDelete);

                            deleteCommand5.ExecuteNonQuery();


                            string sqlDeleteQuery2 = @"
                                    DELETE pt
                                    FROM Points pt
                                    JOIN Profiles pr ON pt.profileID = pr.id
                                    JOIN Area a ON pr.areaID = a.id
                                    JOIN Projects p ON a.projectID = p.id
                                    WHERE p.projectName = @_projectName;
                                    ";

                            SqlCommand deleteCommand2 = new SqlCommand(sqlDeleteQuery2, connection);
                            deleteCommand2.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                            deleteCommand2.ExecuteNonQuery();


                            string sqlDeleteQuery1 = @"
                                    DELETE pr
                                    FROM Profiles pr
                                    JOIN Area a ON pr.areaID = a.id
                                    JOIN Projects p ON a.projectID = p.id
                                    WHERE p.projectName = @_projectName;
                                    ";

                            SqlCommand deleteCommand1 = new SqlCommand(sqlDeleteQuery1, connection);
                            deleteCommand1.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                            deleteCommand1.ExecuteNonQuery();


                            string sqlDeleteQuery3 = @"
                                    DELETE a
                                    FROM Area a
                                    JOIN Projects p ON a.projectID = p.id
                                    WHERE p.projectName = @_projectName;
                                    ";

                            SqlCommand deleteCommand3 = new SqlCommand(sqlDeleteQuery3, connection);
                            deleteCommand3.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                            deleteCommand3.ExecuteNonQuery();

                            string sqlDeleteQuery4 = @"
                                    DELETE p
                                    FROM Projects p
                                    WHERE p.projectName = @_projectName;
                                ";

                            SqlCommand deleteCommand4 = new SqlCommand(sqlDeleteQuery4, connection);
                            deleteCommand4.Parameters.Add(new SqlParameter("@_projectName", _projectName));

                            deleteCommand3.ExecuteNonQuery();


                            UpdateData();

                            using (SqlCommand deleteAreaCommand = new SqlCommand(sqlDeleteQuery4, connection))
                            {
                                deleteAreaCommand.Parameters.AddWithValue("@projectId", projectIdToDelete);
                                deleteAreaCommand.Parameters.Add(new SqlParameter("@_projectName", _projectName));
                                int rowsAffected = deleteAreaCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Проект успешно удален.");

                                    Form1 initialForm = new Form1();
                                    initialForm.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось удалить проект из базы данных.");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Проект с указанным именем не найден.");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Имя проекта не может быть пустым.");
            }
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();   
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}