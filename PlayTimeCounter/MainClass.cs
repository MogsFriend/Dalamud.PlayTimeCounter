using Dalamud.Game.ClientState;
using Dalamud.Game.Gui;
using Dalamud.Game.Text;
using Dalamud.IoC;
using Dalamud.Plugin;
using System;
using System.Diagnostics;
using System.Threading;

#pragma warning disable CA1416
namespace PlayTimeCounter
{
    public class PlayTimeCounter : IDalamudPlugin, IDisposable
    {
        [PluginService] public static ChatGui ChatGui { get; private set; } = null!;
        [PluginService] public static ClientState ClientState { get; private set; } = null!;

        public string Name => "PlayTimeCounter";
        internal static Timer Timer;
        internal static Stopwatch Elapse;

        public PlayTimeCounter()
        {
            Elapse = new Stopwatch();
            Elapse.Start();
            Timer = new Timer(CallBack);
            Timer.Change(0, 1000);
            ClientState.Login += ClientState_Login;
        }

        private void CallBack(object status)
        {
            var time = Elapse.ElapsedMilliseconds;

            if ((time / 1000) % 3600 == 0 && time > 1000)
            {
                var elapsed = time / 3600000;
                var flag = elapsed > 1500;
                var message = "";

                switch (ClientState.ClientLanguage)
                {
                    case Dalamud.ClientLanguage.Japanese:
                        // in logmessage.exd (409)
                        message = $"ゲームを開始してから{(flag ? "1500時間以上" : elapsed + "時間")}が経過しました。\n過剰なゲームの利用は正常な日常生活に支障を与える恐れがあります。";
                        break;
                    case Dalamud.ClientLanguage.English:
                        // in logmessage.exd (409)
                        message = $"You've played more than you should. It's time you got some vitamin D.";
                        break;
                    case Dalamud.ClientLanguage.German:
                        // google translate
                        message = $"{(flag ? "Mehr als 1500" : elapsed)} Stunden sind vergangen, seit ich das Spiel gestartet habe.\nÜbermäßiger Gebrauch von Spielen kann den normalen Alltag beeinträchtigen.";
                        break;
                    case Dalamud.ClientLanguage.French:
                        // google translate
                        message = $"{(flag ? "Plus de 1500 " : elapsed)} heures se sont écoulées depuis que j'ai commencé le jeu.\nL'utilisation excessive de jeux peut interférer avec la vie quotidienne normale.";
                        break;
                }

                if (ClientState.IsLoggedIn)
                {
                    ChatGui.PrintChat(new()
                    {
                        Message = message,
                        Type = XivChatType.SystemMessage
                    });
                }
            }
        }

        private void ClientState_Login(object sender, EventArgs e)
        {
            Elapse.Restart();
        }

        public void Dispose()
        {
            Elapse.Stop();
            Timer.Dispose();
            ClientState.Login -= ClientState_Login;
            GC.SuppressFinalize(this);
        }
    }
}
#pragma warning restore CA1416
