using System;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.VisualBasic.ApplicationServices;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using LiveCharts;
using LiveCharts.Wpf;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using System.Reflection;
using static System.Windows.Forms.LinkLabel;
using System.Globalization;
using System.Windows.Forms.VisualStyles;
using System.Runtime.Remoting.Lifetime;

namespace Магниторазведка
{
    public partial class Form3 : Form
    {
        private string conn = "Data Source = DBSRV\\AG2023;Initial Catalog = magneticSyrat; Integrated Security = True";

        private SqlConnection sqlconnection = null;

        private SqlDataAdapter dataAdapter = null;

        private SqlDataAdapter dataAdapter1 = null;

        private SqlDataAdapter dataAdapter2 = null;

        private DataSet dataSet = null;

        private DataSet dataSet1 = null;

        private DataTable table = null;

        private DataTable table1 = null;

        private DataTable table2 = null;

        private int AreaID;
        private int ProjectID;

        public Form3(int area, int project)
        {
            AreaID = area;
            ProjectID = project; 
            InitializeComponent();
            comboBox1.KeyPress += (sender, e) =>
            {
                e.Handled = true;
            };
        }

        private void UpdateProfile()
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                dataSet = new DataSet();
                dataAdapter2 = new SqlDataAdapter("SELECT pf.profileName FROM Profiles pf INNER JOIN Area a ON pf.areaID = a.id WHERE a.id = @AreaID;", connection);
                dataAdapter2.SelectCommand.Parameters.AddWithValue("@AreaID", AreaID);
                table2 = new DataTable();
                dataAdapter2.Fill(table2);
                dataGridView3.ColumnHeadersVisible = false;

                DataTable filteredDt = new DataTable();
                filteredDt = table2.Clone();

