namespace JTuresson.Social.IO
{
    public interface IFileManager
    {
        public bool WriteToFile(string fileName, string fileContents);
        public bool LoadFromFile(string fileName, out string result);
        public bool MoveFile(string fileName, string newFileName);
    }
}