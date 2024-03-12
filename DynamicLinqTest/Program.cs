using System.Linq.Dynamic.Core;

namespace DynamicLinqTest
{
    internal class Program
    {
        List<dynamic> testDynamicList = new List<dynamic>();
        List<TestData> testDataList = new List<TestData>();

        static void Main(string[] args)
        {
            //Queryable.GroupBy(

            Program program = new Program();
            program.SampleData(100);
            program.DynamicClass();
            program.BasicQuery();
            program.GroupBy();
            program.Expression();
        }

        private void SampleData(int count)
        {
            for (int i = 0; i < count; i++)
            {
                TestData testData = new TestData()
                {
                    Name = "NAME" + (i / 10),
                    Age = i,
                    Data1 = "Data1_" + (i / 5),
                    Data2 = "Data2_" + i,
                    intData1 = i + 1,
                    intData2 = count - i,
                };

                testDynamicList.Add(testData);
                testDataList.Add(testData);
            }
        }

        private void DynamicClass()
        {
            DynamicProperty[] props = new DynamicProperty[]
            {
                new DynamicProperty("Name", typeof(string)),
                new DynamicProperty("Birthday", typeof(DateTime))
            };

            Type type = DynamicClassFactory.CreateType(props);

            object obj = Activator.CreateInstance(type);

            type.GetProperty("Name").SetValue(obj, "kei", null);
            type.GetProperty("Birthday").SetValue(obj, new DateTime(1995, 3, 14), null);

            Console.WriteLine(obj);
        }

        private void BasicQuery()
        {
            var queryableData = testDataList.AsQueryable();

            var where = queryableData.Where("Name == @0", "NAME0").ToList();

            var avg = queryableData.Aggregate("Average", "intData1");

            var and = queryableData.Where("Name == @0 and intData1 > @1", "NAME0", 3).ToList();

            var select = queryableData.Select("new { Name, Data1 }").ToDynamicList();

            var orderedDynamic = queryableData.OrderBy("Name, intData2").ToList();
        }

        private void GroupBy()
        {
            var queryableData = testDynamicList.AsQueryable();

            // 싱글 컬럼 GroupBy
            var onColumnGroups = queryableData.GroupBy("Name");

            foreach (IGrouping<object, object> group in onColumnGroups)
            {
                string Key = group.Key.ToString();
                var dataList = group.ToList();

                Console.WriteLine($"{Key} : {dataList.Count()}");
            }

            // 멀티 컬럼 GroupBy
            var multiColumnGroups = queryableData.GroupBy("new (Name as Name, Data1 as Data1)");

            foreach (IGrouping<object, object> group in multiColumnGroups)
            {
                string Key = group.Key.ToString();
                var dataList = group.ToList();

                Console.WriteLine($"{Key} : {dataList.Count()}");
            }

            // 멀티 컬럼 GroupBy + 키값 및 키별 Count
            var gResult = queryableData.GroupBy("new (Name as Name, Data1 as Data1)").Select("new(Key, Count() AS Count)");

            foreach (dynamic result in gResult)
            {
                Console.WriteLine($"{result.Key} : {result.Count}");
            }

            // 멀티 컬럼 GroupBy + 특정 컬럼만 선택
            var multiColumnGroupsResult = queryableData.GroupBy("new (Name as Name, Data1 as Data1)", "new (Data2 as Data2, intData2 as intData2)");

            foreach (IGrouping<object, object> group in multiColumnGroupsResult)
            {
                Console.WriteLine($"{group.Key} : {group.Count()}");
            }
        }

        private void Expression()
        {
            var rangeOfNumbers = Enumerable.Range(1, 100).ToArray();
            var result1 = rangeOfNumbers.AsQueryable().Where("it in (1,3,5,7, 101)").ToArray();

            var values = new int[] { 2, 4, 6, 8, 102 };
            var result2 = rangeOfNumbers.AsQueryable().Where("it in @0", values).ToArray();

            var result3 = rangeOfNumbers.AsQueryable().Where("it % 2 = 0");

            var result4 = rangeOfNumbers.AsQueryable().Select("it % 2 == 0 ? true : false");

        }

        public class TestData
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Data1 { get; set; }
            public string Data2 { get; set; }
            public int intData1 { get; set; }
            public int intData2 { get; set; }
        }
    }
}
