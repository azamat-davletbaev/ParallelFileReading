using System.Diagnostics;

namespace ParallelFileReadApp
{
    /// <summary>
    /// Класс чтения файлов
    /// </summary>
    public class FileReader
    {
        /// <summary>
        /// Поддерживаемые типы файлов
        /// </summary>
        private string supportedExtensions = "*.txt,*.xml,*.json,*.cs,*.ini";
        /// <summary>
        /// Делегат вывода сообщений
        /// </summary>
        private Action<string> logger;

        /// <summary>
        /// итоговое количество пробелов
        /// </summary>
        private long count = 0;
        
        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="logger"></param>
        public FileReader(Action<string> logger)
        {
            this.logger = logger;
        }
                                
        /// <summary>
        /// Публичный метод получить сумму количества пробелов из всех файлов указанной папки
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public async Task<long> CalcSpaces(string path)
        {
            var stopwatch = Stopwatch.StartNew();

            count = 0;
            var files = GetFiles(path);
            List<Task<(long cnt, string file)>> tasks = files.Select(CalcSpacesInFile).ToList();
                        
            while (tasks.Any())
            {
                var finishedTask = await Task.WhenAny(tasks);
                tasks.Remove(finishedTask);
                var task = await finishedTask;                
                count += task.cnt;

                logger.Invoke($"читаем файл '{task.file}', id потока: {finishedTask.Id}");
            }

            stopwatch.Stop();
            logger?.Invoke($"выполнено за {stopwatch.ElapsedMilliseconds} миллисекунд \n");

            return count;
        }

        /// <summary>
        /// Получить список файлов
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private IEnumerable<string> GetFiles(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                    logger?.Invoke($"Каталог {path} не найден!");

                var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(s => supportedExtensions.Contains(Path.GetExtension(s).ToLower()));

                if (files?.Any() ?? false)
                    return files;
                else
                    logger?.Invoke($"В папке {path} нет файлов!");
            }
            catch (Exception ex)
            {
                logger?.Invoke(ex.Message);
            }

            return default;
        }

        /// <summary>
        /// Количество пробелов в файле
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private Task<(long spaces, string file)> CalcSpacesInFile(string filePath)
        {            
            string text = File.ReadAllText(filePath);
            return Task.FromResult((GetSpaceCount(text), Path.GetFileName(filePath)));
        }

        /// <summary>
        /// Количество пробелов в тексте
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private long GetSpaceCount(string text)
        {
            return text.Count(Char.IsWhiteSpace);
        }
    }
}
