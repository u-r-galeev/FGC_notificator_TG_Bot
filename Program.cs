using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Dynamic;

namespace FGC_notificator_TG_Bot
{
    class Program
    {
        //список всех пользователей бота (очищается при отключении)
        public static List<User> Users { get; private set; } = new List<User>();
        public static List<Player> Players { get; private set; } = new List<Player>();
        private static Api_info _apiConnect;
        public static Tournament _tournament = new Tournament("", "", 0);
        public static bool tournamentIsStarted = false;
        public static ITelegramBotClient telegramBotClient;
        public static bool isRunning = false;
        public Tournament SelectTournament(string name, string url, int id)
        {
            Tournament runningTournament = new Tournament(name, url, id);
            return runningTournament;
        }

        public const string hello = "Добро пожаловать в бота! На данный момент он находится в тестовом режиме. Если бот будет вести себя некорректно - перезапусти его или нажми /start";
        public const string hello2 = "Бот был перезапущен без твоего ведома. Возможно, некоторый функционал не будет корректно работать (но это не точно!), поэтому нажми /start на всякий случай";
        public const string lowBracketText = "перешёл в нижнюю сетку";
        public const string loseText = "гг";
        public const string moreInfo = "Этот бот сделан для уведомления участников об их играх. Вы можете добавить в список уведомлений любых участников, " +
            "как себя, так и ваших друзей. Уведомления отправляются автоматически, по факту появления соперника в сетке с игроком из вашего списка. " +
            "На первых этапах уведомления получат все пользователи, но, если все турнирные компьютеры будут заняты, нужно будет дождаться их освобождения " +
            "или команды ведущего на отыгрыш вашей пары.\nТурниры проходят до трёх побед (если изменение не будет оглашено во время старта) по системе " +
            "Full double elimination. В ней участники, проигравшие одному сопернику, не вылетают из турнира, а продолжают играть среди тех же, кто потерпел " +
            "поражение. После поражения от второго соперника участие на турнире завершается.\nСсылки на стеки для турниров:\nGuilty Gear -Strive- " +
            "https://challonge.com/ru/fenix24ggs\nStreet Fighter 6 https://challonge.com/ru/fenix24sf6n\nTekken 8 https://challonge.com/ru/fenix24t8\n" +
            "Mortal Kombat 1 https://challonge.com/ru/fenix24mk1\n" +
            "\nВне турниров вы так же можете свободно играть с гостями фестиваля в запущенные файтинги или попросить организаторов поставить другие. " +
            "Это отличный способ сыграть без соревновательного напряжения, узнать что-то новое, а так же познакомиться с новыми боевыми товарищами. " +
            "Большая просьба уважать участников и зрителей фестиваля. Если вы отыграли более трёх игр, а за вами собрались следующие желающие -- освободите им место. " +
            "Не ругайтесь громко -- на фестивале присутствуют дети. Когда судья объявит о старте турнира, вам необходимо освободить компьютер для участников." +
            "\n\nЭто первая релизная версия бота, поэтому в нём могут быть ошибки разной степени " +
            "критичности. Если бот ведёт себя некорректно, рекомендуется перезапустить его или нажать /start. Если выходит повторное оповещение о " +
            "противнике, с которым вы уже сыграли, проверьте, дополнительно, сетку. Если игрок пропал из уведомлений, просто добавьте его снова." +
            "\nКонтакт для обратной связи @devilasuka (просьба писать об ошибках и пожеланиях вне турниров)";

        public const string info = "Дополнительная информация";
        public const string addPl = "Добавить игрока";
        public const string remPl = "Удалить игрока";
        public const string startTrn = "/NewTournament";
        public const string endTrn = "/StopTournament";
        public const string checkPlayers = "check players";
        public const string cancelButton = "/cancel";
        public const string addPlInline = "Добавить игрока";


        const string TgToken = "6877743649:AAHa180QNu7mhltaImvLGkcGW-yrC8qgJjA";


        //static Tournament TestToutnament = new Tournament("GGS", "https://challonge.com/ru/fenix24ggs")
        //{
        //    Players = new List<Player>() { new Player("pl1"), new Player("pl2"), new Player("pl3"), new Player("лох") },
        //    isRunning = true,
        //};

        //Изменить, как изучу API


