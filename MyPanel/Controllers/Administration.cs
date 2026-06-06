using Data;
using Data.Config;
using Data.Models;
using Microsoft.EntityFrameworkCore;
using MyPanel.APIs.SandboxieAPI;
using MyPanel.Data;
using MyPanel.Models;
using MyPanel.Services;
using Newtonsoft.Json;
using SteamKit2.WebUI.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MyPanel.Controllers
{
    public class Administration
    {
        #region Services and DbContext
        private readonly ApplicationDbContext _db;
        private readonly BotServices _bs;
        private readonly SandboxController _sbc;
        private readonly SteamServices _ss;
        private readonly EmailService _email;
        #endregion
        private List<BotModel> Bots { get; set; }

        public Administration()
        {
            _db = new ApplicationDbContext();
            _bs = new BotServices(_db);
            _ss = new SteamServices();
            _sbc = new SandboxController();
        }

        public async Task<Result> Registration(string email, string login, string password, string phoneNumber)
        {

            var bot = new BotModel(email, login, password);

            try
            {
                bot.MaFile = await _ss.CreateMaFile(bot.Login, bot.Password, phoneNumber);
            }
            catch (Exception ex)
            {
                return Result.Failure(new Error(ErrorType.OuterLibraryError, $"Ошибка при создании MaFile: {ex.Message}"));
            }

            if (bot.MaFile is null)
                return Result.Failure(new Error(ErrorType.OuterLibraryError, "Не удалось создать MaFile"));

            var rCode = _ss.GetRestoreCode(bot.MaFile);

            if (rCode is null)
                return Result.Failure(new Error(ErrorType.OuterLibraryError, "Не удалось получить код восстановления"));

            bot.RestoreCode = rCode;
            return _bs.Create(bot);
        }

        public async Task<Result> RegistrationByEmail(string email, string login, string password, string emailPassword)
        {
            var bot = new BotModel(email, login, password, emailPassword);

            return _bs.Create(bot);
        }

        public async Task Start(List<BotModel> bots)
        {
            Bots = bots;
            foreach (var bot in Bots)
            {
                string pipeName = $"bot_{bot.Id}_pipe";
                string agentExePath = ConfigManager.Instance.Config.Paths.AgentExePath;
                await _sbc.CreateBox(bot);

                bot.PipeServer = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);
                var connectionTask = bot.PipeServer.WaitForConnectionAsync();

                string args = $"/box:{bot.BoxName} {agentExePath} --pipe {pipeName}";

                if ((await _sbc.RunBox(args)).IsSuccess)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await connectionTask;
                            await AgentCommunication(bot);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"[Panel] Ошибка связи: {ex.Message}");
                        }
                    });

                }
                //Ожидание подключения до закрытия метода
                for (int i = 0; i < 150; i++)
                {
                    if (Bots.All(b => b.IsConnected == true))
                        break;
                    await Task.Delay(100);
                }

            }
        }

        private async Task AgentCommunication(BotModel bot)
        {
            try
            {
                if (bot.PipeServer == null || !bot.PipeServer.IsConnected)
                    return;

                using var reader = new StreamReader(bot.PipeServer);
                using var writer = new StreamWriter(bot.PipeServer) { AutoFlush = true };

                if ((await Handshake(reader, writer)).IsSuccess)
                {
                    Console.WriteLine("подключено");
                    bot.IsConnected = true;
                }

                while (Bots.Any(b => b.IsConnected != true))
                {
                    await Task.Delay(100);
                }

                await CommandHandler(reader, writer, bot);
            }
            catch (Exception ex)
            {

            }
            finally
            {
                bot.PipeServer?.Dispose();
            }
        }

        private static async Task<Result> Handshake(StreamReader reader, StreamWriter writer)
        {
            string response = await reader.ReadLineAsync();
            if (response is null)
                return Result.Failure(new Error(ErrorType.CommunicationError, "Агент не ответил на приветствие"));
            if (response != Response.Ready.ToString())
                return Result.Failure(new Error(ErrorType.CommunicationError, "Агент не готов к подключению"));

            await writer.WriteLineAsync(Commands.DoConnect.ToString());

            if(await reader.ReadLineAsync() != Response.Connected.ToString())
                return Result.Failure(new Error(ErrorType.CommunicationError, "Агент не подтвердил подключение"));

            return Result.Success();

        }
        
        private static string DataToJson(string login, string password)
        {
            var data = new AuthDataModel
            {
                Login = login,
                Password = password
            };
            return JsonConvert.SerializeObject(data);
        }

        private async Task CommandHandler(StreamReader reader, StreamWriter writer, BotModel bot)
        {
            // Цикл прослушивания статусов от агента
            while (bot.PipeServer.IsConnected)
            {
                var response = await reader.ReadLineAsync();
                if (Enum.TryParse(response, out Response rsp))
                {
                    switch (rsp)
                    {
                        case Response.NeedAuthData:
                            var authData = DataToJson(bot.Login, bot.Password);
                            await writer.WriteLineAsync(authData);
                            break;
                        case Response.NeedGuardCode:
                            var guardCode = await _email.GetAuthCodeByEmail(bot.Email, bot.EmailPassword);
                            await writer.WriteLineAsync(guardCode);
                            break;
                    }
                }
                else
                    MessageBox.Show("Неизвестный ответ агента");
            }
        }

    }
}
