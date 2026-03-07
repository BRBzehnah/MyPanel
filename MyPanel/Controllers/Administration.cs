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

        public async Task Start(List<BotModel> bots)
        {
            Bots = bots;
            foreach (var bot in Bots)
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
                    _ = Task.Run(() => AgentCommunication(bot));
            }
        }

        private async Task AgentCommunication(BotModel bot)
        {
            try
            {
                await bot.PipeServer.WaitForConnectionAsync();

                using var reader = new StreamReader(bot.PipeServer);
                using var writer = new StreamWriter(bot.PipeServer) { AutoFlush = true };

                if (await Handshake(reader, writer))
                    bot.PipeStatus = Response.Connected;

                while (Bots.Any(b => b.PipeStatus != Response.Connected))
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
                    await writer.WriteLineAsync(Command.DoConnect.ToString());
                    if(await reader.ReadLineAsync() == Response.Connected.ToString())
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
                if (response != null)
                {

                }
            }
        }
    }
}
