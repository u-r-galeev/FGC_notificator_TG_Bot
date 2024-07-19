using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGC_notificator_TG_Bot.Challonge
{
    public class match
    {
        public int id;
        public int tournament_id;
        public string state;
        public int? player1_id;
        public int? player2_id;
        public int? player1_prereq_match_id;
        public int? player2_prereq_match_id;
        public int? winner_id;
        public int? loser_id;
        public int round;
    }
}
