using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using RestSharp.Serializers;
using RestSharp;
using RestSharp.Authenticators;
using System.Runtime.Remoting.Contexts;
using System.Net.Http;
using FGC_notificator_TG_Bot.Challonge;
using System.Text.Json.Nodes;
using System.Security.Cryptography.X509Certificates;
using System.Numerics;

namespace FGC_notificator_TG_Bot
{
    public class Api_info
    {
        public static string apiKeyRoot = "fTqmw1ldo77MneelZRChaWor31GnrMomEUbsyMQo";
        public string _apiKey = apiKeyRoot;
        readonly string url = "https://api.challonge.com/v1/";
        string link;
        string anLink;
        string function;
        string trnId;
        public string Response { get; set; }
        public List<string> ResponseList { get; set; }
        public JsonArray jsonArray;
        public Challonge.ChallongeMain cM;

        public Api_info(string function, string link, string anLink = null)
        {
            this.link = link;
            Console.WriteLine(link);
            this.anLink = anLink;
            this.function = function;
            this.trnId = link;

            //if (link.Contains("/ru/"))
            //{
            //    trnId = link.Substring(link.IndexOf("/ru/") + 4);
            //}
            //else
            //{
            //    trnId =  link.Substring(link.IndexOf("com/") + 4);
            //}
        }
        public void NewTournament(string trnLink, string anLink = null)
        {
            //WebRequest webRequest = (WebRequest)WebRequest.Create($"{url}{trnLink.Split('/').Last()}.json?Nagibator228:{_apiKey}");

            //WebResponse webResponse;
            //try
            //{
            //    webResponse = (WebResponse)webRequest.GetResponse();
            //}
            //catch (WebException ex)
            //{
            //    Console.WriteLine(ex);
            //    return;
            //}
            //webResponse = (WebResponse)webRequest.GetResponse();

            //string response;

            //using (StreamReader reader = new StreamReader(webResponse.GetResponseStream()))
            //{
            //    //Перенос данных из JSON в строковую переменную
            //    response = reader.ReadToEnd();
            //}

            //webResponse.Close();

            //Challonge.ChallongeMain info = JsonConvert.DeserializeObject<Challonge.ChallongeMain>(response);

            //Console.WriteLine(info.ToString());
        }

        public void StartTournament2()
        {
            HttpClient client = new HttpClient();
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($@"{url}/tournaments/{trnId}.json?api_key={_apiKey}");
            //httpWebRequest.ContentType = @"application/json";
            httpWebRequest.Host = "challonge.com";
            httpWebRequest.UserAgent = "PostmanRuntime/7.37.0";
            httpWebRequest.Accept = @"*/*";
            //httpWebRequest.AcceptEncoding = "gzip, deflate, br";
            //httpWebRequest.Connection = "keep-alive";
            httpWebRequest.Method = @"Get";
            httpWebRequest.Headers.Add(@"api_key", _apiKey);

            //string jsonData = client.GetAsync(httpWebRequest).Result;

            HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse();

            //выходит ошибка
            //System.Net.ProtocolViolationException: Невозможно отправить тело содержимого с данным типом предиката.
            //в System.Net.HttpWebRequest.CheckProtocol(Boolean onRequestStream)
            //в System.Net.HttpWebRequest.GetRequestStream(TransportContext & context)
            //в System.Net.HttpWebRequest.GetRequestStream()
            //в FGC_notificator_TG_Bot.Api_info.StartTournament() в C:\Coding\cSharp\MyProjects\Телеграм бот для турниров\FGC_notificator_TG_Bot\Api_info.cs:строка 80
            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                // we want to remove new line characters otherwise it will return an error
                StreamReader stream = new StreamReader(response.GetResponseStream());
                if (stream != null) { Response = stream.ReadToEnd(); }
            }

            response.Close();


            Console.WriteLine(Response.ToString());
        }

        public void ApiHueta()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var endpoint = new Uri($@"{url}/tournaments/{trnId}.json?api_key={_apiKey}");

