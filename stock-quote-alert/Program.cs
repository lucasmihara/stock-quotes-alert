using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using stock_quote_alert.classes;

namespace stock_quote_alert
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if(args.Length == 1)
            {
                if(args[0] == "config")
                {
                    SetConfig();
                    return;
                }
                else if(args[0] == "email")
                {
                    SetEmail();
                    return;
                }
            }
            if(args.Length != 3)
            {
                Console.WriteLine("Invalid command: please use the right syntax: 'stock-quote-alert [symbol] [sell price] [buy price]'. Press enter to continue.");
                Console.ReadKey();
                return;
            }

            string symbol;
            double sellPrice;
            double buyPrice;

            try
            {
                symbol = args[0].ToUpper();
                sellPrice = Double.Parse(args[1].Replace('.', ','));
                buyPrice = Double.Parse(args[2].Replace('.', ','));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error at getting prices.");
                return;
            }

            JObject config;
            List<string> emails;
            try
            {
                config = ReadConfig();
                emails = ReadEmails();
            }
            catch(Exception ex)
            {
                return;
            }
            string key;
            string host;
            int port;
            bool enableSsl;
            string from;
            string password;
            string name;

            try
            {
                key = config["key"].ToString();
                host = config["host"].ToString();
                port = Int32.Parse(config["port"].ToString());
                if(port < 1)
                {
                    throw new Exception();
                }
                enableSsl = bool.Parse(config["enableSsl"].ToString());
                from = config["from"].ToString();
                password = config["password"].ToString();
                name = config["name"].ToString();
            }
            catch(Exception ex)
            {
                Console.WriteLine("JSON config file is bad formatted. Run 'stock-quote-alert config' to access the configurations.");
                return;
            }

            StockObserver stockObserver = new StockObserver(key);
            MailSender mailSender = new MailSender(stockObserver, host, port, enableSsl, from, password, name);

            stockObserver.Add(mailSender);

            foreach (string email in emails)
            {
                mailSender.AddTo(email);
            }

            Stock stock = new Stock(symbol, buyPrice, sellPrice);

            await stockObserver.CheckQuotes(stock);

            return;
        }

        private static void SetEmail()
        {
            string filename = "emails.txt";
            if (!File.Exists(filename))
            {
                File.Create(filename).Dispose();
            }
            System.Diagnostics.Process.Start(filename);
        }

        private static void SetConfig()
        {
            string filename = "config.json";
            if (!File.Exists(filename))
            {
                CreateConfig();
            }
            System.Diagnostics.Process.Start(filename);
        }

        private static void CreateConfig()
        {
            string filename = "config.json";

            JObject json = new JObject();
            json.Add("key", "");
            json.Add("host", "");
            json.Add("port", "0");
            json.Add("enableSsl", "true");
            json.Add("from", "");
            json.Add("password", "");
            json.Add("name", "");

            File.WriteAllText(filename, json.ToString());
        }

        private static JObject ReadConfig()
        {
            string filename = "config.json";

            try
            {
                StreamReader r = new StreamReader(filename);
                
                string jsonString = r.ReadToEnd();

                r.Close();
                JObject jsonConfig = JObject.Parse(jsonString);

                List<string> jsonKeys = jsonConfig.Properties().Select(p => p.Name).ToList();
                List<string> keys = new List<string>() { "key", "host", "port", "enableSsl", "from", "password", "name" };
                foreach (string key in keys)
                {
                    if(!jsonKeys.Contains(key))
                    {
                        File.Delete("config.json");
                        throw new Exception();
                    }
                }

                if(jsonConfig["key"].ToString() == "" || jsonConfig["host"].ToString() == "" || jsonConfig["port"].ToString() == "" || jsonConfig["from"].ToString() == "" || jsonConfig["password"].ToString() == "")
                {
                    throw new Exception();
                }

                return jsonConfig;
                
            }
            catch(Exception ex)
            {
                Console.WriteLine("JSON config file not find or it is at bad formmating. Run 'stock-quote-alert config' to access the configurations.");
                SetConfig();
                throw ex;
            }
        }

        private static List<string> ReadEmails()
        {
            string filename = "emails.txt";
            List<string> emails;

            try
            {
                StreamReader r = new StreamReader(filename);
                emails = r.ReadToEnd().Replace("\r", "").Split('\n').Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                foreach (string email in emails)
                {
                    if(email.Length == 0)
                    {
                        emails.Remove(email);
                    }
                }
                if (emails.Count == 0)
                {
                    Console.WriteLine("emails.txt is empty. Run 'stock-quote-alert email' to access the emails.");
                    throw new Exception();
                }
                return emails;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error at getting emails. Run 'stock-quote-alert email' to access the emails.");
                SetEmail();
                throw ex;
            }
        }
    }
}
