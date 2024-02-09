using System.Text.RegularExpressions;

namespace WeaterData
{
    public delegate void SaveToFileDelegate(string fileName, string path, IEnumerable<string> data);

    internal static class FileHandler
    {
        public static List<WeatherRecord> ReadDataFromFile(this List<WeatherRecord> records, string fileName, string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path + fileName))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        Match match = Regex.Match(line, @"(?<date>\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}),(?<location>\w+),(?<temperature>[\d.]+),(?<humidity>\d+)");
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
        public static SaveToFileDelegate SaveToFile = (fileName, path, data) =>
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
        };
        public static void FileCreate(List<WeatherRecord> averageValues, string path)
        {
            List<string> dateAvgTemp = new List<string>();
            List<string> dateAvgHumidity = new List<string>();

            foreach (var item in averageValues)
            {
                string recordTemp = $"Date: {item.Date.ToShortDateString()}, Location: {item.Location}, Average Temperature: {item.Temperature:F1}";
                string recordHumidity = $"Date: {item.Date.ToShortDateString()}, Location: {item.Location}, Average Humidity: {item.Humidity:F1}%";

                dateAvgTemp.Add(recordTemp);
                dateAvgHumidity.Add(recordHumidity);
            }

            SaveToFile("average_temperature_data.txt", path, dateAvgTemp);

            SaveToFile("average_humidity_data.txt", path, dateAvgHumidity);
        }
    }
}
