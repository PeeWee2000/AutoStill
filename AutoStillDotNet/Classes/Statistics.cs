
using System.Data;

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
            column.DataType = System.Type.GetType("System.Int32");
            column.ReadOnly = false;
            column.Unique = false;

            StillStats.Columns.Add(column);

            DataColumn[] PrimaryKeyColumns = new DataColumn[1];
        PrimaryKeyColumns[0] = StillStats.Columns["id"];
            StillStats.PrimaryKey = PrimaryKeyColumns;
            return StillStats;
            }
}
}
