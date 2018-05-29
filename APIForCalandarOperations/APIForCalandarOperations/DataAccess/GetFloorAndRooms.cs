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

        public IList<Room> GetAllRooms(string connKey)
        {
            IList<Room> roomList = new List<Room>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM ROOM"))
                {
                    Room room;
                    while (reader.Read())
                    {
                        room = new Room()
                        {
                            Id = SqlHelper.To<int>(reader["ID"], 0),
                            Name = SqlHelper.To<string>(reader["RoomName"], string.Empty),
                            Email = SqlHelper.To<string>(reader["RoomEmail"], string.Empty)

                        };
                        roomList.Add(room);
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
            return roomList;
        }

        public IList<Room> GetRoomsByID(string connKey, int roomID)
        {
            IList<Room> roomList = new List<Room>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM ROOM WHERE ID = "+ roomID))
                {
                    Room room;
                    while (reader.Read())
                    {
                        room = new Room()
                        {
                            Id = SqlHelper.To<int>(reader["ID"], 0),
                            Name = SqlHelper.To<string>(reader["RoomName"], string.Empty),
                            Email = SqlHelper.To<string>(reader["RoomEmail"], string.Empty)

                        };
                        roomList.Add(room);
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
            return roomList;
        }


        public IList<Room> GetRoomsByFloorID(string connKey, int floorID)
        {
            IList<Room> roomList = new List<Room>();
            SqlConnection connection = new SqlConnection(SqlHelper.GetDBConnectionString(connKey));
            try
            {
                using (SqlDataReader reader = SqlHelper.ExecuteReader(connection, CommandType.Text, "SELECT * FROM ROOM WHERE FloorID = " + floorID))
                {
                    Room room;
                    while (reader.Read())
                    {
                        room = new Room()
                        {
                            Id = SqlHelper.To<int>(reader["ID"], 0),
                            Name = SqlHelper.To<string>(reader["RoomName"], string.Empty),
                            Email = SqlHelper.To<string>(reader["RoomEmail"], string.Empty)

                        };
                        roomList.Add(room);
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
            return roomList;
        }
    }
}