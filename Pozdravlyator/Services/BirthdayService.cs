using System;
using System.Collections.Generic;
using System.Linq;
using Pozdravlyator.Models;
using Pozdravlyator.Repositories;

namespace Pozdravlyator.Services
{
    public enum SortOrder
    {
        ByName,
        ByDate,
        ByDaysLeft
    }

    /// <summary>
    /// Бизнес-логика приложения: фильтрация, сортировка, валидация
    /// и операции над записями. Не зависит от способа хранения данных
    /// (работает через интерфейс репозитория) и от способа отображения.
    /// </summary>
    public class BirthdayService
    {
        private readonly IBirthdayRepository _repository;
        private const int UpcomingDaysWindow = 7;

        public BirthdayService(IBirthdayRepository repository)
        {
            _repository = repository;
        }

        public List<BirthdayPerson> GetAll(SortOrder sort = SortOrder.ByDaysLeft)
        {
            var list = _repository.GetAll();
            return Sort(list, sort);
        }

        public List<BirthdayPerson> GetTodayAndUpcoming(SortOrder sort = SortOrder.ByDaysLeft)
        {
            var list = _repository.GetAll()
                .Where(p => p.DaysUntilNextBirthday <= UpcomingDaysWindow)
                .ToList();
            return Sort(list, sort);
        }

        private static List<BirthdayPerson> Sort(List<BirthdayPerson> list, SortOrder sort)
        {
            return sort switch
            {
                SortOrder.ByName => list.OrderBy(p => p.FullName).ToList(),
                SortOrder.ByDate => list.OrderBy(p => p.BirthDate.Month).ThenBy(p => p.BirthDate.Day).ToList(),
                _ => list.OrderBy(p => p.DaysUntilNextBirthday).ToList()
            };
        }

        public (bool IsValid, string ErrorMessage) Validate(string? fullName, DateTime birthDate, string? phone)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                return (false, "Имя не может быть пустым.");

            if (fullName.Trim().Length > 200)
                return (false, "Имя слишком длинное (максимум 200 символов).");

            if (birthDate.Date > DateTime.Today)
                return (false, "Дата рождения не может быть в будущем.");

            if (birthDate < new DateTime(1900, 1, 1))
                return (false, "Дата рождения выглядит некорректной (раньше 1900 года).");

            if (!string.IsNullOrWhiteSpace(phone) && phone.Trim().Length > 50)
                return (false, "Номер телефона слишком длинный.");

            return (true, string.Empty);
        }

        public BirthdayPerson Add(string fullName, DateTime birthDate, string? phone, string? notes)
        {
            var person = new BirthdayPerson
            {
                FullName = fullName.Trim(),
                BirthDate = birthDate.Date,
                PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
                Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim()
            };
            _repository.Add(person);
            return person;
        }

        public bool Update(int id, string fullName, DateTime birthDate, string? phone, string? notes)
        {
            var existing = _repository.GetById(id);
            if (existing == null)
                return false;

            existing.FullName = fullName.Trim();
            existing.BirthDate = birthDate.Date;
            existing.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
            existing.Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();

            _repository.Update(existing);
            return true;
        }

        public bool Delete(int id)
        {
            var existing = _repository.GetById(id);
            if (existing == null)
                return false;

            _repository.Delete(id);
            return true;
        }

        public BirthdayPerson? GetById(int id) => _repository.GetById(id);
    }
}
