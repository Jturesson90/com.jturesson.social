using System;
using System.IO;
using UnityEngine;

namespace JTuresson.Social.IO
{
    public class FileManager : IFileManager
    {
        public bool WriteToFile(string fileName, string fileContents)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                File.WriteAllText(fullPath, fileContents);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to {fullPath} with exception {e}");
                return false;
            }
        }

        public bool LoadFromFile(string fileName, out string result)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, fileName);
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, "");
            }

            try
            {
                result = File.ReadAllText(fullPath);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read from {fullPath} with exception {e}");
                result = "";
                return false;
            }
        }

        public bool MoveFile(string fileName, string newFileName)
        {
            var fullPath = Path.Combine(Application.persistentDataPath, fileName);
            var newFullPath = Path.Combine(Application.persistentDataPath, newFileName);

            try
            {
                if (File.Exists(newFullPath))
                {
                    File.Delete(newFullPath);
                }

                if (!File.Exists(fullPath))
                {
                    return false;
                }

                File.Move(fullPath, newFullPath);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"Failed to move file from {fullPath} to {newFullPath} with exception {e}");
                return false;
            }

            return true;
        }
    }
}