using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;

namespace SensorTester
{
    class Program
    {
        static void Main(string[] args)
        {
            var collection = new SensorCollectionPico("http://localhost:8080/sky/event/", "http://localhost:8080/sky/cloud/", "RuNi1qt8oUim6mApqxbgx4");
            var picos = collection.GetPicos();
            Console.WriteLine("There are currently " + picos.Keys.Count() + " picos");

            Console.Write("Adding three picos ");
            for (int x = 0; x < 3; x++)
            {
                collection.CreatePico("Sensor " + x);
                Console.Write(".");
            }

            Console.ReadLine();

            picos = collection.GetPicos();
            Console.WriteLine("There are currently " + picos.Keys.Count() + " picos");

            Console.WriteLine("\nSending random temps");

            foreach (var pico in picos)
            {
               pico.Value.SendTemp((new Random()).Next(66, 99));
            }


            Console.ReadLine();

            Console.WriteLine("Removing three picos ...");
            for (int x = 0; x < 3; x++)
            {
                collection.RemovePico("Sensor " + x);
                Console.Write(".");
            }

            picos = collection.GetPicos();
            Console.WriteLine("There are currently " + picos.Keys.Count() + " picos");

            Console.ReadLine();
        }
    }

    class SensorCollectionPico
    {
        public string EId { get; private set; }
        public string EventAddress { get; private set; }
        public string QueryAddress { get; private set; }

        public SensorCollectionPico(string eventAddress, string queryAddress, string eid)
        {
            this.EId = eid;
            this.EventAddress = eventAddress;
            this.QueryAddress = queryAddress;
        }

        public void CreatePico(string name)
        {
            var client = new RestClient(EventAddress + EId);
            var request = new RestRequest("25/sensor/new_sensor", Method.POST)
                .AddHeader("Content-type", "application/json")
                .AddJsonBody(new { sensor_name = name });
            var response = client.Execute(request);
        }

        public void RemovePico(string name)
        {
            var client = new RestClient(EventAddress + EId);
            var request = new RestRequest("25/sensor/unneeded_sensor", Method.POST)
                .AddHeader("Content-type", "application/json")
                .AddJsonBody(new { sensor_name = name });
            var response = client.Execute(request);
        }

        public Dictionary<string, SensorPico> GetPicos()
        {
            var client = new RestClient(QueryAddress + EId);
            var request = new RestRequest("manage_sensors/sensors", Method.GET);
            var response =  client.Execute<Dictionary<string, SensorPico>>(request);
            return response.Data;
        }
    }

    class SensorPico
    {
        public string id { get; set; }
        public string eci { get; set; }

        public void SendTemp(int temperature)
        {
            var client = new RestClient("http://localhost:8080/sky/event/" + eci);
            var request = new RestRequest("107/wovyn/new_temperature_reading", Method.POST)
                .AddHeader("Content-type", "application/json")
                .AddJsonBody(new { timestamp = DateTime.Now.ToString(), temp = temperature});
            client.Execute(request);
        }

    }
}
