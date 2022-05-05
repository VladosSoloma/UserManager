using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class RegisterUserDto
    {
        public string Mail { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Password { get; set; }
    }
}
