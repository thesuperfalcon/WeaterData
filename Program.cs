namespace WeaterData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileName = "tempdata5-med fel.txt";
            string path = "../../../";

            List<WeatherRecord> allRecords = FileHandler.ReadDataFromFile(fileName, path);

            while (true)
            {
                WeatherRecord.WeatherMenu(allRecords, path);
            }
        }
    }
}