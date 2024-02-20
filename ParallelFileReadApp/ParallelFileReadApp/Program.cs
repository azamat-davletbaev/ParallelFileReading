using ParallelFileReadApp;

Console.WriteLine("Домашнее задание 'Паралельное считывание файлов'");

string dir = $@"{Environment.CurrentDirectory}\Files";

Console.WriteLine($"Читаем каталог: {dir}");

var reader = new FileReader(Console.WriteLine);

// для простоты используем готовые файлы расположенные в каталоге сборки
var task = reader.CalcSpaces(dir);
task.Wait();

Console.WriteLine($"Количество пробелов во всех файлах {task.Result}");

Console.ReadLine();