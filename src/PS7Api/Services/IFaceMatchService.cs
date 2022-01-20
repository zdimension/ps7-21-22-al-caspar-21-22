namespace PS7Api.Services;

public interface IFaceMatchService
{
    float GetMatchScore(byte[] user, byte[] given);
}

public class MockFaceMatchService : IFaceMatchService
{
    public float GetMatchScore(byte[] user, byte[] given)
    {
        return user.Length < given.Length ? user.Length / given.Length : given.Length / user.Length;
    }
}