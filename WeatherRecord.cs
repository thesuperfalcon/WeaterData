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
            DateTime startDate = new DateTime(2016, 08, 01);
            DateTime endDate = new DateTime(2017, 02, 14);
            double weatherTemp;
            double weatherTamp;

            WeatherRecord autuumDate = CalculateSeason(records, 10, startDate, endDate);
            WeatherRecord winterDate = CalculateSeason(records, 0, null, null);


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
                Console.WriteLine("5. Seasons");
                Console.WriteLine("6. Search specific date");
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
                        Console.WriteLine($"Autuum: {autuumDate.Date.ToShortDateString()} Temperature: {autuumDate.Temperature:F1}");
                        Console.WriteLine($"Winter: {winterDate.Date.ToShortDateString()} Temperature: {winterDate.Temperature:F1}");
                        break;
                    case 6:
                        SpecificDate(records);
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
                if (avg.Temperature < 0 || avg.Temperature > 50 || avg.Humidity < 78)
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
        private static WeatherRecord CalculateSeason(List<WeatherRecord> allValues, double temperature, DateTime? startDate, DateTime? endDate)
        {
            int count = 0;
            DateTime? seasonDate = null;

            var values = allValues.Where(x => x.Location == "Ute" &&
                                       (!startDate.HasValue || x.Date >= startDate) &&
                                       (!endDate.HasValue || x.Date <= endDate))
                          .ToList();

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
                            seasonDate = values[startIndex].Date;
                            break;
                        }
                    }
                    else
                    {
                        count = 0;
                    }
                }
                if(seasonDate != null)
                {
                    var date = allValues.Where(x => x.Date == seasonDate).FirstOrDefault();
                    return date;
                }
                else
                {
                    temperature += .1;
                }
            }
        }
        private static string SaveAutuumWinterDate(WeatherRecord date, string season, List<WeatherRecord> weatherRecords)
        {
            if (date != null)
            {
                var specificDate = weatherRecords.FirstOrDefault(x => x.Date.ToShortDateString() == date.Date.ToShortDateString());
                if (specificDate != null)
                {
                    return $"Date: {specificDate.Date.ToShortDateString()} Season: {season} Temperature: {specificDate.Temperature:F1}";
                }
            }
            return string.Empty;
        }
        private static void SpecificDate(List<WeatherRecord> allRecords)
        {
            bool success = false;
            while (!success)
            {
                DateTime date = DateTime.Now;
                Console.Write("Date (yyyy-mm-dd):");
                if (DateTime.TryParse(Console.ReadLine(), out date))
                {
                    var specificDate = allRecords.Where(x => x.Date == date).ToList();

                    if (specificDate.Any())
                    {
                        Console.WriteLine(date.ToShortDateString());
                        foreach (var day in specificDate)
                        {
                            Console.WriteLine($"Location: {day.Location} Temperature: {day.Temperature:F1} Humditiy: {day.Humidity:F1}%");
                        }
                        Console.ReadLine();
                        Console.Clear();
                        success = true;
                    }
                    else
                    {
                        Console.WriteLine("No records found for the date. Try with another date...");
                        continue;
                    }
                }
                Console.WriteLine("Invalid date format. Get good at spelling pls...");
                continue;
            }
        }
    }
}
