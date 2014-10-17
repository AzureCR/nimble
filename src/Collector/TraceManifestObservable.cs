using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using Tx.Windows;


namespace Controller
{
    class TraceManifestObservable
    {
        Dictionary<Guid, ManifestReassembly> Manifests { get; set; }

        private TraceManifestObservable()
        {
            Manifests = new Dictionary<Guid, ManifestReassembly>();
        }

        public static IObservable<string> GetManifests(string session)
        {
            var reader = new TraceManifestObservable();
            return reader.CreateObservable(session);
        }

        public IObservable<string> CreateObservable(string session)
        {
            return Observable.Create<string>(obs =>
             {
                 try
                 {
                     Dictionary<Guid, string> manifestContent = new Dictionary<Guid, string>();
                     IObservable<EtwNativeEvent> all = EtwObservable.FromSession(session);
                     ManifestReassembly ra = null;
                     return all.Subscribe(e =>
                     {
                         if (TryExtractManifest(ref e, ref ra))
                         {
                             var content = ra.BuildManifestString();
                             if (!manifestContent.ContainsKey(e.ProviderId)
                                 || manifestContent[e.ProviderId] != content)
                             {
                                 manifestContent[e.ProviderId] = content;
                                 obs.OnNext(content);
                             }
                         }
                     },
                     ex => obs.OnError(ex),
                     () => obs.OnCompleted());
                 }
                 catch (Exception exception)
                 {
                     obs.OnError(exception);
                 }

                 return null;
             });
        }

        private bool TryExtractManifest(ref EtwNativeEvent e, ref ManifestReassembly reassembly)
        {
            if (e.Id != 0xfffe) // 65534
            {
                return false;
            }

            ManifestEnvelope envelope = new ManifestEnvelope();
            if (!ReadEnvelop(e, ref envelope))
            {
                return false;
            }

            ManifestReassembly ra = null;
            if (!Manifests.TryGetValue(e.ProviderId, out ra))
            {
                ra = new ManifestReassembly
                {
                    TotalChunks = envelope.TotalChunks,
                };

                Manifests.Add(e.ProviderId, ra);
            }

            ra.UpdateChunk(envelope.ChunkNumber, envelope.Chunk);
            if (ra.IsComplete)
            {
                reassembly = ra;
            }

            return ra.IsComplete;
        }

        class ManifestReassembly
        {
            public int TotalChunks { get; set; }
            private int _lastChunkNumber;
            List<string> ManifestChunk = new List<string>(1);

            public ManifestReassembly()
            {
                _lastChunkNumber = -1;
            }

            public bool IsComplete
            {
                get { return ManifestChunk.Count == TotalChunks; }
            }

            public void UpdateChunk(int chunkCount, string data)
            {
                if (_lastChunkNumber == (chunkCount - 1))
                {
                    ManifestChunk.Add(data);
                    _lastChunkNumber = chunkCount;
                }
                else if (ManifestChunk.Count > chunkCount)
                {
                    //Rewrite the data if already present
                    ManifestChunk[chunkCount] = data;
                }
            }

            public string BuildManifestString()
            {
                if (TotalChunks == 1)
                {
                    return ManifestChunk[0];
                }

                StringBuilder build = new StringBuilder();
                for (int i = 0; i < ManifestChunk.Count; i++)
                {
                    build.Append(ManifestChunk[i]);
                }
                return build.ToString();
            }
        }

        public static bool ReadEnvelop(EtwNativeEvent e, ref ManifestEnvelope envelope)
        {
            if (e.Id != ManifestEnvelopeId)
            {
                return false;
            }

            envelope.Format = (ManifestEnvelope.ManifestFormats)e.ReadByte();
            if (envelope.Format != ManifestEnvelope.ManifestFormats.SimpleXmlFormat)
                throw new Exception("Unsuported manifest format found in EventSource event" + (byte)envelope.Format);

            envelope.MajorVersion = e.ReadByte();
            envelope.MinorVersion = e.ReadByte();
            envelope.Magic = e.ReadByte();
            if (envelope.Magic != 0x5b)
                throw new Exception("Unexpected content in EventSource event that was supposed to have manifest");

            envelope.TotalChunks = e.ReadUInt16();
            envelope.ChunkNumber = e.ReadUInt16();

            envelope.Chunk = e.ReadAnsiString();

            return true;
        }

        public const int ManifestEnvelopeId = 0xfffe;

        internal struct ManifestEnvelope
        {
            public const int MaxChunkSize = 0xFF00;
            public enum ManifestFormats : byte
            {
                SimpleXmlFormat = 1,          // simply dump the XML manifest as UTF8
            }

            public ManifestFormats Format;
            public byte MajorVersion;
            public byte MinorVersion;
            public byte Magic;
            public ushort TotalChunks;
            public ushort ChunkNumber;
            public string Chunk;
        };
    }
}
