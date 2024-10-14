using System;
using System.Collections.Generic;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using TaskScheduler;

class Program
{
    static async Task Main()
    {
        SpeechSynthesizer synthesizer = new SpeechSynthesizer();

        var voices = synthesizer.GetInstalledVoices();
        List<string> voiceNames = new List<string>();

        Console.WriteLine("Доступні голоси:");

        foreach (var voice in voices)
        {
            voiceNames.Add(voice.VoiceInfo.Name);
            Console.WriteLine($"- {voice.VoiceInfo.Name}");
        }

        int selectedIndex = 0;
        ConsoleKeyInfo keyInfo;

        do
        {
            Console.Clear();
            Console.WriteLine("Виберіть голос (використовуйте клавіші ВГОРУ/ВНИЗ для переміщення та ENTER для вибору):");
            for (int i = 0; i < voiceNames.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine(voiceNames[i]);
                Console.ResetColor();
            }

            keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : voiceNames.Count - 1;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex < voiceNames.Count - 1) ? selectedIndex + 1 : 0;
            }

        } while (keyInfo.Key != ConsoleKey.Enter);

        synthesizer.SelectVoice(voiceNames[selectedIndex]);
        synthesizer.Speak($"Voice {voiceNames[selectedIndex]} selected.");

        string filePath = "tasks.json";
        if (!System.IO.File.Exists(filePath))
        {
            System.IO.File.WriteAllText(filePath, "[]");
        }

        List<TaskModel> tasks = TaskManager.LoadTasksFromFile(filePath);

        _ = MonitorDeadlinesAsync(tasks, synthesizer);

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Головне меню:");
            Console.WriteLine("1. Додати нове завдання");
            Console.WriteLine("2. Переглянути список завдань");
            Console.WriteLine("3. Видалити завдання");
            Console.WriteLine("4. Вийти");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                AddTask(tasks, filePath);
            }
            else if (choice == "2")
            {
                DisplayTasks(tasks);
                Console.WriteLine("Натисніть будь-яку клавішу для повернення до меню...");
                Console.ReadKey();
            }
            else if (choice == "3")
            {
                DeleteTask(tasks, filePath);
            }
            else if (choice == "4")
            {
                break;
            }
        }
    }

    public static void AddTask(List<TaskModel> tasks, string filePath)
    {
        Console.WriteLine("Введіть нове завдання (або 'exit' для повернення до меню):");
        string input = Console.ReadLine();
        if (input.ToLower() == "exit")
        {
            return;
        }

        Console.WriteLine("Введіть дату дедлайну (формат: dd.MM.yyyy HH:mm:ss):");
        string dateInput = Console.ReadLine();
        if (DateTime.TryParse(dateInput, out DateTime deadline))
        {
            tasks.Add(new TaskModel { TaskText = input, Deadline = deadline });
            TaskManager.SaveTasksToFile(tasks, filePath);
        }
        else
        {
            Console.WriteLine("Невірний формат дати. Спробуйте ще раз.");
        }
    }

    public static void DisplayTasks(List<TaskModel> tasks)
    {
        Console.Clear();
        Console.WriteLine("Список завдань:");
        foreach (var task in tasks)
        {
            Console.WriteLine($"{task.TaskText} - Дедлайн: {task.Deadline}");
        }
    }

    public static void DeleteTask(List<TaskModel> tasks, string filePath)
    {
        if (tasks.Count == 0)
        {
            Console.WriteLine("Немає завдань для видалення.");
            Console.WriteLine("Натисніть будь-яку клавішу для повернення до меню...");
            Console.ReadKey();
            return;
        }

        int selectedIndex = 0;
        ConsoleKeyInfo keyInfo;

        do
        {
            Console.Clear();
            Console.WriteLine("Виберіть завдання для видалення (використовуйте клавіші ВГОРУ/ВНИЗ для переміщення, ENTER для вибору або 'Esc' для виходу):");
            for (int i = 0; i < tasks.Count; i++)
            {
                if (i == selectedIndex)
                {
                    Console.BackgroundColor = ConsoleColor.Gray;
                    Console.ForegroundColor = ConsoleColor.Black;
                }

                Console.WriteLine($"{tasks[i].TaskText} - Дедлайн: {tasks[i].Deadline}");
                Console.ResetColor();
            }

            keyInfo = Console.ReadKey(intercept: true);

            if (keyInfo.Key == ConsoleKey.UpArrow)
            {
                selectedIndex = (selectedIndex > 0) ? selectedIndex - 1 : tasks.Count - 1;
            }
            else if (keyInfo.Key == ConsoleKey.DownArrow)
            {
                selectedIndex = (selectedIndex < tasks.Count - 1) ? selectedIndex + 1 : 0;
            }
            else if (keyInfo.Key == ConsoleKey.Escape) 
            {
                Console.WriteLine("Видалення скасовано.");
                return;
            }

        } while (keyInfo.Key != ConsoleKey.Enter);

        Console.WriteLine($"Ви впевнені, що хочете видалити завдання '{tasks[selectedIndex].TaskText}'? (y/n)");
        if (Console.ReadKey().Key == ConsoleKey.Y)
        {
            tasks.RemoveAt(selectedIndex);
            TaskManager.SaveTasksToFile(tasks, filePath);
            Console.WriteLine("Завдання видалено.");
        }
        else
        {
            Console.WriteLine("Видалення скасовано.");
        }

        Console.WriteLine("Натисніть будь-яку клавішу для повернення до меню...");
        Console.ReadKey();
    }


    public static async Task MonitorDeadlinesAsync(List<TaskModel> tasks, SpeechSynthesizer synthesizer)
    {
        while (true)
        {
            DateTime now = DateTime.Now;
            List<TaskModel> overdueTasks = new List<TaskModel>();

            foreach (var task in tasks)
            {
                if (task.Deadline <= now)
                {
                    overdueTasks.Add(task);
                    synthesizer.Speak($"Task {task.TaskText} deadline finished!");
                }
            }

            tasks.RemoveAll(t => overdueTasks.Contains(t));

            TaskManager.SaveTasksToFile(tasks, "tasks.json");

            await Task.Delay(1000);
        }
    }
}
