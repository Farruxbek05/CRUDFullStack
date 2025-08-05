using Npgsql;
using System.Data;

namespace CRUDdemo.Models
{
    public class ChildDAL
    {
        string _connectionString = "Host=localhost;Database=employee;Username=postgres;Password=20050725";

        public IEnumerable<Child> GetChildrenByEmployeeId(int employeeId)
        {
            List<Child> children = new List<Child>();
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                string query = "SELECT * FROM sp_getchildrenbyemployeeid(@p_employee_id)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("p_employee_id", employeeId);

                con.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Child child = new Child
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                            Name = reader["Name"].ToString(),
                            Age = Convert.ToInt32(reader["Age"]),
                            Gender = reader["Gender"].ToString()
                        };
                        children.Add(child);
                    }
                }
            }
            return children;
        }

        public void AddChild(Child child)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand("sp_insertchild", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("p_employee_id", child.EmployeeId);
                    cmd.Parameters.AddWithValue("p_name", child.Name);
                    cmd.Parameters.AddWithValue("p_age", child.Age);
                    cmd.Parameters.AddWithValue("p_gender", child.Gender);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateChild(Child child)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                var cmd = new NpgsqlCommand("sp_updatechild", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", child.Id);
                cmd.Parameters.AddWithValue("p_employee_id", child.EmployeeId);
                cmd.Parameters.AddWithValue("p_name", child.Name);
                cmd.Parameters.AddWithValue("p_age", child.Age);
                cmd.Parameters.AddWithValue("p_gender", child.Gender);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteChild(int id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("sp_deletechild", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public Child GetChildById(int id)
        {
            Child child = new Child();

            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                string query = "SELECT * FROM sp_getchildbyid(@p_id)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("p_id", id);

                con.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        child.Id = Convert.ToInt32(reader["Id"]);
                        child.EmployeeId = Convert.ToInt32(reader["EmployeeId"]);
                        child.Name = reader["Name"].ToString();
                        child.Age = Convert.ToInt32(reader["Age"]);
                        child.Gender = reader["Gender"].ToString();
                    }
                }
            }

            return child;
        }
    }
}