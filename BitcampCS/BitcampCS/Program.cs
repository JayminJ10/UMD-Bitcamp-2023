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
using Google.Apis.Bigquery.v2.Data;
using Google.Api.Gax;

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
        EnvLoad.loadEnv();

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

    public static void ListTables(
        string projectId = "",
        string datasetId = "testID"
    )
    {
        projectId = Environment.GetEnvironmentVariable("GOOGLE_PROJECT_ID");

        BigQueryClient client = BigQueryClient.Create(projectId);
        TableReference tableReference = new TableReference()
        {
            TableId = "employee",
            DatasetId = "testID",
            ProjectId = projectId
        };
        // Load all rows from a table
        PagedEnumerable<TableDataList, BigQueryRow> result = client.ListRows(
            tableReference: tableReference,
            schema: null
        );
        // Print the first 10 rows
        Console.WriteLine("Name  |  Risk");
        Console.WriteLine("_____________");
        foreach (BigQueryRow row in result.Take(10))
        {
            Console.WriteLine($"{row["first_name"]}: {row["risk"]}");
        }
        Console.WriteLine("\n\n");
        // Retrieve list of tables in the dataset
        List<BigQueryTable> tables = client.ListTables(datasetId).ToList();
        // Display the results
        if (tables.Count > 0)
        {
            Console.WriteLine($"Tables in dataset {datasetId}:");
            foreach (var table in tables)
            {
                Console.WriteLine($"\t{table.Reference.TableId}");
            }
        }
        else
        {
            Console.WriteLine($"{datasetId} does not contain any tables.");
        }
    }

    public static void sendMsg()
    {
        
        string accountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        string authToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

        TwilioClient.Init(accountSid, authToken);
        
        var message = MessageResource.Create(
            body: "This is Twilio. You are being notified as an emergency contact for At The Wheel user #1. Please provide them with assistance or contact 911.",
            from: new Twilio.Types.PhoneNumber("+18337530691"),
            to: new Twilio.Types.PhoneNumber("+12027386228")
        );
        
        Console.WriteLine();
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
                    Console.WriteLine("3rd time doze-off detected. Alerting emergency contact.");
                    sendMsg();
                    Console.WriteLine("Google Cloud BigQuery Table Data:");
                    ListTables();

                    Environment.Exit(1);
                }
            }
            catch (TimeoutException) { }
        }
    }

}