using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.PhantomJS;

namespace MultiThreadExample
{
    public partial class Form1 : Form
    {
        private const string RESETLINK = "https://www.pinterest.com/password/reset/";
        static int number = 0;

        private int _threadCount = 15;

        List<string> _proxy;
        List<string> _email;
        List<ThreadAndDriver> _threadAndDriver;


        string _proxyFileName = "proxy.txt";
        string _emailFileName = @"C:\my_work_files\pinterest\source_all_account_for_blaster.txt";


        private int waitTask = 30000;

        public Form1()
        {
            InitializeComponent();
            _LoadSettings();


        }

        private void _LoadSettings()
        {
            _email = new List<string>();
            this._ClearAll();
            try
            {
                _proxy = File.ReadAllLines(_proxyFileName).ToList();
                var temp = File.ReadAllLines(_emailFileName).ToList();


                foreach (var oneRow in temp)
                {
                    _email.Add(oneRow.Split(':')[0]);
                }

                con.Text += "All done" + Environment.NewLine; ;

            }
            catch
            {
                con.Text += $"not loaded email && proxy " + Environment.NewLine; ;
            }
        }

        private void makeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _ClearFixed();

            while (true)
            {
                if (number > _email.Count())
                {
                    break;
                }
                _threadAndDriver = new List<ThreadAndDriver>();
                for (int i = 0; i < this._threadCount; i++)
                {
                    string proxy = this._ProxyGet();
                    var driver = OpenAndReturnDriver(proxy);

                    Thread th = new Thread(Make);
                    th.Name = "QQQ";

                    ThreadAndDriver threadAndDriver = new ThreadAndDriver() { Driver = driver, Proxy = proxy, Thread = th };

                    th.Start(threadAndDriver);
                    _threadAndDriver.Add(threadAndDriver);

                }
                Thread.Sleep(this.waitTask);
                this._CloseAll();

            }


        }

        private void _ClearFixed()
        {
            try
            {
                List<string> fix = File.ReadAllLines("fixed.txt").ToList();
                con.Text = $"{fix.Count} email already done";

                foreach (var line in fix)
                    _email.Remove(line);

                con.Text = $"{_email.Count} after clreaing";
            }
            catch
            {

            }
        }

        private void _CloseAll()
        {


            foreach (var oneRow in _threadAndDriver)
            {
                try
                {
                    oneRow.Driver.Quit();
                    oneRow.Thread.Abort();
                }
                catch { }

            }


        }
        private string _ProxyGet()
        {

            return _proxy.Randomize().FirstOrDefault();
        }

        private void Make(object state)
        {
            ThreadAndDriver current = (ThreadAndDriver)state;
            current.Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0, 20);
            try
            {

                
                for (int i = 0; i < 15; i++)
                {
                    current.Driver.Url = RESETLINK;
                    current.Driver.FindElementById("userQuery").SendKeys(_email[number]);
                    current.Driver.FindElementByCssSelector(".sendBar button").Click();
                    UpdateList(_email[number]);
                   
                    number++;
                }
            }
            catch (Exception ex)
            {
                _TryCloseThread(current);
            }

            finally
            {
                _TryCloseThread(current);
            }




        }

        private void _ClearAll()
        {

            var proccess = Process.GetProcesses();
            foreach (Process pr in proccess)
            {

                var x = pr.ProcessName;
                if (pr.ProcessName.ToLower().Contains("phantom"))
                    pr.Kill();

            }

        }

        private void _TryCloseThread(ThreadAndDriver current)
        {
            try
            {

                current.Driver.Quit();
                current.Thread.Abort();

            }
            catch
            {

            }
        }

        private PhantomJSDriver OpenAndReturnDriver(string proxy)
        {
            //ChromeOptions option = new ChromeOptions();
            //option.AddArgument($"--proxy-server={proxy}");
            // option.AddArgument("--no-startup-window");
            var driver = new PhantomJSDriver(_GetJsSettings(proxy));
            return driver; 
        }

        private static PhantomJSDriverService _GetJsSettings(string proxy)
        {
            var serviceJs = PhantomJSDriverService.CreateDefaultService();
            serviceJs.HideCommandPromptWindow = true;
            serviceJs.Proxy = proxy;
            return serviceJs;
        }

        private void UpdateList(string email)
        {
            _email.Remove(email);
            File.AppendAllText("fixed.txt", email + Environment.NewLine);

        }



        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            this._CloseAll();

        }


    }
    public static class IEnumerableExtensions
    {

        public static IEnumerable<t> Randomize<t>(this IEnumerable<t> target)
        {
            Random r = new Random();

            return target.OrderBy(x => (r.Next()));
        }
    }
}
