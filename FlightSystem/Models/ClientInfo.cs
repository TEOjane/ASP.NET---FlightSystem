using System.ComponentModel.DataAnnotations;

namespace FlightSystem.Models
{
    public class ClientInfo
    {
        public int Id;

        [Required(ErrorMessage = "Введите имя")]
        public string FirstName { get; set; } = null!;

        [Required(ErrorMessage = "Введите фамилию")]
        public string SecondName { get; set; } = null!;


        [EmailAddress]
        public string Email { get; set; } 

        [Required(ErrorMessage = "Введите отчество")]
        public string FatherName { get; set; } = null!;

        public int FlightId { get; set; }

        [Required(ErrorMessage = "Введите паспортные данные")] 
        public string Passport { get; set; } = null!;
        //public string Sex;

        [Required(ErrorMessage = "Введите дату рождения")]
        public string DateOfBirth { get; set; }

    }
}
