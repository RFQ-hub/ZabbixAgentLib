using System;
using System.IO;
using System.Text;

namespace Itg.ZabbixAgentLib
{
    internal class ZabbixConstants
    {
        public const string NotSupported = "ZBX_NOTSUPPORTED";
        public const string HeaderString = "ZBXD\x01";
        public static readonly byte[] HeaderBytes = Encoding.ASCII.GetBytes(HeaderString);
    }

    internal class ZabbixProtocol
    {
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

            stream.Write(ZabbixConstants.HeaderBytes, 0, ZabbixConstants.HeaderBytes.Length);
            var sizeBytes = BitConverter.GetBytes((long)valueStringBytes.Length);
            stream.Write(sizeBytes, 0, sizeBytes.Length);
            stream.Write(valueStringBytes, 0, valueStringBytes.Length);
        }
    }
}
