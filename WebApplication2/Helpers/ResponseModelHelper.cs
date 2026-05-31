using WebApplication2.Models;

namespace WebApplication2.Helpers
{
    public class ResponseModelHelper
    {
        public static ResponseModel<T> CreateSuccessResponse<T>(T data)
        {
            return new ResponseModel<T>
            {
                Success = true,
                Errors = null,
                Data = data
            };
        }

        public static ResponseModel<T> CreateNotFoundResponse<T>(string error)
        {
            return new ResponseModel<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Data = default
            };
        }

        public static ResponseModel<T> CreateBadRequestResponse<T>(string error)
        {
            return new ResponseModel<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Data = default
            };
        }

        public static ResponseModel<T> CreateErrorResponse<T>(List<string> errors)
        {
            return new ResponseModel<T>
            {
                Success = false,
                Errors = errors,
                Data = default
            };
        }
    
    }
}
