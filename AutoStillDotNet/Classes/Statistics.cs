
using System.Data;
using System.Data.SqlClient;
using System;

namespace AutoStillDotNet
{
    class Statistics
    {
        public static DataTable InitializeTable()
        { 
        DataTable StillStats =  new DataTable("StillStats");
        DataColumn column;

        column = new DataColumn();
        column.DataType = System.Type.GetType("System.Int32");
            column.ColumnName = "ID";
            column.AutoIncrement = true;
            column.ReadOnly = true;
            column.Unique = true; 

            StillStats.Columns.Add(column);

            column = new DataColumn();
        column.ColumnName = "Time";
            column.DataType = System.Type.GetType("System.DateTime");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            column = new DataColumn();
        column.ColumnName = "Temperature";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);
            
            column = new DataColumn();
        column.ColumnName = "TemperatureDelta";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);
            column = new DataColumn();
            column.ColumnName = "Pressure";
            column.DataType = System.Type.GetType("System.Decimal");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Phase";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            column = new DataColumn();
            column.ColumnName = "Amperage";
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
        PrimaryKeyColumns[0] = StillStats.Columns["id"];
            StillStats.PrimaryKey = PrimaryKeyColumns;
            return StillStats;
            }

        public static void CreateHeader(DateTime RunStart, DateTime RunEnd, bool RunComplete, string Units)
        {
            DateTime RunDate = RunStart.Date;
            TimeSpan Duration = RunEnd - RunStart;
            int Complete;
            if (RunComplete == true) { Complete = 1; } else { Complete = 0; }

            String Values = ("'"+RunDate.ToShortDateString() + "'" + ", '" + RunStart.ToString() + "'" + ", '" + RunEnd.ToString() + "'" + ", '" + Duration.ToString() + "', " + Complete + ", '" + Units + "'");

            var properties = new SystemProperties();
            SqlConnection sqlConnection = properties.sqlConnection;
            using (var command = new SqlCommand("InsertTable") { CommandType = CommandType.Text })
            {
                command.CommandText = "insert into RunHeaders (rhDate, rhStart, rhEnd, rhDuration, rhComplete, rhUnits) values ("+ Values +")";
                command.Connection = sqlConnection;
                sqlConnection.Open();
                command.ExecuteNonQuery();
                sqlConnection.Close();
            }
        }

        public static void SaveRun(DataTable RunData, DateTime RunStart)
        {
            //Stuff to get the connection string
            var properties = new SystemProperties();
            SqlConnection sqlConnection = properties.sqlConnection;

            //Variable to hold the run header ID to make sure the records are linked to the run properly
            int HeaderID;

            //Open the connection, get the header ID and then insert the table
            sqlConnection.Open();
            using (var command = new SqlCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = "select max(rhID) from runheaders where rhStart = '" + RunStart + "'";
                command.Connection = sqlConnection;
                HeaderID = Convert.ToInt32(command.ExecuteScalar());
            }

            //Command to save the table using stored procedure InsertRunRecord
            using (var command = new SqlCommand("InsertRunRecord") { CommandType = CommandType.StoredProcedure })
            {
                command.Parameters.Add(new SqlParameter("@runrecordtype", RunData));
                command.Parameters.Add(new SqlParameter("@RHID", HeaderID));
                command.Connection = sqlConnection;
                command.ExecuteNonQuery();
            }
            sqlConnection.Close();
        }
}
}
