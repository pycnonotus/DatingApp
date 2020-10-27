namespace API.Errors {
    public class ApiException {
        public ApiException (int stuntsCode, string message = null, string details = null) {
            StuntsCode = stuntsCode;
            Message = message;
            Details = details;
        }

        public int StuntsCode { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }
}
