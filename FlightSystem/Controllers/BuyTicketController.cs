using FlightSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Globalization;
using System.Text.RegularExpressions;

namespace FlightSystem.Controllers
{
    public class BuyTicketController : Controller
    {
        int idFlights;

        static String connectionString = "string for connection with server";
        static NpgsqlDataSourceBuilder dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        NpgsqlDataSource dataSource = dataSourceBuilder.Build();

        public List<String> errorMessage = new List<String>();

        [HttpGet]
        public IActionResult Index(int id)
        {
            idFlights = id;
            ViewBag.SelectedFlight = GetSelectedFlight(id);
            ViewBag.Errors = errorMessage;

            ClientInfo clientInfo = new ClientInfo();
            clientInfo.FlightId = id;

            return View(clientInfo);
        }

        //Отправляет данные клиента
        [HttpPost]
        public IActionResult Index(ClientInfo clientInfo)
        {
            Console.WriteLine("Запуск HttpPost BuyTicket");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("Проверка на валидность");

                ViewBag.SelectedFlight = GetSelectedFlight(clientInfo.FlightId);
                ViewBag.Errors = errorMessage;

                return View(clientInfo);
            }

            bool firstName = Regex.IsMatch(clientInfo.FirstName, @"^[\p{IsCyrillic}]+$");
            bool secondName = Regex.IsMatch(clientInfo.SecondName, @"^[\p{IsCyrillic}]+$");
            bool fatherName = Regex.IsMatch(clientInfo.FatherName, @"^[\p{IsCyrillic}]+$");
            bool passport = Regex.IsMatch(clientInfo.Passport, @"^\d{10}$");
            bool dateBirth = IsDateBeforeOrToday(clientInfo.DateOfBirth);

            

            if (firstName && secondName && fatherName && passport && dateBirth)
            {
                AddNewClientToDB(clientInfo);

                Ticket ticket = new Ticket();

                ticket.idClient = AddNewClientToDB(clientInfo);
                ticket.idFlight = clientInfo.FlightId;

                return RedirectToAction("Index", "ChooseSeat", ticket);
            }
            
            if (!firstName)
            {
                errorMessage.Add("Некорректное имя! Имя может содержать только буквы!");
            }
            if (!secondName)
            {
                errorMessage.Add("Некорректная фамилия! Фамилия может содержать только буквы!");
            }
            if (!fatherName)
            {
                errorMessage.Add("Некорректное отчество! Отчество может содержать только буквы!");
            }
            if (!passport)
            {
                errorMessage.Add("Некорректный паспорт!");
            }
            if (!dateBirth)
            {
                errorMessage.Add("Некорректная дата! Введеная дата превышает текущую!");
            }

            ViewBag.Errors = errorMessage;
            ViewBag.SelectedFlight = GetSelectedFlight(clientInfo.FlightId);
            return View(clientInfo);
        }

        public int AddNewClientToDB(ClientInfo clientInfo)
        {
            Console.WriteLine("Запуск подключения к БД из GetSelectedFlight");

            int lastId = 1;
            try
            {
                var conn = dataSource.OpenConnection();

                string sqlString = "INSERT INTO \"Clients\" " +
                                   "(\"Id\", \"FirstName\", \"LastName\", \"FatherName\", \"DateOfBirth\", \"FlightID\", \"Passport\", \"Email\") VALUES " +
                                   "(@Id, @FirstName, @SecondName, @FatherName, @DateOfBirth, @FlightId, @Passport, @email);";

                lastId = GetCountOfRowsDB();

                DateTime myDate = DateTime.ParseExact(clientInfo.DateOfBirth, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
                // Retrieve all rows
                using (var command = new NpgsqlCommand(sqlString, conn))
                {
                    //cmd.Parameters.AddWithValue("@Id", id);
                    command.Parameters.AddWithValue("@Id", lastId + 1);
                    command.Parameters.AddWithValue("@FirstName", clientInfo.FirstName);
                    command.Parameters.AddWithValue("@SecondName", clientInfo.SecondName);
                    command.Parameters.AddWithValue("@FatherName", clientInfo.FatherName);
                    command.Parameters.AddWithValue("@email", clientInfo.Email);
                    command.Parameters.AddWithValue("@Passport", clientInfo.Passport);
                    command.Parameters.AddWithValue("@FlightId", clientInfo.FlightId);
                    command.Parameters.AddWithValue("@DateOfBirth", myDate);

                    command.ExecuteNonQuery();
                    Console.WriteLine("Клиент добавлен в БД");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка на выводе" + ex.Message);
            }

            return lastId;
        }


        public int GetCountOfRowsDB()
        {
            //string table = "\"Flight\"";

            int count = 0;

            Console.WriteLine("Поиск количества строк");
            try
            {
                var conn = dataSource.OpenConnection();

                string sqlRequest = " SELECT COUNT (\"Id\") FROM \"Clients\"";

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

        public Flight GetSelectedFlight(int id)
        {
            Flight flight = new Flight();
            Console.WriteLine("Запуск подключения к БД из GetSelectedFlight");
            try
            {
                var conn = dataSource.OpenConnection();

                string sqlString = " SELECT * FROM \"Flights\" WHERE \"Id\" = @Id";

                // Retrieve all rows
                using (var cmd = new NpgsqlCommand(sqlString, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

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


        public static bool IsDateBeforeOrToday(string input)
        {
            DateTime pDate;
            pDate = DateTime.ParseExact(input, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

            return DateTime.Today >= pDate;
        }
    }
}
