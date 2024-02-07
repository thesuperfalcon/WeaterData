using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeaterData
{
    public class WeatherRecord
    {
        public DateTime Date { get; set; }
        public string Location { get; set; }
        public double Temperature { get; set; }
        public double Humidity { get; set; }

        public WeatherRecord(DateTime date, string location, double temperature, double humidity)
        {
            Date = date;
            Location = location;
            Temperature = temperature;
            Humidity = humidity;
        }

        public static void WeatherMenu(List<WeatherRecord> allRecords, string path)
        {
            string toggleState = "Inne";

            List<WeatherRecord> records = GetAverageValues(allRecords);
            Dictionary<WeatherRecord, double> moldIndexValues = CalculateMoldIndexValues(records, toggleState);
            SaveMoldIndexValuesToFile(moldIndexValues, path);
            FileHandler.FileCreate(records, path);
            DateTime startDate = new DateTime(2016 - 08 - 01);
            DateTime endDate = new DateTime(2017 - 02 - 14);
            DateTime? autuumDate = CalculateSeason(records, 10, startDate, endDate);
            DateTime? winterDate = CalculateSeason(records, 0, null, null);


            List<string> list = new List<string>();
            list.Add(SaveAutuumWinterDate(autuumDate, "Autuum", records));
            list.Add(SaveAutuumWinterDate(winterDate, "Winter", records));


            FileHandler.SaveToFile("season_dates.txt", "../../../", list);
            while (true)
            {
                Console.WriteLine("Toggle State: " + toggleState);
                Console.WriteLine("1. Toggle");
                Console.WriteLine("2. Average Temperatures");
                Console.WriteLine("3. Average Humidities");
                Console.WriteLine("4. Mold Index");
                Console.WriteLine("5. Autumm");
                Console.WriteLine("6. Exit");
                Console.Write("Input: ");
                int choice;
                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Invalid input. Please enter a number.");
                }

                switch (choice)
                {
                    case 1:
                        toggleState = toggleState == "Inne" ? "Ute" : "Inne";
                        Console.WriteLine("Toggle State: " + toggleState);
                        break;
                    case 2:
                        PrintAverageTemperatures(records, toggleState);
                        break;
                    case 3:
                        PrintAverageHumidity(records, toggleState);
                        break;
                    case 4:
                        PrintMoldIndexValues(moldIndexValues, toggleState);
                        break;
                    case 5:
                        Console.WriteLine(autuumDate.ToString());
                        Console.WriteLine(winterDate.ToString());
                        break;
                    case 6:
                        Console.WriteLine("Exiting Weather Menu...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        break;
                }
                Console.ReadLine();
                Console.Clear();
            }
        }

        private static void PrintAverageTemperatures(List<WeatherRecord> averageValues, string location)
        {
            var locationAverageValues = averageValues
                .Where(avg => avg.Location == location)
                .OrderByDescending(temp => temp.Temperature)
                .ToList();

            Console.WriteLine($"Location: {location}");

            foreach (var avg in locationAverageValues)
            {
                Console.WriteLine($"Date: {avg.Date.ToShortDateString()} Temperature: {avg.Temperature:F1}");
            }
        }

        private static void PrintAverageHumidity(List<WeatherRecord> averageValues, string location)
        {
            var locationAverageValues = averageValues
                .Where(avg => avg.Location == location)
                .OrderByDescending(humid => humid.Humidity)
                .ToList();

            Console.WriteLine($"Location: {location}");

            foreach (var avg in locationAverageValues)
            {
                Console.WriteLine($"Date: {avg.Date.ToShortDateString()} Humidity: {avg.Humidity:F1}%");
            }
        }
        private static List<WeatherRecord> GetAverageValues(List<WeatherRecord> weatherRecords)
        {
            List<WeatherRecord> averageValues = new List<WeatherRecord>();

            var groupedData = weatherRecords.GroupBy(d => new { Date = d.Date.Date, Location = d.Location });

            foreach (var group in groupedData)
            {
                double averageTemperature = group.Average(d => d.Temperature);
                double averageHumidity = group.Average(d => d.Humidity);

                WeatherRecord averageRecord = new WeatherRecord(group.Key.Date, group.Key.Location, averageTemperature, averageHumidity);

                averageValues.Add(averageRecord);
            }
            return averageValues;
        }

        private static Dictionary<WeatherRecord, double> CalculateMoldIndexValues(List<WeatherRecord> averageValues, string location)
        {
            Dictionary<WeatherRecord, double> values = new Dictionary<WeatherRecord, double>();

            foreach (var avg in averageValues)
            {
                if (avg.Temperature < 0 || avg.Temperature > 50 || avg.Humidity < 78 || avg.Humidity > 100)
                {
                    values.Add(avg, 0);
                    continue;
                }

                double avgValue = ((avg.Humidity - 78) * (avg.Temperature / 15)) / 0.22;
                values.Add(avg, avgValue);
            }

            return values.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        private static void PrintMoldIndexValues(Dictionary<WeatherRecord, double> moldIndexValues, string path)
        {
            foreach (var value in moldIndexValues)
            {
                if (value.Key.Location.Equals(path))
                {
                    Console.WriteLine($"Date: {value.Key.Date.ToShortDateString()} {value.Key.Location} MoldIndex: {value.Value:F1}");
                }
                else
                {
                    continue;
                }
            }
        }

        private static void SaveMoldIndexValuesToFile(Dictionary<WeatherRecord, double> moldIndexValues, string path)
        {
            List<string> moldIndex = GetStringList(moldIndexValues);

            Action<string, string, IEnumerable<string>> saveToFileAction = (fileName, savePath, data) =>
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(savePath + fileName))
                    {
                        foreach (var line in data)
                        {
                            writer.WriteLine(line);
                        }
                    }
                    Console.WriteLine($"Data saved to {fileName} successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while saving data to {fileName}: {ex.Message}");
                }
            };

            saveToFileAction("mold_index.txt", path, moldIndex);
        }


        private static List<string> GetStringList(Dictionary<WeatherRecord, double> moldIndexValues)
        {
            var list = new List<string>();
            string text = string.Empty;
            foreach (var value in moldIndexValues)
            {
                string location = value.Key.Location == "Inne" ? "Inne" : "Ute";
                text = $"Date: {value.Key.Date.ToShortDateString()} {location} MoldIndex: {value.Value:F1}";
                list.Add(text);
            }
            string moldAlgorith = @"
Algoritm för Mögelindexberäkning:

1. Kontrollera om temperaturen är mellan 0 och 50 grader Celsius och luftfuktigheten är mellan 78% och 100%.
2. Om villkoren uppfylls:
- Beräkna Mögelindex med formeln: ((Luftfuktighet - 78) * (Temperatur / 15)) / 0.22
- Högre luftfuktighet och temperaturvärden bidrar till ett högre Mögelindex.
3. Om villkoren inte är uppfyllda betraktas Mögelindex som 0.

Obs: Mögelindexvärden indikerar risken för mögeltillväxt. Högre värden indikerar högre risk.";
            list.Add(moldAlgorith);
            return list;
        }
        private static List<WeatherRecord> CalculateValues(List<WeatherRecord> allValues, DateTime? startDate, DateTime? endDate)
        {
            var values = allValues.Where(x => x.Location == "Ute");

            if (startDate != null && endDate != null)
            {
                values = values.Where(x => x.Date >= startDate && x.Date <= endDate);
            }

            List<WeatherRecord> filteredValues = values.ToList();

            return filteredValues;

        }
        private static DateTime? CalculateSeason(List<WeatherRecord> allValues, double temperature, DateTime? startDate, DateTime? endDate)
        {
            int count = 0;
            DateTime? autumnDate = null;

            var values = allValues.Where(x => x.Location == "Ute").ToList();

            while (true)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    if (values[i].Temperature < temperature)
                    {
                        count++;
                        if (count == 5)
                        {
                            int startIndex = i - 4;
                            autumnDate = values[startIndex].Date;
                            break;
                        }
                    }
                    else
                    {
                        count = 0;
                    }
                }
                if(autumnDate != null)
                {
                    return autumnDate;
                }
                else
                {
                    temperature += .2;
                }
            }
        }
        private static string SaveAutuumWinterDate(DateTime? date, string season, List<WeatherRecord> weatherRecords)
        {
            if (date.HasValue)
            {
                var specificDate = weatherRecords.FirstOrDefault(x => x.Date.ToShortDateString() == date.Value.ToShortDateString());
                if (specificDate != null)
                {
                    return $"Date: {specificDate.Date.ToShortDateString()} Season: {season} Temperature: {specificDate.Temperature:F1}";
                }
            }
            return string.Empty;
        }
    }
}
