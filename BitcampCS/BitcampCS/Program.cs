// Use this code inside a project created with the Visual C# > Windows Desktop > Console Application template.
// Replace the code in Program.cs with this code.

using BitcampCS;
using System;
using System.IO.Ports;
using System.Numerics;
using System.Text.Json.Serialization;
using System.Threading;
using System.Linq;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Google.Cloud.BigQuery.V2;

public class AtTheWheel
{
    static bool _continue;
    static SerialPort _sp;

    static BigInteger risk = 0;

    public static void Main()
    {
        string name;
        string message;
        StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
        Thread readThread = new Thread(Read);

        // Create a new SerialPort object with default settings.
        _sp = new SerialPort();

        // Allow the user to set the appropriate properties.
        _sp.PortName = "COM3";
        _sp.BaudRate = 9600;
        _sp.Parity = System.IO.Ports.Parity.None;
        _sp.DataBits = 8;
        _sp.StopBits = System.IO.Ports.StopBits.One;
        _sp.Handshake = System.IO.Ports.Handshake.None;

        // Set the read/write timeouts
        _sp.ReadTimeout = 500;
        _sp.WriteTimeout = 500;

        _sp.Open();
        _continue = true;
        readThread.Start();

        name = "Bitcamp";

        Console.WriteLine("Type QUIT to exit");

        while (_continue)
        {
            message = Console.ReadLine();

            if (stringComparer.Equals("quit", message))
            {
                _continue = false;
            }
            else
            {
                _sp.WriteLine(
                    String.Format("<{0}>: {1}", name, message));
            }
        }

        readThread.Join();
        _sp.Close();
    }

    public static void sendMsg()
    {
        EnvLoad.loadEnv();
        string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        TwilioClient.Init(accountSid, authToken);
        
        var message = MessageResource.Create(
            body: "This is Twilio. You are being notified as an emergency contact for At The Wheel user #1. Please provide them with assistance or contact 911.",
            from: new Twilio.Types.PhoneNumber("+18337530691"),
            to: new Twilio.Types.PhoneNumber("+12027386228")
        );
        
        Console.WriteLine(message.Sid);
    }

    public static void Read()
    {
        while (_continue)
        {
            try
            {
                string message = _sp.ReadLine();
                if (message.Contains("RISK: "))
                {
                    string[] riskArg = message.Split(" ");
                    risk = BigInteger.Parse(riskArg[1]);
                    Console.WriteLine(risk);
                }
                else
                {
                    Console.WriteLine(message);
                }
                if (risk == 3)
                {
                    sendMsg();
                    Environment.Exit(1);
                }
            }
            catch (TimeoutException) { }
        }
    }

}