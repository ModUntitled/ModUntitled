using System;
using System.IO;
using System.Reflection;

namespace ModUntitled {
    public class ResourceUtil {
        public static StreamReader GetStreamReader(string name) {
            return new StreamReader(GetStream(name));
        }

        public static Stream GetStream(string name) {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        }

        //https://stackoverflow.com/a/5867371
        public static byte[] GetBytes(string name) {
            var bytes = default(byte[]);
            var stream = GetStream(name);

            using (var memstream = new MemoryStream()) {
                var buffer = new byte[512];
                var bytesRead = default(int);
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    memstream.Write(buffer, 0, bytesRead);
                bytes = memstream.ToArray();
            }

            return bytes;
        }

        public static string GetText(string name) {
            using (var reader = GetStreamReader(name)) {
                return reader.ReadToEnd();
            }
        }
    }
}
