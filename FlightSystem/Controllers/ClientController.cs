using FlightSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace FlightSystem.Controllers
{
    public class ClientController : Controller
    {
        public List<Flight> Flights = new List<Flight>();
        public Flight selectedFlight = new Flight();
        String requestSql = " SELECT * FROM \"Flights\" ORDER BY \"ArrivalData\"";


        //Сначала все IActionResult
        public IActionResult Index()
        {
            //requestSql = " SELECT * FROM \"Flights\" ";
            GetAllFlightsInfo();
            Console.WriteLine($"requestSql = {requestSql}");

            ViewBag.Flights = Flights;
            string sqlString = "SELECT DISTINCT \"ArrivalPlace\" FROM \"Flights\" ORDER BY \"ArrivalPlace\"";
            ViewBag.ArrivalCities = GetAllCities(sqlString);
            sqlString = "SELECT DISTINCT \"DeparturePlace\" FROM \"Flights\" ORDER BY \"DeparturePlace\"";
            ViewBag.DepartureCities = GetAllCities(sqlString);

            return View();
        }


        [HttpGet]
        public IActionResult SearchFlight()
        {
            Console.WriteLine("Запуск HttpGet SearchFlight");
            Flight searchFlight = new Flight();

            try
            {
                searchFlight.ArrivalData = DateTime.ParseExact(Request.Query["selectedToDate"], "yyyy-MM-dd",
                                       System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                searchFlight.ArrivalData= DateTime.MinValue;
            }

           Console.WriteLine("searchFlight.ArrivalData = " + searchFlight.ArrivalData);

            searchFlight.ArrivalPlace = Request.Query["selectedArrivalPlace"];
            searchFlight.DeparturePlace = Request.Query["selectedDeparturePlace"];

            string dateString = searchFlight.ArrivalData.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

            requestSql = $"SELECT * FROM \"Flights\" WHERE DATE(\"ArrivalData\") = '{dateString}'::date AND \"ArrivalPlace\" = '{searchFlight.ArrivalPlace}' AND \"DeparturePlace\" = '{searchFlight.DeparturePlace}' ORDER BY \"ArrivalData\"";
            GetAllFlightsInfo();
            ViewBag.Flights = Flights;

            string sqlString = "SELECT DISTINCT \"ArrivalPlace\" FROM \"Flights\"";
            ViewBag.ArrivalCities = GetAllCities(sqlString);
            sqlString = "SELECT DISTINCT \"DeparturePlace\" FROM \"Flights\"";
            ViewBag.DepartureCities = GetAllCities(sqlString);

            return View("Index");
        }

        //Пошли функции
        public void GetAllFlightsInfo()
        {
            Console.WriteLine("Запуск подключения к БД из GetAllFlightsInfo");
            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                using (var cmd = new NpgsqlCommand(requestSql, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Flight flight = new Flight();

                            flight.Id = reader.GetInt32(0);

                            flight.isSoldOut = IsSoldOut(flight.Id);

                            flight.ArrivalData = reader.GetDateTime(1);
                            flight.DepartureData = reader.GetDateTime(2);
                            flight.ArrivalPlace = reader.GetString(3);
                            flight.DeparturePlace = reader.GetString(4);

                            if (IsFlightNotOverdue(flight.ArrivalData))
                            {
                                Flights.Add(flight);
                            }
                        }
                        Console.WriteLine("Данные из БД получены");
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }
        }

        public List<String> GetAllCities(string sqlString)
        {
            List<String> Cities = new List<String>();

            Console.WriteLine("Запуск подключения к БД из GetAllFlightsInfo");
            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sqlString, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Cities.Add(reader.GetString(0));
                        }

                        Console.WriteLine("Данные из БД получены");

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }

            return Cities;
        }


        public bool IsSoldOut (int flightId)
        {
            Console.WriteLine("Запуск проверки на наличие свободных посадочных мест");
            int countOfBookedSeat = 0;
            try
            {
                String connectionString = "string for connection with server";

                var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
                var dataSource = dataSourceBuilder.Build();

                var conn = dataSource.OpenConnection();

                string sqlRequest = "SELECT COUNT(\"ClientId\") FROM \"Tickets\"\r\nGROUP BY \"FlightId\"\r\nHAVING \"FlightId\" = @FlightId";

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sqlRequest, conn))
                {
                    cmd.Parameters.AddWithValue("@FlightId", flightId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            countOfBookedSeat = reader.GetInt32(0);
                        }

                        Console.WriteLine("Данные из БД получены");

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }

            Console.WriteLine("Количество занятых мест на рейсе номер " + flightId + " равно " + countOfBookedSeat);
            if (countOfBookedSeat >= 60)
            {
                return true;
            }

            return false;
        }


        public static bool IsFlightNotOverdue(DateTime dateTime)
        {
            return DateTime.Today <= dateTime;
        }


    }
}
