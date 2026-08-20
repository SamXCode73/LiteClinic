<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    internal class LoginTracker
    {
        public int LoginId { get; set; }
        public string? UserId { get; set; }
        public DateTime LoginTime { get; set; }

        public string? DeviceInfo { get; set; }
        public string? IPAddress { get; set; }
        public string? Location { get; set; }

        public string LoginStatus { get; set; } = "Success"; // Should be 'Success', 'Failed', or 'LockedOut'
        public string? LoginMethod { get; set; }
        public string? AppVersion { get; set; }
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiteClinic.Models
{
    internal class LoginTracker
    {
        public int LoginId { get; set; }
        public string? UserId { get; set; }
        public DateTime LoginTime { get; set; }

        public string? DeviceInfo { get; set; }
        public string? IPAddress { get; set; }
        public string? Location { get; set; }

        public string LoginStatus { get; set; } = "Success"; // Should be 'Success', 'Failed', or 'LockedOut'
        public string? LoginMethod { get; set; }
        public string? AppVersion { get; set; }
    }
}
>>>>>>> 9bd97308ed79d11fb3a9601f83e76357c193962c
