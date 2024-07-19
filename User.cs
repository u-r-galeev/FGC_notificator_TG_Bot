using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace FGC_notificator_TG_Bot
{

    internal class User
    {
        public string Name { get; set; }
        public long ID { get; set; }
        public string function;
        public Chat chat;
        public InlineKeyboardMarkup inLinePlayerList = new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>());
        public List<Player> Players { get; private set; } = new List<Player>();

        public InlineKeyboardMarkup GetUsrPlayerList()
        {
            InlineKeyboardMarkup kB = inLinePlayerList;
            foreach (Player pl in GetPlayers())
            {
                InlineKeyboardButton button = InlineKeyboardButton.WithCallbackData(pl.nickName, pl.nickName);
                kB = new InlineKeyboardMarkup(kB.InlineKeyboard.Append(new[] { button }));
            }
            InlineKeyboardButton cancel = InlineKeyboardButton.WithCallbackData("Отмена", Program.cancelButton);
            kB = new InlineKeyboardMarkup(kB.InlineKeyboard.Append(new[] { cancel }));
            return kB;
        }
        public Chat GetChatID()
        {
            return chat;
        }
        public List<Player> GetPlayers()
        {
            return Players;
        }
        public List<string> ShowPlayerNickNames(bool inline = true)
        {
            List<string> list = new List<string>();
            foreach (Player player in Players)
            {
                list.Add(player.nickName);
            }
            return list;
        }
        public static User GetUserFromID(long ID)
        {
            if (Program.Users == null || Program.Users.Count < 0)
            { return null; }
            else
            {
                foreach (User user in Program.Users)
                {
                    if (user.ID == ID)
                    { return user; }
                }
            }
            return null;
        }
        public void NullFunction()
        {
            function = null;
        }
        public string GetFunction()
        {
            return function;
        }
        public void SetFunction(string function)
        {
            this.function = function;
        }
        public User(Telegram.Bot.Types.User user, Chat chat = null)
        {
            this.Name = user.FirstName;
            this.ID = user.Id;
            this.chat = chat;

            Program.Users.Add(this);
        }
        public User()
        {
            Name = "newName";
            ID = 0;
        }


        //вывести список участников
        public string ShowPlayers()
        {
            string list = "Удалить учатника:\n";
            foreach (Player player in Players)
            {
                list += $"/{player.nickName}\n";
            }
            list += "/cancel";
            return list;
        }

        public void AddPlayerToUserList(Player player)
        {
            player.addTgToPlayer(User.GetUserFromID(ID));
            Players.Add(player);
        }

        public void RemovePlayerFromUserList(Player player)
        {
            player.removeTgFromPlayer(GetUserFromID(ID));
            Players.Remove(player);
        }

        static public bool IsRegistred(Telegram.Bot.Types.User user, Chat chat = null)
        {
            foreach(User tgUser in Program.Users)
            {
                if(tgUser.Name == user.FirstName)
                {
                    if (tgUser.chat != chat)
                    { tgUser.chat = chat; }
                    return true;
                }
            }
            Program.Users.Add(new User(user, chat));
            return false;
        }
        public void SendNote()
        {

        }
    }
}