        //метод сообщений



        //метод добавления участников в общий список
        public void AddPlayerToTournament(Player player)
        {
            Players.Add(player);
        }

        // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
        private static ITelegramBotClient _botClient;

        // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
        private static ReceiverOptions _receiverOptions;


        static async Task Main(string[] args)
        {
            _botClient = new TelegramBotClient(TgToken);
            telegramBotClient = _botClient;
            _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[]
                {
                    UpdateType.Message,
                    UpdateType.CallbackQuery // Inline кнопки

                },
                // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
                // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
                ThrowPendingUpdates = true,


            };

            using var cts = new CancellationTokenSource();

            // UpdateHander - обработчик приходящих Update`ов
            // ErrorHandler - обработчик ошибок, связанных с Bot API
            _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

            var me = await _botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте.
            Console.WriteLine($"{me.FirstName} запущен!");

            await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
        }

        private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            string GetPlayers(Tournament tournament)
            {
                return _tournament.ShowPlayers();
            }

            Tournament GetActTrn()
            {
                return _tournament;
            }
            string GetTrnLink(Tournament tournament)
            {
                if (tournament.tournamentLink.Length > 0)
                { return tournament.tournamentLink; }
                else { return "https://t.me/fgc_notif_bot"; }
            }


            async void SendMessage(User user, string message)
            {
                try
                {
                    await botClient.SendTextMessageAsync(
                        chatId: user.GetChatID(),
                        message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }









            //получение всех участников турнира
            // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
            try
            {
                var inlinekeyboard = new InlineKeyboardMarkup(
                                            new List<InlineKeyboardButton[]>() // здесь создаем лист (массив), который содрежит в себе массив из класса кнопок
                                            {
                                            // каждый новый массив - это дополнительные строки,
                                            // а каждая дополнительная кнопка в массиве - это добавление ряда

                                            new InlineKeyboardButton[] // тут создаем массив кнопок
                                            {

                                                InlineKeyboardButton.WithUrl("открыть сетку", GetTrnLink(GetActTrn())),
                                                InlineKeyboardButton.WithCallbackData(addPl, addPl),
                                            },
                                            new InlineKeyboardButton[]
                                            {
                                                InlineKeyboardButton.WithCallbackData("дополнительная информация", info),
                                                InlineKeyboardButton.WithCallbackData("удалить игрока", remPl),
                                            },
                                            });

                // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
                switch (update.Type)
                {

                    case UpdateType.CallbackQuery:
                        {
                            // Переменная, которая будет содержать в себе всю информацию о кнопке, которую нажали
                            var callbackQuery = update.CallbackQuery;

                            // Аналогично и с Message мы можем получить информацию о чате, о пользователе и т.д.
                            var user = callbackQuery.From;
                            User tgUser = User.GetUserFromID(user.Id);
                            // Выводим на экран нажатие кнопки
                            Console.WriteLine($"{user.FirstName} ({user.Id}) нажал на кнопку: {callbackQuery.Data}");

                            // Вот тут нужно уже быть немножко внимательным и не путаться!
                            // Мы пишем не callbackQuery.Chat , а callbackQuery.Message.Chat , так как
                            // кнопка привязана к сообщению, то мы берем информацию от сообщения.
                            var chat = callbackQuery.Message.Chat;

                            async void SendMessageToChat(String message)
                            {
                                await botClient.SendTextMessageAsync(
                                                                    chat.Id,
                                                                    message);
                            }

                            async void ShowButtons()
                            {
                                await botClient.SendTextMessageAsync(
                                    chat.Id,
                                    "Выберите действие",
                                    replyMarkup: inlinekeyboard); // все клавиатуры передаются в параметр replymarkup
                            }
                            async void Canceled()
                            {

                                await botClient.SendTextMessageAsync(
                                                chat.Id,
                                               "Возвращение в главное меню"
                                               );
                                User.GetUserFromID(user.Id).NullFunction();
                                ShowButtons();
                            }

                            if (tgUser.function != null)
                            {
                                switch (tgUser.function)
                                {
                                    case (addPl):
                                        if (callbackQuery.Data == cancelButton)
                                        {
                                            Canceled();
                                            break;
                                        }
                                        if (tgUser.ShowPlayerNickNames().Contains(callbackQuery.Data))
                                        { SendMessageToChat("Данный игрок уже есть в списке"); }
                                        else
                                        {
                                            foreach (Player player in _tournament.Players)
                                            {
                                                if (player.nickName == callbackQuery.Data)
                                                {
                                                    tgUser.AddPlayerToUserList(player);
                                                    SendMessageToChat($@"{player.nickName} добавлен в список уведомлений");
                                                    SendMessageToChat($@"{player.nickName}, {player.GetReady()}");
                                                    break;
                                                }
                                            }

                                        }
                                        Canceled();
                                        break;
                                    case (remPl):
                                        if (callbackQuery.Data == cancelButton)
                                        {
                                            Canceled();
                                            break;
                                        }
                                        if (tgUser.ShowPlayerNickNames().Contains(callbackQuery.Data))
                                        {
                                            bool isDeleted = false;
                                            foreach (Player pl in tgUser.Players)
                                            {
                                                if (pl.nickName == callbackQuery.Data)
                                                {
                                                    tgUser.RemovePlayerFromUserList(pl);
                                                    SendMessageToChat($@"{pl.nickName} удалён из списка уведомлений");
                                                    isDeleted = true;
                                                    break;
                                                }
                                            }
                                            if (!isDeleted)
                                            {
                                                SendMessageToChat("Не удалось удалить игрока. Повторите попытку");
                                            }
                                        }
                                        Canceled(); break;
                                    default:
                                        break;
                                }
                            }
                            else
                                // Добавляем блок switch для проверки кнопок
                                switch (callbackQuery.Data)
                                {
                                    // Data - это придуманный нами id кнопки, мы его указывали в параметре
                                    // callbackData при создании кнопок. У меня это button1, button2 и button3


                                    case addPl:
                                        {
                                            // В этом типе клавиатуры обязательно нужно использовать следующий метод
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);
                                            // Для того, чтобы отправить телеграмму запрос, что мы нажали на кнопку
                                          //  await botClient.SendTextMessageAsync(
                                          //      chat.Id,
                                          //GetPlayers(_tournament));
                                          
                                            if(_tournament.Players.Count == 0 || _tournament.Players == null)
                                            {
                                                SendMessageToChat("Турнир ещё не запущен. Дождитесь начала турнира");
                                                tgUser.NullFunction();
                                                break;
                                            }

                                            await botClient.SendTextMessageAsync(
                                                   chat.Id,
                                                   "Выберите игрока",
                                                   replyMarkup: _tournament.InlineTrnPlayerList()); // все клавиатуры передаются в параметр replymarkup

                                            if (!(Program.Users == null || Program.Users.Count < 0))
                                            {
                                                foreach (User usr in Users)
                                                {
                                                    if (usr.ID == user.Id)
                                                    { usr.function = (addPl); }
                                                }
                                            }
                                            else
                                            { SendMessageToChat("Ошибка регистрации. Нажимте /start или перезапустите бота"); }
                                            return;
                                        }

                                    case remPl:
                                        {
                                            // А здесь мы добавляем наш сообственный текст, который заменит слово "загрузка", когда мы нажмем на кнопку
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id, "Уведомления об этих игроках больше не будут приходить");
                                            User.IsRegistred(user);
                                            tgUser = User.GetUserFromID(user.Id);
                                            if (tgUser.Players.Count > 0)
                                            {
                                                await botClient.SendTextMessageAsync(
                                                    chat.Id,
                                                    "Список добавленных участников",
                                                    replyMarkup: tgUser.GetUsrPlayerList());
                                                tgUser.SetFunction(remPl);
                                            }
                                            else { SendMessageToChat("Уведомления не установлены ни на одного игрока"); }


                                            return;
                                        }

                                    case info:
                                        {
                                            // А тут мы добавили еще showAlert, чтобы отобразить пользователю полноценное окно
                                            await botClient.AnswerCallbackQueryAsync(callbackQuery.Id);

                                            await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                moreInfo);
                                            return;
                                        }
                                }


                            return;
                        }

                    case UpdateType.Message:
                        {
                            // Эта переменная будет содержать в себе все связанное с сообщениями
                            var message = update.Message;

                            // From - это от кого пришло сообщение (или любой другой Update)
                            var user = message.From;
                            //Объект-прослойка, без него не работает
                            User TgUser = User.GetUserFromID(user.Id);
                            // Chat - содержит всю информацию о чате
                            var chat = message.Chat;


                            // Выводим на экран то, что пишут нашему боту, а также небольшую информацию об отправителе
                            Console.WriteLine($"{user.FirstName} | {message.Text}");


                            async void SendMessageToChat(String message)
                            {
                                await botClient.SendTextMessageAsync(
                                                                    chat.Id,
                                                                    message);
                            }


                            // Добавляем проверку на тип Message
                            switch (message.Type)
                            {
                                // Тут понятно, текстовый тип
                                case MessageType.Text:
                                    {

                                        // тут создаем нашу клавиатуру


                                        async void ShowButtons()
                                        {
                                            await botClient.SendTextMessageAsync(
                                                chat.Id,
                                                "Выберите действие",
                                                replyMarkup: inlinekeyboard); // все клавиатуры передаются в параметр replymarkup
                                        }
                                        async void Canceled()
                                        {

                                            await botClient.SendTextMessageAsync(
                                                            chat.Id,
                                                           "Возвращение в главное меню"
                                                           );
                                            TgUser.NullFunction();
                                            ShowButtons();
                                        }
                                        // тут обрабатываем команду /start, остальные аналогично
                                        if (message.Text == "/start")
                                        {
                                            if (!User.IsRegistred(user, chat))
                                            {
                                                SendMessageToChat(hello);
                                            }
                                            ShowButtons();
                                            return;

                                        }

                                        if (message.Text == startTrn && user.Id == 36001635)
                                        {
                                            if (User.IsRegistred(user))
                                            {
                                                TgUser.function = startTrn;
                                                SendMessageToChat("Ссылка на турнир\n/cancel");
                                            }
                                            else { SendMessageToChat("Нажми /start"); }
                                            return;
                                        }
                                        if (message.Text == endTrn && user.Id == 36001635)
                                        {
                                            if (User.IsRegistred(user))
                                            {
                                                foreach (var user1 in Users)
                                                {
                                                    SendMessageToChat($@"Турнир {_tournament.tournamentName} завершён");
                                                }
                                                _tournament = new Tournament("", "", 0);
                                                isRunning = false;
                                                TgUser.NullFunction();
                                            }
                                            else { SendMessageToChat("Нажми /start"); }
                                            return;
                                        }

                                        if (User.IsRegistred(user))
                                        {

                                            switch (TgUser.GetFunction())
                                            {
                                                //case addPl:
                                                //    switch (message.Text)
                                                //    {
                                                //        case cancelButton:
                                                //            Canceled();
                                                //            break;

                                                //        default:

                                                //            bool isFound = false;
                                                //            foreach (Player player in _tournament.Players)
                                                //            {
                                                //                if (message.Text == "/" + player.nickName)
                                                //                {
                                                //                    isFound = true;

                                                //                    if (TgUser.Players.Contains(player))
                                                //                    {
                                                //                        SendMessageToChat("Данный игрок уже есть в списке");
                                                //                    }
                                                //                    else
                                                //                    {
                                                //                        TgUser.AddPlayerToUserList(player);
                                                //                        SendMessageToChat("Игрок добавлен в список уведомлений");
                                                //                        SendMessageToChat($@"{player.nickName}, {player.GetReady()}");
                                                //                    }
                                                //                    TgUser.NullFunction();
                                                //                    break;
                                                //                }





                                                //            }
                                                //            if (!isFound)
                                                //            {
                                                //                SendMessageToChat($"Игрок {message.Text} не найден");
                                                //            }
                                                //            TgUser.NullFunction();
                                                //            ShowButtons();
                                                //            break;
                                                //    }
                                                //    break;

                                                //case remPl:
                                                //    if (message.Text == cancelButton)
                                                //    {
                                                //        Canceled();
                                                //        break;
                                                //    }
                                                //    else
                                                //    {
                                                //        bool isDelented = false;
                                                //        foreach (Player player in TgUser.Players)
                                                //        {
                                                //            if (message.Text == "/" + player.nickName)
                                                //            {
                                                //                TgUser.RemovePlayerFromUserList(player);
                                                //                SendMessageToChat($"Игрок {player.nickName} удалён из уведомлений");
                                                //                isDelented = true;
                                                //                break;
                                                //            }
                                                //        }
                                                //        if (!isDelented)
                                                //        {
                                                //            SendMessageToChat("Не удалось удалить игрока. Повторите попытку");
                                                //        }
                                                //    }
                                                //    TgUser.NullFunction();
                                                //    ShowButtons();
                                                //    break;
                                                case startTrn:
                                                    if (message.Text == cancelButton)
                                                    { Canceled(); break; }
                                                    else
                                                    {
                                                        char[] cA = message.Text.ToCharArray();
                                                        Array.Reverse(cA);
                                                        string trnName = new string(cA);
                                                        trnName = trnName.Substring(0, trnName.IndexOf("/"));
                                                        cA = trnName.ToCharArray();
                                                        Array.Reverse(cA);
                                                        trnName = new string(cA);
                                                        Api_info trn = new Api_info(startTrn, trnName);
                                                        _apiConnect = trn;
                                                        _apiConnect.LoadToAPI(startTrn);
                                                        foreach (User user1 in Users)
                                                        {
                                                            await botClient.SendTextMessageAsync(
                                                                chatId: user1.ID,
                                                                $"Начался турнир по {_tournament.tournamentName} {_tournament.tournamentLink}");
                                                        }
                                                    }
                                                    TgUser.NullFunction();
                                                    ShowButtons();
                                                    break;
                                                case checkPlayers:

                                                    TgUser.NullFunction();
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        else { SendMessageToChat(hello2); }
                                    }

                                    //reply keyboard (useless)
                                    {
                                        //базовые настройки reply кнопок 
                                        {
                                            //    // Тут все аналогично Inline клавиатуре, только меняются классы
                                            //    // НО! Тут потребуется дополнительно указать один параметр, чтобы
                                            //    // клавиатура выглядела нормально, а не как абы что

                                            //    var replyKeyboard = new ReplyKeyboardMarkup(
                                            //        new List<KeyboardButton[]>()
                                            //        {
                                            //new KeyboardButton[]
                                            //{
                                            //    new KeyboardButton("Добавить игрока"),
                                            //    new KeyboardButton("Удалить игрока"),
                                            //},
                                            //new KeyboardButton[]
                                            //{
                                            //    new KeyboardButton("Открыть сетку"),
                                            //    new KeyboardButton("Дополнительная информация")
                                            //},
                                            //new KeyboardButton[]
                                            //{
                                            //}
                                            //        })
                                            //    {
                                            //        // автоматическое изменение размера клавиатуры, если не стоит true,
                                            //        // тогда клавиатура растягивается чуть ли не до луны,
                                            //        // проверить можете сами
                                            //        ResizeKeyboard = true,
                                            //    };

                                            //    await botClient.SendTextMessageAsync(
                                            //        chat.Id,
                                            //        "Пожалуйста, выберите нужную кнопку",
                                            //        replyMarkup: replyKeyboard); // опять передаем клавиатуру в параметр replyMarkup

                                        }

                                        //расширенные настройки reply кнопок
                                        {
                                            //    if (message.Text == "Добавить игрока")
                                            //    {
                                            //        UpdateListOfTrnPlayers(botClient, update, cancellationToken);


                                            //        break;
                                            //        //Вывод списка участников турнира в формате строки для выбора


                                            //    }

                                            //    if (message.Text == "Удалить игрока")
                                            //    {

                                            //        return;
                                            //    }

                                            //    if (message.Text == "Открыть сетку")
                                            //    {
                                            //        //переход в браузер на сетку актуального турнира
                                            //    }

                                            //    if (message.Text == "Дополнительная информация")
                                            //    {
                                            //        SendMessageToChat(info);
                                            //        return;
                                            //    }
                                        }
                                    }
                                    return;

                                // Добавил default , чтобы показать вам разницу типов Message
                                default:
                                    {
                                        await botClient.SendTextMessageAsync(
                                            chat.Id,
                                            "Используй только текст!");
                                        return;
                                    }
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                //var chat = update.Message.Chat;


                //    await botClient.SendTextMessageAsync(
                //                                        chat.Id,
                //                                        ex.ToString());

            }
        }
        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }



        //костыль в виде нового апдейт таска
    }
}
