using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WeaterData
{
    internal class FileHandler
    {
        public static List<WeatherRecord> ReadDataFromFile(string fileName, string path)
        {
            List<WeatherRecord> records = new List<WeatherRecord>();

            try
            {
                using (StreamReader sr = new StreamReader(path + fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Match match = Regex.Match(line, @"(\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}),(\w+),([\d.]+),(\d+)");
                        if (match.Success)
                        {
                            DateTime date;
                            string dateString = match.Groups[1].Value;
                            if (DateTime.TryParse(dateString, out date) &&
                            !(date.Year == 2016 && date.Month == 5) &&
                            !(date.Year == 2017 && date.Month == 1))
                            {
                                if (date.Hour >= 24)
                                {
                                    date = date.AddDays(1).Date;
                                }
                                string location = match.Groups[2].Value;
                                double temperature = double.Parse(match.Groups[3].Value, System.Globalization.CultureInfo.InvariantCulture);
                                int humidity = int.Parse(match.Groups[4].Value, System.Globalization.CultureInfo.InvariantCulture);
                                WeatherRecord record = new WeatherRecord(date, location, temperature, humidity);
                                records.Add(record);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ett fel inträffade vid läsning av filen: {ex.Message}");
            }

            return records;
        }
        public static void SaveToFile(string fileName, string path, IEnumerable<string> data)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path + fileName))
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
        }
        public static void FileCreate(List<WeatherRecord> averageValues, string path)
        {
            //file_for_average_temp

            List<string> dateAvgTemp = new List<string>();

            string recordString = string.Empty;

            string fileName = string.Empty;

            foreach (var item in averageValues)
            {
                recordString = $"Date: {item.Date.ToShortDateString()}, Location: {item.Location}, Average Temperature: {item.Temperature:F1}";

                dateAvgTemp.Add(recordString);
            }

            fileName = "average_temperature_data.txt";

            FileHandler.SaveToFile(fileName, path, dateAvgTemp);

            //file_for_average_humidity

            List<string> dateAvgHumidity = new List<string>();

            foreach (var item in averageValues)
            {
                recordString = $"Date: {item.Date.ToShortDateString()}, Location: {item.Location}, Average Humidity: {item.Humidity:F1}%";

                dateAvgHumidity.Add(recordString);
            }

            fileName = "average_humidity_data.txt";

            FileHandler.SaveToFile(fileName, path, dateAvgHumidity);
        }
    }
}
