using QRCoder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;

namespace xuexihelper
{
    class Program
    {
        static Regex rx = new Regex("(http|ftp|https)://([\\w_-]+(?:(?:\\.[\\w_-]+)+))([\\w.,@?^=%&:/~+#-]*[\\w@?^=%&/~+#-])?");
        static string serverid;
        static HttpClient httpClient;
        static ProcessStartInfo psi;
        static Process process;
        static Timer timer;
        static int Interval;
        static void Main(string[] args)
        {


            serverid = Environment.GetEnvironmentVariable("Serverid");
            Interval = Environment.GetEnvironmentVariable("Interval") == null ? 3600 * 24 : int.Parse(Environment.GetEnvironmentVariable("Interval"));
            if (string.IsNullOrEmpty(serverid))
            {
                Wlog("请配置环境变量serverid");
                // return;
            }
            httpClient = new HttpClient();
            Wlog("Start script");
            psi = new ProcessStartInfo();
            psi.FileName = "/app/Fuck学习强国";
            psi.Arguments = "--headless";
            //psi.FileName = "test.bat";
            psi.RedirectStandardOutput = true;
            psi.RedirectStandardError = true;
            timer = new Timer();
            timer.Interval = Interval * 1000;
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            //  timer.Start();
            Start();
            Console.ReadLine();
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Start();
        }

        static void Start()
        {
            try
            {
                if (process == null)
                {
                    process = new Process();
                    process.StartInfo = psi;

                }
                else
                {
                    process.OutputDataReceived -= Process_OutputDataReceived;
                    process.Kill();
                    process.Dispose();
                    process = new Process();
                    process.StartInfo = psi;

                }
                process.EnableRaisingEvents = true;
                process.OutputDataReceived += Process_OutputDataReceived;
                process.Start();
                process.BeginOutputReadLine();

            }
            catch (Exception ex)
            {

                Wlog(ex.ToString());
            }
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            var msg = e.Data;
            if (msg == null)
                return;
            Wlog(msg);
            var date = DateTime.Now.ToLocalTime();
            if (date.Hour > 9 && date.Hour < 22)
            {
                //早上9点到晚上10点之间不推送
                if (msg.Contains("https"))
                {
                    var m = rx.Match(msg);
                    if (m.Success)
                    {
                        SendUrl(m.Value);
                    }

                }
            }

        }
        static void Wlog(string log)
        {
            Console.WriteLine(DateTime.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss") + " " + log);
        }

        async static void SendUrl(string url)
        {
            try
            {
                var imgType = Base64QRCode.ImageType.Png;
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(url, QRCodeGenerator.ECCLevel.L);
                Base64QRCode qrCode = new Base64QRCode(qrCodeData);
                string qrCodeImageAsBase64 = qrCode.GetGraphic(5, Color.Black, Color.White, true, imgType);
                var title = "学习强国登录";
                var body = "访问网址：" + url + "\n\n![avatar][base64str]\n[base64str]:data:image/png;base64," + qrCodeImageAsBase64;
                var sendurl = $"https://sctapi.ftqq.com/{serverid}.send";
                var dc = new Dictionary<string, string>();
                dc.Add("title", title);
                dc.Add("desp", body);
                var content = new FormUrlEncodedContent(dc);
                var result = await httpClient.PostAsync(sendurl, content);
                Wlog(await result.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {

                Wlog(ex.ToString());
            }
        }
    }
}
