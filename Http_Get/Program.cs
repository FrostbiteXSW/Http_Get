using System;
using System.IO;
using System.Net;
using System.Threading;

namespace Http_Get {
    internal static class Program {
        private static void Main() {
            Directory.CreateDirectory("./DataCache");
            var urls = new UrlHandler("url.txt");
            var locker = new Locker();
            var sigFlag = 0;
            const int maxThreads = 16;

            for (var i = 0; i < maxThreads; i++) {
                var thread = new Thread(program => {
                    string url;
                    var webClient = new WebClient();
                    while ((url = urls.GetNext()) != null) {
                        var fileName = url.Substring(url.LastIndexOf('/') + 1);
                        try {
                            webClient.DownloadFile(url, $"./DataCache/{fileName}");
                        }
                        catch (Exception) {
                            Console.WriteLine($"Unable to download {url}");
                        }
                    }
                    lock (locker) {
                        sigFlag++;
                    }
                });
                thread.Start();
            }

            var buf = 0;
            while (buf != maxThreads) {
                Thread.Sleep(5 * 1000);
                lock (locker) {
                    buf = sigFlag;
                }
            }
        }

        private class Locker { }

        private class UrlHandler {
            private readonly StreamReader _reader;

            public UrlHandler(string urlFile) {
                _reader = new StreamReader(new FileStream(urlFile, FileMode.Open));
            }

            public string GetNext() {
                lock (this) {
                    return !_reader.EndOfStream ? _reader.ReadLine() : null;
                }
            }
        }
    }
}