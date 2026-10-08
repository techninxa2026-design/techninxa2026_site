using System.ComponentModel.DataAnnotations;

namespace techninxa.Models
{
    public enum Weekday
    {
        Saturday,
        Sunday,
        Monday,
        Tuesday,
        Wednesday,
        Thursday,
        Friday
    }
    public class HolidaySetting
    {
        public int Id { get; set; }

        [Required]
        public Weekday Day { get; set; }

        public bool IsHoliday { get; set; }

    }
    
        public class HolidayDayViewModel
        {
            public Weekday Day { get; set; }

            public bool IsHoliday { get; set; }
        }
    
}
