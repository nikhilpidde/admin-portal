namespace DTPortal.Web.ViewModel.Clients
{
    public class Reaponse
    {
        public  bool Success { get; set; }
        public  string Message { get; set; }
        public  object Result { get; set; }

        public Reaponse(string msg)
        {
            Success = false;
            Message = msg;
            Result = null;
        }

        public Reaponse(object data,string msg)
        {
            Success = true;
            Message = msg;
            Result = data;
        }
    }
}
