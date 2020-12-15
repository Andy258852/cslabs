using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SqlManagerLib
{
    public class SqlManager
    {
        private static string ConnectionString { get; set; }

        public SqlManager(string sqlServer, string dbName, bool integratedSecurity)
        {
            ConnectionString = @"Data Source=" + sqlServer + ";Initial Catalog=" + dbName + ";Integrated Security=" + integratedSecurity;
        }

        public void InsertUser(string FirstName, string LastName, bool Gender, int Age, string Nationality, string Email, string PhoneNumber, string Country, string Company, string Password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("dbo.InsertUser", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FirstName", FirstName);
                    command.Parameters.AddWithValue("@LastName", LastName);
                    command.Parameters.AddWithValue("@Gender", Gender);
                    command.Parameters.AddWithValue("@Age", Age);
                    command.Parameters.AddWithValue("@Nationality", Nationality);
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                    command.Parameters.AddWithValue("@Country", Country);
                    command.Parameters.AddWithValue("@Company", Company);
                    command.Parameters.AddWithValue("@Password", Password);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void EditUser(string OldEmail, string OldPassword, string FirstName, string LastName, bool Gender, int Age, string Nationality, string Email, string PhoneNumber, string Country, string Company, string Password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("dbo.UpdateUser", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@FirstName", FirstName);
                    command.Parameters.AddWithValue("@LastName", LastName);
                    command.Parameters.AddWithValue("@Gender", Gender);
                    command.Parameters.AddWithValue("@Age", Age);
                    command.Parameters.AddWithValue("@Nationality", Nationality);
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@PhoneNumber", PhoneNumber);
                    command.Parameters.AddWithValue("@Country", Country);
                    command.Parameters.AddWithValue("@Company", Company);
                    command.Parameters.AddWithValue("@Password", Password);
                    command.Parameters.AddWithValue("@OldEmail", OldEmail);
                    command.Parameters.AddWithValue("@OldPassword", OldPassword);
                    command.ExecuteNonQuery();
                }
            }
        }

        public bool CompanyExists(string company)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[sp_GetCompanyID]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", company);
                    return command.ExecuteScalar() != null;
                }
            }
        }

        public bool CountryExists(string country)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[GetCountryRegion]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", country);
                    return command.ExecuteScalar() != null;
                }
            }
        }

        public bool NationalityExists(string Nat)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[GetNationalityID]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Name", Nat);
                    return command.ExecuteScalar() != null;
                }
            }
        }

        public object[] GetUserInfo(string Email, string Password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[GetUserInfo]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@Password", Password);
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            object[] values = new object[dataReader.FieldCount];
                            while (dataReader.Read())
                            {
                                dataReader.GetValues(values);
                            }
                            return values;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public object[][] GetLastCommentInfo(System.DateTime date)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[GetLastCommentInfo]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@datetime", date);
                    DataTable dt = new DataTable();
                    using (SqlDataReader dataReader1 = command.ExecuteReader())
                    {
                        dt.Load(dataReader1);
                    }
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            object[][] values = new object[dt.Rows.Count][];
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                values[j] = new object[dataReader.FieldCount];
                            }
                            int i = 0;
                            while (dataReader.Read())
                            {
                                dataReader.GetValues(values[i]);
                                i++;
                            }
                            return values;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }

        public bool EmailExists(string email)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[GetNationalityID]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", email);
                    return command.ExecuteScalar() != null;
                }
            }
        }

        public void MakeComment(string email, string password, string text)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("dbo.MakeComment", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Text", text);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    command.ExecuteNonQuery();
                }
            }
        }

        public object[] GetComments(string email, string password)
        {
            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand("[dbo].[GetComments]", connection))
                {
                    command.CommandType = System.Data.CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);
                    using (SqlDataReader dataReader = command.ExecuteReader())
                    {
                        if (dataReader.HasRows)
                        {
                            List<object> list = new List<object>();
                            while (dataReader.Read())
                            {
                                list.Add(dataReader.GetValue(0));
                            }
                            return list.ToArray();
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
    }
}
