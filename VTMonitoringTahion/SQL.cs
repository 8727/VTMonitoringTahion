using System.Data.SqlClient;
using System.Data;

namespace VTMonitoringTahion
{
    internal class SQL
    {
        static string connectionString = $@"Data Source={Service.sqlSource};Initial Catalog=VTEventLogApplication;User Id={Service.sqlUser};Password={Service.sqlPassword};Connection Timeout=60";

        static object SQLQuery(string query)
        {
            object response = -1;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    response = command.ExecuteScalar();
                }
                catch (SqlException)
                {
                    connection.Close();
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                    }
                }
            }
            return response;
        }

        public static string ViewCameraSources(string ch)
        {
            string sqlQuery = $"SELECT ID FROM VTEventLogApplication.dbo.SOURCES WHERE SOURCE_ID = '{ch}'";
            return SQLQuery(sqlQuery).ToString(); ;
        }

        public static string ViewCameraStatus(string id)
        {
            string sqlQuery = $"SELECT TOP(1) EVENTTYPE_ID FROM VTEventLogApplication.dbo.EVENTS WHERE SOURCE_ID = {id} AND EVENTTYPE_ID = 175 OR SOURCE_ID = {id} AND EVENTTYPE_ID = 177 OR SOURCE_ID = {id} AND EVENTTYPE_ID = 66 OR SOURCE_ID = {id} AND EVENTTYPE_ID = 67 ORDER BY KEY_ID DESC";
            string result = "0";
            var otvet =  SQLQuery(sqlQuery);
            if (otvet != null)
            {
                if (otvet.ToString() == "175" || otvet.ToString() == "66")
                {
                    result = "1";
                }
            }
            return result;
        }
    }
}
