public class LoginEventArgs : System.EventArgs
{
    public string SessionId { get; set; }
}

public class BalanceEventArgs : System.EventArgs
{
    public string Balance { get; set; } // Could use decimal for wei
}