﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Remote;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MultiThreadExample
{
    public class ThreadAndDriver
    {
        public ChromeDriver Driver { get; set; }
        public Thread Thread { get; set; }

        public string Proxy { get; set; }

        public string Email { get; set; } = null;
    }
}
