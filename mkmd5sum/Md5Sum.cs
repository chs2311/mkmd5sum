using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace mkmd5sum
{
    [XmlRoot("MD5Sum")]
    public class Md5Sum
    {
        [XmlElement("File")]
        public List<SFile> Files = new List<SFile>();

        public void Save(string path)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Md5Sum));
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, this);
            }
        }

        public static Md5Sum Load(string path) 
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Md5Sum));
            using (StreamReader reader = new StreamReader(path))
            {
                return (Md5Sum)serializer.Deserialize(reader);
            }
        }
    }

    public class SFile
    {
        [XmlText]
        public string Filename { get; set; }

        [XmlAttribute("MD5")]
        public string Checksum { get; set; }
    }
}
