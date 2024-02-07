namespace WeaterData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string fileName = "tempdata5-med fel.txt";
            string path = "../../../";

            List<WeatherRecord> allRecords = new List<WeatherRecord> ();
            allRecords.ReadDataFromFile(fileName, path);

            while (true)
            {
                WeatherRecord.WeatherMenu(allRecords, path);
            }
        }
    }
}