using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGC_notificator_TG_Bot.Challonge
{
    public class Participant
    {
        public int id;
        public int tournament_id;
        public int? final_rank;
        public string display_name_with_invitation_email_address;
    }
}
