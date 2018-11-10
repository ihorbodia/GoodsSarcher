using GoodsSearcher.Common.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoodsSearcher.Common.Helpers
{
    public static class CustomTaskFactory
    {
        public static List<TaskItem> tasks = new List<TaskItem>(20);

        public static Task GetNewTask(object combination)
        {
            var unusedTask = tasks.FirstOrDefault(x => x.Task == null || x.Task.IsCompleted);
            string combinationStr = string.Empty;
            if (unusedTask != null && combination != null)
            {
                combinationStr = combination.ToString();
                return unusedTask.StartTask(combinationStr);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        static CustomTaskFactory()
        {
            for (int i = 0; i < 20; i++)
            {
                tasks.Add(new TaskItem());
            }
        }
    }
}
