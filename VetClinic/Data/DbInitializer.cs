using VetClinic.Models;
using VetClinic.Services;

namespace VetClinic.Data;

public static class DbInitializer
{
    public static void Initialize(AppDbContext context, IPasswordHasher passwordHasher)
    {
        // Создание БД, если она ещё не создана
        context.Database.EnsureCreated();

        // Проверяем, есть ли уже роли
        if (context.Roles.Any())
        {
            return; // БД уже инициализирована
        }

        // 1. Роли
        var adminRole = new Role { Name = "Администратор" };
        var vetRole = new Role { Name = "Ветеринар" };
        var clientRole = new Role { Name = "Клиент" };

        context.Roles.AddRange(adminRole, vetRole, clientRole);
        context.SaveChanges();

        // 2. Пользователи
        var adminUser = new User
        {
            Login = "admin",
            PasswordHash = passwordHasher.HashPassword("admin123"),
            FullName = "Администратор Клиники",
            Email = "admin@vetclinic.ru",
            Phone = "+7 (999) 111-22-33",
            RoleId = adminRole.Id,
            CreatedAt = DateTime.Now.AddMonths(-6)
        };

        var vet1 = new User
        {
            Login = "vet_ivanov",
            PasswordHash = passwordHasher.HashPassword("vet123"),
            FullName = "Иванов Алексей Сергеевич",
            Email = "ivanov@vetclinic.ru",
            Phone = "+7 (999) 222-33-44",
            Specialization = "Ведущий хирург-ортопед",
            RoleId = vetRole.Id,
            CreatedAt = DateTime.Now.AddMonths(-5)
        };

        var vet2 = new User
        {
            Login = "vet_smirnova",
            PasswordHash = passwordHasher.HashPassword("vet123"),
            FullName = "Смирнова Елена Павловна",
            Email = "smirnova@vetclinic.ru",
            Phone = "+7 (999) 333-44-55",
            Specialization = "Терапевт, врач УЗИ-диагностики",
            RoleId = vetRole.Id,
            CreatedAt = DateTime.Now.AddMonths(-4)
        };

        var client1 = new User
        {
            Login = "client_danil",
            PasswordHash = passwordHasher.HashPassword("client123"),
            FullName = "Макаренко Данил Сергеевич",
            Email = "danil.makarenko@mail.ru",
            Phone = "+7 (999) 444-55-66",
            RoleId = clientRole.Id,
            CreatedAt = DateTime.Now.AddMonths(-3)
        };

        var client2 = new User
        {
            Login = "client_anna",
            PasswordHash = passwordHasher.HashPassword("client123"),
            FullName = "Ковалёва Анна Дмитриевна",
            Email = "anna.kovaleva@mail.ru",
            Phone = "+7 (999) 555-66-77",
            RoleId = clientRole.Id,
            CreatedAt = DateTime.Now.AddMonths(-2)
        };

        context.Users.AddRange(adminUser, vet1, vet2, client1, client2);
        context.SaveChanges();

        // 3. Виды животных
        var dogType = new PetType { Name = "Собака" };
        var catType = new PetType { Name = "Кошка" };
        var birdType = new PetType { Name = "Птица" };
        var rodentType = new PetType { Name = "Грызун" };
        var exoticType = new PetType { Name = "Экзотическое" };

        context.PetTypes.AddRange(dogType, catType, birdType, rodentType, exoticType);
        context.SaveChanges();

        // 4. Услуги
        var s1 = new Service
        {
            Name = "Первичный клинический осмотр",
            Description = "Комплексный осмотр питомца: сбор анамнеза, термометрия, пальпация, аускультация сердца и легких, консультация владельца.",
            Price = 1200,
            DurationMinutes = 30,
            IsActive = true
        };
        var s2 = new Service
        {
            Name = "Комплексная вакцинация",
            Description = "Вакцинация зарубежным препаратом с защитой от основных вирусных инфекций и бешенства с отметкой в ветеринарном паспорте.",
            Price = 2500,
            DurationMinutes = 20,
            IsActive = true
        };
        var s3 = new Service
        {
            Name = "УЗИ брюшной полости",
            Description = "Полное ультразвуковое исследование внутренних органов брюшной полости на аппарате экспертного класса с протоколом.",
            Price = 2200,
            DurationMinutes = 40,
            IsActive = true
        };
        var s4 = new Service
        {
            Name = "Электронное микрочипирование",
            Description = "Безболезненная имплантация микрочипа под кожу и регистрация в международной базе данных AnimalFace.",
            Price = 1800,
            DurationMinutes = 15,
            IsActive = true
        };
        var s5 = new Service
        {
            Name = "Ультразвуковая чистка зубов",
            Description = "Удаление зубного камня и налета ультразвуковым скалером с последующей полировкой зубной эмали.",
            Price = 3500,
            DurationMinutes = 60,
            IsActive = true
        };
        var s6 = new Service
        {
            Name = "Клинический анализ крови",
            Description = "Развернутый общий анализ крови на автоматическом анализаторе (результат в день обращения).",
            Price = 1400,
            DurationMinutes = 15,
            IsActive = true
        };

        context.Services.AddRange(s1, s2, s3, s4, s5, s6);
        context.SaveChanges();

        // 5. Питомцы
        var pet1 = new Pet
        {
            Name = "Барон",
            PetTypeId = dogType.Id,
            Breed = "Лабрадор-ретривер",
            BirthDate = DateTime.Today.AddYears(-2).AddMonths(-4),
            Gender = "Самец",
            Weight = 32.5,
            OwnerId = client1.Id,
            MedicalNotes = "Непереносимость некоторых сухих кормов с курицей. Вакцинация по графику.",
            CreatedAt = DateTime.Now.AddMonths(-3)
        };

        var pet2 = new Pet
        {
            Name = "Барсик",
            PetTypeId = catType.Id,
            Breed = "Британская короткошёрстная",
            BirthDate = DateTime.Today.AddYears(-3).AddMonths(-1),
            Gender = "Самец",
            Weight = 5.1,
            OwnerId = client1.Id,
            MedicalNotes = "Кастрирован. Спокойный темперамент.",
            CreatedAt = DateTime.Now.AddMonths(-2)
        };

        var pet3 = new Pet
        {
            Name = "Луна",
            PetTypeId = catType.Id,
            Breed = "Мейн-кун",
            BirthDate = DateTime.Today.AddYears(-1).AddMonths(-8),
            Gender = "Самка",
            Weight = 6.4,
            OwnerId = client2.Id,
            MedicalNotes = "Особых патологий не выявлено. Чипирована.",
            CreatedAt = DateTime.Now.AddMonths(-2)
        };

        var pet4 = new Pet
        {
            Name = "Кеша",
            PetTypeId = birdType.Id,
            Breed = "Волнистый попугай",
            BirthDate = DateTime.Today.AddMonths(-11),
            Gender = "Самец",
            Weight = 0.04,
            OwnerId = client2.Id,
            MedicalNotes = "Активный, яркое оперение.",
            CreatedAt = DateTime.Now.AddMonths(-1)
        };

        context.Pets.AddRange(pet1, pet2, pet3, pet4);
        context.SaveChanges();

        // 6. Приёмы
        var v1 = new Visit
        {
            PetId = pet1.Id,
            ClientId = client1.Id,
            VeterinarianId = vet1.Id,
            ServiceId = s1.Id,
            VisitDate = DateTime.Today.AddDays(-14).AddHours(11),
            Status = "Завершён",
            Symptoms = "Легкая хромота на правую переднюю лапу после активной пробежки в парке.",
            Diagnosis = "Растяжение связочного аппарата правого лучезапястного сустава лёгкой степени.",
            Treatment = "Ограничение активных нагрузок на 7 дней. Назначен мелоксидил 1.5 мг 1 раз в сутки с пищей в течение 4 дней.",
            DoctorNotes = "Повторный осмотр при сохранении симптомов через неделю. Рекомендован ортопедический покой.",
            TotalCost = s1.Price,
            CreatedAt = DateTime.Today.AddDays(-15)
        };

        var v2 = new Visit
        {
            PetId = pet2.Id,
            ClientId = client1.Id,
            VeterinarianId = vet2.Id,
            ServiceId = s2.Id,
            VisitDate = DateTime.Today.AddDays(-7).AddHours(15),
            Status = "Завершён",
            Symptoms = "Плановая ежегодная вакцинация кота.",
            Diagnosis = "Клинически здоров, противопоказаний к вакцинации нет.",
            Treatment = "Введена вакцина Nobivac Tricat Trio + Nobivac Rabies. Побочных реакций не наблюдалось.",
            DoctorNotes = "Сделать отметку в ветпаспорте. Следующая ревакцинация через 12 месяцев.",
            TotalCost = s2.Price,
            CreatedAt = DateTime.Today.AddDays(-8)
        };

        var v3 = new Visit
        {
            PetId = pet1.Id,
            ClientId = client1.Id,
            VeterinarianId = vet2.Id,
            ServiceId = s3.Id,
            VisitDate = DateTime.Today.AddDays(2).AddHours(14),
            Status = "Запланирован",
            Symptoms = "Профилактическое плановое УЗИ органов брюшной полости.",
            Diagnosis = null,
            Treatment = null,
            DoctorNotes = null,
            TotalCost = s3.Price,
            CreatedAt = DateTime.Today.AddDays(-1)
        };

        var v4 = new Visit
        {
            PetId = pet3.Id,
            ClientId = client2.Id,
            VeterinarianId = vet1.Id,
            ServiceId = s5.Id,
            VisitDate = DateTime.Today.AddDays(4).AddHours(16),
            Status = "Запланирован",
            Symptoms = "Неприятный запах из пасти, налёт на зубах.",
            Diagnosis = null,
            Treatment = null,
            DoctorNotes = null,
            TotalCost = s5.Price,
            CreatedAt = DateTime.Today
        };

        context.Visits.AddRange(v1, v2, v3, v4);
        context.SaveChanges();
    }
}
