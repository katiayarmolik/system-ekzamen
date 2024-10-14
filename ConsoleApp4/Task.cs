using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TaskScheduler
{
    public class TaskModel
    {
        public string TaskText { get; set; }
        public DateTime Deadline { get; set; }
    }

    public static class TaskManager
    {
        public static List<TaskModel> LoadTasksFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<List<TaskModel>>(json);
            }
            return new List<TaskModel>();
        }

        public static void SaveTasksToFile(List<TaskModel> tasks, string filePath)
        {
            string json = JsonConvert.SerializeObject(tasks, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
