using Data;
using Data.Config;
using Microsoft.EntityFrameworkCore;
using MyPanel.APIs.SandboxieAPI;
using MyPanel.Communication;
using MyPanel.Data;
using MyPanel.Models;
using MyPanel.Services;
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
        private readonly ApplicationDbContext _db;
        private readonly BotServices _bs;
        private readonly SandboxController _sbc;
        private readonly SteamServices _ss;
        private List<BotModel> Bots { get; set; }

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

        public async Task<bool> RegistrationByEmail(string email, string login, string password, string emailPassword)
        {
            var bot = new BotModel(email,login, password, emailPassword);
            
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

                if (await _sbc.RunBox(args))
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

                if (await Handshake(reader, writer))
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

        private static async Task<bool> Handshake(StreamReader reader, StreamWriter writer)
        {
            string response =  await reader.ReadLineAsync();
            if (response != null)
            {
                if (response == Response.Ready.ToString())
                {
                    await writer.WriteLineAsync(Commands.DoConnect.ToString());
                    if (await reader.ReadLineAsync() == Response.Connected.ToString())
                        return true;
                }
            }
            return false;
        }

        private static async Task CommandHandler(StreamReader reader, StreamWriter writer, BotModel bot)
        {
            // Цикл прослушивания статусов от агента
            while (bot.PipeServer.IsConnected)
            {
                var response = await reader.ReadLineAsync();
                if (Enum.TryParse(response, out Response rsp))
                {
                    switch (rsp)
                    {

                    }
                }
                else
                    MessageBox.Show("Неизвестный ответ агента");
            }
        }

    }
}
