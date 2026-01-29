using Newtonsoft.Json;
using SteamAuth;
using SteamKit2;
using SteamKit2.Authentication;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyPanel.Services
{
    public class SteamServices
    {
        private async Task<SessionData> GetSession(string username, string password)
        {
            var steamClient = new SteamClient();
            var auth = steamClient.Authentication;

            steamClient.Connect();

            while(!steamClient.IsConnected) await Task.Delay(100);

            try
            {
                var authSession = await auth.BeginAuthSessionViaCredentialsAsync(new AuthSessionDetails()
                {
                    Username = username,
                    Password = password,
                    IsPersistentSession = true,
                    PlatformType = EAuthTokenPlatformType.k_EAuthTokenPlatformType_MobileApp,
                    ClientOSType = EOSType.Android9,
                    Authenticator = new UserConsoleAuthenticator()
                });

                var result = await authSession.PollingWaitForResultAsync();

                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    return new SessionData
                    {
                        SteamID = steamClient.SteamID.ConvertToUInt64(),
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        // SessionID генерируем сами для совместимости со SteamAuth
                        SessionID = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                    };
                }
                else
                    return null;
            }
            catch (Exception ex) 
            {
                return null;
            };
        }

        public async Task<string> CreateMaFile(string username, string password, string phoneNumber)
        {
            var session = await GetSession(username, password);
            AuthenticatorLinker linker = new AuthenticatorLinker(session);
            linker.PhoneNumber = phoneNumber;

            var result = await linker.AddAuthenticator();

            if (result == AuthenticatorLinker.LinkResult.AwaitingFinalization)
            {
                Console.WriteLine("Введите SMS код в консоль:");
                string? smsCode = await Task.Run(() => Console.ReadLine());

                if (!string.IsNullOrEmpty(smsCode))
                {
                    var final = await linker.FinalizeAddAuthenticator(smsCode);
                        if (final == AuthenticatorLinker.FinalizeResult.Success)
                            return JsonConvert.SerializeObject(linker.LinkedAccount);
                }
            }
            return null;
        }

        public string GetGuardCode(string maFileJson)
        {
            var account = JsonConvert.DeserializeObject<SteamGuardAccount>(maFileJson);
            return account.GenerateSteamGuardCode();
        }

        public string GetRestoreCode(string maFile)
        {
            if (string.IsNullOrEmpty(maFile)) return "";

            var account = JsonConvert.DeserializeObject<SteamGuardAccount>(maFile);
            return account?.RevocationCode ?? "";
        }
    }
}
