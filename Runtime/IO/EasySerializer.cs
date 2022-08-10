/*
 * EasySerializer
 *
 * Author: Anton Holmquist
 * Copyright (c) 2013 Anton Holmquist. All rights reserved.
 * http://github.com/antonholmquist/easy-serializer-unity
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace JTuresson.Social.IO
{
    public static class EasySerializer
    {
        public static void SerializeObjectToFile(object serializableObject, string filePath)
        {
            SetEnvironmentVariables();

            Stream stream = File.Open(filePath, FileMode.Create);

            var formatter = new BinaryFormatter
            {
                Binder = new VersionDeserializationBinder()
            };
            formatter.Serialize(stream, serializableObject);

            stream.Close();
        }

        public static void RemoveFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public static object DeserializeObjectFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }


            EasySerializer.SetEnvironmentVariables();

            Stream stream = null;

            try
            {
                stream = File.Open(filePath, FileMode.Open);
            }
            catch (FileNotFoundException e)
            {
                return null;
            }

            var formatter = new BinaryFormatter
            {
                Binder = new VersionDeserializationBinder()
            };
            var o = formatter.Deserialize(stream);

            stream.Close();

            return o;
        }

        /* SetEnvironmentVariables required to avoid run-time code generation that will break iOS compatibility
         * Suggested by Nico de Poel:
         * http://answers.unity3d.com/questions/30930/why-did-my-binaryserialzer-stop-working.html?sort=oldest
         */
        private static void SetEnvironmentVariables()
        {
            Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        }
    }


    /* VersionDeserializationBinder is required to guarantee a fixed serialization assembly name, which Unity likes to randomize on each compile
     * Suggested by TowerOfBricks:
     * http://answers.unity3d.com/questions/8480/how-to-scrip-a-saveload-game-option.html
     * */
    public sealed class VersionDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (string.IsNullOrEmpty(assemblyName) || string.IsNullOrEmpty(typeName)) return null;
            Type typeToDeserialize = null;

            assemblyName = Assembly.GetExecutingAssembly().FullName;

            // The following line of code returns the type. 
            typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");

            return typeToDeserialize;
        }
    }
}