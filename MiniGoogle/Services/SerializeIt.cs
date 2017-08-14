using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace MiniGoogle.Services
{
    public   class SerializeIt
    {
       
            public static string SerializeThis(object thing)
            {
                if (thing == null) return string.Empty;

                var xmlSerializer = new XmlSerializer(thing.GetType());

                using (var stringWriter = new StringWriter())
                {
                    using (var xmlWriter = XmlWriter.Create(stringWriter, new XmlWriterSettings { Indent = true }))
                    {
                        xmlSerializer.Serialize(xmlWriter, thing);
                        return stringWriter.ToString();
                    }
                }
            }
        }
    }