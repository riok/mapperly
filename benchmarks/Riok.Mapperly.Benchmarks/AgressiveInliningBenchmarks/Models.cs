namespace Riok.Mapperly.Benchmarks.AgressiveInliningBenchmarks;

#region Class
public class MyComplexClass
{
    public static readonly MyComplexClass Instance =
        new()
        {
            Int = 100,
            String = "String",
            Boolean = true,
            Long = 100,
            Double = 100.0,
            DateTime = DateTime.Now,
            Enum = MyEnum.Enum1,
            SubClass1 = new MySubClass1
            {
                Int = 101,
                String = "SubStruct1",
                SubClass2 = new MySubClass2
                {
                    Int = 102,
                    String = "SubStruct2",
                    SubClass3 = new MySubClass3 { Int = 103, String = "SubStruct3" }
                }
            }
        };

    public int Int { get; set; }
    public string? String { get; set; }
    public bool Boolean { get; set; }
    public long Long { get; set; }
    public double Double { get; set; }
    public DateTime DateTime { get; set; }
    public MyEnum Enum { get; set; }
    public MySubClass1? SubClass1 { get; set; }
}

public class MySubClass1
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubClass2? SubClass2 { get; set; }
}

public class MySubClass2
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubClass3? SubClass3 { get; set; }
}

public class MySubClass3
{
    public int Int { get; set; }
    public string? String { get; set; }
}

public class MyComplexClassDto
{
    public int Int { get; set; }
    public string? String { get; set; }
    public bool Boolean { get; set; }
    public long Long { get; set; }
    public double Double { get; set; }
    public DateTime DateTime { get; set; }
    public MyEnum Enum { get; set; }
    public MySubClass1Dto? SubClass1 { get; set; }
}

public class MySubClass1Dto
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubClass2Dto? SubClass2 { get; set; }
}

public class MySubClass2Dto
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubClass3Dto? SubClass3 { get; set; }
}

public class MySubClass3Dto
{
    public int Int { get; set; }
    public string? String { get; set; }
}
#endregion

#region Struct
public struct MyComplexStruct
{
    public static readonly MyComplexStruct Instance =
        new()
        {
            Int = 100,
            String = "String",
            Boolean = true,
            Long = 100,
            Double = 100.0,
            DateTime = DateTime.Now,
            Enum = MyEnum.Enum1,
            SubStruct1 = new MySubStruct1
            {
                Int = 101,
                String = "SubStruct1",
                SubStruct2 = new MySubStruct2
                {
                    Int = 102,
                    String = "SubStruct2",
                    SubStruct3 = new MySubStruct3 { Int = 103, String = "SubStruct3" }
                }
            }
        };

    public int Int { get; set; }
    public string? String { get; set; }
    public bool Boolean { get; set; }
    public long Long { get; set; }
    public double Double { get; set; }
    public DateTime DateTime { get; set; }
    public MyEnum Enum { get; set; }
    public MySubStruct1? SubStruct1 { get; set; }
}

public struct MySubStruct1
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubStruct2? SubStruct2 { get; set; }
}

public struct MySubStruct2
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubStruct3? SubStruct3 { get; set; }
}

public struct MySubStruct3
{
    public int Int { get; set; }
    public string? String { get; set; }
}

public struct MyComplexStructDto
{
    public int Int { get; set; }
    public string? String { get; set; }
    public bool Boolean { get; set; }
    public long Long { get; set; }
    public double Double { get; set; }
    public DateTime DateTime { get; set; }
    public MyEnum Enum { get; set; }
    public MySubStruct1Dto? SubStruct1 { get; set; }
}

public struct MySubStruct1Dto
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubStruct2Dto? SubStruct2 { get; set; }
}

public struct MySubStruct2Dto
{
    public int Int { get; set; }
    public string? String { get; set; }
    public MySubStruct3Dto? SubStruct3 { get; set; }
}

public struct MySubStruct3Dto
{
    public int Int { get; set; }
    public string? String { get; set; }
}
#endregion

public enum MyEnum
{
    Enum1,
    Enum2,
    Enum3
}
