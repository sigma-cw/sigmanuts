using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using CefSharp;
using CefSharp.WinForms;
using CefSharp.Example;

namespace sigmanuts
{
    public partial class Form1 : Form
    {
        public ChromiumWebBrowser browser;

        public Form1()
        {
            InitializeComponent();

            // Start chromium
            var chatUrl = "https://www.youtube.com/live_chat?is_popout=1&v=S9bpo-iTbys";
            InitChromium(chatUrl);

            var pathToScript = Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\js\\script.js");
            string contents = File.ReadAllText(pathToScript);

            browser.LoadingStateChanged += (sender, args) =>
            {
                //Wait for the Page to finish loading
                if (args.IsLoading == false)
                {
                    browser.ShowDevTools();
                    browser.ExecuteScriptAsync(contents);
                }
            };

            var eventObject = new ScriptedMethodsBoundObject();
            eventObject.EventArrived += OnJavascriptEventArrived;

            CefSharpSettings.WcfEnabled = true;
            browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
            browser.JavascriptObjectRepository.Register("boundEvent", eventObject, isAsync: false, options: BindingOptions.DefaultBinder);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void InitChromium(string chatUrl)
        {
            CefSettings settings = new CefSettings();

            // Initialize Cef with the provided settings
            Cef.Initialize(settings);

            // Create a browser component
            // and provide the URL
            browser = new ChromiumWebBrowser(chatUrl);

            // Add browser to the form
            this.Controls.Add(browser);

            browser.Name = "sigmanuts";
            browser.Dock = DockStyle.Fill;
        }

        public static void OnJavascriptEventArrived(string eventName, object eventData)
        {

            var jsonString = eventData.ToString();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            var dataDict = serializer.Deserialize<Dictionary<string, object>>(jsonString);

            //Console.WriteLine("Event arrived: {0}", eventName); // output 'click'

            switch (eventName)
            {
                case "click":
                    {
                        Console.WriteLine("Click");
                        break;
                    }
                default:
                    {
                        Console.WriteLine(jsonString);
                        Server.SendMessageToClient(jsonString);
                        break;
                    }
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
        }
    }
}
