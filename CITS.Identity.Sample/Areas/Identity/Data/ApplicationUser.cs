using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace CITS.Identity.Sample.Areas.Identity.Data
{

	public class ApplicationUser : CITS.Identity.Dapper.IdentityUser
	{
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public int emailConfirmed { get; set; }
        public int phoneNumberConfirmed { get; set; }
        public int twoFactorEnabled { get; set; }
    }
}
