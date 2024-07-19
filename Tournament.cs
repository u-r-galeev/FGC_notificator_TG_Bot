using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace FGC_notificator_TG_Bot
{
    internal class Tournament
    {
        public string tournamentName;
        public string tournamentLink;
        int id;
        public List<Challonge.ChallongeMain> tournamentMatches { get; set; }
        public bool isRunning = false;
        public bool needNotification = false;
        public List<Player> Players = new List<Player>();
        public InlineKeyboardMarkup inLinePlayerList = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>());


        public InlineKeyboardMarkup InlineTrnPlayerList()
        {
            InlineKeyboardMarkup kB = inLinePlayerList;
            foreach (Player pl in Players)
            {
                InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData(pl.nickName, pl.nickName);
                kB = new InlineKeyboardMarkup(kB.InlineKeyboard.Append(new[] { button }));
            }
            InlineKeyboardButton cancel = InlineKeyboardButton.WithCallbackData("Отмена", Program.cancelButton);
            kB = new InlineKeyboardMarkup(kB.InlineKeyboard.Append(new[] { cancel }));
            return kB;
        }

        public string GetPair(int matchId)
        {
            foreach (Challonge.ChallongeMain match in tournamentMatches)
            {
                if (match.match.id == matchId)
                {
                    if (match.match.player1_id.GetValueOrDefault(0) != 0 && match.match.player2_id.GetValueOrDefault(0) != 0)
                    {
                        return $@"{GetPlayerFromId(match.match.player1_id.GetValueOrDefault(0)).nickName} - {GetPlayerFromId(match.match.player2_id.GetValueOrDefault(0))}";
                        break;
                    }
                }
            }
            return "";
        }

            public string GetTrnName() { return tournamentName; }
            public void AddPlayerToTrn(string nickName, int id, int tournamentId)
            {
                bool isRegistred = false;
                foreach (Player player in Players)
                {
                    if (nickName == player.nickName)
                    {
                        isRegistred = true;
                        break;
                    }
                }
                if (!isRegistred)
                {
                    Players.Add(new Player(nickName, id, tournamentId));
                }
            }

            public Tournament(string disciplineName, string link, int id)
            {
                this.tournamentName = disciplineName;
                this.tournamentLink = link;
                this.id = id;
            }


            public string ShowPlayers()
            {
                Tournament.ReferenceEquals(this, this.Players);
                string list = "Список игроков:\n";
                foreach (Player player in Players)
                {
                    list += $"/{player.nickName}\n";
                }
                list += "/cancel";
                return list;
            }
            public List<string> ShowPlayers(bool inline = true)
            {
                List<string> list = new List<string>();
                foreach (Player player in Players)
                {
                    list.Add(player.nickName);
                }
                return list;
            }

            public void AddPlayer(Player player)
            {
                if (Players.Contains(player))
                { Players.Add(player); }
            }
            public Player GetPlayerFromId(int id)
            {
                foreach (Player player in Players)
                { if (player.id == id) return player; }
                return null;
            }
            public void SendMessageToUser(int plId, string function, string bothNames = null)
            {
                Player player = GetPlayerFromId(plId);

                player.SendNote(player, function, bothNames);

            }

        }

    }
