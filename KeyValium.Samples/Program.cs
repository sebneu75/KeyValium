namespace KeyValium.Samples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rs = new Raw.Samples();
            rs.CreateDatabase();
            rs.CreateDatabase2();
            rs.Sample2();
            rs.Sample3();

            var ms=new MultiDictionary.Samples();
            ms.Create();
            ms.Sample1();
        }
    }
}
