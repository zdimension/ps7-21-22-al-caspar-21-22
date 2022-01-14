using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using PS7Api.Utilities;
using Xunit;

namespace PS7Api.UnitTests.Utilities;

public class CircuitBreakerTest
{
    [Fact]
    public void Fine()
    {
        CircuitBreaker cb = new CircuitBreaker();
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new Mock<HttpResponseMessage>();
        respFunc.Setup(message => message.Invoke()).Returns(resp.Object);
        
        cb.Send(respFunc.Object);
        Assert.Equal(State.Closed, cb.State);
        Assert.Equal(0, cb.FailCount);
    }
    
    [Fact]
    public void One_Fault()
    {
        CircuitBreaker cb = new CircuitBreaker();
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        respFunc.Setup(message => message.Invoke()).Throws(new HttpRequestException());

        Send(cb, respFunc);
        Assert.Equal(State.Closed, cb.State);
        Assert.Equal(1, cb.FailCount);
    }
    
    [Fact]
    public void Server_Unavailable()
    {
        CircuitBreaker cb = new CircuitBreaker();
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new HttpResponseMessage();
        resp.StatusCode = HttpStatusCode.ServiceUnavailable;
        respFunc.Setup(message => message.Invoke()).Returns(resp);

        Send(cb, respFunc);
        Assert.Equal(State.Closed, cb.State);
        Assert.Equal(1, cb.FailCount);
    }
    
    [Fact]
    public void Too_Many_Request()
    {
        CircuitBreaker cb = new CircuitBreaker();
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new HttpResponseMessage();
        resp.StatusCode = HttpStatusCode.ServiceUnavailable;
        respFunc.Setup(message => message.Invoke()).Returns(resp);

        Send(cb, respFunc);
        Assert.Equal(State.Closed, cb.State);
        Assert.Equal(1, cb.FailCount);
    }
    
    [Fact]
    public void Circuit_Opened()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1000);
        /*var respFunc = new Mock<Func<HttpResponseMessage>>();
        respFunc.Setup(message => message.Invoke()).Throws(new HttpRequestException());

        Send(cb, respFunc);
        Send(cb, respFunc);
        Send(cb, respFunc);
        Send(cb, respFunc);
        Send(cb, respFunc);*/
        SetOpened(cb);
        Assert.Equal(State.Opened, cb.State);
        Assert.Equal(5, cb.FailCount);
    }
    
    [Fact]
    public void Circuit_Opened_Call()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1000);
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new Mock<HttpResponseMessage>();
        respFunc.Setup(message => message.Invoke()).Returns(resp.Object);
        SetOpened(cb);

        Assert.Throws<CircuitBreakerOpenedException>(() => { cb.Send(respFunc.Object); });
        Assert.Equal(State.Opened, cb.State);
    }
    
    [Fact]
    public void Circuit_Half_Opened()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1);
        SetOpened(cb);

        Task.Delay(50).Wait();
        Assert.Equal(State.HalfOpened, cb.State);
        Assert.Equal(0, cb.SuccessCount);
    }

    [Fact]
    public void Circuit_Half_Opened_Fault()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1);
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        respFunc.Setup(message => message.Invoke()).Throws(new HttpRequestException());
        SetOpened(cb);
        Task.Delay(50).Wait();

        Send(cb, respFunc);
        
        Assert.Equal(State.Opened, cb.State);
    }
    
    [Fact]
    public void Circuit_Half_Opened_Service_Unavailable()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1);
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new HttpResponseMessage();
        resp.StatusCode = HttpStatusCode.ServiceUnavailable;
        respFunc.Setup(message => message.Invoke()).Returns(resp);
        SetOpened(cb);
        Task.Delay(50).Wait();

        Send(cb, respFunc);
        
        Assert.Equal(State.Opened, cb.State);
    }
    
    [Fact]
    public void Circuit_Half_Opened_Success()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1);
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new Mock<HttpResponseMessage>();
        respFunc.Setup(message => message.Invoke()).Returns(resp.Object);
        SetOpened(cb);
        Task.Delay(50).Wait();

        cb.Send(respFunc.Object);
        
        Assert.Equal(State.HalfOpened, cb.State);
        Assert.Equal(1, cb.SuccessCount);
    }
    
    [Fact]
    public void Circuit_Half_To_Closed()
    {
        CircuitBreaker cb = new CircuitBreaker(5, 5, 1);
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        var resp = new Mock<HttpResponseMessage>();
        respFunc.Setup(message => message.Invoke()).Returns(resp.Object);
        SetOpened(cb);
        Task.Delay(50).Wait();

        cb.Send(respFunc.Object);
        cb.Send(respFunc.Object);
        cb.Send(respFunc.Object);
        cb.Send(respFunc.Object);
        cb.Send(respFunc.Object);
        
        Assert.Equal(State.Closed, cb.State);
        Assert.Equal(0, cb.FailCount);
    }


    private void SetOpened(CircuitBreaker cb)
    {
        var respFunc = new Mock<Func<HttpResponseMessage>>();
        respFunc.Setup(message => message.Invoke()).Throws(new HttpRequestException());
        for (int i = 0; i < 5; i++)
        {
            Send(cb, respFunc);
        }
    }

    private void Send(CircuitBreaker cb, Mock<Func<HttpResponseMessage>> mock)
    {
        try
        {
            cb.Send(mock.Object);
        }
        catch (Exception e)
        {
            // ignored
        }
    }
    
}