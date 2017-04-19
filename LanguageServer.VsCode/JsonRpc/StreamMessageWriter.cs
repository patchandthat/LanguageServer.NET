﻿using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LanguageServer.VsCode.JsonRpc
{
    public class StreamMessageWriter : MessageWriter
    {
        public StreamMessageWriter(Stream stream)
            : this(stream, Encoding.UTF8, null)
        {
        }

        public StreamMessageWriter(Stream stream, IStreamMessageLogger messageLogger) : this(stream, Encoding.UTF8, messageLogger)
        {
        }

        public StreamMessageWriter(Stream stream, Encoding encoding, IStreamMessageLogger messageLogger)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));
            BaseStream = stream;
            Encoding = encoding;
            MessageLogger = messageLogger;
        }

        public Stream BaseStream { get; }

        public Encoding Encoding { get; }

        public IStreamMessageLogger MessageLogger { get; }

        public override void Write(Message message)
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding, 4096, true))
                {
                    if (MessageLogger == null)
                    {
                        RpcSerializer.SerializeMessage(writer, message);
                    }
                    else
                    {
                        var content = RpcSerializer.SerializeMessage(message);
                        MessageLogger.NotifyMessageSent(content);
                        writer.Write(content);
                    }
                }
                using (var writer = new StreamWriter(BaseStream, Encoding, 4096, true))
                {
                    writer.Write("Content-Length: ");
                    writer.Write(ms.Length);
                    writer.Write("\r\nContent-Type: application/vscode-jsonrpc; charset=utf8\r\n\r\n");
                }
                ms.CopyTo(BaseStream);
            }
        }
    }
}
