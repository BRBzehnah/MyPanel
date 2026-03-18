using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace MyPanel.Services
{
    public class EmailService
    {
        private const string Host = "imap.gmail.com";
        private const int Port = 993;

        private static async Task<string> GetAuthCodeByEmail(string email, string password)
        {
            using (var client = new ImapClient())
            {
                try
                {
                    //Подключение (SSL обязательно)
                    await client.ConnectAsync(Host, Port, true);

                    //Авторизация
                    await client.AuthenticateAsync(email, password);

                    //Открываем папку "Входящие"
                    var inbox = client.Inbox;
                    await inbox.OpenAsync(FolderAccess.ReadWrite);

                    //яИщем непрочитанные письма от Steam за последние 5 минут
                    var query = SearchQuery.FromContains("noreply@steampowered.com")
                                .And(SearchQuery.SubjectContains("Steam Guard"))
                                .And(SearchQuery.NotSeen);

                    for (int i = 0; i < 15; i++)
                    {
                        var uids = await inbox.SearchAsync(query);

                        if (uids.Count > 0)
                        {
                            // Берем последнее найденное письмо
                            var message = await inbox.GetMessageAsync(uids.Last());

                            // Помечаем как прочитанное
                            await inbox.AddFlagsAsync(uids.Last(), MessageFlags.Seen, true);

                            string body = message.TextBody ?? message.HtmlBody;
                            string code = ExtractSteamCode(body);

                            if (!string.IsNullOrEmpty(code))
                            {
                                await client.DisconnectAsync(true);
                                return code;
                            }
                        }

                        await Task.Delay(2000); // Ждем 2 секунды перед повторной проверкой
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка сервиса почты: {ex.Message}");
                }
                finally
                {
                    if (client.IsConnected)
                        await client.DisconnectAsync(true);
                }
            }
            return null;
        }

        private static string ExtractSteamCode(string text)
        {
            var match = Regex.Match(text, @"\b[A-Z0-9]{5}\b");
            return match.Success ? match.Value : null;
        }
    }
}
