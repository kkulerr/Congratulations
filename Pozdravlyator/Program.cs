using Pozdravlyator.Data;
using Pozdravlyator.Repositories;
using Pozdravlyator.Services;
using Pozdravlyator.UI;

// Создаём базу данных (файл birthdays.db), если она ещё не существует.
using (var db = new AppDbContext())
{
    db.Database.EnsureCreated();
}

var repository = new BirthdayRepository();
var service = new BirthdayService(repository);
var menu = new ConsoleMenu(service);

menu.Run();
