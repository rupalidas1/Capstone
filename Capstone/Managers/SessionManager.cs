﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Capstone.Managers
{
    public static class SessionManager
    {
        public static int SessionID { get; set; }
        public static string Username { get; set; }
        public static bool LoggedIn { get; set; }
    }
}