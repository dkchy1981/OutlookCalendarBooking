using APIForCalandarOperations.Models;
using APIForCalandarOperations.MSSQL.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace APIForCalandarOperations.DataAccess
{
    public class GetFloorAndRooms
    {
        public GetFloorAndRooms()
        {

        }

        public IList<Floor> GetFloorList(string connKey)
        {
            IList<Floor> floorList = new List<Floor>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM FLOORLIST"))
                {
                    Floor floor;
                    while (reader.Read())
                    {
                        floor = new Floor()
                        {
                            Id = SqlHelper.To<int>(reader["Id"], 0),
                            Name = SqlHelper.To<string>(reader["FloorName"], string.Empty)
                        };
                        floorList.Add(floor);
                    }
                }
            }
            finally
            {
                if (connection.State != ConnectionState.Closed)
                {
                    connection.Close();
                }
            }
            return floorList;
        }
    }
}