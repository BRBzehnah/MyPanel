using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Pipes;
using Data.Config;
using System.Diagnostics;
using SteamKit2.Internal;
using Newtonsoft.Json;
using WindowsInput;
using WindowsInput.Native;
using Data.Models;
using Data;

namespace Agent
{

    namespace MyPanel.Agent
    {
        class Program
        {
            private static readonly PixelBot _pixelBot = new PixelBot();
            private static readonly InputSimulator _inputSim = new InputSimulator();

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
                            if ((await OpenApp()).IsSuccess)
                            {
                                await writer.WriteLineAsync(Response.NeedAuthData.ToString());  
                                var authData = JsonConvert.DeserializeObject<AuthDataModel>(await reader.ReadLineAsync());
                                if ((await Login(authData?.Login, authData?.Password)).IsSuccess)
                                {
                                    await writer.WriteLineAsync(Response.NeedGuardCode.ToString());
                                    var guardCode = await reader.ReadLineAsync();
                                    Thread.Sleep(2000); //test
                                    if ((await EnterGuardCode(guardCode)).IsSuccess)
                                        await writer.WriteLineAsync(Response.Success.ToString());
                                }
                                else
                                    await writer.WriteLineAsync(Response.Failure.ToString());
                            }
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

                        if ((await Handshake(reader, writer)).IsSuccess)
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

            private static async Task<Result> Handshake(StreamReader reader, StreamWriter writer)
            {
                await writer.WriteLineAsync(Response.Ready.ToString());

                string verification = await reader.ReadLineAsync();
                if (verification is null)
                    return Result.Failure(new Error(ErrorType.CommunicationError, "Панель не ответила на приветствие"));
                
                if (verification != Commands.DoConnect.ToString())
                    return Result.Failure(new Error(ErrorType.CommunicationError, "Панель не подтвердила подключение"));

                await writer.WriteLineAsync(Response.Connected.ToString());
                return Result.Success();            
            }

            private static async Task<Result> OpenApp()
            {
                var appPath = ConfigManager.Instance.Config.Paths.AppPath;
                var width = ConfigManager.Instance.Config.SizeOf.WindowWidth;
                var height = ConfigManager.Instance.Config.SizeOf.WindowHeight;

                try
                {
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = appPath,
                        Arguments = $"-windowed -novid -low -w {width} - h {height}",
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(appPath)
                    });
                }
                catch (Exception ex)
                {
                    return Result.Failure(new Error(ErrorType.SystemError, $"Ошибка при запуске приложения: {ex.Message}"));
                }

                return await _pixelBot.WaitForLoginWindow();
            }

            private static async Task<Result> Login(string login, string password)
            {
                var result = await _pixelBot.WaitForLoginWindow();
                if (!result.IsSuccess) return result;

                try
                {
                    _inputSim.Keyboard.TextEntry(login);
                    _inputSim.Keyboard.KeyPress(VirtualKeyCode.TAB);
                    _inputSim.Keyboard.TextEntry(password);
                    _inputSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);

                    return Result.Success();
                }
                catch (Exception ex)
                {
                    return Result.Failure(new Error(ErrorType.AgentError, $"Ошибка при входе: {ex.Message}"));
                }
            }

            private static async Task<Result> EnterGuardCode(string code)
            {
                try
                {
                    _inputSim.Keyboard.TextEntry(code);
                    _inputSim.Keyboard.KeyPress(VirtualKeyCode.RETURN);
                    return Result.Success();
                }
                catch (Exception ex)
                {
                    return Result.Failure(new Error(ErrorType.AgentError, $"Ошибка при вводе Guard кода: {ex.Message}"));
                }
            }
        }
    }
}
