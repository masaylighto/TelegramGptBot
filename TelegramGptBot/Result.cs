
namespace TelegramGptBot;

public class Result<DataType, ErrorType>
{
    //constructors 
    public Result(ErrorType error)
    {
        Error = error;
        ErrorExist = true;
    }
    public Result(DataType data)
    {
        Data = data;
        ErrorExist = false;
    }
    protected Result()
    {


    }
    //Fields
    DataType? Data { get; set; }
    ErrorType? Error { get; set; }
    bool ErrorExist { get; set; }

    //Polymorphism Method
    public bool HasError() => ErrorExist;
    public bool HasNoError() => !ErrorExist;
    public ErrorType? GetError() => Error;
    public bool IsErrorOfType<Type>() => Error is Type;
    public bool HasData() => Data is not null;
    public DataType? GetData() => Data;
    public void SetData(DataType dataType) => Data = dataType;
    public void SetError(ErrorType? error)
    {
        ErrorExist = true;
        Error = error;
    }


    //Implict Conversation  
    public static implicit operator Result<DataType, ErrorType>(DataType data) => new Result<DataType, ErrorType>
    {
        Data = data,
        ErrorExist = false,
    };

    public static implicit operator Result<DataType, ErrorType>(ErrorType error) => new Result<DataType, ErrorType>
    {
        Error = error,
        ErrorExist = true,
    };

     
    public static Result<DataType, ErrorType>Ok(DataType data) => new Result<DataType, ErrorType>
    {
        Data = data,
        ErrorExist = false,
    };

    public static Result<DataType, ErrorType> NotOk(ErrorType error) => new Result<DataType, ErrorType>
    {
        Error = error,
        ErrorExist = true,
    };

}
