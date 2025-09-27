using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Models
{
    public record RegisterRequest(
       string Login,
       string Password,
       string Email
   );
}
