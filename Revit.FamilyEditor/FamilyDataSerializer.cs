using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Revit.FamilyEditor.Models;

namespace Revit.FamilyEditor
{
    internal static class FamilyDataSerializer
    {
        public static void Serialize(FamilyData data, string path)
        {
            var serializer = new DataContractJsonSerializer(typeof(FamilyData));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, data);
                string s = Encoding.UTF8.GetString(stream.ToArray());
                File.WriteAllText(path, s);
            }
        }

        public static FamilyData Deserialize(string path)
        {
            var serializer = new DataContractJsonSerializer(typeof(FamilyData));
            string json = File.ReadAllText(path);
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
            {
                return (FamilyData)serializer.ReadObject(stream);
            }
        }
    }
}