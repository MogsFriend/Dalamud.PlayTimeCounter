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
        internal static System.Timers.Timer Timer;
        internal static Stopwatch Elapse;

        public PlayTimeCounter()
        {
            Elapse = new Stopwatch();
            Elapse.Start();
            Timer = new System.Timers.Timer();
            Timer.Interval = 1000;
            Timer.Elapsed += Ticker;
            Timer.Start();
            ClientState.Login += ClientState_Login;
        }

        private void Ticker(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (Elapse.ElapsedMilliseconds < 1000) return;
            bool flag_a = Elapse.ElapsedMilliseconds / 1000 % 3600 == 0;
            if (!flag_a)
            {
                string message = "";
                long elapsed = Elapse.ElapsedMilliseconds / 3600000;
                bool flag_b = elapsed > 1500;
                switch (ClientState.ClientLanguage)
                {
                    case Dalamud.ClientLanguage.Japanese:
                        // in logmessage.exd (409)
                        message = $"ゲームを開始してから{(flag_b ? "1500時間以上" : elapsed + "時間")}が経過しました。\n過剰なゲームの利用は正常な日常生活に支障を与える恐れがあります。";
                        break;
                    case Dalamud.ClientLanguage.English:
                        // in logmessage.exd (409)
                        // message = $"You've played more than you should. It's time you got some vitamin D.";
                        message = $"You've played more than you should. {(flag_b ? "More then 1500 hours" : elapsed + " hours")} have passed since you started the game.";
                        break;
                    case Dalamud.ClientLanguage.German:
                        // google translated
                        message = $"{(flag_b ? "Mehr als 1500" : elapsed)} Stunden sind vergangen, seit ich das Spiel gestartet habe.\nÜbermäßiger Gebrauch von Spielen kann den normalen Alltag beeinträchtigen.";
                        break;
                    case Dalamud.ClientLanguage.French:
                        // google translated
                        message = $"{(flag_b ? "Plus de 1500 " : elapsed)} heures se sont écoulées depuis que j'ai commencé le jeu.\nL'utilisation excessive de jeux peut interférer avec la vie quotidienne normale.";
                        break;
                }

                try
                {
                    ChatGui.PrintChat(new()
                    {
                        Message = message,
                        Type = XivChatType.SystemMessage
                    });
                }
                catch
                {

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
            Timer.Stop();
            Timer.Dispose();
            ClientState.Login -= ClientState_Login;
            GC.SuppressFinalize(this);
        }
    }
}
#pragma warning restore CA1416
