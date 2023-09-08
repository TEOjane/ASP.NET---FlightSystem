using FlightSystem.Data;
using FlightSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FlightSystem.Controllers
{
    public class ChooseSeatController : Controller
    {

        [HttpPost]
        public IActionResult AddSeat(string rdbtn)
        {
            Console.WriteLine("Ура, получили данные");

            Console.WriteLine(rdbtn);

            Ticket ticket = new Ticket();
            var ticketElements = rdbtn.Split(' ');
            ticket.Seat = ticketElements[0];
            ticket.idClient = Convert.ToInt32(ticketElements[1]);
            ticket.idFlight = Convert.ToInt32(ticketElements[2]);

            BookSeat(ticket);

            return RedirectToAction("ShowTicket", "ChooseSeat", ticket);
        }


        public IActionResult Index(Ticket ticket)
        {
            Dictionary<string, bool> seats = TakeSeatCondition(ticket.idFlight);



            ViewBag.Ticket = ticket;

            foreach (var seat in seats)
            {
                Console.WriteLine(seat);
            }

            return View(seats);
        }

        public IActionResult ShowTicket(Ticket ticket) 
        {
            ViewBag.Client = GetClientInfo(ticket.idClient);
            ViewBag.Flight = GetFlightInfo(ticket.idFlight);
            ViewBag.Seat = ticket.Seat;

            return View();
        }



        public Dictionary<string,bool> TakeSeatCondition(int flightID)
        {
            Console.WriteLine("Запуск поиска занятых мест");
            Seats seats = new Seats();

            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                string sqlString = "SELECT \"SeatNumber\" FROM \"Tickets\" WHERE \"FlightId\" = @flightId";

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sqlString, conn))
                {

                    cmd.Parameters.AddWithValue("@flightId", flightID);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (seats.seatsNum.ContainsKey(reader.GetString(0)))
                            {
                                seats.seatsNum[reader.GetString(0)] = false;
                            }
                           
                        }

                        Console.WriteLine("Информация о местах получена");

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }

            return seats.seatsNum;
        }

        public void BookSeat(Ticket ticket)
        {
            Console.WriteLine("Запуск бронирования места");
            Seats seats = new Seats();

            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                string sqlString = "INSERT INTO \"Tickets\" " +
                                   "(\"Id\", \"ClientId\", \"FlightId\", \"SeatNumber\") VALUES " +
                                   "(@Id, @ClientId, @FlightId, @SeatNumber);";

                int ticketId = GetCountOfRowsDB();
                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sqlString, conn))
                {

                    cmd.Parameters.AddWithValue("@Id", ticketId+1);
                    cmd.Parameters.AddWithValue("@ClientId", ticket.idClient);
                    cmd.Parameters.AddWithValue("@FlightId", ticket.idFlight);
                    cmd.Parameters.AddWithValue("@SeatNumber", ticket.Seat);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Билет добавлен в БД");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }
        }

        public int GetCountOfRowsDB()
        {
            //string table = "\"Flight\"";

            int count = 0;

            Console.WriteLine("Поиск количества строк");
            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                string sqlRequest = " SELECT COUNT (\"Id\") FROM \"Tickets\"";

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sqlRequest, conn))
                {

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            count = reader.GetInt32(0);
                        }

                        Console.WriteLine("COUNT из БД получены, COUNT = " + count);
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }
            return count;
        }

        public Flight GetFlightInfo(int flightId)
        {
            Flight flight = new Flight();
            Console.WriteLine("Запуск подключения к БД из GetFlightInfo");
            try
            {
                String connectionString = "Host=service.roninore.ru;Username=ejeviki;Password=5tAbNCVhTvEPE0jjkT9VlzV4cdVL9rUj3YKKYNiCIbjYQ5mAM5;Database=ejeviki";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                string requestSql = "SELECT * FROM \"Flights\" WHERE \"Id\" = @FlightID";

                using (var cmd = new NpgsqlCommand(requestSql, conn))
                {
                    cmd.Parameters.AddWithValue("@FlightID", flightId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flight.Id = reader.GetInt32(0);
                            flight.ArrivalData = reader.GetDateTime(1);
                            flight.DepartureData = reader.GetDateTime(2);
                            flight.ArrivalPlace = reader.GetString(3);
                            flight.DeparturePlace = reader.GetString(4);
                        }
                        Console.WriteLine("Данные из БД получены");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }

            return flight;
        }


        public ClientInfo GetClientInfo(int clientId)
        {
            ClientInfo client = new ClientInfo();
            Console.WriteLine("Запуск подключения к БД из GetClientInfo");
            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                string requestSql = "SELECT * FROM \"Clients\" WHERE \"Id\" = @ClientId";

                using (var cmd = new NpgsqlCommand(requestSql, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientId", clientId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            client.Id = reader.GetInt32(0);
                            client.FirstName = reader.GetString(1);
                            client.SecondName = reader.GetString(2);
                            client.FatherName = reader.GetString(3);
                            client.DateOfBirth = reader.GetString(4);
                            client.FlightId = reader.GetInt32(5);
                            client.Passport = reader.GetString(6);
                            client.Email = reader.GetString(7);
                        }
                        Console.WriteLine("Данные из БД получены");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }

            return client;
        }

    }
}
