using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using Data;
using MyPanel.Communication;
using Data.Config;
using System.Diagnostics;

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

            // Логика обработки команд от Панели
            private static async Task Execute(string command, StreamWriter writer, StreamReader reader)
            {
                if (Enum.TryParse(command, out Commands cmd))
                {
                    switch (cmd)
                    {
                        case Commands.OpenApp:
                            if (await OpenApp())
                                await writer.WriteLineAsync(Response.Success.ToString());
                            else
                                await writer.WriteLineAsync(Response.Failure.ToString());
                            break;

                        default:
                            await writer.WriteLineAsync(Response.Error.ToString());
                            break;
                    }
                }
                else
                    await writer.WriteLineAsync(Response.Failure.ToString());
            }

            static async Task Main(string[] args)
            {
                Console.WriteLine("[Agent] Запущен");

                string pipeName = GetArgument(args, "--pipe") ?? "default_bot_pipe";

                //Console.WriteLine($"[Agent] Запуск для бота {login}. Подключение к {pipeName}...");

                try
                {
                    using var client = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                    // Пытаемся подключиться (ждем до 10 секунд)
                    await client.ConnectAsync(10000);

                    if (client.IsConnected)
                    {
                        Console.WriteLine("[Agent] Подключено");
                        using var reader = new StreamReader(client);
                        using var writer = new StreamWriter(client) { AutoFlush = true };

                        if (await Handshake(reader, writer))
                        {
                            Console.WriteLine("[Agent] Совершено рукопожатие");

                            // 3. Основной цикл прослушивания команд
                            while (client.IsConnected)
                            {
                                string? command = await reader.ReadLineAsync();

                                if (command != null)
                                {
                                    await Execute(command, writer, reader);
                                }
                            }
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
                    Console.ReadLine();
                }
            }

            private static async Task<bool> Handshake(StreamReader reader, StreamWriter writer)
            {
                await writer.WriteLineAsync(Response.Ready.ToString());
                
                string verification = await reader.ReadLineAsync();
                if (verification != null)
                {
                    if (verification == Commands.DoConnect.ToString())
                    {
                        await writer.WriteLineAsync(Response.Connected.ToString());
                        return true;
                    }
                }
                return false;
            }

            private static async Task<bool> OpenApp()
            {
                var appPath = ConfigManager.Instance.Config.Paths.AppPath;
                var width = ConfigManager.Instance.Config.SizeOf.WindowWidth;
                var height = ConfigManager.Instance.Config.SizeOf.WindowHeight;

                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = appPath,
                        Arguments= $"-windowed -novid -low -w {width} - h {height}",
                        UseShellExecute = true,
                        WorkingDirectory = System.IO.Path.GetDirectoryName(appPath)
                    });
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
        }
    }
}
