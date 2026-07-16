using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Pozdravlyator.Data;
using Pozdravlyator.Models;

namespace Pozdravlyator.Repositories
{
    /// <summary>
    /// Абстракция доступа к данным — позволяет в будущем заменить
    /// реализацию (например, на другую СУБД) без изменения бизнес-логики.
    /// </summary>
    public interface IBirthdayRepository
    {
        List<BirthdayPerson> GetAll();
        BirthdayPerson? GetById(int id);
        void Add(BirthdayPerson person);
        void Update(BirthdayPerson person);
        void Delete(int id);
    }

    /// <summary>
    /// Реализация репозитория поверх Entity Framework Core.
    /// Каждый метод открывает короткоживущий контекст — это простой
    /// и надёжный подход для консольного приложения.
    /// </summary>
    public class BirthdayRepository : IBirthdayRepository
    {
        public List<BirthdayPerson> GetAll()
        {
            using var db = new AppDbContext();
            return db.People.AsNoTracking().ToList();
        }

        public BirthdayPerson? GetById(int id)
        {
            using var db = new AppDbContext();
            return db.People.AsNoTracking().FirstOrDefault(p => p.Id == id);
        }

        public void Add(BirthdayPerson person)
        {
            using var db = new AppDbContext();
            db.People.Add(person);
            db.SaveChanges();
        }

        public void Update(BirthdayPerson person)
        {
            using var db = new AppDbContext();
            db.People.Update(person);
            db.SaveChanges();
        }

        public void Delete(int id)
        {
            using var db = new AppDbContext();
            var entity = db.People.Find(id);
            if (entity != null)
            {
                db.People.Remove(entity);
                db.SaveChanges();
            }
        }
    }
}
