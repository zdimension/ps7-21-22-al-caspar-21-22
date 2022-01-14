using System.Net;

namespace PS7Api.Utilities;

public class CircuitBreaker
{
    public State State { get; private set; }
    public int FailCount { get; private set; }
    public int SuccessCount { get; private set; }
    private readonly int _failThreshold;
    private readonly int _successThreshold;
    private readonly int _timeout;

    public CircuitBreaker(): this(5, 5, 30000) {}
    
    public CircuitBreaker(int failThreshold, int successThreshold, int timeout)
    {
        State = State.Closed;
        _failThreshold = failThreshold;
        _successThreshold = successThreshold;
        _timeout = timeout;
    }

    public HttpResponseMessage Send(Func<HttpResponseMessage> req)
    {
        switch (State)
        {
            case State.Closed: default:
                try
                {
                    var response = req.Invoke();
                    if(IsServerOut(response))
                        OnFail();
                    return response;
                }
                catch (Exception e)
                {
                    OnFail();
                    throw;
                }
            case State.HalfOpened:
                try
                {
                    var response = req.Invoke();
                    if(IsServerOut(response))
                        Open();
                    else
                        SuccessCount++;
                    if (SuccessCount == _successThreshold)
                    {
                        FailCount = 0;
                        State = State.Closed;
                    }
                    return response;
                }
                catch (Exception e)
                {
                    Open();
                    throw;
                } 
            case State.Opened:
                throw new CircuitBreakerOpenedException();
        }
    }

    private void OnFail()
    {
        FailCount++;
        if (FailCount == _failThreshold)
        {
            Open();
        }
    }

    private async void Open()
    {
        State = State.Opened;
        await Task.Delay(_timeout).ContinueWith(_ =>
        {
            SuccessCount = 0;
            State = State.HalfOpened;
        });
    }

    private bool IsServerOut(HttpResponseMessage response)
    {
        return response.StatusCode is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable;
    }

}

public enum State
{
    Closed, HalfOpened, Opened
}


public class CircuitBreakerOpenedException : Exception
{
    public CircuitBreakerOpenedException()
    {
    }

    public CircuitBreakerOpenedException(string? message) : base(message)
    {
    }

    public CircuitBreakerOpenedException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}