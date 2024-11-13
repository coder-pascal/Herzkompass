using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Herzkompass
{
    public static class UserSession
    {
        public static int UserId { get; set; }  // Benutzer-ID speichern

        // Methode zum Zurücksetzen der Sitzung beim Logout
        public static void ClearSession()
        {
            UserId = 0;
        }
    }
}