namespace FileCloud.DataAccess.Entities;
public class FileEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    // Относительный или полный путь
    public string Path { get; set; } = string.Empty;

    // Размер файла (можно сделать nullable, если не хочешь хранить всегда)
    public long? Size { get; set; }

    // Ссылка на папку
    public Guid FolderId { get; set; }
    public FolderEntity Folder { get; set; } = null!;
}
