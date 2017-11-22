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
        List<Acc> _acc;

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
            _acc = new List<Acc>();

            this._ClearAll();
            try
            {
                _proxy = File.ReadAllLines(_proxyFileName).ToList();
                var temp = File.ReadAllLines(_emailFileName).ToList();


                foreach (var oneRow in temp)
                {
                    _email.Add(oneRow.Split(':')[0]);

                }

                this.ConText ( "All done" + Environment.NewLine) ;

            }
            catch
            {
                this.ConText($"not loaded email && proxy " + Environment.NewLine);
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
                this.ConText(  $"{fix.Count} email already done");

                foreach (var line in fix)
                    _email.Remove(line);

                this.ConText( $"{_email.Count} after clreaing");
            }
            catch
            {

            }
        }

        private void _CloseAll()
        {

            if (_threadAndDriver != null)
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

        private void MakeOne(object state)
        {
            ThreadAndDriver current = (ThreadAndDriver)state;
            current.Driver.Manage().Timeouts().ImplicitWait = new TimeSpan(0, 0, 0, 20);

            this.ConText ( current.Email);
            try
            {


                current.Driver.Url = RESETLINK;
                current.Driver.FindElementById("userQuery").SendKeys(current.Email);
                current.Driver.FindElementByCssSelector(".sendBar button").Click();
                // UpdateList(current.Email);
                this.ConText($"{current.Email} done");


            }
            catch (Exception ex)
            {
                this.ConText($"{current.Email} error");
                _TryCloseThread(current);
            }

            finally
            {
                _TryCloseThread(current);
            }




        }

        private void ConText(string text)
        {

            try
            {
                con.Text = text;
            }
            catch
            {
                MethodInvoker inv = delegate
                {
                    this.con.Text = text;
                };

                this.Invoke(inv);
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _acc = new List<Acc>();

            var temp = File.ReadAllLines(_emailFileName).ToList();
            foreach (var oneRow in temp)
            {
                _acc.Add(new Acc() { Email = oneRow.Split(':')[0], Name = oneRow.Split(':')[2] });
            }
            dataGridView.DataSource = _acc;
        }

        private void dataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            string email = dataGridView[0, e.RowIndex].Value.ToString();
            string proxy = this._ProxyGet();
            var driver = OpenAndReturnDriver(proxy);

            _threadAndDriver = new List<ThreadAndDriver>();
            Thread th = new Thread(MakeOne);
            ThreadAndDriver threadAndDriver = new ThreadAndDriver() { Driver = driver, Proxy = proxy, Thread = th, Email = email };

            th.Start(threadAndDriver);
            _threadAndDriver.Add(threadAndDriver);
            _threadAndDriver.Clear();
        }

        private void toolStripTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (_acc == null)
            {
                con.Text = "no filtered";
                return;
            }
            else if( String.IsNullOrEmpty(toolStripTextBox1.Text.Trim()) )
            {
                con.Text = "cleread";
                dataGridView.DataSource = _acc;
            }
            else
            {
                List<Acc> newData = _acc.Where((x) => x.Name.StartsWith(toolStripTextBox1.Text) | x.Name.Contains(toolStripTextBox1.Text)    ).ToList();
               
               dataGridView.DataSource = newData;
            }

           

        }

        private void clearProxyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            con.Text = $"start proxy clean {_proxy.Count()}";

            Parallel.ForEach(_proxy, CheckProxyMethod );
            con.Text = $"good proxy  {_proxy.Count()}";
            File.WriteAllLines(this._proxyFileName, _proxy);
        }

        private void CheckProxyMethod(string proxy)
        {
            PhantomJSDriver driver;
            try
            {

                driver = OpenAndReturnDriver(proxy);
                driver.Url = RESETLINK;
                driver.FindElementById("userQuery").SendKeys("hallo@ahloc.com");

                // UpdateList(current.Email);
                this.ConText($"{proxy} good");

                driver.Close();
            }
            catch (Exception ex)
            {
                this.ConText($"{proxy} bad");
                this._email.Remove(proxy);

            }
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
