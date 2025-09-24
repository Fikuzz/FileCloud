namespace FileCloud.DataAccess.Entities
{
    public class FolderEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public bool IsRoot { get; set; }

        // Родительская папка
        public Guid? ParentId { get; set; }
        public FolderEntity? Parent { get; set; }

        // Дочерние папки
        public ICollection<FolderEntity> SubFolders { get; set; } = new List<FolderEntity>();

        // Файлы в папке
        public ICollection<FileEntity> Files { get; set; } = new List<FileEntity>();

        // Владелец папки
        public Guid OwnerId { get; set; }
        public virtual UserEntity? Owner { get; set; }
    }
}