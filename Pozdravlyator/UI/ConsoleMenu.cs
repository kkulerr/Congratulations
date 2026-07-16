using System;
using System.Collections.Generic;
using System.Globalization;
using Pozdravlyator.Models;
using Pozdravlyator.Services;

namespace Pozdravlyator.UI
{
    /// <summary>
    /// Презентационный слой: консольное меню, ввод/вывод данных,
    /// цветовое выделение. Не содержит бизнес-логики — только
    /// обращается к сервису.
    /// </summary>
    public class ConsoleMenu
    {
        private readonly BirthdayService _service;

        public ConsoleMenu(BirthdayService service)
        {
            _service = service;
        }

        public void Run()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            PrintHeader();
            ShowTodayAndUpcoming();

            var exit = false;
            while (!exit)
            {
                PrintMenu();
                var choice = Console.ReadLine();
                Console.WriteLine();

                switch (choice)
                {
                    case "1":
                        ShowAll();
                        break;
                    case "2":
                        ShowTodayAndUpcoming();
                        break;
                    case "3":
                        AddPerson();
                        break;
                    case "4":
                        EditPerson();
                        break;
                    case "5":
                        DeletePerson();
                        break;
                    case "0":
                        exit = true;
                        break;
                    default:
                        PrintError("Неверный пункт меню. Попробуйте снова.");
                        break;
                }
            }

            Console.WriteLine("До встречи!");
        }

        private void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("========================================");
            Console.WriteLine("        П О З Д Р А В Л Я Т О Р        ");
            Console.WriteLine("========================================");
            Console.ResetColor();
            Console.WriteLine();
        }

        private void PrintMenu()
        {
            Console.WriteLine();
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("1 - Показать весь список ДР");
            Console.WriteLine("2 - Показать сегодняшние и ближайшие ДР");
            Console.WriteLine("3 - Добавить запись");
            Console.WriteLine("4 - Редактировать запись");
            Console.WriteLine("5 - Удалить запись");
            Console.WriteLine("0 - Выход");
            Console.WriteLine("----------------------------------------");
            Console.Write("Выберите пункт меню: ");
        }

        private void ShowAll()
        {
            var sort = AskSortOrder();
            var list = _service.GetAll(sort);
            PrintTable(list, "Полный список дней рождения");
        }

        private void ShowTodayAndUpcoming()
        {
            var list = _service.GetTodayAndUpcoming(SortOrder.ByDaysLeft);
            PrintTable(list, "Сегодняшние и ближайшие дни рождения (в пределах 7 дней)");
        }

        private SortOrder AskSortOrder()
        {
            Console.WriteLine("Сортировка: 1 - по имени, 2 - по дате рождения, Enter - по числу дней до ДР");
            var input = Console.ReadLine();
            return input switch
            {
                "1" => SortOrder.ByName,
                "2" => SortOrder.ByDate,
                _ => SortOrder.ByDaysLeft
            };
        }

        private void PrintTable(List<BirthdayPerson> people, string title)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(title);
            Console.ResetColor();

            if (people.Count == 0)
            {
                Console.WriteLine("Список пуст.");
                return;
            }

            Console.WriteLine($"{"Id",-4} {"Имя",-25} {"Дата рождения",-15} {"Телефон",-15} {"Через дней",-10}");
            Console.WriteLine(new string('-', 75));

            foreach (var p in people)
            {
                if (p.IsToday)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (p.DaysUntilNextBirthday <= 3)
                    Console.ForegroundColor = ConsoleColor.Magenta;
                else
                    Console.ResetColor();

                var status = p.IsToday ? "СЕГОДНЯ!" : p.DaysUntilNextBirthday.ToString();
                var dateStr = p.BirthDate.ToString("dd.MM.yyyy", CultureInfo.InvariantCulture);

                Console.WriteLine(
                    $"{p.Id,-4} {p.FullName,-25} {dateStr,-15} {p.PhoneNumber ?? "-",-15} {status,-10}");
                Console.ResetColor();
            }
        }

        private void AddPerson()
        {
            Console.WriteLine("Добавление новой записи.");

            Console.Write("Имя: ");
            var name = Console.ReadLine() ?? string.Empty;

            var date = AskDate("Дата рождения (дд.мм.гггг): ");
            if (date == null)
                return;

            Console.Write("Телефон (можно оставить пустым): ");
            var phone = Console.ReadLine();

            Console.Write("Заметка (можно оставить пустой): ");
            var notes = Console.ReadLine();

            var (isValid, error) = _service.Validate(name, date.Value, phone);
            if (!isValid)
            {
                PrintError(error);
                return;
            }

            _service.Add(name, date.Value, phone, notes);
            PrintSuccess("Запись добавлена.");
        }

        private void EditPerson()
        {
            Console.Write("Введите Id записи для редактирования: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                PrintError("Некорректный Id.");
                return;
            }

            var existing = _service.GetById(id);
            if (existing == null)
            {
                PrintError("Запись с таким Id не найдена.");
                return;
            }

            Console.WriteLine($"Текущее имя: {existing.FullName}. Новое имя (Enter - оставить без изменений):");
            var name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
                name = existing.FullName;

            Console.WriteLine($"Текущая дата: {existing.BirthDate:dd.MM.yyyy}. Новая дата дд.мм.гггг (Enter - оставить без изменений):");
            var dateInput = Console.ReadLine();
            DateTime? date = string.IsNullOrWhiteSpace(dateInput) ? existing.BirthDate : ParseDate(dateInput);
            if (date == null)
            {
                PrintError("Некорректный формат даты.");
                return;
            }

            Console.WriteLine($"Текущий телефон: {existing.PhoneNumber ?? "-"}. Новый телефон (Enter - оставить без изменений):");
            var phoneInput = Console.ReadLine();
            var phone = string.IsNullOrWhiteSpace(phoneInput) ? existing.PhoneNumber : phoneInput;

            Console.WriteLine($"Текущая заметка: {existing.Notes ?? "-"}. Новая заметка (Enter - оставить без изменений):");
            var notesInput = Console.ReadLine();
            var notes = string.IsNullOrWhiteSpace(notesInput) ? existing.Notes : notesInput;

            var (isValid, error) = _service.Validate(name, date.Value, phone);
            if (!isValid)
            {
                PrintError(error);
                return;
            }

            _service.Update(id, name, date.Value, phone, notes);
            PrintSuccess("Запись обновлена.");
        }

        private void DeletePerson()
        {
            Console.Write("Введите Id записи для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out var id))
            {
                PrintError("Некорректный Id.");
                return;
            }

            Console.Write($"Вы уверены, что хотите удалить запись {id}? (да/нет): ");
            var confirm = Console.ReadLine();
            if (confirm?.Trim().ToLower(CultureInfo.InvariantCulture) != "да")
            {
                Console.WriteLine("Удаление отменено.");
                return;
            }

            if (_service.Delete(id))
                PrintSuccess("Запись удалена.");
            else
                PrintError("Запись с таким Id не найдена.");
        }

        private DateTime? AskDate(string prompt)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();
            var date = ParseDate(input);
            if (date == null)
                PrintError("Некорректный формат даты. Ожидается дд.мм.гггг.");
            return date;
        }

        private static DateTime? ParseDate(string? input)
        {
            if (DateTime.TryParseExact(
                    input,
                    "dd.MM.yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var result))
            {
                return result;
            }
            return null;
        }

        private static void PrintError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        private static void PrintSuccess(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
