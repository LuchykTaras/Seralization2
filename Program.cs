using System;
using System.Collections.Generic;
using Bogus;
using Serilog;

namespace FakeUserGeneratorApp
{
    // 1. Модель фейкового користувача
    public class FakeUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public override string ToString()
        {
            return $"{FirstName} {LastName} | Тел: {PhoneNumber} | Email: {Email} | Адреса: {Address}";
        }
    }

    // 2. Клас для генерації фейкових користувачів
    public class UserGenerator
    {
        private readonly Faker<FakeUser> _userFaker;

        public UserGenerator(string locale = "uk")
        {
            // Налаштовуємо локалізацію (за замовчуванням українська "uk")
            _userFaker = new Faker<FakeUser>(locale)
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber("+380#########"))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.Address, f => $"{f.Address.City()}, {f.Address.StreetAddress()}");

            Log.Information("UserGenerator ініціалізовано з локалізацією: {Locale}", locale);
        }

        // Генерація одного користувача
        public FakeUser GenerateSingle()
        {
            Log.Debug("Запит на генерацію одного користувача.");
            var user = _userFaker.Generate();
            Log.Debug("Згенеровано користувача: {FirstName} {LastName}", user.FirstName, user.LastName);
            return user;
        }

        // Генерація списку користувачів
        public List<FakeUser> GenerateList(int count)
        {
            Log.Information("Запит на генерацію списку з {Count} користувачів.", count);

            if (count <= 0)
            {
                Log.Warning("Передано некоректну кількість для генерації: {Count}. Повертаємо порожній список.", count);
                return new List<FakeUser>();
            }

            var users = _userFaker.Generate(count);
            Log.Information("Успішно згенеровано {Count} користувачів.", users.Count);
            return users;
        }
    }

    // 3. Тестування програми
    class Program
    {
        static void Main(string[] args)
        {
            // Налаштування Serilog
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() // Встановлюємо мінімальний рівень логування
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("Додаток для генерації фейкових користувачів запущено.");

            try
            {
                // Створення екземпляра генератора (використовує українську локаль за замовчуванням)
                var generator = new UserGenerator();

                // Тест 1: Генерація одного користувача
                Console.WriteLine("\n--- Тест 1: Поодинока генерація ---");
                var singleUser = generator.GenerateSingle();
                Console.WriteLine($"Результат: {singleUser}");

                // Тест 2: Генерація списку користувачів
                Console.WriteLine("\n--- Тест 2: Масова генерація ---");
                int usersCount = 5;
                var userList = generator.GenerateList(usersCount);

                Console.WriteLine($"\nЗгенерований список із {usersCount} осіб:");
                foreach (var user in userList)
                {
                    Console.WriteLine($"- {user}");
                }

                // Тест 3: Обробка некоректних даних (для перевірки логування Warning)
                Console.WriteLine("\n--- Тест 3: Перевірка валідації ---");
                generator.GenerateList(-1);

            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Додаток завершив роботу з критичною помилкою!");
            }
            finally
            {
                Log.Information("Робота додатку завершена. Закриття логера.");
                Log.CloseAndFlush(); // Очищення буферів логування
            }

            Console.ReadLine();
        }
    }
}