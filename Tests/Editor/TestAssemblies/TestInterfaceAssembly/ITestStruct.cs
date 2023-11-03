using PocketGems.Parameters.Interface;

namespace PocketGems
{
    public interface ITestStruct : IBaseStruct
    {

    }

    public interface ITest2Struct : ITestStruct
    {

    }

    public interface ITest3Struct : ITest2Struct
    {

    }
}
