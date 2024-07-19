using FGC_notificator_TG_Bot.Challonge;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace FGC_notificator_TG_Bot
{
    internal class Player
    {
        public string nickName;
        public List<User> userList = new List<User>();
        public int id;
        public int tournamentId;
        public int finalRank;
        bool loserBracket = false;
        string getReady = "ваш новый противник - ";
        ITelegramBotClient botClient = Program.telegramBotClient;

        public Player(string nickName)
        {
            this.nickName = nickName;
        }
        public Player(string nickName, int id, int tournamentId)
        {
            this.nickName = nickName;
            this.id = id;
            this.tournamentId = tournamentId;
        }

        public void addTgToPlayer(User user)
        {
            userList.Add(user);
        }

        public void removeTgFromPlayer(User user)
        {
            userList.Remove(user);
        }

        public List<User> getUserList()
        {
            if (userList == null && userList.Count == 0)
            { return new List<User>(); }
            else { return userList; }
        }
        public List<long> GetTgId()
        {
            if (userList == null && userList.Count == 0)
            { return null; }
            List<long> tmp = new List<long>();
            foreach (User user in userList)
            {
                tmp.Add(user.ID);
            }
            return tmp;
        }
        public string GetReady()
        {
            for (int i = -Program._tournament.tournamentMatches.Last().match.round; i < 0; i++)
            {
                foreach (var match in Program._tournament.tournamentMatches)
                {
                    if (match.match.round == i)
                    {
                        if (!(i == -Program._tournament.tournamentMatches.Last().match.round &&
                            Program._tournament.tournamentMatches.Last().match.player2_id != 0 &&
                            Program._tournament.tournamentMatches.Last().match.player1_id != 0))
                        {
                            if (match.match.player2_id == this.id)
                            {
                                if (Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player1_id)) == null)
                                {
                                    if (match.match.player1_prereq_match_id.GetValueOrDefault(0) == 0)
                                    { return "Противник ещё не определён"; }
                                    else
                                    {
                                        foreach (var battle in Program._tournament.tournamentMatches)
                                        {
                                            string bothPlayers = Program._tournament.GetPair(battle.match.id);
                                            if (bothPlayers != "")
                                            {
                                                return WaitOpponent(bothPlayers);

                                            }
                                        }
                                    }
                                }
                                else
                                { return getReady + Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player1_id)).nickName; }
                            }
                            if (match.match.player1_id == this.id)
                            {
                                if (Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player2_id)) == null)
                                {
                                    if (match.match.player2_prereq_match_id.GetValueOrDefault(0) == 0)
                                        return "Противник ещё не определён";
                                    else
                                    {
                                        foreach (var battle in Program._tournament.tournamentMatches)
                                        {
                                            string bothPlayers = Program._tournament.GetPair(battle.match.id);
                                            if (bothPlayers != "")
                                            {
                                                return WaitOpponent(bothPlayers);

                                            }
                                        }
                                    }
                                }
                                else
                                { return getReady + Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player2_id)).nickName; }
                            }
                        }
                    }
                }
            }
            for (int i = Program._tournament.tournamentMatches.Last().match.round; i > 0; i--)
            {
                foreach (var match in Program._tournament.tournamentMatches)
                {
                    if (match.match.round == i)
                    {
                        if (match.match.player2_id == this.id)
                        {
                            if (Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player1_id)) == null)
                            {
                                if (match.match.player1_prereq_match_id.GetValueOrDefault(0) == 0)
                                { return "Противник ещё не определён"; }
                                else
                                {
                                    foreach (var battle in Program._tournament.tournamentMatches)
                                    {
                                        string bothPlayers = Program._tournament.GetPair(battle.match.id);
                                        if (bothPlayers != "")
                                        {
                                            return WaitOpponent(bothPlayers);

                                        }
                                    }
                                }
                            }
                            else
                            { return getReady + Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player1_id)).nickName; }
                        }
                        if (match.match.player1_id == this.id)
                        {
                            if (Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player2_id)) == null)
                            {
                                if (match.match.player2_prereq_match_id.GetValueOrDefault(0) == 0)
                                    return "Противник ещё не определён";
                                else
                                {
                                    foreach (var battle in Program._tournament.tournamentMatches)
                                    {
                                        string bothPlayers = Program._tournament.GetPair(battle.match.id);
                                        if (bothPlayers != "")
                                        {
                                            return WaitOpponent(bothPlayers);

                                        }
                                    }
                                }
                            }
                            else
                            { return getReady + Program._tournament.GetPlayerFromId(Convert.ToInt32(match.match.player2_id)).nickName; }
                        }
                    }
                }
            }
            return getReady;
        }
        public string LosingBattle(Player player)
        {
            if (!loserBracket)
            {
                player.loserBracket = true;
                return Program.lowBracketText;
            }
            else
            {
                return Program.loseText;
            }
        }

        public string WinnigBattle(Player player)
        {
            return $"{player.nickName} победил";
        }

        public string WaitOpponent(string text)
        {
            return $@", вашим следующим противником будет игрок из пары {text}. ";
        }

        async public void SendNote(Player player, string function, string bothNames)
        {
            string message = "";
            string f = function;
            switch (f)
            {
                case "matchFound":
                    message = player.nickName + ", " + GetReady();
                    break;
                case "winMessage":
                    message = WinnigBattle(player);
                    break;
                case "loseMessage":
                    message = player.nickName + " " + LosingBattle(player);
                    break;
                case "waitOpponent":
                    message = player.nickName + WaitOpponent(bothNames);
                    break;
                default:
                    break;

            }
            if (player.getUserList().Count != 0 && player.getUserList() != null)
            {
                foreach (User user in player.getUserList())
                {
                    await botClient.SendTextMessageAsync(
                       chatId: user.ID,
                       message);
                }
            }
        }
    }
}
