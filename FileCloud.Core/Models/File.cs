using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCloud.Core.Models
{
    public class File
    {
        private File(Guid id, string name, string path)
        {
            this.id = id;
            Name = name;
            Path = path;
        }
        public Guid id { get; }
        public string Name { get; }
        public string Path { get; }

        public static (File file, string Error) Create(Guid id, string name, string path)
        {
            var error = string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                error = "incorrect file name";
            }

            var file = new File(id, name, path);

            return (file, error);
        }
    }
}
