using System;
using System.IO;
using System.Text;

namespace Itg.ZabbixAgent.Core
{
    internal class ZabbixProtocol
    {
        private static readonly byte[] headerBytes = Encoding.ASCII.GetBytes(ZabbixConstants.HeaderString);

        /// <summary>
        /// Write a string to the stream prefixed with it's size and the Zabbix protocol header.
        /// </summary>
        public static void WriteWithHeader(Stream stream, string valueString)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (valueString == null)
            {
                throw new ArgumentNullException("valueString");
            }

            var valueStringBytes = Encoding.UTF8.GetBytes(valueString);

            stream.Write(headerBytes, 0, headerBytes.Length);
            var sizeBytes = BitConverter.GetBytes((long)valueStringBytes.Length);
            stream.Write(sizeBytes, 0, sizeBytes.Length);
            stream.Write(valueStringBytes, 0, valueStringBytes.Length);
        }
    }
}
