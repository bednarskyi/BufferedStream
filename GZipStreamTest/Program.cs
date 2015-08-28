using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GZipStreamTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startTime;
            double ellapsedTime;
            byte[] bytes = new byte[100000];
            int numberOfLoops = 20;
            var random = new Random();
            List<Tuple<int, long, double>> listGzipBuffer = new List<Tuple<int, long, double>>();
            List<Tuple<int, long, double>> listGzip = new List<Tuple<int, long, double>>();

            Parallel.For(0, bytes.Length, (i) => bytes[i] = (byte) random.Next());
            for(int i = 0; i < 1000000; i++)
            {
                var log = Math.Log10(i);
                var pow = Math.Pow(log, i);
                Math.IEEERemainder(log, pow);
            }
            for (int buffer = 1; buffer < Math.Pow(2, numberOfLoops); buffer *= 2)
            {
                using (MemoryStream output = new MemoryStream())
                {
                    startTime = DateTime.Now;
                    using(var compressor = new GZipStream(output, CompressionMode.Compress, true))
                    {
                        using(var buf = new BufferedStream(compressor))
                        {
                            buf.Write(bytes, 0, bytes.Length);
                            //using(BufferedStream buffer = new BufferedStream(compressor, bufferSize))
                            //{
                            //    buffer.Write(bytes, 0, bytes.Length);
                            //    buffer.Flush();
                            //}
                            compressor.Flush();
                        }

                    }
                    ellapsedTime = (DateTime.Now - startTime).TotalMilliseconds;
                    listGzipBuffer.Add(new Tuple<int, long, double>(buffer, output.Length, ellapsedTime));
                }
            }
            for (int buffer = 1; buffer < Math.Pow(2, numberOfLoops); buffer *= 2)
            {
                using (MemoryStream output = new MemoryStream())
                {
                    startTime = DateTime.Now;
                    using (var compressor = new GZipStream(output, CompressionMode.Compress, true))
                    {

                        compressor.Write(bytes, 0, bytes.Length);
                        //using(BufferedStream buffer = new BufferedStream(compressor, bufferSize))
                        //{
                        //    buffer.Write(bytes, 0, bytes.Length);
                        //    buffer.Flush();
                        //}
                       // compressor.Flush();
                    }
                    ellapsedTime = (DateTime.Now - startTime).TotalMilliseconds;
                    listGzip.Add(new Tuple<int, long, double>(buffer, output.Length, ellapsedTime));
                }
               
            }

            for (int i = 0; i < numberOfLoops; i ++)
            {
                Console.WriteLine(
                    "Buffer ({2}) + gzip stream were {0} {1} than used gzip stream alone\n",
                    listGzipBuffer[i].Item3.Equals(listGzip[i].Item3)? 0.ToString("P") : (listGzipBuffer[i].Item3 / listGzip[i].Item3).ToString("P"), 
                    listGzipBuffer[i].Item3 < listGzip[i].Item3 ? "faster" : 
                    (listGzipBuffer[i].Item3.Equals(listGzip[i].Item3) ? "equal" : "slower"), listGzipBuffer[i].Item1);

                Console.WriteLine(
                    "Buffer ({2}) + gzip stream were {0} {1} than used gzip stream alone\n",
                    listGzipBuffer[i].Item2 == listGzip[i].Item2 ?  0.ToString("P"): (listGzipBuffer[i].Item2 / listGzip[i].Item2).ToString("P"),
                    listGzipBuffer[i].Item2 < listGzip[i].Item2 ? "more powerful" : (listGzipBuffer[i].Item2 == listGzip[i].Item2 ? "equal" : "less powerful"), listGzipBuffer[i].Item1);

            }
            Console.ReadLine();
        }


        static Stream createCompressingGZipStream(Stream stream, int bufferLength, bool leaveOpen)
        {
            return new BufferedStream(
                new GZipStream(stream, CompressionMode.Compress, leaveOpen),
                bufferLength);
        }

    }
}
