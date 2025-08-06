using Npgsql;
using System.Data;

namespace CRUDdemo.Models
{
    public class EmployeeDAL
    {
        string _connectionString = "Host=localhost;Database=employee;Username=postgres;Password=20050725";
        private ChildDAL _childDAL = new ChildDAL();

        public IEnumerable<Employee> GetAllEmployee()
        {
            List<Employee> employees = new List<Employee>();
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                string query = "SELECT * FROM sp_getallemployee()";
                NpgsqlCommand cmd = new NpgsqlCommand(query, con);
                cmd.CommandType = CommandType.Text;

                con.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employee employee = new Employee
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Gender = reader["Gender"].ToString(),
                            Company = reader["Company"].ToString(),
                            Department = reader["Department"].ToString(),
                            IsMarried = Convert.ToBoolean(reader["IsMarried"])
                        };
                        employees.Add(employee);
                    }
                }
            }

            foreach (var employee in employees)
            {
                employee.Children = _childDAL.GetChildrenByEmployeeId(employee.Id).ToList();
            }

            return employees;
        }

        public void AddEmployee(Employee employee)
        {
            int employeeId = 0;

            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand cmd = new NpgsqlCommand("sp_insertemployee_returnid", con, transaction))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;

                            cmd.Parameters.AddWithValue("p_name", employee.Name);
                            cmd.Parameters.AddWithValue("p_gender", employee.Gender);
                            cmd.Parameters.AddWithValue("p_company", employee.Company);
                            cmd.Parameters.AddWithValue("p_department", employee.Department);
                            cmd.Parameters.AddWithValue("p_ismarried", employee.IsMarried);

                            var result = cmd.ExecuteScalar();
                            employeeId = Convert.ToInt32(result);
                        }

                        if (employee.IsMarried && employee.Children != null && employee.Children.Any())
                        {
                            foreach (var child in employee.Children)
                            {
                                if (!string.IsNullOrWhiteSpace(child.Name) &&
                                    !string.IsNullOrWhiteSpace(child.Gender) &&
                                    child.Age > 0)
                                {
                                    using (NpgsqlCommand childCmd = new NpgsqlCommand("sp_insertchild", con, transaction))
                                    {
                                        childCmd.CommandType = CommandType.StoredProcedure;

                                        childCmd.Parameters.AddWithValue("p_employee_id", employeeId);
                                        childCmd.Parameters.AddWithValue("p_name", child.Name);
                                        childCmd.Parameters.AddWithValue("p_age", child.Age);
                                        childCmd.Parameters.AddWithValue("p_gender", child.Gender);

                                        childCmd.ExecuteNonQuery();
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void AddEmployeeWithChildren(Employee employee)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand("sp_insertemployee", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("p_name", employee.Name);
                    cmd.Parameters.AddWithValue("p_gender", employee.Gender);
                    cmd.Parameters.AddWithValue("p_company", employee.Company);
                    cmd.Parameters.AddWithValue("p_department", employee.Department);
                    cmd.Parameters.AddWithValue("p_ismarried", employee.IsMarried);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            int employeeId = GetLastInsertedEmployeeId();

            if (employee.IsMarried && employee.Children != null && employee.Children.Any())
            {
                foreach (var child in employee.Children)
                {
                    if (!string.IsNullOrWhiteSpace(child.Name) &&
                        !string.IsNullOrWhiteSpace(child.Gender) &&
                        child.Age > 0)
                    {
                        child.EmployeeId = employeeId;
                        _childDAL.AddChild(child);
                    }
                }
            }
        }

        private int GetLastInsertedEmployeeId()
        {
            int employeeId = 0;
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                string query = "SELECT MAX(id) FROM employee";
                NpgsqlCommand cmd = new NpgsqlCommand(query, con);

                con.Open();
                var result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    employeeId = Convert.ToInt32(result);
                }
            }
            return employeeId;
        }

        public void UpdateEmployee(Employee employee)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                var cmd = new NpgsqlCommand("sp_updateemployee", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", employee.Id);
                cmd.Parameters.AddWithValue("p_name", employee.Name);
                cmd.Parameters.AddWithValue("p_gender", employee.Gender);
                cmd.Parameters.AddWithValue("p_company", employee.Company);
                cmd.Parameters.AddWithValue("p_department", employee.Department);
                cmd.Parameters.AddWithValue("p_is_married", employee.IsMarried);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void DeleteEmployee(int? id)
        {
            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                NpgsqlCommand cmd = new NpgsqlCommand("sp_deleteemployee", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_id", id);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public Employee GetEmployeeById(int? id)
        {
            Employee employee = new Employee();

            using (NpgsqlConnection con = new NpgsqlConnection(_connectionString))
            {
                string query = "SELECT * FROM sp_getemployeebyid(@p_id)";
                NpgsqlCommand cmd = new NpgsqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("p_id", id);

                con.Open();
                using (NpgsqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        employee.Id = Convert.ToInt32(reader["Id"]);
                        employee.Name = reader["Name"].ToString();
                        employee.Gender = reader["Gender"].ToString();
                        employee.Company = reader["Company"].ToString();
                        employee.Department = reader["Department"].ToString();
                        employee.IsMarried = Convert.ToBoolean(reader["IsMarried"]);
                    }
                }
            }

            employee.Children = _childDAL.GetChildrenByEmployeeId(employee.Id).ToList();

            return employee;
        }
    }
}