                foreach (DataRow row in table2.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["profileName"].ToString()))
                    {
                        filteredDt.ImportRow(row);
                    }
                }

                dataGridView3.DataSource = filteredDt;
            }
        }



        private void UpdatePoints(string ProfileName, int AreaID)
        {
            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();

                string query = "SELECT Points.number AS number FROM Points INNER JOIN Profiles ON Points.profileID = Profiles.id WHERE Profiles.profileName = @ProfileName AND Profiles.areaID = @AreaID;";
                dataAdapter1 = new SqlDataAdapter(query, connection);
                dataAdapter1.SelectCommand.Parameters.AddWithValue("@ProfileName", ProfileName);
                dataAdapter1.SelectCommand.Parameters.AddWithValue("@AreaID", AreaID);
                table1 = new DataTable();
                dataAdapter1.Fill(table1);
                dataGridView2.DataSource = table1;
                dataGridView2.ColumnHeadersVisible = false;

                foreach (DataGridViewColumn column in dataGridView2.Columns)
                {
                    column.Visible = false;
                }

                if (dataGridView2.Columns.Contains("number"))
                {
                    dataGridView2.Columns["number"].Visible = true;
                }
            }
        }


        private void UpdateOperator(string ProfileName, int AreaID)
        {
             using (SqlConnection connection = new SqlConnection(conn))
             {
                    connection.Open();

                    string query2 = "SELECT u.fullname FROM Users u INNER JOIN Profiles p ON u.id = p.operator WHERE p.profileName = @ProfileName AND p.areaID = @AreaID;";

                    using (SqlCommand command = new SqlCommand(query2, connection))
                    {
                        command.Parameters.AddWithValue("@ProfileName", ProfileName);
                        command.Parameters.AddWithValue("@AreaID", AreaID);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                comboBox1.Text = reader["fullname"].ToString();
                            }

                        }
                    }
             }
        }

        private void Окно_площади_Load(object sender, EventArgs e)
        {
            UpdateProfile();

            using (SqlConnection connection = new SqlConnection(conn))
            {
                connection.Open();

                string query = "SELECT fullname FROM Users WHERE post = 'operator'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.HasRows && reader.Read())
                        {
                            comboBox1.Items.Add(reader["fullname"].ToString());
                        }
                    }
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentCell != null && dataGridView2.CurrentCell.Value != null)
            {
                DataGridViewRow selectedRow = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];

                //DataGridViewRow selectedRow = dataGridView3.Rows[0];
                string ProfileName = selectedRow.Cells["profileName"].Value.ToString();

                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();
                    string query2 = "SELECT * FROM Points p INNER JOIN Profiles pr ON pr.id = p.profileID WHERE pr.profileName = @ProfileName AND pr.areaID = @AreaID;";

                    if (dataSet.Tables["Points"] != null)
                    {
                        dataSet.Tables["Points"].Clear();
                    }

                    SqlDataAdapter dataAdapter2 = new SqlDataAdapter(query2, connection);
                    dataAdapter2.SelectCommand.Parameters.AddWithValue("@ProfileName", ProfileName);
                    dataAdapter2.SelectCommand.Parameters.AddWithValue("@AreaID", AreaID);
                    dataAdapter2.Fill(dataSet, "Points");
                    table2 = dataSet.Tables["Points"];

                    SeriesCollection series = new SeriesCollection();
                    ChartValues<int> values = new ChartValues<int>();
                    List<int> valuesList = new List<int>();

                    values.Clear();

                    int n = table2.Rows.Count;
                    for (int i = 0; i < n; i++)
                    {
                        if (table2.Rows[i]["induction"] != DBNull.Value)
                        {
                            values.Add(Convert.ToInt32(table2.Rows[i]["induction"]));
                            valuesList.Add(i + 1);
                        }
                        else
                        {
                            values.Add(0);
                            valuesList.Add(i + 1);
                        }
                    }


                    cartesianChart1.AxisX.Clear();
                    cartesianChart1.AxisY.Clear();

                    cartesianChart1.AxisX.Add(new Axis
                    {
                        Title = "Номер пикета",
                        Labels = valuesList.Select(x => x.ToString()).ToArray()
                    });

                    cartesianChart1.AxisY.Add(new Axis
                    {
                        Title = "Значение индукции, нТл"
                    });

                    LineSeries line = new LineSeries();

                    line.Title = "нТл =";
                    line.Foreground = System.Windows.Media.Brushes.White;
                    line.Values = values;

                    line.Stroke = System.Windows.Media.Brushes.Red;
                    line.Fill = System.Windows.Media.Brushes.Black;

                    cartesianChart1.Series = new SeriesCollection { line };
                }
            }
        }


        private void button6_Click(object sender, EventArgs e)
        {
            string profileName = Interaction.InputBox("Введите название профиля", "Новый профиль", "");

            if (!string.IsNullOrEmpty(profileName))
            {
                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    string sqlCheckQuery = "SELECT COUNT(*) FROM Profiles WHERE profileName = @profileName AND areaID = @areaID;";
                    SqlCommand checkCommand = new SqlCommand(sqlCheckQuery, connection);
                    checkCommand.Parameters.Add(new SqlParameter("@profileName", profileName));
                    checkCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));

                        int count = (int)checkCommand.ExecuteScalar();

                        if (count == 0)
                        {
                                string sqlInsertQuery = "INSERT INTO Profiles (profileName, areaID) VALUES (@profileName, @areaID)";
                                SqlCommand insertCommand = new SqlCommand(sqlInsertQuery, connection);
                                insertCommand.Parameters.Add(new SqlParameter("@profileName", profileName));
                                insertCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));

                                insertCommand.ExecuteNonQuery();
                                MessageBox.Show("Профиль добавлен!");

                                UpdateProfile();
                        }
                        else
                        {
                            MessageBox.Show("Профиль с таким названием уже существует");
                        }
                    }
                }
             else
             {
                MessageBox.Show("Название профиля не может быть пустым", "Ошибка");
             }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Random random = new Random();

            int randomValue = random.Next(27800, 32000);
            float randomX = (float)random.NextDouble() * 100;
            float randomY = (float)random.NextDouble() * 100;

            textBox1.Text = randomX.ToString();
            textBox2.Text = randomY.ToString();

            textBox3.Text = DateTime.Now.ToString("dd.MM.yyyy");

            textBox5.Text = DateTime.Now.ToString("HH:mm:ss");

            textBox6.Text = randomValue.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell != null && dataGridView3.CurrentCell.Value != null)
            {
                DataGridViewRow selectedRow = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];

                if (selectedRow.Cells["profileName"].Value != null)
                {
                    string ProfileName = selectedRow.Cells["profileName"].Value.ToString();

                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string selectProfileIDQuery = "SELECT id FROM Profiles WHERE profileName = @profileName AND areaID = @areaID";
                        SqlCommand selectProfileIDCommand = new SqlCommand(selectProfileIDQuery, connection);
                        selectProfileIDCommand.Parameters.Add(new SqlParameter("@profileName", ProfileName));
                        selectProfileIDCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));

                        int profileID = (int)selectProfileIDCommand.ExecuteScalar();
                        int number = 1;

                        if (profileID > 0)
                        {
                            string selectMaxNumberQuery = "SELECT MAX(number) FROM Points WHERE profileID = @profileID";
                            SqlCommand selectMaxNumberCommand = new SqlCommand(selectMaxNumberQuery, connection);
                            selectMaxNumberCommand.Parameters.Add(new SqlParameter("@profileID", profileID));

                            object maxNumber = selectMaxNumberCommand.ExecuteScalar();

                            if (maxNumber != DBNull.Value)
                            {
                                number = (int)maxNumber + 1;
                            }

                            string sqlInsertQuery = "INSERT INTO Points (profileID, number) VALUES (@profileID, @number)";
                            SqlCommand insertCommand = new SqlCommand(sqlInsertQuery, connection);
                            insertCommand.Parameters.Add(new SqlParameter("@profileID", profileID));
                            insertCommand.Parameters.Add(new SqlParameter("@number", number));

                            insertCommand.ExecuteNonQuery();

                            UpdatePoints(ProfileName, AreaID);
                        }

                        else
                        {
                            MessageBox.Show("Не удалось найти профиль");
                        }
                    }
                }
            }
        }

        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView3.Rows[e.RowIndex];
                string ProfileName = selectedRow.Cells["profileName"].Value.ToString();

                UpdatePoints(ProfileName, AreaID);
                UpdateOperator(ProfileName, AreaID);
                cartesianChart1.Series.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView2.CurrentCell != null && dataGridView2.CurrentCell.Value != null)
            {
                if (float.TryParse(textBox1.Text, out float x) &&
                float.TryParse(textBox2.Text, out float y) &&
                int.TryParse(textBox6.Text, out int induction) &&
                DateTime.TryParseExact(textBox3.Text, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shootingDate) &&
                DateTime.TryParseExact(textBox5.Text, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime shootingTime))
                {
                    DataGridViewRow selectedRow = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex];
                    string PointNumber = selectedRow.Cells["number"].Value.ToString();

                    DataGridViewRow selectedRow2 = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];
                    string ProfileName = selectedRow2.Cells["profileName"].Value.ToString();


                    float xValue = float.Parse(textBox1.Text);
                    float yValue = float.Parse(textBox2.Text);
                    int inductionValue = int.Parse(textBox6.Text);
                    DateTime Date = DateTime.Parse(textBox3.Text);
                    DateTime Time = DateTime.Parse(textBox5.Text);

                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string query = "UPDATE Points " +
                        "SET x = @xValue, y = @yValue, induction = @inductionValue, shootingDate = @Date, shootingTime = @Time " +
                        "WHERE Points.number = @PointNumber " +
                        "AND Points.profileID IN (SELECT id FROM Profiles WHERE profileName = @ProfileName AND areaID = @AreaID);";

                        using (SqlCommand updateCommand = new SqlCommand(query, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@xValue", xValue);
                            updateCommand.Parameters.AddWithValue("@yValue", yValue);
                            updateCommand.Parameters.AddWithValue("@inductionValue", inductionValue);
                            updateCommand.Parameters.AddWithValue("@Date", Date);
                            updateCommand.Parameters.AddWithValue("@Time", Time);
                            updateCommand.Parameters.AddWithValue("@PointNumber", PointNumber);
                            updateCommand.Parameters.AddWithValue("@AreaID", AreaID);
                            updateCommand.Parameters.AddWithValue("@ProfileName", ProfileName);

                            updateCommand.ExecuteNonQuery();

                            MessageBox.Show("Данные обновлены");
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Пожалуйста, введите правильные данные");
                }
            }
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell != null && dataGridView3.CurrentCell.Value != null)
            {
                string newOperator = comboBox1.Text;
                DataGridViewRow selectedRow2 = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];
                string ProfileName = selectedRow2.Cells["profileName"].Value.ToString();

                if (string.IsNullOrWhiteSpace(ProfileName))
                {
                    MessageBox.Show("Не выбран профиль! Пожалуйста, выберите профиль перед изменением оператора.");
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(newOperator))
                    {
                        using (SqlConnection connection = new SqlConnection(conn))
                        {
                            connection.Open();

                            string getUserIdQuery = "SELECT id FROM Users WHERE fullname = @newOperator;";
                            using (SqlCommand getUserIdCommand = new SqlCommand(getUserIdQuery, connection))
                            {
                                getUserIdCommand.Parameters.AddWithValue("@newOperator", newOperator);
                                int userId = (int)getUserIdCommand.ExecuteScalar();


                                string getProfileIdQuery = "SELECT id FROM Profiles WHERE profileName = @ProfileName;";
                                using (SqlCommand getProfileIdCommand = new SqlCommand(getProfileIdQuery, connection))
                                {
                                    getProfileIdCommand.Parameters.AddWithValue("@ProfileName", ProfileName);
                                    int profileId = (int)getProfileIdCommand.ExecuteScalar();

                                    string insertOrUpdateQuery = "UPDATE Profiles SET operator = @userId WHERE id = @profileId";

                                    using (SqlCommand insertOrUpdateCommand = new SqlCommand(insertOrUpdateQuery, connection))
                                    {
                                        insertOrUpdateCommand.Parameters.AddWithValue("@userId", userId);
                                        insertOrUpdateCommand.Parameters.AddWithValue("@profileId", profileId);
                                        insertOrUpdateCommand.ExecuteNonQuery();
                                        MessageBox.Show("Имя оператора обновлено!");
                                    }
                                }
                            }

                        }
                    }
                    else
                    {
                        MessageBox.Show("Имя оператора не может быть пустым", "Ошибка");
                    }
                }
            }
            else
            {
            MessageBox.Show("Не выбран профиль!");
            }
        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView3.CurrentCell != null && dataGridView3.CurrentCell.Value != null)
            {
                DataGridViewRow selectedRow = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex];
            string PointNumber = selectedRow.Cells["number"].Value.ToString();

            DataGridViewRow selectedRow2 = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];
            string ProfileName = selectedRow2.Cells["profileName"].Value.ToString();

                using (SqlConnection connection = new SqlConnection(conn))
                {
                    connection.Open();

                    string query = "SELECT x FROM Points JOIN Profiles ON Points.profileID = Profiles.id WHERE Points.number = @PointNumber AND Profiles.profileName = @ProfileName AND Profiles.areaID = @AreaID;" +
                    "SELECT y FROM Points JOIN Profiles ON Points.profileID = Profiles.id WHERE Points.number = @PointNumber AND Profiles.profileName = @ProfileName AND Profiles.areaID = @AreaID;" +
                    "SELECT induction FROM Points JOIN Profiles ON Points.profileID = Profiles.id WHERE Points.number = @PointNumber AND Profiles.profileName = @ProfileName AND Profiles.areaID = @AreaID;" +
                    "SELECT shootingDate FROM Points JOIN Profiles ON Points.profileID = Profiles.id WHERE Points.number = @PointNumber AND Profiles.profileName = @ProfileName AND Profiles.areaID = @AreaID;" +
                    "SELECT shootingTime FROM Points JOIN Profiles ON Points.profileID = Profiles.id WHERE Points.number = @PointNumber AND Profiles.profileName = @ProfileName AND Profiles.areaID = @AreaID;";

                    using (SqlCommand сommand = new SqlCommand(query, connection))
                    {
                        сommand.Parameters.AddWithValue("@PointNumber", PointNumber);
                        сommand.Parameters.AddWithValue("@AreaID", AreaID);
                        сommand.Parameters.AddWithValue("@ProfileName", ProfileName);

                        using (SqlDataReader reader = сommand.ExecuteReader())
                        {
                            if (reader.HasRows && reader.Read())
                            {
                                textBox1.Text = reader["x"].ToString();
                            }

                            reader.NextResult();

                            if (reader.HasRows && reader.Read())
                            {
                                textBox2.Text = reader["y"].ToString();
                            }

                            reader.NextResult();

                            if (reader.HasRows && reader.Read())
                            {
                                textBox6.Text = reader["induction"].ToString();
                            }

                            reader.NextResult();

                            if (reader.HasRows && reader.Read())
                            {
                                textBox3.Text = reader["shootingDate"].ToString();
                            }

                            reader.NextResult();


                            if (reader.HasRows && reader.Read())
                            {
                                textBox5.Text = reader["shootingTime"].ToString();
                            }
                        }

                    }
                

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell != null && dataGridView3.CurrentCell.Value != null && dataGridView2.CurrentCell != null && dataGridView2.CurrentCell.Value != null)
            {
                DataGridViewRow selectedRow = dataGridView2.Rows[dataGridView2.CurrentCell.RowIndex];

                if (selectedRow.Cells["number"].Value != null)
                {
                    string PointNumber = selectedRow.Cells["number"].Value.ToString();

                DataGridViewRow selectedRow2 = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];
                    if (selectedRow2.Cells["profileName"].Value != null)
                    {
                        string ProfileName = selectedRow2.Cells["profileName"].Value.ToString();

                        using (SqlConnection connection = new SqlConnection(conn))
                        {
                            connection.Open();

                            string selectProfileIDQuery = "SELECT p.profileID FROM Points p JOIN Profiles pr ON p.profileID = pr.id WHERE p.number = @number AND pr.profileName = @profileName AND pr.areaID = @areaID;";
                            SqlCommand selectProfileIDCommand = new SqlCommand(selectProfileIDQuery, connection);
                            selectProfileIDCommand.Parameters.Add(new SqlParameter("@number", PointNumber));
                            selectProfileIDCommand.Parameters.Add(new SqlParameter("@profileName", ProfileName));
                            selectProfileIDCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));

                            int profileID = (int)selectProfileIDCommand.ExecuteScalar();

                            if (profileID > 0)
                            {
                                string deletePointQuery = "DELETE p FROM Points p JOIN Profiles pr ON p.profileID = pr.id WHERE p.number = @number AND pr.profileName = @profileName AND pr.areaID = @areaID;";
                                SqlCommand deletePointCommand = new SqlCommand(deletePointQuery, connection);
                                deletePointCommand.Parameters.Add(new SqlParameter("@number", PointNumber));
                                deletePointCommand.Parameters.Add(new SqlParameter("@profileName", ProfileName));
                                deletePointCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));

                                int rowsAffected = deletePointCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Точка удалена.");
                                    UpdatePoints(ProfileName, AreaID);
                                    cartesianChart1.Series.Clear();

                                }
                                else
                                {
                                    MessageBox.Show("Не удалось удалить точку.");
                                }
                            }
                            else
                            {
                                MessageBox.Show("Не удалось найти профиль для данной точки.");
                            }
                        }
                    }
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell != null && dataGridView3.CurrentCell.Value != null)
            {
                DataGridViewRow selectedRow = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];

                if (selectedRow.Cells["profileName"].Value != null)
                {
                    string ProfileName = selectedRow.Cells["profileName"].Value.ToString();

                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string deletePointsQuery = "DELETE p FROM Points p JOIN Profiles pr ON p.profileID = pr.id WHERE pr.profileName = @profileName AND pr.areaID = @areaID;";
                        SqlCommand deletePointsCommand = new SqlCommand(deletePointsQuery, connection);
                        deletePointsCommand.Parameters.Add(new SqlParameter("@profileName", ProfileName));
                        deletePointsCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));
                        deletePointsCommand.ExecuteNonQuery();

                        string deleteProfileQuery = "DELETE FROM Profiles WHERE profileName = @profileName AND areaID = @areaID";
                        SqlCommand deleteProfileCommand = new SqlCommand(deleteProfileQuery, connection);
                        deleteProfileCommand.Parameters.Add(new SqlParameter("@profileName", ProfileName));
                        deleteProfileCommand.Parameters.Add(new SqlParameter("@areaID", AreaID));

                        int rowsAffected = deleteProfileCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Профиль удален.");
                            
                            dataGridView2.DataSource = null;
                            dataGridView2.Rows.Clear();
                            UpdateProfile();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить профиль.");
                        }
                    }
                }
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (dataGridView3.CurrentCell != null && dataGridView3.CurrentCell.Value != null)
            {
                DataGridViewRow selectedRow2 = dataGridView3.Rows[dataGridView3.CurrentCell.RowIndex];
                string ProfileName = selectedRow2.Cells["profileName"].Value.ToString();

                if (!string.IsNullOrEmpty(ProfileName))
                {

                    using (SqlConnection connection = new SqlConnection(conn))
                    {
                        connection.Open();

                        string getProfileIdQuery = "SELECT id FROM Profiles WHERE profileName = @ProfileName;";
                        using (SqlCommand getProfileIdCommand = new SqlCommand(getProfileIdQuery, connection))
                        {
                            getProfileIdCommand.Parameters.AddWithValue("@ProfileName", ProfileName);
                            int profileId = (int)getProfileIdCommand.ExecuteScalar();

                            string deleteOperatorQuery = "UPDATE Profiles SET operator = NULL WHERE id = @profileId";

                            using (SqlCommand deleteOperatorCommand = new SqlCommand(deleteOperatorQuery, connection))
                            {
                                deleteOperatorCommand.Parameters.AddWithValue("@profileId", profileId);
                                int rowsAffected = deleteOperatorCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Оператор удален из профиля!");
                                    comboBox1.Text = "";
                                }
                                else
                                {
                                    MessageBox.Show("Не удалось удалить оператор из профиля.");
                                }
                            }
                        }
                    }
                }
                    else
                    {
                        MessageBox.Show("Имя профиля не может быть пустым.");
                    }

                }
            else
            {
                MessageBox.Show("Не выбран профиль! Пожалуйста, выберите профиль перед удалением оператора.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void cartesianChart1_ChildChanged(object sender, System.Windows.Forms.Integration.ChildChangedEventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}