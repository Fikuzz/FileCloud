using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.DataAccess.Entities
{
    public class FileEntitiy
    {
        public Guid id {  get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

    }
}
