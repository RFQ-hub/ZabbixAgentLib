using System;
using System.IO;
using System.Text;

namespace Itg.ZabbixAgentLib
{
    internal class ZabbixProtocol
    {
        public const string NOT_SUPPORTED = "ZBX_NOTSUPPORTED";

        private const string HEADER_STRING = "ZBXD\x01";
        private static readonly byte[] HeaderBytes = Encoding.ASCII.GetBytes(HEADER_STRING);

        /// <summary>
        /// Write a string to the stream prefixed with it's size and the Zabbix protocol header.
        /// </summary>
        public static void WriteWithHeader(Stream stream, string valueString)
        {
            if (stream == null) throw new ArgumentNullException("stream");
            if (valueString == null) throw new ArgumentNullException("valueString");

            var valueStringBytes = Encoding.UTF8.GetBytes(valueString);

            stream.Write(HeaderBytes, 0, HeaderBytes.Length);
            var sizeBytes = BitConverter.GetBytes((long)valueStringBytes.Length);
            stream.Write(sizeBytes, 0, sizeBytes.Length);
            stream.Write(valueStringBytes, 0, valueStringBytes.Length);
        }
    }
}