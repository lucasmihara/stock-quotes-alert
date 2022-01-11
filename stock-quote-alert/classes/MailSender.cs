using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using FluentEmail.Core;
using FluentEmail.Smtp;
using stock_quote_alert.intefaces;

namespace stock_quote_alert.classes
{
    /// <summary>
    /// Allows to send email by using SMTP observing an instance of StockObserver. Implements the IObserver interface for the observer pattern.
    /// </summary>
    internal class MailSender : IObserver
    {
        private List<string> to = new List<string>();
        private SmtpClient smtp;
        private StockObservable stockObserver;

        public string host { get; set; }
        public int port { get; set; }
        public bool enableSsl { get; set; }
        public string from { get; set; }
        public string password { get; set; }
        public string name { get; set; }

        /// <summary>
        /// Initializes a new instance of MailSender, takes all the configuration of the STMP server and an instance of StockObsever
        /// </summary>
        /// <param name="stockObserver">aa</param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="enableSsl"></param>
        /// <param name="from"></param>
        /// <param name="password"></param>
        /// <param name="name"></param>
        public MailSender(StockObservable stockObserver, string host, int port, bool enableSsl, string from, string password, string name = "")
        {
            this.stockObserver = stockObserver;
            this.host = host;
            this.port = port;
            this.enableSsl = enableSsl;
            this.from = from;
            this.password = password;
            this.name = name;

            // Setting the SMTP client up
            this.smtp = new SmtpClient
            {
                Host = host,
                EnableSsl = enableSsl,
                Port = port,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from, password)
            };


            Email.DefaultSender = new SmtpSender(smtp);
            Console.WriteLine("Email will try to be sent as {0}", this.from);
        }

        /// <summary>
        /// Send an email about the stock to every recipient in MailSender.to parameter 
        /// </summary>
        /// <param name="stock"></param>
        void SendEmail(Stock stock)
        {
            string condition;
            double referencePrice;

            // Customizing the message
            if(stock.currPrice < stock.buyPrice)
            {
                condition = "under";
                referencePrice = stock.buyPrice;
            }
            else
            {
                condition = "over";
                referencePrice = stock.sellPrice;
            }

            try
            {
                foreach (string s in to)
                {
                    // Send the email
                    var email = Email
                        .From(this.from, this.name)
                        .To(s)
                        .Subject(String.Format("{0} Stock Price Alert", stock.symbol))
                        .Body(String.Format(@"The {0} Stock is at the price of {1}, {2} the reference price of {3}", stock.symbol, stock.currPrice, condition, referencePrice))
                        .HighPriority()
                        .Send();
                    if (email.Successful)
                    {
                        Console.WriteLine("Email sent to {0}", s);
                    }
                    else
                    {
                        throw new Exception();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while sending the email. Check the email credentials and configurations");
                Console.WriteLine(ex.ToString());
            }


        }
        /// <summary>
        /// Add a new email to MailSender.to parameter which will be recipients to SendEmail() method
        /// </summary>
        /// <param name="email"></param>
        public void AddTo(string email)
        {
            this.to.Add(email);
        }

        /// <summary>
        /// Send an email about the last stock added in the stockObserver.stocksAlert list. This method must be called everytime StockOberver detects a stock price under or over the referece prices. Part of Observable pattern.
        /// </summary>
        public void Update()
        {
            this.SendEmail(this.stockObserver.GetLastStock());
        }
    }
}
