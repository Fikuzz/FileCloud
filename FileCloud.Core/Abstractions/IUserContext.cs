using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Abstractions
{
    public interface IUserContext
    {
        Guid? UserId { get; }
        string? Login { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
    }
}
