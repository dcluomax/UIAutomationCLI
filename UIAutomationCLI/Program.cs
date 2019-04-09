using System;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Runtime.InteropServices;
using OpenQA.Selenium.Support.UI;

namespace UIAutomationCLI
{
    class Program
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);
        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;
        static IWebDriver Driver;

        enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType sig)
        {
            switch (sig)
            {
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                default:
                    if (Driver != null) { Driver.Quit(); }
                    return false;
            }
        }

        private static void WaitUntilFinish(IWebDriver Driver, int seconds)
        {
            new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds)).Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
        }
        private static IWebElement WaitFindElement(WebDriverWait wait, By by)
        {
            return wait.Until<IWebElement>((d) => { return d.FindElement(by); });
        }
        static void Main(string[] args)
        {
            int SecondsToWait = 10;
            Console.ForegroundColor = ConsoleColor.Green;
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            Dictionary<string, string> localValues = new Dictionary<string, string>();
            var options = new ChromeOptions();
            options.AddArguments("--incognito");
            var service = ChromeDriverService.CreateDefaultService(AppDomain.CurrentDomain.BaseDirectory);
            service.HideCommandPromptWindow = true;
            Driver = new ChromeDriver(service, options);
            IWebElement element = null;
            By ByPath = null;
            WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(SecondsToWait));
            while (true)
            {
                try
                {
                    string cmd = Console.ReadLine().Trim();
                    if (cmd.ToLower() == "end")
                    {
                        if (Driver != null) { Driver.Quit(); }
                        return;
                    } else if (cmd.StartsWith("#")) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine(cmd);
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (cmd.StartsWith("goto", true, null))
                    {

                        string url = cmd.Split(new Char[] { ' ' }, 2)[1];
                        if (url.ToLower().StartsWith("//"))
                        {
                            url = "http:" + url;
                        }
                        else if (!url.ToLower().StartsWith("http"))
                        {
                            url = "http://" + url;
                        }

                        Driver.Url = url;
                        Driver.Navigate();
                    }
                    else if (cmd.StartsWith("find", true, null))
                    {
                        string[] localArgs = cmd.Split(new Char[] { ' ' }, 3);
                        string type = localArgs[1].ToLower();
                        switch (type)
                        {
                            case "xpath":
                                ByPath = By.XPath(localArgs[2]);
                                break;
                            case "id":
                                ByPath = By.Id(localArgs[2]);
                                break;
                            case "class":
                                ByPath = By.ClassName(localArgs[2]);
                                break;
                            default:
                                ByPath = By.XPath("//*[@*[contains(., '" + localArgs[2] + "')]]/"+ type);
                                break;
                        }

                        string xpath = localArgs[2];
                        element = WaitFindElement(wait, ByPath);
                    }
                    // click
                    else if (cmd.StartsWith("click", true, null))
                    {
                        if (ByPath == null) { throw new Exception("no element selected"); }
                        wait.Until((d) => { element = WaitFindElement(wait, ByPath); return element.Enabled; });
                        element.Click();
                    }
                    // sleep 10
                    else if (cmd.StartsWith("sleep", true, null))
                    {
                        System.Threading.Thread.Sleep(SecondsToWait * 1000);
                    }
                    // waituntilenabled
                    else if (cmd.StartsWith("waituntilenabled", true, null))
                    {
                        WaitUntilFinish(Driver, SecondsToWait);
                        new WebDriverWait(Driver, TimeSpan.FromSeconds(SecondsToWait)).Until(d => element.Enabled);
                    }
                    // type [value]
                    else if (cmd.StartsWith("type", true, null))
                    {
                        element = WaitFindElement(wait, ByPath);
                        if (element == null) { throw new Exception("no element selected"); }
                        string[] localArgs = cmd.Split(new Char[] { ' ' }, 2);
                        string content = localArgs[1];
                        element.SendKeys(content);
                    }
                    // test [enabled|texteq|textcontains] [compare]
                    else if (cmd.StartsWith("test", true, null))
                    {
                        string[] localArgs = cmd.Split(new Char[] { ' ' }, 3);
                        switch (localArgs[1].ToLower())
                        {
                            case "enabled":
                                element = WaitFindElement(wait, ByPath);
                                if (!element.Enabled) { throw new Exception("Test failed."); }
                                continue;
                            case "texteq":
                                element = WaitFindElement(wait, ByPath);
                                if (!element.Text.Equals(localArgs[2])) { throw new Exception("Test failed."); }
                                continue;
                            case "textcontains":
                                element = WaitFindElement(wait, ByPath);
                                if (!element.Text.Contains(localArgs[2])) { throw new Exception("Test failed."); }
                                continue;
                            case "textnotcontains":
                                element = WaitFindElement(wait, ByPath);
                                if (element.Text.Contains(localArgs[2])) { throw new Exception("Test failed."); }
                                continue;
                            default:
                                continue;
                        }
                    }

                    WaitUntilFinish(Driver, SecondsToWait);
                }
                catch (Exception e) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Error: " + e.Message);
                    Console.ForegroundColor = ConsoleColor.Green;
                }
            }
        }

    }
}