                    var result = client.GetAsync(endpoint).Result;

                    var Response = result.Content.ReadAsStringAsync().Result;
                    cM = JsonConvert.DeserializeObject<Challonge.ChallongeMain>(Response);
                }
                catch (Exception ex) { Console.WriteLine(ex); }
            }
            Program._tournament = new Tournament(cM.tournament.game_name, "challonge.com/ru/" + cM.tournament.url, cM.tournament.id);
            Program._tournament.isRunning = true;
            Program._tournament.tournamentMatches = new List<ChallongeMain>();

            Console.WriteLine(Program._tournament.GetTrnName());

        }

        public void ApiHuetaMatchList()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var endpoint = new Uri($@"{url}/tournaments/{trnId}/matches.json?api_key={_apiKey}");
                    string response = client.GetAsync(endpoint).Result.Content.ReadAsStringAsync().Result;
                    List<ChallongeMain> matches = JsonConvert.DeserializeObject<List<ChallongeMain>>(response);
                    foreach (ChallongeMain thisMatch in matches)
                    {
                        match tmpMatch = new match();
                        int? GetNull(int? tmp)
                        {
                            if (tmp == null)
                            { return 0; }
                            else { return tmp; }
                        }
                        thisMatch.match.player2_id = GetNull((thisMatch.match.player2_id));
                        thisMatch.match.player1_id = GetNull(GetNull(thisMatch.match.player1_id));
                        thisMatch.match.player1_prereq_match_id = GetNull(thisMatch.match.player1_prereq_match_id);
                        thisMatch.match.player2_prereq_match_id = GetNull(thisMatch.match.player2_prereq_match_id);
                        thisMatch.match.winner_id = GetNull(thisMatch.match.winner_id);
                        thisMatch.match.loser_id = GetNull(GetNull(thisMatch.match.loser_id));




                        Program._tournament.tournamentMatches.Add(thisMatch);
                    }

                }
                catch (Exception e) { Console.WriteLine(e); }
                List<int?> players_id = new List<int?>();

                foreach (ChallongeMain match in Program._tournament.tournamentMatches)
                {
                    bool isFind = false;
                    int? pl1 = match.match.player1_id;
                    foreach (int player in players_id)
                    {
                        if (pl1 == player)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind && pl1 != 0)
                    { players_id.Add(pl1.GetValueOrDefault(0)); }
                }


                foreach (ChallongeMain match in Program._tournament.tournamentMatches)
                {
                    bool isFind = false;
                    int? pl1 = match.match.player2_id;
                    foreach (int player in players_id)
                    {
                        if (pl1 == player)
                        {
                            isFind = true;
                            break;
                        }
                    }
                    if (!isFind && pl1 != 0)
                    { players_id.Add(pl1.GetValueOrDefault(0)); }
                }

                foreach (int player in players_id)
                {
                    try
                    {
                        var endpoint = new Uri($@"{url}/tournaments/{trnId}/participants/{player}.json?api_key={_apiKey}");
                        string response = client.GetAsync(endpoint).Result.Content.ReadAsStringAsync().Result;
                        ChallongeMain p = JsonConvert.DeserializeObject<ChallongeMain>(response);

                        Program._tournament.AddPlayerToTrn(p.Participant.display_name_with_invitation_email_address,
                            p.Participant.id, p.Participant.tournament_id);

                    }
                    catch (Exception ex) { Console.WriteLine(ex); }

                }
            }
        }

        void GetAnPeriod()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var endpoint = new Uri($@"{url}/tournaments/{trnId}/matches.json?api_key={_apiKey}");
                    string response = client.GetAsync(endpoint).Result.Content.ReadAsStringAsync().Result;
                    List<ChallongeMain> matches = JsonConvert.DeserializeObject<List<ChallongeMain>>(response);
                    foreach (ChallongeMain thisMatch in matches)
                    {
                        //Реализация детектинга матчей и передача сообщений
                        foreach (ChallongeMain match in Program._tournament.tournamentMatches)
                        {
                            if (thisMatch.match.id == match.match.id)
                            {
                                //отправление результатов
                                if (thisMatch.match.winner_id.GetValueOrDefault(0) != 0 && thisMatch.match.winner_id.GetValueOrDefault(0) != match.match.winner_id)
                                {
                                    match.match.winner_id = thisMatch.match.winner_id.GetValueOrDefault(0);
                                    match.match.loser_id = thisMatch.match.loser_id.GetValueOrDefault(0);
                                    Program._tournament.SendMessageToUser(Convert.ToInt32(match.match.winner_id), "winMessage");
                                    Program._tournament.SendMessageToUser(Convert.ToInt32(match.match.loser_id), "loseMessage");
                                }

                                //обработка оповещений
                                if (thisMatch.match.player1_id.GetValueOrDefault(0) != 0 && thisMatch.match.player1_id.GetValueOrDefault(0) != match.match.player1_id ||
                                    thisMatch.match.player2_id.GetValueOrDefault(0) != 0 && thisMatch.match.player2_id.GetValueOrDefault(0) != match.match.player2_id)
                                {
                                    //если оба игрока известны
                                    if (thisMatch.match.player1_id.GetValueOrDefault(0) != 0 && thisMatch.match.player1_id.GetValueOrDefault(0) != match.match.player1_id &&
                                    thisMatch.match.player2_id.GetValueOrDefault(0) != 0 && thisMatch.match.player2_id.GetValueOrDefault(0) != match.match.player2_id)
                                    {
                                        //обновление ид
                                        match.match.player1_id = thisMatch.match.player1_id.GetValueOrDefault(0);
                                        match.match.player2_id = thisMatch.match.player2_id.GetValueOrDefault(0);

                                        //отправление оповещений
                                        Program._tournament.SendMessageToUser(Convert.ToInt32(match.match.player1_id), "matchFound");
                                        Program._tournament.SendMessageToUser(Convert.ToInt32(match.match.player2_id), "matchFound");
                                    }
                                    //если игрок 1 неизвестен
                                    if (thisMatch.match.player1_id.GetValueOrDefault(0) == 0 && thisMatch.match.player1_prereq_match_id.GetValueOrDefault(0) != 0)
                                    {
                                        string bothPlayers = Program._tournament.GetPair(match.match.id);
                                        if (bothPlayers != "")
                                        { Program._tournament.SendMessageToUser(Convert.ToInt32(match.match.player2_id), "waitOpponent", bothPlayers); }
                                    }
                                    //если игрок 2 неизвестен
                                    if (thisMatch.match.player2_id.GetValueOrDefault(0) == 0 && thisMatch.match.player2_prereq_match_id.GetValueOrDefault(0) != 0)
                                    {
                                        string bothPlayers = Program._tournament.GetPair(match.match.id);
                                        if (bothPlayers != "")
                                        { Program._tournament.SendMessageToUser(Convert.ToInt32(match.match.player1_id), "waitOpponent", bothPlayers); }
                                    }
                                }


                                match.match.player1_prereq_match_id = thisMatch.match.player1_prereq_match_id.GetValueOrDefault(0);
                                match.match.player2_prereq_match_id = thisMatch.match.player2_prereq_match_id.GetValueOrDefault(0);


                                break;
                            }
                        }
                    }

                }
                catch (Exception ex)
                { Console.WriteLine(ex); }
            }
            //MakePeriod(Program.isRunning);
        }

        async public void StartTournament()
        {
            ApiHueta();
            ApiHuetaMatchList();

            while (Program._tournament.isRunning)
            {
                GetAnPeriod();
                await Task.Delay(20000);

            }
        }

        public void LoadToAPI(string command, string link = null, string anLink = null)
        {
            switch (command)
            {
                case Program.startTrn:
                    StartTournament();
                    break;
                case Program.checkPlayers:

                    break;
            }
        }
    }
}

