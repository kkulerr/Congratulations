using System;

namespace Pozdravlyator.Models
{
    /// <summary>
    /// Модель именинника: хранит основные данные и вычисляемые поля,
    /// связанные с датой ближайшего дня рождения.
    /// </summary>
    public class BirthdayPerson
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Notes { get; set; }

        /// <summary>
        /// Возраст на текущий момент.
        /// </summary>
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age))
                    age--;
                return age;
            }
        }

        /// <summary>
        /// Дата ближайшего дня рождения (в этом году, если ещё не прошёл,
        /// иначе — в следующем). Корректно обрабатывает 29 февраля.
        /// </summary>
        public DateTime NextBirthday
        {
            get
            {
                var today = DateTime.Today;
                var next = SafeDate(today.Year, BirthDate.Month, BirthDate.Day);
                if (next < today)
                    next = SafeDate(today.Year + 1, BirthDate.Month, BirthDate.Day);
                return next;
            }
        }

        /// <summary>
        /// Сколько дней осталось до ближайшего дня рождения (0 — сегодня).
        /// </summary>
        public int DaysUntilNextBirthday => (NextBirthday - DateTime.Today).Days;

        /// <summary>
        /// True, если день рождения сегодня.
        /// </summary>
        public bool IsToday => BirthDate.Month == DateTime.Today.Month
                                && BirthDate.Day == DateTime.Today.Day;

        private static DateTime SafeDate(int year, int month, int day)
        {
            // Защита от 29 февраля в невисокосный год — переносим на 28 февраля.
            var daysInMonth = DateTime.DaysInMonth(year, month);
            if (day > daysInMonth)
                day = daysInMonth;
            return new DateTime(year, month, day);
        }
    }
}
