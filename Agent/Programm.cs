using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;

namespace Agent
{

    namespace MyPanel.Agent
    {
        class Program
        {
            private static string? GetArgument(string[] args, string key)
            {
                int index = Array.IndexOf(args, key);
                return (index != -1 && index < args.Length - 1) ? args[index + 1] : null;
            }
            static async Task Main(string[] args)
            {
                string pipeName = GetArgument(args, "--pipe") ?? "default_bot_pipe";

                //Console.WriteLine($"[Agent] Запуск для бота {login}. Подключение к {pipeName}...");

                try
                {
                    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                    // Пытаемся подключиться (ждем до 10 секунд)
                    await client.ConnectAsync(10000);

                    using var reader = new StreamReader(client);
                    using var writer = new StreamWriter(client) { AutoFlush = true };

                    //Console.WriteLine("[Agent] Соединение с Панелью установлено!");

                    // Сообщаем Панели, что мы живы
                    await writer.WriteLineAsync("READY");

                    // 3. Основной цикл прослушивания команд
                    while (client.IsConnected)
                    {
                        string? command = await reader.ReadLineAsync();

                        if (command != null)
                        {
                            Console.WriteLine($"[Agent] Получена команда: {command}");
                            await Execute(command, writer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Agent] Ошибка связи или песочницы: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("[Agent] Завершение работы.");
                }
            }

            // Логика обработки команд от Панели
            private static async Task Execute(string command, StreamWriter writer)
            {
                switch (command)
                {
                    case "START_GAME":
                        // Здесь будет логика запуска Steam/Игры
                        await writer.WriteLineAsync("STATUS|GAME_LAUNCHING");
                        break;

                    case "MOVE_FORWARD":
                        // Здесь будет эмуляция нажатия клавиши 'W'
                        await writer.WriteLineAsync("STATUS|MOVING");
                        break;

                    default:
                        await writer.WriteLineAsync("ERROR|UNKNOWN_COMMAND");
                        break;
                }
            }
        }
    }
}
