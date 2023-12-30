namespace KeyValium.Samples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rs = new Raw.Samples();
            rs.CreateDatabase();
            rs.CreateDatabase2();
            rs.Sample1();
            rs.Sample2();

            var ms=new MultiDictionary.Samples();
            ms.Sample1();
        }
    }
}
