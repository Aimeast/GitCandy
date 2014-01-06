using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GitCandy.Base
{
    public static class FileHelper
    {
        public const string BinaryMimeType = "application/octet-stream";

        public static readonly IReadOnlyDictionary<string, string> BrushMapping;
        //public static readonly IReadOnlyDictionary<string, string> AchiveMapping;
        public static readonly IReadOnlyCollection<string> ImageSet;

        private static readonly string[] SizeUnits = { "B", "KiB", "MiB", "GiB", "TiB" };
        private static readonly Encoding[] BomMapping =
        {
            new UTF8Encoding(true),             // UTF-8
            new UnicodeEncoding(false, true),   // UTF-16 (LE)
            new UnicodeEncoding(true, true),    // UTF-16 (BE)
            new UTF32Encoding(false, true),     // UTF-32 (LE)
            new UTF32Encoding(true, true),      // UTF-32 (BE)
        };
        private static readonly byte[][] Boms;

        static FileHelper()
        {
            BrushMapping = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { ".sh", "hash" },
                { ".cs", "cs" },
                { ".h", "cpp" },
                { ".hh", "cpp" },
                { ".c", "cpp" },
                { ".cc", "cpp" },
                { ".cp", "cpp" },
                { ".cpp", "cpp" },
                { ".c++", "cpp" },
                { ".cxx", "cpp" },
                { ".css", "css" },
                { ".ini", "ini" },
                { ".json", "json" },
                { ".java", "java" },
                { ".js", "javascript" },
                { ".jscript", "javascript" },
                { ".javascript", "javascript" },
                { ".php", "php" },
                { ".pl", "perl" },
                { ".py", "python" },
                { ".rb", "ruby" },
                { ".sql", "sql" },
                { ".as", "actionscript" },
                { ".applescript", "applescript" },
                { ".bf", "brainfuck" },
                { ".cmake", "cmake" },
                { ".clj", "clojure" },
                { ".coffee", "coffeescript" },
                { ".bat", "dos" },
                { ".cmd", "dos" },
                { ".pas", "delphi" },
                { ".erl", "erlang" },
                { ".fs", "fsharp" },
                { ".fsx", "fsharp" },
                { ".haml", "haml" },
                { ".go", "go" },
                { ".hs", "haskell" },
                { ".lisp", "lisp" },
                { ".lsp", "lisp" },
                { ".cl", "lisp" },
                { ".lua", "lua" },
                { ".md", "markdown" },
                { ".m", "matlab" },
                { ".rs", "rust" },
                { ".scala", "scala" },
                { ".vb", "vbnet" },
                { ".vbs", "vbscript" },
                
                { ".xml", "xml" },
                { ".htm", "xml" },
                { ".html", "xml" },
                { ".shtml", "xml" },
                { ".webp", "xml" },
                { ".xht", "xml" },
                { ".xhtml", "xml" },
                { ".config", "xml" },
                { ".vssettings", "xml" },
                { ".csproj", "xml" },
                { ".vbproj", "xml" },
                { ".resx", "xml" },
                { ".xaml", "xml" },
                { ".vsmdi", "xml" },
                { ".testsettings", "xml" },

                // Thanks for st52 <130990851@qq.com> list the extensions of action script
                { ".old", "xml" },
                { ".as3proj", "xml" },
                { ".actionscriptproperties", "xml" },
                { ".project", "xml" },
                { ".morn", "xml" },

                { ".diff", "diff" },
                { ".patch", "diff" },
                { ".http", "http" },
            });

            //AchiveMapping = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            //{
            //    { "zip", "application/zip" },
            //    { "gz", "application/x-gzip" },
            //    { "tar.gz", "application/x-tgz" },
            //});

            ImageSet = new ReadOnlyCollection<string>(new List<string>
            {
                ".bmp",
                ".gif",
                ".jpeg", ".jpg", ".jpe",
                ".png",
                ".svg", ".svgz",
                ".tiff", ".tif",
                ".ico",
                ".pbm",
            });

            Boms = BomMapping.Select(s => s.GetPreamble()).ToArray();
        }

        public static string GetBrush(string path)
        {
            var extension = Path.GetExtension(path).ToLower();
            if (BrushMapping.ContainsKey(extension))
                return BrushMapping[extension];
            return "no-highlight";
        }

        public static Encoding DetectEncoding(byte[] bytes, params Encoding[] encodings)
        {
            var index = BomIndex(bytes);
            if (index != -1) // Read BOM if existing
                return BomMapping[index];

            if (bytes.Length > 1024)
                bytes = bytes.Take(1024).ToArray();

            var collection = encodings.Concat(new[]
                    {
                        // generic
                        Encoding.UTF8,
                        // default encoding for user selected UI lanaguage
                        Encoding.GetEncoding(CultureInfo.CurrentUICulture.TextInfo.ANSICodePage),
                        // as more as possible, default encoding for server side
                        Encoding.Default,
                    })
                .Where(s => s != null)
                .GroupBy(s => s.CodePage)
                .Select(s => s.First())
                .ToArray();

            var hasNonSingleByte = collection.Any(s => !s.IsSingleByte);
            if (hasNonSingleByte)
            {
                var pendings = collection
                    .Where(s => IsMatchEncoding(s, bytes))
                    .ToArray();

                return pendings.FirstOrDefault(s => !s.IsSingleByte)
                    ?? pendings.FirstOrDefault();
            }
            else
            {
                return collection.FirstOrDefault(s => IsMatchEncoding(s, bytes));
            }
        }

        public static string ReadToEnd(byte[] bytes, Encoding encoding = null, string newline = null)
        {
            using (var reader = new StreamReader(new MemoryStream(bytes), encoding ?? Encoding.UTF8, true))
            {
                var str = reader.ReadToEnd();
                return RegularExpression.ReplaceNewline.Replace(str, newline ?? Environment.NewLine);
            }
        }

        public static string GetSizeString(long size)
        {
            if (size < 0)
                return "unknow size";

            double r = size;
            foreach (var unit in SizeUnits)
            {
                if (r < 1000)
                    return r.ToString("f2") + " " + unit;
                r /= 1024;
            }

            return "largest size";
        }

        public static byte[] ReplaceNewline(byte[] bytes, Encoding encoding = null, string newline = null)
        {
            if (newline == null)
                return bytes;
            if (encoding == null)
                encoding = Encoding.UTF8;

            var bomIndex = BomIndex(bytes);
            var pure = encoding.GetBytes(ReadToEnd(bytes, encoding, newline));
            if (bomIndex != -1)
            {
                var bom = Boms[bomIndex];
                var buffer = new byte[bom.Length + pure.Length];
                Array.Copy(bom, buffer, bom.Length);
                Array.Copy(pure, 0, buffer, bom.Length, pure.Length);
                return buffer;
            }

            return pure;
        }

        private static int BomIndex(byte[] bytes)
        {
            for (var i = 0; i < Boms.Length; i++)
            {
                if (bytes.Length < Boms[i].Length)
                    continue;
                var flag = true;
                for (var j = 0; flag && j < Boms[i].Length; j++)
                    flag = bytes[j] == Boms[i][j];
                if (flag)
                    return i;
            }
            return -1;
        }

        private static bool IsMatchEncoding(Encoding encoding, byte[] bytes)
        {
            try
            {
                var s = encoding.GetString(bytes); // ignore unknow BOM, supposing there is no BOM
                var r = encoding.GetBytes(s);

                var match = 0.0;
                for (var i = 0; i < r.Length; i++)
                    if (bytes[i] == r[i])
                        match++;
                if (match >= r.Length * 0.9)
                    return true;
            }
            catch { }
            return false;
        }
    }
}