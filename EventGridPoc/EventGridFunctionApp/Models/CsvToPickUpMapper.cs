using CsvHelper.Configuration;

namespace ListenEventGrid.Models
{
    public class CsvToPickUpMapper : ClassMap<Info>
    {
        public CsvToPickUpMapper()
        {
            this.Map(x => x.Name).Name("Name");
            this.Map(x => x.Age).Name(" Age");
        }
    }
}