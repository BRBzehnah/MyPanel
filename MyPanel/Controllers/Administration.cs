using Microsoft.EntityFrameworkCore;
using MyPanel.APIs.SandboxieAPI;
using MyPanel.Data;
using MyPanel.Models;
using MyPanel.Services;
using SteamKit2.WebUI.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MyPanel.Controllers
{
    public class Administration
    {
        private readonly ApplicationDbContext _db;
        private readonly BotServices _bs;
        private readonly SandboxController _sbc;
        private readonly SteamServices _ss;
        public Administration()
        {
            _db = new ApplicationDbContext();
            _bs = new BotServices(_db);
            _ss = new SteamServices();
            _sbc = new SandboxController();
        }

        public async Task<bool> Registration(string email, string login, string password, string phoneNumber)
        {

            var bot = new BotModel(email, login, password);

            try
            {
                bot.MaFile = await _ss.CreateMaFile(bot.Login, bot.Password, phoneNumber);
            }
            catch 
            {
                return false;
            }

            if (bot.MaFile is not null)
            {
                var rCode = _ss.GetRestoreCode(bot.MaFile);
                if (rCode is not null)
                {
                    bot.RestoreCode = rCode;
                    _bs.Create(bot);
                    return true;
                }
            }
            return false;
        }

        public async Task Start(List<BotModel> bots)
        {
            foreach (var bot in bots)
            {
                string pipeName = $"bot_{bot.Id}_pipe";
                string agentExePath = ConfigManager.Instance.Config.Path.AgentExePath;
                await _sbc.CreateBox(bot);

                bot.PipeServer = new NamedPipeServerStream(
                    pipeName,
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous);

                string args = $"/box:{bot.BoxName} {agentExePath} --pipe {pipeName}";

                if (await _sbc.RunBox(args))
                    _ = Task.Run(() => HandleAgentCommunication(bot));
                
            }
        }
        private async Task HandleAgentCommunication(BotModel bot)
        {
            try
            {
                await bot.PipeServer.WaitForConnectionAsync();

                using var reader = new StreamReader(bot.PipeServer);
                using var writer = new StreamWriter(bot.PipeServer) { AutoFlush = true };

                //Console.WriteLine($"[Bot {bot.Id}] Агент успешно подключен и готов к работе.");

                //// Пример: отправляем команду на запуск игры сразу после подключения
                //await writer.WriteLineAsync("START_GAME");

                //// Цикл прослушивания статусов от агента
                //while (bot.PipeServer.IsConnected)
                //{
                //    var status = await reader.ReadLineAsync();
                //    if (status != null)
                //    {
                //        Console.WriteLine($"[Bot {bot.Id}] Статус: {status}");
                //        // Здесь можно обновлять UI или БД на основе данных от агента
                //    }
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Bot {bot.Id}] Ошибка связи: {ex.Message}");
            }
            finally
            {
                bot.PipeServer?.Dispose();
            }
        }
    }
}
