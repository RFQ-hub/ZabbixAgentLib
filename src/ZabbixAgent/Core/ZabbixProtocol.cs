using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Itg.ZabbixAgent.Core
{
    internal class ZabbixProtocol
    {
        private static readonly byte[] headerBytes = Encoding.ASCII.GetBytes(ZabbixConstants.HeaderString);

        private static readonly byte[] zero = new byte[ 0 ];

        /// <summary>
        /// Write a string to the stream prefixed with it's size and the Zabbix protocol header.
        /// </summary>
        public static void WriteWithHeader([NotNull] Stream stream, [NotNull] string valueString, [CanBeNull] string errorString)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Debug.Assert(stream != null);
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Debug.Assert(valueString != null);

            // <HEADER>
            stream.Write(headerBytes, 0, headerBytes.Length);

            // <DATALEN>
            var valueStringBytes = Encoding.UTF8.GetBytes(valueString);
            var sizeBytes = BitConverter.GetBytes((long)valueStringBytes.Length);
            stream.Write(sizeBytes, 0, sizeBytes.Length);

            // <DATA>
            stream.Write(valueStringBytes, 0, valueStringBytes.Length);

            if (errorString != null)
            {
                // \0
                stream.Write(zero, 0, 1);

                // <ERROR>
                var errorStringBytes = Encoding.UTF8.GetBytes(errorString);
                stream.Write(errorStringBytes, 0, errorStringBytes.Length);
            }
        }
    }
}
